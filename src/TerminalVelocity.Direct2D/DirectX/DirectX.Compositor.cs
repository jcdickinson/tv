using System;
using NetCoreEx.Geometry;
using SharpDX;
using SharpDX.DirectComposition;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class DirectX
    {
        private struct Compositor
        {
            public readonly Ref<Dxgi> Dxgi;
            public DirectCompositionVariant Variant;
            public ComObject Device;
            public Target Target;
            public Visual Visual;

            public Compositor(Ref<Dxgi> dxgi)
            {
                Dxgi = dxgi;
                Variant = default;
                Device = default;
                Target = default;
                Visual = default;
            }

            public void Create(IntPtr hwnd, DirectCompositionVariant variant)
            {
                Variant = variant;
                Device = Variant.HasFlag(DirectCompositionVariant.Native)
                    ? new DesktopDevice(Dxgi().Device)
                    : (ComObject)new Device(Dxgi().Device);

                if (Variant.HasFlag(DirectCompositionVariant.Native))
                {
                    var device = (DesktopDevice)Device;
                    Target = Target.FromHwnd(device, hwnd, false);
                    Visual = new Visual2(device);
                }
                else
                {
                    var device = (Device)Device;
                    Target = Target.FromHwnd(device, hwnd, false);
                    Visual = new Visual(device);
                }

                Visual.Content = Dxgi().SwapChain;
                Target.Root = Visual;

                Commit();
            }

            public void Resize(Size size)
            {

            }

            public void Commit()
            {
                if (Variant.HasFlag(DirectCompositionVariant.Native))
                    ((DesktopDevice)Device).Commit();
                else
                    ((Device)Device).Commit();
            }
        }
    }
}
