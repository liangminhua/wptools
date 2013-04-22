using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WindowsPhone.Profiler
{
    public partial class EtwProviderType : INotifyPropertyChanged
    {
        private bool _isEnabled = false;

        [XmlIgnore]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;

                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Returns a shallow copy of this object
        /// </summary>
        /// <returns></returns>
        public EtwProviderType Copy()
        {
            return (EtwProviderType)this.MemberwiseClone();
        }


        /// <summary>
        /// Returns a shallow copy of this object with its properties modified
        /// based on the function's parameters
        /// </summary>
        /// <param name="level"></param>
        /// <param name="hexKeywords"></param>
        /// <param name="CLR"></param>
        /// <returns></returns>
        public EtwProviderType Copy(XmlProviderLevel level, string hexKeywords, bool CLR)
        {
            EtwProviderType rv = (EtwProviderType)this.MemberwiseClone();

            rv.Level = level;
            rv.HexKeywords = hexKeywords;
            rv.CLR = CLR;

            return rv;
        }

        # region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
