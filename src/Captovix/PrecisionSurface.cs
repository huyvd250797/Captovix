using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
namespace Captovix;
public sealed partial class EditorSurface
{
    public bool KeepAspectRatio {get;set;}=true;
    public Point? ContextSourcePoint {get;private set;}
    private string? _hoveredId;
    private double ScreenDistance(Point a,Point b){var l=Layout!;double x=a.X-b.X,y=a.Y-b.Y;return Math.Sqrt(Math.Pow(l.A*x+l.C*y,2)+Math.Pow(l.B*x+l.D*y,2))*Zoom;}
    public Annotation[] HitStack(Point source,bool includeLocked=false)
    {
        if(Session==null||Layout is not {} l)return [];var p=l.ToOutput(source.X,source.Y);if(p.X<0||p.Y<0||p.X>l.Width||p.Y>l.Height)return [];
        var candidates=includeLocked?Session.Document.Annotations.Where(a=>a.Visible):ObjectControl.CanvasCandidates(Session.Document);
        double tolerance=5/Zoom;
        return candidates.Select((a,i)=>(Object:a,Index:i)).OrderByDescending(x=>AnnotationEditing.Tier(x.Object)).ThenByDescending(x=>x.Index).Where(x=>{
            var world=EditorRenderer.Box(x.Object);double sourceTolerance=5/InteractionScale;if(source.X<world.X-sourceTolerance||source.X>world.Right+sourceTolerance||source.Y<world.Y-sourceTolerance||source.Y>world.Bottom+sourceTolerance)return false;var b=ObjectPrecision.Map(world,l);var target=x.Object;if(includeLocked&&target.Locked){target=target.Copy();target.Locked=false;}return p.X>=b.X-tolerance&&p.X<=b.Right+tolerance&&p.Y>=b.Y-tolerance&&p.Y<=b.Bottom+tolerance&&EditorRenderer.Hit(target,source,5/InteractionScale);
        }).Select(x=>x.Object).ToArray();
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
        dc.DrawRectangle(null,new Pen(Brushes.MediumSlateBlue,1/InteractionScale){DashStyle=DashStyles.Dot},EditorRenderer.Bounds(a));
    }
    private void SnapObjectMove(double dx,double dy,bool shift,bool enabled)
    {
        var l=Layout!;var box=ObjectPrecision.Map(_dragBounds,l);double ox=l.A*dx+l.C*dy,oy=l.B*dx+l.D*dy;
        var targets=_snapOutputPeers;
        var result=ObjectControl.Snap(box,new(0,0,l.Width,l.Height),targets,ox,oy,6/Zoom,shift,enabled);var delta=l.SourceDelta(result.X,result.Y);_guideX=result.GuideX;_guideY=result.GuideY;
        foreach(var original in _originals){var a=Session!.Document.Annotations.First(x=>x.Id==original.Id);ObjectControl.RestoreGeometry(a,original);a.Translate(delta.X,delta.Y);}
    }
}
