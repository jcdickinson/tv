using System;
using System.Text;

namespace TerminalVelocity.VT
{
    public readonly ref struct VTOscDispatchAction
    {
        public ReadOnlySpan<ReadOnlyMemory<byte>> Parameters { get; }

        public VTOscDispatchAction(
            ReadOnlySpan<ReadOnlyMemory<byte>> parameters)
        {
            Parameters = parameters;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("OSC Dispatch ");

            for (var i = 0; i < Parameters.Length; i++)
            {
                if (i > 0) sb.Append("; ");

                var parameter = Parameters[i].Span;
                for (var j = 0; j < parameter.Length; j++)
                {
                    if (j > 0) sb.Append(", ");
                    sb.Append(parameter[j].ToString("x2"));
                }
            }
            return sb.ToString();
        }
    }
}