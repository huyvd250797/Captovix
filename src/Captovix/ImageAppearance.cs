using System.IO;
namespace Captovix;

// Source image and annotations stay in original pixels. This is an output recipe.
public sealed class ImageAppearance
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] public bool DrawOutsideImage {get;set;}
    public int QuarterTurns {get;set;}
    public bool FlipHorizontal {get;set;}
    public bool FlipVertical {get;set;}
    public int ResizeWidth {get;set;}
    public int ResizeHeight {get;set;}
    public int CanvasWidth {get;set;}
    public int CanvasHeight {get;set;}
    public int Anchor {get;set;}=4;
    public int Padding {get;set;}
    public int BorderWidth {get;set;}
    public string BorderColor {get;set;}="#26364B";
    public bool Transparent {get;set;}=true;
    public string BackgroundColor {get;set;}="#FFFFFF";
    public bool Shadow {get;set;}
    public int ShadowBlur {get;set;}=16;
    public int ShadowX {get;set;}=8;
    public int ShadowY {get;set;}=12;
    public int ShadowOpacity {get;set;}=25;
    public ImageAppearance Copy()=>(ImageAppearance)MemberwiseClone();
    public bool HasEdits=>QuarterTurns!=0||FlipHorizontal||FlipVertical||ResizeWidth!=0||CanvasWidth!=0||Padding!=0||BorderWidth!=0||!Transparent||Shadow;
    public static bool ValidColor(string? color)=>color is {Length:7}&&color[0]=='#'&&color.AsSpan(1).ToString().All(Uri.IsHexDigit);
    public void Validate()
    {
        if(QuarterTurns<0||QuarterTurns>3||ResizeWidth<0||ResizeHeight<0||CanvasWidth<0||CanvasHeight<0||ResizeWidth>32768||ResizeHeight>32768||CanvasWidth>32768||CanvasHeight>32768||(ResizeWidth==0)!=(ResizeHeight==0)||(CanvasWidth==0)!=(CanvasHeight==0)||Anchor<0||Anchor>8||Padding<0||Padding>512||BorderWidth<0||BorderWidth>128||ShadowBlur<0||ShadowBlur>80||Math.Abs((long)ShadowX)>80||Math.Abs((long)ShadowY)>80||ShadowOpacity<0||ShadowOpacity>100||!ValidColor(BorderColor)||!ValidColor(BackgroundColor))throw new InvalidDataException("Thông số biến đổi/khung ảnh không hợp lệ.");
    }
    public void Rotate(int direction)
    {
        QuarterTurns=(QuarterTurns+(direction%4)+4)%4;
        if(Math.Abs(direction%2)==1){(FlipHorizontal,FlipVertical)=(FlipVertical,FlipHorizontal);(ResizeWidth,ResizeHeight)=(ResizeHeight,ResizeWidth);}
    }
}
public readonly record struct ImagePoint(double X,double Y);
public readonly record struct ImageRect(double X,double Y,double Width,double Height)
{
    public double Right=>X+Width;
    public double Bottom=>Y+Height;
    public bool Contains(double x,double y)=>x>=X&&x<=Right&&y>=Y&&y<=Bottom;
}
public sealed class ImageLayout
{
    public int Width {get;}
    public int Height {get;}
    public int ImageWidth {get;}
    public int ImageHeight {get;}
    public ImageRect Frame {get;}
    public ImageRect Canvas {get;}
    public double A {get;}
    public double B {get;}
    public double C {get;}
    public double D {get;}
    public double Tx {get;}
    public double Ty {get;}
    public double ScaleMin=>Math.Min(Math.Sqrt(A*A+B*B),Math.Sqrt(C*C+D*D));
    public ImagePoint ToOutput(double x,double y)=>new(A*x+C*y+Tx,B*x+D*y+Ty);
    public ImagePoint ToSource(double x,double y){var det=A*D-B*C;return new((D*(x-Tx)-C*(y-Ty))/det,(-B*(x-Tx)+A*(y-Ty))/det);}
    public ImagePoint SourceDelta(double x,double y){var start=ToSource(0,0);var end=ToSource(x,y);return new(end.X-start.X,end.Y-start.Y);}
    public bool ContainsImage(PixelRect view,double x,double y){var p=ToSource(x,y);return Canvas.Contains(x,y)&&p.X>=view.X&&p.X<=view.Right&&p.Y>=view.Y&&p.Y<=view.Bottom;}
    public static (int Width,int Height) Natural(PixelRect view,ImageAppearance style)=>style.QuarterTurns%2==0?(view.Width,view.Height):(view.Height,view.Width);
    public ImageLayout(PixelRect view,ImageAppearance s)
    {
        s.Validate();if(view.IsEmpty)throw new InvalidDataException("Vùng ảnh trống.");
        var natural=Natural(view,s);ImageWidth=s.ResizeWidth==0?natural.Width:s.ResizeWidth;ImageHeight=s.ResizeHeight==0?natural.Height:s.ResizeHeight;
        int cw=s.CanvasWidth==0?ImageWidth:s.CanvasWidth,ch=s.CanvasHeight==0?ImageHeight:s.CanvasHeight;
        int left=s.Shadow?Math.Max(0,s.ShadowBlur-s.ShadowX):0,right=s.Shadow?Math.Max(0,s.ShadowBlur+s.ShadowX):0;
        int top=s.Shadow?Math.Max(0,s.ShadowBlur-s.ShadowY):0,bottom=s.Shadow?Math.Max(0,s.ShadowBlur+s.ShadowY):0;
        int inset=s.BorderWidth+s.Padding,fw=cw+2*inset,fh=ch+2*inset;
        Width=fw+left+right;Height=fh+top+bottom;
        if(Width>32768||Height>32768||(long)Width*Height>100_000_000)throw new InvalidDataException("Ảnh đầu ra vượt 32.768 px mỗi chiều hoặc 100 triệu pixel. Hãy giảm kích thước/khung.");
        Frame=new(left,top,fw,fh);Canvas=new(left+inset,top+inset,cw,ch);
        // A half-pixel center offset blurs an otherwise unscaled screenshot.
        // Odd spare pixels are assigned to the right/bottom, keeping all anchors pixel-aligned.
        double ox=Canvas.X+Math.Floor((cw-ImageWidth)*(s.Anchor%3)/2d),oy=Canvas.Y+Math.Floor((ch-ImageHeight)*(s.Anchor/3)/2d);
        ImagePoint Map(double x,double y){double u=x-view.X,v=y-view.Y;var p=s.QuarterTurns switch{1=>new ImagePoint(view.Height-v,u),2=>new(view.Width-u,view.Height-v),3=>new(v,view.Width-u),_=>new(u,v)};
            if(s.FlipHorizontal)p=p with{X=natural.Width-p.X};if(s.FlipVertical)p=p with{Y=natural.Height-p.Y};return new(ox+p.X*ImageWidth/natural.Width,oy+p.Y*ImageHeight/natural.Height);}
        var zero=Map(view.X,view.Y);var xUnit=Map(view.X+1d,view.Y);var yUnit=Map(view.X,view.Y+1d);
        A=xUnit.X-zero.X;B=xUnit.Y-zero.Y;C=yUnit.X-zero.X;D=yUnit.Y-zero.Y;Tx=zero.X-A*view.X-C*view.Y;Ty=zero.Y-B*view.X-D*view.Y;
    }
}
