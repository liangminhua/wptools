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
        private RemoteApplicationEx _appEx;

        public string Name { get; set; }

        /// <summary>
        /// Is this item the top level application object
        /// </summary>
        public bool IsApplication { get; set; }

        public RemoteFileInfo RemoteFile { get; private set; }

        /// <summary>
        /// The weird and wonderful world of Icons. Will either return a stream with the icon in it
        /// or will return RemoteFile for you to convert nicely into a generic icon.
        /// </summary>
        public object Icon
        {
            get
            {
                if (IsApplication)
                    return _appEx.Icon;
                else
                    return RemoteFile;
            }
        }

        private string _path;

        private ObservableCollection<RemoteAppIsoStoreItem> _children = new ObservableCollection<RemoteAppIsoStoreItem>();
        public ObservableCollection<RemoteAppIsoStoreItem> Children
        {
            get { return _children; }

            // no set - cannot be replaced
        }

        private RemoteAppIsoStoreItem _parent;
        public RemoteAppIsoStoreItem Parent
        {
            get { return _parent; }
            private set { _parent = value; }
        }

        private bool _updated = false;

        /// <summary>
        /// Construct a new toplevel IsoStore representation for this xap
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xap"></param>
        public RemoteAppIsoStoreItem(Microsoft.SmartDevice.Connectivity.Device device, RemoteApplicationEx app)
        {
            this._device = device;
            this._app    = app.RemoteApplication;
            this._appEx  = app;
            
            // the public constructor is only used to construct the first level
            // which represents the app itself
            Name = app.RemoteApplication.ProductID.ToString();

            _path = "";

            IsApplication = true;

            // add a fake item so that anyone binding to us will show expanders
            Children.Add(new FakeRemoteAppIsoStoreItem(this));
        }

        /// <summary>
        /// Construct a representation of a real remote file (or directory)
        /// </summary>
        /// <param name="app"></param>
        /// <param name="remoteFile"></param>
        private RemoteAppIsoStoreItem(RemoteApplicationEx app, RemoteFileInfo remoteFile, RemoteAppIsoStoreItem parent)
        {
            _app   = app.RemoteApplication;
            _appEx = app;
            Parent = parent;

            RemoteFile = remoteFile;
            
            string name = RemoteFile.Name;

            Name = Path.GetFileName(name);

            // "\\Applications\\Data\\8531f2be-f4c3-4822-9fa6-bcc70c9d50a8\\Data\\IsolatedStore\\\\Shared"

            _path = RemoteFile.GetRelativePath();

            if (RemoteFile.IsDirectory())
            {
                Children.Add(new FakeRemoteAppIsoStoreItem(this));
            }

        }

        /// <summary>
        /// Copies the remote item to the path specified. If this is a directory then it 
        /// will recursively copy it down.
        /// </summary>
        /// <param name="path">Returns the full local path (i.e. localPath + [file/dir]name)</param>
        public string Get(string localPath)
        {
            RemoteIsolatedStorageFile remoteIso = _app.GetIsolatedStore();

            string fullLocalPath = Path.Combine(localPath, Path.GetFileName(Name));

            if (IsApplication || RemoteFile.IsDirectory())
            {
                Directory.CreateDirectory(fullLocalPath);

                // make sure we know about all of our children
                Update();

                foreach (RemoteAppIsoStoreItem item in Children)
                {
                    item.Get(fullLocalPath);
                }

            }
            else
            {

                remoteIso.ReceiveFile(_path, fullLocalPath, true);
            }
                
            return fullLocalPath;
        }

        public void Put(string localFile, string relativeDirectory = "", bool overwrite = false)
        {
            RemoteIsolatedStorageFile remoteIso = _app.GetIsolatedStore();

            FileAttributes attrib = File.GetAttributes(localFile);

            if ((attrib & FileAttributes.Directory) == FileAttributes.Directory)
            {
                string newRelativeDirectory = Path.Combine(relativeDirectory, PathHelper.GetLastDirectory(localFile));
                List<string> files = new List<string>(Directory.GetFiles(localFile));

                files.AddRange(Directory.GetDirectories(localFile));
                
                // create the directory on the device
                remoteIso.CreateDirectory(newRelativeDirectory);

                foreach (string file in files)
                {
                    Put(file, newRelativeDirectory, overwrite);
                }

            }
            else
            {
                string deviceFilename = Path.Combine(relativeDirectory, Path.GetFileName(localFile));

                if (overwrite || !remoteIso.FileExists(deviceFilename))
                {
                    remoteIso.SendFile(localFile, deviceFilename, createNew: true);
                }
            }
        }

        public void Delete()
        {
            RemoteIsolatedStorageFile remoteIso = _app.GetIsolatedStore();

            if (IsApplication)
            {
                // delete everything under this application
                Update();

                foreach (RemoteAppIsoStoreItem item in Children)
                {
                    item.Delete();
                }

                // leave things in a clean state with the default directories. If the user
                // really wants to kill these they can delete them individually
                string shared = Path.Combine(_path, "Shared");

                remoteIso.CreateDirectory(shared);
                remoteIso.CreateDirectory(Path.Combine(shared, "Transfers"));
                remoteIso.CreateDirectory(Path.Combine(shared, "ShellContent"));
            } 
            else if (RemoteFile.IsDirectory())
            {
                remoteIso.DeleteDirectory(_path);
            }
            else
            {
                remoteIso.DeleteFile(_path);
            }
        }

        /// <summary>
        /// Used to create a fake entry so that directories can be queried
        /// </summary>
        internal RemoteAppIsoStoreItem(RemoteAppIsoStoreItem parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// Call when you are ready to pull down the data associated with this object
        /// </summary>
        public void Update(bool force = false)
        {

            if ((_updated && !force) || (RemoteFile != null && !RemoteFile.IsDirectory())) 
            {
                return;
            }

            Children.Clear();

            _updated = true;

            RemoteIsolatedStorageFile remoteIso = _app.GetIsolatedStore();

            List<RemoteFileInfo> remoteFiles;

            try
            {
                remoteFiles = remoteIso.GetDirectoryListing(_path);

                foreach (RemoteFileInfo remoteFile in remoteFiles)
                {
                    Children.Add(new RemoteAppIsoStoreItem(_appEx, remoteFile, this));
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

    public class FakeRemoteAppIsoStoreItem : RemoteAppIsoStoreItem
    {
        public FakeRemoteAppIsoStoreItem(RemoteAppIsoStoreItem parent) : base(parent) { }
    }


}
