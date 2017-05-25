/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: REGISTRY MANAGEMENT FILE
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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.AccessControl;
using System.ServiceProcess;

namespace DaemonMasterCore
{
    public static class RegistryManagement
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                            REGISTRY                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Registry

        public static void SaveInRegistry(Daemon daemon)
        {
            using (RegistryKey serviceKey = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\" + daemon.ServiceName + @"\Parameters"))
            {
                serviceKey.SetValue("DisplayName", daemon.DisplayName, RegistryValueKind.String);
                serviceKey.SetValue("ServiceName", daemon.ServiceName, RegistryValueKind.String);

                serviceKey.SetValue("FileDir", daemon.FileDir, RegistryValueKind.String);
                serviceKey.SetValue("FileName", daemon.FileName, RegistryValueKind.String);

                serviceKey.SetValue("Parameter", daemon.Parameter, RegistryValueKind.String);
                serviceKey.SetValue("UserName", daemon.UserName, RegistryValueKind.String);
                serviceKey.SetValue("UserPassword", daemon.UserPassword, RegistryValueKind.String);
                serviceKey.SetValue("MaxRestarts", daemon.MaxRestarts, RegistryValueKind.DWord);

                serviceKey.SetValue("ProcessKillTime", daemon.ProcessKillTime, RegistryValueKind.DWord);
                serviceKey.SetValue("ProcessRestartDelay", daemon.ProcessRestartDelay, RegistryValueKind.DWord);
                serviceKey.SetValue("CounterResetTime", daemon.CounterResetTime, RegistryValueKind.DWord);

                serviceKey.SetValue("ConsoleApplication", daemon.ConsoleApplication, RegistryValueKind.DWord);
                serviceKey.SetValue("UseCtrlC", daemon.UseCtrlC, RegistryValueKind.DWord);

                serviceKey.Close();
            }
        }

        public static Daemon LoadDaemonFromRegistry(string serviceName)
        {
            //Open Regkey folder
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName, false))
            {
                if (key == null)
                    throw new Exception("Can't open registry key!");

                Daemon daemon = new Daemon();

                daemon.DisplayName = Convert.ToString(key.GetValue("DisplayName"));
                daemon.Description = Convert.ToString(key.GetValue("Description"));
                daemon.DependOnService = (string[])key.GetValue("DependOnService", String.Empty);

                //Open Parameters SubKey
                using (RegistryKey parameters = key.OpenSubKey(@"Parameters"))
                {
                    if (parameters == null)
                        throw new Exception("Can't open registry key!");

                    //daemon.DisplayName = (string)key.GetValue("DisplayName");
                    daemon.ServiceName = Convert.ToString(parameters.GetValue("ServiceName"));
                    daemon.FileDir = Convert.ToString(parameters.GetValue("FileDir"));
                    daemon.FileName = Convert.ToString(parameters.GetValue("FileName"));
                    daemon.Parameter = Convert.ToString(parameters.GetValue("Parameter"));
                    daemon.UserName = Convert.ToString(parameters.GetValue("UserName"));
                    daemon.UserPassword = Convert.ToString(parameters.GetValue("UserPassword"));
                    daemon.MaxRestarts = Convert.ToInt32(parameters.GetValue("MaxRestarts"));
                    daemon.ProcessKillTime = Convert.ToInt32(parameters.GetValue("ProcessKillTime"));
                    daemon.ProcessRestartDelay = Convert.ToInt32(parameters.GetValue("ProcessRestartDelay"));
                    daemon.CounterResetTime = Convert.ToInt32(parameters.GetValue("CounterResetTime"));
                    daemon.ConsoleApplication = Convert.ToBoolean(parameters.GetValue("ConsoleApplication"));
                    daemon.UseCtrlC = Convert.ToBoolean(parameters.GetValue("UseCtrlC"));

                    return daemon;
                }
            }
        }

        public static ObservableCollection<DaemonInfo> LoadDaemonInfosFromRegistry()
        {
            ObservableCollection<DaemonInfo> daemons = new ObservableCollection<DaemonInfo>();

            ServiceController[] sc = ServiceController.GetServices();

            foreach (ServiceController service in sc)
            {
                try
                {
                    if (service.ServiceName.Contains("DaemonMaster_"))
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + service.ServiceName + @"\Parameters", false))
                        {
                            if (key == null)
                                throw new Exception("Can't open registry key!");

                            DaemonInfo daemonInfo = new DaemonInfo
                            {
                                DisplayName = service.DisplayName,
                                ServiceName = service.ServiceName,
                                FullPath = (string)key.GetValue("FileDir") + @"/" + (string)key.GetValue("FileName")
                            };

                            daemons.Add(daemonInfo);
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return daemons;
        }








        //�ndert den Regkey so das Interactive Services erlaubt werden (Set NoInteractiveServices to 0)
        public static bool ActivateInteractiveServices()
        {
            try
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Windows", true))
                {
                    if (regKey == null)
                        return false;

                    regKey.SetValue("NoInteractiveServices", "0", RegistryValueKind.DWord);
                    regKey.Close();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        //�ndert den Regkey so das Interactive Services erlaubt werden (Set NoInteractiveServices to 0)
        public static bool CheckNoInteractiveServicesRegKey()
        {
            try
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Windows", false))
                {
                    return regKey != null && !Convert.ToBoolean(regKey.GetValue("NoInteractiveServices"));
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
