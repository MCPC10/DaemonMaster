using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using DaemonMaster.Core.Win32.PInvoke.Kernel32;

namespace DaemonMaster.Core
{
    public static class Security
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                            Security                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Security

        /// <summary>
        /// Convert the given string to a SecureString
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static SecureString ConvertStringToSecureString(this string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return null;

            var secString = new SecureString();

            if (data.Length <= 0) return null;

            foreach (char c in data)
            {
                secString.AppendChar(c);
            }
            return secString;
        }

        /// <summary>
        /// Convert the given SecureString to a normal string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ConvertSecureStringToString(this SecureString data)
        {
            if (data == null)
                return null;

            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(data);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        /// <summary>
        /// Compare two secure strings in unmanaged code
        /// </summary>
        /// <param name="data1">String 1</param>
        /// <param name="data2">String 2</param>
        /// <returns></returns>
        public static bool IsEquals(this SecureString data1, SecureString data2)
        {
            if (data1 == null && data2 == null)
                return true;

            if (data1 == null || data2 == null)
                return false;

            IntPtr ptr1 = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;

            try
            {
                ptr1 = Marshal.SecureStringToGlobalAllocUnicode(data1);
                ptr2 = Marshal.SecureStringToGlobalAllocUnicode(data2);

                int result = Kernel32.CompareStringOrdinal(ptr1, data1.Length, ptr2, data2.Length, ignoreCase: false);
                if (result == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                return result == Kernel32.CstrEqual;
            }
            finally
            {
                if (ptr1 != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr1);

                if (ptr2 != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr2);
            }
        }
        #endregion
    }
}
