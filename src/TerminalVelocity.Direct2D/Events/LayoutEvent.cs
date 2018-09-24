using SharpDX;

namespace TerminalVelocity.Direct2D.Events
{
    public struct LayoutEvent
    {
        public const string ContractName = "Layout.Events.Direct2D.TerminalVelocity";

        public readonly RectangleF NewSize;

        public LayoutEvent(in RectangleF newSize)
        {
            NewSize = newSize;
        }
    }
}