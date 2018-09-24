using System;
using System.Composition;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Preferences;
using WinApi.User32;

namespace TerminalVelocity.Direct2D.UI
{
    [Shared, Export]
    public sealed class GridView
    {
        [Import(RenderEvent.ContractName)]
        public Event<RenderEvent> OnRender { private get; set; }

        private readonly DeviceContext _context;
        private readonly SharpDX.DirectWrite.Factory _factory;

        private readonly Configurable<Brush> _color0;
        private readonly Configurable<Brush> _color1;
        private readonly Configurable<TextFormat> _font;

        private RectangleF _frame;

        [ImportingConstructor]
        public GridView(
            [Import(BrushProvider.TerminalColor0Contract)] Configurable<Brush> color0,
            [Import(BrushProvider.TerminalColor1Contract)] Configurable<Brush> color1,
            [Import(FontProvider.TerminalTextContract)] Configurable<TextFormat> font,
            [Import] SharpDX.DirectWrite.Factory factory,
            [Import] DeviceContext context)
        {
            _context = context;
            _color0 = color0;
            _color1 = color1;
            _font = font;
            _factory = factory;
        }

        public void Layout(in RectangleF container)
        {
            _frame = container;
        }

        public void HitTest(ref HitTestEvent payload)
        {
            if (_frame.Contains(payload.Point))
                payload.Region = WinApi.User32.HitTestResult.HTCLIENT;
        }

        public void Render()
        {
            _context.Transform = Matrix3x2.Identity;
            _context.FillRectangle(_frame, _color0);
            var origin = _frame.TopLeft;
            var text = "test => text 😁 " + Environment.TickCount;

            using (var layout = new TextLayout(_factory, text, _font, _frame.Width, _frame.Height))
            {
                _context.DrawTextLayout(origin, layout, _color1, DrawTextOptions.EnableColorFont);
                layout.SetFontWeight(FontWeight.Bold, new TextRange(1, 2));
                origin.Y += layout.Metrics.Height;
            }

            OnRender.Publish(new RenderEvent());
        }
    }
}