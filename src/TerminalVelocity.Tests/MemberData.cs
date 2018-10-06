using System.Collections.Generic;

namespace TerminalVelocity
{
    public sealed class MemberData : List<object[]>
    {
        public new void Add(params object[] values)
            => base.Add(values);
    }
}
