/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: EDIT/ADD GUI 
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


using DaemonMasterCore;
using DaemonMasterCore.Win32.PInvoke;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows;
using Tulpep.ActiveDirectoryObjectPicker;

namespace DaemonMaster
{
    /// <summary>
    /// Interaktionslogik für EditAddWindow.xaml
    /// </summary>
    public partial class EditAddWindow : Window
    {
        private readonly ResourceManager resManager = new ResourceManager("DaemonMaster.Language.lang", typeof(EditAddWindow).Assembly);

        //Erstellt ein Event 
        internal delegate void DaemonSavedDelegate(DaemonInfo daemon);
        internal static event DaemonSavedDelegate DaemonSavedEvent;
        internal delegate void DaemonEditDelegate(DaemonInfo oldDaemonInfo, DaemonInfo newDaemonInfo);
        internal static event DaemonEditDelegate DaemonEditEvent;

        private Daemon daemon = null;
        private DaemonInfo _oldDaemonInfo = null;

        private readonly bool onEdit = false;

        public EditAddWindow()
        {
            InitializeComponent();

            textBoxFilePath.IsReadOnly = true;

            buttonOpenADOP.IsEnabled = false;
            textBoxPassword.IsEnabled = false;
            textBoxUsername.IsEnabled = false;

            daemon = new Daemon();
        }

        public EditAddWindow(DaemonInfo daemonInfo) : this() // This = Konstruktor davor wird auch ausgeführt (=> Ableitung vom Oberen)
        {
            textBoxServiceName.IsReadOnly = true;
            _oldDaemonInfo = daemonInfo;

            try
            {
                if (ServiceManagement.StopService(daemonInfo.ServiceName) < 0)
                    throw new Exception("Service must be stopped");

                daemon = RegistryManagement.LoadDaemonFromRegistry(daemonInfo.ServiceName);
                LoadDataIntoUI(daemon);

                onEdit = true;
            }
            catch (Exception)
            {
                MessageBox.Show(resManager.GetString("cannot_load_data_from_registry"), resManager.GetString("error"), MessageBoxButton.OK, MessageBoxImage.Error);

                this.Close();
            }
        }

