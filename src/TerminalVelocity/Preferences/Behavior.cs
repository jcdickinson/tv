using System.Composition;

namespace TerminalVelocity.Preferences
{
    [Shared]
    public class Behavior
    {
        public const string DisplayFpsContract = "Fps.Behavior.TerminalVelocity";

        [Export(DisplayFpsContract)]
        public Configurable<bool> DisplayFps { get; } 

        public Behavior()
        {
            DisplayFps = true;
        }
    }
}