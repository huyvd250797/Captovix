using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
namespace Captovix;
public sealed record HybridWindow(long Handle,uint ProcessId,PixelRect Bounds);
public sealed record HybridDisplay(PixelRect Bounds,uint DpiX,uint DpiY);
public sealed record HybridQuery(HybridWindow[] Windows,HybridDisplay[] Displays,int ExcludedProcess,long TargetWindow=0);
public sealed record HybridNode(string Id,string? ParentId,long Window,uint ProcessId,PixelRect Bounds,string Type,string Name="",string Font="",double FontPoints=0,bool Bold=false,uint DpiY=96,string PixelHash="",bool Italic=false,bool Underline=false,string? TextColor=null,bool StyleComplete=false);
public sealed record HybridEvidence(int Version,DateTimeOffset CapturedAt,DateTimeOffset CompletedAt,string Topology,PixelRect ScreenBounds,HybridNode[] Nodes,string Status="",string SourceHash="");
public static class HybridCapture
{
    public const int MaxNodes=256,MaxJsonBytes=512_000;
    public const long MaxVerifyPixels=20_000_000;
    public static bool Valid(PixelRect b)=>b.X is >-1000000 and <1000000&&b.Y is >-1000000 and <1000000&&b.Width is >0 and <=32768&&b.Height is >0 and <=32768;
    public static PixelRect ToImage(PixelRect physical,PixelRect screenshot)=>physical with{X=physical.X-screenshot.X,Y=physical.Y-screenshot.Y};
    // UIA bounds are already physical pixels. DPI is used for font points only.
    public static double FontPixels(HybridNode n)=>Math.Clamp(n.FontPoints*(n.DpiY is >=48 and <=768?n.DpiY:96)/72d,8,72);
    public static uint RenderDpi(PixelRect window,IReadOnlyList<HybridDisplay> displays,uint windowDpi,bool perMonitorAware){
        if(perMonitorAware&&windowDpi is >=48 and <=768)return windowDpi;
        var display=displays.OrderByDescending(d=>(long)d.Bounds.Intersect(window).Width*d.Bounds.Intersect(window).Height).FirstOrDefault();return display?.DpiY is >=48 and <=768?display.DpiY:96;
    }
    public static bool Occluded(HybridNode node,IReadOnlyList<HybridWindow> windows){
        int owner=-1;for(int i=0;i<windows.Count;i++)if(windows[i].Handle==node.Window&&windows[i].ProcessId==node.ProcessId){owner=i;break;}
        if(owner<0)return true;
        return windows.Take(owner).Any(w=>!w.Bounds.Intersect(node.Bounds).IsEmpty);
    }
    public static string PatchHash(byte[] pixels,int width,int height,PixelRect local){
        if(!Valid(local)||local.X<0||local.Y<0||local.Right>width||local.Bottom>height||pixels.LongLength!=(long)width*height*4)return "";
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for(int y=local.Y;y<local.Bottom;y++)hash.AppendData(pixels.AsSpan((y*width+local.X)*4,local.Width*4));
        return Convert.ToHexString(hash.GetHashAndReset());
    }
    public static HybridEvidence Verify(HybridEvidence draft,byte[] before,byte[] after,IReadOnlyList<HybridWindow> earlier,IReadOnlyList<HybridWindow> later,string topology,DateTimeOffset now){
        var b=draft.ScreenBounds;var kept=new List<HybridNode>();long budget=MaxVerifyPixels;
        if(draft.Version!=1||!Valid(b)||b.Width*(long)b.Height>MaxVerifyPixels||draft.Topology!=topology||now<draft.CapturedAt||now-draft.CapturedAt>TimeSpan.FromSeconds(3))return draft with{Nodes=[],Status="Ảnh/OCR · giao diện đổi hoặc dữ liệu quá thời điểm chụp"};
        foreach(var node in draft.Nodes.Take(MaxNodes)){
            if(!Valid(node.Bounds)||node.Bounds.Width<8||node.Bounds.Height<8||node.Bounds.Intersect(b)!=node.Bounds||Occluded(node,earlier)||Occluded(node,later))continue;
            var old=earlier.FirstOrDefault(w=>w.Handle==node.Window&&w.ProcessId==node.ProcessId);var fresh=later.FirstOrDefault(w=>w.Handle==node.Window&&w.ProcessId==node.ProcessId);
            if(old==null||fresh==null||old.Bounds!=fresh.Bounds||node.Bounds.Intersect(old.Bounds)!=node.Bounds)continue;
            long area=(long)node.Bounds.Width*node.Bounds.Height;if(area>1_000_000||area>budget)continue;budget-=area;
            var local=ToImage(node.Bounds,b);string hash=PatchHash(before,b.Width,b.Height,local);if(hash.Length==0||hash!=PatchHash(after,b.Width,b.Height,local))continue;
            kept.Add(node with{PixelHash=hash});
        }
        var ids=kept.Select(n=>n.Id).ToHashSet();return draft with{Nodes=kept.Select(n=>n.ParentId!=null&&!ids.Contains(n.ParentId)?n with{ParentId=null}:n).ToArray(),CompletedAt=now,Status=kept.Count>0?$"Hybrid · {kept.Count} phần tử UI khớp ảnh":"Ảnh/OCR · không có cấu trúc UI ổn định"};
    }
    public static HybridEvidence? Bind(HybridEvidence? evidence,PixelRect crop,byte[] pixels,int width,int height,DateTimeOffset now){
        if(evidence==null||evidence.Version!=1||crop.Width!=width||crop.Height!=height||evidence.Nodes.Length>MaxNodes||now-evidence.CompletedAt>TimeSpan.FromSeconds(3)||now<evidence.CapturedAt)return null;
        var nodes=evidence.Nodes.Where(n=>n.Bounds.Intersect(crop)==n.Bounds&&n.PixelHash.Length==64&&PatchHash(pixels,width,height,ToImage(n.Bounds,crop))==n.PixelHash).ToArray();
        return evidence with{ScreenBounds=crop,Nodes=nodes,Status=nodes.Length>0?$"Hybrid · {nodes.Length} phần tử UI khớp ảnh":"Ảnh/OCR · không có phần tử UI khớp vùng chụp"};
    }
    public static HybridEvidence? CropRetained(HybridEvidence? evidence,PixelRect crop){
        if(evidence==null||evidence.Version!=1||crop.Intersect(evidence.ScreenBounds)!=crop)return null;
        var nodes=evidence.Nodes.Where(n=>n.Bounds.Intersect(crop)==n.Bounds).ToArray();return evidence with{ScreenBounds=crop,Nodes=nodes,Status=$"Hybrid · {nodes.Length} phần tử UI khớp ảnh"};
    }
    public static string? MetadataPath(string root,string id)=>Guid.TryParseExact(id,"N",out _)?Path.Combine(root,"HybridCaptures",id+".json"):null;
    public static void Save(string root,string id,string source,HybridEvidence evidence){
        string? path=MetadataPath(root,id);if(path==null||evidence.Nodes.Length==0)return;
        using var input=File.OpenRead(source);var verified=evidence with{SourceHash=Convert.ToHexString(SHA256.HashData(input))};byte[] json=JsonSerializer.SerializeToUtf8Bytes(verified);
        if(json.Length>MaxJsonBytes)throw new InvalidDataException("Dữ liệu Hybrid vượt giới hạn.");Directory.CreateDirectory(Path.GetDirectoryName(path)!);string temp=path+".tmp";try{File.WriteAllBytes(temp,json);File.Move(temp,path,true);}finally{if(File.Exists(temp))File.Delete(temp);}
    }
    public static HybridEvidence? Load(string root,string id,string source,int width,int height){
        try{string? path=MetadataPath(root,id);if(path==null||!File.Exists(path)||new FileInfo(path).Length>MaxJsonBytes)return null;var data=JsonSerializer.Deserialize<HybridEvidence>(File.ReadAllBytes(path));
            if(data==null||data.Version!=1||data.Nodes==null||data.Nodes.Length>MaxNodes||data.ScreenBounds.Width!=width||data.ScreenBounds.Height!=height||data.Nodes.Any(n=>n==null||n.Name.Length>200||n.Font.Length>100||!Valid(n.Bounds)))return null;
            using var input=File.OpenRead(source);return Convert.ToHexString(SHA256.HashData(input))==data.SourceHash?data:null;
        }catch(Exception e)when(e is IOException or UnauthorizedAccessException or JsonException or ArgumentException or NullReferenceException){return null;}
    }
}
