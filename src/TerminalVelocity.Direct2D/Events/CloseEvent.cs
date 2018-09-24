using System;
using NetCoreEx.Geometry;
using WinApi.User32;
using WinApi.Windows;

namespace TerminalVelocity.Direct2D.Events
{
    public struct CloseEvent
    {
        public const string ContractName = "Close.Events.Direct2D.TerminalVelocity";

        public bool IsHandled;
    }
}