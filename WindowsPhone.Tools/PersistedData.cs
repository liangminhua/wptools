using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WindowsPhone.Tools
{
    [Serializable]
    public class PersistedData
    {

        #region Singleton

        private static PersistedData _theOne;

        public static PersistedData Current
        {
            get
            {
                if (_theOne == null)
                    _theOne = Load();

                return _theOne;
            }
        }

        #endregion

        private const string PERSISTED_DATA_FILE = "persisted_data";
        
        private Dictionary<Guid, string> _knownApplications;
        public Dictionary<Guid, string> KnownApplication
        {
            get { return _knownApplications; }
            private set { _knownApplications = value; }
        }

        private PersistedData()
        {
            KnownApplication = new Dictionary<Guid, string>();
        }

        private static PersistedData Load()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly();

            if (store.FileExists(PERSISTED_DATA_FILE))
            {
                try
                {
                    using (IsolatedStorageFileStream stream = store.OpenFile(PERSISTED_DATA_FILE, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();

                        return formatter.Deserialize(stream) as PersistedData;
                    }
                }
                catch { } // ignore the errors, anything falling through will get the default, empty, object
            }

            // default empty object
            return new PersistedData();
        }

        private static void Save()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly();

            using (IsolatedStorageFileStream stream = store.OpenFile(PERSISTED_DATA_FILE, FileMode.OpenOrCreate, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, _theOne);
            }
        }

        ~PersistedData() 
        {
            Save();
        }

    }
}
