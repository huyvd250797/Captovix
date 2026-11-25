using System.IO;
using System.Text.Json;

namespace Captovix;

/// <summary>Write-ahead journal allows recovery around metadata commits and Windows Recycle Bin.</summary>
internal sealed record RetentionBatch(string Folder,string Metadata,TrashEntry[] Entries,bool Committed=false)
{
    private static string Journal=>Path.Combine(App.DataRoot,"retention-pending.json");
    internal void Save()=>MediaStore.AtomicWrite(Journal,JsonSerializer.Serialize(this));
    internal static RetentionBatch Stage(MediaItem[] items,IEnumerable<string> paths){
        if(File.Exists(Journal))throw new IOException("Còn một phiên tự dọn cần khôi phục. Khởi động lại Captovix để kiểm tra.");
        string folder=Path.Combine(App.DataRoot,"RetentionStaging",Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var entries=paths.Where(File.Exists).Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase)
            .Select((path,i)=>new TrashEntry(path,Path.Combine(folder,i+"-"+Path.GetFileName(path)))).ToArray();
        var batch=new RetentionBatch(folder,JsonSerializer.Serialize(items),entries);
        batch.Save(); // Durable before the first move.
        try{
            foreach(var entry in entries){
                if((File.GetAttributes(entry.Original)&FileAttributes.ReparsePoint)!=0)throw new IOException("Không tự dọn liên kết tệp.");
                File.Move(entry.Original,entry.Stored);
            }
            File.WriteAllText(Path.Combine(folder,"Captovix-recovery.json"),JsonSerializer.Serialize(batch));
            return batch;
        }catch{batch.Rollback();throw;}
    }
    internal void Rollback(bool finish=true){
        // A crash after restoring files must restore metadata, not treat the absent stage as recycled.
        (this with{Committed=false}).Save();
        foreach(var entry in Entries.Reverse()){
            if(!File.Exists(entry.Stored))continue;
            if(File.Exists(entry.Original))throw new IOException("Tệp gốc đã thay đổi; giữ phiên tự dọn để khôi phục thủ công: "+Folder);
            Directory.CreateDirectory(Path.GetDirectoryName(entry.Original)!);File.Move(entry.Stored,entry.Original);
        }
        // Only remove the receipt; never permanently delete captured media on this path.
        string receipt=Path.Combine(Folder,"Captovix-recovery.json");if(File.Exists(receipt))File.Delete(receipt);
        if(Directory.Exists(Folder))Directory.Delete(Folder,false);
        if(finish)File.Delete(Journal);
    }
    internal static void FinishRollback()=>File.Delete(Journal);
    internal void Recycle(){
        if(!Committed)throw new InvalidOperationException("Chưa lưu thay đổi thư viện.");
        var drive=new DriveInfo(Path.GetPathRoot(Path.GetFullPath(Folder))!);
        if(drive.DriveType!=DriveType.Fixed)throw new IOException("Thư mục dữ liệu cần nằm trên ổ cục bộ hỗ trợ Thùng rác Windows.");
        Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(Folder,
            Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
            Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
            Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);
        File.Delete(Journal);
    }
    internal static void Recover(MediaStore store){
        if(!File.Exists(Journal))return;
        var batch=JsonSerializer.Deserialize<RetentionBatch>(File.ReadAllText(Journal))??throw new InvalidDataException("Nhật ký tự dọn không hợp lệ.");
        var items=JsonSerializer.Deserialize<MediaItem[]>(batch.Metadata)??[];
        if(batch.Committed&&!Directory.Exists(batch.Folder)){
            foreach(var item in store.Items.Where(item=>items.Any(old=>old.Id==item.Id)).ToArray())store.Items.Remove(item);
            store.Save();File.Delete(Journal);return;
        }
        batch.Rollback(false);
        foreach(var item in items)if(!store.Items.Any(old=>old.Id==item.Id))store.Items.Add(item);
        store.Save();FinishRollback();
    }
}
