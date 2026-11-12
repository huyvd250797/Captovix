using System.IO;
using ScreenRecorderLib;

namespace Captovix;
public record CaptureRequest(bool Video,PixelRect Region,WindowInfo? Window,IReadOnlyList<DisplayInfo> Displays,bool Cursor,bool Mic,bool SystemAudio,int Fps,bool Hardware)
{
    public RecordingProfile? Profile {get;init;}
    public bool HighlightClicks {get;init;}
}
public sealed class CaptureEngine : IDisposable
{
    private Recorder? _recorder;
    private TaskCompletionSource<string>? _complete;
    private TaskCompletionSource? _started;
    public event Action<string>? Status;
    internal RecordingAudioClock? AudioClock {get;init;}
    internal RecordingAudioSettings? AudioDevices {get;init;}
    private readonly Dictionary<bool,AudioSourceBase> _mixedSources=[];
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
            foreach(var part in CaptureGeometry.Split(request.Region,request.Displays.Select(d=>(d.Device,d.Bounds)))){
                var source=new DisplayRecordingSource(part.Device){
                    RecorderApi=RecorderApi.DesktopDuplication,
                    SourceRect=new ScreenRect(part.Crop.X,part.Crop.Y,part.Crop.Width,part.Crop.Height),
                    Position=new ScreenPoint(part.X,part.Y),OutputSize=new ScreenSize(part.Crop.Width,part.Crop.Height),IsCursorCaptureEnabled=request.Cursor
                };
                if(request.Video)source.Stretch=StretchMode.None;
                options.SourceOptions.RecordingSources.Add(source);
            }
            if(options.SourceOptions.RecordingSources.Count==0)throw new InvalidOperationException("Vùng chọn không nằm trên màn hình đang kết nối.");
        }
        options.OutputOptions.RecorderMode=request.Video?RecorderMode.Video:RecorderMode.Screenshot;
        if(request.Video||request.Window==null){
            var frame=request.Video?request.Profile?.Frame(request.Region)??CaptureGeometry.RecordingFrame(request.Region):request.Region;
            options.OutputOptions.OutputFrameSize=new ScreenSize(frame.Width,frame.Height);
            if(request.Video)options.OutputOptions.Stretch=frame.Width>=request.Region.Width&&frame.Height>=request.Region.Height?StretchMode.None:StretchMode.Uniform;
        }
        options.SnapshotOptions.SnapshotFormat=ImageFormat.PNG;
        options.MouseOptions.IsMousePointerEnabled=request.Cursor;
        options.VideoEncoderOptions.Framerate=request.Fps;
        if(request.Video&&request.HighlightClicks)options.MouseOptions.IsMouseClicksDetected=true;
        options.VideoEncoderOptions.Bitrate=request.Profile is {} profile?profile.Bitrate(profile.Frame(request.Region)):8_000_000;
        options.VideoEncoderOptions.IsHardwareEncodingEnabled=request.Hardware;
        options.VideoEncoderOptions.IsFixedFramerate=true;
        options.AudioOptions.IsAudioEnabled=request.Video&&(request.Mic||request.SystemAudio);
        if(request.Video&&request.Mic){
            var mic=AudioDevices?.Microphone is {} input?new CaptureAudioSource(input.Id):CaptureAudioSource.Default
                ??throw new InvalidOperationException("Không tìm thấy micro.");
            _mixedSources[false]=mic;
            options.AudioOptions.AudioSources.Add(mic);
        }
        if(request.Video&&request.SystemAudio){
            var system=AudioDevices?.System is {} output?new LoopbackAudioSource(output.Id):LoopbackAudioSource.Default
                ??throw new InvalidOperationException("Không tìm thấy thiết bị phát âm thanh.");
            _mixedSources[true]=system;
            options.AudioOptions.AudioSources.Add(system);
        }
        options.LogOptions.IsLogEnabled=true;options.LogOptions.LogFilePath=Path.Combine(App.DataRoot,"recorder.log");
        _recorder=Recorder.CreateRecorder(options);
        _recorder.OnRecordingComplete+=(_,e)=>{AudioClock?.Pause(RecordingAudioClock.Now,true);_complete.TrySetResult(e.FilePath);_started.TrySetResult();};
        _recorder.OnRecordingFailed+=(_,e)=>{AudioClock?.Pause(RecordingAudioClock.Now,true);var error=new InvalidOperationException(e.Error);_complete.TrySetException(error);_started.TrySetException(error);};
        _recorder.OnStatusChanged+=(_,e)=>{
            long now=RecordingAudioClock.Now;
            if(e.Status==RecorderStatus.Recording){AudioClock?.Resume(now);_started.TrySetResult();}
            else if(e.Status==RecorderStatus.Paused)AudioClock?.Pause(now);
            Status?.Invoke(e.Status.ToString());
        };
        _recorder.Record(path);
        await _started.Task.WaitAsync(TimeSpan.FromSeconds(20));
    }
    public void Stop()
    {
        var completion=_complete;
        if(completion?.Task.IsCompleted!=false)return;
        AudioClock?.Pause(RecordingAudioClock.Now,true);
        try{_recorder?.Stop();}
        catch(Exception error){completion.TrySetException(error);return;}
        if(completion!=null&&!completion.Task.IsCompleted)_=WatchStopAsync(completion);
    }
    private static async Task WatchStopAsync(TaskCompletionSource<string> completion)
    {
        try{await completion.Task.WaitAsync(TimeSpan.FromSeconds(45));}
        catch(TimeoutException){completion.TrySetException(new TimeoutException("Engine không hoàn tất file sau 45 giây. File pending được giữ để kiểm tra; không được xem là bản quay thành công."));}
        catch{ /* Caller observes the original recording exception. */ }
    }
    public bool TogglePause()
    {
        if(_recorder==null||_complete?.Task.IsCompleted!=false)return IsPaused;
        if(IsPaused)_recorder.Resume();else _recorder.Pause();
        return IsPaused=!IsPaused;
    }
    internal void DisableAudioSource(bool system)
    {
        if(_recorder==null||!_mixedSources.TryGetValue(system,out var source)||_complete?.Task.IsCompleted!=false)return;
        source.IsAudioCaptureEnabled=false;
        var options=_recorder.GetDynamicOptionsBuilder();
        options.SetUpdatedAudioSource(source);
        options.Apply();
    }
    public void Dispose(){_recorder?.Dispose();_recorder=null;IsPaused=false;}
}
