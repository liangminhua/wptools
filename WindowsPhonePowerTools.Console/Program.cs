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
            public string Xap { get; set; }
            public bool Install { get; set; }
            public bool Update { get; set; }
            public bool Launch { get; set; }
            public bool Usage { get; set; }
        }

        private static Arguments _args;
        private static WindowsPhoneDevice _device;

        static int Main(string[] args)
        {
            CommandLineDictionary cmdlineDict  = CommandLineDictionary.FromArguments(args, '-', ':');
            
            _args = new Arguments();
            try
            {
                _args.ParseArguments(args, cmdlineDict);
            }
            catch (Exception e)
            {
                return Usage("Oops. Something bad happened with the parameters that you gave me.\n\nException:\n" + e.ToString());
            }

            if (_args.Usage)
                return Usage();

            if (string.IsNullOrEmpty(_args.Target))
                return Usage("Must specify a target to connect to. Use: [emulator|xde]/[device|phone]");

            if (string.IsNullOrEmpty(_args.App) && string.IsNullOrEmpty(_args.Xap))
                return Usage("Must specify one of either -app or -xap");

            if (!string.IsNullOrEmpty(_args.App) && !string.IsNullOrEmpty(_args.Xap))
                return Usage("Confused me, you have. Should I use -app or -xap?");

            if (!_args.Install && !_args.Update && !_args.Launch)
                return Usage("Yawn. You haven't asked me to install, update or launch the app, so what do you want me to do?");

            if (!Connect(_args.Target))
                return 1;

            if (!string.IsNullOrEmpty(_args.Xap))
                _args.Xap = _args.Xap.Replace('=', ':');

            if (_args.Install)
                Install(_args.Xap);

            if (_args.Update)
                Update(_args.Xap);

            if (_args.Launch)
                Launch(new Guid(_args.App));

            return 0;
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
Usage:
    -target
    -app
    -xap
    -install
    -update
    -launch
    -usage
");

            return 1;
        }
    }
}
