using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Windows.Media.Editing;
using Windows.Storage;


namespace Captovix;

public partial class MainWindow
{
    private void VideoThumbnail_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||sender is not FrameworkElement anchor)return;
        var item=(anchor as Button)?.Tag as MediaItem??_videoItem;
        if(item is not {IsVideo:true}||!_store.Items.Contains(item)||_thumbnailWrites.Contains(item.Id))return;
        PauseVideoForEditing();
        var menu=new ContextMenu();
        menu.PlacementTarget=anchor;
        if(ReferenceEquals(item,_videoItem)&&_timeline is {} timeline){
            var position=timeline.Project.Position;
            var (index,source)=timeline.Map(position);
            var clip=timeline.Project.Clips[index];
            if(source>=clip.End)source=LastVideoFrame(clip);
            string input=timeline.Project.Input(clip.SourceIndex).Path;
            Add($"Dùng khung tại {VideoTimecode.Format(position)}",()=>SaveVideoThumbnailAsync(item,input,source));
        }
        Add("Chọn ảnh từ máy…",async ()=>{
            var picker=new OpenFileDialog{Filter="Ảnh|*.png;*.jpg;*.jpeg;*.bmp;*.webp"};
            if(picker.ShowDialog(this)==true)await SaveVideoThumbnailAsync(item,picker.FileName,null);
        });
        Add("Bỏ ảnh đại diện",()=>{
            File.Delete(DocumentStore.ThumbnailPath(item.Id));
            item.Thumbnail=null;
            SetStatus("Đã bỏ ảnh đại diện của video.");
            return Task.CompletedTask;
        });
        menu.IsOpen=true;
        void Add(string title,Func<Task> action){
            var entry=new MenuItem{Header=title};
            entry.Click+=async (_,_)=>{
                try{await action();}
                catch(Exception error){App.Log(error);SetStatus("Chưa lưu được ảnh đại diện: "+error.Message);}
            };
            menu.Items.Add(entry);
        }
    }

    private static Task<byte[]> ReadVideoFrameAsync(string input,TimeSpan at,int maxWidth,int maxHeight,CancellationToken token)=>
        Task.Run(async ()=>{
            if(VideoTrimExport.FindFfmpeg()!=null)return await VideoPreviewFrames.ReadAsync(input,at,maxWidth,maxHeight,token);
            var file=await StorageFile.GetFileFromPathAsync(input).AsTask(token);
            var clip=await MediaClip.CreateFromFileAsync(file).AsTask(token);
            var composition=new MediaComposition();
            composition.Clips.Add(clip);
            var size=clip.GetVideoEncodingProperties();
            if(size.Width==0||size.Height==0)throw new IOException("Video không có khung hình.");
            double scale=Math.Min(1,Math.Min(maxWidth/(double)size.Width,maxHeight/(double)size.Height));
            if(at>=clip.OriginalDuration)at=clip.OriginalDuration-TimeSpan.FromTicks(1);
            using var frame=await composition.GetThumbnailAsync(at,Math.Max(1,(int)(size.Width*scale)),
                Math.Max(1,(int)(size.Height*scale)),VideoFramePrecision.NearestFrame).AsTask(token);
            using var image=frame.AsStreamForRead();
            using var output=new MemoryStream();
            await image.CopyToAsync(output,token);
            return output.ToArray();
        },token);

    private readonly HashSet<string> _thumbnailWrites=[];

    private async Task SaveVideoThumbnailAsync(MediaItem item,string input,TimeSpan? position)
    {
        if(!_thumbnailWrites.Add(item.Id))return;
        VideoThumbnailButton.IsEnabled=false;
        string destination=DocumentStore.ThumbnailPath(item.Id);
        string temp=destination+"."+Guid.NewGuid().ToString("N")+".png";
        try{
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            if(position is {} at){
                using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(25));
                var bytes=await ReadVideoFrameAsync(input,at,480,320,timeout.Token);
                await StaImageWorker.Run(()=>{
                    using var data=new MemoryStream(bytes);
                    var encoder=new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(data,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad));
                    using var output=File.Create(temp);
                    encoder.Save(output);
                });

            }else{
                await StaImageWorker.Run(()=>{
                    using var stream=File.OpenRead(input);
                    var header=BitmapDecoder.Create(stream,BitmapCreateOptions.DelayCreation,BitmapCacheOption.None).Frames[0];
                    int width=Math.Max(1,(int)(header.PixelWidth*Math.Min(1,Math.Min(480d/header.PixelWidth,320d/header.PixelHeight))));
                    stream.Position=0;
                    var image=new BitmapImage();
                    image.BeginInit();image.CacheOption=BitmapCacheOption.OnLoad;image.DecodePixelWidth=width;image.StreamSource=stream;image.EndInit();
                    var encoder=new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    using var output=File.Create(temp);encoder.Save(output);
                });
            }
            if(!_store.Items.Contains(item))return;
            var bitmap=await Task.Run(()=>MediaStore.LoadImage(temp,240));
            if(!_store.Items.Contains(item))return;
            File.Move(temp,destination,true);
            item.Thumbnail=bitmap;
            _store.RequestThumbnail(item);
            SetStatus("Đã cập nhật ảnh trên ô video trong Thư viện.");
        }finally{
            File.Delete(temp);
            _thumbnailWrites.Remove(item.Id);
            VideoThumbnailButton.IsEnabled=true;
        }
    }
}
