﻿/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class Surface
    {
        private struct D3D : IDisposable
        {
            public readonly Ref<Dxgi> Dxgi;
            public Device1 Device;
            public DeviceContext1 Context;
            public RenderTargetView RenderTargetView;

            public D3D(Ref<Dxgi> dxgi)
            {
                Dxgi = dxgi;
                Device = default;
                Context = default;
                RenderTargetView = default;
            }

            public void CreateFactory()
            {
                if (Device != null) Dispose();
            }

            public void Create(
                IntPtr hwnd, 
                Size2 size, 
                CompositionType compositionType)
            {
                DebugSelect(DeviceCreationFlags.Debug, DeviceCreationFlags.None,
                    out DeviceCreationFlags creationFlags);
                creationFlags |= DeviceCreationFlags.BgraSupport | DeviceCreationFlags.SingleThreaded;
                
                Device = CreateDevice(creationFlags);                
                Context = Device.ImmediateContext1;

                Dxgi().Initialize(Device.QueryInterface<DXGI.Device2>(), hwnd, compositionType, size);

                Connect();
            }

            private Device1 CreateDevice(DeviceCreationFlags creationFlags)
            {
                try
                {
                    return new Device(DriverType.Hardware, creationFlags)
                        .QueryInterface<Device1>();
                }
                catch
                {
                    return new Device(DriverType.Warp, creationFlags)
                        .QueryInterface<Device1>();
                }
            }

            public void Connect()
            {
                if (Device == null) return;
                using (Texture2D backBuffer = Dxgi().SwapChain.GetBackBuffer<Texture2D>(0))
                {
                    RenderTargetView = new RenderTargetView(Device, backBuffer);
                }
                Context.OutputMerger.SetRenderTargets(RenderTargetView);
            }

            public void Disconnect()
            {
                if (Context == null) return;
                Context.OutputMerger.SetRenderTargets((RenderTargetView)null);
                Disposable.Dispose(ref RenderTargetView);
            }

            public void Dispose()
            {
                Disconnect();
                Disposable.Dispose(ref Device);
            }
        }
    }
}
