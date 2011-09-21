using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SmartDevice.Connectivity;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace WindowsPhone.Tools
{

    public class WindowsPhoneDevice : INotifyPropertyChanged
    {
        #region Statics

        /// <summary>
        /// Retrieve possible devices from CoreCon
        /// </summary>
        /// <returns></returns>
        public static List<Device> GetDevices()
        {
            List<Device> list = new List<Device>();

            DatastoreManager manager = new DatastoreManager(CultureInfo.InvariantCulture.LCID);

            foreach (Platform platform in manager.GetPlatforms())
            {
                // Note: CoreCon 10.0 only seems to support Windows Phone so no filtering is required
                list.AddRange(platform.GetDevices());
            }

            return list;
        }

        #endregion

        #region Properties

        private List<Device> _devices;
        public List<Device> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = GetDevices();

                    // set CurrentDevice to a default
                    if (_devices != null)
                        CurrentDevice = _devices[0];
                }

                return _devices;
            }
            set
            {
                if (_devices != value)
                {
                    _devices = value;

                    NotifyPropertyChanged("Devices");
                }
            }
        }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get { return _currentDevice; }
            set
            {
                if (_currentDevice != value)
                {
                    _currentDevice = value;

                    NotifyPropertyChanged("CurrentDevice");
                }
            }
        }

        private SystemInfo _systemInfo;
        public SystemInfo SystemInfo
        {
            get { return _systemInfo; }
            set
            {
                if (_systemInfo != value)
                {
                    _systemInfo = value;

                    NotifyPropertyChanged("SystemInfo");
                }
            }
        }

        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set
            {
                if (_connected != value)
                {
                    _connected = value;

                    NotifyPropertyChanged("Connected");

                    if (_currentDevice != null)
                    {
                        DeviceType = (_currentDevice.IsEmulator() ? "EMULATOR" : "PHONE");
                    }
                    else
                    {
                        DeviceType = "UNKNOWN";
                    }
                }
            }
        }

        private bool _isError;
        public bool IsError
        {
            get { return _isError; }
            set
            {
                if (_isError != value)
                {
                    _isError = value;

                    NotifyPropertyChanged("IsError");
                }
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;

                    NotifyPropertyChanged("StatusMessage");
                }
            }
        }

        private string _deviceType;
        public string DeviceType
        {
            get { return _deviceType; }
            set
            {
                if (_deviceType != value)
                {
                    _deviceType = value;

                    NotifyPropertyChanged("DeviceType");
                }
            }
        }

        private Collection<RemoteApplicationEx> _installedApplications;
        public Collection<RemoteApplicationEx> InstalledApplications
        {
            get { return _installedApplications; }
            set
            {
                if (_installedApplications != value)
                {
                    _installedApplications = value;

                    NotifyPropertyChanged("InstalledApplications");
                }
            }
        }

        private List<RemoteAppIsoStoreItem> _remoteIsoStores;
        public List<RemoteAppIsoStoreItem> RemoteIsoStores
        {
            get { return _remoteIsoStores; }
            set
            {
                if (_remoteIsoStores != value)
                {
                    _remoteIsoStores = value;

                    NotifyPropertyChanged("RemoteIsoStores");
                }
            }
        }

        #endregion

        private Device _connectedDevice = null;

        public bool Connect()
        {
            if (CurrentDevice != null)
            {
                // we're already connected to this device! :)
                if (CurrentDevice == _connectedDevice)
                    return true;

                try
                {
                    // disconnect the existing device
                    if (_connectedDevice != null)
                        _connectedDevice.Disconnect();

                    CurrentDevice.Connect();

                    SystemInfo = CurrentDevice.GetSystemInfo();

                    StatusMessage = "Connected to " + CurrentDevice.Name + "!";

                    Connected = true;
                    IsError = false;

                    RefreshInstalledApps();
                }
                catch (SmartDeviceException ex)
                {
                    if (ex.Message == "0x89731811")
                    {
                        StatusMessage = "Connection Error! Zune does not appear to be running";
                    }
                    else if (ex.Message == "0x89731812")
                    {
                        StatusMessage = "Connection Error! Unlock your phone and make sure it is paired with Zune";
                    }
                    else
                    {
                        StatusMessage = "Connection Error! Message: " + ex.Message;
                    }

                    IsError          = true;
                    Connected        = false;
                    _connectedDevice = null;
                    SystemInfo       = null;                    
                }
            }

            return Connected;
        }

        public void RefreshInstalledApps()
        {
            //RemoteApplicationEx

            Collection<RemoteApplication> installed = CurrentDevice.GetInstalledApplications();

            Collection<RemoteApplicationEx> installedCollection = new Collection<RemoteApplicationEx>();

            foreach (RemoteApplication app in installed)
                installedCollection.Add(new RemoteApplicationEx(app));

            InstalledApplications = installedCollection;

            RefreshRemoteIsoStores();
        }

        private void RefreshRemoteIsoStores()
        {
            List<RemoteAppIsoStoreItem> xapIsoStores = new List<RemoteAppIsoStoreItem>();

            foreach (RemoteApplicationEx app in _installedApplications)
            {
                xapIsoStores.Add(new RemoteAppIsoStoreItem(_currentDevice, app));
            }

            RemoteIsoStores = xapIsoStores;
        }

        # region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }

}
