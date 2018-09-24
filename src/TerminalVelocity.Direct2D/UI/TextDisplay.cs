using System;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using WinApi.User32;

namespace TerminalVelocity.Direct2D.UI
{
    // internal struct TextDisplay : IDisposable
    // {
    //     private readonly Direct2DRenderer _renderer;
    //     private RectangleF _frame;

    //     public TextDisplay(Direct2DRenderer renderer)
    //     {
    //         _renderer = renderer;
    //         _frame = default;
    //     }

    //     public void Dispose()
    //     {

    //     }

    //     public void Layout(RectangleF container)
    //     {
    //         _frame = container;
    //     }

    //     public void HitTest(ref HitTestResult result, Point point)
    //     {
    //         if (_frame.Contains(point))
    //             result.Region = WinApi.User32.HitTestResult.HTCLIENT;
    //     }

    //     internal bool Event<T>(ref T evt)
    //         where T : struct
    //     {
    //         return false;
    //     }

    //     public void Render()
    //     {
    //         var context = _renderer.Direct2DContext;
    //         var dpiHeight = _renderer.Direct2DFactory.DesktopDpi.Height / 96.0f;
    //         context.Transform = Matrix3x2.Identity;

    //         var fc = _renderer.DirectWriteFactory.GetSystemFontCollection(false);
    //         fc.FindFamilyName("Fira Code", out var index);
    //         var family = fc.GetFontFamily(index);
    //         var list = family.GetMatchingFonts(FontWeight.Regular, FontStretch.Normal, FontStyle.Normal);
    //         var font = list.GetFont(0);
    //         var face = new FontFace(font);

    //         using (var background = new SolidColorBrush(context, _renderer.Theme.TerminalBackground))
    //         using (var foreground = new SolidColorBrush(context, _renderer.Theme.Color1))
    //         using (var tf = new TextFormat(_renderer.DirectWriteFactory, _renderer.Theme.Font, 16))
    //         using (var ta = new TextAnalyzer(_renderer.DirectWriteFactory))
    //         {
    //             using (var num = new NumberSubstitution(_renderer.DirectWriteFactory, NumberSubstitutionMethod.None, "en-us", true))
    //             {
    //                 context.FillRectangle(_frame, background);
    //                 var origin = _frame.TopLeft;
                    
    //                 foreach (var row in _renderer.Grid)
    //                 {
    //                     var cluster = new short[row.Text.Length];
    //                     var shaping = new ShapingTextProperties[row.Text.Length];
    //                     var glyphs = new short[row.Text.Length];
    //                     var gshaping = new ShapingGlyphProperties[row.Text.Length];


    //                     try
    //                     {
    //                     ta.GetGlyphs(row.Text, row.Text.Length, face, false, false, new ScriptAnalysis(), "en-us", num, null, null, 100, cluster, shaping, glyphs, gshaping, out var count);

    //                     using (var run = new GlyphRun())
    //                     {
    //                         run.FontFace = face;
    //                         run.FontSize = 16;
    //                         run.Indices = glyphs;
    //                         run.Advances = new float[] { 10, 10, 10, 10, 10 };
                            
    //                         context.DrawGlyphRun(origin, run, foreground, MeasuringMode.Natural);
    //                     }
    //                     }
    //                     catch{}


    //                     // using (var layout = new TextLayout(_renderer.DirectWriteFactory, row.Text, tf, _frame.Width, _frame.Height))
    //                     // {
    //                     //     context.DrawTextLayout(origin, layout, foreground);
    //                     //     origin.Y += layout.Metrics.Height;
    //                     // }
    //                 }
    //             }
    //         }
    //     }
    // }
}