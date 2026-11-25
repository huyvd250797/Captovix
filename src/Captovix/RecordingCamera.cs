using System.IO;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace Captovix;

internal sealed class RecordingCamera : IDisposable
{
    private readonly MediaCapture _device=new();
    private readonly string _path;
    private long _started;
    private long _lostAt;
    private bool _recording;
    internal event Action<string>? Lost;

    internal RecordingCamera(string path)
    {
        _path=path;
        _device.Failed+=(_,error)=>{
            Interlocked.CompareExchange(ref _lostAt,RecordingAudioClock.Now,0);
            Lost?.Invoke("Camera đã ngắt. Tiếp tục quay màn hình và giữ phần camera đã ghi. "+error.Message);
        };
    }

    internal async Task StartAsync(string deviceId,CancellationToken token)
    {
        await _device.InitializeAsync(new MediaCaptureInitializationSettings{
            VideoDeviceId=deviceId,StreamingCaptureMode=StreamingCaptureMode.Video,SharingMode=MediaCaptureSharingMode.ExclusiveControl
        }).AsTask(token);
        var folder=await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(_path)!).AsTask(token);
        var target=await folder.CreateFileAsync(Path.GetFileName(_path),CreationCollisionOption.FailIfExists).AsTask(token);
        await _device.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga),target).AsTask(token);
        _started=RecordingAudioClock.Now;
        _recording=true;
    }

    internal async Task<VideoCamera?> FinishAsync(RecordingAudioClock clock,TimeSpan screenLength)
    {
        if(!_recording)return null;
        _recording=false;
        try{await _device.StopRecordAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(8));}
        catch(Exception error){App.Log(error);}
        if(!File.Exists(_path)||new FileInfo(_path).Length==0)return null;
        var input=await VideoTrimExport.ReadInputAsync(_path);
        long lost=Interlocked.Read(ref _lostAt);
        var usable=lost==0?input.Duration:TimeSpan.FromTicks(Math.Clamp(lost-_started,0,input.Duration.Ticks));
        var sync=clock.CameraSpans(_started,usable,screenLength).Select(s=>new CameraSyncSpan(s.Screen,s.Camera,s.Length)).ToArray();
        return sync.Length==0?null:new VideoCamera(_path,input.Duration){Sync=sync};
    }

    internal static string PathFor(string screen)=>screen+".camera.mp4";
    internal static string MetadataFor(string screen)=>screen+".camera.json";
    public void Dispose()=>_device.Dispose();
}
