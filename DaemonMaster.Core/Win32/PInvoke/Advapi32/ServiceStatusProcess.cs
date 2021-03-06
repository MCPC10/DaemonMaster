﻿using System.Runtime.InteropServices;

namespace DaemonMaster.Core.Win32.PInvoke.Advapi32
{
    public static partial class Advapi32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatusProcess
        {
            public uint serviceType;
            public ServiceCurrentState currentState;
            public uint controlsAccepted;
            public uint win32ExitCode;
            public uint serviceSpecificExitCode;
            public uint checkPoint;
            public uint waitHint;
            public uint processId;
            public uint serviceFlags;
        }
    }
}
