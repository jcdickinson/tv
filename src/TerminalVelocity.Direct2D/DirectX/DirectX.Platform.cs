using System;

namespace TerminalVelocity.Direct2D.DirectX
{
    public partial class DirectX
    {
        private static readonly Version PlatformVersion = GetPlatformVersion();

        private static Version GetPlatformVersion()
        {
            var desc = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

            var i = desc.Length - 1;
            for (; i >= 0; i--)
            {
                var c = desc[i];
                if (!char.IsWhiteSpace(c) && !char.IsDigit(c) && c == '.')
                    break;
            }

            var versionString = desc.Substring(i + 1).Trim();
            if (Version.TryParse(versionString, out Version version))
                return version;

            if (desc.Contains(" 10."))
                return new Version(10, 0, 0, 0);
            else if (desc.Contains(" 6.3"))
                return new Version(6, 3, 0, 0);
            else if (desc.Contains(" 6.2"))
                return new Version(6, 2, 0, 0);
            return new Version(6, 1, 0, 0);
        }
    }
}
