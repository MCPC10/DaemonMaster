/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: EditAddWindow
//  
//  This file is part of DeamonMaster.
// 
//  DeamonMaster is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//   DeamonMaster is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
/////////////////////////////////////////////////////////////////////////////////////////


using DaemonMaster.Language;
using DaemonMasterCore;
using DaemonMasterCore.Exceptions;
using DaemonMasterCore.Win32.PInvoke;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Data;
using DaemonMasterCore.Config;
using Tulpep.ActiveDirectoryObjectPicker;

namespace DaemonMaster
{
    /// <summary>
    /// Interaktionslogik für EditAddWindow.xaml
    /// </summary>
    public partial class EditAddWindow : Window
    {
        private readonly ResourceManager _resManager = new ResourceManager(typeof(lang));
        private readonly Config _config;

        public DaemonItem DaemonItem { get; private set; }
        public DaemonItem OldDaemonItem { get; private set; }

        private ObservableCollection<ServiceInfo> _dependOnServiceObservableCollection;
        private ObservableCollection<ServiceInfo> _allServicesObservableCollection;
        private ObservableCollection<string> _dependOnGroupObservableCollection;
        private ObservableCollection<string> _allGroupsObservableCollection;

        private bool _onEditMode = false;
        private Daemon _daemon = null;

        private EditAddWindow()
        {
            //Load config settings from file
            _config = ConfigManagement.LoadConfig();

            InitializeComponent();

            #region Legacy functions

            if (!_config.ActivateLegacyFunctions)
                buttonLoadShortcut.IsEnabled = false;
            #endregion

            textBoxFilePath.IsReadOnly = true;
            _daemon = new Daemon();
        }

        public static EditAddWindow OpenEditAddWindowWithDefaultValues()
        {
            EditAddWindow editAddWindow = new EditAddWindow();
            editAddWindow.LoadDataIntoUI(editAddWindow._daemon);
            return editAddWindow;
        }

        public static EditAddWindow OpenEditAddWindowForEditing(DaemonItem daemonItem)
        {
            EditAddWindow editAddWindow = new EditAddWindow();
            editAddWindow.textBoxServiceName.IsReadOnly = true;
            editAddWindow.OldDaemonItem = daemonItem;

            if (ServiceManagement.StopService(daemonItem.ServiceName) < 0)
                throw new ServiceNotStoppedException();

            editAddWindow._daemon = RegistryManagement.LoadDaemonFromRegistry(daemonItem.ServiceName);
            editAddWindow.LoadDataIntoUI(editAddWindow._daemon);

            editAddWindow._onEditMode = true;
            return editAddWindow;
        }

        public static EditAddWindow OpenEditAddWindowForImporting(Daemon daemon)
        {
            EditAddWindow editAddWindow = new EditAddWindow();
            editAddWindow.LoadDataIntoUI(daemon);
            return editAddWindow;
        }


