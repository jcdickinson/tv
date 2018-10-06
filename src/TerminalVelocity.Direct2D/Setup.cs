using System;
using SimpleInjector;

namespace TerminalVelocity.Direct2D
{
    public static class Setup
    {
        public static void SetupContainer(Container container)
        {
            var factory = WinApi.Windows.WindowFactory.Create(
                className: "TerminalVelocity",
                hBgBrush: IntPtr.Zero);

            //container.RegisterSingleton<BrushProvider>();
            //container.RegisterSingleton<FontProvider>();

            container.RegisterSingleton<RenderWindow>();
            container.RegisterInitializer<RenderWindow>(window => factory.CreateWindowEx(() => window));

            container.RegisterSingleton<DirectX.DirectX>();

            container.RegisterEventLoop<UIEventLoop>();
            container.RegisterEventLoop<RenderEventLoop>();
        }
    }
}
