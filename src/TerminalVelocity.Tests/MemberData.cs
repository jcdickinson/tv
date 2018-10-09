/* Copyright (c) Jonathan Dickinson and contributors. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for details.
*/

using System.Collections.Generic;

namespace TerminalVelocity
{
    public sealed class MemberData : List<object[]>
    {
        public new void Add(params object[] values)
            => base.Add(values);
    }
}
