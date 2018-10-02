using System;
using System.Text;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTOscDispatchAction
    {
        private readonly ReadOnlySpan<ReadOnlyMemory<byte>> _parameters;

        public int Length => _parameters.Length;

        public ReadOnlySpan<byte> this[int index] => _parameters[index].Span;

        public VTIgnore Ignored { get; }

        public VTOscDispatchAction(
            ReadOnlySpan<ReadOnlyMemory<byte>> parameters,
            VTIgnore ignored)
        {
            _parameters = parameters;
            Ignored = ignored;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("OSC Dispatch");

            for (var i = 0; i < _parameters.Length; i++)
            {
                sb.Append(i== 0 ? " " : "; ");

                var parameter = _parameters[i].Span;
                for (var j = 0; j < parameter.Length; j++)
                {
                    if (j > 0) sb.Append(", ");
                    sb.Append(parameter[j].ToString("x2"));
                }
            }

            if (Ignored.HasFlag(VTIgnore.Parameters))
                sb.Append(_parameters.Length > 0 ? "; ignored" : " ignored");

            return sb.ToString();
        }
    }
}