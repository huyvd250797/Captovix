using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace Captovix;
public sealed partial class MediaStore
{
    private sealed record ThumbEntry(MediaItem Item,BitmapSource Image);
    private readonly BudgetCache<(string Id,string Path,long Time,long Length),ThumbEntry> _thumbCache=new(12*1024*1024,96);
    private readonly HashSet<string> _pendingThumbnails=[];
    private readonly Dictionary<string,MediaItem> _retryThumbnails=[];
    private int _thumbnailGeneration;
    private readonly SemaphoreSlim _thumbnailWorkers=new(2);
    public CacheSnapshot ThumbnailCacheStats=>_thumbCache.Stats;
    private static (string Id,string Path,long Time,long Length) ThumbKey(MediaItem item){string thumb=DocumentStore.ThumbnailPath(item.Id),path=File.Exists(thumb)?thumb:item.FilePath;var info=new FileInfo(path);return(item.Id,path,info.LastWriteTimeUtc.Ticks,info.Length);}
    private static BitmapSource DecodeThumbnail(string path){using var input=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read|FileShare.Delete);var header=BitmapDecoder.Create(input,BitmapCreateOptions.DelayCreation,BitmapCacheOption.None).Frames[0];double ratio=Math.Min(1,Math.Min(240d/header.PixelWidth,160d/header.PixelHeight));int width=Math.Max(1,(int)Math.Floor(header.PixelWidth*ratio));input.Position=0;var image=new BitmapImage();image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.DecodePixelWidth=width;image.StreamSource=input;image.EndInit();image.Freeze();return image;}
    private static BitmapSource RebuildThumbnail(MediaItem item,string path){var original=LoadImage(path);var session=DocumentStore.Load(item,original,out _);try{return EditorRenderer.Render(original,session.Document,240);}finally{EditorRenderer.ReleaseImageCaches(original,clearSharedRaster:false);}}
    public void ClearThumbnailCache()
    {
        _thumbnailGeneration++;
        _retryThumbnails.Clear();
        _thumbCache.Clear();
        foreach(var item in Items.Where(i=>i.Thumbnail!=null))item.Thumbnail=null;
    }

    public async void RequestThumbnail(MediaItem item)
    {
        if(item.IsVideo)return;
        var dispatcher=Application.Current.Dispatcher;
        if(dispatcher.HasShutdownStarted)return;
        if(!dispatcher.CheckAccess()){_=dispatcher.BeginInvoke(()=>RequestThumbnail(item),DispatcherPriority.Background);return;}
        if(!_pendingThumbnails.Add(item.Id))
        {
            _retryThumbnails[item.Id]=item;
            return;
        }
        var generation=_thumbnailGeneration;
        var source=new MediaItem{Id=item.Id,FilePath=item.FilePath};
        bool retry=false;
        try
        {
            await _thumbnailWorkers.WaitAsync();
            try
            {
                var (key,image,changed)=await Task.Run(async ()=>
                {
                    var current=ThumbKey(source);
                    BitmapSource decoded;
                    if(_thumbCache.TryGet(current,out var cached))decoded=cached.Image;
                    else if(File.Exists(DocumentStore.EditPath(source.Id))&&!File.Exists(DocumentStore.ThumbnailPath(source.Id)))
                    {
                        BitmapSource? rebuilt=null;
                        await StaImageWorker.Run(()=>rebuilt=RebuildThumbnail(source,current.Path)).ConfigureAwait(false);
                        decoded=rebuilt!;
                    }
                    else decoded=DecodeThumbnail(current.Path);
                    return (current,decoded,ThumbKey(source)!=current);
                });
                retry=changed;
                await Dispatcher.Yield(DispatcherPriority.Background);
                if(retry||generation!=_thumbnailGeneration||_retryThumbnails.ContainsKey(item.Id)||
                    !string.Equals(source.FilePath,item.FilePath,StringComparison.OrdinalIgnoreCase))return;
                var evicted=_thumbCache.Add(key,new(item,image),(long)image.PixelWidth*image.PixelHeight*Math.Max(4,(image.Format.BitsPerPixel+7)/8));
                foreach(var old in evicted)
                    if(ReferenceEquals(old.Item.Thumbnail,old.Image)&&!ReferenceEquals(old.Image,image))old.Item.Thumbnail=null;
                item.Thumbnail=image;
            }
            finally{_thumbnailWorkers.Release();}
        }
        catch(Exception error){App.Log(error);}
        finally
        {
            _pendingThumbnails.Remove(item.Id);
            if(_retryThumbnails.Remove(item.Id,out var latest))RequestThumbnail(latest);
            else if(retry&&generation==_thumbnailGeneration)RequestThumbnail(item);
        }
    }
}