        private void LoadDataIntoUI(Daemon daemon)
        {
            //General Tab
            textBoxDisplayName.Text = daemon.DisplayName;
            if (!String.IsNullOrWhiteSpace(daemon.ServiceName))
                textBoxServiceName.Text = daemon.ServiceName.Substring(13);
            if (!String.IsNullOrWhiteSpace(daemon.FileName) && !String.IsNullOrWhiteSpace(daemon.FileDir))
                textBoxFilePath.Text = daemon.FullPath;
            textBoxParam.Text = daemon.Parameter;
            textBoxDescription.Text = daemon.Description;

            //Advanced Tab
            textBoxCounterResetTime.Text = daemon.CounterResetTime.ToString();
            textBoxMaxRestarts.Text = daemon.MaxRestarts.ToString();
            textBoxProcessKillTime.Text = daemon.ProcessKillTime.ToString();
            textBoxProcessRestartDelay.Text = daemon.ProcessRestartDelay.ToString();
            checkBoxIsConsoleApp.IsChecked = _daemon.ConsoleApplication;
            radioButtonUseCtrlC.IsChecked = _daemon.UseCtrlC;
            radioButtonUseCtrlBreak.IsChecked = !_daemon.UseCtrlC;


            if (String.IsNullOrWhiteSpace(daemon.Username) || daemon.UseLocalSystem || daemon.Password == null)
            {
                checkBoxUseLocalSystem.IsChecked = true;
                textBoxPassword.Password = String.Empty;
                textBoxUsername.Text = String.Empty;
            }
            else
            {
                textBoxPassword.Password = "***Super_sicheres_Passwort***";
                textBoxUsername.Text = daemon.Username;
            }


            switch (daemon.StartType)
            {
                case NativeMethods.SERVICE_START.SERVICE_AUTO_START:
                    comboBoxStartType.SelectedIndex = daemon.DelayedStart ? 1 : 0;
                    break;

                case NativeMethods.SERVICE_START.SERVICE_DEMAND_START:
                    comboBoxStartType.SelectedIndex = 2;
                    break;

                case NativeMethods.SERVICE_START.SERVICE_DISABLED:
                    comboBoxStartType.SelectedIndex = 3;
                    break;
            }

            #region Dependency Listboxes

            #region DependOnService
            //Load Data into _dependOnServiceObservableCollection
            _dependOnServiceObservableCollection = new ObservableCollection<ServiceInfo>();
            foreach (var dep in daemon.DependOnService)
            {
                ServiceInfo serviceInfo = new ServiceInfo()
                {
                    DisplayName = DaemonMasterUtils.GetDisplayName(dep),
                    ServiceName = dep
                };

                _dependOnServiceObservableCollection.Add(serviceInfo);
            }

            //Sort list alphabetical
            ICollectionView collectionView1 = CollectionViewSource.GetDefaultView(_dependOnServiceObservableCollection);
            collectionView1.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
            listBoxDependOnService.ItemsSource = collectionView1;
            #endregion

            #region AllServices

            //Load Data into _allServicesObservableCollection
            _allServicesObservableCollection = new ObservableCollection<ServiceInfo>();
            foreach (var service in ServiceController.GetServices())
            {
                ServiceInfo serviceInfo = new ServiceInfo()
                {
                    DisplayName = service.DisplayName,
                    ServiceName = service.ServiceName
                };

                if (_dependOnServiceObservableCollection.All(x => x.ServiceName != serviceInfo.ServiceName))
                    _allServicesObservableCollection.Add(serviceInfo);
            }
            //Sort list alphabetical
            ICollectionView collectionView2 = CollectionViewSource.GetDefaultView(_allServicesObservableCollection);
            collectionView2.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
            listBoxAllServices.ItemsSource = collectionView2;
            #endregion

            #region AllGroups

            //Load Data into _allGroupsObservableCollection
            _allGroupsObservableCollection = new ObservableCollection<string>(RegistryManagement.GetAllServiceGroups());
            //Sort list alphabetical
            ICollectionView collectionView3 = CollectionViewSource.GetDefaultView(_allGroupsObservableCollection);
            collectionView3.SortDescriptions.Add(new SortDescription());
            listBoxAllGroups.ItemsSource = collectionView3;
            #endregion

            #region DependOnGroup

            //Load Data into _dependOnGroupObservableCollection
            _dependOnGroupObservableCollection = new ObservableCollection<string>(daemon.DependOnGroup);
            //Sort list alphabetical
            ICollectionView collectionView4 = CollectionViewSource.GetDefaultView(_dependOnGroupObservableCollection);
            collectionView3.SortDescriptions.Add(new SortDescription());
            listBoxDependOnGroup.ItemsSource = collectionView4;
            #endregion

            #endregion
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                          GUI ELEMENTS                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region GUI_Elements

        private void buttonSave_OnClick(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void buttonSearchPath_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog =
                new OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
                    Filter = "Exe files (*.exe)|*.exe|" +
                             "All files (*.*)|*.*",
                    AddExtension = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    DereferenceLinks = false,
                    Multiselect = false
                };

            //Wenn eine Datei gewählt worden ist
            if (openFileDialog.ShowDialog() == true)
            {
                textBoxFilePath.Text = openFileDialog.FileName;

                //Wenn der Name noch leer oder der Standart Name geladen ist, soll er ihn mit dem Datei namen befüllen
                if (String.IsNullOrWhiteSpace(textBoxDisplayName.Text))
                {
                    textBoxDisplayName.Text = Path.GetFileNameWithoutExtension(openFileDialog.SafeFileName);
                }
            }
        }

        private void buttonOpenADOP_OnClick(object sender, RoutedEventArgs e)
        {
            DirectoryObjectPickerDialog pickerDialog = new DirectoryObjectPickerDialog()
            {

                AllowedObjectTypes = ObjectTypes.Users,
                DefaultObjectTypes = ObjectTypes.Users,
                AllowedLocations = Locations.LocalComputer,
                DefaultLocations = Locations.LocalComputer,
                MultiSelect = false,
                ShowAdvancedView = true
            };

            if (pickerDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxUsername.Text = pickerDialog.SelectedObject.Name;
            }
        }

