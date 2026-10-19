using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace CaptureFoundation;
public sealed class MediaItem
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public string Title {get;set;}="";
    public string FilePath {get;set;}="";
    public DateTimeOffset Created {get;set;}=DateTimeOffset.Now;
    public bool IsVideo {get;set;}
    public int Width {get;set;}
    public int Height {get;set;}
    public string Source {get;set;}="";
    public bool Microphone {get;set;}
    public bool SystemAudio {get;set;}
    public int Fps {get;set;}
    public double Seconds {get;set;}
    [JsonIgnore] public BitmapSource? Thumbnail {get;set;}
    [JsonIgnore] public string Badge => IsVideo ? "▶ VIDEO" : "▧ ẢNH";
    [JsonIgnore] public string Detail => $"{Created:dd/MM HH:mm} · {(IsVideo?$"{Seconds:F0} giây":$"{Width}×{Height}")}";
}
public sealed class AppSettings
{
    public int SourceMode {get;set;}
    public int Delay {get;set;}=0;
    public int Fps {get;set;}=30;
    public bool Cursor {get;set;}=true;
    public bool Mic {get;set;}=false;
    public bool SystemAudio {get;set;}=true;
    public bool Hardware {get;set;}=true;
    public string Display {get;set;}="";
}
public sealed class MediaStore
{
    private static readonly JsonSerializerOptions Json = new(){WriteIndented=true};
    public string Root {get;}=Path.Combine(App.DataRoot,"Media");
    public string Metadata => Path.Combine(App.DataRoot,"library.json");
    public string SettingsPath => Path.Combine(App.DataRoot,"settings.json");
    public ObservableCollection<MediaItem> Items {get;}=[];
    public MediaStore()
    {
        Directory.CreateDirectory(Root);
        if(File.Exists(Metadata))try {foreach(var i in JsonSerializer.Deserialize<List<MediaItem>>(File.ReadAllText(Metadata))??[])Items.Add(i);}catch(Exception e){App.Log(e);File.Copy(Metadata,Metadata+".corrupt-"+DateTime.Now.ToString("yyyyMMddHHmmss"),true);}
        // Recover media committed before a crash between file completion and metadata commit.
        foreach(var path in Directory.EnumerateFiles(Root).Where(p=>!p.Contains(".pending.",StringComparison.OrdinalIgnoreCase)&&(Path.GetExtension(p) is ".png" or ".mp4")))
            if(!Items.Any(i=>i.FilePath==path))Items.Insert(0,new(){FilePath=path,Title=Path.GetFileNameWithoutExtension(path),IsVideo=Path.GetExtension(path)==".mp4",Created=File.GetCreationTime(path),Source="Khôi phục từ ổ đĩa"});
        foreach(var i in Items)TryThumbnail(i);
    }
    public static BitmapSource LoadImage(string path,int decodeWidth=0)
    {
        var bitmap=new BitmapImage();bitmap.BeginInit();bitmap.CacheOption=BitmapCacheOption.OnLoad;
        if(decodeWidth>0)bitmap.DecodePixelWidth=decodeWidth;
        using var s=File.OpenRead(path);bitmap.StreamSource=s;bitmap.EndInit();bitmap.Freeze();return bitmap;
    }
    public void TryThumbnail(MediaItem i) {if(i.IsVideo)return;try{i.Thumbnail=LoadImage(i.FilePath,240);}catch(Exception e){App.Log(e);}}
    public string NewPath(bool video) => Path.Combine(Root,$"{DateTime.Now:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid().ToString("N")[..6]}{(video?".mp4":".png")}");
    public void Add(MediaItem item){TryThumbnail(item);Items.Insert(0,item);Save();}
    public void Save()=>AtomicWrite(Metadata,JsonSerializer.Serialize(Items,Json));
    public AppSettings LoadSettings(){try{return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath))??new();}catch{return new();}}
    public void SaveSettings(AppSettings s)=>AtomicWrite(SettingsPath,JsonSerializer.Serialize(s,Json));
    public static void AtomicWrite(string path,string content){Directory.CreateDirectory(Path.GetDirectoryName(path)!);var temp=path+".tmp";File.WriteAllText(temp,content);File.Move(temp,path,true);}
}
