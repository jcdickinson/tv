using System;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    public sealed class Direct2DRenderer : IDisposable
    {
        public RenderWindow RenderWindow { get; }
        public SharpDX.DirectWrite.Factory DirectWriteFactory => RenderWindow.DirectX.TextFactory;
        public SharpDX.Direct2D1.Factory1 Direct2DFactory => RenderWindow.DirectX.D2D.Factory1;
        public SharpDX.Direct2D1.Device Direct2DDevice => RenderWindow.DirectX.D2D.Device;
        public SharpDX.Direct2D1.DeviceContext Direct2DContext => RenderWindow.DirectX.D2D.Context;
        public TextFormat TextFormat { get; }
        public Theme Theme { get; }
        public ChromeRenderer Chrome { get; }

        public Direct2DRenderer()
        {
            RenderWindow = RenderWindow.Create(this);
            TextFormat = new TextFormat(DirectWriteFactory, "Fira Code", 20);
            Direct2DContext.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;
            Theme = new Theme();
            Chrome = new ChromeRenderer(this);
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
            Direct2DContext.Clear(new RawColor4(0, 0, 0, 1));
            Chrome.Render();

            using(var b = new SolidColorBrush(Direct2DContext, new RawColor4(1, 1, 1, 1)))
            {
                Direct2DContext.DrawText("Test => Text", TextFormat, rect, b);
            }

            Direct2DContext.EndDraw();
            RenderWindow.DirectX.D3D.SwapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
        }

        public void Dispose()
        {
            using (RenderWindow)
            { }
        }
    }
}