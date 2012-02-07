using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Test.CommandLineParsing;
using WindowsPhone.Tools;
using Microsoft.SmartDevice.Connectivity;
using System.Collections.ObjectModel;
using System.IO;

namespace WindowsPhonePowerTools.Console
{
    class Program
    {
        private class Arguments
        {
            public string Target { get; set; }
            public string App { get; set; }

            private string _xap;
            public string Xap
            {
                get { return _xap; }
                set
                {
                    if (_xap != value)
                    {
                        if (!File.Exists(value))
                            throw new FileNotFoundException("Could not find file: " + value);

                        _xap = value;
                    }
                }
            }

            public bool Install { get; set; }
            public bool UnInstall { get; set; }
            public bool Update { get; set; }
            public bool Launch { get; set; }
            public bool Usage { get; set; }
            public string Get { get; set; }
            public string Put { get; set; }
            public bool Directories { get; set; }
            public bool To { get; set; }
        }

        private static Arguments _args;
        private static WindowsPhoneDevice _device;

        static int Main(string[] args)
        {
            _args = new Arguments();
            try
            {
                CommandLineDictionary cmdlineDict = CommandLineDictionary.FromArguments(args, '-', ':');

                _args.ParseArguments(args, cmdlineDict);
            }
            catch (Exception e)
            {
                return Usage("Oops. Something bad happened with the parameters that you gave me.\n\n" + e.Message);
            }

            if (_args.Usage)
                return Usage();

            if (string.IsNullOrEmpty(_args.Target))
                return Usage("Must specify a target to connect to. Use: [emulator|xde]/[device|phone]");

            if (string.IsNullOrEmpty(_args.App) && string.IsNullOrEmpty(_args.Xap))
                return Usage("Must specify one of either -app or -xap");

            if (!string.IsNullOrEmpty(_args.App) && !string.IsNullOrEmpty(_args.Xap))
                return Usage("Confused me, you have. Should I use -app or -xap?");

            if (!_args.Install && !_args.Update && !_args.Launch && !_args.UnInstall && string.IsNullOrEmpty(_args.Get) && string.IsNullOrEmpty(_args.Put))
                return Usage("Yawn. You haven't asked me to install, update or launch the app, so what do you want me to do?");

            int rv = 0;

            try
            {
                rv = DoWork();
            }
            catch (ConsoleMessageException e)
            {
                System.Console.Error.WriteLine(e.Message);
                rv = 1;
            }

            return rv;
        }

        private static int DoWork()
        {
            if (!Connect(_args.Target))
                return 1;

            if (!string.IsNullOrEmpty(_args.Xap))
                _args.Xap = _args.Xap.Replace('=', ':');

            if (_args.Install)
            {
                Install(_args.Xap);
            }
            else if (_args.Update)
            {
                Update(_args.Xap);
            }
            else if (_args.UnInstall)
            {
                if (!string.IsNullOrEmpty(_args.App))
                {
                    UnInstall(new Guid(_args.App));
                }
                else
                {
                    UnInstall(_args.Xap);
                }
            }

            if (_args.Launch)
            {
                if (!string.IsNullOrEmpty(_args.App))
                {
                    Launch(new Guid(_args.App));
                }
                else
                {
                    Launch(_args.Xap);
                }
            }

            if (!string.IsNullOrEmpty(_args.Get))
            {
                if (!string.IsNullOrEmpty(_args.App))
                {
                    GetFiles(new Guid(_args.App), _args.Get, _args.To, _args.Directories);
                }
                else
                {
                    GetFiles(_args.Xap, _args.Get, _args.To, _args.Directories);
                }
            }

            if (!string.IsNullOrEmpty(_args.Put))
            {
                if (!string.IsNullOrEmpty(_args.App))
                {
                    PutFiles(new Guid(_args.App), _args.Put, _args.To, _args.Directories);
                }
                else
                {
                    PutFiles(_args.Xap, _args.Put, _args.To, _args.Directories);
                }
            }

            return 0;
        }

        private static void PutFiles(string p, string p_2, bool p_3, bool p_4)
        {
            throw new ConsoleMessageException("Use ISETool for Get/Put operations");
        }

        private static void PutFiles(Guid guid, string p, bool p_2, bool p_3)
        {
            throw new ConsoleMessageException("Use ISETool for Get/Put operations");
        }

