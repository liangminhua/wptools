using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using Microsoft.SmartDevice.Connectivity;
using System.IO;

namespace WindowsPhonePowerTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int MIN_SEC_BETWEEN_EXCEPTIONS = 5;
        private const int EXIT_WITH_ERROR = 1;

        public App()
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Application.Current.DispatcherUnhandledException -= Current_DispatcherUnhandledException;
        }

        private DateTime _lastException = DateTime.Now;
        private bool _ignoreExceptions = false;

        /// <summary>
        /// It's kind of gross to capture and report these errors here, but since there are multiple places that can trigger
        /// these exceptions it's kind of nicer than having lots of try/catch. The alternative may be to add try/catches that
        /// just send the exception to a global responder ("HandleSmartDeviceException") which still centralises things, but
        /// would probably be best since it wouldn't trigger the global exception handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            bool needGenericMessage = false;
            WindowsPhonePowerTools.OldMainWindow mainWindow = WindowsPhonePowerTools.OldMainWindow.Current;

            // handled from the start so that exceptions that cause us to exit at least don't crash out, but exit when
            // the user hits "ok"
            e.Handled = true;

            // this flag is set when we're being flooded with exceptions and are exiting, so don't show a message
            if (_ignoreExceptions)
                return;

            // if we're being flooded with unhandled exceptions, report the exception and exit
            if (DateTime.Now.Subtract(_lastException).TotalSeconds < MIN_SEC_BETWEEN_EXCEPTIONS)
                _ignoreExceptions = true;

            _lastException = DateTime.Now;

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
                    else if (e.Exception is XamlParseException)
                    {
                        MissingMethodException missingEx = e.Exception.InnerException as MissingMethodException;

                        if (missingEx != null)
                        {
                            MessageBox.Show("A standard phone communication function is missing from your system (see below for details). Please upgrade your Windows Phone SDK to the latest tools found on http://create.msdn.com and try again.\n\nMessage: " + missingEx.Message + "\n\nStack: " + missingEx.ToString().Substring(0, 1000));

                            // non recoverable error here...
                            _ignoreExceptions = true;
                        }
                    }
                    else if (e.Exception is InvalidDataException)
                    {
                        mainWindow.ShowError("We couldn't open the xap file you gave us! It was either invalid or a Marketplace xap, which is not supported via this tool.");
                    }
                    else if (e.Exception is DeviceNotConnectedException)
                    {
                        mainWindow.ShowError("Your device disconnected, we tried to reconnect, but we were out of luck :(\n\nReconnect your device and try again");
                        mainWindow.Device.Connected = false;
                    }
                    else if (e.Exception is RemoteIsolatedStorageException)
                    {
                        mainWindow.ShowError("Oops! We ran into a problem navigating the device's file system. You may need to reconnect your device.\n\nRaw Error: " + e.Exception.Message);

                        if (e.Exception.Message.ToLower() == "device not connected")
                        {
                            mainWindow.Device.Connected = false;
                        }
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

                // if we're ignoring then we should be exiting
                if (_ignoreExceptions)
                    Application.Current.Shutdown(EXIT_WITH_ERROR);
            }
        }
    }
}
