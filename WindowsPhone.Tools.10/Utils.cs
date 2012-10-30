using Microsoft.SmartDevice.Connectivity;
using Microsoft.SmartDevice.Connectivity.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPhone.Tools.Legacy10
{
    /// <summary>
    /// Container class for everything that must use the "10" version of Microsoft.SmartDevice.Connectivity
    /// 
    /// Kept static as it should not store state - pass in the generic wrapper objects and let it do its magic
    /// </summary>
    public static class Utils
    {
        public static void DeleteFile(RemoteIsolatedStorageFileObject remoteIsoStoreFileWrapperObj, string path)
        {
            var remoteIsoStoreFile = GetRemoteIsolatedStorageFileFromWrapper(remoteIsoStoreFileWrapperObj);

            if (remoteIsoStoreFile != null)
            {
                remoteIsoStoreFile.DeleteFile(path);
            }
        }

        /// <summary>
        /// We need access to the actual RemoteIsolatedStorageFile object since the wrapper doesn't expost a DeleteFile method
        /// so get to it via reflection
        /// </summary>
        /// <param name="wrapperRemoteIsoFile"></param>
        /// <returns></returns>
        private static RemoteIsolatedStorageFile GetRemoteIsolatedStorageFileFromWrapper(RemoteIsolatedStorageFileObject wrapperRemoteIsoFile)
        {
            BindingFlags eFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var fieldInfo = (typeof(RemoteIsolatedStorageFileObject)).GetField("mRemoteIsolatedStorageFile", eFlags);

            if (fieldInfo != null)
                return fieldInfo.GetValue(wrapperRemoteIsoFile) as RemoteIsolatedStorageFile;

            return null;
        }
    }
}
