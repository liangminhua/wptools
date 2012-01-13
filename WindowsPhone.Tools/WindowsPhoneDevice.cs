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
        /// Minimum support build is one of the recent Mango previews and upwards (RTM is 7720)
        /// </summary>
        public const int MIN_SUPPORTED_BUILD_NUMBER = 7600;

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
            get {
                // attempt to reconnect the device if we previously connected it and
                // it is not longer connected
                if (_currentDevice != null && Connected && !_currentDevice.IsConnected())
                {
                    // attempt to reconnect
                    Connect();

                    // still not connected?
                    if (!_currentDevice.IsConnected())
                    {
                        CurrentDevice = null;

                        throw new DeviceNotConnectedException(StatusMessage);
                    }
                }

                return _currentDevice; 
            }
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
            // do not use CurrentDevice here (rather, use _currentDevice) because CurrentDevice.Get() 
            // can call back into Connect

            if (_currentDevice != null)
            {
                // we're already connected to this device! :)
                if (_currentDevice == _connectedDevice && _connectedDevice.IsConnected())
                    return true;

                try
                {
                    // disconnect the existing device
                    if (_connectedDevice != null)
                        _connectedDevice.Disconnect();

                    _currentDevice.Connect();

                    SystemInfo = _currentDevice.GetSystemInfo();

                    if (SystemInfo.OSBuildNo < MIN_SUPPORTED_BUILD_NUMBER)
                    {
                        throw new Exception("Windows Phone Power Tools only support build " + MIN_SUPPORTED_BUILD_NUMBER + " and above. This device is on " + SystemInfo.OSBuildNo + ".");
                    }

                    StatusMessage = "Connected to " + _currentDevice.Name + "!";

                    Connected = true;
                    IsError = false;

                    RefreshInstalledApps();
                }
                catch (Exception ex)
                {
                    SmartDeviceException smartDeviceEx = ex as SmartDeviceException;

                    if (smartDeviceEx != null)
                    {
                        if (ex.Message == "0x89731811")
                        {
                            StatusMessage = "Connection Error! Zune is either not running or not connected to the device.";
                        }
                        else if (ex.Message == "0x89731812")
                        {
                            StatusMessage = "Connection Error! Unlock your phone and make sure it is paired with Zune";
                        }
                        else
                        {
                            StatusMessage = "Connection Error! Message: " + ex.Message;
                        }
                    }
                    else
                    {
                        StatusMessage = ex.Message;
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
                xapIsoStores.Add(new RemoteAppIsoStoreItem(CurrentDevice, app));
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
