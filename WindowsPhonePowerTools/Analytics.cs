using GoogleAnalyticsTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPhonePowerTools
{
    public class Analytics
    {

        #region Singleton

        private static Analytics _theOne = new Analytics();
        
        public static Analytics Instance { get { return _theOne; } }

        #endregion

        public enum Categories { PowerTools, Device, App, IsoStore };

        private Tracker _tracker;

        private bool _disabled = false;

        /// <summary>
        /// A unique ID - while this cannot be used to remotely identify a user it is used to track
        /// the number of unique installations of the phones tools
        /// </summary>
        public string UniqueId { get; private set; }
        
        private Analytics()
        {
            // disable tracking when running a debug build or under debugger
            if (System.AppDomain.CurrentDomain.FriendlyName.EndsWith("vshost.exe") || System.Diagnostics.Debugger.IsAttached)
            {
                _disabled = true;
            }

            _tracker = new Tracker("UA-11132531-2", "wptools.nachmore.com");

            if (string.IsNullOrEmpty(Properties.Settings.Default.UniqueId))
            {
                // assume that this is a new installation
                Track(Categories.PowerTools, "New Installation");

                Properties.Settings.Default.UniqueId = Guid.NewGuid().ToString();
            }

            UniqueId = Properties.Settings.Default.UniqueId;
        }
        
        public void Track(Categories category, string action, string label = null, int value = 0)
        {
            if (!_disabled)
                _tracker.TrackEventAsync(category.ToString(), action, label, value);
        }
    }
}
