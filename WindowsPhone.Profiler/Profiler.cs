using Microsoft.SmartDevice.Connectivity;
using Microsoft.SmartDevice.Connectivity.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WindowsPhone.Tools;

namespace WindowsPhone.Profiler
{
    public enum ProfilerState
    {
        Deploying,
        Profiling,
        GeneratingEtl,
        CopyingEtl,
        Stopping,
    }

    public class ProfilerStatusUpdatedArgs : EventArgs
    {
        public string Message { get; private set; }
        public ProfilerState State { get; private set; }

        public ProfilerStatusUpdatedArgs(ProfilerState state, string msg)
        {
            State = state;
            Message = msg;
        }
    }

    public partial class Profiler
    {

        #region Events

        public event EventHandler<ProfilerStatusUpdatedArgs> StatusUpdated;

        protected virtual void OnStatusUpdated(ProfilerState state, string msg = null)
        {
            // copy to avoid race conditions - we're playing it safe :)
            // at least according to: http://msdn.microsoft.com/en-us/library/w369ty8x.aspx
            EventHandler<ProfilerStatusUpdatedArgs> handler = StatusUpdated;

            // Event will be null if there are no subscribers 
            if (handler != null)
            {

                // Use the () operator to raise the event.
                handler(this, new ProfilerStatusUpdatedArgs(state, msg));
            }
        }

        #endregion

        private enum ProfilerCommand
        {
            CheckVersionCompatibility = 1,
            UpdateSilverlightProfilerBinaries,
            StartProfiling,
            PauseProfiling,
            ResumeProfiling,
            StopProfiling,
            WaitForProfilerEvent,
            RegisterInstrumentedBinaries,
            UnregisterInstrumentedBinaries,
            GetDeviceFile,
            GetDeviceFileData,
            EndSession
        }

