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

        /// <summary>
        /// It's kind of gross to capture and report these errors here, but since there are multiple places that can trigger
        /// these exceptions it's kind of nicer than having lots of try/catch. The alternative may be to add try/catches that
        /// just send the exception to a global responder ("HandleSmartDeviceException") which still centralises things.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            bool needGenericMessage = false;

            if (e != null)
            {
                if (e.Exception != null)
                {
                    if (e.Exception.Message == "0x81030118")
                    {
                        MessageBox.Show("Oops! Looks like your device is not Developer Unlocked.\n\nUnlock your phone using the \"Windows Phone Developer Registration\" tool and try again",
                            "Phone Developer Locked", MessageBoxButton.OK, MessageBoxImage.Warning);
                    } 
                    else if (e.Exception.Message == "0x81030120") 
                    {
                        MessageBox.Show(
                            "I tried to install / update your xap but your phone rejected it because it uses capabilities that your developer account is not provisioned for (usually ID_CAP_INTEROPSERVICES).\n\nIf you believe that your account should be provisioned for these then please contact AppHub support and try unregistering and reregistering your phone.\n\nIf you believe that this is a bug in the tools, and your XAP should have installed correctly, feel free to log a bug (with a repro xap) at http://wptools.codeplex.com",
                            "Denied by Developer Unlock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else if (e.Exception.Message == "0x81030121")
                    {
                        MessageBox.Show(
                            "Unable to launch application that requires ID_CAP_INTEROPSERVICES on your developer account.\n\nIf you believe that this is a mistake, contact AppHub support.\n\nNote: This is a newly enforced restriction in Windows Phone Mango and will apply to previously installed developer Apps that used ID_CAP_INTEROPSERVICES, as well as to any newly installed apps.",
                            "Developer Account Not Allowed to use ID_CAP_INTEROPSERVICES", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else if (e.Exception.Message == "0x81030119")
                    {
                        MessageBox.Show(
                            "Unable to install xap - you've reached the maximum number of developer xaps that you can install. Uninstall a developer app and try again",
                            "Too Many Developer Apps", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    string errString;

                    if (e == null || e.Exception == null)
                    {
                        errString = "Null Exception";
                    }
                    else
                    {
                        // Note: only take 1000 characters, otherwise the stack can easily overflow the screen
                        errString =
                            "Exception: " + e.Exception.ToString().Substring(0, 1000) + "\n" +
                            (e.Exception.InnerException != null ? "Inner Exception: " + e.Exception.InnerException.Message : "");
                    }

                    MessageBox.Show(
                        "Oh oh. Something bad happened, that we didn't anticipate. Please file a bug at http://wptools.codebox.com.\n\n" + errString,
                        "Unhandled Exception in Windows Phone Power Tools",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                e.Handled = true;
            }
        }
    }
}
