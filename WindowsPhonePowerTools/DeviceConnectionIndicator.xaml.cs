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

namespace WindowsPhonePowerTools
{
    /// <summary>
    /// Interaction logic for DeviceConnectionIndicator.xaml
    /// </summary>
    public partial class DeviceConnectionIndicator : UserControl, INotifyPropertyChanged
    {

        #region Dependency Properties

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
           DependencyProperty.Register("DisconnectedColorProperty", typeof(SolidColorBrush), typeof(DeviceConnectionIndicator), new UIPropertyMetadata(null));


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
           DependencyProperty.Register("ConnectedColorProperty", typeof(SolidColorBrush), typeof(DeviceConnectionIndicator), new UIPropertyMetadata(null));

        #endregion

        #region Properties

        private bool? _connected;
        public bool Connected
        {
            get { return (_connected == true ? true : false); }
            set
            {
                if (_connected != value)
                {
                    _connected = value;

                    NotifyPropertyChanged("Connected");

                    if (_connected == true)
                    {
                        StatusColor = ConnectedColor;
                    }
                    else
                    {
                        StatusColor = DisconnectedColor;
                    }
                }
            }
        }

        private SolidColorBrush _statusColor;
        public SolidColorBrush StatusColor
        {
            get { return _statusColor; }
            private set
            {
                if (_statusColor != value)
                {
                    _statusColor = value;

                    NotifyPropertyChanged("StatusColor");
                }
            }
        }

        #endregion

        public DeviceConnectionIndicator()
        {
            InitializeComponent();
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

    }
}
