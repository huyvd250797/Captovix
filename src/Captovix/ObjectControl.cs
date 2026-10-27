namespace Captovix;
public readonly record struct SnapResult(double X,double Y,double? GuideX=null,double? GuideY=null);
public static class ObjectControl
{
    public static double Normalize(double angle)=>((angle+180)%360+360)%360-180;
    public static AnnotationPoint RotatePoint(AnnotationPoint p,AnnotationPoint center,double angle)
    {
        double radians=angle*Math.PI/180,c=Math.Cos(radians),s=Math.Sin(radians),dx=p.X-center.X,dy=p.Y-center.Y;return new(center.X+c*dx-s*dy,center.Y+s*dx+c*dy);
    }
    public static AnnotationBox RotatedBounds(AnnotationBox b,double angle)
    {
        if(Math.Abs(Normalize(angle))<1e-9)return b;
        var center=new AnnotationPoint(b.CenterX,b.CenterY);var corners=new[]{new AnnotationPoint(b.X,b.Y),new(b.Right,b.Y),new(b.Right,b.Bottom),new(b.X,b.Bottom)}.Select(p=>RotatePoint(p,center,angle)).ToArray();
        return new(corners.Min(p=>p.X),corners.Min(p=>p.Y),corners.Max(p=>p.X)-corners.Min(p=>p.X),corners.Max(p=>p.Y)-corners.Min(p=>p.Y));
    }
    public static AnnotationBox SelectionBounds(AnnotationBox b)
    {
        double w=Math.Max(1,b.Width),h=Math.Max(1,b.Height);return new(b.X-(w-b.Width)/2,b.Y-(h-b.Height)/2,w,h);
    }
    public static Annotation[] Members(EditorDocument doc,IEnumerable<string> ids)
    {
        var requested=ids.ToArray();if(requested.Length==0)return [];var all=AnnotationEditing.ExpandGroups(doc,requested);return doc.Annotations.Where(a=>all.Contains(a.Id)).ToArray();
    }
    public static bool CanEdit(EditorDocument doc,IEnumerable<string> ids){var a=Members(doc,ids);return a.Length>0&&a.All(x=>!x.Locked&&x.Visible);}
    public static bool CanvasEditable(EditorDocument doc,Annotation a)=>a.Visible&&!a.Locked&&(a.GroupId==null||!doc.Annotations.Any(b=>b.GroupId==a.GroupId&&(b.Locked||!b.Visible)));
    public static Annotation[] CanvasCandidates(EditorDocument doc)
    {
        var blocked=doc.Annotations.Where(a=>a.GroupId!=null&&(a.Locked||!a.Visible)).Select(a=>a.GroupId!).ToHashSet();
        return doc.Annotations.Where(a=>a.Visible&&!a.Locked&&(a.GroupId==null||!blocked.Contains(a.GroupId))).ToArray();
    }
    public static void Lock(EditorDocument doc,IEnumerable<string> ids,bool value){foreach(var a in Members(doc,ids))a.Locked=value;}
    public static void Show(EditorDocument doc,IEnumerable<string> ids,bool value){foreach(var a in Members(doc,ids))a.Visible=value;}
    public static void Rename(Annotation a,string name){name=name.Trim();if(name.Length>80||name.Any(char.IsControl))throw new ArgumentException("Tên layer tối đa 80 ký tự, không có ký tự điều khiển.");a.Name=name;}
    public static void RestoreGeometry(Annotation target,Annotation original)
    {
        target.TextPadding=original.TextPadding;target.TextWidth=original.TextWidth;target.Control=original.Control;target.Route=original.Route;target.StartAnchor=original.StartAnchor;target.EndAnchor=original.EndAnchor;target.X1=original.X1;target.X2=original.X2;target.Y1=original.Y1;target.Y2=original.Y2;target.TailX=original.TailX;target.TailY=original.TailY;target.Points=[..original.Points];target.FontSize=original.FontSize;target.Rotation=original.Rotation;
    }
    public static void Rotate(Annotation target,Annotation original,double delta,AnnotationPoint pivot,Func<Annotation,AnnotationBox> localBounds)
    {
        if(ConnectorStudio.IsPath(original)){RestoreGeometry(target,original);var s=RotatePoint(ConnectorStudio.Start(original),pivot,delta);var e=RotatePoint(ConnectorStudio.End(original),pivot,delta);target.X1=s.X;target.Y1=s.Y;target.X2=e.X;target.Y2=e.Y;target.Control=RotatePoint(ConnectorStudio.Control(original),pivot,delta);target.Rotation=0;return;}
        RestoreGeometry(target,original);var b=localBounds(original);var center=new AnnotationPoint(b.CenterX,b.CenterY);var next=RotatePoint(center,pivot,delta);target.Translate(next.X-center.X,next.Y-center.Y);target.Rotation=Normalize(original.Rotation+delta);
    }
    public static void ResizeSingle(Annotation target,Annotation original,AnnotationBox localBox,AnnotationBox newBox,Func<Annotation,AnnotationBox>? actualBounds=null)
    {
        AnnotationEditing.Transform(target,original,localBox,newBox);
        // Resize in the old rotated coordinate system, then move the new pivot
        // so the opposite handle stays fixed in world/source coordinates.
        var actual=actualBounds?.Invoke(target)??newBox;var desired=RotatePoint(new(actual.CenterX,actual.CenterY),new(localBox.CenterX,localBox.CenterY),original.Rotation);
        target.Translate(desired.X-actual.CenterX,desired.Y-actual.CenterY);
    }
    public static void ApplyFrame(EditorDocument doc,IEnumerable<string> ids,double x,double y,double width,double height,double angle,Func<Annotation,AnnotationBox> localBounds)
    {
        if(!CanEdit(doc,ids))throw new InvalidOperationException("Mở khóa và hiện toàn bộ lựa chọn trước khi chỉnh.");
        if(new[]{x,y,width,height,angle}.Any(v=>!double.IsFinite(v))||Math.Abs(x)>1000000||Math.Abs(y)>1000000||width<0||height<0||width>100000||height>100000||Math.Abs(angle)>360)throw new ArgumentException("Khung hoặc góc xoay không hợp lệ.");
        ConnectorStudio.DetachForTransform(doc,ids);
        var objects=Members(doc,ids);var originals=objects.Select(a=>a.Copy()).ToArray();
        var frame=objects.Length==1?localBounds(originals[0]):AnnotationEditing.Union(originals.Select(a=>{var b=RotatedBounds(localBounds(a),a.Rotation);return SelectionBounds(b);}));
        bool line=objects.Length==1&&ShapeStudio.IsLine(objects[0].Kind);
        if(line){if(Math.Sqrt(width*width+height*height)<2)throw new ArgumentException("Đoạn thẳng cần dài ít nhất 2 px.");}
        else if(width<1||height<1)throw new ArgumentException("Khung cần rộng và cao ít nhất 1 px.");
        double sx=width/Math.Max(1,frame.Width),sy=height/Math.Max(1,frame.Height);
        if(objects.Length>1&&originals.Any(a=>Math.Abs(a.Rotation)>1e-8)&&Math.Abs(sx-sy)>1e-6)throw new ArgumentException("Nhóm có đối tượng xoay cần giữ tỷ lệ để tránh méo hình.");
        var next=new AnnotationBox(x,y,width,height);
        if(line&&!ConnectorStudio.IsPath(originals[0])){
            var a=objects[0];var old=originals[0];a.X1=old.X1<=old.X2?x:x+width;a.X2=old.X1<=old.X2?x+width:x;a.Y1=old.Y1<=old.Y2?y:y+height;a.Y2=old.Y1<=old.Y2?y+height:y;
        }else for(int i=0;i<objects.Length;i++)AnnotationEditing.Transform(objects[i],originals[i],frame,next);
        if(objects.Length==1){if(ConnectorStudio.IsPath(objects[0])){var snapshot=objects[0].Copy();Rotate(objects[0],snapshot,angle,new(next.CenterX,next.CenterY),localBounds);}else objects[0].Rotation=Normalize(angle);}
        else{
            var pivot=new AnnotationPoint(next.CenterX,next.CenterY);for(int i=0;i<objects.Length;i++){var snapshot=objects[i].Copy();Rotate(objects[i],snapshot,angle,pivot,localBounds);}
        }
    }
    public static SnapResult Snap(AnnotationBox box,PixelRect view,IEnumerable<AnnotationBox> peers,double dx,double dy,double tolerance,bool axisLock,bool enabled,bool clamp=true)
    {
        if(axisLock){if(Math.Abs(dx)>=Math.Abs(dy))dy=0;else dx=0;}
        if(!enabled){var free=clamp?AnnotationEditing.ClampMove(box,view,dx,dy):(X:dx,Y:dy);return new(free.X,free.Y);}
        double bestX=tolerance+1,bestY=tolerance+1,offsetX=0,offsetY=0;double? guideX=null,guideY=null;
        void Consider(double edge,bool horizontal){for(int i=0;i<3;i++){double own=(horizontal?box.X+dx:box.Y+dy)+(horizontal?box.Width:box.Height)*i/2;double diff=edge-own;if(Math.Abs(diff)>tolerance)continue;if(horizontal&&Math.Abs(diff)<bestX){bestX=Math.Abs(diff);offsetX=diff;guideX=edge;}if(!horizontal&&Math.Abs(diff)<bestY){bestY=Math.Abs(diff);offsetY=diff;guideY=edge;}}}
        void Target(AnnotationBox b){Consider(b.X,true);Consider(b.CenterX,true);Consider(b.Right,true);Consider(b.Y,false);Consider(b.CenterY,false);Consider(b.Bottom,false);}
        Target(new(view.X,view.Y,view.Width,view.Height));foreach(var peer in peers)Target(peer);
        if(axisLock&&dx==0){offsetX=0;guideX=null;}if(axisLock&&dy==0){offsetY=0;guideY=null;}
        var actual=clamp?AnnotationEditing.ClampMove(box,view,dx+offsetX,dy+offsetY):(X:dx+offsetX,Y:dy+offsetY);
        if(Math.Abs(actual.X-(dx+offsetX))>1e-8)guideX=null;if(Math.Abs(actual.Y-(dy+offsetY))>1e-8)guideY=null;
        return new(actual.X,actual.Y,guideX,guideY);
    }
}
