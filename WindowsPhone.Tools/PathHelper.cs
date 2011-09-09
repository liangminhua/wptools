using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WindowsPhone.Tools
{
    internal static class PathHelper
    {
        private static string DirectorySeparatorString = Path.DirectorySeparatorChar + "";

        /// <summary>
        /// Returns the last directory from a path
        /// 
        /// c:\a\b\c\file.txt returns "c"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetLastDirectory(string path) {

            // often paths get passed in as c:\temp instead of c:\temp\, check for this
            // and add a separator. This is not performant, but functional, if you need
            // a performant copy, remove this line :)
            if (Directory.Exists(path) && !path.EndsWith(DirectorySeparatorString))
                path += Path.DirectorySeparatorChar;

            // get the directory from the path, remove the trailing \ and return the resulting filename
            return Path.GetFileName(Path.GetDirectoryName(path).TrimEnd(Path.DirectorySeparatorChar));

        }
    }
}
