/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace TerminalVelocity.WinPty
{
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        public const string Kernel32 = "Kernel32.DLL";

        [DllImport(Kernel32)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport(Kernel32)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport(Kernel32)]
        public static extern bool FreeLibrary(IntPtr hModule);
    }
}
