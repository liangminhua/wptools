using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;

namespace WindowsPhoneToolbox
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

        private const string PERSISTED_DATA_FILE = "persisted_data.xml";

        private PersistedData()
        {
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
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(PersistedData));

                        return xmlSerializer.Deserialize(stream) as PersistedData;
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
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PersistedData));

                xmlSerializer.Serialize(stream, _theOne);
            }
        }

        ~PersistedData() 
        {
            Save();
        }

    }
}
