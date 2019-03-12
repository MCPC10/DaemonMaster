﻿/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: RegistryManagement
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
//   along with DeamonMaster.  If not, see <http://www.gnu.org/licenses/>.
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using DaemonMaster.Core.Win32;
using DaemonMaster.Core.Win32.PInvoke.Advapi32;
using Microsoft.Win32;

namespace DaemonMaster.Core
{
    public static class RegistryManagement
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                            REGISTRY                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Registry

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                              CONST                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////


        private const string RegPath = @"SYSTEM\CurrentControlSet\Services\";
        private const string RegPathServiceGroups = @"SYSTEM\CurrentControlSet\Control\ServiceGroupOrder\";

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                             METHODS                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void SaveInRegistry(DmServiceDefinition serviceDefinition)
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(RegPath + serviceDefinition.ServiceName))
            {
                //Open Parameters SubKey
                using (RegistryKey parameters = key.CreateSubKey("Parameters"))
                {
                    //Strings
                    parameters.SetValue("BinaryPath", serviceDefinition.BinaryPath, RegistryValueKind.String);
                    parameters.SetValue("Arguments", serviceDefinition.Arguments, RegistryValueKind.String);

                    //Ints
                    parameters.SetValue("ProcessMaxRestarts", serviceDefinition.ProcessMaxRestarts, RegistryValueKind.DWord);
                    parameters.SetValue("ProcessTimeoutTime", serviceDefinition.ProcessTimeoutTime, RegistryValueKind.DWord);
                    parameters.SetValue("ProcessRestartDelay", serviceDefinition.ProcessRestartDelay, RegistryValueKind.DWord);
                    parameters.SetValue("CounterResetTime", serviceDefinition.CounterResetTime, RegistryValueKind.DWord);
                    parameters.SetValue("ProcessPriority", serviceDefinition.ProcessPriority, RegistryValueKind.DWord);

                    //Bools
                    parameters.SetValue("IsConsoleApplication", serviceDefinition.IsConsoleApplication, RegistryValueKind.DWord);
                    parameters.SetValue("UseCtrlC", serviceDefinition.UseCtrlC, RegistryValueKind.DWord);
                    parameters.SetValue("CanInteractWithDesktop", serviceDefinition.CanInteractWithDesktop, RegistryValueKind.DWord);

                    parameters.Close();
                }


                //Create an give the user the permission to write to this key (needed for save the PID of the process if it's not the LocalSystem account)
                using (RegistryKey processInfo = key.CreateSubKey("ProcessInfo"))
                {
                    #region Setting permissions
                    //Only needed when user account has changed
                    if (!Equals(serviceDefinition.Credentials, ServiceCredentials.NoChange))
                    {
                        //Create a new RegistrySecurity object
                        var rs = new RegistrySecurity();

                        //  Author: Nick Sarabyn - https://stackoverflow.com/questions/3282656/setting-inheritance-and-propagation-flags-with-set-acl-and-powershell
                        //  ╔═════════════╦═════════════╦═════════════════════════════════╦══════════════════════════╦══════════════════╦═════════════════════════╦═══════════════╦═════════════╗
                        //  ║             ║ folder only ║ folder, sub - folders and files ║ folder and sub - folders ║ folder and files ║ sub - folders and files ║ sub - folders ║    files    ║
                        //  ╠═════════════╬═════════════╬═════════════════════════════════╬══════════════════════════╬══════════════════╬═════════════════════════╬═══════════════╬═════════════╣
                        //  ║ Propagation ║ none        ║ none                            ║ none                     ║ none             ║ InheritOnly             ║ InheritOnly   ║ InheritOnly ║
                        //  ║ Inheritance ║ none        ║ Container|Object                ║ Container                ║ Object           ║ Container|Object        ║ Container     ║ Object      ║
                        //  ╚═════════════╩═════════════╩═════════════════════════════════╩══════════════════════════╩══════════════════╩═════════════════════════╩═══════════════╩═════════════╝

                        ////Add access rule for user (only when it is not LocalSystem)                                          
                        if (Equals(serviceDefinition.Credentials, ServiceCredentials.LocalSystem))
                        {
                            rs.AddAccessRule(new RegistryAccessRule((NTAccount)new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null).Translate(typeof(NTAccount)), RegistryRights.WriteKey, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                        }
                        else
                        {
                            rs.AddAccessRule(new RegistryAccessRule(new NTAccount(DaemonMasterUtils.GetDomainFromUsername(serviceDefinition.Credentials.Username), DaemonMasterUtils.GetLoginFromUsername(serviceDefinition.Credentials.Username)), RegistryRights.WriteKey, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                        }

                        processInfo.SetAccessControl(rs);

                        #endregion
                    }

                    processInfo.Close();
                }
            }
        }


        public static DmServiceDefinition LoadServiceStartInfosFromRegistry(string serviceName)
        {
            //Open Regkey folder
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegPath + serviceName, false))
            {

                var serviceDefinition = new DmServiceDefinition(Convert.ToString(serviceName))
                {
                    DisplayName = Convert.ToString(key.GetValue("DisplayName")),
                    Credentials = new ServiceCredentials(Convert.ToString(key.GetValue("ObjectName", ServiceCredentials.LocalSystem)), null),
                    Description = Convert.ToString(key.GetValue("Description", string.Empty)),
                    DependOnService = (string[])key.GetValue("DependOnService", Array.Empty<string>()),
                    DependOnGroup = (string[])key.GetValue("DependOnGroup", Array.Empty<string>()),
                    DelayedStart = Convert.ToBoolean(key.GetValue("DelayedAutostart", false)),
                    StartType = (Advapi32.ServiceStartType)Convert.ToUInt32(key.GetValue("Start", 2))
                };


                //Open Parameters SubKey
                using (RegistryKey parameters = key.OpenSubKey("Parameters", false))
                {
                    serviceDefinition.BinaryPath = Convert.ToString(parameters.GetValue("BinaryPath"));
                    serviceDefinition.Arguments = Convert.ToString(parameters.GetValue("Parameter", string.Empty));
                    serviceDefinition.ProcessMaxRestarts = Convert.ToInt32(parameters.GetValue("ProcessMaxRestarts", 3));
                    serviceDefinition.ProcessTimeoutTime = Convert.ToInt32(parameters.GetValue("ProcessTimeoutTime", 9500));
                    serviceDefinition.ProcessRestartDelay = Convert.ToInt32(parameters.GetValue("ProcessRestartDelay", 2000));
                    serviceDefinition.CounterResetTime = Convert.ToInt32(parameters.GetValue("CounterResetTime", 43200));
                    serviceDefinition.ProcessPriority = (ProcessPriorityClass)parameters.GetValue("ProcessPriority", ProcessPriorityClass.Normal);
                    serviceDefinition.IsConsoleApplication = Convert.ToBoolean(parameters.GetValue("IsConsoleApplication", false));
                    serviceDefinition.UseCtrlC = Convert.ToBoolean(parameters.GetValue("UseCtrlC", false));
                    serviceDefinition.CanInteractWithDesktop = Convert.ToBoolean(parameters.GetValue("CanInteractWithDesktop", false));

                    return serviceDefinition;
                }
            }
        }

        public static object GetParameterFromRegistry(string serviceName, string parameterName, string subkey = "\\Parameters")
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegPath + serviceName + subkey, false))
            {
                return key.GetValue(parameterName, null);
            }
        }

        public static List<DmServiceDefinition> LoadInstalledServices()
        {
            var daemons = new List<DmServiceDefinition>();

            ServiceController[] sc = ServiceController.GetServices();

            foreach (ServiceController service in sc)
            {
                if (service.ServiceName.Contains("DaemonMaster_"))
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegPath + service.ServiceName + @"\Parameters", false))
                    {
                        if (key == null)
                            throw new Exception("Can't open registry key!");

                        var serviceDefinition = new DmServiceDefinition(service.ServiceName)
                        {
                            DisplayName = service.DisplayName,
                            BinaryPath = Convert.ToString(key.GetValue("BinaryPath")),
                        };

                        daemons.Add(serviceDefinition);
                    }
                }
            }
            return daemons;
        }

        public static string[] GetAllServiceGroups()
        {
            //Open Regkey folder
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegPathServiceGroups, false))
            {
                return (string[])key.GetValue("List", string.Empty);
            }
        }





        //Set NoInteractiveServices to 0
        public static bool EnableInteractiveServices(bool enable)
        {
            //If Windows10 1803 or higher return false (UI0Detect service does not exist anymore)
            if (!DaemonMasterUtils.IsSupportedWindows10VersionOrLower())
                return false;

            try
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Windows", true))
                {
                    if (regKey == null)
                        return false;

                    regKey.SetValue("NoInteractiveServices", enable ? 0 : 1, RegistryValueKind.DWord);
                    regKey.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        //Check if NoInteractiveServices is 0
        public static bool CheckNoInteractiveServicesRegKey()
        {
            //If Windows10 1803 or higher return false (UI0Detect service does not exist anymore)
            if (!DaemonMasterUtils.IsSupportedWindows10VersionOrLower())
                return false;

            try
            {
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Windows", false))
                {
                    return regKey != null && !Convert.ToBoolean(regKey.GetValue("NoInteractiveServices"));
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
