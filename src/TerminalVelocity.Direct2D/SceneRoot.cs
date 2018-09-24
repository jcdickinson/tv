using System;
using System.Composition;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using TerminalVelocity.Direct2D.Events;
using TerminalVelocity.Direct2D.UI;

namespace TerminalVelocity.Direct2D
{
    [Shared, Export]
    public class SceneRoot
    {
        private Chrome _chrome;

        [ImportingConstructor]
        public SceneRoot(
            [Import] Chrome chrome
        )
        {
            _chrome = chrome;
        }

        public void OnRender() => _chrome.Render();

        public void OnLayout(ref LayoutEvent e) => _chrome.Layout(in e.NewSize);
        
        public void OnHitTest(ref HitTestEvent e) => _chrome.HitTest(ref e);
    }
}