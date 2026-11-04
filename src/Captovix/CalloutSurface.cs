using System.Windows;
using System.Windows.Media;
namespace Captovix;
public sealed partial class EditorSurface
{
    private bool BeginCalloutDrag()
    {
        if(Session==null||Tool!=EditorTool.Callout||Session.Document.Annotations.Count>=10000)return false;
        Session.Begin();if(AllowsOutside)Session.Document.Appearance.DrawOutsideImage=true;
        var a=new Annotation{Kind=AnnotationKind.Callout,X1=_start.X,Y1=_start.Y,X2=_start.X,Y2=_start.Y,Color=InkColor,Stroke=StrokeSize,FontSize=TextSize,EffectSize=EffectSize,Text=""};
        ShapeStudio.CopyStyle(ShapeDefaults,a);InitializeTextObject(a);InitializeFocusObject(a);BeginConnector(a);
        a.TailX=_start.X;a.TailY=_start.Y;Session.Document.Annotations.Add(a);
        _selected.Clear();_selected.Add(a.Id);_primary=_createdId=a.Id;_action=DragAction.Create;SelectionChanged?.Invoke();return true;
    }
    private Point? CalloutBodyCenter()
    {
        if(InlineEditingId!=null||Session==null||_selected.Count!=1||Selected is not {Kind:AnnotationKind.Callout} a||!ObjectControl.CanEdit(Session.Document,_selected))return null;
        return EditorRenderer.WorldPoint(a,new(a.Left+a.Width/2,a.Top+a.Height/2));
    }
    private void DrawCalloutBodyCenter(DrawingContext dc,ImageLayout layout)
    {
        if(CalloutBodyCenter() is not {} center)return;var output=layout.ToOutput(center.X,center.Y);var point=new Point(output.X,output.Y);
        dc.DrawEllipse(Brushes.DodgerBlue,new Pen(Brushes.White,1.2/Zoom),point,6/Zoom,6/Zoom);
        var pen=new Pen(Brushes.White,1/Zoom);dc.DrawLine(pen,new(point.X-3/Zoom,point.Y),new(point.X+3/Zoom,point.Y));dc.DrawLine(pen,new(point.X,point.Y-3/Zoom),new(point.X,point.Y+3/Zoom));
    }
    private void MoveCalloutBodySelection(Point p,bool shift)
    {
        if(Session==null||_originals.Length!=1||Selected is not {Kind:AnnotationKind.Callout} a)return;
        double dx=p.X-_start.X,dy=p.Y-_start.Y;if(shift){if(Math.Abs(dx)>=Math.Abs(dy))dy=0;else dx=0;}
        ObjectControl.MoveCalloutBody(a,_originals[0],dx,dy,EditorRenderer.LocalBox);
    }
}
