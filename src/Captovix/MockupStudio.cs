using System.IO;
namespace Captovix;
public sealed record MockupColors(string Background,string Fill,string Foreground,double Flatness);
public static class MockupStudio
{
    public static PixelRect Region(double x1,double y1,double x2,double y2,PixelRect view)=>CaptureSelection.Clip(OcrContract.Selection(x1,y1,x2,y2,Math.Max(1,view.Right),Math.Max(1,view.Bottom)),view);
    public static Annotation Create(PixelRect region,PixelRect image,MockupColors colors)
    {
        if(region.Width<8||region.Height<8||CaptureSelection.Clip(region,image)!=region)throw new InvalidDataException("Chọn vùng chữ/nút từ 8 × 8 px, nằm trong ảnh.");
        return new(){Kind=AnnotationKind.Rectangle,Name="Mockup · Chữ",X1=region.X,Y1=region.Y,X2=region.Right,Y2=region.Bottom,Mockup=new(){SourceBounds=region,Background=colors.Background},FillEnabled=true,FillColor=colors.Fill,TextColor=colors.Foreground,Color=colors.Foreground,Stroke=1,StrokeEnabled=false,CornerRadius=4,FontSize=Math.Clamp(region.Height*.65,8,72),TextPadding=0,TextAlignment=TextAlign.Left,VerticalAlignment=TextVertical.Center,AutoFitText=false};
    }
    public static void Apply(EditorDocument doc,Annotation value,string? editingId=null)
    {
        var next=value.Copy();next.Mockup?.Validate(next,doc.OriginalWidth,doc.OriginalHeight);
        if(next.Mockup==null)throw new InvalidDataException("Thiếu thông tin Mockup.");
        if(editingId==null){if(doc.Annotations.Count>=10000)throw new InvalidDataException("Đã đạt giới hạn đối tượng.");doc.Annotations.Add(next);}
        else{int at=doc.Annotations.FindIndex(a=>a.Id==editingId);if(at<0||doc.Annotations[at].Mockup==null||doc.Annotations[at].Locked||!doc.Annotations[at].Visible)throw new InvalidOperationException("Đối tượng đã thay đổi, bị khóa hoặc không còn trong ảnh.");next.Id=editingId;doc.Annotations[at]=next;}
        doc.Version=18;doc.Validate(doc.OriginalWidth,doc.OriginalHeight,doc.SourceHash);
    }
    public static MockupColors Colors(byte[] bgra,int width,int height,PixelRect selected)
    {
        if(width<1||height<1||bgra.LongLength!=(long)width*height*4||selected.IsEmpty||CaptureSelection.Clip(selected,new(0,0,width,height))!=selected)throw new ArgumentException("Sai dữ liệu màu xem trước.");
        static string Hex(int v)=>$"#{v:X6}";
        int Pixel(int x,int y){int n=(y*width+x)*4;int a=bgra[n+3];return ((bgra[n+2]*a+255*(255-a))/255)<<16|((bgra[n+1]*a+255*(255-a))/255)<<8|(bgra[n]*a+255*(255-a))/255;}
        var outside=new Dictionary<int,int>();var inside=new Dictionary<int,int>();
        void Add(Dictionary<int,int> map,int color){map[color]=map.GetValueOrDefault(color)+1;}
        int step=Math.Max(1,(int)Math.Sqrt((long)width*height/40000d));
        for(int y=0;y<height;y+=step)for(int x=0;x<width;x+=step){bool within=x>=selected.X&&x<selected.Right&&y>=selected.Y&&y<selected.Bottom;Add(within?inside:outside,Pixel(x,y));}
        var ring=outside.Count>0?outside:inside;int background=ring.OrderByDescending(p=>p.Value).First().Key;double flatness=ring[background]/(double)ring.Values.Sum();
        int fill=inside.OrderByDescending(p=>p.Value).First().Key;int foreground=inside.Where(p=>Distance(p.Key,fill)>70).OrderByDescending(p=>p.Value).Select(p=>p.Key).FirstOrDefault((fill>>16&255)*.299+(fill>>8&255)*.587+(fill&255)*.114>150?0:0xffffff);
        return new(Hex(background),Hex(fill),Hex(foreground),flatness);
    }
    private static double Distance(int a,int b)=>Math.Abs((a>>16&255)-(b>>16&255))+Math.Abs((a>>8&255)-(b>>8&255))+Math.Abs((a&255)-(b&255));
}
