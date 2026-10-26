using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
namespace Captovix;
public sealed partial class MediaStore
{
    private sealed record ThumbEntry(MediaItem Item,BitmapSource Image);
    private readonly BudgetCache<(string Id,string Path,long Time,long Length),ThumbEntry> _thumbCache=new(16*1024*1024,128);
    private readonly object _thumbnailGate=new();private readonly HashSet<string> _pendingThumbnails=[];
    private readonly SemaphoreSlim _thumbnailWorkers=new(2);
    public CacheSnapshot ThumbnailCacheStats=>_thumbCache.Stats;
    private static (string Id,string Path,long Time,long Length) ThumbKey(MediaItem item){string thumb=DocumentStore.ThumbnailPath(item.Id),path=File.Exists(thumb)?thumb:item.FilePath;var info=new FileInfo(path);return(item.Id,path,info.LastWriteTimeUtc.Ticks,info.Length);}
    private static BitmapSource DecodeThumbnail(string path){using var input=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read|FileShare.Delete);var header=BitmapDecoder.Create(input,BitmapCreateOptions.DelayCreation,BitmapCacheOption.None).Frames[0];double ratio=Math.Min(1,Math.Min(240d/header.PixelWidth,160d/header.PixelHeight));int width=Math.Max(1,(int)Math.Floor(header.PixelWidth*ratio));input.Position=0;var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.DecodePixelWidth=width;image.StreamSource=input;image.EndInit();image.Freeze();return image;}
    public async void RequestThumbnail(MediaItem item)
    {
        if(item.IsVideo)return;var dispatcher=Application.Current.Dispatcher;if(!dispatcher.CheckAccess()){_=dispatcher.BeginInvoke(()=>RequestThumbnail(item));return;}
        try{var key=ThumbKey(item);if(_thumbCache.TryGet(key,out var cached)){item.Thumbnail=cached.Image;return;}lock(_thumbnailGate)if(!_pendingThumbnails.Add(item.Id))return;
            bool stale=false;await _thumbnailWorkers.WaitAsync().ConfigureAwait(false);try{var image=await Task.Run(()=>DecodeThumbnail(key.Path)).ConfigureAwait(false);await dispatcher.InvokeAsync(()=>{if(ThumbKey(item)!=key){stale=true;return;}var evicted=_thumbCache.Add(key,new(item,image),(long)image.PixelWidth*image.PixelHeight*Math.Max(4,(image.Format.BitsPerPixel+7)/8));foreach(var old in evicted)if(ReferenceEquals(old.Item.Thumbnail,old.Image))old.Item.Thumbnail=null;item.Thumbnail=image;});}finally{_thumbnailWorkers.Release();lock(_thumbnailGate)_pendingThumbnails.Remove(item.Id);}if(stale)await dispatcher.InvokeAsync(()=>RequestThumbnail(item));
        }catch(Exception ex){lock(_thumbnailGate)_pendingThumbnails.Remove(item.Id);App.Log(ex);}
    }
}
