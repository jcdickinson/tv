namespace TerminalVelocity.Preferences
{
    public class Behavior
    {
        public const string DisplayFpsContract = "Fps.Behavior.TerminalVelocity";

        public Configurable<bool> DisplayFps { get; }

        public Behavior() => DisplayFps = true;
    }
}
