using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Test.CommandLineParsing;

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

            if (!string.IsNullOrEmpty(_args.App) && !string.IsNullOrEmpty(_args.App))
                return Usage("Confused me, you have. Should I use -app or -xap?");

            if (!_args.Install && !_args.Update && !_args.Launch)
                return Usage("Yawn. You haven't asked me to install, update or launch the app, so what do you want me to do?");

            if (!Connect())
                return 1;

            if (_args.Install)
                Install();

            if (_args.Update)
                Update();

            if (_args.Launch)
                Launch();

            return 0;
        }

        private static bool Connect()
        {
            throw new NotImplementedException();
        }

        private static void Install()
        {
            throw new NotImplementedException();
        }

        private static void Update()
        {
            throw new NotImplementedException();
        }

        private static void Launch()
        {
            throw new NotImplementedException();
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
