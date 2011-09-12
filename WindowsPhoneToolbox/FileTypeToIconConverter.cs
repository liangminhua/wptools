using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WindowsPhone.Tools;
using Microsoft.SmartDevice.Connectivity;
using System.IO;

namespace WindowsPhoneToolbox
{
    class FileTypeToIconConverter : IValueConverter
    {
        static BitmapImage imageDir = new BitmapImage(new Uri("images/folder.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage imageApp = new BitmapImage(new Uri("images/WindowsPhoneIcon.png", UriKind.RelativeOrAbsolute));
        static BitmapImage imageUnknown = new BitmapImage(new Uri("images/unknown.png", UriKind.RelativeOrAbsolute));

        static Dictionary<string, BitmapImage> fileTypeImages = new Dictionary<string, BitmapImage>()
        {
            {"png", new BitmapImage(new Uri("images/png.png", UriKind.RelativeOrAbsolute))},
            {"jpg", new BitmapImage(new Uri("images/jpeg.png", UriKind.RelativeOrAbsolute))},
            {"jpeg", new BitmapImage(new Uri("images/jpeg.png", UriKind.RelativeOrAbsolute))}, //TODO: hmm...
        };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Stream stream = value as Stream;

            if (stream != null)
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.StreamSource = stream;
                img.EndInit();

                return img;
            }

            RemoteFileInfo file = value as RemoteFileInfo;

            if (file == null)
            {
                return imageApp;
            }
            else if (file.IsDirectory())
            {
                return imageDir;
            }
            else
            {
                BitmapImage img;

                if (fileTypeImages.TryGetValue(file.GetExtension(), out img))
                    return img;
            }

            return imageUnknown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
