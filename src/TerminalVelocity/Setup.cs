using System.Composition.Hosting;

namespace TerminalVelocity
{
    public static class Setup
    {
        public static void SetupContainer(ContainerConfiguration configuration)
        {
            configuration.WithAssembly(typeof(Setup).Assembly);
        }
    }
}