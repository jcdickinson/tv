using System;
using NetCoreEx.Geometry;
using SharpDX;
using SharpDX.DirectComposition;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class Surface
    {
        private struct Compositor
        {
            public readonly Ref<Dxgi> Dxgi;
            public CompositionType CompositionType;
            public ComObject Device;
            public Target Target;
            public Visual Visual;

            public Compositor(Ref<Dxgi> dxgi)
            {
                Dxgi = dxgi;
                CompositionType = default;
                Device = default;
                Target = default;
                Visual = default;
            }

            public void Create(IntPtr hwnd, CompositionType compositionType)
            {
                CompositionType = compositionType;
                Device = CompositionType.HasFlag(CompositionType.Native)
                    ? new DesktopDevice(Dxgi().Device)
                    : (ComObject)new Device(Dxgi().Device);

                if (CompositionType.HasFlag(CompositionType.Native))
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
                if (CompositionType.HasFlag(CompositionType.Native))
                    ((DesktopDevice)Device).Commit();
                else
                    ((Device)Device).Commit();
            }
        }
    }
}
