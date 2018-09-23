using SharpDX;
using SharpDX.Direct2D1;

namespace TerminalVelocity.Direct2D.UI
{
    internal static class IconFactory
    {
        // TODO: SVG

        public static Geometry MinButton(SharpDX.Direct2D1.Factory factory)
        {
            var result = new PathGeometry(factory);
            using (var sink = result.Open())
            {
                sink.BeginFigure(new Vector2(0.0f, 0.4f), FigureBegin.Hollow);
                sink.AddLine(new Vector2(0.9f, 0.4f));
                sink.EndFigure(FigureEnd.Open);

                sink.Close();
            }
            
            return result;
        }

        public static Geometry MaxButton(SharpDX.Direct2D1.Factory factory)
        {
            var result = new PathGeometry(factory);
            using (var sink = result.Open())
            {
                sink.BeginFigure(new Vector2(0.0f, 0.0f), FigureBegin.Hollow);
                sink.AddLine(new Vector2(0.9f, 0.0f));
                sink.AddLine(new Vector2(0.9f, 0.9f));
                sink.AddLine(new Vector2(0.0f, 0.9f));
                sink.EndFigure(FigureEnd.Closed);

                sink.Close();
            }
            
            return result;
        }

        public static Geometry RestoreButton(SharpDX.Direct2D1.Factory factory)
        {
            var result = new PathGeometry(factory);
            using (var sink = result.Open())
            {
                sink.BeginFigure(new Vector2(0.0f, 0.2f), FigureBegin.Hollow);
                sink.AddLine(new Vector2(0.7f, 0.2f));
                sink.AddLine(new Vector2(0.7f, 0.9f));
                sink.AddLine(new Vector2(0.0f, 0.9f));
                sink.EndFigure(FigureEnd.Closed);

                sink.BeginFigure(new Vector2(0.2f, 0.2f), FigureBegin.Hollow);
                sink.AddLine(new Vector2(0.2f, 0.0f));
                sink.AddLine(new Vector2(0.9f, 0.0f));
                sink.AddLine(new Vector2(0.9f, 0.7f));
                sink.AddLine(new Vector2(0.7f, 0.7f));
                sink.EndFigure(FigureEnd.Open);

                sink.Close();
            }
            
            return result;
        }
        
        public static Geometry CloseButton(SharpDX.Direct2D1.Factory factory)
        {
            var result = new PathGeometry(factory);
            using (var sink = result.Open())
            {
                sink.BeginFigure(new Vector2(0.0f, 0.0f), FigureBegin.Hollow);
                sink.AddLine(new Vector2(0.9f, 0.9f));
                sink.EndFigure(FigureEnd.Open);

                sink.BeginFigure(new Vector2(0.0f, 0.9f), FigureBegin.Hollow);
                sink.AddLine(new Vector2(0.9f, 0.0f));
                sink.EndFigure(FigureEnd.Open);

                sink.Close();
            }

            return result;
        }
        
        public static Geometry AppIcon(SharpDX.Direct2D1.Factory factory)
        {
            var result = new PathGeometry(factory);
            using (var sink = result.Open())
            {
                sink.BeginFigure(new Vector2(10.0f, 0.862f), FigureBegin.Filled);
                sink.AddLine(new Vector2(7.759f, 0.069f));
                sink.AddLine(new Vector2(7.759f, 7.931f));
                sink.AddLine(new Vector2(0.0f, 7.586f));
                sink.AddLine(new Vector2(7.759f, 9.483f));
                sink.AddLine(new Vector2(10.0f, 8.448f));
                sink.EndFigure(FigureEnd.Closed);
                
                sink.BeginFigure(new Vector2(6.638f, 3.966f), FigureBegin.Filled);
                sink.AddLine(new Vector2(4.052f, 0.69f));
                sink.AddLine(new Vector2(3.362f, 1.034f));
                sink.AddLine(new Vector2(5.086f, 3.966f));
                sink.AddLine(new Vector2(3.362f, 6.207f));
                sink.AddLine(new Vector2(4.052f, 6.724f));
                sink.EndFigure(FigureEnd.Closed);

                sink.Close();
            }

            return result;
        }
    }
}