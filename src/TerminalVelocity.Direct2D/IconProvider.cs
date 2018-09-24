using System.Composition;
using SharpDX;
using SharpDX.Direct2D1;

namespace TerminalVelocity.Direct2D
{
    [Shared]
    public class IconProvider
    {
        // TODO: SVG
        
        public const string MinButtonContract = BrushProvider.ChromeMinButtonContract;
        public const string MaxButtonContract = BrushProvider.ChromeMaxButtonContract;
        public const string RestoreButtonContract = BrushProvider.ChromeRestoreButtonContract;
        public const string CloseButtonContract = BrushProvider.ChromeCloseButtonContract;
        public const string LogoContract = BrushProvider.LogoContract;

        [Export(MinButtonContract)]
        public Geometry MinButton { get; }
        [Export(MaxButtonContract)]
        public Geometry MaxButton { get; }
        [Export(RestoreButtonContract)]
        public Geometry RestoreButton { get; }
        [Export(CloseButtonContract)]
        public Geometry CloseButton { get; }
        [Export(LogoContract)]
        public Geometry Logo { get; }

        [ImportingConstructor]
        public IconProvider(
            [Import] SharpDX.Direct2D1.Factory factory)
        {
            MinButton = CreateMinButton(factory);
            MaxButton = CreateMaxButton(factory);
            RestoreButton = CreateRestoreButton(factory);
            CloseButton = CreateCloseButton(factory);
            Logo = CreateLogo(factory);
        }

        private static Geometry CreateMinButton(SharpDX.Direct2D1.Factory factory)
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

        private static Geometry CreateMaxButton(SharpDX.Direct2D1.Factory factory)
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

        private static Geometry CreateRestoreButton(SharpDX.Direct2D1.Factory factory)
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
        
        private static Geometry CreateCloseButton(SharpDX.Direct2D1.Factory factory)
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
        
        private static Geometry CreateLogo(SharpDX.Direct2D1.Factory factory)
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