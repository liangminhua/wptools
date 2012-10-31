using MahApps.Metro.Controls;
using MetroUI;
using Microsoft.SmartDevice.Connectivity;
using Microsoft.SmartDevice.Connectivity.Interface;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;
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
using WindowsPhone.Tools;

namespace WindowsPhonePowerTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            Current = this;

            Device = new WindowsPhoneDevice();

            this.DataContext = this;
        }

        private void NavigationButton_OnSelectionChanged(object sender, EventArgs e)
        {

            NavigationButton button = sender as NavigationButton;

            if (button != null)
                navigator.SelectionChanged(button);
        }

        #region Fake Singleton

        public static MainWindow Current { get; private set; }

        #endregion

        #region Properties

        private WindowsPhoneDevice _device;
        public WindowsPhoneDevice Device
        {
            get { return _device; }
            set
            {
                if (_device != value)
                {
                    _device = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private RemoteApplicationEx _curSelectedInstalledApp;
        public RemoteApplicationEx CurSelectedInstalledApp
        {
            get { return _curSelectedInstalledApp; }
            set
            {
                if (_curSelectedInstalledApp != value)
                {
                    _curSelectedInstalledApp = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private string _errorString;
        public string ErrorString
        {
            get { return _errorString; }
            set
            {
                if (_errorString != value)
                {
                    _errorString = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private bool? _isElevated = null;
        public bool IsElevated
        {
            get
            {
                if (_isElevated == null)
                {
                    _isElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
                }

                return (_isElevated == true);
            }
        }

        #endregion

        #region General Goo
        #endregion

        #region Error Dialog

        public void ShowError(string error)
        {
            ErrorString = error;
            dialogError.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnDismissErrorDialog_Click(object sender, RoutedEventArgs e)
        {
            dialogError.Visibility = System.Windows.Visibility.Collapsed;
            ErrorString = "";
        }

        #endregion

        #region Connection Dialog

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            _device.Connect();

            if (_device.Connected)
                dialogConnect.Close();
        }

        private void btnLaunchElevated_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

            // relaunch with runas to get elevated
            processInfo.UseShellExecute = true;
            processInfo.Verb = "runas";

            try
            {
                Process.Start(processInfo);

                // only shutdown if we launch successfully
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Install | Update

        // OREN Remove!
        /*
        TextBox txtAppGuid = new TextBox();
        TextBox txtXapFile = new TextBox();
        TreeView treeIsoStore = new TreeView();
        StackPanel stackFileProperties = new StackPanel();
        CheckBox chkOverwrite = new CheckBox();
        */
        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            if (CurSelectedInstalledApp != null)
            {
                CurSelectedInstalledApp.RemoteApplication.Uninstall();

                txtAppGuid.Text = "";

                _device.RefreshInstalledApps();
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            // RemoveEmptyEntries does not remove an entry that has a space, so don't waste time with it
            string[] files = txtXapFile.Text.Split(';');

            Xap xap;
            IRemoteApplication existingInstall;

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
            IRemoteApplication existingInstall;

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

                try
                {
                    _device.CurrentDevice.InstallApplication(xap.Guid, Guid.Empty, "genre", "noicon", xap.FilePath);
                }
                catch (SmartDeviceException ex)
                {
                    if (ex.Message == "0x8103010B")
                    {
                        ShowError("Your xap (" + xap.Name + ") appears to target a newer OS than the one you are deploying to. Are you trying to deploy an 8.0 app to the 7.5 emulator?");
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    ShowError("You've run out of space on your device! Please try again (this can sometimes be a device API issue) and if the error persists remove some apps and then try again.\n\nRaw Error: " + ex.Message);
                    break;
                }
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

        #endregion

        #region Apps

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (_device != null)
                _device.RefreshInstalledApps();
        }

        private void btnLaunchApp_Click(object sender, RoutedEventArgs e)
        {
            if (CurSelectedInstalledApp != null)
                CurSelectedInstalledApp.RemoteApplication.Launch();
        }

        private void btnKillApp_Click(object sender, RoutedEventArgs e)
        {
            if (CurSelectedInstalledApp != null)
                CurSelectedInstalledApp.RemoteApplication.TerminateRunningInstances();
        }

        #endregion

        #region Isolated Storage Browser

        private void btnLaunchMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem contextMenuItem = (MenuItem)sender;
            ContextMenu contextMenu = (ContextMenu)contextMenuItem.Parent;

            FrameworkElement element = contextMenu.PlacementTarget as FrameworkElement;

            if (element != null)
            {
                FrameworkElement elemParent = element.TemplatedParent as FrameworkElement;

                if (elemParent != null)
                {
                    TreeViewItem item = elemParent.TemplatedParent as TreeViewItem;

                    if (item != null)
                    {
                        RemoteAppIsoStoreItem isoStoreItem = item.DataContext as RemoteAppIsoStoreItem;

                        while (isoStoreItem != null && isoStoreItem.IsApplication == false)
                        {
                            isoStoreItem = isoStoreItem.Parent;
                        }

                        if (isoStoreItem != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Launching: " + isoStoreItem.RemoteApp.ProductID);
                            isoStoreItem.RemoteApp.Launch();
                        }
                    }
                }
            }

            if (contextMenu.PlacementTarget.GetType() == typeof(TreeViewItem))
            {
                TreeViewItem originatingTreeViewItem = (TreeViewItem)contextMenu.PlacementTarget;
            }
        }

        private void treeIsoStore_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RemoteAppIsoStoreItem item = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;

            if (item == null)
                return;

            item.Update();

            stackFileProperties.DataContext = item.RemoteFile;
        }

        private void treeIsoStoreItem_OnDrop(object sender, DragEventArgs e)
        {
            RemoteAppIsoStoreItem item = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;
            string[] files = e.Data.GetData("FileDrop") as string[];

            // only allow dropping on Application root level items and directories
            if (files == null || item == null || (!item.IsApplication && !item.RemoteFile.IsDirectory()))
                return;

            foreach (string file in files)
            {
                item.Put(file, (bool)chkOverwrite.IsChecked);
            }

            item.Update(force: true);
        }

        private void treeIsoStoreItem_OnMouseMove(object sender, MouseEventArgs e)
        {
            TreeViewItem treeViewItem = sender as TreeViewItem;
            RemoteAppIsoStoreItem isoStoreItem = treeViewItem.DataContext as RemoteAppIsoStoreItem;

            if (treeViewItem != null && isoStoreItem != null && e.LeftButton == MouseButtonState.Pressed)
            {
                // We're about to enter a drag operation (which is a blocking operation)
                // but we need to have the files on the local system first.
                // Unfortunately DoDragDrop assumes that the source data (in our case IsoStore files)
                // are ready to be copied, which means we need to copy them to the host system before
                // calling DoDragDrop. Since this can take a while the user can get stuck with the program
                // seemingly hung while dragging, until the files have copied. If the user doesn't wait the
                // full amount of time the drag will fail.
                // An alternative is to download on a background thread, but then if the drop happens before
                // the files are down, we're stuck.
                // By the way, neither of the extra events (QueryContinueDrag & GiveFeedback) provide indication
                // that the drop has occurred.

                string tempDir = FileSystemHelpers.CreateTemporaryDirectory();

                isoStoreItem.Get(tempDir, overwrite: true);

                DataObject dataObject = new DataObject("FileDrop", new string[] { System.IO.Path.Combine(tempDir, isoStoreItem.Name) });

                DragDrop.DoDragDrop(treeViewItem, dataObject, DragDropEffects.Copy);

                Directory.Delete(tempDir, recursive: true);
            }
        }

        private void treeIsoStoreItem_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OpenFileFromIsoStore();
                e.Handled = true;
            }
        }

        private static DateTime _lastDoubleClick = DateTime.Now;

        private void treeIsoStoreItem_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // for some unknown reason OnDoubleClick gets called twice (and we don't want to
            // set handled since we want it to bubble).
            // Unfortunately some systems call this 4 times, to work around this we take a
            // timestamp for filtering
            if (DateTime.Now.Subtract(_lastDoubleClick).TotalMilliseconds > 500)
            {
                OpenFileFromIsoStore();

                _lastDoubleClick = DateTime.Now;
            }
        }

        private void OpenFileFromIsoStore()
        {
            RemoteAppIsoStoreItem item = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;

            if (item != null && !item.IsApplication && !item.RemoteFile.IsDirectory())
            {
                string path = System.IO.Path.GetTempPath();

                // when double clicking we should always overwrite to make sure we don't get
                // stale files
                string localFilePath = item.Get(path, true);

                System.Diagnostics.Debug.WriteLine(path);

                // double click should launch the file
                ProcessStartInfo info = new ProcessStartInfo(localFilePath);
                info.UseShellExecute = true;

                // decide which verb to use ("open for recognised files, otherwise "openas")
                // Note: info.Verbs is determined according to localFilePath
                info.Verb = (info.Verbs.Contains("open") ? "open" : "openas");

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

            item.Get(dialog.SelectedPath, (chkOverwrite.IsChecked == true ? true : false));
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

            item.Put(dialog.SelectedPath, chkOverwrite.IsChecked == true);

            item.Update(force: true);

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
                item.Put(filename, chkOverwrite.IsChecked == true);
            }

            item.Update(force: true);
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

            RemoteAppIsoStoreItem isoStoreItem = treeIsoStore.SelectedItem as RemoteAppIsoStoreItem;

            // refresh the icon on item expansion
            if (isoStoreItem != null && isoStoreItem.Opened)
                isoStoreItem.Icon = null;
        }

        #endregion

        #region General UI Features

        private void btnLaunchConnect_Click(object sender, RoutedEventArgs e)
        {
            dialogConnect.Show();
        }

        private void btnCloseConnectDialog_Click(object sender, RoutedEventArgs e)
        {
            dialogConnect.Close();
        }

        #endregion

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
