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
                {
                    _guid = GetGuid();
                }

                return _guid;
            }
        }

        public Xap(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(file);

            FilePath = file;
        }

        private Guid GetGuid()
        {
            Stream manifestStream = GetFileStreamFromXap("WMAppManifest.xml");

            XPathDocument document = new XPathDocument(manifestStream);
            return new Guid(document.CreateNavigator().SelectSingleNode("//App").GetAttribute("ProductID", string.Empty));
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
