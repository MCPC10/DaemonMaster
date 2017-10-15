/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: JobHandle
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

using DaemonMasterCore.Win32.PInvoke;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DaemonMasterCore.Win32
{
    public class JobHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public JobHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(handle);
        }

        public static JobHandle CreateJob(NativeMethods.SECURITY_ATTRIBUTES lpJobAttributes, string lpName)
        {
            JobHandle jobHandle = NativeMethods.CreateJobObject(lpJobAttributes, lpName);

            if (jobHandle.IsInvalid)
                throw new Win32Exception("Unable to create job. Error: " + Marshal.GetLastWin32Error());

            return jobHandle;
        }

        public void SetInformation(NativeMethods.JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength)
        {
            if (!NativeMethods.SetInformationJobObject(this, infoType, lpJobObjectInfo, cbJobObjectInfoLength))
                throw new Win32Exception("Unable to set job informations. Error: " + Marshal.GetLastWin32Error());
        }

        public void AssignProcess(SafeProcessHandle hProcess)
        {
            if (!NativeMethods.AssignProcessToJobObject(this, hProcess))
                throw new Win32Exception("Unable to assign process to the job. Error: " + Marshal.GetLastWin32Error());
        }
    }
}
