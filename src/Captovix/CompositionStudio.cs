using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Captovix;
public enum ImageFit { Fit,Fill }
public sealed record LayerImageStyle
{
    public ImageFit Fit {get;set;}
    public PixelRect? Crop {get;set;}
    public double PanX {get;set;}=.5;
    public double PanY {get;set;}=.5;
    public double Radius {get;set;}
    public double Border {get;set;}
    public string BorderColor {get;set;}="#CBD5E1";
    public LayerImageStyle Copy()=>this with{};
    public void Validate(int width,int height){if(!Enum.IsDefined(Fit)||!double.IsFinite(PanX)||!double.IsFinite(PanY)||PanX<0||PanX>1||PanY<0||PanY>1||!double.IsFinite(Radius)||Radius<0||Radius>1000||!double.IsFinite(Border)||Border<0||Border>40||!StudioStyle.Hex(BorderColor)||Crop is {} c&&(c.IsEmpty||c.X<0||c.Y<0||c.Right>width||c.Bottom>height))throw new InvalidDataException("Vùng cắt / kiểu lớp ảnh không hợp lệ.");}
}
public enum CompositionLayout { SideBySide,Stacked,ThreeAcross,ThreeDown,LargeAndTwo,Grid,Free }
public sealed record CompositionOptions
{
    public int Width {get;set;}=1600;
    public int Height {get;set;}=1000;
    public int Margin {get;set;}=40;
    public int Gap {get;set;}=24;
    public string Background {get;set;}="#FFFFFF";
    public CompositionLayout Layout {get;set;}
    public string Title {get;set;}="";
    public string Footer {get;set;}="";
    public bool Captions {get;set;}=true;
    public void Validate(){if(Width<128||Height<128||Width>16000||Height>16000||(long)Width*Height>PixelStudio.MaxPixels||Margin<0||Margin>1000||Gap<0||Gap>1000||!StudioStyle.Hex(Background)||!Enum.IsDefined(Layout)||Title==null||Footer==null||Title.Length>500||Footer.Length>500)throw new InvalidDataException("Canvas 128–16000 px, tối đa 40 MP; đệm/khoảng cách 0–1000 px.");}
}
public static class CompositionStudio
{
    public static List<AnnotationBox> Slots(CompositionOptions o,int count)
    {
        o.Validate();if(count<1||count>16)throw new InvalidDataException("Bố cục hỗ trợ 1–16 ảnh.");
        double x=o.Margin,y=o.Margin+(o.Title.Length>0?76:0),w=o.Width-2*o.Margin,h=o.Height-y-o.Margin-(o.Footer.Length>0?48:0);double cap=o.Captions?42:0;
        var result=new List<AnnotationBox>();int cols=o.Layout switch{CompositionLayout.SideBySide=>2,CompositionLayout.ThreeAcross=>3,CompositionLayout.Stacked or CompositionLayout.ThreeDown=>1,_=>(int)Math.Ceiling(Math.Sqrt(count))};int rows=(count+cols-1)/cols;
        if(o.Layout==CompositionLayout.LargeAndTwo&&count==3){double cw=(w-o.Gap)/2,ch=(h-o.Gap)/2;result.Add(new(x,y,cw,h-cap));result.Add(new(x+cw+o.Gap,y,cw,ch-cap));result.Add(new(x+cw+o.Gap,y+ch+o.Gap,cw,ch-cap));}
        else{double cw=(w-o.Gap*(cols-1))/cols,ch=(h-o.Gap*(rows-1))/rows;for(int i=0;i<count;i++)result.Add(new(x+i%cols*(cw+o.Gap),y+i/cols*(ch+o.Gap),cw,ch-cap));}
        if(result.Any(b=>b.Width<32||b.Height<32))throw new InvalidDataException("Canvas quá nhỏ cho bố cục. Giảm đệm/khoảng cách hoặc tăng canvas.");return result;
    }
    public static void Apply(EditorDocument d,CompositionOptions options)
    {
        if(d.Crop!=null)throw new InvalidDataException("Khôi phục crop toàn ảnh trước khi sắp xếp lại bố cục.");
        var images=d.Annotations.Where(a=>a.Kind==AnnotationKind.ImageLayer).ToList();if(!ObjectControl.CanEdit(d,images.Select(a=>a.Id)))throw new InvalidDataException("Hiện và mở khóa các ảnh/nhóm trước khi áp dụng bố cục.");var slots=Slots(options,images.Count);
        if(options.Width!=d.OriginalWidth||options.Height!=d.OriginalHeight)throw new InvalidDataException("Kích thước bố cục phải khớp canvas hiện tại.");
        var captions=d.Annotations.Where(a=>a.Name=="@composition:caption").Select(a=>a.Text).ToArray();
        d.Annotations.RemoveAll(a=>a.Name.StartsWith("@composition:",StringComparison.Ordinal));
        for(int i=0;i<images.Count;i++){var a=images[i];var b=slots[i];if(options.Layout!=CompositionLayout.Free){a.X1=b.X;a.Y1=b.Y;a.X2=b.Right;a.Y2=b.Bottom;a.Rotation=0;}a.ImageStyle??=new();
            if(options.Captions){var group=a.GroupId??=Guid.NewGuid().ToString("N");d.Annotations.Add(Text("caption",a.Left,a.Top+a.Height+10,Math.Max(32,a.Width),i<captions.Length?captions[i]:$"Ảnh {i+1}",22,group));}}
        if(options.Title.Length>0)d.Annotations.Add(Text("title",options.Margin,options.Margin,options.Width-2*options.Margin,options.Title,36));
        if(options.Footer.Length>0)d.Annotations.Add(Text("footer",options.Margin,options.Height-options.Margin-34,options.Width-2*options.Margin,options.Footer,18));
        d.Appearance.BackgroundColor=options.Background;d.Appearance.Transparent=false;
    }
    private static Annotation Text(string name,double x,double y,double width,string text,double size,string? group=null)=>new(){Kind=AnnotationKind.Text,Name="@composition:"+name,X1=x,Y1=y,X2=x+width,Y2=y+size*1.5,Text=text,TextWidth=width,FontSize=size,TextColor="#26364B",TextPadding=0,GroupId=group,Bold=name=="title"};
    public static AnnotationBox ImagePlacement(AnnotationBox slot,int width,int height,LayerImageStyle style)
    {
        style.Validate(width,height);if(slot.Width<=0||slot.Height<=0||width<1||height<1)throw new InvalidDataException("Kích thước ảnh không hợp lệ.");var crop=style.Crop??new(0,0,width,height);double scale=style.Fit==ImageFit.Fit?Math.Min(slot.Width/crop.Width,slot.Height/crop.Height):Math.Max(slot.Width/crop.Width,slot.Height/crop.Height);double w=crop.Width*scale,h=crop.Height*scale;return new(slot.X+(slot.Width-w)*style.PanX,slot.Y+(slot.Height-h)*style.PanY,w,h);
    }
    public static bool SameFamily(AnnotationKind active,AnnotationKind favorite)=>active==AnnotationKind.Callout?favorite==AnnotationKind.Callout:ShapeStudio.IsClosed(active)?ShapeStudio.IsClosed(favorite):ShapeStudio.IsLine(active)&&ShapeStudio.IsLine(favorite);
}
public sealed class CompositionTemplate
{
    public int Version {get;set;}=1;
    public int Width {get;set;}
    public int Height {get;set;}
    public PixelRect? Crop {get;set;}
    public ImageAppearance Appearance {get;set;}=new();
    public List<Annotation> Objects {get;set;}=[];
    public void Validate(){if(Version!=1||Width<128||Height<128||Width>16000||Height>16000||(long)Width*Height>PixelStudio.MaxPixels||Objects==null||Objects.Count>1000||Objects.Any(a=>a==null||!Enum.IsDefined(a.Kind)||a.PixelPng!=null))throw new InvalidDataException("Mẫu bố cục không hợp lệ hoặc chứa ảnh công việc.");}
    public static CompositionTemplate From(EditorDocument d){var t=new CompositionTemplate{Width=d.OriginalWidth,Height=d.OriginalHeight,Crop=d.Crop,Appearance=d.Appearance.Copy(),Objects=d.Annotations.Select(a=>a.Copy()).ToList()};foreach(var a in t.Objects){a.PixelPng=null;if(a.Kind==AnnotationKind.ImageLayer){a.ImagePlaceholder=true;a.ImageStyle??=new();a.ImageStyle.Crop=null;a.Name="Ô ảnh";}else if(a.Kind is AnnotationKind.Text or AnnotationKind.Callout)a.Text=a.Name.Contains("title")?"Tiêu đề":a.Name.Contains("footer")?"Ghi chú":"Chú thích";}t.Validate();return t;}
}
public static class CompositionProject
{
    public static void Save(string path,byte[] png,EditorDocument source)
    {
        var size=PixelStudio.PngSize(Convert.ToBase64String(png));var d=source.Clone();d.SourceHash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(png));d.Validate(size.Width,size.Height,d.SourceHash);string json=d.Serialize();if(System.Text.Encoding.UTF8.GetByteCount(json)>140_000_000)throw new InvalidDataException("Tài liệu dự án vượt giới hạn 140 MB.");
        string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{using(var zip=System.IO.Compression.ZipFile.Open(temp,System.IO.Compression.ZipArchiveMode.Create)){using(var stream=zip.CreateEntry("original.png").Open())stream.Write(png);using var writer=new StreamWriter(zip.CreateEntry("document.json").Open());writer.Write(json);}File.Move(temp,path,true);}finally{if(File.Exists(temp))File.Delete(temp);}
    }
    public static (byte[] Original,EditorDocument Document) Load(string path)
    {
        using var zip=System.IO.Compression.ZipFile.OpenRead(path);if(zip.Entries.Count!=2||zip.Entries.Any(e=>e.FullName is not ("original.png" or "document.json"))||zip.Entries.Select(e=>e.FullName).Distinct().Count()!=2)throw new InvalidDataException("Cấu trúc dự án không hợp lệ.");
        byte[] Read(string name,int max){var entry=zip.GetEntry(name)!;if(entry.Length>max)throw new InvalidDataException("Dự án vượt giới hạn.");using var input=entry.Open();using var output=new MemoryStream();var buffer=new byte[65536];int n;while((n=input.Read(buffer))>0){if(output.Length+n>max)throw new InvalidDataException("Dữ liệu vượt giới hạn.");output.Write(buffer,0,n);}return output.ToArray();}
        var png=Read("original.png",72_000_000);var size=PixelStudio.PngSize(Convert.ToBase64String(png));var hash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(png));var doc=EditorDocument.Deserialize(System.Text.Encoding.UTF8.GetString(Read("document.json",140_000_000)));doc.Validate(size.Width,size.Height,hash);return(png,doc);
    }
}
