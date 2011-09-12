using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;

namespace WindowsPhoneToolbox
{
    class StreamToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BitmapImage image = new BitmapImage();

            try
            {
                if (value != null)
                {
                    image.BeginInit();
                    image.StreamSource = value as Stream;
                    image.EndInit();
                }
            }
            catch {
                image = FileTypeToIconConverter.imageApp;
            }

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
