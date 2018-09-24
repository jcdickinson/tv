using System;
using System.Composition;
using System.Composition.Hosting;
using SharpDX.Direct2D1;
using TerminalVelocity.Preferences;
using WinApi.DxUtils.Component;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    public sealed class RenderWindowProvider
    {
        private readonly CompositionHost _host;
        private readonly WindowFactory _factory;
        private readonly Dx11Component _directX;
        private readonly IConstructionParams _constructionParams;
        private readonly Configurable<System.Drawing.Size> _windowPadding;

        [ImportingConstructor]
        public RenderWindowProvider(
            [Import] CompositionHost host,
            [Import] Dx11Component directX,
            [Import] IConstructionParams constructionParams,
            [Import(WindowsMetricsProvider.WindowPaddingContract)] Configurable<System.Drawing.Size> windowPadding)
        {
            _factory = WindowFactory.Create(
                className: "TerminalVelocity",
                hBgBrush: IntPtr.Zero);
            _host = host;
            _directX = directX;
            _constructionParams = constructionParams;
            _windowPadding = windowPadding;
        }

        private RenderWindow CreateWindow()
        {
            var window = new RenderWindow(_directX, _windowPadding);
            _host.SatisfyImports(window);
            return window;
        }

        [Export]
        public RenderWindow RenderWindow
        {
            get
            {
                return _factory.CreateWindow(CreateWindow, constructionParams: _constructionParams);
            }
        } 
    }
}