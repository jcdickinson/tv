using System;
using System.Diagnostics;
using System.Threading;

namespace TerminalVelocity.Terminal
{
    public readonly struct TerminalIdentifier : IEquatable<TerminalIdentifier>
    {
        private static long _identifierFactory;
        private readonly ulong _identifier;
        public bool IsEmpty => _identifier == 0;

        private TerminalIdentifier(ulong identifier) => _identifier = identifier;

        public static TerminalIdentifier Create() => new TerminalIdentifier((ulong)Interlocked.Increment(ref _identifierFactory));

        public override string ToString() => IsEmpty ? "Empty" : _identifier.ToString();

        public override bool Equals(object obj) => obj is TerminalIdentifier o && Equals(o);

        public bool Equals(TerminalIdentifier other) => _identifier == other._identifier;

        public override int GetHashCode() => HashCode.Combine(_identifier);

        public static bool operator ==(TerminalIdentifier terminal1, TerminalIdentifier terminal2) => terminal1.Equals(terminal2);

        public static bool operator !=(TerminalIdentifier terminal1, TerminalIdentifier terminal2) => !(terminal1 == terminal2);
    }
}
