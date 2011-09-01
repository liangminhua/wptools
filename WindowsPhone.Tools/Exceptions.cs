using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhone.Tools
{
    public class WindowsPhoneZuneNotRunningException : Exception 
    {
        public WindowsPhoneZuneNotRunningException() : base() {}    
    }
    
    public class WindowsPhoneConnectionException : Exception
    {
        public WindowsPhoneConnectionException(string message) : base(message) {}
        public WindowsPhoneConnectionException(string message, Exception innerException) : base(message, innerException) {}
    }
}
