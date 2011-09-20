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
using System.Windows.Controls.Primitives;

namespace MetroUI
{
    /// <summary>
    /// This control has a slight logic bug, if you click and drag outside of the control,
    /// release, click and drag back in and then release it will trigger a Click.
    /// 
    /// At some point it would be nice to make this a proper control, based on ButtonBase
    /// </summary>
    public partial class ImageButton : UserControl, INotifyPropertyChanged
    {
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); CurrentImage = value; }
        }

        public static readonly DependencyProperty ImageProperty =
           DependencyProperty.Register("ImageProperty", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));

        public ImageSource HoverImage
        {
            get { return (ImageSource)GetValue(HoverImageProperty); }
            set { SetValue(HoverImageProperty, value); }
        }

        public static readonly DependencyProperty HoverImageProperty =
           DependencyProperty.Register("HoverImageProperty", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));

        private ImageSource _currentImage;
        public ImageSource CurrentImage
        {
            get { return (_currentImage == null ? Image : _currentImage); }
            set
            {
                if (_currentImage != value)
                {
                    _currentImage = value;

                    NotifyPropertyChanged("CurrentImage");
                }
            }
        }

        public event RoutedEventHandler Click;

        public ImageButton()
        {
            InitializeComponent();
        }

        #region Mouse Handling

        private bool _overControl = false;
        private bool _clickStartedOverControl = false;

        private void ImageButtonUserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            CurrentImage = HoverImage;

            _overControl = true;
        }

        private void ImageButtonUserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            CurrentImage = Image;

            _overControl = false;
        }

        private void ImageButtonUserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentImage = Image;
            _clickStartedOverControl = true;
        }

        private void ImageButtonUserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CurrentImage = HoverImage;

            if (_overControl && _clickStartedOverControl && Click != null)
                Click(this, new RoutedEventArgs());

            _clickStartedOverControl = false;
        }

        #endregion

        #region INotifyPropertyChanged

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