        private void LoadDataIntoUI(Daemon daemon)
        {
            textBoxDisplayName.Text = daemon.DisplayName;
            textBoxServiceName.Text = daemon.ServiceName.Substring(13);
            textBoxFilePath.Text = daemon.FullPath;
            textBoxParam.Text = daemon.Parameter;
            textBoxDescription.Text = daemon.Description;


            if (String.IsNullOrWhiteSpace(daemon.UserName))
            {
                checkBoxUseLocalSystem.IsChecked = true;
                textBoxPassword.Password = String.Empty;
                textBoxUsername.Text = String.Empty;
            }
            else
            {
                textBoxPassword.Password = "***";
                textBoxUsername.Text = daemon.UserName;
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
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                          GUI ELEMENTS                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region GUI_Elements

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void buttonSearchPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog =
                new OpenFileDialog
                {
                    //Show the path of the shortcuts
                    DereferenceLinks = false,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    AddExtension = true,
                    InitialDirectory = Environment.GetFolderPath(System.Environment.SpecialFolder.MyComputer),
                    Filter = "Exe files (*.exe)|*.exe|" + "Shortcut (*.lnk)|*.lnk|" +
                             "All files (*.*)|*.*"
                };

            //Wenn eine Datei gewählt worden ist
            if (openFileDialog.ShowDialog() == true)
            {
                textBoxFilePath.Text = openFileDialog.FileName;

                //Wenn der Name noch leer oder der Standart Name geladen ist, soll er ihn mit dem Datei namen befüllen
                if (String.IsNullOrWhiteSpace(textBoxDisplayName.Text))
                {
                    textBoxDisplayName.Text = openFileDialog.SafeFileName;
                }
            }
        }

        private void buttonOpenADOP_Click(object sender, RoutedEventArgs e)
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

        #endregion


        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                             OTHER                                                    //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Other

        private void Save()
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(textBoxFilePath.Text)) || !System.IO.File.Exists(textBoxFilePath.Text))
                {
                    MessageBox.Show(resManager.GetString("invalid_path", CultureInfo.CurrentUICulture), resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (String.IsNullOrWhiteSpace(textBoxDisplayName.Text) ||
                    String.IsNullOrWhiteSpace(textBoxServiceName.Text))
                {
                    MessageBox.Show(resManager.GetString("invalid_values", CultureInfo.CurrentUICulture), resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!(bool)checkBoxUseLocalSystem.IsChecked)
                {
                    if (String.IsNullOrWhiteSpace(textBoxUsername.Text) ||
                        String.IsNullOrWhiteSpace(textBoxPassword.Password))
                    {
                        MessageBox.Show(resManager.GetString("invalid_user", CultureInfo.CurrentUICulture), resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!SystemManagement.ValidateUser(textBoxUsername.Text,
                        SecurityManagement.ConvertStringToSecureString(textBoxPassword.Password)))
                    {
                        MessageBox.Show(resManager.GetString("invalid_user", CultureInfo.CurrentUICulture), resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    daemon.UserName = textBoxUsername.Text;
                    daemon.UserPassword = SecurityManagement.ConvertStringToSecureString(textBoxPassword.Password);
                }
                else
                {
                    daemon.UserName = String.Empty;
                    daemon.UserPassword = null;
                }

                string fileDir = Path.GetDirectoryName(textBoxFilePath.Text);
                string fileName = Path.GetFileName(textBoxFilePath.Text);
                string fileExtension = Path.GetExtension(textBoxFilePath.Text);


                daemon.DisplayName = textBoxDisplayName.Text;
                daemon.ServiceName = "DaemonMaster_" + textBoxServiceName.Text;

                daemon.FileDir = fileDir;
                daemon.FileName = fileName;
                daemon.FileExtension = fileExtension;

                daemon.Parameter = textBoxParam.Text;
                daemon.Description = textBoxDescription.Text;

                switch (comboBoxStartType.SelectedIndex)
                {

                    //Automatic
                    case 0:
                        daemon.DelayedStart = false;
                        daemon.StartType = NativeMethods.SERVICE_START.SERVICE_AUTO_START;
                        break;


                    //Automatic with delay
                    case 1:
                        daemon.DelayedStart = true;
                        daemon.StartType = NativeMethods.SERVICE_START.SERVICE_AUTO_START;
                        break;

                    //Manual
                    case 2:
                        daemon.DelayedStart = false;
                        daemon.StartType = NativeMethods.SERVICE_START.SERVICE_DEMAND_START;
                        break;

                    //Disabled
                    case 3:
                        daemon.DelayedStart = false;
                        daemon.StartType = NativeMethods.SERVICE_START.SERVICE_DISABLED;
                        break;
                }




                if (!onEdit)
                {
                    try
                    {
                        ServiceManagement.CreateInteractiveService(daemon);
                        RegistryManagement.SaveInRegistry(daemon);

                        DaemonInfo daemonInfo = new DaemonInfo
                        {
                            DisplayName = daemon.DisplayName,
                            ServiceName = daemon.ServiceName,
                            FullPath = daemon.FullPath
                        };

                        OnDaemonSavedEvent(daemonInfo);

                        MessageBox.Show(
                            resManager.GetString("the_service_installation_was_successful",
                                CultureInfo.CurrentUICulture), resManager.GetString("success"), MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            resManager.GetString("the_service_installation_was_unsuccessful",
                                CultureInfo.CurrentUICulture) + ex.Message, "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                else
                {
                    try
                    {
                        ServiceManagement.ChangeServiceConfig(daemon);
                        RegistryManagement.SaveInRegistry(daemon);

                        DaemonInfo newDaemonInfo = new DaemonInfo
                        {
                            ServiceName = daemon.ServiceName,
                            DisplayName = daemon.DisplayName,
                            FullPath = daemon.FullPath
                        };

                        //Replace the GUI Item with the new infos
                        OnDaemonEditEvent(_oldDaemonInfo, newDaemonInfo);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            resManager.GetString("data_cannot_be_saved", CultureInfo.CurrentUICulture) + ex.Message,
                            resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion










        private static void OnDaemonSavedEvent(DaemonInfo daemonInfo)
        {
            DaemonSavedEvent?.Invoke(daemonInfo);
        }

        private static void OnDaemonEditEvent(DaemonInfo oldDaemonInfo, DaemonInfo newDaemonInfo)
        {
            DaemonEditEvent?.Invoke(oldDaemonInfo, newDaemonInfo);
        }

        private void CheckBoxUseLocalSystem_OnClick(object sender, RoutedEventArgs e)
        {
            if (checkBoxUseLocalSystem.IsChecked == null)
                return;

            if ((bool)checkBoxUseLocalSystem.IsChecked)
            {
                buttonOpenADOP.IsEnabled = false;
                textBoxPassword.IsEnabled = false;
                textBoxUsername.IsEnabled = false;
            }
            else
            {
                buttonOpenADOP.IsEnabled = true;
                textBoxPassword.IsEnabled = true;
                textBoxUsername.IsEnabled = true;
            }
        }
    }
}
//[] 