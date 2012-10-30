using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SmartDevice.Connectivity;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.SmartDevice.Connectivity.Interface;

namespace WindowsPhone.Tools
{
    public class RemoteApplicationEx
    {
        public IRemoteApplication RemoteApplication { get; private set; }

        bool inited = false;
        private KnownApplication _knownApplication;

        private bool Init(Guid guid)
        {
            if (inited && _knownApplication == null)
                return false;

            inited = true;

            return PersistedData.Current.KnownApplication.TryGetValue(guid, out _knownApplication);
        }

        private string _name;
        public string Name
        {
            set
            {
                // make sure that we persist any name changes
                if (_name != value)
                {
                    _name = value;

                    if (!Init(RemoteApplication.ProductID))
                    {
                        _knownApplication = new KnownApplication();

                        PersistedData.Current.KnownApplication[RemoteApplication.ProductID] = _knownApplication;
                    }

                    if (_knownApplication != null)
                        _knownApplication.Name = value;
                }
            }
            get
            {
                if (_name == null)
                {
                    Guid productId = RemoteApplication.ProductID;

                    if (Init(productId))
                    {
                        _name = _knownApplication.Name;
                    } 
                    else
                    {
                        _name = productId.ToString();
                    }
                }

                return _name;
            }
        }

        public Stream Icon
        {
            get
            {
                try
                {
                    if (Init(RemoteApplication.ProductID))
                    {
                        string file = _knownApplication.Icon;

                        if (file != null)
                        {
                            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly())
                            {
                                if (store.FileExists(file))
                                {
                                    using (IsolatedStorageFileStream sourceStream = store.OpenFile(file, FileMode.Open)) {
                                        // copy the stream from IsolatedStorage to a generic MemoryStream
                                        MemoryStream stream = new MemoryStream();

                                        sourceStream.CopyTo(stream);
                                        
                                        stream.Seek(0, SeekOrigin.Begin);

                                        return stream;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }

                return null;
            }
        }

        public RemoteApplicationEx(IRemoteApplication remoteApplication)
        {
            RemoteApplication = remoteApplication;
        }

    }
}
