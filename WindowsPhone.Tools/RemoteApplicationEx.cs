using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SmartDevice.Connectivity;
using System.IO;
using System.IO.IsolatedStorage;

namespace WindowsPhone.Tools
{
    public class RemoteApplicationEx
    {

        public RemoteApplication RemoteApplication { get; private set; }

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

        public RemoteApplicationEx(RemoteApplication remoteApplication)
        {
            RemoteApplication = remoteApplication;
        }

    }
}
