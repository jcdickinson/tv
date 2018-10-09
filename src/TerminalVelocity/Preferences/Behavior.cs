/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

namespace TerminalVelocity.Preferences
{
    public class Behavior
    {
        public const string DisplayFpsContract = "Fps.Behavior.TerminalVelocity";

        public Configurable<bool> DisplayFps { get; }

        public Behavior() => DisplayFps = true;
    }
}
