using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public static partial class EditorRenderer
{
    public static void DrawMockupPatch(DrawingContext dc,Annotation a)
    {
        if(!a.Visible||a.Mockup is not {EraseOriginal:true} style)return;
        var r=style.SourceBounds;dc.DrawRectangle(Ink(style.Background),null,new Rect(r.X,r.Y,r.Width,r.Height));
    }
    private static void DrawMockup(DrawingContext dc,Annotation a,bool suppressText=false)
    {
        if(a.Mockup==null)return;var rect=new Rect(a.Left,a.Top,a.Width,a.Height);
        // Label/title also get an opaque current-area background so old glyphs
        // never bleed through the replacement while editing in place.
        dc.DrawRectangle(Ink(a.Mockup.Background),null,rect);
        if(a.Mockup.Type is MockupKind.Button or MockupKind.Input or MockupKind.Block){var width=a.StrokeEnabled?a.Stroke:0;var body=rect;body.Inflate(-width/2,-width/2);if(body.Width>0&&body.Height>0)dc.DrawRoundedRectangle(Ink(a.FillColor),width>0?new Pen(Ink(a.Color),width):null,body,a.CornerRadius,a.CornerRadius);}
        DrawStyledText(dc,a,suppressText);
    }
    public static BitmapSource MockupSnapshot(BitmapSource original,EditorDocument document,PixelRect region)
    {
        var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){dc.PushTransform(new TranslateTransform(-region.X,-region.Y));Draw(dc,original,document);dc.Pop();}
        var result=new RenderTargetBitmap(region.Width,region.Height,96,96,PixelFormats.Pbgra32);result.Render(visual);return ImageTransfer.Detach(result);
    }
    public static BitmapSource MockupPreview(BitmapSource source,PixelRect sampled,Annotation draft)
    {
        var a=draft.Copy();a.Translate(-sampled.X,-sampled.Y);if(a.Mockup is {} m)m.SourceBounds=m.SourceBounds with{X=m.SourceBounds.X-sampled.X,Y=m.SourceBounds.Y-sampled.Y};
        double scale=Math.Min(3,Math.Min(900d/source.PixelWidth,480d/source.PixelHeight));
        var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){dc.PushTransform(new ScaleTransform(scale,scale));dc.DrawImage(source,new Rect(0,0,source.PixelWidth,source.PixelHeight));DrawMockupPatch(dc,a);DrawAnnotation(dc,a);dc.Pop();}
        var output=new RenderTargetBitmap(Math.Max(1,(int)Math.Ceiling(source.PixelWidth*scale)),Math.Max(1,(int)Math.Ceiling(source.PixelHeight*scale)),96,96,PixelFormats.Pbgra32);output.Render(visual);output.Freeze();return output;
    }
}
