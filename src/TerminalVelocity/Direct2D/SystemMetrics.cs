using System;
using System.Runtime.InteropServices;
using NetCoreEx.Geometry;
using WinApi.User32;

namespace TerminalVelocity.Direct2D
{
    internal static class WindowsLayout
    {
        private const string User32 = "User32.dll";
        private const string UxTheme = "UxTheme.dll";
        
        private struct NonClientMetrics
        {
            private readonly int cbSize;
            public int iBorderWidth;
            public int iScrollWidth;
            public int iScrollHeight;
            public int iCaptionWidth;
            public int iCaptionHeight;
            public Font lfCaptionFont;
            public int iSmCaptionWidth;
            public int iSmCaptionHeight;
            public Font lfSmCaptionFont;
            public int iMenuWidth;
            public int iMenuHeight;
            public Font lfMenuFont;
            public Font lfStatusFont;
            public Font lfMessageFont;

            public NonClientMetrics(int size)
                : this()
            {
                cbSize = size;
            }

            public static NonClientMetrics Current
            {
                get
                {
                    var metrics = new NonClientMetrics(Marshal.SizeOf<NonClientMetrics>());
                    SystemParametersInfo(0x0029, 0, ref metrics, 0);
                    return metrics;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct Font
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string lfFaceName;
        }

        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SystemParametersInfo(int action,
                                                int intParam,
                                                ref NonClientMetrics metrics,
                                                int update);

        private static readonly Refresh<int> _sizeFrameWidth = new Refresh<int>(() => User32Methods.GetSystemMetrics(SystemMetrics.SM_CXSIZEFRAME));
        public static int SizeFrameWidth => _sizeFrameWidth;

        private static readonly Refresh<int> sizeFrameHeight = new Refresh<int>(() => User32Methods.GetSystemMetrics(SystemMetrics.SM_CYSIZEFRAME));
        public static int SizeFrameHeight => sizeFrameHeight;

        // HACK: Not sure what the correct metric is here, add a constant.
        private static readonly Refresh<int> _captionButtonWidth = new Refresh<int>(() => User32Methods.GetSystemMetrics(SystemMetrics.SM_CXSIZE));
        public static int CaptionButtonWidth => _captionButtonWidth + 9;

        private static readonly Refresh<int> _captionBarHeight = new Refresh<int>(() => User32Methods.GetSystemMetrics(SystemMetrics.SM_CYSIZE));
        public static int CaptionBarHeight => _captionBarHeight;
        
        private static readonly Refresh<int> _windowPaddingWidth = new Refresh<int>(() => User32Methods.GetSystemMetrics(SystemMetrics.SM_CXPADDEDBORDER));
        public static int WindowPaddingWidth => _windowPaddingWidth;
        
        // HACK: There is no SM_CYPADDEDBORDER
        private static readonly Refresh<int> _windowPaddingHeight = new Refresh<int>(() => User32Methods.GetSystemMetrics(SystemMetrics.SM_CXPADDEDBORDER));
        public static int WindowPaddingHeight => _windowPaddingHeight;

        private static readonly Refresh<NonClientMetrics> _nonClientMetrics = new Refresh<NonClientMetrics>(() => NonClientMetrics.Current);
        public static int CaptionTextHeight => _nonClientMetrics.Value.iCaptionHeight;
        public static string CaptionFont => _nonClientMetrics.Value.lfCaptionFont.lfFaceName;
    }
}