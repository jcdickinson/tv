using System;
using System.Drawing;
using TerminalVelocity.Eventing;
using TerminalVelocity.Plugins;
using TerminalVelocity.Renderer.Events;

namespace TerminalVelocity.Renderer
{
    public class GridRenderer : IPlugin
    {
        private readonly ISurface _surface;
        private SizeF Size;

        public GridRenderer(
            ISurface surface,

            RenderEvent onRenderEvent = null,
            ResizeEvent onResize = null
        )
        {
            _surface = surface ?? throw new ArgumentNullException(nameof(surface));
            onRenderEvent?.Subscribe(OnRender);
            onResize?.Subscribe(OnResize);
        }

        private EventStatus OnResize(in ResizeEventData e)
        {
            Size = e.Size;
            return EventStatus.Continue;
        }

        private EventStatus OnRender(in RenderEventData e)
        {
            var size = new SizeF(Size.Width, Size.Height);
            using (ISolidColorBrush brush = _surface.CreateSolidColorBrush(Color.White))
            using (ISolidColorBrush black = _surface.CreateSolidColorBrush(Color.Black))
            using (IFont font = _surface.CreateFont("Fira Code", 16))
            using (IText text = font.CreateText(brush, "Hello 😁 World".AsMemory(), size))
            {
                _surface.FillRectangle(black, new RectangleF(new PointF(0, 0), Size));
                text.Draw(new PointF(0, 0));
            }
            return EventStatus.Halt;
        }

        public void Dispose()
        {

        }
    }
}
