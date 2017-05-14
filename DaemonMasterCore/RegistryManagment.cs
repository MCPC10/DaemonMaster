/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: REGISTRY MANAGMENT FILE
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
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceProcess;

namespace DaemonMasterCore
{
    public static class RegistryManagment
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
            }
        }

        public static ObservableCollection<Daemon> LoadFromRegistry()
        {
            ObservableCollection<Daemon> daemons = new ObservableCollection<Daemon>();

            ServiceController[] sc = ServiceController.GetServices();

            foreach (ServiceController service in sc)
            {
                try
                {
                    if (service.ServiceName.Contains("DaemonMaster_"))
                    {
                        daemons.Add(LoadDaemonFromRegistry(service.ServiceName));
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return daemons;
        }

        public static Daemon LoadDaemonFromRegistry(string serviceName)
        {
            //Open Regkey folder
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName + @"\Parameters", false))
            {
                if (key == null)
                    throw new Exception("Can't open registry key!");

                Daemon daemon = new Daemon();

                daemon.DisplayName = (string)key.GetValue("DisplayName");
                daemon.ServiceName = (string)key.GetValue("ServiceName");

                daemon.FileDir = (string)key.GetValue("FileDir");
                daemon.FileName = (string)key.GetValue("FileName");

                daemon.Parameter = (string)(key.GetValue("Parameter") ?? String.Empty);
                daemon.UserName = (string)(key.GetValue("UserName") ?? String.Empty);
                daemon.UserPassword = (string)(key.GetValue("UserPassword") ?? String.Empty);
                daemon.MaxRestarts = (int)(key.GetValue("MaxRestarts") ?? 3);

                daemon.ProcessKillTime = (int)(key.GetValue("ProcessKillTime") ?? 5000);
                daemon.ProcessRestartDelay = (int)(key.GetValue("ProcessRestartDelay") ?? 0);
                daemon.CounterResetTime = (int)(key.GetValue("CounterResetTime") ?? 2000);

                daemon.ConsoleApplication = Convert.ToBoolean((key.GetValue("ConsoleApplication") ?? false));
                daemon.UseCtrlC = Convert.ToBoolean((key.GetValue("UseCtrlC") ?? false));

                return daemon;
            }
        }

        #endregion
    }
}
