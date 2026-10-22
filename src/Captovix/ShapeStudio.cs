using System.IO;
namespace Captovix;
public enum StrokePattern { Solid, Dash, Dot, DashDot }
public enum ArrowTip { Triangle, Open, Diamond, Circle }
public static class ShapeStudio
{
    public static readonly AnnotationKind[] Shapes=[AnnotationKind.Rectangle,AnnotationKind.RoundedRectangle,AnnotationKind.Ellipse,AnnotationKind.Triangle,AnnotationKind.Diamond,AnnotationKind.Trapezoid,AnnotationKind.Pentagon,AnnotationKind.Hexagon,AnnotationKind.Star];
    public static bool IsClosed(AnnotationKind kind)=>Array.IndexOf(Shapes,kind)>=0;
    public static bool IsLine(AnnotationKind kind)=>kind is AnnotationKind.Line or AnnotationKind.Arrow;
    public static bool HasStroke(AnnotationKind kind)=>IsClosed(kind)||IsLine(kind)||kind==AnnotationKind.Freehand;
    public static string Label(AnnotationKind kind)=>kind switch{AnnotationKind.Rectangle=>"Chữ nhật",AnnotationKind.RoundedRectangle=>"Chữ nhật bo góc",AnnotationKind.Ellipse=>"Ellipse / hình tròn",AnnotationKind.Triangle=>"Tam giác",AnnotationKind.Diamond=>"Hình thoi",AnnotationKind.Trapezoid=>"Hình thang",AnnotationKind.Pentagon=>"Ngũ giác",AnnotationKind.Hexagon=>"Lục giác",AnnotationKind.Star=>"Ngôi sao",AnnotationKind.Line=>"Đường thẳng",AnnotationKind.Arrow=>"Mũi tên",AnnotationKind.Text=>"Chữ",AnnotationKind.Highlight=>"Đánh dấu",AnnotationKind.Magnifier=>"Kính phóng đại",AnnotationKind.Spotlight=>"Spotlight",AnnotationKind.Step=>"Số bước",AnnotationKind.Blur=>"Làm mờ",AnnotationKind.Pixelate=>"Pixel hóa",AnnotationKind.Cover=>"Che kín",AnnotationKind.Callout=>"Hộp chú thích",AnnotationKind.Freehand=>"Nét tự do",_=>kind.ToString()};
    public static void ValidateStyle(Annotation a)
    {
        static bool Color(string? s)=>s is {Length:7}&&s[0]=='#'&&s.AsSpan(1).ToString().All(Uri.IsHexDigit);
        if(!Color(a.FillColor)||!Enum.IsDefined(a.Dash)||!Enum.IsDefined(a.Tip)||!double.IsFinite(a.StrokeOpacity)||a.StrokeOpacity<0||a.StrokeOpacity>100||!double.IsFinite(a.FillOpacity)||a.FillOpacity<0||a.FillOpacity>100||!double.IsFinite(a.CornerRadius)||a.CornerRadius<0||a.CornerRadius>1000||a.StarPoints<3||a.StarPoints>20||!double.IsFinite(a.StarInset)||a.StarInset<.1||a.StarInset>.9||!double.IsFinite(a.ArrowHeadSize)||a.ArrowHeadSize<0||a.ArrowHeadSize>160)throw new InvalidDataException("Thuộc tính hình dạng, màu nền hoặc kiểu nét không hợp lệ.");
    }
    public static void CopyStyle(Annotation from,Annotation to)
    {
        to.Shadow=from.Shadow.Copy();
        to.FillColor=from.FillColor;to.FillEnabled=from.FillEnabled;to.StrokeEnabled=from.StrokeEnabled;
        to.StrokeOpacity=from.StrokeOpacity;to.FillOpacity=from.FillOpacity;to.Dash=from.Dash;to.CornerRadius=from.CornerRadius;to.StarPoints=from.StarPoints;to.StarInset=from.StarInset;to.Tip=from.Tip;to.DoubleArrow=from.DoubleArrow;to.ArrowHeadSize=from.ArrowHeadSize;
    }
    // Normalized vertices: rendering and hit testing build the same geometry.
    public static AnnotationPoint[] Vertices(AnnotationKind kind,int starPoints=5,double inset=.45)
    {
        if(kind==AnnotationKind.Triangle)return [new(.5,0),new(1,1),new(0,1)];
        if(kind==AnnotationKind.Diamond)return [new(.5,0),new(1,.5),new(.5,1),new(0,.5)];
        if(kind==AnnotationKind.Trapezoid)return [new(.2,0),new(.8,0),new(1,1),new(0,1)];
        int n=kind switch{AnnotationKind.Pentagon=>5,AnnotationKind.Hexagon=>6,AnnotationKind.Star=>Math.Clamp(starPoints,3,20)*2,_=>0};
        var raw=new AnnotationPoint[n];for(int i=0;i<n;i++){double angle=-Math.PI/2+i*2*Math.PI/n, radius=kind==AnnotationKind.Star&&i%2==1?Math.Clamp(inset,.1,.9):1;raw[i]=new(Math.Cos(angle)*radius,Math.Sin(angle)*radius);}
        if(n==0)return raw;
        double minX=raw.Min(p=>p.X),minY=raw.Min(p=>p.Y),w=raw.Max(p=>p.X)-minX,h=raw.Max(p=>p.Y)-minY;
        return raw.Select(p=>new AnnotationPoint((p.X-minX)/w,(p.Y-minY)/h)).ToArray();
    }
    public static AnnotationPoint ConstrainedEnd(AnnotationPoint start,AnnotationPoint end,PixelRect view)
    {
        double dx=end.X-start.X,dy=end.Y-start.Y,angle=Math.Round(Math.Atan2(dy,dx)/(Math.PI/4))*Math.PI/4,ux=Math.Cos(angle),uy=Math.Sin(angle),length=Math.Sqrt(dx*dx+dy*dy);
        if(Math.Abs(ux)>1e-10)length=Math.Min(length,Math.Max(0,((ux>0?view.Right:view.X)-start.X)/ux));
        if(Math.Abs(uy)>1e-10)length=Math.Min(length,Math.Max(0,((uy>0?view.Bottom:view.Y)-start.Y)/uy));
        return new(Math.Clamp(start.X+ux*length,view.X,view.Right),Math.Clamp(start.Y+uy*length,view.Y,view.Bottom));
    }
    public static double[] Dashes(StrokePattern dash)=>dash switch{StrokePattern.Dash=>[4,3],StrokePattern.Dot=>[.01,2.5],StrokePattern.DashDot=>[4,2,.01,2],_=>[]};
    public static double HeadSize(Annotation a,double length)=>Math.Min(a.ArrowHeadSize>0?a.ArrowHeadSize:Math.Max(12,a.Stroke*4),length*(a.DoubleArrow ? .4 : .5));
}
