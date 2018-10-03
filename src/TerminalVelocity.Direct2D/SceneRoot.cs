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
        private GridView _grid;

        [ImportingConstructor]
        public SceneRoot(
            [Import] GridView chrome
        )
        {
            _grid = chrome;
        }

        public void OnRender() => _grid.Render();

        public void OnLayout(in RectangleF rectangle) => _grid.Layout(rectangle);
    }
}