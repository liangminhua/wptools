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

        public static string GetExtension(this RemoteFileInfo fileInfo)
        {
            return Path.GetExtension(fileInfo.Name);
        }

    }
}
