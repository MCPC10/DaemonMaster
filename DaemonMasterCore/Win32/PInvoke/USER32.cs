/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: NativeMethods
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
using System.Runtime.InteropServices;

namespace DaemonMasterCore.Win32.PInvoke
{
    //FROM PINVOKE
    public static partial class NativeMethods
    {
        [Obsolete("Not needed anymore!")]
        [DllImport(DLLFiles.USER32, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, wParam wParam, IntPtr lParam);

        public const Int32 _SYSCOMMAND = 0x0112;

        public enum wParam : Int32
        {
            SC_CLOSE = 0xF060,
            SC_CONTEXTHELP = 0xF180,
            SC_DEFAULT = 0xF160,
            SC_HOTKEY = 0xF150,
            SC_HSCROLL = 0xF080,
            SCF_ISSECURE = 0x00000001,
            SC_KEYMENU = 0xF100,
            SC_MAXIMIZE = 0xF030,
            SC_MINIMIZE = 0xF020,
            SC_MONITORPOWER = 0xF170,
            SC_MOUSEMENU = 0xF090,
            SC_MOVE = 0xF010,
            SC_NEXTWINDOW = 0xF040,
            SC_PREVWINDOW = 0xF050,
            SC_RESTORE = 0xF120,
            SC_SCREENSAVE = 0xF140,
            SC_SIZE = 0xF000,
            SC_TASKLIST = 0xF130,
            SC_VSCROLL = 0xF070
        }

    }
}
