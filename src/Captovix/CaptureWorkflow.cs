using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Captovix;
public enum PhotoAction { Current=1, Region=5, Window=6, Display=7, Repeat=8, Element=9 }
public sealed record CaptureHotkey(PhotoAction Action, bool Enabled, uint Modifiers, uint Key)
{
    public static List<CaptureHotkey> Defaults()=>Enum.GetValues<PhotoAction>().Select(a=>new CaptureHotkey(a,a is PhotoAction.Region or PhotoAction.Display or PhotoAction.Repeat,6,(uint)(0x30+(int)a))).ToList();
    public string Label => !Enabled ? "Tắt" : $"{((Modifiers&8)!=0?"Win + ":"")}{((Modifiers&2)!=0?"Ctrl + ":"")}{((Modifiers&1)!=0?"Alt + ":"")}{((Modifiers&4)!=0?"Shift + ":"")}{CaptureKeys.KeyName(Key)}";
}
public sealed record CapturePreset(string Id,string Name,int Mode,string Display,int Delay,bool Cursor,bool Background,bool AutoCopy);
public sealed record WorkflowDisplay(string Device,PixelRect Bounds,uint DpiX,uint DpiY,string Identity="");
public sealed record SavedCaptureRegion(PixelRect Bounds,string Topology);
public sealed class CapturePhotoStyle
{
    public int Version {get;set;}=1;
    public bool BorderEnabled {get;set;}
    public int BorderWidth {get;set;}=2;
    public string BorderColor {get;set;}="#26364B";
    public void Validate(){if(Version!=1||BorderWidth<1||BorderWidth>128||BorderColor is not {Length:7}||BorderColor[0]!='#'||!BorderColor.AsSpan(1).ToString().All(Uri.IsHexDigit))throw new InvalidDataException("Viền chụp cần màu #RRGGBB và độ dày 1–128 px.");}
}
public sealed record EditorShortcut(string Command,bool Enabled,uint Modifiers,uint Key)
{
    public string Label=>new CaptureHotkey(PhotoAction.Region,Enabled,Modifiers,Key).Label;
}
public static class EditorShortcutRules
{
    public static bool Reserved(uint m,uint k){if(k is 0x1B or 0x0D or 0x09 or 0x20 or >=0x21 and <=0x28 or 0x2E or 0x71 or 0x79)return true;if((m&8)!=0)return true;if(m==1&&k==0x73)return true;
        if(m==2&&new uint[]{0x41,0x42,0x43,0x44,0x47,0x49,0x4C,0x4F,0x52,0x53,0x55,0x56,0x58,0x59,0x5A,0x30,0x31,0xBB,0xBD,0x6B,0x6D,0xDB,0xDD}.Contains(k))return true;
        return m==6&&new uint[]{0x32,0x33,0x34,0x42,0x43,0x45,0x47,0x4C,0x52,0x53,0x5A,0xDB,0xDD,0xBB,0xBD}.Contains(k);
    }
    public static void Validate(IEnumerable<EditorShortcut> shortcuts,IEnumerable<CaptureHotkey> capture){var used=capture.Where(x=>x.Enabled).Select(x=>(x.Modifiers,x.Key)).ToHashSet();foreach(var r in new[]{(6u,0x32u),(6u,0x33u),(6u,0x34u)})used.Add(r);var commands=new HashSet<string>();foreach(var s in shortcuts){if(string.IsNullOrWhiteSpace(s.Command)||s.Command.Length>80||!commands.Add(s.Command))throw new InvalidDataException("Công cụ có cấu hình phím không hợp lệ.");if(!s.Enabled)continue;if(!CaptureKeys.IsValid(s.Modifiers,s.Key)||Reserved(s.Modifiers,s.Key))throw new InvalidDataException("Phím "+s.Label+" dành cho thao tác hệ thống/soạn thảo. Chọn phím khác.");if(!used.Add((s.Modifiers,s.Key)))throw new InvalidDataException("Phím tắt "+s.Label+" bị trùng với công cụ hoặc chức năng chụp/quay.");}}
}
public sealed class CaptureWorkflowSettings
{
    public bool HybridUi {get;set;}=true;
    public bool RequireEnter {get;set;}
    public List<EditorShortcut> EditorHotkeys {get;set;}=[];
    public CapturePhotoStyle PhotoStyle {get;set;}=new();
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
        if(value.PhotoStyle==null)throw new InvalidDataException("Thiếu cấu hình trình bày ảnh chụp.");value.PhotoStyle.Validate();
        if(value.SchemaVersion!=1)throw new InvalidDataException("Phiên bản cấu hình chụp chưa được hỗ trợ.");
        if(value.Hotkeys==null||value.Hotkeys.Count!=6||value.Hotkeys.Select(h=>h.Action).Distinct().Count()!=6||value.Hotkeys.Any(h=>!Enum.IsDefined(h.Action)))throw new InvalidDataException("Danh sách phím tắt không hợp lệ.");
        var used=new HashSet<(uint,uint)>{(6,0x32),(6,0x33),(6,0x34)};
        foreach(var h in value.Hotkeys.Where(h=>h.Enabled)){
            if(!CaptureKeys.IsValid(h.Modifiers,h.Key))
                throw new InvalidDataException("Chọn một phím bàn phím hoặc tổ hợp Ctrl/Alt/Shift/Win. Phím dành riêng hoặc đang bị ứng dụng khác giữ có thể bị Windows từ chối.");
            if(!used.Add((h.Modifiers,h.Key)))throw new InvalidDataException("Phím tắt bị trùng hoặc trùng phím quay Ctrl+Shift+2/3/4.");
        }
        if(value.EditorHotkeys==null||value.EditorHotkeys.Count>100)throw new InvalidDataException("Cấu hình phím công cụ không hợp lệ.");EditorShortcutRules.Validate(value.EditorHotkeys,value.Hotkeys);
        if(value.Presets==null||value.Presets.Count>20||value.Presets.Any(p=>p==null||string.IsNullOrWhiteSpace(p.Id)||string.IsNullOrWhiteSpace(p.Name)||p.Name.Length>40||p.Mode is <0 or >3||p.Delay is not (0 or 3 or 5)||p.Display==null)||value.Presets.Select(p=>p.Id).Distinct().Count()!=value.Presets.Count||value.Presets.Select(p=>p.Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=value.Presets.Count)throw new InvalidDataException("Preset cần tên riêng (1–40 ký tự), tối đa 20 preset; trễ 0/3/5 giây.");
        if(value.LastRegion is {} r && (r.Bounds.Width<8||r.Bounds.Height<8||string.IsNullOrWhiteSpace(r.Topology)))throw new InvalidDataException("Vùng chụp cũ không hợp lệ.");
    }
    public static CaptureWorkflowSettings Load(string root,out string? warning)
    {
        warning=null;var path=Path.Combine(root,FileName);if(!File.Exists(path))return new();
        try{var value=JsonSerializer.Deserialize<CaptureWorkflowSettings>(File.ReadAllText(path))??throw new InvalidDataException("Cấu hình trống.");if(value.Hotkeys is {Count:5}&&value.Hotkeys.All(h=>h.Action!=PhotoAction.Element))value.Hotkeys.Add(new(PhotoAction.Element,false,6,0x39));value.Hotkeys=value.Hotkeys.Select(h=>h.Action is PhotoAction.Current or PhotoAction.Window or PhotoAction.Element?h with{Enabled=false}:h).ToList();Validate(value);return value;}
        catch(Exception e){warning="Chưa đọc được cấu hình Capture Workflow. File được giữ nguyên; dùng Khôi phục mặc định để sao lưu và tạo lại. "+e.Message;return new();}
    }
    public static void Save(string root,CaptureWorkflowSettings value)
    {
        Validate(value);Directory.CreateDirectory(root);var path=Path.Combine(root,FileName);var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        if(File.Exists(path)&&!File.Exists(path+".pre-v1.40.bak"))File.Copy(path,path+".pre-v1.40.bak");
        if(File.Exists(path)&&!File.Exists(path+".pre-v1.11.1.bak"))File.Copy(path,path+".pre-v1.11.1.bak");
        if(File.Exists(path)&&!File.Exists(path+".pre-v1.9.bak"))File.Copy(path,path+".pre-v1.9.bak");
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