        private static void GetFiles(Guid guid, string p, bool p_2, bool p_3)
        {
            throw new ConsoleMessageException("Use ISETool for Get/Put operations");
        }

        private static void GetFiles(string p, string p_2, bool p_3, bool p_4)
        {
            throw new ConsoleMessageException("Use ISETool for Get/Put operations");
        }

        private static bool Connect(string target)
        {
            bool wantEmulator = false;
            bool isEmulator = false;

            //foreach (Device
            if (target == "xde" || target == "emulator")
            {
                wantEmulator = true;
            }
            else if (target == "phone" || target == "device")
            {
                wantEmulator = false;
            }
            else
            {
                throw new ConsoleMessageException("Invalid device target (" + target + ")");
            }

            List<Device> devices = WindowsPhoneDevice.GetDevices();

            foreach (Device d in devices)
            {
                isEmulator = d.IsEmulator();

                if ((wantEmulator && isEmulator) || (!wantEmulator && !isEmulator))
                {
                    _device = new WindowsPhoneDevice();
                    _device.CurrentDevice = d;
                    
                    break;
                }
            }

            _device.Connect();

            return true;
        }

        private static void Install(string xapFile)
        {
            Xap xap = new Xap(xapFile);
            RemoteApplicationEx app = GetApp(xap.Guid);

            // first uninstall any existing instances
            if (app != null)
            {
                app.RemoteApplication.Uninstall();
            }

            _device.CurrentDevice.InstallApplication(xap.Guid, Guid.Empty, "genre", "noicon", xapFile);
        }

        private static void Update(string xap)
        {
            if (string.IsNullOrEmpty(xap) || !File.Exists(xap))
                throw new ConsoleMessageException("Must specify a valid xap with -update");

            RemoteApplicationEx app = GetApp(xap);

            app.RemoteApplication.UpdateApplication("genre", "noicon", xap);
        }

        private static void UnInstall(string xapFile)
        {
            Xap xap = new Xap(xapFile);

            UnInstall(xap.Guid);
        }

        private static void UnInstall(Guid guid)
        {
            RemoteApplicationEx app = GetApp(guid);

            if (app == null)
                throw new ConsoleMessageException("Unable to find installed app with GUID: " + guid);

            app.RemoteApplication.Uninstall();
        }

        private static RemoteApplicationEx GetApp(Guid guid)
        {
            // we'll need to find the app first
            Collection<RemoteApplicationEx> apps = _device.InstalledApplications;

            foreach (RemoteApplicationEx app in apps)
            {
                if (app.RemoteApplication.ProductID == guid)
                {
                    return app;
                }
            }

            return null;
        }

        private static RemoteApplicationEx GetApp(string xapFile)
        {
            Xap xap = new Xap(xapFile);

            return GetApp(xap.Guid);
        }

        private static void Launch(string xapFile)
        {
            Xap xap = new Xap(xapFile);

            Launch(xap.Guid);
        }

        private static void Launch(Guid guid)
        {
            RemoteApplicationEx app = GetApp(guid);

            if (app == null)
                throw new ConsoleMessageException("Unable to find app with guid: " + guid);

            app.RemoteApplication.Launch();
        }

        private static int Usage(string error = null)
        {
            System.Console.Error.WriteLine(error);

            System.Console.WriteLine(@"
General Usage:
    WindowsPhonePowerTools.Console -<param>:<value>    

Usage:
    -target    : what device to connect to. Supports emulator, xde, 
                 device or phone
    -app       : an app guid to interact with
    -xap       : a xap to interact with. Note that we'll extract the app guid 
                 from the xap so that you can say something like 
                 -launch -xap <file> 
                 instead of having to dig out the guid yourself
    -install   : installs a xap. Use with -app or -xap
    -uninstall : uninstalls a xap. Use with -app or -xap
    -update    : updates a xap. Use with -app or -xap
    -launch    : launches a xap. Use with -app or -xap
    -usage     : really?

Note:
    1. This is extremely rough. On purpose. Feel free to log bugs / feature 
       requests for future expansion.
    2. To interact with the Isolated Storage from the command line use 
       ISETool which comes with the SDK (you'll find it under 
       Program Files(x86)\Microsoft SDKs\Windows Phone\v7.1\
           Tools\IsolatedStorageExplorerTool)
");

            return 1;
        }
    }
}
