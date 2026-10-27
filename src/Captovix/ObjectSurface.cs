using System.Windows;
using System.Windows.Media;
namespace Captovix;
public sealed partial class EditorSurface
{
    public bool SnapEnabled {get;set;}=true;
    private double? _guideX,_guideY;
    private AnnotationBox[] _snapPeers=[];
    private AnnotationBox[] _snapOutputPeers=[];
    private Vector _resizePointerOffset;
    private AnnotationPoint _rotationPivot;
    private double _rotationStart;
    private Point[] ObjectHandles()
    {
        if(Session==null||!ObjectControl.CanEdit(Session.Document,_selected))return [];
        if(_selected.Count==1&&Selected is {} single){
            if(ConnectorStudio.IsPath(single)){var start=new Point(single.X1,single.Y1);var end=new Point(single.X2,single.Y2);var c=ConnectorStudio.Control(single);return single.Route==ConnectorRoute.Straight?[start,end]:[start,end,new(c.X,c.Y)];}
            if(ShapeStudio.IsLine(single.Kind))return [EditorRenderer.WorldPoint(single,new(single.X1,single.Y1)),EditorRenderer.WorldPoint(single,new(single.X2,single.Y2))];
            var b=EditorRenderer.LocalBox(single);var points=BoxHandles(b).Select(p=>EditorRenderer.WorldPoint(single,p)).ToArray();
            return single.Kind==AnnotationKind.Callout&&single.CalloutStyle is not (CalloutStyle.Rectangle or CalloutStyle.RoundedRectangle or CalloutStyle.Pill or CalloutStyle.Ellipse)?[..points,EditorRenderer.WorldPoint(single,new(single.TailX,single.TailY))]:points;
        }
        return BoxHandles(SelectionBox());
    }
    private static Point[] BoxHandles(AnnotationBox b)=>[new(b.X,b.Y),new(b.CenterX,b.Y),new(b.Right,b.Y),new(b.Right,b.CenterY),new(b.Right,b.Bottom),new(b.CenterX,b.Bottom),new(b.X,b.Bottom),new(b.X,b.CenterY)];
    private Point RotationHandle()
    {
        Point top,center;if(_selected.Count==1&&Selected is {} a){var b=EditorRenderer.LocalBox(a);top=EditorRenderer.WorldPoint(a,new(b.CenterX,b.Y));center=EditorRenderer.WorldPoint(a,new(b.CenterX,b.CenterY));}else{var b=SelectionBox();top=new(b.CenterX,b.Y);center=new(b.CenterX,b.CenterY);}
        var l=Layout!;var delta=top-center;double x=l.A*delta.X+l.C*delta.Y,y=l.B*delta.X+l.D*delta.Y,length=Math.Sqrt(x*x+y*y);if(length<1e-9){x=0;y=-1;length=1;}var offset=l.SourceDelta(x/length*28/Zoom,y/length*28/Zoom);return new(top.X+offset.X,top.Y+offset.Y);
    }
    private void DrawObjectFrame(DrawingContext dc)
    {
        var canEdit=ObjectControl.CanEdit(Session!.Document,_selected);var pen=new Pen(canEdit?Brushes.DodgerBlue:Brushes.SlateGray,1/Zoom){DashStyle=DashStyles.Dash};
        Point[] corners;if(_selected.Count==1&&Selected is {} a){var b=EditorRenderer.LocalBox(a);corners=new[]{new Point(b.X,b.Y),new(b.Right,b.Y),new(b.Right,b.Bottom),new(b.X,b.Bottom)}.Select(p=>EditorRenderer.WorldPoint(a,p)).ToArray();}
        else{var b=SelectionBox();corners=[new(b.X,b.Y),new(b.Right,b.Y),new(b.Right,b.Bottom),new(b.X,b.Bottom)];}
        var l=Layout!;Point Output(Point p){var q=l.ToOutput(p.X,p.Y);return new(q.X,q.Y);}corners=corners.Select(Output).ToArray();var inverse=EditorRenderer.LayoutMatrix(l);inverse.Invert();dc.PushTransform(new MatrixTransform(inverse));
        var path=new StreamGeometry();using(var c=path.Open()){c.BeginFigure(corners[0],false,true);c.PolyLineTo(corners.Skip(1).ToArray(),true,false);}dc.DrawGeometry(null,pen,path);
        if(canEdit&&!(_selected.Count==1&&Selected is {} line&&ConnectorStudio.IsPath(line))){var top=new Point((corners[0].X+corners[1].X)/2,(corners[0].Y+corners[1].Y)/2);var rotate=Output(RotationHandle());dc.DrawLine(new Pen(Brushes.DodgerBlue,1/Zoom),top,rotate);dc.DrawEllipse(Brushes.White,new Pen(Brushes.DodgerBlue,1/Zoom),rotate,5/Zoom,5/Zoom);}
        dc.Pop();
    }
    private void DrawSnapGuides(DrawingContext dc,PixelRect view)
    {
        var l=Layout!;var matrix=EditorRenderer.LayoutMatrix(l);matrix.Invert();dc.PushTransform(new MatrixTransform(matrix));var pen=new Pen(Brushes.MediumVioletRed,1/Zoom){DashStyle=DashStyles.Dash};
        if(_guideX is {} x)dc.DrawLine(pen,new(x,0),new(x,l.Height));if(_guideY is {} y)dc.DrawLine(pen,new(0,y),new(l.Width,y));dc.Pop();
    }

