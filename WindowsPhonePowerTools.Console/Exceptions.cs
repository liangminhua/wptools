using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhonePowerTools.Console
{
    /// <summary>
    /// Something fatal has happened, let's exit with a nice message
    /// </summary>
    class ConsoleMessageException : Exception
    {
        public ConsoleMessageException(string msg) : base(msg) { }
    }
}
