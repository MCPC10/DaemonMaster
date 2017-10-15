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
//   along with DeamonMaster.  If not, see <http://www.gnu.org/licenses/>.
/////////////////////////////////////////////////////////////////////////////////////////

namespace DaemonMasterCore.Win32.PInvoke
{
    public static partial class NativeMethods
    {
        /// <summary>
        /// Needed for QueryServiceStatusEx as infoLevel
        /// </summary>
        public const uint SC_STATUS_PROCESS_INFO = 0x0;
        public const int CREATE_NEW_CONSOLE = 0x00000010;
        public const string SC_GROUP_IDENTIFIER = "+";

    }
}
