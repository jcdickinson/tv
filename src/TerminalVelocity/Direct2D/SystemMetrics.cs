using System;
using System.Runtime.InteropServices;
using NetCoreEx.Geometry;

namespace TerminalVelocity.Direct2D
{
    internal static class SystemMetrics
    {
        private const string User32 = "User32.dll";

        private enum Metric
        {
            SizeFrameWidth = 0x20,
            SizeFrameHeight = 0x21,
            CaptionButtonWidth = 0x1E,
            CaptionButtonHeight = 0x1F,
        }

        private enum MonitorFlags
        {
            DefaultToNull = 0,
            DefaultToPrimary = 1,
            DefaultToNearest = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RectangleL
        {
            public ulong Left;
            public ulong Top;
            public ulong Right;
            public ulong Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MonitorInfo
        {
            public int Size;
            public Rectangle Monitor;
            public Rectangle WorkingArea;
            public uint Flags;

            public static MonitorInfo Create() => new MonitorInfo()
            {
                Size = Marshal.SizeOf<MonitorInfo>()
            };
        }

        [DllImport(User32)]
        private static extern int GetSystemMetrics(Metric metric);

        [DllImport(User32)]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, MonitorFlags flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(uint monitor, ref MonitorInfo mInfo);

        public static int SizeFrameWidth => GetSystemMetrics(Metric.SizeFrameWidth);
        public static int SizeFrameHeight => GetSystemMetrics(Metric.SizeFrameHeight);
        public static int CaptionButtonWidth => GetSystemMetrics(Metric.CaptionButtonWidth);
        public static int CaptionButtonHeight => GetSystemMetrics(Metric.CaptionButtonHeight);

        public static Rectangle GetMonitorWorkingArea(IntPtr handle)
        {
            var monitor = MonitorFromWindow(IntPtr.Zero, MonitorFlags.DefaultToPrimary);
            var info = MonitorInfo.Create();
            var result = GetMonitorInfo((uint)monitor, ref info);
            return info.WorkingArea;
            return new Rectangle(
                (int)info.WorkingArea.Left, (int)info.WorkingArea.Top,
                (int)info.WorkingArea.Right, (int)info.WorkingArea.Bottom);
        }
    }
}