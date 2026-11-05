using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public static partial class EditorRenderer
{
    // The leaf group contains exactly a control and its caption(s). A backing
    // or layout group is never treated as the caption's visual parent.
    static Annotation? CaptionControl(Annotation a,EditorDocument doc)=>a.Mockup is {Type:MockupKind.Label or MockupKind.Title}&&a.GroupId!=null?doc.Annotations.FirstOrDefault(b=>b.Id!=a.Id&&b.GroupId==a.GroupId&&b.Mockup is {Type:MockupKind.Button or MockupKind.Input}&&b.Mockup.SourceBounds.Intersect(a.Mockup.SourceBounds)==a.Mockup.SourceBounds):null;
    static readonly ConditionalWeakTable<Annotation,CaptionMask> CaptionMasks=new();
    sealed class CaptionMask{public CaptionMask(){}public object? Key;public BitmapSource? Bitmap;}
    static bool DrawOriginalCaption(DrawingContext dc,Annotation a,Annotation root,EditorDocument doc,bool repaint){
        if(a.Mockup?.Type is not(MockupKind.Label or MockupKind.Title))return false;var control=CaptionControl(a,doc);bool cleared=(root.Mockup!.RasterClears??[]).Any(h=>h.ElementId!=a.Id&&h.Bounds.Intersect(a.Mockup.SourceBounds)==a.Mockup.SourceBounds);if(control==null&&!cleared||control!=null&&control.Mockup!.PreserveRaster&&!repaint&&!cleared)return false;
        var piece=a.Mockup!.SourceBounds;var source=root.Mockup!.RasterImageBounds??root.Mockup.SourceBounds;var bitmap=ScanBitmap(root.PixelPng!);var cache=CaptionMasks.GetOrCreateValue(a);object key=(root.PixelPng!,piece,a.Mockup.Background,a.TextColor??a.Color);
        if(!Equals(cache.Key,key)){
            int w=piece.Width,h=piece.Height;if((long)w*h>1_000_000)return false;
            var crop=new CroppedBitmap(bitmap,new Int32Rect(piece.X-source.X,piece.Y-source.Y,w,h));var pixels=ImageTransfer.Bgra(crop);var bg=(Color)ColorConverter.ConvertFromString(a.Mockup.Background);var fg=(Color)ColorConverter.ConvertFromString(a.TextColor??a.Color);double dr=fg.R-bg.R,dg=fg.G-bg.G,db=fg.B-bg.B,den=dr*dr+dg*dg+db*db;
            // Reconstruct alpha against the old flat control fill. Glyph shape,
            // spacing and antialiasing stay raster; recolouring the body does
            // not substitute an estimated typeface for an untouched caption.
            for(int i=0;i<pixels.Length;i+=4){double alpha=den<100?0:Math.Clamp(((pixels[i+2]-bg.R)*dr+(pixels[i+1]-bg.G)*dg+(pixels[i]-bg.B)*db)/den,0,1);pixels[i]=(byte)(fg.B*alpha);pixels[i+1]=(byte)(fg.G*alpha);pixels[i+2]=(byte)(fg.R*alpha);pixels[i+3]=(byte)(255*alpha);}
            cache.Bitmap=BitmapSource.Create(w,h,96,96,PixelFormats.Pbgra32,null,pixels,w*4);cache.Bitmap.Freeze();cache.Key=key;
        }
        dc.DrawImage(cache.Bitmap,new Rect(a.Left,a.Top,a.Width,a.Height));return true;
    }
    static Annotation CaptionPaint(Annotation a,EditorDocument doc){var parent=CaptionControl(a,doc);if(parent==null||parent.Mockup?.PreserveRaster!=false)return a;var copy=a.Copy();copy.Mockup!.Background=parent.FillColor;return copy;}
}
