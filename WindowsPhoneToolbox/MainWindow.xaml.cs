using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.SmartDevice.Connectivity;
using System.ComponentModel;
using WindowsPhone.Tools;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;

namespace WindowsPhoneToolbox
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private WindowsPhoneDevice _device = new WindowsPhoneDevice();
        
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = _device;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            _device.Connect();
        }

        private void lstInstalledApps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoteApplicationEx app = lstInstalledApps.SelectedItem as RemoteApplicationEx;

            if (app != null)
            {
                // TODO: not sure why we're not doing this directly in binding
                txtAppGuid.Text = app.RemoteApplication.ProductID.ToString();
            }
        }

        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            RemoteApplicationEx app = lstInstalledApps.SelectedItem as RemoteApplicationEx;

            if (app != null)
            {
                app.RemoteApplication.Uninstall();

                txtAppGuid.Text = "";

                _device.RefreshInstalledApps();
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // RemoveEmptyEntries does not remove an entry that has a space, so don't waste time with it
            string[] files = txtXapFile.Text.Split(';');

            Xap xap;
            RemoteApplication existingInstall;

            foreach (string file in files)
            {
                if (string.IsNullOrWhiteSpace(file))
                    continue;

                // trim filename, otherwise the native side of UpdateApplication gets annoyed
                xap = new Xap(file.Trim());

                if (!_device.CurrentDevice.IsApplicationInstalled(xap.Guid))
                {
                    // TODO: notification to user
                    continue;
                }

                existingInstall = _device.CurrentDevice.GetApplication(xap.Guid);

                existingInstall.UpdateApplication("genre", "noicon", xap.FilePath);
            }
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            // RemoveEmptyEntries does not remove an entry that has a space, so don't waste time with it
            string[] files = txtXapFile.Text.Split(';');

            Xap xap;
            RemoteApplication existingInstall;

            foreach (string file in files)
            {
                if (string.IsNullOrWhiteSpace(file))
                    continue;

                // trim filename, otherwise the native side of UpdateApplication gets annoyed
                xap = new Xap(file.Trim());

                if (_device.CurrentDevice.IsApplicationInstalled(xap.Guid))
                {
                    existingInstall = _device.CurrentDevice.GetApplication(xap.Guid);

                    existingInstall.Uninstall();
                }

                _device.CurrentDevice.InstallApplication(xap.Guid, Guid.Empty, "genre", "noicon", xap.FilePath);
            }

            _device.RefreshInstalledApps();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = txtXapFile.Text;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;

            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    dialog.InitialDirectory = System.IO.Path.GetFullPath(path);
                }
                catch { }
            }

            dialog.Filter = "XAP Files (*.xap)|*.xap;";

            if (dialog.ShowDialog() == true)
            {
                txtXapFile.Text = "";
 
                foreach (string file in dialog.FileNames)
                {
                    txtXapFile.Text += file + "; ";
                }
            }
        }

        private void treeIsoStore_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RemoteAppIsoStoreItem item = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;

            if (item != null)
                item.Update();

            stackFileProperties.DataContext = item.RemoteFile;
        }

        private static int _doubleClickCount = 0;

        private void treeIsoStore_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (++_doubleClickCount % 2 == 0)
            {

                RemoteAppIsoStoreItem item = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;

                if (item != null && !item.IsApplication && !item.RemoteFile.IsDirectory())
                {
                    string path = System.IO.Path.GetTempPath();

                    string localFilePath = item.Get(path);

                    System.Diagnostics.Debug.WriteLine(path);

                    // double click should launch the file
                    ProcessStartInfo info = new ProcessStartInfo(localFilePath);
                    info.UseShellExecute = true;
                    info.Verb = "open";

                    try
                    {
                        Process preview = new Process();
                        preview.StartInfo = info;

                        preview.Exited += (exitSender, exitE) => { File.Delete(localFilePath); };

                        preview.Start();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }

                e.Handled = true;
            }
        }

        private void btnGet_Click(object sender, RoutedEventArgs e)
        {
            RemoteAppIsoStoreItem item = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;

            // not interesting 
            if (item == null)
                return;

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            dialog.Description = "Select the destination folder for the downloaded files:";

            dialog.ShowDialog();

            if (string.IsNullOrEmpty(dialog.SelectedPath))
                return;

            item.Get(dialog.SelectedPath);
        }

        private void btnPutDirectory_Click(object sender, RoutedEventArgs e)
        {
            RemoteAppIsoStoreItem item = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;

            // nowhere to put this
            if (item == null)
                return;

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            dialog.Description = "Select the folder for to send to the device:";

            dialog.ShowDialog();

            if (string.IsNullOrEmpty(dialog.SelectedPath))
                return;

            item.Put(dialog.SelectedPath);

        }
        
        private void btnPutFile_Click(object sender, RoutedEventArgs e)
        {
            RemoteAppIsoStoreItem item = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;

            // nowhere to put this
            if (item == null)
                return;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.ShowDialog();

            foreach (string filename in dialog.FileNames)
            {
                item.Put(filename);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RemoteAppIsoStoreItem item = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;

            if (item == null)
                return;

            if (item.IsApplication)
            {
                if (MessageBox.Show("Are you sure you want to delete the IsolatedStorage for this application?\nIt will be reverted to a default state",
                    "Delete Isolated Storage?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No
                 )
                    return;
            }

            item.Delete();

            if (item.IsApplication)
                item.Update(force: true);
            else
                item.Parent.Update(force: true);
        }


        /// <summary>
        /// There has to be a better way to do this.
        /// 
        /// We use FakeRemoteAppIsoStoreItems to make sure that the little expander arrows
        /// appear near next to expandables (apps & folders) in the file browser, and we 
        /// remove them when Update is called. Expanded for some reason seems to always pass in
        /// the actual items and not the item that we're trying to expand, so we walk upwards to
        /// the parent and update that.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeIsoStore_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;

            if (item.Items.Count == 1)
            {
                FakeRemoteAppIsoStoreItem fake = item.Items[0] as FakeRemoteAppIsoStoreItem;

                if (fake != null)
                {
                    fake.Parent.Update();
                }

            }
            
        }

        # region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}