        // C:\ProgramData\Microsoft\Phone Tools\CoreCon\11.0\addons\OrenNachman.WindowsPhone.PowerTools.PackageDefinition.xsl
        // public so that we can use it in error messages if writing fails
        public static readonly string WPTOOLS_PACKAGE_PATH = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), 
            @"Microsoft\Phone Tools\CoreCon\11.0\addons\", 
            "OrenNachman.WindowsPhone.PowerTools.PackageDefinition.xsl");

        // TODO: might be nice to have the base class for this in a project-global
        private static readonly string WPTOOLS_PROFILER_DATA_PATH = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
            "WindowsPhonePowerTools",
            "Profiler");

        // standard install location: C:\Program Files (x86)\Windows Kits\8.0\Windows Performance Toolkit\
        // win8 install URL: http://msdn.microsoft.com/en-us/windows/hardware/hh852363.aspx
        private static readonly string WPT_PATH = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86),
            "Windows Kits",
            "8.0",
            "Windows Performance Toolkit");

        public static readonly string XPERF_PATH = Path.Combine(
            WPT_PATH,
            "xperf.exe");

        public static readonly string XPERFVIEW_PATH = Path.Combine(
            WPT_PATH,
            "xperfview.exe");

        public static readonly string WPA_PATH = Path.Combine(
            WPT_PATH,
            "wpa.exe");  

        // the name of the file that will be installed alongside the profiler. The contents of this file
        // drive the profiler session.
        private const string WPTOOLS_PROFILER_DATA_FILE = "ProfilingTasks.xml";

        private WindowsPhoneDevice _device;
        private Microsoft.SmartDevice.Connectivity.Device _internalDevice;
        private Microsoft.VisualStudio.DeviceConnectivity.Interop.ConManServerClass _conManServer;

        // required packages 

        // local path: C:\ProgramData\Microsoft\Phone Tools\CoreCon\11.0\addons\Microsoft.WindowsPhone.NativeProfiler.PackageDefinition.xsl
        private ObjectId _packageVsPerfCommon  = new ObjectId("429289E6-9C6D-4694-BBAC-AEC7C2093689");
        private ObjectId _packageVsPerfArch    = new ObjectId("4C7E8707-1E66-41C3-A2A3-3C0088DAA188");
        private ObjectId _packageVsPerfCrtDlls = new ObjectId("C1AE1947-6100-4932-A806-D963A08F44B1");

        private ObjectId _packageWpToolsTasks = new ObjectId("9CFE2E89-1ECD-413D-830F-DCB605AD5781");

        // local path: C:\ProgramData\Microsoft\Phone Tools\CoreCon\11.0\addons\Microsoft.Silverlight.Diagnostic.PackageDefinition.xsl
        private ObjectId _packageDeviceAgentTransporter = new ObjectId("DC97FEAB-9CC8-4693-8BC1-A601BAC31952");

        private ObjectId _remoteAgentGuid = new ObjectId("11EE50CA-6CD3-45ba-9D65-46E133CFF009");
        private ObjectId _remoteServiceId = new ObjectId("B2FC26AB-D6EC-4426-91FA-9E039F92A639");

        private FileDeployer _fileDeployer;
        private RemoteAgent _agent;
        private DevicePacketStream _devicePacketStream;

        private string _profilerLaunchService = "*{7F7534A1-293D-495E-88DA-F1EA984EF075}(%FOLDERID_SharedData%\\PhoneTools\\11.0\\corecon\\lib\\profilerdll.dll,InitializeService)";

        private int _sessionID;

        private bool _stopProfiler;

        /// <summary>
        /// Return a new Profiler, or null if the profiler does not apply to this kind of device (< WP8)
        /// </summary>
        /// <param name="_device"></param>
        /// <returns></returns>
        public static Profiler Get(Tools.WindowsPhoneDevice _device)
        {
            // TODO: need to verify WP8 device
            try
            {
                return new Profiler(_device);
            }
            catch { }

            return null;
        }

        private Profiler(WindowsPhoneDevice device)
        {
            _device = device;
            
            GetInternalDevice();

            GetConmanServer();

            _fileDeployer = _internalDevice.GetFileDeployer();
        }

        public static bool ProfilerConfiguredLocally()
        {
            return File.Exists(WPTOOLS_PACKAGE_PATH);
        }

        /// <summary>
        /// Creates the WPTools profiler package for the phone. Will fail if not launched elevated.
        /// 
        /// Note: this forces an install regardless of whether or not the file already exists in order to support
        /// easier forced update logic
        /// </summary>
        public static void ConfigureProfilerLocally()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowsPhone.Profiler.profiler_package.xsl");
            var reader = new StreamReader(stream);

            var packageText = reader.ReadToEnd();

            File.WriteAllText(WPTOOLS_PACKAGE_PATH, packageText);
        }

        public void StartProfiling(ProfilerSession session)
        {
            OnStatusUpdated(ProfilerState.Deploying, "Deploying the profiler and your session information to the device...");

            InstallProfiler();

            GenerateProfilerTasks(session);
           
            InstallProfilerTasks();

            OnStatusUpdated(ProfilerState.Profiling, "Connecting to profiler...");
            ConnectProfiler();

            OnStatusUpdated(ProfilerState.Profiling, "Profiling...");
            LaunchProfiler(session.TargetApp.RemoteApplication.ProductID);
        }

        private static void GenerateProfilerTasks(ProfilerSession session)
        {
            var tasks = session.GenerateTasks();

            XmlSerializer serializer = new XmlSerializer(typeof(ProfilingTasksType));

            if (!Directory.Exists(WPTOOLS_PROFILER_DATA_PATH))
                Directory.CreateDirectory(WPTOOLS_PROFILER_DATA_PATH);

            using (var writer = new StreamWriter(Path.Combine(WPTOOLS_PROFILER_DATA_PATH, WPTOOLS_PROFILER_DATA_FILE)))
            {
                serializer.Serialize(writer, tasks);

                writer.Flush();
                writer.Close();
            }
        }

        private void LaunchProfiler(Guid appId)
        {
            Packet responsePacket;

            var packet = new Packet();

            packet.WriteString("{" + appId.ToString() + "}");
            
            packet.WriteInt32(2); // 2 == magic number that gives us the ETW profiler

            packet.WriteString(_profilerLaunchService);

            // pad out the rest of the packet. Yup, 11 of them, thank you trial & error!
            for (int i = 0; i < 11; i++)
            {
                packet.WriteInt32(0);
            }
            
            this.RunProfilerCommand(ProfilerCommand.StartProfiling, packet, out responsePacket);

            _sessionID = responsePacket.ReadInt32();
        }

        private void RunProfilerCommand(ProfilerCommand command, Packet requestPacket, out Packet responsePacket)
        {
            responsePacket = null;

            RunProfilerCommand(command);

            if (requestPacket != null)
            {
                _devicePacketStream.Write(requestPacket);
            }

            // the response status code will be the second int read back (first is session ID)
            var packetStatusResponse = _devicePacketStream.Read();
            
            var sessionId = packetStatusResponse.ReadInt32();

            var status = packetStatusResponse.ReadInt32();

            if (status == 0)
            {
                responsePacket = _devicePacketStream.Read();
                return;
            }

            throw new Exception(command.ToString() + " failed. Return status: " + status);
        }

        private void RunProfilerCommand(ProfilerCommand command)
        {
            // construct the command packet
            var commandPacket = new Packet();
            commandPacket.WriteInt32((int)command);

            _devicePacketStream.Write(commandPacket);
        }

        public void WaitForProfilingStop(string localEtlFile)
        {
            OnStatusUpdated(ProfilerState.Profiling, "Profiling... (Exit the app or press the Stop button below to stop profiling)");

            int code = -1;

            // code == 0 -> Profiler stopped
            while (code != 0)
            {
                if (_stopProfiler)
                    break;

                // TODO: magic numbers!
                code = WaitForEvent(3000);
            }

            OnStatusUpdated(ProfilerState.Stopping, "Stopping the profiler...");

            // TODO: no status check on these :S

            // since we may have gotten here by requesting a stop, instead of the profiler stopping
            // by itself, force a stop anyway
            this.RunProfilerCommand(ProfilerCommand.StopProfiling);

            string tempVspxFile = Path.GetTempFileName();
            string tempEtlFile = Path.GetTempFileName();

            try
            {
                CopyVspx(tempVspxFile);
                ExtractEtl(tempVspxFile, tempEtlFile);
                MergeEtl(tempEtlFile, localEtlFile);
            }
            finally
            {
                try { File.Delete(tempVspxFile); } catch { }
                try { File.Delete(tempEtlFile); } catch { }
            }

            EndSession();
        }

        private void ExtractEtl(string tempVspxPath, string tempEtlPath)
        {
            OnStatusUpdated(ProfilerState.Stopping, "Extracting ETL file from profiler output...");

            ZipStorer vspx = ZipStorer.Open(tempVspxPath, FileAccess.Read);

            List<ZipStorer.ZipFileEntry> entries = vspx.ReadCentralDir();

            // Look for the etl
            foreach (ZipStorer.ZipFileEntry entry in entries)
            {
                if (entry.FilenameInZip.EndsWith(".etl"))
                {
                    vspx.ExtractFile(entry, tempEtlPath);

                    vspx.Close();

                    return;
                }
            }

            vspx.Close();

            throw new FileNotFoundException("Couldn't find an etl file in the vspx that we retrieved from the device");
        }

        /// <summary>
        /// Remerge the ETL file on the local machine to pickup any developer specific providers
        /// </summary>
        /// <param name="localEtlFile"></param>
        private void MergeEtl(string tempEtlFile, string localEtlFile)
        {
            // this is non-fatal and we expect the calling UI to warn about this
            if (!File.Exists(XPERF_PATH))
                return;

            OnStatusUpdated(ProfilerState.Stopping, "Baking local ETW symbols into your trace...");

            // merge the temporary etl file into itself at the local path. Merging one file only will simply copy the file
            // to the new location but bake in any missing ETW symbols that may be registered on the local machine (for example
            // the developer's manifest which will not be registered on the device)
            var proc = Process.Start(XPERF_PATH, "-merge \"" + tempEtlFile + "\" \"" + localEtlFile + "\"");
            proc.WaitForExit();
        }

        /// <summary>
        /// Signal a stop (this will break into WaitForProfilingStop)
        /// </summary>
        /// <param name="localEtlFile"></param>
        public void StopProfiling()
        {
            _stopProfiler = true;
        }

        private void EndSession()
        {
            this.RunProfilerCommand(ProfilerCommand.EndSession);
        }

        private int WaitForEvent(int timeout = 1000)
        {
            Packet responsePacket;

            var packet = new Packet();

            packet.WriteInt32(timeout);

            this.RunProfilerCommand(ProfilerCommand.WaitForProfilerEvent, packet, out responsePacket);

            int intEventType = responsePacket.ReadInt32();

            return intEventType;
        }

        /// <summary>
        /// There is an event to receive the file from the profiler, but we'll only implement that if this turns out to be unreliable
        /// </summary>
        /// <param name="targetFilename"></param>
        public void CopyVspx(string targetFilename)
        {
            string sourceDeviceFileName;
            long oldSize = -1;
            int timeout = 5 * 60 * 1000; //ms

            OnStatusUpdated(ProfilerState.Stopping, "Generating the trace file...");

            sourceDeviceFileName = @"\Data\SharedData\PhoneTools\11.0\CoreCon\Logs\VSPerfLog" + _sessionID.ToString("X8") + ".vspx";

            // wait for the file to become available and for processing to finish on it (hence the size checks)
            Microsoft.VisualStudio.DeviceConnectivity.Interop.tagFileInfo fileInfo;

            while (timeout > 0)
            {
                try
                {
                    _conManServer.GetFileInfo(sourceDeviceFileName, out fileInfo);

                    if (oldSize == fileInfo.m_FileSize)
                    {
                        // the file is ready!
                        break;
                    }

                    // still being updated
                    oldSize = fileInfo.m_FileSize;
                }
                catch (FileNotFoundException)
                {
                }

                // wait a little (hopefully you're not running this on the UI thread!)
                Thread.Sleep(5000);
                timeout -= 5000;
            }

            OnStatusUpdated(ProfilerState.Stopping, "Copying trace file (" + targetFilename + ") from device...");

            _fileDeployer.ReceiveFile(sourceDeviceFileName, targetFilename, true);
        }
        
        private void ConnectProfiler()
        {
            _agent = _internalDevice.GetRemoteAgent(_remoteAgentGuid);
            _agent.Start("");
            _devicePacketStream = _agent.CreatePacketStream(_remoteServiceId);
        }

        private void InstallProfiler()
        {
            _fileDeployer.DownloadPackage(_packageDeviceAgentTransporter);

            _fileDeployer.DownloadPackage(_packageVsPerfCommon);
            _fileDeployer.DownloadPackage(_packageVsPerfArch);
            _fileDeployer.DownloadPackage(_packageVsPerfCrtDlls);

        }

        private void InstallProfilerTasks()
        {
            _fileDeployer.DownloadPackage(_packageWpToolsTasks);
        }

        private void GetInternalDevice() 
        {
            BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fieldInfo = (typeof(Microsoft.SmartDevice.Connectivity.Wrapper.DeviceObject)).GetField("mDevice", eFlags);

            if (fieldInfo != null)
            {
                _internalDevice = fieldInfo.GetValue(_device.CurrentDevice) as Microsoft.SmartDevice.Connectivity.Device;
            }

            // TODO: throw exception if this fails?
        }

        private void GetConmanServer()
        {
            BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fieldInfo = (typeof(Microsoft.SmartDevice.Connectivity.Device)).GetField("mConmanServer", eFlags);

            if (fieldInfo != null)
            {
                _conManServer = fieldInfo.GetValue(_internalDevice) as Microsoft.VisualStudio.DeviceConnectivity.Interop.ConManServerClass;
            }

        }

    }
}
