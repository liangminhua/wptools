using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using WindowsPhone.Tools;
using Microsoft.SmartDevice.Connectivity;

namespace WindowsPhonePowerTools
{
    /// <summary>
    /// Interaction logic for DeviceConnectionIndicator.xaml
    /// </summary>
    public partial class DeviceConnectionIndicator : UserControl//, INotifyPropertyChanged
    {
/*
        #region DataContext sink

        private INotifyPropertyChanged _dataContext;
        private WindowsPhoneDevice _currentDevice;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_dataContext != e.NewValue)
            {
                // deregister from the old control
                if (_dataContext != null) 
                {
                    _dataContext.PropertyChanged -= _dataContext_PropertyChanged;
                }

                // remember the new DataContext and register for notifications
                _dataContext   = e.NewValue as INotifyPropertyChanged;
                _currentDevice = _dataContext as WindowsPhoneDevice;

                _dataContext.PropertyChanged += _dataContext_PropertyChanged;
            }
        }

        private void _dataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            WindowsPhoneDevice device = sender as WindowsPhoneDevice;

            if (device != null)
            {
                if (e.PropertyName == "Connected")
                {
                    Connected = device.Connected;

                    if (_currentDevice != null)
                    {
                        DeviceType = (_currentDevice.CurrentDevice.IsEmulator() ? "EMULATOR" : "PHONE");
                    }
                    else
                    {
                        DeviceType = "UNKNOWN";
                    }
                }
            }
        }

        #endregion

        #region Dependency Properties

        public Device Device
        {
            get { return (Device)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
           DependencyProperty.Register("Device", typeof(Device), typeof(DeviceConnectionIndicator), new UIPropertyMetadata(null));

        public SolidColorBrush DisconnectedColor
        {
            get { return (SolidColorBrush)GetValue(DisconnectedColorProperty); }
            set 
            { 
                SetValue(DisconnectedColorProperty, value);

                if (!Connected)
                    StatusColor = value;
            }
        }

        public static readonly DependencyProperty DisconnectedColorProperty =
           DependencyProperty.Register("DisconnectedColor", typeof(SolidColorBrush), typeof(DeviceConnectionIndicator), new UIPropertyMetadata(null));

        public SolidColorBrush ConnectedColor
        {
            get { return (SolidColorBrush)GetValue(ConnectedColorProperty); }
            set 
            { 
                SetValue(ConnectedColorProperty, value);

                if (Connected)
                    StatusColor = value;
            }
        }

        public static readonly DependencyProperty ConnectedColorProperty =
           DependencyProperty.Register("ConnectedColor", typeof(SolidColorBrush), typeof(DeviceConnectionIndicator), new UIPropertyMetadata(null));

        #endregion

        #region Properties


        #endregion

        public DeviceConnectionIndicator()
        {
            InitializeComponent();

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(OnDataContextChanged);
        }
        
        #region Events
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Connected = false;
        }

        #endregion

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
 * */
    }
}
