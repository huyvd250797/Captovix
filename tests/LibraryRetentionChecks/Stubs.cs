using System.Text.Json;
namespace Captovix;
internal static class App{internal static string DataRoot=Path.Combine(Path.GetTempPath(),"cvx-retention-"+Guid.NewGuid().ToString("N"));}
public sealed class MediaItem{public string Id{get;set;}="";public string FilePath{get;set;}="";}
public sealed class MediaStore{
    public List<MediaItem> Items{get;}=[];
    public void Save()=>AtomicWrite(Path.Combine(App.DataRoot,"library.json"),JsonSerializer.Serialize(Items));
    public static void AtomicWrite(string path,string text){Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path+".tmp",text);File.Move(path+".tmp",path,true);}
}
