/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: MAIN GUI 
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


using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DaemonMasterCore;
using AutoUpdaterDotNET;
using DaemonMasterCore.Win32;


namespace DaemonMaster
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<DaemonInfo> _processCollection = null;
        private readonly ResourceManager _resManager = new ResourceManager("DaemonMaster.Language.lang", typeof(MainWindow).Assembly);

        public MainWindow()
        {
            //Set the language of the threads
            //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
            //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
            //CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            //CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture;

            //Initialize GUI
            InitializeComponent();

            //Erstellt die Liste (leere oder mit gespeicherten Elementen)

            //Add events
            EditAddWindow.DaemonSavedEvent += EditAddWindow_DaemonSavedEvent;

            //Fragt, wenn der RegKey nicht gesetzt ist, ob dieser gesetzt werden soll
            if (!AskToEnableInteractiveServices())
                this.Close();

            _processCollection = RegistryManagement.LoadDaemonInfosFromRegistry();

            //Add Event
            _processCollection.CollectionChanged += ProcessList_CollectionChanged;

            //Aktualisiert die Liste zum start
            listBoxDaemons.ItemsSource = _processCollection;

            if (!ServiceManagement.CheckUI0DetectService())
            {
                MessageBox.Show(_resManager.GetString("error_ui0service", CultureInfo.CurrentUICulture), _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                          GUI ELEMENTS                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region GUI_Elements



        //Buttons

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddDaemon();
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            EditDaemon();
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDaemons.SelectedItem == null)
                return;

            RemoveDaemon((DaemonInfo)listBoxDaemons.SelectedItem);
        }

        private void buttonFilter_Click(object sender, RoutedEventArgs e)
        {

            foreach (DaemonInfo d in _processCollection)
            {
                if (d.DisplayName.Contains(textBoxFilter.Text))
                {
                    listBoxDaemons.SelectedItem = d;
                    break;
                }
            }
        }

        private void buttonSwitchToSession0_Click(object sender, RoutedEventArgs e)
        {
            SwitchToSession0();
        }

        //ListBox
        private void MenuItemStart_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDaemons.SelectedItem == null)
                return;

            StartService((DaemonInfo)listBoxDaemons.SelectedItem);
        }

        private void MenuItemStop_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDaemons.SelectedItem == null)
                return;

            StopService((DaemonInfo)listBoxDaemons.SelectedItem);
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDaemons.SelectedItem == null)
                return;

            RemoveDaemon((DaemonInfo)listBoxDaemons.SelectedItem);
        }

        private void listBoxDaemons_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditDaemon();
        }

        //MENU

        private void MenuItem_Click_AddDaemon(object sender, RoutedEventArgs e)
        {
            AddDaemon();
        }

        private void MenuItem_Click_RemoveDaemon(object sender, RoutedEventArgs e)
        {
            if (listBoxDaemons.SelectedItem == null)
                return;

            RemoveDaemon((DaemonInfo)listBoxDaemons.SelectedItem);
        }

        private void MenuItem_Click_EditDaemon(object sender, RoutedEventArgs e)
        {
            if (listBoxDaemons.SelectedItem == null)
                return;

            EditDaemon();
        }

        private void MenuItem_Click_CheckForUpdates(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();
        }

        private void MenuItem_Click_Credits(object sender, RoutedEventArgs e)
        {
            CreditsWindow creditsWindow = new CreditsWindow();
            creditsWindow.ShowDialog();
        }

        private void MenuItem_Click_Export(object sender, RoutedEventArgs e)
        {
            //DaemonMasterCore.ExportList(_processCollection);
            MessageBox.Show(_resManager.GetString("currently_unavailable"), _resManager.GetString("information"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_Click_Import(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(_resManager.GetString("currently_unavailable"), _resManager.GetString("information"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItemStartWS_OnClick(object sender, RoutedEventArgs e)
        {
            if (listBoxDaemons.SelectedItem == null)
                return;

            DaemonInfo daemonInfo = (DaemonInfo)listBoxDaemons.SelectedItem;
            ProcessManagement.CreateNewProcess(daemonInfo.ServiceName);
        }

        private void MenuItemStopWS_OnClick(object sender, RoutedEventArgs e)
        {
            if (listBoxDaemons.SelectedItem == null)
                return;

            DaemonInfo daemonInfo = (DaemonInfo)listBoxDaemons.SelectedItem;

            switch (ProcessManagement.DeleteProcess(daemonInfo.ServiceName))
            {
                case 0:
                    MessageBoxResult result = MessageBox.Show(_resManager.GetString("stop_was_unsuccessful"),
                        _resManager.GetString("error"), MessageBoxButton.YesNo, MessageBoxImage.Error);

                    if (result == MessageBoxResult.Yes)
                        ProcessManagement.KillAndDeleteProcess(daemonInfo.ServiceName);
                    break;
                case 1:
                    MessageBox.Show(_resManager.GetString("stop_was_successful"),
                        _resManager.GetString("information"), MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case -1:
                    MessageBox.Show(_resManager.GetString("the_selected_process_does_not_exist"),
                        _resManager.GetString("information"), MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                             OTHER                                                    //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Other

        private void ProcessList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) // Wenn sich was ändert kommt es in die Liste
        {
            listBoxDaemons.ItemsSource = _processCollection;
        }


        private void OpenAddDaemonWindow()
        {
            EditAddWindow addProcessWindow = new EditAddWindow(); // Neues Event Im EditAddWindow Fenster
            addProcessWindow.ShowDialog(); // Fenster geht auf, Code geht erst weiter wenn Fesnter geschlossen ist
        }

        private void OpenEditDaemonWindow(DaemonInfo daemonInfo)
        {
            EditAddWindow addProcessWindow = new EditAddWindow(daemonInfo);
            addProcessWindow.ShowDialog();
        }


        private bool AskToEnableInteractiveServices()
        {
            //Wenn der RegKey nicht gestetzt ist, soll der Nutzer gefragt werden
            if (!RegistryManagement.CheckNoInteractiveServicesRegKey())
            {
                MessageBoxResult result = MessageBox.Show(_resManager.GetString("interactive_service_regkey_not_set"), _resManager.GetString("question"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (RegistryManagement.ActivateInteractiveServices())
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(_resManager.GetString("problem_occurred"), _resManager.GetString("error"), MessageBoxButton.OK);
                    }
                }

                return false;
            }

            return true;
        }

        private void AddDaemon()
        {
            if (listBoxDaemons.Items.Count <= 256)
            {
                OpenAddDaemonWindow();
            }
            else
            {
                MessageBox.Show(_resManager.GetString("max_limit_reached"), _resManager.GetString("warning"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveDaemon(DaemonInfo daemonInfo)
        {
            try
            {
                switch (ServiceManagement.DeleteService(daemonInfo.ServiceName))
                {
                    case ServiceManagement.State.NotStopped:

                        MessageBoxResult result = MessageBox.Show(_resManager.GetString("you_must_stop_the_service_first"), _resManager.GetString("information"), MessageBoxButton.YesNo, MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            StopService(daemonInfo);
                        }
                        break;

                    case ServiceManagement.State.Successful:

                        _processCollection.RemoveAt(listBoxDaemons.SelectedIndex);

                        MessageBox.Show(_resManager.GetString("the_service_deletion_was_successful"),
                            _resManager.GetString("success"), MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resManager.GetString("the_service_deletion_was_unsuccessful") + "\n" + ex.Message, _resManager.GetString("error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditDaemon()
        {
            if (listBoxDaemons.SelectedItem == null)
                return;

            OpenEditDaemonWindow((DaemonInfo)listBoxDaemons.SelectedItem);
        }

        private void StartService(DaemonInfo daemonInfo)
        {
            switch (ServiceManagement.StartService(daemonInfo.ServiceName))
            {
                case ServiceManagement.State.Error | ServiceManagement.State.Unsuccessful:
                    MessageBox.Show(_resManager.GetString("cannot_start_the_service"), _resManager.GetString("error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case ServiceManagement.State.AlreadyStarted:
                    MessageBox.Show(_resManager.GetString("cannot_start_the_service_already_running"), _resManager.GetString("information"), MessageBoxButton.OK, MessageBoxImage.Information);
                    break;

                case ServiceManagement.State.Successful:
                    MessageBox.Show(_resManager.GetString("service_start_was_successful"), _resManager.GetString("information"), MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        private void StopService(DaemonInfo daemonInfo)
        {
            switch (ServiceManagement.StopService(daemonInfo.ServiceName))
            {
                case ServiceManagement.State.Error | ServiceManagement.State.Unsuccessful:
                    MessageBox.Show(_resManager.GetString("cannot_stop_the_service"), _resManager.GetString("error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case ServiceManagement.State.AlreadyStopped:
                    MessageBox.Show(_resManager.GetString("cannot_stop_the_service_already_stopped"), _resManager.GetString("information"), MessageBoxButton.OK, MessageBoxImage.Information);
                    break;

                case ServiceManagement.State.Successful:
                    MessageBox.Show(_resManager.GetString("service_stop_was_successful"), _resManager.GetString("information"), MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        private void StartProcess(DaemonInfo daemonInfo)
        {

        }

        private void StopProcess(DaemonInfo daemonInfo)
        {

        }

        private void SwitchToSession0()
        {
            if (ServiceManagement.CheckUI0DetectService())
            {
                MessageBoxResult result = MessageBox.Show(_resManager.GetString("windows10_mouse_keyboard", CultureInfo.CurrentUICulture), _resManager.GetString("warning", CultureInfo.CurrentUICulture), MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    NativeMethods.WinStationSwitchToServicesSession();
                }
            }
            else
            {
                MessageBox.Show(_resManager.GetString("failed_start_UI0detect_service", CultureInfo.CurrentUICulture), _resManager.GetString("error", CultureInfo.CurrentUICulture), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CheckForUpdates()
        {
            AutoUpdater.CurrentCulture = CultureInfo.CurrentCulture;
            AutoUpdater.ShowSkipButton = true;
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start("https://raw.githubusercontent.com/TWC-Software/DaemonMaster/master/AutoUpdater.xml", typeof(MainWindow).Assembly);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                          EVENT HANDLER                                               //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region EventHandler

        private void EditAddWindow_DaemonSavedEvent(DaemonInfo daemonInfo) // Fügt Deamon Objekt der Liste hinzu
        {
            _processCollection.Add(daemonInfo);
        }

        #endregion


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();
        }
    }
}