using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhone.Tools
{
    /// <summary>
    /// This is mainly for persisting data about known applications and you shouldn't
    /// generally need to interact with it directly
    /// </summary>
    [Serializable]
    internal class KnownApplication
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
