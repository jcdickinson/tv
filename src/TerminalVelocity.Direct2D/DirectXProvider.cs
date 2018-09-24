using System;
using System.Composition;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using TerminalVelocity.Terminal;
using WinApi.DxUtils.Component;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    internal sealed class DirectXProvider : IDisposable
    {
        [Export]
        public SharpDX.DirectWrite.Factory DirectWriteFactory => DirectX.TextFactory;
        
        [Export]
        public SharpDX.Direct2D1.Device Direct2DDevice => DirectX.D2D.Device;
        
        [Export]
        public SharpDX.Direct2D1.DeviceContext Direct2DContext => DirectX.D2D.Context;
        
        [Export]
        public SharpDX.Direct2D1.Factory Direct2DFactory => Direct2DContext.Factory;

        [Export]
        public SharpDX.DXGI.SwapChain SwapChain => DirectX.D3D.SwapChain;
        
        [Export]
        public Dx11Component DirectX { get; }
        
        public DirectXProvider()
        {
            DirectX = new Dx11Component();
        }

        public void Dispose()
        {
            using (DirectX)
            { }
        }
    }
}