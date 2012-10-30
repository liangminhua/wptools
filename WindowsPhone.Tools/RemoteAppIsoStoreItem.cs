extern alias SmartDeviceConnectivityWrapper10;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.SmartDevice.Connectivity;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.SmartDevice.Connectivity.Interface;
using Microsoft.SmartDevice.Connectivity.Wrapper;

namespace WindowsPhone.Tools
{
    public class RemoteAppIsoStoreItem : INotifyPropertyChanged
    {
        private IDevice _device;
        private RemoteApplicationEx _appEx;

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
        private RemoteAppIsoStoreItem(RemoteApplicationEx app, IRemoteFileInfo remoteFile, RemoteAppIsoStoreItem parent)
        {
            RemoteApp = app.RemoteApplication;

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
        public string Get(string localPath, bool overwrite)
        {
            var remoteIso = RemoteApp.GetIsolatedStore();

            string fullLocalPath = Path.Combine(localPath, Path.GetFileName(Name));

            if (IsApplication || RemoteFile.IsDirectory())
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
                    remoteIso.ReceiveFile(_path, fullLocalPath, true);
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
            var remoteIso = RemoteApp.GetIsolatedStore();

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
            var remoteIso = RemoteApp.GetIsolatedStore();

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
                // really? no delete file??? RemoteIsolatedStorageFile is hidden (internal) within the wrapper
                // not sure if there is an easy way to get it
                var remoteFileObject = remoteIso as RemoteIsolatedStorageFileObject;

                if (remoteFileObject != null)
                {
                    remoteFileObject.GetRemoteIsolatedStorageFile().DeleteFile(_path);
                }
                else
                {
                    // try the 10.0 version
                    var remoteFileObject10 = remoteIso as SmartDeviceConnectivityWrapper10::Microsoft.SmartDevice.Connectivity.Wrapper.RemoteIsolatedStorageFileObject;

                    if (remoteFileObject10 != null) 
                    {
                        WindowsPhone.Tools.Legacy10.Utils.DeleteFile(remoteFileObject10, _path);
                    }
                }
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
            if (!force && (Opened || (RemoteFile != null && !RemoteFile.IsDirectory())))
                return;

            // if we are forcing an update then the assumption is that we want an update
            // of either the current item or its container. If this is not a directory and
            // we have been forced, force an update on the parent
            if (force && !IsApplication && !RemoteFile.IsDirectory())
            {
                if (this.Parent != null)
                    this.Parent.Update(force: true);

                return;
            }

            Children.Clear();

            //Opened = true;
            Opened = true;

            var remoteIso = RemoteApp.GetIsolatedStore();

            List<IRemoteFileInfo> remoteFiles;

            try
            {
                remoteFiles = remoteIso.GetDirectoryListing(_path);

                foreach (IRemoteFileInfo remoteFile in remoteFiles)
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
