using System;
using System.Diagnostics;
using Xunit;

namespace TerminalVelocity
{
    public static class BufferAssert
    {
        [DebuggerNonUserCode]
        public static void Equal<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual)
        {
            var array1 = expected.ToArray();
            var array2 = actual.ToArray();

            try
            {
                Assert.Equal(array1, array2);
            }
            catch (Exception ex)
            {
                throw ex; // Nuke the stack trace.
            }
        }
    }
}