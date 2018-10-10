/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;
using SimpleInjector;
using TerminalVelocity.Renderer;

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

            container.RegisterSingleton<DirectX.Surface>();
            container.RegisterAlternateInterface<ISurface, DirectX.Surface>();

            container.RegisterEventLoop<UIEventLoop>();

            container.RegisterEventLoop<RenderEventLoop, DirectX.DirectXRenderEventLoop>();
            container.RegisterPlugin<WinPty.WinPtyPlugin>();
        }
    }
}
