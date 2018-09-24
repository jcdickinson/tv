using SharpDX;

namespace TerminalVelocity.Direct2D.Events
{
    public struct HitTestEvent
    {
        public const string ContractName = "HitTest.Events.Direct2D.TerminalVelocity";

        public readonly Point Point;
        
        public WinApi.User32.HitTestResult Region;

        public bool IsInBounds => (int)Region > 0;

        public HitTestEvent(in Point point)
        {
            Point = point;
            Region = default;
        }
    }
}