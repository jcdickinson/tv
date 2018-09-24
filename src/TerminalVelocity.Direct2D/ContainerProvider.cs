using System.Composition;
using System.Composition.Hosting;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    public class ContainerProvider
    {
        internal static CompositionHost _compositionHost;
        [Export]
        public CompositionHost CompositionHost => _compositionHost;
    }
}