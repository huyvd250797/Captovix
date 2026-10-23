using System.Windows;
using System.Windows.Media;
namespace Captovix;
public static partial class EditorRenderer
{
    private static Geometry ConnectorGeometry(Annotation a)
    {
        var s=ConnectorStudio.Start(a);var e=ConnectorStudio.End(a);var c=ConnectorStudio.Control(a);var g=new StreamGeometry();
        using(var ctx=g.Open()){
            ctx.BeginFigure(new(s.X,s.Y),false,false);
            if(a.Route==ConnectorRoute.Curve)ctx.QuadraticBezierTo(new(c.X,c.Y),new(e.X,e.Y),true,false);
            else ctx.PolyLineTo(ConnectorStudio.Samples(a).Skip(1).Select(p=>new Point(p.X,p.Y)).ToArray(),true,false);
        }g.Freeze();return g;
    }
    private static IEnumerable<Geometry> ConnectorHeads(Annotation a)
    {
        if(a.Kind!=AnnotationKind.Arrow||a.Tip==ArrowTip.None)yield break;
        var p=ConnectorStudio.Samples(a);var distinct=p.Where((v,i)=>i==0||v!=p[i-1]).ToArray();if(distinct.Length<2)yield break;
        double length=0;for(int i=1;i<p.Length;i++)length+=new Vector(p[i].X-p[i-1].X,p[i].Y-p[i-1].Y).Length;
        var end=new Point(distinct[^1].X,distinct[^1].Y);var start=new Point(distinct[0].X,distinct[0].Y);
        var d=end-new Point(distinct[^2].X,distinct[^2].Y);d.Normalize();var head=ShapeStudio.HeadSize(a,length);yield return HeadGeometry(a,end,d,head);
        if(a.DoubleArrow){d=start-new Point(distinct[1].X,distinct[1].Y);d.Normalize();yield return HeadGeometry(a,start,d,head);}
    }
    private static void DrawConnector(DrawingContext dc,Annotation a)
    {
        if(!a.StrokeEnabled||a.StrokeOpacity<=0)return;
        dc.DrawGeometry(null,ShapePen(a),ConnectorGeometry(a));
        foreach(var head in ConnectorHeads(a))dc.DrawGeometry(a.Tip==ArrowTip.Open?null:ShapeInk(a.Color,a.StrokeOpacity),a.Tip==ArrowTip.Open?ShapePen(a,dashed:false):null,head);
    }
    private static bool HitConnector(Annotation a,Point p,double tolerance)=>ConnectorGeometry(a).StrokeContains(new Pen(Brushes.Black,a.Stroke+2*tolerance){LineJoin=PenLineJoin.Round},p)||ConnectorHeads(a).Any(g=>g.FillContains(p)||g.StrokeContains(new Pen(Brushes.Black,a.Stroke+2*tolerance),p));
}
