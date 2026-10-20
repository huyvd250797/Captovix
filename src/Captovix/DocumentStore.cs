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
        if(File.Exists(path)){
            try{var doc=EditorDocument.Deserialize(File.ReadAllText(path));doc.Validate(bitmap.PixelWidth,bitmap.PixelHeight,hash);return new(doc);}
            catch(Exception ex){
                App.Log(ex);var backup=path+".invalid-"+DateTime.Now.ToString("yyyyMMddHHmmssfff");File.Copy(path,backup,false);
                warning="Không áp dụng được bản chú thích cũ. Đã giữ bản sao: "+backup+"\n"+ex.Message;
            }
        }
        return new(new(){OriginalWidth=bitmap.PixelWidth,OriginalHeight=bitmap.PixelHeight,SourceHash=hash});
    }
    public static void Save(MediaItem item,EditorSession session,BitmapSource original)
    {
        var doc=session.Document;doc.Validate(original.PixelWidth,original.PixelHeight,doc.SourceHash);
        // The document is authoritative; failure to cache a thumbnail must not lose edits.
        MediaStore.AtomicWrite(EditPath(item.Id),doc.Serialize());session.MarkSaved();
        try{
            var thumb=EditorRenderer.Render(original,doc,240);var path=ThumbnailPath(item.Id);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temp=path+".tmp";var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(thumb));using(var f=File.Create(temp))encoder.Save(f);File.Move(temp,path,true);item.Thumbnail=thumb;
        }catch(Exception ex){App.Log(ex);}
    }
    public static void Delete(MediaItem item){File.Delete(EditPath(item.Id));File.Delete(ThumbnailPath(item.Id));}
}
