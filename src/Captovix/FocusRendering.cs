using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public static partial class EditorRenderer
{
    public static Geometry RegionGeometry(Annotation a,bool world=false)
    {
        var rect=new Rect(a.Left,a.Top,Math.Max(1,a.Width),Math.Max(1,a.Height));
        Geometry geometry=a.RegionShape==RegionShape.Ellipse?new EllipseGeometry(rect):new RectangleGeometry(rect);
        if(world&&Math.Abs(a.Rotation)>1e-9)geometry.Transform=ObjectTransform(a);return geometry;
    }
    private static void DrawSpotlights(DrawingContext dc,EditorDocument doc)
    {
        var lights=doc.Annotations.Where(a=>a.Visible&&a.Kind==AnnotationKind.Spotlight&&a.Width>0&&a.Height>0).ToArray();if(lights.Length==0)return;
        // All visible holes remain bright. The topmost spotlight supplies the shared shade.
        var holes=new GeometryGroup{FillRule=FillRule.Nonzero};foreach(var a in lights)holes.Children.Add(RegionGeometry(a,true));
        var v=doc.Viewport;var outside=new CombinedGeometry(GeometryCombineMode.Exclude,new RectangleGeometry(new(v.X,v.Y,v.Width,v.Height)),holes);
        var style=lights[^1];dc.DrawGeometry(ShapeInk(style.Color,style.FocusOpacity),null,outside);
    }
    private static DrawingGroup ProtectedSource(BitmapSource source,EditorDocument doc,IReadOnlySet<string>? drafts,PrivacySampling sampling)
    {
        var group=new DrawingGroup();using(var dc=group.Open()){
            var v=doc.Viewport;dc.PushClip(new RectangleGeometry(new(v.X,v.Y,v.Width,v.Height)));dc.DrawImage(source,new(0,0,doc.OriginalWidth,doc.OriginalHeight));
            foreach(var a in doc.Annotations.Where(a=>a.Visible&&a.IsRedaction).OrderBy(a=>a.Kind==AnnotationKind.Cover))RedactionRenderer.Draw(dc,source,a,drafts?.Contains(a.Id)==true,sampling);
            dc.Pop();
        }group.Freeze();return group;
    }
    private static void DrawMagnifier(DrawingContext dc,DrawingGroup protectedSource,Annotation a)
    {
        if(a.Width<=0||a.Height<=0)return;var geometry=RegionGeometry(a,true);var sample=FocusStudio.LensSample(a);
        dc.PushClip(geometry);dc.DrawGeometry(Brushes.White,null,geometry);
        dc.PushTransform(new MatrixTransform(a.Magnification,0,0,a.Magnification,a.Left-sample.X*a.Magnification,a.Top-sample.Y*a.Magnification));dc.DrawDrawing(protectedSource);dc.Pop();dc.Pop();
        if(a.StrokeEnabled)dc.DrawGeometry(null,ShapePen(a),geometry);
    }
}