        private void buttonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void buttonLoadShortcut_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog =
                new OpenFileDialog
                {
                    //Show the path of the shortcuts
                    DereferenceLinks = false,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    AddExtension = true,
                    Multiselect = false,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
                    Filter = "Shortcut (*.lnk)|*.lnk|" +
                             "All files (*.*)|*.*"
                };

            //Wenn eine Datei gewählt worden ist
            if (openFileDialog.ShowDialog() == true)
            {
                if (DaemonMasterUtils.IsShortcut(openFileDialog.FileName))
                {
                    MessageBoxResult result = MessageBox.Show(_resManager.GetString("data_will_be_overwritten", CultureInfo.CurrentUICulture), _resManager.GetString("warning", CultureInfo.CurrentUICulture), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        //TODO: Remove
                        //ShortcutInfo shortcutInfo = DaemonMasterUtils.GetShortcutInfos(openFileDialog.FileName);
                        //textBoxParam.Text = shortcutInfo.Arguments;
                        //textBoxFilePath.Text = shortcutInfo.FilePath;

                        //if (String.IsNullOrWhiteSpace(textBoxDescription.Text))
                        //{
                        //    textBoxDescription.Text = shortcutInfo.Description;
                        //}

                        //if (String.IsNullOrWhiteSpace(textBoxDisplayName.Text))
                        //{
                        //    textBoxDisplayName.Text = Path.GetFileNameWithoutExtension(openFileDialog.SafeFileName);
                        //}
                    }
                }
                else
                {
                    MessageBox.Show(_resManager.GetString("invalid_shortcut", CultureInfo.CurrentUICulture), _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void buttonRemoveDependentService_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDependOnService.SelectedItem == null)
                return;

            _allServicesObservableCollection.Add((ServiceInfo)listBoxDependOnService.SelectedItem);
            _dependOnServiceObservableCollection.Remove((ServiceInfo)listBoxDependOnService.SelectedItem);
        }

        private void buttonAddDependentService_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxAllServices.SelectedItem == null)
                return;

            _dependOnServiceObservableCollection.Add((ServiceInfo)listBoxAllServices.SelectedItem);
            _allServicesObservableCollection.Remove((ServiceInfo)listBoxAllServices.SelectedItem);
        }

