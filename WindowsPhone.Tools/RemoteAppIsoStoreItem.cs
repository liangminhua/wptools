//extern alias Wrapper11;
extern alias Wrapper12;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.SmartDevice.Connectivity;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.SmartDevice.Connectivity.Interface;
using Wrapper12.Microsoft.SmartDevice.Connectivity.Wrapper;

//using Wrapper11.Microsoft.SmartDevice.Connectivity.Wrapper;

namespace WindowsPhone.Tools
{
    public class RemoteAppIsoStoreItem : INotifyPropertyChanged
    {
        private IDevice _device;
        private RemoteApplicationEx _appEx;

        // used for Modern apps who have multiple IsoStores
        private IRemoteIsolatedStorageFile _remoteStore;
        
        public IRemoteApplication RemoteApp { get; private set; }

        public string Name { get; set; }

        /// <summary>
        /// Wheteher or not this item has been expanded
        /// </summary>
        public bool Opened { get; private set; }

        /// <summary>
        /// Is this item the top level application object
        /// </summary>
        public bool IsApplication { get; set; }

        public IRemoteFileInfo RemoteFile { get; private set; }

        public bool IsRemoteStore { get; private set; }

        /// <summary>
        /// So, why is this an Object? RemoteFileInfo needs to be polymorphic, accepting
        /// either a RemoteFileInfo or, when that is not available, an IRemoteFileInfo.
        /// This makes binding a lot easier (bind to RemoteFileInfo without caring about the type)
        /// </summary>
        public object RemoteFileInfo { get; private set; }

