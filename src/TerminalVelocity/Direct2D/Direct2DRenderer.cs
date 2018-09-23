using System;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using TerminalVelocity.Terminal;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    public sealed class Direct2DRenderer : IDisposable
    {
        public RenderWindow RenderWindow { get; }
        public SharpDX.DirectWrite.Factory DirectWriteFactory => RenderWindow.DirectX.TextFactory;
        public SharpDX.Direct2D1.Device Direct2DDevice => RenderWindow.DirectX.D2D.Device;
        public SharpDX.Direct2D1.DeviceContext Direct2DContext => RenderWindow.DirectX.D2D.Context;
        public SharpDX.Direct2D1.Factory Direct2DFactory => Direct2DContext.Factory;
        public TextFormat TextFormat { get; }
        public Theme Theme { get; }
        public ChromeRenderer Chrome { get; }
        public Preferences Preferences { get; }
        public Grid Grid { get; }
        
        private long _i;

        public Direct2DRenderer()
        {
            RenderWindow = RenderWindow.Create(this);
            TextFormat = new TextFormat(DirectWriteFactory, "Fira Code", 20);
            Direct2DContext.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;
            Theme = new Theme();
            Preferences = new Preferences();
            Chrome = new ChromeRenderer(this);
            Grid = new Grid();
        }

        public int Run()
        {
            RenderWindow.Show();
            return new RealtimeEventLoop().Run(RenderWindow);
        }

        public void Render()
        {
            var rect = new RawRectangleF(10, 10, 100, 100);

            Direct2DContext.BeginDraw();
            Direct2DContext.Clear(new RawColor4(0, 0, 0, 0));
            Chrome.Render();

            Direct2DContext.EndDraw();
            RenderWindow.DirectX.D3D.SwapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
            
            Grid.Append(new Row((++_i).ToString() + " => "));
            RenderWindow.Invalidate();
        }

        public void Dispose()
        {
            using (RenderWindow)
            { }
        }
    }
}