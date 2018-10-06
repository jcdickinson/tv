using System;
using SharpDX.Direct2D1;
using TerminalVelocity.Preferences;

namespace TerminalVelocity.Direct2D
{
    public class BrushProvider
    {
        public Configurable<Brush> TerminalColor0 { get; }
        public Configurable<Brush> TerminalColor1 { get; }

        private readonly DeviceContext _deviceContext;

        public BrushProvider(
            DeviceContext deviceContext,
            TerminalConfiguration terminalConfiguration)
        {
            _deviceContext = deviceContext ?? throw new ArgumentNullException(nameof(deviceContext));

            TerminalColor0 = terminalConfiguration.Color0.Select(SolidColorBrush);
            TerminalColor1 = terminalConfiguration.Color1.Select(SolidColorBrush);
        }

        private Brush SolidColorBrush(System.Drawing.Color color)
            => new SolidColorBrush(_deviceContext, color.ToSharpDX());
    }
}
