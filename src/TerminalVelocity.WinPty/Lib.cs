/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace TerminalVelocity.WinPty
{
    internal readonly struct Lib
    {
        private sealed class ConstLPWStrMarshaler : ICustomMarshaler
        {
            private static readonly ICustomMarshaler Instance = new ConstLPWStrMarshaler();

            public static ICustomMarshaler GetInstance(string cookie) => Instance;

            public object MarshalNativeToManaged(IntPtr pNativeData) => Marshal.PtrToStringUni(pNativeData);

            public void CleanUpNativeData(IntPtr pNativeData) { }

            public int GetNativeDataSize() => throw new NotSupportedException();

            public IntPtr MarshalManagedToNative(object ManagedObj) => throw new NotSupportedException();

            public void CleanUpManagedData(object ManagedObj) => throw new NotSupportedException();
        }

        public enum Error : int
        {
            Success = 0,
            OutOfMemory = 1,
            SpawnCreateProcessFailed = 2,
            LostConnection = 3,
            AgentExeMissing = 4,
            Unspecified = 5,
            AgentDied = 6,
            AgentTimeout = 7,
            AgentCreationFailed = 8
        }

        [Flags]
        public enum AgentOptions : ulong
        {
            None = 0x0,
            ConError = 0x1,
            PlainOutput = 0x2,
            ColorEscapes = 0x4,
            AllowDesktopCreation = 0x8
        }

        [Flags]
        public enum SpawnOptions : ulong
        {
            None = 0x0,
            AutoShutdown = 0x1,
            ExitAfterShutdown = 0x2
        }

        public enum MouseMode : ulong
        {
            None = 0x0,
            Auto = 0x1,
            Force = 0x2
        }

        [SuppressUnmanagedCodeSecurity]
        public static class Prototypes
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error ErrorCode(IntPtr error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstLPWStrMarshaler))]
            public delegate string ErrorMessage(IntPtr error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ErrorFree(IntPtr error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr ConfigNew(AgentOptions agentFlags, out IntPtr error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ConfigFree(IntPtr config);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ConfigSetInitialSize(IntPtr config, int cols, int rows);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ConfigSetMouseMode(IntPtr config, MouseMode mouseMode);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void ConfigSetAgentTimeout(IntPtr config, int timeout);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr Open(IntPtr config, out IntPtr error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstLPWStrMarshaler))]
            public delegate string ConInName(IntPtr pty);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstLPWStrMarshaler))]
            public delegate string ConOutName(IntPtr pty);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstLPWStrMarshaler))]
            public delegate string ConErrorName(IntPtr pty);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate IntPtr SpawnConfigNew(SpawnOptions spawnFlags, string appname, string cmdline, string cwd, string env, out IntPtr error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SpawnConfigFree(IntPtr cfg);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool Spawn(IntPtr pty, IntPtr cfg, out IntPtr processHandle, out IntPtr threadHandle, out int createProcessError, out IntPtr error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool SetSize(IntPtr pty, int cols, int rows, out IntPtr err);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void Free(IntPtr pty);
        }

        public readonly Prototypes.ErrorCode ErrorCode;
        public readonly Prototypes.ErrorMessage ErrorMessage;
        public readonly Prototypes.ErrorFree ErrorFree;
        public readonly Prototypes.ConfigNew ConfigNew;
        public readonly Prototypes.ConfigFree ConfigFree;
        public readonly Prototypes.ConfigSetInitialSize ConfigSetInitialSize;
        public readonly Prototypes.ConfigSetMouseMode ConfigSetMouseMode;
        public readonly Prototypes.ConfigSetAgentTimeout ConfigSetAgentTimeout;
        public readonly Prototypes.Open Open;
        public readonly Prototypes.ConInName ConInName;
        public readonly Prototypes.ConOutName ConOutName;
        public readonly Prototypes.ConErrorName ConErrorName;
        public readonly Prototypes.SpawnConfigNew SpawnConfigNew;
        public readonly Prototypes.SpawnConfigFree SpawnConfigFree;
        public readonly Prototypes.Spawn Spawn;
        public readonly Prototypes.SetSize SetSize;
        public readonly Prototypes.Free Free;

        private Lib(IntPtr module)
        {
            GetFunction(out ErrorCode, module, "winpty_error_code");
            GetFunction(out ErrorMessage, module, "winpty_error_msg");
            GetFunction(out ErrorFree, module, "winpty_error_free");

            GetFunction(out ConfigNew, module, "winpty_config_new");
            GetFunction(out ConfigFree, module, "winpty_config_free");
            GetFunction(out ConfigSetInitialSize, module, "winpty_config_set_initial_size");
            GetFunction(out ConfigSetMouseMode, module, "winpty_config_set_mouse_mode");
            GetFunction(out ConfigSetAgentTimeout, module, "winpty_config_set_agent_timeout");

            GetFunction(out Open, module, "winpty_open");
            // winpty_agent_process";

            GetFunction(out ConInName, module, "winpty_conin_name");
            GetFunction(out ConOutName, module, "winpty_conout_name");
            GetFunction(out ConErrorName, module, "winpty_conerr_name");

            GetFunction(out SpawnConfigNew, module, "winpty_spawn_config_new");
            GetFunction(out SpawnConfigFree, module, "winpty_spawn_config_free");

            GetFunction(out Spawn, module, "winpty_spawn");
            GetFunction(out SetSize, module, "winpty_set_size");
            // "winpty_get_console_process_list"

            GetFunction(out Free, module, "winpty_free");
        }

        public static Lib Create()
        {
            var bits = IntPtr.Size == 8 ? "x64": "x86";

            var searchPaths = new[]
            {
                Path.GetFullPath(Path.Combine(typeof(Lib).Assembly.Location, @"winpty", bits, "winpty.dll")),
#               if DEBUG
                Path.GetFullPath(Path.Combine(typeof(Lib).Assembly.Location, @"..\..\..\..\..\..\publish\winpty", bits, "winpty.dll"))
#               endif
            };

            var path = searchPaths.FirstOrDefault(File.Exists);
            if (path == null) throw new InvalidOperationException("WinPty not found.");

            IntPtr module = NativeMethods.LoadLibrary(path);
            if (module == IntPtr.Zero) throw new InvalidOperationException($"Could not load {path}.");

            return new Lib(module);
        }

        public IntPtr CheckResult(IntPtr result, IntPtr error, string defaultMessage)
        {
            if (result == IntPtr.Zero)
            {
                if (error != IntPtr.Zero)
                {
                    Error errorCode = ErrorCode(error);
                    string message = ErrorMessage(error);
                    ErrorFree(error);
                    throw new InvalidOperationException($"{errorCode}: {ErrorMessage}");
                }
                throw new InvalidOperationException(defaultMessage);
            }
            return result;
        }

        public void CheckResult(bool result, IntPtr error, string defaultMessage)
        {
            if (!result)
            {
                if (error != IntPtr.Zero)
                {
                    Error errorCode = ErrorCode(error);
                    string message = ErrorMessage(error);
                    ErrorFree(error);
                    throw new InvalidOperationException($"{errorCode}: {ErrorMessage}");
                }
                throw new InvalidOperationException(defaultMessage);
            }
        }

        private static void GetFunction<T>(out T field, IntPtr module, string name)
        {
            IntPtr addr = NativeMethods.GetProcAddress(module, name);
            if (addr == IntPtr.Zero) throw new InvalidOperationException($"Export {name} not found.");
            field = Marshal.GetDelegateForFunctionPointer<T>(addr);
        }

        public static void ParsePipeName(ref string pipeName, out string serverName)
        {
            ReadOnlySpan<char> result = pipeName.AsSpan();
            if (result.StartsWith("\\\\", StringComparison.Ordinal))
            {
                result = result.Slice(2);
                var index = result.IndexOf("\\", StringComparison.Ordinal);
                if (index >= 0)
                {
                    serverName = new string(result.Slice(0, index));
                    result = result.Slice(index + 1);
                }
                else
                {
                    serverName = ".";
                }

                index = result.IndexOf("\\", StringComparison.Ordinal);
                if (index >= 0)
                {
                    pipeName = new string(result.Slice(index + 1));
                }
            }
            else
            {
                serverName = ".";
            }
        }
    }
}
