using System.IO;
namespace Captovix;
public enum ConnectorRoute { Straight, Elbow, Curve }
public sealed record ConnectorAnchor(string TargetId,double U,double V);
public static class ConnectorStudio
{
    public static bool IsPath(Annotation a)=>ShapeStudio.IsLine(a.Kind)&&(a.Route!=ConnectorRoute.Straight||a.StartAnchor!=null||a.EndAnchor!=null);
    public static AnnotationPoint Start(Annotation a)=>new(a.X1,a.Y1);
    public static AnnotationPoint End(Annotation a)=>new(a.X2,a.Y2);
    public static AnnotationPoint Control(Annotation a)=>a.Control??(a.Route==ConnectorRoute.Curve?new((a.X1+a.X2)/2-(a.Y2-a.Y1)*.3,(a.Y1+a.Y2)/2+(a.X2-a.X1)*.3):new((a.X1+a.X2)/2,(a.Y1+a.Y2)/2));
    public static AnnotationPoint[] Samples(Annotation a,int steps=48)
    {
        var s=Start(a);var e=End(a);var c=Control(a);
        if(a.Route==ConnectorRoute.Straight)return [s,e];
        if(a.Route==ConnectorRoute.Elbow)return [s,new(c.X,s.Y),new(c.X,e.Y),e];
        steps=Math.Clamp(steps,8,128);return Enumerable.Range(0,steps+1).Select(i=>{double t=(double)i/steps,u=1-t;return new AnnotationPoint(u*u*s.X+2*u*t*c.X+t*t*e.X,u*u*s.Y+2*u*t*c.Y+t*t*e.Y);}).ToArray();
    }
    public static AnnotationBox Bounds(Annotation a)
    {
        var p=Samples(a);double x=p.Min(p=>p.X),y=p.Min(p=>p.Y);return new(x,y,p.Max(p=>p.X)-x,p.Max(p=>p.Y)-y);
    }
    public static AnnotationPoint AnchorPoint(Annotation target,ConnectorAnchor link)
    {
        var center=new AnnotationPoint(target.Left+target.Width/2,target.Top+target.Height/2);
        var raw=ObjectControl.RotatePoint(new(target.Left+target.Width*link.U,target.Top+target.Height*link.V),center,target.Rotation);
        var projected=NearestAnchor(target,raw);return ObjectControl.RotatePoint(new(target.Left+target.Width*projected.U,target.Top+target.Height*projected.V),center,target.Rotation);
    }
    public static ConnectorAnchor NearestAnchor(Annotation target,AnnotationPoint world)
    {
        var p=ObjectControl.RotatePoint(world,new(target.Left+target.Width/2,target.Top+target.Height/2),-target.Rotation);
        double w=Math.Max(1,target.Width),h=Math.Max(1,target.Height);
        var vertices=ShapeStudio.Vertices(target.Kind,target.StarPoints,target.StarInset);
        if(target.Kind==AnnotationKind.Ellipse)vertices=Enumerable.Range(0,96).Select(i=>new AnnotationPoint(.5+.5*Math.Cos(i*Math.PI/48),.5+.5*Math.Sin(i*Math.PI/48))).ToArray();
        else if(target.Kind==AnnotationKind.RoundedRectangle){
            double radius=Math.Min(target.CornerRadius,Math.Min(w,h)/2);var points=new List<AnnotationPoint>();
            foreach(var (cx,cy,offset) in new[]{(w-radius,radius,-90d),(w-radius,h-radius,0d),(radius,h-radius,90d),(radius,radius,180d)})
                for(int i=0;i<=12;i++){double angle=(offset+i*7.5)*Math.PI/180;points.Add(new((cx+radius*Math.Cos(angle))/w,(cy+radius*Math.Sin(angle))/h));}
            vertices=points.ToArray();
        }
        if(vertices.Length==0)vertices=[new(0,0),new(1,0),new(1,1),new(0,1)];
        double best=double.MaxValue;var nearest=new AnnotationPoint();
        for(int i=0;i<vertices.Length;i++){
            var a=vertices[i];var b=vertices[(i+1)%vertices.Length];double ax=target.Left+a.X*w,ay=target.Top+a.Y*h,dx=(b.X-a.X)*w,dy=(b.Y-a.Y)*h;
            double t=Math.Clamp(((p.X-ax)*dx+(p.Y-ay)*dy)/Math.Max(.0001,dx*dx+dy*dy),0,1);double x=ax+t*dx,y=ay+t*dy,d=(p.X-x)*(p.X-x)+(p.Y-y)*(p.Y-y);
            if(d<best){best=d;nearest=new((x-target.Left)/w,(y-target.Top)/h);}
        }
        return new(target.Id,Math.Clamp(nearest.X,0,1),Math.Clamp(nearest.Y,0,1));
    }
    public static (ConnectorAnchor? Link,AnnotationPoint Point) FindAnchor(EditorDocument doc,AnnotationPoint point,double tolerance,string? exclude=null)
    {
        foreach(var target in doc.Annotations.AsEnumerable().Reverse().Where(a=>a.Id!=exclude&&a.Visible&&ShapeStudio.IsClosed(a.Kind))){
            var p=ObjectControl.RotatePoint(point,new(target.Left+target.Width/2,target.Top+target.Height/2),-target.Rotation);
            if(p.X<target.Left-tolerance||p.Y<target.Top-tolerance||p.X>target.Left+target.Width+tolerance||p.Y>target.Top+target.Height+tolerance)continue;
            var link=NearestAnchor(target,point);return (link,AnchorPoint(target,link));
        }return(null,point);
    }
    public static void Resolve(EditorDocument doc)
    {
        if(!doc.Annotations.Any(a=>a.StartAnchor!=null||a.EndAnchor!=null))return;
        var targets=doc.Annotations.Where(a=>ShapeStudio.IsClosed(a.Kind)).ToDictionary(a=>a.Id);
        foreach(var a in doc.Annotations.Where(a=>ShapeStudio.IsLine(a.Kind))){
            if(a.StartAnchor is {} s){if(targets.TryGetValue(s.TargetId,out var target)){var p=AnchorPoint(target,s);a.X1=p.X;a.Y1=p.Y;}else a.StartAnchor=null;}
            if(a.EndAnchor is {} e){if(targets.TryGetValue(e.TargetId,out var target)){var p=AnchorPoint(target,e);a.X2=p.X;a.Y2=p.Y;}else a.EndAnchor=null;}
        }
    }
    public static void DetachForTransform(EditorDocument doc,IEnumerable<string> ids)
    {
        var all=AnnotationEditing.ExpandGroups(doc,ids);
        foreach(var a in doc.Annotations.Where(a=>all.Contains(a.Id)&&ShapeStudio.IsLine(a.Kind))){if(a.StartAnchor is {} s&&!all.Contains(s.TargetId))a.StartAnchor=null;if(a.EndAnchor is {} e&&!all.Contains(e.TargetId))a.EndAnchor=null;}
    }
    public static void Validate(Annotation a,IReadOnlyDictionary<string,Annotation> targets)
    {
        if(!Enum.IsDefined(a.Route)||a.Control is {} c&&(!double.IsFinite(c.X)||!double.IsFinite(c.Y)||Math.Abs(c.X)>1000000||Math.Abs(c.Y)>1000000))throw new InvalidDataException("Điểm điều khiển đường nối không hợp lệ.");
        if(!ShapeStudio.IsLine(a.Kind)&&(a.Route!=ConnectorRoute.Straight||a.Control!=null||a.StartAnchor!=null||a.EndAnchor!=null))throw new InvalidDataException("Liên kết chỉ dành cho đường và mũi tên.");
        foreach(var link in new[]{a.StartAnchor,a.EndAnchor})if(link!=null&&(string.IsNullOrWhiteSpace(link.TargetId)||link.TargetId==a.Id||!targets.TryGetValue(link.TargetId,out var target)||!ShapeStudio.IsClosed(target.Kind)||!double.IsFinite(link.U)||!double.IsFinite(link.V)||link.U<0||link.U>1||link.V<0||link.V>1))throw new InvalidDataException("Liên kết phải trỏ tới biên một shape có thật.");
        if(IsPath(a)&&Math.Abs(a.Rotation)>1e-8)throw new InvalidDataException("Đường nối dùng tọa độ ảnh và không lưu góc xoay riêng.");
    }
}
