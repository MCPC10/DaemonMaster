/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: CustomActions
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
using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Threading;

namespace DaemonMasterCustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult UninstallAllServices(Session session)
        {
            session.Log("Begin the unistall of all services");

            try
            {
                session.Log("Killing all services...");
                ServiceManagement.KillAllServices();

                Thread.Sleep(2000);

                session.Log("Deleting all services...");
                ServiceManagement.DeleteAllServices();
            }
            catch (Exception e)
            {
                session.Log(e.Message);
                return ActionResult.Failure;
            }

            session.Log("Success");
            return ActionResult.Success;
        }
    }
}