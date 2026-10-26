using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;

namespace Captovix;
public static class DocumentStore
{
    private static string Key(string id)=>Guid.TryParse(id,out var guid)?guid.ToString("N"):Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(id)));
    public static string EditPath(string id)=>Path.Combine(App.DataRoot,"Edits",Key(id)+".json");
    public static string ThumbnailPath(string id)=>Path.Combine(App.DataRoot,"Thumbnails",Key(id)+".png");
    public static EditorSession Load(MediaItem item,BitmapSource bitmap,out string? warning)
    {
        warning=null;using var stream=File.OpenRead(item.FilePath);var hash=Convert.ToHexString(SHA256.HashData(stream));
        var path=EditPath(item.Id);
        if(File.Exists(path)||File.Exists(RecoveryFiles.BackupPath(path))){
            try{var doc=RecoveryFiles.Load(path,text=>{var d=EditorDocument.Deserialize(text);d.Validate(bitmap.PixelWidth,bitmap.PixelHeight,hash);RasterPixels.ValidateDecoded(d);return d;},out bool recovered);if(recovered){if(File.Exists(path))try{File.Copy(path,path+".invalid-"+Guid.NewGuid().ToString("N"),false);}catch(Exception ex){App.Log(ex);}warning="Đã phục hồi bản lưu tốt gần nhất. Bản chính lỗi được giữ riêng; kiểm tra nội dung trước khi lưu tiếp.";}var session=new EditorSession(doc);if(recovered)session.RequireSave();return session;}
            catch(Exception ex){App.Log(ex);if(File.Exists(path))try{File.Copy(path,path+".invalid-"+Guid.NewGuid().ToString("N"),false);}catch(Exception copy){App.Log(copy);}warning="Chưa áp dụng được bản chỉnh. File hiện có được giữ; không tự gán lên nguồn khác.\n"+ex.Message;}
        }
        return new(new(){OriginalWidth=bitmap.PixelWidth,OriginalHeight=bitmap.PixelHeight,SourceHash=hash});
    }
    public static void Save(MediaItem item,EditorSession session,BitmapSource original)
    {
        var doc=session.Document;doc.Validate(original.PixelWidth,original.PixelHeight,doc.SourceHash);
        // The document is authoritative; failure to cache a thumbnail must not lose edits.
        var editPath=EditPath(item.Id);
        // First save of an older schema keeps the exact old bytes, including a
        // manual Setup upgrade that did not run the in-app backup flow.
        if(File.Exists(editPath)){
            int priorVersion=0;
            try{using var prior=System.Text.Json.JsonDocument.Parse(File.ReadAllText(editPath));if(prior.RootElement.ValueKind==System.Text.Json.JsonValueKind.Object&&prior.RootElement.TryGetProperty("Version",out var version)&&version.ValueKind==System.Text.Json.JsonValueKind.Number&&version.TryGetInt32(out var number))priorVersion=number;}
            catch(System.Text.Json.JsonException){ /* Keep corrupt sidecar bytes before replacing the recovered document. */ }
            if(priorVersion>14)throw new InvalidDataException("Không ghi đè sidecar từ phiên bản app mới hơn.");
            if(priorVersion<12&&!File.Exists(editPath+".pre-v1.13.bak"))File.Copy(editPath,editPath+".pre-v1.13.bak",false);
        }
        try{RecoveryFiles.Backup(editPath,text=>{var d=EditorDocument.Deserialize(text);d.Validate(original.PixelWidth,original.PixelHeight,doc.SourceHash);return d;});}catch(Exception ex){App.Log(ex);}
        string json=doc.Serialize();if(Encoding.UTF8.GetByteCount(json)>140_000_000)throw new InvalidDataException("Tài liệu vượt140MB. Tách dự án hoặc xuất ảnh PNG; bản đang chỉnh vẫn giữ trong Editor.");
        MediaStore.AtomicWrite(editPath,json);session.MarkSaved();
        try{
            var thumb=EditorRenderer.Render(original,doc,240);var path=ThumbnailPath(item.Id);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temp=path+".tmp";var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(thumb));using(var f=File.Create(temp))encoder.Save(f);File.Move(temp,path,true);item.Thumbnail=thumb;
        }catch(Exception ex){App.Log(ex);}
    }
    public static void Delete(MediaItem item){File.Delete(EditPath(item.Id));File.Delete(ThumbnailPath(item.Id));}
}