        private void buttonAddDependentGroup_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxAllGroups.SelectedItem == null)
                return;

            _dependOnGroupObservableCollection.Add((string)listBoxAllGroups.SelectedItem);
            _allGroupsObservableCollection.Remove((string)listBoxAllGroups.SelectedItem);
        }

        private void buttonRemoveDependentGroup_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDependOnGroup.SelectedItem == null)
                return;

            _allGroupsObservableCollection.Add((string)listBoxDependOnGroup.SelectedItem);
            _dependOnGroupObservableCollection.Remove((string)listBoxDependOnGroup.SelectedItem);
        }

        #endregion


        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                             OTHER                                                    //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Other

        private void Save()
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(textBoxFilePath.Text)) || !File.Exists(textBoxFilePath.Text))
                {
                    MessageBox.Show(_resManager.GetString("invalid_path", CultureInfo.CurrentUICulture), _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                if (String.IsNullOrWhiteSpace(textBoxDisplayName.Text) ||
                    String.IsNullOrWhiteSpace(textBoxServiceName.Text) ||
                    !int.TryParse(textBoxMaxRestarts.Text, out var maxRestarts) ||
                    !int.TryParse(textBoxProcessKillTime.Text, out var processKillTime) ||
                    !int.TryParse(textBoxProcessRestartDelay.Text, out var processRestartDelay) ||
                    !int.TryParse(textBoxCounterResetTime.Text, out var counterResetTime) ||
                    ((checkBoxIsConsoleApp.IsChecked ?? false) && !(radioButtonUseCtrlBreak.IsChecked ?? true) && !(radioButtonUseCtrlC.IsChecked ?? true)))
                {
                    MessageBox.Show(_resManager.GetString("invalid_values", CultureInfo.CurrentUICulture), _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!checkBoxUseLocalSystem.IsChecked ?? true)
                {
                    if (String.IsNullOrWhiteSpace(textBoxUsername.Text) ||
                        String.IsNullOrWhiteSpace(textBoxPassword.Password))
                    {
                        MessageBox.Show(_resManager.GetString("invalid_values", CultureInfo.CurrentUICulture), _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (textBoxPassword.Password != "***Super_sicheres_Password***")
                    {
                        if (!SystemManagement.ValidateUserWin32(textBoxUsername.Text,
                            SecurityManagement.ConvertStringToSecureString(textBoxPassword.Password)))
                        {
                            MessageBox.Show(_resManager.GetString("invalid_user", CultureInfo.CurrentUICulture), _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        _daemon.Username = textBoxUsername.Text;
                        _daemon.Password = textBoxPassword.SecurePassword;
                    }
                    else
                    {
                        if (!SystemManagement.ValidateUserWin32(textBoxUsername.Text, _daemon.Password))
                        {
                            MessageBox.Show(_resManager.GetString("invalid_user", CultureInfo.CurrentUICulture), _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                else
                {
                    _daemon.Username = String.Empty;
                    _daemon.Password = null;
                }

                string fileDir = Path.GetDirectoryName(textBoxFilePath.Text);
                string fileName = Path.GetFileName(textBoxFilePath.Text);
                string fileExtension = Path.GetExtension(textBoxFilePath.Text);

                _daemon.UseLocalSystem = checkBoxUseLocalSystem.IsChecked ?? true;

                _daemon.DisplayName = textBoxDisplayName.Text;
                _daemon.ServiceName = "DaemonMaster_" + textBoxServiceName.Text;

                _daemon.FileDir = fileDir;
                _daemon.FileName = fileName;
                _daemon.FileExtension = fileExtension;

                _daemon.Parameter = textBoxParam.Text;
                _daemon.Description = textBoxDescription.Text;

                _daemon.MaxRestarts = maxRestarts;
                _daemon.ProcessKillTime = processKillTime;
                _daemon.ProcessRestartDelay = processRestartDelay;
                _daemon.CounterResetTime = counterResetTime;
                _daemon.DependOnService = _dependOnServiceObservableCollection.Select(x => x.ServiceName).ToArray();
                _daemon.DependOnGroup = _dependOnGroupObservableCollection.ToArray();
                _daemon.ConsoleApplication = checkBoxIsConsoleApp.IsChecked ?? false;
                _daemon.UseCtrlC = _daemon.ConsoleApplication && (radioButtonUseCtrlC.IsChecked ?? true) && !(radioButtonUseCtrlBreak.IsChecked ?? false);

                switch (comboBoxStartType.SelectedIndex)
                {

                    //Automatic
                    case 0:
                        _daemon.DelayedStart = false;
                        _daemon.StartType = NativeMethods.SERVICE_START.SERVICE_AUTO_START;
                        break;


                    //Automatic with delay
                    case 1:
                        _daemon.DelayedStart = true;
                        _daemon.StartType = NativeMethods.SERVICE_START.SERVICE_AUTO_START;
                        break;

                    //Manual
                    case 2:
                        _daemon.DelayedStart = false;
                        _daemon.StartType = NativeMethods.SERVICE_START.SERVICE_DEMAND_START;
                        break;

                    //Disabled
                    case 3:
                        _daemon.DelayedStart = false;
                        _daemon.StartType = NativeMethods.SERVICE_START.SERVICE_DISABLED;
                        break;
                }




                if (!_onEditMode)
                {
                    try
                    {
                        ServiceManagement.CreateInteractiveService(_daemon);
                        RegistryManagement.SaveInRegistry(_daemon);

                        DaemonItem = new DaemonItem
                        {
                            DisplayName = _daemon.DisplayName,
                            ServiceName = _daemon.ServiceName,
                            FullPath = _daemon.FullPath
                        };

                        MessageBox.Show(
                            _resManager.GetString("the_service_installation_was_successful",
                                CultureInfo.CurrentUICulture), _resManager.GetString("success"), MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        DialogResult = true;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            _resManager.GetString("the_service_installation_was_unsuccessful",
                                CultureInfo.CurrentUICulture) + "\n" + ex.Message, "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                else
                {
                    try
                    {
                        ServiceManagement.ChangeServiceConfig(_daemon);
                        RegistryManagement.SaveInRegistry(_daemon);

                        DaemonItem = new DaemonItem
                        {
                            DisplayName = _daemon.DisplayName,
                            ServiceName = _daemon.ServiceName,
                            FullPath = _daemon.FullPath
                        };

                        DialogResult = true;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            _resManager.GetString("data_cannot_be_saved", CultureInfo.CurrentUICulture) + ex.Message,
                            _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}