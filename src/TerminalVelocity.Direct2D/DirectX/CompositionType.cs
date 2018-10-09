/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class Surface
    {
        [Flags]
        private enum CompositionType : int
        {
            Default = -1,
            WindowTarget = 0,
            Composited = 1,
            Native = 2,
            NativeComposited = Composited | Native,
        }

        private static CompositionType GetCompositionType()
        {
            Version platformVersion = PlatformVersion;
            if (platformVersion.Major > 6) return CompositionType.NativeComposited;
            if (platformVersion.Major == 6)
            {
                if (platformVersion.Minor > 2) return CompositionType.NativeComposited;
                if (platformVersion.Minor > 1) return CompositionType.Composited;
            }
            return 0;
        }
    }
}
