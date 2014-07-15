using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WindowsPhone.Tools;
using Microsoft.SmartDevice.Connectivity;
using System.IO;

namespace WindowsPhonePowerTools
{
    class FileTypeToIconConverter : IValueConverter
    {
        static BitmapImage imageDir = new BitmapImage(new Uri("images/folder.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage imageOpenDir = new BitmapImage(new Uri("images/folder_open.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage imageApp = new BitmapImage(new Uri("images/WindowsPhoneIcon.png", UriKind.RelativeOrAbsolute));
        static BitmapImage imageUnknown = new BitmapImage(new Uri("images/unknown.png", UriKind.RelativeOrAbsolute));

        static Dictionary<string, BitmapImage> fileTypeImages = new Dictionary<string, BitmapImage>()
        {
            {"png", new BitmapImage(new Uri("images/png.png", UriKind.RelativeOrAbsolute))},
            {"jpg", new BitmapImage(new Uri("images/jpeg.png", UriKind.RelativeOrAbsolute))},
            {"jpeg", new BitmapImage(new Uri("images/jpeg.png", UriKind.RelativeOrAbsolute))},
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

            RemoteAppIsoStoreItem isoStoreItem = value as RemoteAppIsoStoreItem;

            if (isoStoreItem != null)
            {
                if (isoStoreItem.IsApplication)
                {
                    return imageApp;
                } 
                else if (isoStoreItem.IsRemoteStore)
                {
                    return imageDir;
                }

                var file = isoStoreItem.RemoteFile;

                if (file != null)
                {
                    if (file.IsDirectory())
                    {
                        if (isoStoreItem.Opened)
                        {
                            return imageOpenDir;
                        }
                        else
                        {
                            return imageDir;
                        }
                    }
                    else
                    {
                        BitmapImage img;

                        if (fileTypeImages.TryGetValue(file.GetExtension(), out img))
                            return img;
                    }
                }
                
            }

            return imageUnknown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
