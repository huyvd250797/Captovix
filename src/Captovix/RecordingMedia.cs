using System.IO;
using System.Windows.Media;

namespace Captovix;

internal sealed record RecordingMediaInfo(int Width,int Height,TimeSpan Duration);

internal static class RecordingMedia
{
    internal static async Task<RecordingMediaInfo> VerifyAsync(string path,CancellationToken cancellation)
    {
        await Task.Run(()=>RecordingContainer.Verify(path),cancellation);
        var ready=new TaskCompletionSource<RecordingMediaInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        var player=new MediaPlayer{Volume=0};
        player.MediaOpened+=(_,_)=>{
            if(player.NaturalVideoWidth<=0||player.NaturalVideoHeight<=0||
                !player.NaturalDuration.HasTimeSpan||player.NaturalDuration.TimeSpan<=TimeSpan.Zero)
                ready.TrySetException(new InvalidDataException("MP4 chưa có hình hoặc thời lượng hợp lệ."));
            else ready.TrySetResult(new(player.NaturalVideoWidth,player.NaturalVideoHeight,player.NaturalDuration.TimeSpan));
        };
        player.MediaFailed+=(_,e)=>ready.TrySetException(new InvalidDataException("Windows chưa mở được MP4 đã ghi.",e.ErrorException));
        try{
            player.Open(new Uri(Path.GetFullPath(path)));
            return await ready.Task.WaitAsync(TimeSpan.FromSeconds(15),cancellation);
        }catch(TimeoutException error){
            throw new IOException("Windows chưa xác nhận mở được MP4 sau 15 giây. Tệp tạm được giữ để kiểm tra.",error);
        }finally{
            player.Close();
        }
    }
}
