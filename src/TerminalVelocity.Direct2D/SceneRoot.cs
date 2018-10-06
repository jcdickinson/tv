using SharpDX;
using TerminalVelocity.Direct2D.UI;

namespace TerminalVelocity.Direct2D
{
    public sealed class SceneRoot
    {
        private GridView _grid;

        public SceneRoot(GridView grid) => _grid = grid;

        public void OnRender() => _grid.Render();

        public void OnLayout(in RectangleF rectangle) => _grid.Layout(rectangle);
    }
}
