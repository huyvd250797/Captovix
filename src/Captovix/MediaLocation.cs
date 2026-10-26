using System.IO;
using System.Text.Json;
namespace Captovix;
public static class MediaLocation
{
    public const string FileName="media-location.json";
    public static string Load(string dataRoot)
    {
        string config=Path.Combine(dataRoot,FileName);if(!File.Exists(config))return Path.Combine(dataRoot,"Media");
        var path=JsonSerializer.Deserialize<string>(File.ReadAllText(config));if(string.IsNullOrWhiteSpace(path)||!Path.IsPathFullyQualified(path))throw new InvalidDataException("Thư mục media đã lưu không hợp lệ. Chọn lại thư mục lưu.");return Path.GetFullPath(path);
    }
    public static string Validate(string folder)
    {
        if(!Path.IsPathFullyQualified(folder))throw new ArgumentException("Chọn đường dẫn thư mục đầy đủ.");
        string path=Path.GetFullPath(folder);Directory.CreateDirectory(path);string probe=Path.Combine(path,".captovix-write-"+Guid.NewGuid().ToString("N"));
        try{using var stream=new FileStream(probe,FileMode.CreateNew,FileAccess.Write,FileShare.None);stream.WriteByte(0);}finally{if(File.Exists(probe))File.Delete(probe);}return path;
    }
    public static void Save(string dataRoot,string path){Directory.CreateDirectory(dataRoot);string file=Path.Combine(dataRoot,FileName),temp=file+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllText(temp,JsonSerializer.Serialize(path));File.Move(temp,file,true);}finally{if(File.Exists(temp))File.Delete(temp);}}
}
