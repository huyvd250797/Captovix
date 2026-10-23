using System.Windows;
using System.Windows.Media;
namespace Captovix;
public static partial class EditorRenderer
{
    private static Brush ShapeInk(string color,double opacity)
    {
        var brush=new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)){Opacity=opacity/100};brush.Freeze();return brush;
    }
    private static Pen ShapePen(Annotation a,double? thickness=null,bool dashed=true)
    {
        var pen=new Pen(ShapeInk(a.Color,a.StrokeOpacity),thickness??a.Stroke){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,DashCap=PenLineCap.Round,LineJoin=PenLineJoin.Round};
        if(dashed&&a.Dash!=StrokePattern.Solid)pen.DashStyle=new DashStyle(ShapeStudio.Dashes(a.Dash),0);
        pen.Freeze();return pen;
    }
    public static Geometry ShapeGeometry(Annotation a)
    {
        var rect=new Rect(a.Left,a.Top,Math.Max(.001,a.Width),Math.Max(.001,a.Height));
        if(a.Kind==AnnotationKind.Rectangle)return new RectangleGeometry(rect);
        if(a.Kind==AnnotationKind.RoundedRectangle){double radius=Math.Min(a.CornerRadius,Math.Min(rect.Width,rect.Height)/2);return new RectangleGeometry(rect,radius,radius);}
        if(a.Kind==AnnotationKind.Ellipse)return new EllipseGeometry(rect);
        var points=ShapeStudio.Vertices(a.Kind,a.StarPoints,a.StarInset);var geometry=new StreamGeometry();
        if(points.Length>0)using(var ctx=geometry.Open()){ctx.BeginFigure(new(rect.Left+points[0].X*rect.Width,rect.Top+points[0].Y*rect.Height),true,true);ctx.PolyLineTo(points.Skip(1).Select(p=>new Point(rect.Left+p.X*rect.Width,rect.Top+p.Y*rect.Height)).ToArray(),true,false);}
        geometry.Freeze();return geometry;
    }
    private static Geometry HeadGeometry(Annotation a,Point tip,Vector direction,double size)
    {
        var back=tip-direction*size;var side=new Vector(-direction.Y,direction.X)*size*.45;
        if(a.Tip==ArrowTip.Circle)return new EllipseGeometry(tip-direction*size/2,size/2,size/2);
        var geometry=new StreamGeometry();using(var ctx=geometry.Open()){
            if(a.Tip==ArrowTip.Open){ctx.BeginFigure(back+side,false,false);ctx.LineTo(tip,true,false);ctx.LineTo(back-side,true,false);}
            else{ctx.BeginFigure(tip,true,true);if(a.Tip==ArrowTip.Diamond){ctx.LineTo(tip-direction*size/2+side,true,false);ctx.LineTo(back,true,false);ctx.LineTo(tip-direction*size/2-side,true,false);}else{ctx.LineTo(back+side,true,false);ctx.LineTo(back-side,true,false);}}
        }geometry.Freeze();return geometry;
    }
    private static IEnumerable<Geometry> ArrowHeads(Annotation a)
    {
        if(a.Tip==ArrowTip.None)yield break;
        var start=new Point(a.X1,a.Y1);var end=new Point(a.X2,a.Y2);var d=end-start;if(d.Length<.01)yield break;double head=ShapeStudio.HeadSize(a,d.Length);d.Normalize();
        yield return HeadGeometry(a,end,d,head);if(a.DoubleArrow)yield return HeadGeometry(a,start,-d,head);
    }
    private static void DrawStrokeShape(DrawingContext dc,Annotation a)
    {
        var pen=a.StrokeEnabled&&a.StrokeOpacity>0?ShapePen(a):null;
        if(ShapeStudio.IsClosed(a.Kind)){dc.DrawGeometry(a.FillEnabled&&a.FillOpacity>0?ShapeInk(a.FillColor,a.FillOpacity):null,pen,ShapeGeometry(a));return;}
        if(pen==null)return;
        var p1=new Point(a.X1,a.Y1);var p2=new Point(a.X2,a.Y2);
        if(a.Kind==AnnotationKind.Line||a.Tip==ArrowTip.None){dc.DrawLine(pen,p1,p2);return;}
        var d=p2-p1;double length=d.Length;if(length<.01)return;d.Normalize();
        double head=ShapeStudio.HeadSize(a,length);double trim=a.Tip==ArrowTip.Open?0:Math.Max(0,head-a.Stroke);
        dc.DrawLine(pen,p1+(a.DoubleArrow?d*trim:new Vector()),p2-d*trim);
        foreach(var g in ArrowHeads(a))dc.DrawGeometry(a.Tip==ArrowTip.Open?null:ShapeInk(a.Color,a.StrokeOpacity),a.Tip==ArrowTip.Open?ShapePen(a,dashed:false):null,g);
    }
    private static bool HitStrokeShape(Annotation a,Point p,double tolerance)
    {
        if(ShapeStudio.IsClosed(a.Kind)){
            var geometry=ShapeGeometry(a);
            return a.FillEnabled&&a.FillOpacity>0&&geometry.FillContains(p)||geometry.StrokeContains(new Pen(Brushes.Black,a.Stroke+2*tolerance){LineJoin=PenLineJoin.Round},p);
        }
        if(Distance(p,new(a.X1,a.Y1),new(a.X2,a.Y2))<=tolerance+a.Stroke/2)return true;
        return a.Kind==AnnotationKind.Arrow&&ArrowHeads(a).Any(g=>a.Tip!=ArrowTip.Open&&g.FillContains(p)||g.StrokeContains(new Pen(Brushes.Black,a.Stroke+2*tolerance),p));
    }
}