        /// <summary>
        /// The weird and wonderful world of Icons. Will either return a stream with the icon in it
        /// or will return this object for you to convert nicely into a generic icon.
        /// </summary>
        public object Icon
        {
            get
            {
                if (IsApplication && _appEx.Icon != null)
                    return _appEx.Icon;
                else
                    return this;
            }
            set
            {
                // ignore the set value, it is only here to refresh the icon
                NotifyPropertyChanged("Icon");
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

        /// <summary>
        /// Used to create a fake entry so that directories can be queried
        /// </summary>
        internal RemoteAppIsoStoreItem(RemoteAppIsoStoreItem parent)
        {
            Parent = parent;
        }

        /// <summary>
        /// Construct a new toplevel IsoStore representation for this xap
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xap"></param>
        public RemoteAppIsoStoreItem(IDevice device, RemoteApplicationEx app)
        {
            this._device = device;
            this._appEx  = app;
            
            this.RemoteApp = app.RemoteApplication;
            
            // the public constructor is only used to construct the first level
            // which represents the app itself
            Name = app.Name;

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
        private RemoteAppIsoStoreItem(RemoteApplicationEx app, IRemoteFileInfo remoteFile, IRemoteIsolatedStorageFile remoteStore, RemoteAppIsoStoreItem parent)
        {
            RemoteApp = app.RemoteApplication;

            _remoteStore = remoteStore;
            _appEx = app;
            Parent = parent;

            RemoteFile     = remoteFile;

            // if we can't get the internal object, set it back to the default, which is remoteFile itself.
            // remoteFile only exposes a subset of properties, but these are better than none
            RemoteFileInfo = (object)remoteFile.GetInternalRemoteFileInfo() ?? (object)remoteFile;

            string name = RemoteFile.Name;

            Name = Path.GetFileName(name);

            // "\\Applications\\Data\\8531f2be-f4c3-4822-9fa6-bcc70c9d50a8\\Data\\IsolatedStore\\\\Shared"
            
            _path = RemoteFile.GetRelativePath();

            // Modern applications are rooted by their IsoStore object so don't need the full path
            if (_path.Contains("%"))
                _path = "";

            if (RemoteFile.IsDirectory())
            {
                Children.Add(new FakeRemoteAppIsoStoreItem(this));
            }

        }

        public RemoteAppIsoStoreItem(RemoteApplicationEx app, string store)
        {
            _appEx = app;
            Name = store;
            _remoteStore = app.RemoteApplication.GetIsolatedStore(store);
            
            // these are all fake directories
            Children.Add(new FakeRemoteAppIsoStoreItem(this));

            IsRemoteStore = true;
        }

        /// <summary>
        /// Copies the remote item to the path specified. If this is a directory then it 
        /// will recursively copy it down.
        /// </summary>
        /// <param name="path">Returns the full local path (i.e. localPath + [file/dir]name)</param>
        public string Get(string localPath, bool overwrite)
        {
            string fullLocalPath = Path.Combine(localPath, Path.GetFileName(Name));

            if (IsApplication || IsRemoteStore || RemoteFile.IsDirectory())
            {
                Directory.CreateDirectory(fullLocalPath);

                // make sure we know about all of our children
                Update();

                foreach (RemoteAppIsoStoreItem item in Children)
                {
                    item.Get(fullLocalPath, overwrite);
                }

            }
            else
            {

                if (overwrite || !File.Exists(fullLocalPath))
                    _remoteStore.ReceiveFile(_path, fullLocalPath, true);
            }
                
            return fullLocalPath;
        }

        public void Put(string localFile, bool overwrite = false)
        {
            // relative directory is:
            // "" - if it's an application
            // _path - if it's a directory (since Path.GetDirectoryName will actually strip out the directory name)
            // Path.GetDirectoryName(_path) - if it's a file, since this will strip out the file name and leave us with a directory
            string relativeDirectory = 
                (RemoteFile == null ? "" : 
                    (RemoteFile.IsDirectory() ? _path : Path.GetDirectoryName(_path)));

            Put(localFile, relativeDirectory, overwrite);
        }

        public void Put(string localFile, string relativeDirectory = "", bool overwrite = false)
        {
            // TODO: this shouldn't die silently
            if (IsApplication)
                return;

            FileAttributes attrib = File.GetAttributes(localFile);

            if ((attrib & FileAttributes.Directory) == FileAttributes.Directory)
            {
                string newRelativeDirectory = Path.Combine(relativeDirectory, PathHelper.GetLastDirectory(localFile));
                List<string> files = new List<string>(Directory.GetFiles(localFile));

                files.AddRange(Directory.GetDirectories(localFile));
                
                // create the directory on the device
                _remoteStore.CreateDirectory(newRelativeDirectory);

                foreach (string file in files)
                {
                    Put(file, newRelativeDirectory, overwrite);
                }
            }
            else
            {
                string deviceFilename = Path.Combine(relativeDirectory, Path.GetFileName(localFile));

                if (overwrite || !_remoteStore.FileExists(deviceFilename))
                {
                    _remoteStore.SendFile(localFile, deviceFilename, createNew: true);
                }
            }
        }

        public void Delete()
        {
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

                _remoteStore.CreateDirectory(shared);
                _remoteStore.CreateDirectory(Path.Combine(shared, "Transfers"));
                _remoteStore.CreateDirectory(Path.Combine(shared, "ShellContent"));
            } 
            else if (RemoteFile.IsDirectory())
            {
                _remoteStore.DeleteDirectory(_path);
            }
            else
            {
                // really? no delete file??? RemoteIsolatedStorageFile is hidden (internal) within the wrapper
                // not sure if there is an easy way to get it
                var remoteFileObject = _remoteStore as RemoteIsolatedStorageFileObject;

                if (remoteFileObject != null)
                {
                    remoteFileObject.GetRemoteIsolatedStorageFile().DeleteFile(_path);
                }
            }
        }

        /// <summary>
        /// Call when you are ready to pull down the data associated with this object
        /// </summary>
        public void Update(bool force = false)
        {
            if (!force && (Opened || (RemoteFile != null && !RemoteFile.IsDirectory())))
                return;

            // if we are forcing an update then the assumption is that we want an update
            // of either the current item or its container. If this is not a directory and
            // we have been forced, force an update on the parent
            if (force && !IsApplication && (RemoteFile != null && !RemoteFile.IsDirectory()))
            {
                if (this.Parent != null)
                    this.Parent.Update(force: true);

                return;
            }

            Children.Clear();

            //Opened = true;
            Opened = true;

            IRemoteIsolatedStorageFile remoteIso = _remoteStore;

            if (remoteIso == null)
            {

                if (RemoteApp.InstanceID == Guid.Empty)
                {
                    // 8.1
                    UpdateModern(force);
                    return;
                }
                else
                {
                    remoteIso = RemoteApp.GetIsolatedStore("Local");
                }
            }
            
            List<IRemoteFileInfo> remoteFiles;

            try
            {
                if (remoteIso != null)
                {
                    remoteFiles = remoteIso.GetDirectoryListing(_path);

                    foreach (IRemoteFileInfo remoteFile in remoteFiles)
                    {
                        Children.Add(new RemoteAppIsoStoreItem(_appEx, remoteFile, remoteIso, this));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // no files, oh well :)
            }

        }

        /// <summary>
        /// Modern applications have multiple IsoStores so add these as top level folders which can then be handled in
        /// regular Update
        /// </summary>
        /// <param name="force"></param>
        private void UpdateModern(bool force)
        {
            var isoStores = new List<string> { "Local", "Roaming", "Temp" };

            foreach (var store in isoStores)
            {
                Children.Add(new RemoteAppIsoStoreItem(_appEx, store));
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
