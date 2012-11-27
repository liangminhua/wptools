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

        private Analytics()
        {
            _tracker = new Tracker("UA-11132531-2", "wptools.nachmore.com");

        }

        public void Track(Categories category, string action, string label = null, int value = 0)
        {
            _tracker.TrackEventAsync(category.ToString(), action, label, value);
        }
    }
}
