using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Captovix;
public enum PhotoAction { Current=1, Region=5, Window=6, Display=7, Repeat=8 }
public sealed record CaptureHotkey(PhotoAction Action, bool Enabled, uint Modifiers, uint Key)
{
    public static List<CaptureHotkey> Defaults()=>Enum.GetValues<PhotoAction>().Select(a=>new CaptureHotkey(a,true,6,(uint)(0x30+(int)a))).ToList();
    public string Label => !Enabled ? "Tắt" : $"{((Modifiers&2)!=0?"Ctrl + ":"")}{((Modifiers&1)!=0?"Alt + ":"")}{((Modifiers&4)!=0?"Shift + ":"")}{(Key is >=0x30 and <=0x39?(char)Key:Key is >=0x41 and <=0x5A?(char)Key:Key==0x2C?"PrintScreen":$"F{Key-0x6F}")}";
}
public sealed record CapturePreset(string Id,string Name,int Mode,string Display,int Delay,bool Cursor,bool Background,bool AutoCopy);
public sealed record WorkflowDisplay(string Device,PixelRect Bounds,uint DpiX,uint DpiY,string Identity="");
public sealed record SavedCaptureRegion(PixelRect Bounds,string Topology);
public sealed class CaptureWorkflowSettings
{
    public int SchemaVersion {get;set;}=1;
    public bool CloseToTray {get;set;}
    public bool StartWithWindows {get;set;}
    public bool StartInTray {get;set;}
    public bool Background {get;set;}
    public bool AutoCopy {get;set;}
    public List<CaptureHotkey> Hotkeys {get;set;}=CaptureHotkey.Defaults();
    public List<CapturePreset> Presets {get;set;}=[new("guide","Hướng dẫn",0,"",3,true,false,false),new("quick","Chụp nhanh",0,"",0,false,true,true),new("report","Báo cáo",2,"",0,false,false,false)];
    public SavedCaptureRegion? LastRegion {get;set;}
    public CaptureWorkflowSettings Clone()=>JsonSerializer.Deserialize<CaptureWorkflowSettings>(JsonSerializer.Serialize(this))!;
}
public static class CaptureWorkflowStore
{
    public const string FileName="capture-workflow.json";
    public static void Validate(CaptureWorkflowSettings value)
    {
        if(value.SchemaVersion!=1)throw new InvalidDataException("Phiên bản cấu hình chụp chưa được hỗ trợ.");
        if(value.Hotkeys==null||value.Hotkeys.Count!=5||value.Hotkeys.Select(h=>h.Action).Distinct().Count()!=5||value.Hotkeys.Any(h=>!Enum.IsDefined(h.Action)))throw new InvalidDataException("Danh sách phím tắt không hợp lệ.");
        var used=new HashSet<(uint,uint)>{(6,0x32),(6,0x33),(6,0x34)};
        foreach(var h in value.Hotkeys.Where(h=>h.Enabled)){
            if((h.Modifiers&~7u)!=0 || (h.Modifiers&3)==0 && h.Key!=0x2C && !(h.Key>=0x70&&h.Key<=0x87&&h.Key!=0x7B) || !(h.Key is >=0x30 and <=0x39 or >=0x41 and <=0x5A or >=0x70 and <=0x87 or 0x2C) || h.Key==0x7B)
                throw new InvalidDataException("Phím tắt cần Ctrl/Alt + chữ/số, F1–F24 (trừ F12), hoặc PrintScreen.");
            if(!used.Add((h.Modifiers,h.Key)))throw new InvalidDataException("Phím tắt bị trùng hoặc trùng phím quay Ctrl+Shift+2/3/4.");
        }
        if(value.Presets==null||value.Presets.Count>20||value.Presets.Any(p=>p==null||string.IsNullOrWhiteSpace(p.Id)||string.IsNullOrWhiteSpace(p.Name)||p.Name.Length>40||p.Mode is <0 or >2||p.Delay is not (0 or 3 or 5)||p.Display==null)||value.Presets.Select(p=>p.Id).Distinct().Count()!=value.Presets.Count||value.Presets.Select(p=>p.Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=value.Presets.Count)throw new InvalidDataException("Preset cần tên riêng (1–40 ký tự), tối đa 20 preset; trễ 0/3/5 giây.");
        if(value.LastRegion is {} r && (r.Bounds.Width<8||r.Bounds.Height<8||string.IsNullOrWhiteSpace(r.Topology)))throw new InvalidDataException("Vùng chụp cũ không hợp lệ.");
    }
    public static CaptureWorkflowSettings Load(string root,out string? warning)
    {
        warning=null;var path=Path.Combine(root,FileName);if(!File.Exists(path))return new();
        try{var value=JsonSerializer.Deserialize<CaptureWorkflowSettings>(File.ReadAllText(path))??throw new InvalidDataException("Cấu hình trống.");Validate(value);return value;}
        catch(Exception e){warning="Chưa đọc được cấu hình Capture Workflow. File được giữ nguyên; dùng Khôi phục mặc định để sao lưu và tạo lại. "+e.Message;return new();}
    }
    public static void Save(string root,CaptureWorkflowSettings value)
    {
        Validate(value);Directory.CreateDirectory(root);var path=Path.Combine(root,FileName);var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{File.WriteAllText(temp,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}));File.Move(temp,path,true);}finally{if(File.Exists(temp))File.Delete(temp);}
    }
    public static string Topology(IEnumerable<WorkflowDisplay> displays)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(displays.OrderBy(d=>d.Device,StringComparer.Ordinal).ToArray()))));
    public static bool CanRepeat(SavedCaptureRegion? region,IReadOnlyList<WorkflowDisplay> displays)
    {
        if(region==null||region.Topology!=Topology(displays)||region.Bounds.Width<8||region.Bounds.Height<8||displays.Count==0)return false;
        var r=region.Bounds;return r.X>=displays.Min(d=>d.Bounds.X)&&r.Y>=displays.Min(d=>d.Bounds.Y)&&(long)r.X+r.Width<=displays.Max(d=>(long)d.Bounds.X+d.Bounds.Width)&&(long)r.Y+r.Height<=displays.Max(d=>(long)d.Bounds.Y+d.Bounds.Height)&&displays.Any(d=>!r.Intersect(d.Bounds).IsEmpty);
    }
    public static string StartupCommand(string path)
    {
        if(string.IsNullOrWhiteSpace(path)||path.IndexOfAny(['"','\0','\r','\n'])>=0)throw new ArgumentException("Đường dẫn ứng dụng không hợp lệ.");
        var result="\""+path+"\" --captovix-autostart";if(result.Length>260)throw new ArgumentException("Đường dẫn quá dài để khởi động cùng Windows (tối đa 260 ký tự cả lệnh).");return result;
    }
}
// Registration is transactional: a failed replacement restores the previous live bindings.
public sealed class CaptureHotkeyRegistry(Func<int,uint,uint,bool> register,Action<int> unregister)
{
    private List<CaptureHotkey> _active=[];
    public IReadOnlyList<CaptureHotkey> Active=>_active;
    public List<string> Start(IEnumerable<CaptureHotkey> bindings)
    {
        Stop();var errors=new List<string>();foreach(var h in bindings.Where(h=>h.Enabled))if(register((int)h.Action,h.Modifiers|0x4000,h.Key))_active.Add(h);else errors.Add(h.Label);return errors;
    }
    public void Replace(IEnumerable<CaptureHotkey> bindings)
    {
        var old=_active.ToList();var candidate=bindings.Where(h=>h.Enabled).ToList();var errors=Start(candidate);if(errors.Count==0)return;
        var recovery=Start(old);throw new InvalidOperationException("Windows không cho đăng ký: "+string.Join(", ",errors)+". "+(recovery.Count==0?"Đã phục hồi phím tắt trước đó.":"Không phục hồi được: "+string.Join(", ",recovery)));
    }
    public void Stop(){foreach(var h in _active)unregister((int)h.Action);_active.Clear();}
}
public sealed class CaptureGate
{
    private int _held;
    public bool TryEnter()=>Interlocked.CompareExchange(ref _held,1,0)==0;
    public void Exit()=>Interlocked.Exchange(ref _held,0);
}
