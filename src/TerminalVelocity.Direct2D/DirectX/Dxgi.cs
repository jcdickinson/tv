/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using SharpDX;
using SharpDX.DXGI;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class Surface
    {
        private struct Dxgi : IDisposable
        {
            public Adapter Adapter;
            public Factory2 Factory;
            public Device2 Device;
            public SwapChain1 SwapChain;

            public void Initialize(Device2 device, IntPtr hwnd, CompositionType compositionType, Size2 size)
            {
                if (Device != null) Dispose();

                Device = device;
                Adapter = Device.GetParent<Adapter>();
                Factory = Adapter.GetParent<Factory2>();

                var swapChainDescription = new SwapChainDescription1
                {
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.RenderTargetOutput,
                    BufferCount = 2,
                    SwapEffect = GetBestSwapEffectForPlatform(),
                    Scaling = Scaling.Stretch,
                    Format = Format.B8G8R8A8_UNorm,
                    AlphaMode = compositionType.HasFlag(CompositionType.Composited)
                        ? AlphaMode.Premultiplied
                        : AlphaMode.Ignore,
                    Width = size.Width,
                    Height = size.Height
                };

                SwapChain = compositionType.HasFlag(CompositionType.Composited)
                    ? new SwapChain1(Factory, Device, ref swapChainDescription)
                    : new SwapChain1(Factory, Device, hwnd, ref swapChainDescription);
            }

            public void Resize(Size2 size)
            {
                SwapChain?.ResizeBuffers(0, size.Width, size.Height, Format.Unknown, SwapChainFlags.None);
            }

            public void Dispose()
            {
                Disposable.Dispose(ref Factory);
                Disposable.Dispose(ref Device);
                Disposable.Dispose(ref SwapChain);
            }

            private static SwapEffect GetBestSwapEffectForPlatform()
            {
                Version version = PlatformVersion;
                if (version.Major > 6)
                    return SwapEffect.FlipDiscard; // Win 10+
                if ((version.Major > 5) && (version.Minor > 1))
                    return SwapEffect.FlipSequential; // 6.2+ - Win 8+
                return SwapEffect.Discard;
            }
        }
    }
}
