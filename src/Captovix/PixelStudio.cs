using System.IO;
using System.Buffers.Binary;
namespace Captovix;
public enum PixelSelectionKind { Rectangle,Ellipse,Lasso }
public sealed class PixelSelection
{
    public PixelSelectionKind Kind {get;set;}
    public List<AnnotationPoint> Points {get;set;}=[];
    public PixelRect Bounds {get;set;}
    public bool Contains(double x,double y)
    {
        var b=Bounds;if(x<b.X||y<b.Y||x>=b.Right||y>=b.Bottom)return false;
        if(Kind==PixelSelectionKind.Rectangle)return true;
        if(Kind==PixelSelectionKind.Ellipse){double dx=(x-b.X-b.Width/2d)/(b.Width/2d),dy=(y-b.Y-b.Height/2d)/(b.Height/2d);return dx*dx+dy*dy<=1;}
        bool inside=false;for(int i=0,j=Points.Count-1;i<Points.Count;j=i++){var a=Points[i];var c=Points[j];if((a.Y>y)!=(c.Y>y)&&x<(c.X-a.X)*(y-a.Y)/(c.Y-a.Y)+a.X)inside=!inside;}return inside;
    }
    public PixelSelection Copy()=>new(){Kind=Kind,Bounds=Bounds,Points=[..Points]};
    public void Translate(double dx,double dy,PixelRect view){int x=(int)Math.Round(Math.Clamp(Bounds.X+dx,view.X,view.Right-Bounds.Width))-Bounds.X,y=(int)Math.Round(Math.Clamp(Bounds.Y+dy,view.Y,view.Bottom-Bounds.Height))-Bounds.Y;Bounds=Bounds with{X=Bounds.X+x,Y=Bounds.Y+y};Points=Points.Select(p=>new AnnotationPoint(p.X+x,p.Y+y)).ToList();}
    public void Validate(PixelRect view){if(!Enum.IsDefined(Kind)||!CropGeometry.IsValid(Bounds,view)||Points==null||Points.Count>8192||Kind==PixelSelectionKind.Lasso&&(Points.Count<3||Points.Any(p=>!double.IsFinite(p.X)||!double.IsFinite(p.Y))))throw new ArgumentException("Chọn vùng từ 2 × 2 px; lasso cần đường bao ít nhất 3 điểm.");}
}
public static class PixelStudio
{
    public const int MaxEncoded=96_000_000;
    public const long MaxPixels=40_000_000;
    public static (int Width,int Height) PngSize(string data)
    {
        if(string.IsNullOrEmpty(data)||data.Length>MaxEncoded)throw new InvalidDataException("Lớp pixel vượt giới hạn PNG 72 MB.");
        Span<byte> head=stackalloc byte[33];if(data.Length<44||!Convert.TryFromBase64Chars(data.AsSpan(0,44),head,out int count)||count<24||!head[..8].SequenceEqual(new byte[]{137,80,78,71,13,10,26,10})||!head.Slice(12,4).SequenceEqual("IHDR"u8))throw new InvalidDataException("Dữ liệu lớp pixel không phải PNG hợp lệ.");
        int w=BinaryPrimitives.ReadInt32BigEndian(head.Slice(16,4)),h=BinaryPrimitives.ReadInt32BigEndian(head.Slice(20,4));if(w<1||h<1||w>32768||h>32768||(long)w*h>MaxPixels)throw new InvalidDataException("Chỉnh pixel hỗ trợ tối đa 40 triệu pixel mỗi ảnh.");return(w,h);
    }
    public static void Validate(EditorDocument doc)
    {
        long total=0;if(doc.PixelBasePng!=null){var size=PngSize(doc.PixelBasePng);if(size!=(doc.OriginalWidth,doc.OriginalHeight))throw new InvalidDataException("Nền pixel phải cùng số pixel với ảnh gốc.");total+=doc.PixelBasePng.Length;}
        foreach(var a in doc.Annotations){if(a.Kind==AnnotationKind.ImageLayer){if(a.PixelPng==null||a.Width<1||a.Height<1)throw new InvalidDataException("Lớp ảnh thiếu dữ liệu hoặc kích thước.");PngSize(a.PixelPng);total+=a.PixelPng.Length;}else if(a.PixelPng!=null)throw new InvalidDataException("Chỉ lớp ảnh được chứa bitmap.");}
        if(total>128_000_000)throw new InvalidDataException("Tài liệu pixel quá lớn (tối đa 128 triệu ký tự PNG mã hóa).");
    }
    // Scanline intervals keep lasso work proportional to rows × edges, not pixels × edges.
    private static IEnumerable<(int Y,int Left,int Right)> Spans(PixelSelection selection)
    {
        var b=selection.Bounds;
        for(int y=b.Y;y<b.Bottom;y++){
            if(selection.Kind==PixelSelectionKind.Rectangle){yield return(y,b.X,b.Right);continue;}
            if(selection.Kind==PixelSelectionKind.Ellipse){double dy=(y+.5-b.Y-b.Height/2d)/(b.Height/2d),half=b.Width/2d*Math.Sqrt(Math.Max(0,1-dy*dy)),cx=b.X+b.Width/2d;int left=Math.Max(b.X,(int)Math.Ceiling(cx-half-.5)),right=Math.Min(b.Right,(int)Math.Floor(cx+half-.5)+1);if(right>left)yield return(y,left,right);continue;}
            var crossings=new List<double>();double scan=y+.5;var points=selection.Points;
            for(int i=0,j=points.Count-1;i<points.Count;j=i++){var a=points[i];var c=points[j];if((a.Y>scan)!=(c.Y>scan))crossings.Add(a.X+(scan-a.Y)*(c.X-a.X)/(c.Y-a.Y));}
            crossings.Sort();for(int i=0;i+1<crossings.Count;i+=2){int left=(int)Math.Ceiling(Math.Clamp(crossings[i]-.5,b.X,b.Right)),right=(int)Math.Ceiling(Math.Clamp(crossings[i+1]-.5,b.X,b.Right));if(right>left)yield return(y,left,right);}
        }
    }
    public static byte[] Edit(byte[] original,int width,int height,PixelSelection selection,byte[]? color)
    {
        selection.Validate(new(0,0,width,height));if((long)width*height>MaxPixels||original.Length!=(long)width*height*4||color!=null&&color.Length!=4)throw new ArgumentException("Dữ liệu pixel không hợp lệ.");var result=(byte[])original.Clone();
        foreach(var row in Spans(selection)){int start=(row.Y*width+row.Left)*4,bytes=(row.Right-row.Left)*4;if(color==null)Array.Clear(result,start,bytes);else for(int i=start;i<start+bytes;i+=4)Array.Copy(color,0,result,i,4);}return result;
    }
    public static byte[] Extract(byte[] source,int width,int height,PixelSelection selection)
    {
        selection.Validate(new(0,0,width,height));if(source.Length!=(long)width*height*4||(long)width*height>MaxPixels)throw new ArgumentException("Dữ liệu pixel không khớp.");var b=selection.Bounds;var result=new byte[checked(b.Width*b.Height*4)];
        foreach(var row in Spans(selection))Array.Copy(source,(row.Y*width+row.Left)*4,result,((row.Y-b.Y)*b.Width+row.Left-b.X)*4,(row.Right-row.Left)*4);return result;
    }
}
