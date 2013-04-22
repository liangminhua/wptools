using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WindowsPhone.Profiler
{
    public class EtwKernelFlag : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Keyword { get; set; }
        public string Description { get; set; }

        private bool _isEnabled;
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

        public EtwKernelFlag(string name, int keyword, string description)
        {
            this.Name = name;
            this.Keyword = keyword;
            this.Description = description;
        }

        /// <summary>
        /// Returns a shallow copy of this object
        /// </summary>
        /// <returns></returns>
        public EtwKernelFlag Copy()
        {
            return (EtwKernelFlag)this.MemberwiseClone();
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
