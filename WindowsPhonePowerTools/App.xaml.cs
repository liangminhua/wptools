using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;

namespace WindowsPhonePowerTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Application.Current.DispatcherUnhandledException -= Current_DispatcherUnhandledException;
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            bool needGenericMessage = true;

            if (e != null)
            {
                if (e.Exception != null)
                {
                    if (e.Exception.Message == "0x81030118")
                    {
                        MessageBox.Show("Oops! Looks like your device is not Developer Unlocked.\n\nUnlock your phone using the \"Windows Phone Developer Registration\" tool and try again",
                            "Phone Developer Locked", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        needGenericMessage = true;
                    }

                }
                else
                {
                    needGenericMessage = true;
                }

                if (needGenericMessage)
                {
                    MessageBox.Show("Oh oh. Something bad happened, that we didn't anticipate. Please file a bug at http://wptools.codebox.com.\n\n" + (e == null ? "Null Exception. Weird" : e.ToString()), "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                e.Handled = true;
            }
        }
    }
}
