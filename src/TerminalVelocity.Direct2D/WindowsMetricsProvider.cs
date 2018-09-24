using System;
using System.Composition;
using System.Drawing;
using System.Runtime.InteropServices;
using TerminalVelocity.Preferences;
using WinApi.User32;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    public sealed class WindowsMetricsProvider
    {
        private const string User32 = "User32.dll";
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

            public static NonClientMetrics GetCurrent()
            {
                var metrics = new NonClientMetrics(Marshal.SizeOf<NonClientMetrics>());
                SystemParametersInfo(0x0029, 0, ref metrics, 0);
                return metrics;
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

        public const string SizeFrameContract = "SizeFrame.Window.Windows";
        public const string CaptionButtonContract = "Button.Caption.Window.Windows";
        public const string CaptionBarContract = "Caption.Window.Windows";
        public const string WindowPaddingContract = "Padding.Window.Windows";
        public const string CaptionTextContract = BrushProvider.ChromeTextContract;

        [Export(SizeFrameContract)]
        public Configurable<Size> SizeFrame { get; }
        [Export(CaptionButtonContract)]
        public Configurable<Size> CaptionButton { get; }
        [Export(CaptionBarContract)]
        public Configurable<Size> CaptionBarHeight { get; }
        [Export(WindowPaddingContract)]
        public Configurable<Size> WindowPadding { get; }
        [Export(CaptionTextContract)]
        public Configurable<Size> CaptionTextSize { get; }
        [Export(CaptionTextContract)]
        public Configurable<string> CaptionTextFamily { get; }

        private readonly Configurable<NonClientMetrics> _nonClientMetrics;

        public WindowsMetricsProvider()
        {
            SizeFrame = new Configurable<Size>(() => new Size(
                User32Methods.GetSystemMetrics(SystemMetrics.SM_CXSIZEFRAME),
                User32Methods.GetSystemMetrics(SystemMetrics.SM_CYSIZEFRAME)
            ));

            CaptionButton = new Configurable<Size>(() => new Size(
                // HACK: Not sure what the correct metric is here, add a constant.
                User32Methods.GetSystemMetrics(SystemMetrics.SM_CXSIZE) + 9,
                User32Methods.GetSystemMetrics(SystemMetrics.SM_CYSIZE)
            ));

            CaptionBarHeight = new Configurable<Size>(() => new Size(
                User32Methods.GetSystemMetrics(SystemMetrics.SM_CXSIZE),
                User32Methods.GetSystemMetrics(SystemMetrics.SM_CYSIZE)
            ));

            WindowPadding = new Configurable<Size>(() => new Size(
                User32Methods.GetSystemMetrics(SystemMetrics.SM_CXPADDEDBORDER),
                // HACK: There is no SM_CYPADDEDBORDER
                User32Methods.GetSystemMetrics(SystemMetrics.SM_CXPADDEDBORDER)
            ));

            _nonClientMetrics = new Configurable<NonClientMetrics>(NonClientMetrics.GetCurrent);
            
            CaptionTextSize = _nonClientMetrics.Select(x => new Size(
                FontHeight(x.lfCaptionFont.lfWidth),
                FontHeight(x.lfCaptionFont.lfHeight)
            ));

            CaptionTextFamily = _nonClientMetrics.Select(nc => nc.lfCaptionFont.lfFaceName);
        }

        private int FontHeight(int size)
        {
            if (size < 0)
            {
                // The font mapper transforms this value into device units and
                // matches its absolute value against the character height of
                // the available fonts.

                return -size;
            }
            else if (size > 0)
            {
                // The font mapper transforms this value into device units and
                // matches it against the cell height of the available fonts.

                return size;
            }
            else
            {
                return 16;
            }
        }
    }
}