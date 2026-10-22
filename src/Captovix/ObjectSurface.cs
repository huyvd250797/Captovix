using System.Windows;
using System.Windows.Media;
namespace Captovix;
public sealed partial class EditorSurface
{
    public bool SnapEnabled {get;set;}=true;
    private double? _guideX,_guideY;
    private AnnotationBox[] _snapPeers=[];
    private AnnotationPoint _rotationPivot;
    private double _rotationStart;
    private Point[] ObjectHandles()
    {
        if(Session==null||!ObjectControl.CanEdit(Session.Document,_selected))return [];
        if(_selected.Count==1&&Selected is {} single){
            if(ShapeStudio.IsLine(single.Kind))return [EditorRenderer.WorldPoint(single,new(single.X1,single.Y1)),EditorRenderer.WorldPoint(single,new(single.X2,single.Y2))];
            var b=EditorRenderer.LocalBox(single);var points=BoxHandles(b).Select(p=>EditorRenderer.WorldPoint(single,p)).ToArray();
            return single.Kind==AnnotationKind.Callout?[..points,EditorRenderer.WorldPoint(single,new(single.TailX,single.TailY))]:points;
        }
        return BoxHandles(SelectionBox());
    }
    private static Point[] BoxHandles(AnnotationBox b)=>[new(b.X,b.Y),new(b.CenterX,b.Y),new(b.Right,b.Y),new(b.Right,b.CenterY),new(b.Right,b.Bottom),new(b.CenterX,b.Bottom),new(b.X,b.Bottom),new(b.X,b.CenterY)];
    private Point RotationHandle()
    {
        if(_selected.Count==1&&Selected is {} a){var b=EditorRenderer.LocalBox(a);return EditorRenderer.WorldPoint(a,new(b.CenterX,b.Y-28/InteractionScale));}
        var box=SelectionBox();return new(box.CenterX,box.Y-28/InteractionScale);
    }
    private void DrawObjectFrame(DrawingContext dc)
    {
        var canEdit=ObjectControl.CanEdit(Session!.Document,_selected);var pen=new Pen(canEdit?Brushes.DodgerBlue:Brushes.SlateGray,1/InteractionScale){DashStyle=DashStyles.Dash};
        Point[] corners;if(_selected.Count==1&&Selected is {} a){var b=EditorRenderer.LocalBox(a);corners=new[]{new Point(b.X,b.Y),new(b.Right,b.Y),new(b.Right,b.Bottom),new(b.X,b.Bottom)}.Select(p=>EditorRenderer.WorldPoint(a,p)).ToArray();}
        else{var b=SelectionBox();corners=[new(b.X,b.Y),new(b.Right,b.Y),new(b.Right,b.Bottom),new(b.X,b.Bottom)];}
        var path=new StreamGeometry();using(var c=path.Open()){c.BeginFigure(corners[0],false,true);c.PolyLineTo(corners.Skip(1).ToArray(),true,false);}dc.DrawGeometry(null,pen,path);
        if(canEdit){var top=new Point((corners[0].X+corners[1].X)/2,(corners[0].Y+corners[1].Y)/2);var rotate=RotationHandle();dc.DrawLine(new Pen(Brushes.DodgerBlue,1/InteractionScale),top,rotate);dc.DrawEllipse(Brushes.White,new Pen(Brushes.DodgerBlue,1/InteractionScale),rotate,5/InteractionScale,5/InteractionScale);}
    }
    private void DrawSnapGuides(DrawingContext dc,PixelRect view)
    {
        var pen=new Pen(Brushes.DeepPink,1/InteractionScale){DashStyle=DashStyles.Dash};
        if(_guideX is {} x)dc.DrawLine(pen,new(x,view.Y),new(x,view.Bottom));if(_guideY is {} y)dc.DrawLine(pen,new(view.X,y),new(view.Right,y));
    }
    private void PrepareObjectGesture()
    {
        var center=_originals.Length==1?EditorRenderer.LocalBox(_originals[0]):_dragBounds;_rotationPivot=new(center.CenterX,center.CenterY);
        _rotationStart=Math.Atan2(_start.Y-_rotationPivot.Y,_start.X-_rotationPivot.X)*180/Math.PI;
        _snapPeers=Session!.Document.Annotations.Where(a=>a.Visible&&!_selected.Contains(a.Id)).Select(EditorRenderer.Box).ToArray();_guideX=_guideY=null;
    }
    private void RotateObjectSelection(Point p,bool shift)
    {
        var angle=Math.Atan2(p.Y-_rotationPivot.Y,p.X-_rotationPivot.X)*180/Math.PI;double delta=ObjectControl.Normalize(angle-_rotationStart);
        if(shift){if(_originals.Length==1)delta=Math.Round((_originals[0].Rotation+delta)/15)*15-_originals[0].Rotation;else delta=Math.Round(delta/15)*15;}
        foreach(var original in _originals){var a=Session!.Document.Annotations.First(x=>x.Id==original.Id);ObjectControl.Rotate(a,original,delta,_rotationPivot,EditorRenderer.LocalBox);}
    }
    private void ResizeObjectSelection(Point p,bool shift)
    {
        if(_originals.Length==1){
            var original=_originals[0];var a=Session!.Document.Annotations.First(x=>x.Id==original.Id);var local=EditorRenderer.LocalPoint(original,p);var box=EditorRenderer.LocalBox(original);
            ObjectControl.RestoreGeometry(a,original);
            if(original.Kind==AnnotationKind.Callout&&_handle==8){
                // Move a world-space pointer while retaining a stable rotation pivot.
                a.TailX=local.X;a.TailY=local.Y;var updated=EditorRenderer.LocalBox(a);var center=ObjectControl.RotatePoint(new(updated.CenterX,updated.CenterY),new(box.CenterX,box.CenterY),original.Rotation);a.Translate(center.X-updated.CenterX,center.Y-updated.CenterY);
            }else if(ShapeStudio.IsLine(original.Kind)){
                var fixedPoint=_handle==0?new Point(original.X2,original.Y2):new Point(original.X1,original.Y1);var end=shift?ToConstrainedLocal(fixedPoint,local):local;
                if(_handle==0){a.X1=end.X;a.Y1=end.Y;}else{a.X2=end.X;a.Y2=end.Y;}
                var updated=EditorRenderer.LocalBox(a);var center=ObjectControl.RotatePoint(new(updated.CenterX,updated.CenterY),new(box.CenterX,box.CenterY),original.Rotation);a.Translate(center.X-updated.CenterX,center.Y-updated.CenterY);
            }else{
                var target=AnnotationEditing.Resize(box,_handle,local.X,local.Y,shift||original.Kind is AnnotationKind.Text or AnnotationKind.Step);ObjectControl.ResizeSingle(a,original,box,target,EditorRenderer.LocalBox);
            }
        }else{
            bool uniform=shift||_originals.Any(a=>Math.Abs(a.Rotation)>1e-8);
            var target=AnnotationEditing.Resize(_dragBounds,_handle,p.X,p.Y,uniform);
            if(uniform&&_handle is 1 or 3 or 5 or 7){double ratio=_dragBounds.Width/Math.Max(1,_dragBounds.Height);if(_handle is 3 or 7)target=target with{Height=target.Width/ratio};else target=target with{Width=target.Height*ratio};}
            foreach(var original in _originals)AnnotationEditing.Transform(Session!.Document.Annotations.First(x=>x.Id==original.Id),original,_dragBounds,target);
        }
    }
    private static Point ToConstrainedLocal(Point start,Point end){var d=end-start;double angle=Math.Round(Math.Atan2(d.Y,d.X)/(Math.PI/4))*Math.PI/4;return new(start.X+Math.Cos(angle)*d.Length,start.Y+Math.Sin(angle)*d.Length);}
}
