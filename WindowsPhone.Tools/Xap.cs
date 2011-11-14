using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Xml.XPath;
using Microsoft.SmartDevice.Connectivity;

namespace WindowsPhone.Tools
{
    public class Xap
    {
        /// <summary>
        /// The file that this class is pointing to.
        /// Cannot be changed (set once via the constructor), create a new object
        /// if you want to point to a different file.
        /// </summary>
        public string FilePath { get; private set; }

        private Guid _guid = Guid.Empty;
        public Guid Guid
        {
            get
            {
                if (_guid == Guid.Empty)
                    InitFromManifest();

                return _guid;
            }
            private set { _guid = value; }
        }

        private string _name;
        public string Name
        {
            get {
                if (_name == null)
                    InitFromManifest();

                return _name; 
            }
            private set { _name = value; }
        }

        public string _icon;
        public string Icon
        {
            get
            {
                if (_icon == null)
                    InitFromManifest();

                return _icon;
            }
            private set { _icon = value; }
        }

        public Xap(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(file);

            FilePath = file;
        }

        private void InitFromManifest()
        {
            using (Stream manifestStream = GetFileStreamFromXap("WMAppManifest.xml"))
            {
                KnownApplication persistedAppData = new KnownApplication();

                XPathDocument document = new XPathDocument(manifestStream);
                XPathNavigator navigator = document.CreateNavigator().SelectSingleNode("//App");

                Guid = new Guid(navigator.GetAttribute("ProductID", string.Empty));
                Name = navigator.GetAttribute("Title", string.Empty);
                Icon = navigator.SelectSingleNode("//App//IconPath").ToString();

                if (!string.IsNullOrEmpty(Icon))
                {
                    using (Stream iconStream = GetFileStreamFromXap(Icon))
                    {
                        // some weird xaps don't have icons, so GetFileStreamFromXap returns a null
                        if (iconStream != null)
                        {
                            // save the image as the guid to maintain uniqueness
                            PersistedData.SaveBinaryStream(iconStream, Guid.ToString());

                            persistedAppData.Icon = Guid.ToString();
                        }
                    }
                }

                persistedAppData.Name = Name;

                // add this xap to persisted data
                PersistedData.Current.KnownApplication[Guid] = persistedAppData;
            }
        }

        private Stream GetFileStreamFromXap(string file)
        {
            MemoryStream rv = null;

            ZipStorer zip = ZipStorer.Open(FilePath, FileAccess.Read);

            List<ZipStorer.ZipFileEntry> entries = zip.ReadCentralDir();

            // Look for the desired file
            foreach (ZipStorer.ZipFileEntry entry in entries)
            {
                if (Path.GetFileName(entry.FilenameInZip) == file)
                {
                    rv = new MemoryStream(2048);

                    zip.ExtractFile(entry, rv);

                    rv.Seek(0, SeekOrigin.Begin);
                }
            }

            zip.Close();

            return rv;
        }

    }
}
