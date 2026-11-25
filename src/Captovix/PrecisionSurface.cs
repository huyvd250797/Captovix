using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
namespace Captovix;
public sealed partial class EditorSurface
{
    public bool KeepAspectRatio {get;set;}=false;
    public Point? ContextSourcePoint {get;private set;}
    private string? _hoveredId;
    private double ScreenDistance(Point a,Point b){var l=Layout!;double x=a.X-b.X,y=a.Y-b.Y;return Math.Sqrt(Math.Pow(l.A*x+l.C*y,2)+Math.Pow(l.B*x+l.D*y,2))*Zoom;}
    public Annotation[] HitStack(Point source,bool includeLocked=false)
    {
        if(Session==null||Layout is not {} l)return [];var p=l.ToOutput(source.X,source.Y);if(p.X<0||p.Y<0||p.X>l.Width||p.Y>l.Height)return [];
        var candidates=includeLocked?Session.Document.Annotations.Where(a=>a.Visible):ObjectControl.CanvasCandidates(Session.Document);
        double tolerance=5/Zoom,sourceTolerance=5/InteractionScale;
        return candidates.Select((a,i)=>(Object:a,Index:i)).OrderByDescending(x=>AnnotationEditing.Tier(x.Object))
            .ThenByDescending(x=>x.Index)
            .Where(x=>HitCandidate(x.Object,source,p,l,tolerance,sourceTolerance,includeLocked))
            .Select(x=>x.Object).ToArray();
    }
    private Annotation? HitTop(Point source)
    {
        if(Session==null||Layout is not {} layout)return null;
        var output=layout.ToOutput(source.X,source.Y);
        if(output.X<0||output.Y<0||output.X>layout.Width||output.Y>layout.Height)return null;
        var candidates=ObjectControl.CanvasCandidates(Session.Document);
        double tolerance=5/Zoom,sourceTolerance=5/InteractionScale;
        Annotation? hit=null;
        int tier=int.MinValue;
        for(int i=candidates.Length-1;i>=0;i--){
            var item=candidates[i];
            int candidateTier=AnnotationEditing.Tier(item);
            if(candidateTier<=tier||!HitCandidate(item,source,output,layout,tolerance,sourceTolerance,false))continue;
            hit=item;
            tier=candidateTier;
            if(tier==2)break;
        }
        return hit;
    }
    private static bool HitCandidate(Annotation item,Point source,ImagePoint output,ImageLayout layout,
        double tolerance,double sourceTolerance,bool includeLocked)
    {
        var world=EditorRenderer.Box(item);
        if(source.X<world.X-sourceTolerance||source.X>world.Right+sourceTolerance
            ||source.Y<world.Y-sourceTolerance||source.Y>world.Bottom+sourceTolerance)return false;
        var bounds=ObjectPrecision.Map(world,layout);
        if(output.X<bounds.X-tolerance||output.X>bounds.Right+tolerance
            ||output.Y<bounds.Y-tolerance||output.Y>bounds.Bottom+tolerance)return false;
        if(includeLocked&&item.Locked){item=item.Copy();item.Locked=false;}
        return EditorRenderer.Hit(item,source,sourceTolerance);
    }
    private void UpdateHover(Annotation? hit)
    {
        if(Dragging||Tool==EditorTool.Crop){_hoveredId=null;ToolTip=null;return;}
        string? id=hit!=null&&!_selected.Contains(hit.Id)?hit.Id:null;
        ToolTip=hit==null?null:$"{ShapeStudio.Label(hit.Kind)}{(string.IsNullOrWhiteSpace(hit.Name)?"":" · "+hit.Name)} · Ở Chọn: Alt+click chuyển lớp · Chuột phải để chọn đối tượng tại đây";if(id==_hoveredId)return;_hoveredId=id;InvalidateVisual();
    }
    private void DrawHover(DrawingContext dc)
    {
        if(Dragging||_hoveredId==null||Session?.Document.Annotations.FirstOrDefault(a=>a.Id==_hoveredId) is not {} a)return;
        if(ShapeStudio.IsLine(a.Kind))return;
        dc.DrawRectangle(null,new Pen(Brushes.MediumSlateBlue,1/InteractionScale){DashStyle=DashStyles.Dot},EditorRenderer.Bounds(a));
    }
    private void SnapObjectMove(double dx,double dy,bool shift,bool enabled)
    {
        var l=Layout!;var box=ObjectPrecision.Map(_dragBounds,l);double ox=l.A*dx+l.C*dy,oy=l.B*dx+l.D*dy;
        var targets=_snapOutputPeers;
        var result=ObjectControl.Snap(box,new(0,0,l.Width,l.Height),targets,ox,oy,6/Zoom,shift,enabled,false);var delta=l.SourceDelta(result.X,result.Y);_guideX=result.GuideX;_guideY=result.GuideY;
        foreach(var original in _originals){var a=Session!.Document.Annotations.First(x=>x.Id==original.Id);ObjectControl.RestoreGeometry(a,original);a.Translate(delta.X,delta.Y);}
    }
}
