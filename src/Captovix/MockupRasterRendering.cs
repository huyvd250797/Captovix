using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public static partial class EditorRenderer
{
    static readonly ConditionalWeakTable<string,WeakReference<BitmapSource>> RasterSources=new();static readonly object RasterSourceGate=new();
    static BitmapSource ScanBitmap(string png){lock(RasterSourceGate){if(RasterSources.TryGetValue(png,out var weak)&&weak.TryGetTarget(out var source))return source;var decoded=RasterPixels.Decode(png);RasterSources.Remove(png);RasterSources.Add(png,new(decoded));return decoded;}}
    static Rect MapPiece(PixelRect part,PixelRect source,Annotation target)=>new(target.Left+(part.X-source.X)*target.Width/source.Width,target.Top+(part.Y-source.Y)*target.Height/source.Height,part.Width*target.Width/source.Width,part.Height*target.Height/source.Height);
    static bool SameRect(Annotation a,Rect r)=>Math.Abs(a.Left-r.X)<.001&&Math.Abs(a.Top-r.Y)<.001&&Math.Abs(a.Width-r.Width)<.001&&Math.Abs(a.Height-r.Height)<.001&&Math.Abs(a.Rotation)<.001;
    private static void DrawRasterMockup(DrawingContext dc,Annotation a,EditorDocument doc){
        var m=a.Mockup!;var root=m.RasterOwnerId is {} id?doc.Annotations.FirstOrDefault(x=>x.Id==id):a;var png=root?.PixelPng;if(png==null)return;
        var source=root!.Mockup!.RasterImageBounds??root.Mockup.SourceBounds;var piece=m.SourceBounds;var rootRect=MapPiece(piece,source,root);
        bool moved=m.RasterBacking?m.RasterDetached||!SameRect(a,new(source.X,source.Y,source.Width,source.Height)):m.RasterDetached||!root.Visible||!SameRect(a,rootRect);
        var holes=root.Mockup.RasterClears??[];bool repaint=!m.RasterBacking&&holes.Any(h=>h.Bounds.Intersect(piece)==piece)&&m.PreserveRaster;
        DrawObjectShadow(dc,a);dc.PushTransform(ObjectTransform(a));try{
            var dest=new Rect(a.Left,a.Top,a.Width,a.Height);dc.PushClip(new RectangleGeometry(dest));
            bool caption=DrawOriginalCaption(dc,a,root,doc,moved||repaint);
            if(!caption&&(moved||repaint)){var bitmap=ScanBitmap(png);double sx=a.Width/piece.Width,sy=a.Height/piece.Height;dc.DrawImage(bitmap,new Rect(a.Left-(piece.X-source.X)*sx,a.Top-(piece.Y-source.Y)*sy,source.Width*sx,source.Height*sy));}
            foreach(var h in holes){if(h.ElementId==a.Id||(long)h.Bounds.Width*h.Bounds.Height>=(long)piece.Width*piece.Height)continue;var intersection=h.Bounds.Intersect(piece);if(intersection.IsEmpty)continue;dc.DrawRectangle(Ink(h.Background),null,MapPiece(intersection,piece,a));}
            dc.Pop();
        }finally{dc.Pop();}
    }
}
