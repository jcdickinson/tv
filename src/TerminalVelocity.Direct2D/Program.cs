using System;
using System.Composition.Hosting;
using System.Reflection;
using TerminalVelocity.Direct2D;

namespace TerminalVelocity.Direct2D
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            var configuration = new ContainerConfiguration()
                .WithAssembly(typeof(Program).Assembly);
            TerminalVelocity.Setup.SetupContainer(configuration);

            using (var container = configuration.CreateContainer())
            {
                ContainerProvider._compositionHost = container;
                var app = container.GetExport<Application>();
                return app.Run();
            }
        }
    }
}
