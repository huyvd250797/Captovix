using System.IO;
namespace Captovix;
public static class RecoveryFiles
{
    public static string BackupPath(string path)=>path+".lastgood.bak";
    public static T Load<T>(string path,Func<string,T> validate,out bool recovered,CancellationToken token=default)
    {
        recovered=false;
        Exception prior;
        try{return validate(Read(path,token));}
        catch(OperationCanceledException)when(token.IsCancellationRequested){throw;}
        catch(Exception error){prior=error;}
        try
        {
            var result=validate(Read(BackupPath(path),token));
            recovered=true;
            return result;
        }
        catch(OperationCanceledException)when(token.IsCancellationRequested){throw;}
        catch{throw new InvalidDataException("Không đọc được bản chính và bản lưu tốt gần nhất. "+prior.Message,prior);}
    }
    private static string Read(string path,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(new FileInfo(path).Length>140_000_000)throw new InvalidDataException("Sidecar quá140MB.");
        return token.CanBeCanceled?File.ReadAllTextAsync(path,token).GetAwaiter().GetResult():File.ReadAllText(path);
    }
    public static void Backup<T>(string path,Func<string,T> validate)
    {
        if(!File.Exists(path))return;if(new FileInfo(path).Length>140_000_000)throw new InvalidDataException("Sidecar quá140MB.");var bytes=File.ReadAllBytes(path);if(bytes.Length>140_000_000)throw new InvalidDataException("Sidecar quá140MB.");using(var reader=new StreamReader(new MemoryStream(bytes)))validate(reader.ReadToEnd());var dest=BackupPath(path);string temp=dest+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllBytes(temp,bytes);File.Move(temp,dest,true);}finally{File.Delete(temp);}
    }
}
