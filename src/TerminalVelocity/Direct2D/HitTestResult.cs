namespace TerminalVelocity.Direct2D
{
    public struct HitTestResult
    {
        public WinApi.User32.HitTestResult Region { get; set; }

        public HitTestFlags Flags { get; set; }

        public bool IsInBounds => (int)Region > 0;

        public HitTestResult(WinApi.User32.HitTestResult region, HitTestFlags flags)
        {
            Region = region;
            Flags = flags;
        }

        public HitTestResult(WinApi.User32.HitTestResult region)
            : this(region, HitTestFlags.None)
        { }

        public HitTestResult(HitTestFlags flags)
            : this(WinApi.User32.HitTestResult.HTCLIENT, flags)
        { }

        public bool TrySetRegion(WinApi.User32.HitTestResult result)
        {
            if (Region == WinApi.User32.HitTestResult.HTNOWHERE)
            {
                Region = result;
                return true;
            }
            return false;
        }
    }
}