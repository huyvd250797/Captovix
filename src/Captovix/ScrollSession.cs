using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public sealed class ScrollJoin
{
    public int Overlap {get;set;}
    public bool Uncertain {get;set;}
    public bool Confirmed {get;set;}
    public string Message {get;set;}="";
}
public sealed class ScrollSession
{
    public int Schema {get;set;}=1;
    public int Width {get;set;}
    public int Height {get;set;}
    public int Count {get;set;}
    public string Topology {get;set;}="";
    public PixelRect Region {get;set;}
    public List<ScrollJoin> Joins {get;set;}=[];
    public List<string> Hashes {get;set;}=[];
    [System.Text.Json.Serialization.JsonIgnore] public string Folder {get;set;}="";
    public static string Root=>Path.Combine(App.DataRoot,"ScrollSessions");
    public string FramePath(int index)=>Path.Combine(Folder,$"frame-{index:D3}.png");
    public void Save()=>MediaStore.AtomicWrite(Path.Combine(Folder,"session.json"),JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));
    public static ScrollSession Create(PixelRect region,string topology){var s=new ScrollSession{Width=region.Width,Height=region.Height,Region=region,Topology=topology,Folder=Path.Combine(Root,DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")[..6])};Directory.CreateDirectory(s.Folder);s.Save();return s;}
    public static ScrollSession Open(string path)
    {
        var info=new FileInfo(path);if(!info.Exists||info.Length>128*1024||(info.Attributes&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("File phiên không hợp lệ.");
        var s=JsonSerializer.Deserialize<ScrollSession>(File.ReadAllText(path))??throw new InvalidDataException("Không đọc được phiên.");s.Folder=Path.GetDirectoryName(Path.GetFullPath(path))!;
        if((File.GetAttributes(s.Folder)&FileAttributes.ReparsePoint)!=0||s.Schema!=1||s.Count<1||s.Count>ScrollStitching.MaxFrames||s.Joins==null||s.Joins.Count!=s.Count-1||s.Hashes==null||s.Hashes.Count!=s.Count)throw new InvalidDataException("Phiên chưa có khung hoàn chỉnh hoặc định dạng không hỗ trợ.");
        ScrollStitching.OutputHeight(s.Width,s.Height,s.Joins.Select(j=>j.Overlap).ToArray());for(int i=0;i<s.Count;i++){var frame=new FileInfo(s.FramePath(i));if(!frame.Exists||frame.Length>20_000_000||(frame.Attributes&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Thiếu hoặc vượt giới hạn khung nguồn.");}return s;
    }
    public ScrollFrame Load(int index)
    {
        if(index<0||index>=Count)throw new ArgumentOutOfRangeException(nameof(index));if(Hash(FramePath(index))!=Hashes[index])throw new InvalidDataException("Khung nguồn đã bị thay đổi; không tiếp tục ghép dữ liệu không khớp.");return ReadFrame(FramePath(index),Width,Height);
    }
    public static string Hash(string path){using var stream=File.OpenRead(path);return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream));}
    public void Append(string candidate,ScrollMatch? match)
    {
        int prior=Count;string destination=FramePath(prior);bool moved=false;
        try{
            if(File.Exists(destination))File.Move(destination,Path.Combine(Folder,"uncommitted-"+Guid.NewGuid().ToString("N")+".png"));
            File.Move(candidate,destination);moved=true;Hashes.Add(Hash(destination));if(match!=null)Joins.Add(new(){Overlap=match.Overlap,Uncertain=match.Kind==ScrollMatchKind.Uncertain,Message=match.Message});Count++;Save();
        }catch{Count=prior;while(Hashes.Count>prior)Hashes.RemoveAt(Hashes.Count-1);while(Joins.Count>Math.Max(0,prior-1))Joins.RemoveAt(Joins.Count-1);if(moved&&File.Exists(destination))File.Move(destination,candidate);throw;}
    }
    public static ScrollFrame ReadFrame(string path,int width,int height)
    {
        if(width<8||height<32||(long)width*height>ScrollStitching.MaxFramePixels)throw new InvalidDataException("Kích thước nguồn vượt giới hạn.");
        using var stream=File.OpenRead(path);Span<byte> header=stackalloc byte[24];stream.ReadExactly(header);
        if(!header[..8].SequenceEqual(new byte[]{137,80,78,71,13,10,26,10})||System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(header.Slice(8,4))!=13||!header.Slice(12,4).SequenceEqual("IHDR"u8)||System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(header.Slice(16,4))!=width||System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(header.Slice(20,4))!=height)throw new InvalidDataException("Header PNG không khớp kích thước phiên; không giải mã ảnh vượt giới hạn.");
        stream.Position=0;var decoder=BitmapDecoder.Create(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);var raw=decoder.Frames[0];
        if(raw.PixelWidth!=width||raw.PixelHeight!=height||(long)width*height>ScrollStitching.MaxFramePixels)throw new InvalidDataException("Kích thước khung nguồn đã thay đổi.");
        var source=raw.Format==PixelFormats.Bgra32?(BitmapSource)raw:new FormatConvertedBitmap(raw,PixelFormats.Bgra32,null,0);var pixels=new byte[checked(width*height*4)];source.CopyPixels(pixels,width*4,0);var f=new ScrollFrame(width,height,pixels);f.Validate();return f;
    }
    public BitmapSource Compose(CancellationToken cancellation=default)
    {
        int[] joins=Joins.Select(j=>j.Overlap).ToArray();var pixels=ScrollStitching.Compose(Width,Height,joins,Load,cancellation);var bitmap=BitmapSource.Create(Width,ScrollStitching.OutputHeight(Width,Height,joins),96,96,PixelFormats.Bgra32,null,pixels,Width*4);bitmap.Freeze();return bitmap;
    }
}
