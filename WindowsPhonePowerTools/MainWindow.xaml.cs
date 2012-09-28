using MahApps.Metro.Controls;
using MetroUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WindowsPhonePowerTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            //Resources["AccentColor"] = ColorConverter.ConvertFromString(SystemColors.HighlightColor.ToString());

            
        }

        private void NavigationButton_OnSelectionChanged(object sender, EventArgs e)
        {

            NavigationButton button = sender as NavigationButton;

            if (button != null)
                navigator.SelectionChanged(button);

            /*
            if (button != null)
            {
                if (button.IsSelected)
                {
                    pullTest.Content = button.Content;
                }
                else
                {
                    pullTest.Content = null;
                }
            }
             */
        }
    }
}
