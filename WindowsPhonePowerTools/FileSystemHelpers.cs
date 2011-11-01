using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WindowsPhonePowerTools
{
    public static class FileSystemHelpers
    {
        /// <summary>
        /// Creates a temporary directory
        /// </summary>
        /// <returns>the path to the directory</returns>
        public static string CreateTemporaryDirectory()
        {
            string path;
            string temp = Path.GetTempPath();

            int maxTries = 10;

            do
            {
                path = Path.Combine(temp, Path.GetRandomFileName());

                // this is weird, because it throws before actually checking the current path. *shrug*
                if (maxTries-- < 0)
                    throw new DirectoryNotFoundException("Could not find a free temp directory. The current is: " + path);

            } while (Directory.Exists(path));

            Directory.CreateDirectory(path);

            return path;
        }
    }
}
