using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SmartDevice.Connectivity;
using System.IO;

namespace WindowsPhone.Tools
{
    public static class CoreConExtensions
    {
        private const string RELATIVE_PATH_SEPARATOR = "IsolatedStore\\";
        private static int RELATIVE_PATH_SEPARATOR_LENGTH   = RELATIVE_PATH_SEPARATOR.Length;

        public static string GetExtension(this RemoteFileInfo fileInfo)
        {
            return Path.GetExtension(fileInfo.Name);
        }

        public static string GetRelativePath(this RemoteFileInfo fileInfo)
        {
            string name = fileInfo.Name;
            return name.Substring(name.IndexOf(RELATIVE_PATH_SEPARATOR) + RELATIVE_PATH_SEPARATOR_LENGTH);
        }

    }
}
