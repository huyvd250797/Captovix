using System.IO;
namespace Captovix;

// Pure pixel math: no screen DPI, viewport zoom or WPF units enter the file pipeline.
public static class ImageFidelity
{
    public static double ActualZoom(double dpiScale)=>1/ValidScale(dpiScale);
    public static double PhysicalZoom(double zoom,double dpiScale)=>zoom*ValidScale(dpiScale);
    public static double MoveZoom(double zoom,double oldScale,double newScale)=>PhysicalZoom(zoom,oldScale)/ValidScale(newScale);
    private static double ValidScale(double value)=>double.IsFinite(value)&&value>0?value:throw new ArgumentOutOfRangeException(nameof(value));
    public static bool IsIntegerZoom(double physicalZoom)=>physicalZoom>=1&&Math.Abs(physicalZoom-Math.Round(physicalZoom))<.000001;
    private static bool Integer(double n)=>double.IsFinite(n)&&Math.Abs(n-Math.Round(n))<.000001;
    public static bool CanCopyPixels(ImageLayout layout)=>Integer(layout.A)&&Integer(layout.B)&&Integer(layout.C)&&Integer(layout.D)&&Integer(layout.Tx)&&Integer(layout.Ty)&&Math.Abs(layout.A)+Math.Abs(layout.B)==1&&Math.Abs(layout.C)+Math.Abs(layout.D)==1&&Math.Abs(layout.A*layout.D-layout.B*layout.C)==1;
    public static void ValidateCapture(PixelRect requested,int width,int height,bool window)
    {
        if(width<1||height<1||width>32768||height>32768||(long)width*height>100_000_000)throw new InvalidDataException("Ảnh chụp vượt giới hạn kích thước an toàn.");
        if(!window&&(width!=requested.Width||height!=requested.Height))throw new InvalidDataException($"Engine trả {width} × {height} px, khác vùng pixel đã chọn {requested.Width} × {requested.Height}. File pending được giữ để kiểm tra; app không tự resize để che lỗi.");
    }
    public static byte[] CopyPixels(byte[] source,int width,int height,PixelRect view,ImageLayout layout,byte[]? background=null)
    {
        if(width<1||height<1||source.LongLength!=(long)width*height*4||view.X<0||view.Y<0||view.Width<1||view.Height<1||(long)view.X+view.Width>width||(long)view.Y+view.Height>height||!CanCopyPixels(layout))throw new ArgumentException("Không thể copy pixel với hình học hoặc dữ liệu nguồn này.");
        var output=background??new byte[checked(layout.Width*layout.Height*4)];if(output.LongLength!=(long)layout.Width*layout.Height*4)throw new ArgumentException("Sai kích thước nền.");
        var start=layout.ToOutput(view.X+.5,view.Y+.5);int x0=(int)Math.Floor(start.X),y0=(int)Math.Floor(start.Y);int a=(int)Math.Round(layout.A),b=(int)Math.Round(layout.B),c=(int)Math.Round(layout.C),d=(int)Math.Round(layout.D);
        if(background==null&&a==1&&b==0&&c==0){
            int left=Math.Max(x0,Math.Max(0,(int)layout.Canvas.X)),right=Math.Min(x0+view.Width,Math.Min(layout.Width,(int)layout.Canvas.Right));
            for(int row=0;row<view.Height;row++){int y=y0+d*row;if(right<=left||y<0||y>=layout.Height||y<layout.Canvas.Y||y>=layout.Canvas.Bottom)continue;Array.Copy(source,((view.Y+row)*width+view.X+left-x0)*4,output,(y*layout.Width+left)*4,(right-left)*4);}
            return output;
        }
        for(int row=0;row<view.Height;row++)for(int col=0;col<view.Width;col++){
            int x=x0+a*col+c*row,y=y0+b*col+d*row;
            if(x<0||y<0||x>=layout.Width||y>=layout.Height||x<layout.Canvas.X||y<layout.Canvas.Y||x>=layout.Canvas.Right||y>=layout.Canvas.Bottom)continue;
            int si=((view.Y+row)*width+view.X+col)*4,di=(y*layout.Width+x)*4;
            BlendStraight(source,si,output,di);
        }
        return output;
    }
    private static void BlendStraight(byte[] source,int si,byte[] dest,int di)
    {
        int sa=source[si+3],da=dest[di+3];
        if(sa==255||da==0){Array.Copy(source,si,dest,di,4);return;}if(sa==0)return;
        int denominator=sa*255+da*(255-sa);
        for(int channel=0;channel<3;channel++)dest[di+channel]=(byte)((source[si+channel]*sa*255+dest[di+channel]*da*(255-sa)+denominator/2)/denominator);
        dest[di+3]=(byte)((denominator+127)/255);
    }
    public static void OverlayPremultiplied(byte[] destination,byte[] overlay)
    {
        if(destination.Length!=overlay.Length||destination.Length%4!=0)throw new ArgumentException("Sai kích thước lớp ảnh.");
        for(int i=0;i<destination.Length;i+=4){
            int sa=overlay[i+3];if(sa==0)continue;int da=destination[i+3],denominator=sa*255+da*(255-sa);
            for(int c=0;c<3;c++)destination[i+c]=(byte)Math.Clamp((overlay[i+c]*255*255+destination[i+c]*da*(255-sa)+denominator/2)/denominator,0,255);
            destination[i+3]=(byte)((denominator+127)/255);
        }
    }
    public static byte[] WhiteRgb(byte[] bgra)
    {
        if(bgra.Length%4!=0)throw new ArgumentException("Sai dữ liệu BGRA.");var bgr=new byte[checked(bgra.Length/4*3)];
        for(int i=0,j=0;i<bgra.Length;i+=4,j+=3)for(int c=0;c<3;c++)bgr[j+c]=(byte)((bgra[i+c]*bgra[i+3]+255*(255-bgra[i+3])+127)/255);
        return bgr;
    }
}
public sealed record CapturePixelEvidence(int RequestedWidth,int RequestedHeight,int ActualWidth,int ActualHeight,string Backend,string ColorAssessment="SDR8 pipeline; HDR/wide gamut not assessed");
