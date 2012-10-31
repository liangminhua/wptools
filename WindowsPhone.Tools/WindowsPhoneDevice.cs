using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SmartDevice.Connectivity;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Microsoft.SmartDevice.MultiTargeting.Connectivity;
using Microsoft.SmartDevice.Connectivity.Interface;
using System.Runtime.CompilerServices;

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
        public static List<ConnectableDevice> GetDevices()
        {
            List<ConnectableDevice> list = new List<ConnectableDevice>();
            
            MultiTargetingConnectivity multiConnect = new MultiTargetingConnectivity(CultureInfo.InvariantCulture.LCID);

            foreach (ConnectableDevice device in multiConnect.GetConnectableDevices(false))
            {
                list.Add(device);
            }

            return list;
        }

        #endregion

        #region Properties

        private List<ConnectableDevice> _devices;
        public List<ConnectableDevice> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = GetDevices();

                    // set CurrentDevice to a default
                    if (_devices != null)
                        CurrentConnectableDevice = _devices[0];
                }

                return _devices;
            }
            set
            {
                if (_devices != value)
                {
                    _devices = value;

                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private IDevice _currentDevice;
        public IDevice CurrentDevice
        {
            get {
           
                /* 
                 * The new ConnectableDevice model does not support an IsConnected flag, should possibly create a wrapper?
                 *
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

                 */

                return _currentDevice; 
            }
            set
            {
                if (_currentDevice != value)
                {
                    _currentDevice = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private ConnectableDevice _currentConnectableDevice;
        public ConnectableDevice CurrentConnectableDevice
        {
            get
            {
                return _currentConnectableDevice;
            }
            set
            {
                if (_currentConnectableDevice != value)
                {
                    _currentConnectableDevice = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private ISystemInfo _systemInfo;
        public ISystemInfo SystemInfo
        {
            get { return _systemInfo; }
            set
            {
                if (_systemInfo != value)
                {
                    _systemInfo = value;

                    NotifyPropertyChanged();
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

                    NotifyPropertyChanged();

                    if (_currentDevice != null)
                    {
                        DeviceType = (_currentConnectableDevice.IsEmulator() ? "EMULATOR" : "PHONE");
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

                    NotifyPropertyChanged();
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

                    NotifyPropertyChanged();
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

                    NotifyPropertyChanged();
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

                    NotifyPropertyChanged();
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

                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        public bool Connect()
        {
            // do not use CurrentDevice here (rather, use _currentDevice) because CurrentDevice.Get() 
            // can call back into Connect

            if (CurrentConnectableDevice != null)
            {
                // we're already connected to this device! :)
                //if (_currentDevice == _connectedDevice/* && _connectedDevice.IsConnected()*/)
                //    return true;

                try
                {
                    // disconnect the existing device
                    if (CurrentDevice != null)
                        CurrentDevice.Disconnect();

                    CurrentDevice = CurrentConnectableDevice.Connect();

                    SystemInfo = CurrentDevice.GetSystemInfo();

                    if (SystemInfo.OSBuildNo < MIN_SUPPORTED_BUILD_NUMBER)
                    {
                        throw new Exception("Windows Phone Power Tools only support build " + MIN_SUPPORTED_BUILD_NUMBER + " and above. This device is on " + SystemInfo.OSBuildNo + ".");
                    }

                    StatusMessage = "Currently connected to " + _currentConnectableDevice.Name;

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
                        else if (ex.Message == "0x89740005")
                        {
                            StatusMessage = "Developer unlock has expired. Lock and re-unlock your phone using the SDK registration tool";
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
                    SystemInfo       = null;                    
                }
            }

            return Connected;
        }

        public void RefreshInstalledApps()
        {
            //RemoteApplicationEx
            
            Collection<IRemoteApplication> installed = CurrentDevice.GetInstalledApplications();

            Collection<RemoteApplicationEx> installedCollection = new Collection<RemoteApplicationEx>();

            foreach (IRemoteApplication app in installed)
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

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

}
