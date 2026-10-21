using System.IO;
using ScreenRecorderLib;

namespace Captovix;
public record CaptureRequest(bool Video,PixelRect Region,WindowInfo? Window,IReadOnlyList<DisplayInfo> Displays,bool Cursor,bool Mic,bool SystemAudio,int Fps,bool Hardware);
public sealed class CaptureEngine : IDisposable
{
    private Recorder? _recorder;
    private TaskCompletionSource<string>? _complete;
    private TaskCompletionSource? _started;
    public event Action<string>? Status;
    public bool IsPaused {get;private set;}
    public Task<string> Completion => _complete?.Task??Task.FromException<string>(new InvalidOperationException("Chưa bắt đầu ghi hình."));
    public async Task StartAsync(CaptureRequest request,string path)
    {
        if(_complete!=null)throw new InvalidOperationException("Một phiên ghi hình đang hoạt động.");
        _complete=new(TaskCreationOptions.RunContinuationsAsynchronously);_started=new(TaskCreationOptions.RunContinuationsAsynchronously);
        // Photos of the desktop use native GDI pixels. The video engine rounds
        // source and output rectangles to even sizes, including Screenshot mode.
        if(!request.Video && request.Window==null){
            try{
                Status?.Invoke("Chụp pixel màn hình");
                await Task.Run(()=>DesktopPhotoCapture.Save(request.Region,request.Displays,request.Cursor,path));
                _complete.TrySetResult(path);_started.TrySetResult();return;
            }catch(Exception ex){_complete.TrySetException(ex);_started.TrySetException(ex);throw;}
        }
        var options=RecorderOptions.Default;
        options.SourceOptions=new SourceOptions();
        if(request.Window is { } window) {
            if(!Native.IsWindow(window.Handle)||Native.IsIconic(window.Handle))throw new InvalidOperationException("Cửa sổ đã đóng hoặc đang thu nhỏ. Hãy mở lại và chọn lại nguồn.");
            options.SourceOptions.RecordingSources.Add(new WindowRecordingSource(window.Handle){IsCursorCaptureEnabled=request.Cursor});
        } else {
            foreach(var part in CaptureGeometry.Split(request.Region,request.Displays.Select(d=>(d.Device,d.Bounds))))
                options.SourceOptions.RecordingSources.Add(new DisplayRecordingSource(part.Device){
                    RecorderApi=RecorderApi.DesktopDuplication,
                    SourceRect=new ScreenRect(part.Crop.X,part.Crop.Y,part.Crop.Width,part.Crop.Height),
                    Position=new ScreenPoint(part.X,part.Y),OutputSize=new ScreenSize(part.Crop.Width,part.Crop.Height),IsCursorCaptureEnabled=request.Cursor
                });
            if(options.SourceOptions.RecordingSources.Count==0)throw new InvalidOperationException("Vùng chọn không nằm trên màn hình đang kết nối.");
        }
        options.OutputOptions.RecorderMode=request.Video?RecorderMode.Video:RecorderMode.Screenshot;
        if(request.Window==null)options.OutputOptions.OutputFrameSize=new ScreenSize(request.Region.Width,request.Region.Height);
        options.SnapshotOptions.SnapshotFormat=ImageFormat.PNG;
        options.MouseOptions.IsMousePointerEnabled=request.Cursor;
        options.VideoEncoderOptions.Framerate=request.Fps;
        options.VideoEncoderOptions.Bitrate=8_000_000;
        options.VideoEncoderOptions.IsHardwareEncodingEnabled=request.Hardware;
        options.VideoEncoderOptions.IsFixedFramerate=true;
        options.AudioOptions.IsAudioEnabled=request.Video&&(request.Mic||request.SystemAudio);
        if(request.Video&&request.Mic)options.AudioOptions.AudioSources.Add(CaptureAudioSource.Default??throw new InvalidOperationException("Không tìm thấy microphone mặc định. Kiểm tra quyền Microphone của Windows hoặc tắt Mic."));
        if(request.Video&&request.SystemAudio)options.AudioOptions.AudioSources.Add(LoopbackAudioSource.Default??throw new InvalidOperationException("Không tìm thấy thiết bị phát âm thanh mặc định. Hãy kết nối loa/tai nghe hoặc tắt âm thanh máy."));
        options.LogOptions.IsLogEnabled=true;options.LogOptions.LogFilePath=Path.Combine(App.DataRoot,"recorder.log");
        _recorder=Recorder.CreateRecorder(options);
        _recorder.OnRecordingComplete+=(_,e)=>{_complete.TrySetResult(e.FilePath);_started.TrySetResult();};
        _recorder.OnRecordingFailed+=(_,e)=>{var error=new InvalidOperationException(e.Error);_complete.TrySetException(error);_started.TrySetException(error);};
        _recorder.OnStatusChanged+=(_,e)=>{Status?.Invoke(e.Status.ToString());if(e.Status==RecorderStatus.Recording)_started.TrySetResult();};
        _recorder.Record(path);
        await _started.Task.WaitAsync(TimeSpan.FromSeconds(20));
    }
    public void Stop()
    {
        _recorder?.Stop();
        var completion=_complete;
        if(completion!=null&&!completion.Task.IsCompleted)_=WatchStopAsync(completion);
    }
    private static async Task WatchStopAsync(TaskCompletionSource<string> completion)
    {
        try{await completion.Task.WaitAsync(TimeSpan.FromSeconds(45));}
        catch(TimeoutException){completion.TrySetException(new TimeoutException("Engine không hoàn tất file sau 45 giây. File pending được giữ để kiểm tra; không được xem là bản quay thành công."));}
        catch{ /* Caller observes the original recording exception. */ }
    }
    public void TogglePause(){if(_recorder==null)return;if(IsPaused)_recorder.Resume();else _recorder.Pause();IsPaused=!IsPaused;}
    public void Dispose(){_recorder?.Dispose();_recorder=null;IsPaused=false;}
}
