using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
namespace Captovix;
public sealed partial class EditorSurface
{
    public int NewCalloutPointerCount {get;set;}=1;
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
    // Hover affordance is UI-only. Actual geometry is created through the existing pointer model.
    private Point? _pointerOffer;
    private Point _pointerOrigin;
    private bool CanOfferPointer()=>Session!=null&&Layout!=null&&InlineEditingId==null&&
        (Tool is EditorTool.Select or EditorTool.Callout)&&_selected.Count==1&&
        Selected is {HasCalloutPointers:true,Visible:true} a&&a.CalloutPointerCount<Annotation.MaxCalloutPointers&&
        ObjectControl.CanEdit(Session.Document,_selected);
    private Point? FindPointerOffer(Point p)
    {
        if(!CanOfferPointer()||HitHandle(p)>=0)return null;
        var a=Selected!;var hit=Hit(p);if(hit!=null&&hit.Id!=a.Id)return null;
        // Compare distances in display coordinates (including zoom and canvas transforms).
        var geometry=EditorRenderer.CalloutBodyGeometry(a).GetFlattenedPathGeometry(.25, ToleranceType.Absolute);
        var l=Layout!;Point Output(Point v){var q=l.ToOutput(v.X,v.Y);return new(q.X,q.Y);}
        var cursor=Output(p);double best=8/Zoom;Point? found=null;
        void Segment(Point start,Point end){
            var wa=EditorRenderer.WorldPoint(a,start);var wb=EditorRenderer.WorldPoint(a,end);
            var oa=Output(wa);var ob=Output(wb);var d=ob-oa;
            if(d.LengthSquared<1e-12)return;
            double t=Math.Clamp(Vector.Multiply(cursor-oa,d)/d.LengthSquared,0,1);
            double distance=(cursor-(oa+t*d)).Length;
            if(distance>best)return;
            var candidate=wa+t*(wb-wa);
            // Do not cover resize/rotate/orange handles, or another object's foreground.
            if(HitHandle(candidate)>=0)return;
            best=distance;found=candidate;
        }
        foreach(var figure in geometry.Figures){var start=figure.StartPoint;var last=start;
            foreach(var segment in figure.Segments){if(segment is LineSegment line){Segment(last,line.Point);last=line.Point;}
                else if(segment is PolyLineSegment poly)foreach(var next in poly.Points){Segment(last,next);last=next;}}
            if(figure.IsClosed)Segment(last,start);
        }
        return found;
    }
    private bool UpdatePointerOffer(Point p,ModifierKeys modifiers){
        var next=modifiers==ModifierKeys.None?FindPointerOffer(p):null;
        if(_pointerOffer!=next){_pointerOffer=next;InvalidateVisual();}
        if(next==null)return false;
        Cursor=Cursors.Cross;ToolTip="Kéo ra ngoài để thêm mũi chỉ";return true;
    }
    private void DrawPointerOffer(DrawingContext dc,ImageLayout layout){
        if(Dragging||_pointerOffer is not {} offer||!CanOfferPointer())return;
        var q=layout.ToOutput(offer.X,offer.Y);var p=new Point(q.X,q.Y);
        var violet=Brushes.MediumPurple;dc.DrawEllipse(violet,new Pen(Brushes.White,1.2/Zoom),p,7/Zoom,7/Zoom);
        var pen=new Pen(Brushes.White,1.4/Zoom);dc.DrawLine(pen,new(p.X-3.5/Zoom,p.Y),new(p.X+3.5/Zoom,p.Y));dc.DrawLine(pen,new(p.X,p.Y-3.5/Zoom),new(p.X,p.Y+3.5/Zoom));
    }
    protected override void OnMouseLeave(MouseEventArgs e){base.OnMouseLeave(e);if(_pointerOffer!=null){_pointerOffer=null;InvalidateVisual();}}
    private bool TryBeginPointerAdd(Point p){
        var offer=FindPointerOffer(p);if(offer==null||Session==null||Selected is not {} a)return false;
        _pointerOrigin=offer.Value;_pointerOffer=null;_originals=[a.Copy()];Session.Begin();_action=DragAction.AddCalloutPointer;
        CaptureMouse();InvalidateVisual();return true;
    }
    private void PreviewPointerAdd(Point p){
        if(Session==null||_originals.Length!=1||Selected is not {} a)return;
        var original=_originals[0];ObjectControl.RestoreGeometry(a,original);
        var local=EditorRenderer.LocalPoint(original,p);var body=new Rect(original.Left,original.Top,Math.Max(36,original.Width),Math.Max(36,original.Height));
        // The existing renderer joins tips outside this rectangle. Returning inside cancels preview.
        _moved=double.IsFinite(local.X)&&double.IsFinite(local.Y)&&Math.Abs(local.X)<=999000&&Math.Abs(local.Y)<=999000&&
            !body.Contains(local)&&ScreenDistance(p,_pointerOrigin)>=12;
        if(_moved){
            a.ExtraCalloutPointers??=[];a.ExtraCalloutPointers.Add(new(local.X,local.Y));
            // Legacy documents may rotate about the combined geometry rather than the body.
            var old=EditorRenderer.LocalBox(original);var next=EditorRenderer.LocalBox(a);
            var center=ObjectControl.RotatePoint(new(next.CenterX,next.CenterY),new(old.CenterX,old.CenterY),original.Rotation);
            a.Translate(center.X-next.CenterX,center.Y-next.CenterY);
        }
        InvalidateScenePaint();
    }
    private void FinishPointerAdd(){
        if(Session==null)return;
        if(_moved){ObjectPrecision.RevealOutside(Session.Document,_selected,EditorRenderer.GeometryBox);FitSelectionCanvas();Session.Commit();}
        else Session.Cancel();
        _originals=[];_pointerOffer=null;Refresh();SelectionChanged?.Invoke();
    }
}
