using System.IO;
using System.Text.Json;
namespace Captovix;
public sealed record TrashEntry(string Original,string Stored);
public sealed record TrashReceipt(string Folder,string Metadata,List<TrashEntry> Entries);
public static class MediaTrash
{
    public static TrashReceipt Move(string root,string metadata,IEnumerable<string> files)
    {
        var folder=Path.Combine(root,"Trash",DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")+"-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);var entries=new List<TrashEntry>();
        try{
            foreach(var p in files.Distinct(StringComparer.OrdinalIgnoreCase).Where(File.Exists)){
                if((File.GetAttributes(p)&FileAttributes.ReparsePoint)!=0)throw new IOException("Không chuyển liên kết vào thùng rác.");
                string target=Path.Combine(folder,entries.Count+"-"+Path.GetFileName(p));File.Move(p,target);entries.Add(new(Path.GetFullPath(p),target));
            }
            var receipt=new TrashReceipt(folder,metadata,entries);File.WriteAllText(Path.Combine(folder,"receipt.json"),JsonSerializer.Serialize(receipt));return receipt;
        }catch{foreach(var entry in entries.AsEnumerable().Reverse())if(File.Exists(entry.Stored)&&!File.Exists(entry.Original))File.Move(entry.Stored,entry.Original);throw;}
    }
    public static void UndoRestore(TrashReceipt receipt)
    {
        foreach(var entry in receipt.Entries)if(!File.Exists(entry.Original)||File.Exists(entry.Stored))throw new IOException("Không thể hoàn tác khôi phục: file đã thay đổi.");
        var done=new List<TrashEntry>();try{foreach(var entry in receipt.Entries){File.Move(entry.Original,entry.Stored);done.Add(entry);}var path=Path.Combine(receipt.Folder,"receipt.json");if(File.Exists(path+".restored"))File.Move(path+".restored",path,true);}
        catch{foreach(var entry in done.AsEnumerable().Reverse())if(File.Exists(entry.Stored)&&!File.Exists(entry.Original))File.Move(entry.Stored,entry.Original);throw;}
    }
    public static string Restore(TrashReceipt receipt)
    {
        foreach(var e in receipt.Entries)if(File.Exists(e.Original)||!File.Exists(e.Stored))throw new IOException("Đường dẫn khôi phục đã tồn tại hoặc file trong thùng rác đã mất.");
        var done=new List<TrashEntry>();try{
            foreach(var e in receipt.Entries){Directory.CreateDirectory(Path.GetDirectoryName(e.Original)!);File.Move(e.Stored,e.Original);done.Add(e);}
            var path=Path.Combine(receipt.Folder,"receipt.json");if(File.Exists(path))File.Move(path,path+".restored",true);return receipt.Metadata;
        }catch{foreach(var e in done.AsEnumerable().Reverse())if(File.Exists(e.Original)&&!File.Exists(e.Stored))File.Move(e.Original,e.Stored);throw;}
    }
}
