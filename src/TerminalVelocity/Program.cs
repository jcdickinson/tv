using System;
using TerminalVelocity.Direct2D;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity
{
    class Program
    {
        static int Main(string[] args)
        {
            using (var renderer = new Direct2DRenderer())
            {
                return renderer.Run();
            }
        }
    }
}
