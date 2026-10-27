namespace Captovix;
public enum AlignmentReference { Selection,Image,Canvas }
// Geometry here is in output pixels, independent of zoom and Windows DPI.
public static class ObjectPrecision
{
    public static AnnotationBox Map(AnnotationBox box,ImageLayout layout,bool inverse=false)
    {
        var points=new[]{new ImagePoint(box.X,box.Y),new(box.Right,box.Y),new(box.Right,box.Bottom),new(box.X,box.Bottom)}.Select(p=>inverse?layout.ToSource(p.X,p.Y):layout.ToOutput(p.X,p.Y)).ToArray();
        double x=points.Min(p=>p.X),y=points.Min(p=>p.Y);return new(x,y,points.Max(p=>p.X)-x,points.Max(p=>p.Y)-y);
    }
    public static AnnotationBox Frame(EditorDocument doc,IEnumerable<string> ids,Func<Annotation,AnnotationBox> bounds)=>Map(AnnotationEditing.Union(ObjectControl.Members(doc,ids).Select(bounds)),new(doc.Viewport,doc.Appearance));
    public static AnnotationBox? VisibleImage(EditorDocument doc)
    {
        var l=new ImageLayout(doc.Viewport,doc.Appearance);var b=Map(new(doc.Viewport.X,doc.Viewport.Y,doc.Viewport.Width,doc.Viewport.Height),l);
        double x=Math.Max(Math.Max(0,l.Canvas.X),b.X),y=Math.Max(Math.Max(0,l.Canvas.Y),b.Y),right=Math.Min(Math.Min(l.Width,l.Canvas.Right),b.Right),bottom=Math.Min(Math.Min(l.Height,l.Canvas.Bottom),b.Bottom);return right>x&&bottom>y?new(x,y,right-x,bottom-y):null;
    }
    public static bool RequiresUniform(IEnumerable<Annotation> objects)=>false;
    private static void Move(IEnumerable<Annotation> objects,ImageLayout l,double x,double y){var delta=l.SourceDelta(x,y);foreach(var a in objects)a.Translate(delta.X,delta.Y);}
    public static void RevealOutside(EditorDocument doc,IEnumerable<string> ids,Func<Annotation,AnnotationBox> bounds)
    {
        if(doc.Appearance.DrawOutsideImage)return;var v=doc.Viewport;var l=new ImageLayout(v,doc.Appearance);if(ObjectControl.Members(doc,ids).Select(bounds).Any(b=>{var output=Map(b,l);return b.X<v.X||b.Y<v.Y||b.Right>v.Right||b.Bottom>v.Bottom||output.X<l.Canvas.X||output.Y<l.Canvas.Y||output.Right>l.Canvas.Right||output.Bottom>l.Canvas.Bottom;}))doc.Appearance.DrawOutsideImage=true;
    }
    public static void Align(EditorDocument doc,IEnumerable<string> ids,AnnotationAlignment alignment,AlignmentReference reference,Func<Annotation,AnnotationBox> bounds)
    {
        var selected=AnnotationEditing.ExpandGroups(doc,ids);if(!ObjectControl.CanEdit(doc,selected))throw new InvalidOperationException("Hiện và mở khóa toàn bộ lựa chọn trước khi căn.");
        var l=new ImageLayout(doc.Viewport,doc.Appearance);var units=AnnotationEditing.Units(doc,selected).Select(u=>(Objects:u,Box:Map(AnnotationEditing.Union(u.Select(bounds)),l))).ToArray();
        var target=reference switch{AlignmentReference.Image=>VisibleImage(doc)??throw new InvalidOperationException("Ảnh nền nằm ngoài crop hiện tại."),AlignmentReference.Selection when units.Length>1=>AnnotationEditing.Union(units.Select(u=>u.Box)),_=>new AnnotationBox(0,0,l.Width,l.Height)};
        ConnectorStudio.DetachForTransform(doc,selected);
        foreach(var unit in units){double dx=alignment switch{AnnotationAlignment.Left=>target.X-unit.Box.X,AnnotationAlignment.CenterX=>target.CenterX-unit.Box.CenterX,AnnotationAlignment.Right=>target.Right-unit.Box.Right,_=>0};double dy=alignment switch{AnnotationAlignment.Top=>target.Y-unit.Box.Y,AnnotationAlignment.CenterY=>target.CenterY-unit.Box.CenterY,AnnotationAlignment.Bottom=>target.Bottom-unit.Box.Bottom,_=>0};Move(unit.Objects,l,dx,dy);}RevealOutside(doc,selected,bounds);
    }
    public static bool Distribute(EditorDocument doc,IEnumerable<string> ids,bool horizontal,Func<Annotation,AnnotationBox> bounds)
    {
        var selected=AnnotationEditing.ExpandGroups(doc,ids);if(!ObjectControl.CanEdit(doc,selected))return false;var l=new ImageLayout(doc.Viewport,doc.Appearance);
        var units=AnnotationEditing.Units(doc,selected).Select(u=>(Objects:u,Box:Map(AnnotationEditing.Union(u.Select(bounds)),l))).OrderBy(u=>horizontal?u.Box.X:u.Box.Y).ToArray();if(units.Length<3)return false;
        double low=horizontal?units[0].Box.X:units[0].Box.Y,high=horizontal?units[^1].Box.Right:units[^1].Box.Bottom;double gap=(high-low-units.Sum(u=>horizontal?u.Box.Width:u.Box.Height))/(units.Length-1);if(gap<0)return false;
        ConnectorStudio.DetachForTransform(doc,selected);double position=low;for(int i=0;i<units.Length;i++){var u=units[i];if(i>0&&i<units.Length-1)Move(u.Objects,l,horizontal?position-u.Box.X:0,horizontal?0:position-u.Box.Y);position+=(horizontal?u.Box.Width:u.Box.Height)+gap;}RevealOutside(doc,selected,bounds);return true;
    }
    public static AnnotationBox LimitResize(IEnumerable<Annotation> originals,AnnotationBox from,AnnotationBox target,int handle,bool uniform)
    {
        double minX=0,minY=0,max=double.PositiveInfinity;
        foreach(var a in originals){if(a.Kind==AnnotationKind.Callout){minX=Math.Max(minX,36/Math.Max(1,a.Width));minY=Math.Max(minY,36/Math.Max(1,a.Height));}if(a.Kind is AnnotationKind.Text or AnnotationKind.Step){double min=8/a.FontSize;minX=Math.Max(minX,min);minY=Math.Max(minY,min);max=Math.Min(max,240/a.FontSize);}}
        double sx=target.Width/Math.Max(1,from.Width),sy=target.Height/Math.Max(1,from.Height);if(uniform){double low=Math.Max(minX,minY);if(low>max)return from;sx=sy=Math.Clamp(sx,low,max);}else{sx=Math.Max(sx,minX);sy=Math.Max(sy,minY);}
        double width=from.Width*sx,height=from.Height*sy;double x=handle is 0 or 6 or 7?from.Right-width:handle is 2 or 3 or 4?from.X:from.CenterX-width/2;double y=handle is 0 or 1 or 2?from.Bottom-height:handle is 4 or 5 or 6?from.Y:from.CenterY-height/2;return new(x,y,width,height);
    }
    public static void ApplyFrame(EditorDocument doc,IEnumerable<string> ids,double x,double y,double width,double height,double angle,Func<Annotation,AnnotationBox> worldBounds,Func<Annotation,AnnotationBox> localBounds)
    {
        var selected=AnnotationEditing.ExpandGroups(doc,ids);if(!ObjectControl.CanEdit(doc,selected))throw new InvalidOperationException("Mở khóa và hiện toàn bộ lựa chọn trước khi chỉnh.");
        if(new[]{x,y,width,height,angle}.Any(n=>!double.IsFinite(n))||Math.Abs(x)>1000000||Math.Abs(y)>1000000||width<0||height<0||width>100000||height>100000||Math.Abs(angle)>360)throw new ArgumentException("Khung hoặc góc không hợp lệ.");
        var objects=ObjectControl.Members(doc,selected);var originals=objects.Select(a=>a.Copy()).ToArray();var l=new ImageLayout(doc.Viewport,doc.Appearance);var sourceBox=AnnotationEditing.Union(originals.Select(worldBounds));var output=Map(sourceBox,l);
        // A zero-width/height line may be moved/rotated; resizing it into a box is ambiguous.
        if(output.Width<1e-8||output.Height<1e-8){if(output.Width<1e-8&&width>1e-6||output.Height<1e-8&&height>1e-6)throw new ArgumentException("Đường có một chiều bằng 0: kéo đầu mút để thay kích thước.");}
        else if(width<1||height<1)throw new ArgumentException("Rộng và cao tối thiểu 1 pixel đầu ra.");
        double sx=output.Width>1e-8?width/output.Width:1,sy=output.Height>1e-8?height/output.Height:1;
        if(RequiresUniform(objects)&&Math.Abs(sx-sy)>1e-5)throw new ArgumentException("Đối tượng xoay, chữ hoặc số bước cần giữ tỷ lệ.");
        foreach(var a in originals.Where(a=>a.Kind is AnnotationKind.Text or AnnotationKind.Step))if(a.FontSize*Math.Min(sx,sy)<8||a.FontSize*Math.Min(sx,sy)>240)throw new ArgumentException("Kích thước yêu cầu vượt giới hạn cỡ chữ 8–240.");
        var target=Map(new(x,y,width,height),l,true);
        foreach(var a in originals.Where(a=>a.Kind==AnnotationKind.Callout))if(a.Width*target.Width/Math.Max(1,sourceBox.Width)<36-1e-6||a.Height*target.Height/Math.Max(1,sourceBox.Height)<36-1e-6)throw new ArgumentException("Callout cần thân hộp ít nhất 36 đơn vị nguồn mỗi chiều.");
        ConnectorStudio.DetachForTransform(doc,selected);
        for(int i=0;i<objects.Length;i++)AnnotationEditing.Transform(objects[i],originals[i],sourceBox,target);
        var pivotBox=AnnotationEditing.Union(objects.Select(worldBounds));var pivot=new AnnotationPoint(pivotBox.CenterX,pivotBox.CenterY);
        foreach(var a in objects){double delta=objects.Length==1&&!ConnectorStudio.IsPath(a)?ObjectControl.Normalize(angle-a.Rotation):angle;var before=a.Copy();ObjectControl.Rotate(a,before,delta,pivot,localBounds);}
        var actual=Map(AnnotationEditing.Union(objects.Select(worldBounds)),l);Move(objects,l,x-actual.X,y-actual.Y);
        RevealOutside(doc,selected,worldBounds);doc.Validate(doc.OriginalWidth,doc.OriginalHeight,doc.SourceHash);
    }
}
