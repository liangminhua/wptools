using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.SmartDevice.Connectivity;
using System.Collections.ObjectModel;
using System.IO;

namespace WindowsPhone.Tools
{
    public class RemoteAppIsoStoreItem : INotifyPropertyChanged
    {
        private Device _device;
        private RemoteApplication _app;

        public string Name { get; set; }

        /// <summary>
        /// Is this item the top level application object
        /// </summary>
        public bool IsApplication { get; set; }

        public RemoteFileInfo RemoteFile { get; private set; }

        private string _path;

        private ObservableCollection<RemoteAppIsoStoreItem> _children = new ObservableCollection<RemoteAppIsoStoreItem>();
        public ObservableCollection<RemoteAppIsoStoreItem> Children
        {
            get { return _children; }

            // no set - cannot be replaced
        }

        private bool _updated = false;

        /// <summary>
        /// Construct a new IsoStore representation for this xap
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xap"></param>
        public RemoteAppIsoStoreItem(Microsoft.SmartDevice.Connectivity.Device device, RemoteApplication app)
        {
            this._device = device;
            this._app = app;
            
            // the public constructor is only used to construct the first level
            // which represents the app itself
            Name = app.ProductID.ToString();

            _path = "";

            IsApplication = true;
        }

        private RemoteAppIsoStoreItem(RemoteApplication app, RemoteFileInfo remoteFile)
        {
            _app = app;
            RemoteFile = remoteFile;
            
            string name = RemoteFile.Name;

            Name = Path.GetFileName(name);

            // "\\Applications\\Data\\8531f2be-f4c3-4822-9fa6-bcc70c9d50a8\\Data\\IsolatedStore\\\\Shared"

            _path = name.Substring(name.IndexOf("IsolatedStore\\") + "IsolatedStore\\".Length);

        }

        /// <summary>
        /// Call when you are ready to pull down the data associated with this object
        /// </summary>
        public void Update()
        {

            if (_updated || (RemoteFile != null && !RemoteFile.IsDirectory())) 
            {
                return;
            }

            _updated = true;

            RemoteIsolatedStorageFile remoteIso = _app.GetIsolatedStore();

            List<RemoteFileInfo> remoteFiles;

            try
            {
                remoteFiles = remoteIso.GetDirectoryListing(_path);

                foreach (RemoteFileInfo remoteFile in remoteFiles)
                {
                    Children.Add(new RemoteAppIsoStoreItem(_app, remoteFile));
                }
            }
            catch (FileNotFoundException)
            {
                // no files, oh well :)
            }

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
