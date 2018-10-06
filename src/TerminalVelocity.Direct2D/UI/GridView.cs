using System;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using TerminalVelocity.Direct2D.Events;
using WinApi.DxUtils.Component;

namespace TerminalVelocity.Direct2D.UI
{
    public sealed class GridView
    {
        private readonly Dx11Component _component;
        private readonly SharpDX.DirectWrite.Factory _factory;
        private readonly RenderEvent _renderEvent;

        private readonly BrushProvider _brushes;
        private readonly FontProvider _fonts;

        private RectangleF _frame;

        public GridView(
            Dx11Component component,
            BrushProvider brushes,
            FontProvider fonts,
            SharpDX.DirectWrite.Factory factory,
            RenderEvent renderEvent)
        {
            _component = component;
            _brushes = brushes;
            _fonts = fonts;
            _factory = factory;
            _renderEvent = renderEvent;
        }

        public void Layout(in RectangleF container) => _frame = container;

        public void Render()
        {
            _component.D2D.Context.Transform = Matrix3x2.Identity;
            _component.D2D.Context.FillRectangle(_frame, _brushes.TerminalColor0);
            Vector2 origin = _frame.TopLeft;
            var text = "test => text 😁 " + Environment.TickCount;

            using (var layout = new TextLayout(_factory, text, _fonts.TerminalText, _frame.Width, _frame.Height))
            {
                _component.D2D.Context.DrawTextLayout(origin, layout, _brushes.TerminalColor1, DrawTextOptions.EnableColorFont);
                layout.SetFontWeight(FontWeight.Bold, new TextRange(1, 2));
                origin.Y += layout.Metrics.Height;
            }

            //OnRender.Publish(new RenderEvent());
        }
    }
}
