﻿using System.Runtime.InteropServices;

namespace DaemonMasterCore.Win32.PInvoke.Advapi32
{
    public static partial class Advapi32
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ServiceDescription
        {
            public string description;
        }
    }
}
