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

        [ImportingConstructor]
        public RenderWindowProvider(
            [Import] CompositionHost host,
            [Import] Dx11Component directX,
            [Import] IConstructionParams constructionParams)
        {
            _factory = WindowFactory.Create(
                className: "TerminalVelocity",
                hBgBrush: IntPtr.Zero);
            _host = host;
            _directX = directX;
            _constructionParams = constructionParams;
        }

        private RenderWindow CreateWindow()
        {
            var window = new RenderWindow(_directX);
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