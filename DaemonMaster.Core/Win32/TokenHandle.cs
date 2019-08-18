using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using DaemonMaster.Core.Win32.PInvoke.Advapi32;
using DaemonMaster.Core.Win32.PInvoke.Kernel32;
using DaemonMaster.Core.Win32.PInvoke.Wtsapi32;
using Microsoft.Win32.SafeHandles;

namespace DaemonMaster.Core.Win32
{
    public class TokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public TokenHandle() : base(ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return Kernel32.CloseHandle(handle);
        }

        /// <summary>
        /// Gets the logon token from a session ID. Only possible if the caller has LocalSystem rights
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns></returns>
        public static TokenHandle GetTokenFromSessionId(uint sessionId)
        {
            if (!Wtsapi32.WTSQueryUserToken(sessionId, out TokenHandle currentUserToken))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return currentUserToken;
        }

        /// <summary>
        /// Allows you to get the token from a user logon
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password to login</param>
        /// <param name="logonTyp">The logon type</param>
        /// <returns></returns>
        public static TokenHandle GetTokenFromLogon(string username, SecureString password, Advapi32.LogonType logonTyp)
        {
            IntPtr passwordHandle = IntPtr.Zero;

            try
            {
                passwordHandle = Marshal.SecureStringToGlobalAllocUnicode(password);
                if (!Advapi32.LogonUser(DaemonMasterUtils.GetLoginFromUsername(username), DaemonMasterUtils.GetDomainFromUsername(username), passwordHandle, logonTyp, Advapi32.LogonProvider.Default, out TokenHandle token))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return token;
            }
            finally
            {
                if (passwordHandle != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(passwordHandle);
            }
        }

        /// <summary>
        /// Gets the active session user token.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
        public static TokenHandle GetActiveSessionUserToken()
        {
            //!!! Parts are from Copyright (c) 2014 Justin Murray - MIT-License, thanks to him !!!
            // https://github.com/murrayju/CreateProcessAsUser, 18.08.2019

            var primTokenHandle = new TokenHandle();
            uint activeSessionId = Wtsapi32.InvalidSessionId;
            IntPtr sessionInfos = IntPtr.Zero;
            var sessionCount = 0;

            try
            {
                //Get a handle to the user access token for the current active session.
                if (Wtsapi32.WTSEnumerateSessions(Wtsapi32.WtsCurrentServerHandle, 0, 1, ref sessionInfos, ref sessionCount))
                {
                    int arrayElementSize = Marshal.SizeOf(typeof(Wtsapi32.WtsSessionInfo));
                    IntPtr current = sessionInfos;

                    for (var i = 0; i < sessionCount; i++)
                    {
                        var sessionInfo = (Wtsapi32.WtsSessionInfo)Marshal.PtrToStructure(current, typeof(Wtsapi32.WtsSessionInfo));
                        current += arrayElementSize;

                        if (sessionInfo.State == Wtsapi32.WtsConnectstateClass.WTSActive)
                        {
                            activeSessionId = (uint)sessionInfo.SessionID;
                        }
                    }
                }
            }
            finally
            {
                if (sessionInfos != IntPtr.Zero)
                    Wtsapi32.WTSFreeMemory(sessionInfos);
            }

            //If enumerating not working use the fall back method
            if (activeSessionId == Wtsapi32.InvalidSessionId)
            {
                activeSessionId = Kernel32.WTSGetActiveConsoleSessionId();

                if (activeSessionId == Wtsapi32.InvalidSessionId)
                    throw new Exception("No valid session found.");
            }

            var impersonationToken = new TokenHandle();
            try
            {
                //Get token
                if (!Wtsapi32.WTSQueryUserToken(activeSessionId, out impersonationToken))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                //Convert the impersonation token to a primary token
                if (!Advapi32.DuplicateTokenEx(impersonationToken, 0, IntPtr.Zero, (int)Advapi32.SecurityImpersonationLevel.SecurityImpersonation, (int)Advapi32.TokenType.TokenPrimary, ref primTokenHandle))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                impersonationToken.Close();
            }

            return primTokenHandle;
        }
    }
}
