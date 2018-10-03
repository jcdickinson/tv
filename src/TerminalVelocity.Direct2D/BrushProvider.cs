using System;
using System.Composition;
using SharpDX;
using SharpDX.Direct2D1;
using TerminalVelocity.Preferences;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    public class BrushProvider
    {
        public const string TerminalColor0Contract = TerminalTheme.Color0Contract;
        public const string TerminalColor1Contract = TerminalTheme.Color1Contract;

        public Configurable<Brush> Logo { get; }
        [Export(TerminalColor0Contract)]
        public Configurable<Brush> TerminalColor0 { get; }
        [Export(TerminalColor1Contract)]
        public Configurable<Brush> TerminalColor1 { get; }

        private readonly DeviceContext _deviceContext;

        [ImportingConstructor]
        public BrushProvider(
            [Import(TerminalTheme.Color0Contract)] Configurable<System.Drawing.Color> terminalColor0,
            [Import(TerminalTheme.Color1Contract)] Configurable<System.Drawing.Color> terminalColor1,
            [Import] DeviceContext deviceContext)
        {
            _deviceContext = deviceContext ?? throw new ArgumentNullException(nameof(deviceContext));

            TerminalColor0 = terminalColor0.Select(SolidColorBrush);
            TerminalColor1 = terminalColor1.Select(SolidColorBrush);
        }

        private Brush SolidColorBrush(System.Drawing.Color color)
            => new SolidColorBrush(_deviceContext, color.ToSharpDX());
    }
}