using System;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class DirectX
    {
        [Flags]
        private enum DirectCompositionVariant : int
        {
            Default = -1,
            WindowTarget = 0,
            Composited = 1,
            Native = 2,
            NativeComposited = Composited | Native,
        }

        private static DirectCompositionVariant GetDirectCompositionVariant()
        {
            Version platformVersion = PlatformVersion;
            if (platformVersion.Major > 6) return DirectCompositionVariant.NativeComposited;
            if (platformVersion.Major == 6)
            {
                if (platformVersion.Minor > 2) return DirectCompositionVariant.NativeComposited;
                if (platformVersion.Minor > 1) return DirectCompositionVariant.Composited;
            }
            return 0;
        }
    }
}
