using System.IO;
namespace Captovix;
public static class RecoveryFiles
{
    public static string BackupPath(string path)=>path+".lastgood.bak";
    public static T Load<T>(string path,Func<string,T> validate,out bool recovered)
    {
        recovered=false;Exception? prior=null;try{return validate(Read(path));}catch(Exception ex){prior=ex;}
        try{var result=validate(Read(BackupPath(path)));recovered=true;return result;}catch{throw new InvalidDataException("Không đọc được bản chính và bản lưu tốt gần nhất. "+prior?.Message,prior);}
    }
    private static string Read(string path){if(new FileInfo(path).Length>140_000_000)throw new InvalidDataException("Sidecar quá140MB.");return File.ReadAllText(path);}
    public static void Backup<T>(string path,Func<string,T> validate)
    {
        if(!File.Exists(path))return;if(new FileInfo(path).Length>140_000_000)throw new InvalidDataException("Sidecar quá140MB.");var bytes=File.ReadAllBytes(path);if(bytes.Length>140_000_000)throw new InvalidDataException("Sidecar quá140MB.");using(var reader=new StreamReader(new MemoryStream(bytes)))validate(reader.ReadToEnd());var dest=BackupPath(path);string temp=dest+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllBytes(temp,bytes);File.Move(temp,dest,true);}finally{File.Delete(temp);}
    }
}
