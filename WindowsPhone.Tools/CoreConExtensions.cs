using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SmartDevice.Connectivity;
using System.IO;
using Microsoft.SmartDevice.Connectivity.Interface;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.SmartDevice.Connectivity.Wrapper;

namespace WindowsPhone.Tools
{
    public static class CoreConExtensions
    {
        private const string RELATIVE_PATH_SEPARATOR = "IsolatedStore\\";
        private static int RELATIVE_PATH_SEPARATOR_LENGTH   = RELATIVE_PATH_SEPARATOR.Length;

        // seperator used for everything post Mango
        // WP8 == WP8+
        private const string WP8_SEPERATOR = "%FOLDERID_APPID_ISOROOT%\\";
        
        // random guid to generate the correct length to cull from the path
        private static int WP8_PATH_SEPERATOR_LENGTH = "%FOLDERID_APPID_ISOROOT%\\{de8a200e-c004-471b-9566-8af08b8458ee}".Length;

        public static string GetExtension(this IRemoteFileInfo fileInfo)
        {
            return Path.GetExtension(fileInfo.Name);
        }

        public static string GetRelativePath(this IRemoteFileInfo fileInfo)
        {
            string name = fileInfo.Name;

            if (name.Contains(WP8_SEPERATOR))
            {
                name = name.Substring(WP8_PATH_SEPERATOR_LENGTH);

                // modern applications will have an extra field which needs to be removed
                if (name.Contains("%"))
                {
                    Regex re = new Regex("\\%.*?%");
                    name = re.Replace(name, "");
                }

                return name;
            }
            else
            {
                return name.Substring(name.IndexOf(RELATIVE_PATH_SEPARATOR) + RELATIVE_PATH_SEPARATOR_LENGTH);
            }
        }

        public static RemoteFileInfo GetInternalRemoteFileInfo(this IRemoteFileInfo wrapperRemoteFileInfo)
        {
            BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fieldInfo = (typeof(RemoteFileInfoObject)).GetField("mRemoteFileInfo", eFlags);

            if (fieldInfo != null)
            {
                // An exception will be thrown when referencing CoreCon 10 objects since it won't contain the field on this particular
                // wrapper object (so fall through and return null)
                try
                {
                    return fieldInfo.GetValue(wrapperRemoteFileInfo) as RemoteFileInfo;
                }
                catch { }
            }

            return null;
        }

        /// <summary>
        /// We need access to the actual RemoteIsolatedStorageFile object since the wrapper doesn't expost a DeleteFile method
        /// so get to it via reflection
        /// </summary>
        /// <param name="wrapperRemoteIsoFile"></param>
        /// <returns></returns>
        public static RemoteIsolatedStorageFile GetRemoteIsolatedStorageFile(this RemoteIsolatedStorageFileObject wrapperRemoteIsoFile) 
        {
            BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fieldInfo = (typeof(RemoteIsolatedStorageFileObject)).GetField("mRemoteIsolatedStorageFile", eFlags);

            if (fieldInfo != null)
                return fieldInfo.GetValue(wrapperRemoteIsoFile) as RemoteIsolatedStorageFile;

            return null;
        }

    }
}