    private void PrepareObjectGesture()
    {
        var center=_originals.Length==1?EditorRenderer.LocalBox(_originals[0]):_dragBounds;_rotationPivot=new(center.CenterX,center.CenterY);
        _rotationStart=Math.Atan2(_start.Y-_rotationPivot.Y,_start.X-_rotationPivot.X)*180/Math.PI;
        _resizePointerOffset=_handle>=0&&_handle<Handles().Length?_start-Handles()[_handle]:new Vector();
        _snapPeers=Session!.Document.Annotations.Where(a=>a.Visible&&!_selected.Contains(a.Id)).Select(EditorRenderer.Box).ToArray();var layout=Layout!;_snapOutputPeers=_snapPeers.Select(b=>ObjectPrecision.Map(b,layout)).Concat(ObjectPrecision.VisibleImage(Session.Document) is {} image?new[]{image}:Array.Empty<AnnotationBox>()).ToArray();_guideX=_guideY=null;
    }
    private void RotateObjectSelection(Point p,bool shift)
    {
        var angle=Math.Atan2(p.Y-_rotationPivot.Y,p.X-_rotationPivot.X)*180/Math.PI;double delta=ObjectControl.Normalize(angle-_rotationStart);
        if(shift){if(_originals.Length==1)delta=Math.Round((_originals[0].Rotation+delta)/15)*15-_originals[0].Rotation;else delta=Math.Round(delta/15)*15;}
        foreach(var original in _originals){var a=Session!.Document.Annotations.First(x=>x.Id==original.Id);ObjectControl.Rotate(a,original,delta,_rotationPivot,EditorRenderer.LocalBox);}
    }
    private void ResizeObjectSelection(Point p,bool shift)
    {
        p-=_resizePointerOffset;
        if(_originals.Length==1){
            var original=_originals[0];var a=Session!.Document.Annotations.First(x=>x.Id==original.Id);var local=EditorRenderer.LocalPoint(original,p);var box=EditorRenderer.LocalBox(original);
            ObjectControl.RestoreGeometry(a,original);
            if(ConnectorStudio.IsPath(original)){ResizeConnector(a,original,p,_handle);}
            else if(original.Kind==AnnotationKind.Callout&&_handle==8){
                // Move a world-space pointer while retaining a stable rotation pivot.
                a.TailX=local.X;a.TailY=local.Y;var updated=EditorRenderer.LocalBox(a);var center=ObjectControl.RotatePoint(new(updated.CenterX,updated.CenterY),new(box.CenterX,box.CenterY),original.Rotation);a.Translate(center.X-updated.CenterX,center.Y-updated.CenterY);
            }else if(ShapeStudio.IsLine(original.Kind)){
                var fixedPoint=_handle==0?new Point(original.X2,original.Y2):new Point(original.X1,original.Y1);var end=shift?ToConstrainedLocal(fixedPoint,local):local;
                var world=EditorRenderer.WorldPoint(original,end);var found=ConnectToShapes?ConnectorStudio.FindAnchor(Session!.Document,new(world.X,world.Y),12/InteractionScale,a.Id):(null,new AnnotationPoint(world.X,world.Y));
                if(found.Item1!=null){
                    var start=EditorRenderer.WorldPoint(original,new(original.X1,original.Y1));var finish=EditorRenderer.WorldPoint(original,new(original.X2,original.Y2));
                    a.X1=start.X;a.Y1=start.Y;a.X2=finish.X;a.Y2=finish.Y;a.Rotation=0;
                    if(_handle==0){a.StartAnchor=found.Item1;a.X1=found.Item2.X;a.Y1=found.Item2.Y;}else{a.EndAnchor=found.Item1;a.X2=found.Item2.X;a.Y2=found.Item2.Y;}return;
                }
                if(_handle==0){a.X1=end.X;a.Y1=end.Y;}else{a.X2=end.X;a.Y2=end.Y;}
                var updated=EditorRenderer.LocalBox(a);var center=ObjectControl.RotatePoint(new(updated.CenterX,updated.CenterY),new(box.CenterX,box.CenterY),original.Rotation);a.Translate(center.X-updated.CenterX,center.Y-updated.CenterY);
            }else{
                bool uniform=shift||KeepAspectRatio||ObjectPrecision.RequiresUniform([original]);var target=AnnotationEditing.Resize(box,_handle,local.X,local.Y,uniform);target=ObjectPrecision.LimitResize([original],box,target,_handle,uniform);ObjectControl.ResizeSingle(a,original,box,target,EditorRenderer.LocalBox);
            }
        }else{
            bool uniform=shift||KeepAspectRatio||ObjectPrecision.RequiresUniform(_originals);
            var target=AnnotationEditing.Resize(_dragBounds,_handle,p.X,p.Y,uniform);target=ObjectPrecision.LimitResize(_originals,_dragBounds,target,_handle,uniform);
            foreach(var original in _originals)AnnotationEditing.Transform(Session!.Document.Annotations.First(x=>x.Id==original.Id),original,_dragBounds,target);
        }
    }
    private static Point ToConstrainedLocal(Point start,Point end){var d=end-start;double angle=Math.Round(Math.Atan2(d.Y,d.X)/(Math.PI/4))*Math.PI/4;return new(start.X+Math.Cos(angle)*d.Length,start.Y+Math.Sin(angle)*d.Length);}
}
