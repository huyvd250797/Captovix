using System.IO;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Captovix;

internal sealed record AudioEndpoint(string Id,string Name)
{
    public override string ToString()=>Name;
}
internal sealed record RecordingAudioSettings(AudioEndpoint? Microphone,AudioEndpoint? System);

internal static class RecordingAudioFiles
{
    internal static string PathFor(string video,bool system)=>video+(system?".system.wav":".mic.wav");
    internal static IEnumerable<string> Existing(string video)
    {
        foreach(bool system in new[]{false,true}){
            string path=PathFor(video,system);
            if(File.Exists(path))yield return path;
        }
    }
}

internal sealed class RecordingAudio : IDisposable
{
    private readonly List<(bool System,AudioInput Input)> _inputs=[];
    internal RecordingAudioClock Clock {get;}=new();
    internal event Action<bool,string>? Lost;
    internal IReadOnlyList<string> Warnings=>_inputs.Select(i=>i.Input.Warning).OfType<string>().ToArray();
    internal (bool Enabled,double Level,string? Warning) Meter(bool system)
    {
        foreach(var entry in _inputs)
            if(entry.System==system)return (true,entry.Input.Level,entry.Input.Warning);
        return (false,0,null);
    }
    internal IEnumerable<bool> LostSources=>_inputs.Where(i=>i.Input.Warning!=null).Select(i=>i.System);

    internal void Start(RecordingAudioSettings settings,string pending)
    {
        Add(settings.Microphone,false);
        Add(settings.System,true);
        void Add(AudioEndpoint? endpoint,bool system){
            if(endpoint==null)return;
            var input=new AudioInput(endpoint,system,Clock,RecordingAudioFiles.PathFor(pending,system),message=>Lost?.Invoke(system,message));
            _inputs.Add((system,input));
        }
    }

    internal void Complete(TimeSpan duration)
    {
        Clock.Pause(RecordingAudioClock.Now,true);
        foreach(var (_,input) in _inputs)input.Complete(duration);
    }

    public void Dispose()
    {
        foreach(var (_,input) in _inputs)input.Dispose();
    }

    internal static (AudioEndpoint[] Devices,string? DefaultId) FindEndpoints(bool system)
    {
        using var enumerator=new MMDeviceEnumerator();
        var flow=system?DataFlow.Render:DataFlow.Capture;
        var devices=new List<AudioEndpoint>();
        using var collection=enumerator.EnumerateAudioEndPoints(flow,DeviceState.Active);
        foreach(var device in collection){
            using(device)devices.Add(new(device.ID,device.FriendlyName));
        }
        using var current=enumerator.TryGetDefaultAudioEndpoint(flow,Role.Multimedia,out var endpoint)?endpoint:null;
        return (devices.ToArray(),current?.ID);
    }
}

internal sealed class AudioInput : IDisposable
{
    private const int Rate=48000;
    private readonly MMDevice _device;
    private readonly WasapiRecorder _recorder;
    private readonly RecordingWaveFile? _writer;
    private readonly RecordingAudioClock? _clock;
    private readonly Action<string>? _lost;
    private readonly int _block;
    private readonly string _label;
    private long _signalAt;
    private float _peak;
    private int _closing;
    private string? _warning;
    internal string? Warning=>Volatile.Read(ref _warning);
    internal double Level=>RecordingAudioClock.Now-Interlocked.Read(ref _signalAt)>TimeSpan.TicksPerMillisecond*250?0:Volatile.Read(ref _peak);

    internal AudioInput(AudioEndpoint endpoint,bool system,RecordingAudioClock? clock=null,string? output=null,Action<string>? lost=null)
    {
        _clock=clock;
        _lost=lost;
        _label=$"{(system?"Âm thanh máy":"Micro")} · {endpoint.Name}";
        using var enumerator=new MMDeviceEnumerator();
        _device=enumerator.GetDevice(endpoint.Id);
        var format=WaveFormat.CreateIeeeFloatWaveFormat(Rate,system?2:1);
        _block=format.BlockAlign;
        WasapiRecorder? recorder=null;
        bool workerStarted=false;
        try{
            var builder=new WasapiRecorderBuilder().WithDevice(_device).WithFormat(format).WithBufferLength(30);
            if(system)builder.WithLoopbackCapture().WithPollingSync();
            _recorder=recorder=builder.Build();
            if(output!=null)_writer=new RecordingWaveFile(output,Rate,format.Channels);
            _recorder.DataAvailable+=OnPacket;
            _recorder.RecordingStopped+=(_,args)=>{
                if(Volatile.Read(ref _closing)==0)Report("đã ngắt thu.",args.Exception);
            };
            _recorder.StartRecording();
            workerStarted=true;
            if(!SpinWait.SpinUntil(()=>_recorder.CaptureState!=CaptureState.Starting,TimeSpan.FromSeconds(1)))
                throw new TimeoutException("Thiết bị chưa khởi động.");
            if(_recorder.CaptureState!=CaptureState.Capturing)throw new IOException("Thiết bị đã dừng khi khởi động.");
        }catch(Exception error){
            Interlocked.Exchange(ref _closing,1);
            void Release(){
                try{recorder?.Dispose();}catch(Exception cleanup){App.Log(cleanup);}
                try{_writer?.Dispose();}catch(Exception cleanup){App.Log(cleanup);}
                _device.Dispose();
                if(output!=null)File.Delete(output);
            }
            if(workerStarted&&recorder?.CaptureState==CaptureState.Starting){
                // The native worker can transition Starting -> Capturing even if Stop
                // was requested. Do not join it from the WPF dispatcher in that state.
                _=Task.Run(async ()=>{
                    while(recorder.CaptureState==CaptureState.Starting)await Task.Delay(100).ConfigureAwait(false);
                    try{Release();}catch(Exception cleanup){App.Log(cleanup);}
                });
            }else{
                try{Release();}catch(Exception cleanup){App.Log(cleanup);}
            }
            throw new IOException($"{_label} không mở được. {error.Message}",error);
        }
    }

    private void Report(string detail,Exception? error)
    {
        if(error!=null)App.Log(error);
        var message=$"{_label} {detail}";
        if(Interlocked.CompareExchange(ref _warning,message,null)==null)_lost?.Invoke(message);
    }

    private void OnPacket(ReadOnlySpan<byte> data,AudioClientBufferFlags flags,long devicePosition,long qpcPosition)
    {
        if(Volatile.Read(ref _closing)!=0)return;
        bool silent=flags.HasFlag(AudioClientBufferFlags.Silent);
        float peak=0;
        if(!silent)foreach(float sample in MemoryMarshal.Cast<byte,float>(data))
            if(float.IsFinite(sample))peak=Math.Max(peak,Math.Abs(sample));
        Volatile.Write(ref _peak,Math.Min(peak,1));
        long now=RecordingAudioClock.Now;
        Interlocked.Exchange(ref _signalAt,now);
        if(_writer==null||_clock==null||silent||Warning!=null)return;

        int frames=data.Length/_block;
        if(qpcPosition<=0||qpcPosition>now+TimeSpan.TicksPerMillisecond*100||flags.HasFlag(AudioClientBufferFlags.TimestampError))
            qpcPosition=now-frames*TimeSpan.TicksPerSecond/Rate;
        try{
            foreach(var slice in _clock.Place(qpcPosition,frames,Rate)){
                var samples=MemoryMarshal.Cast<byte,float>(data.Slice(slice.Offset*_block,slice.Count*_block));
                _writer.Write(slice.OutputFrame,samples);
            }
        }catch(Exception error){Report("gặp lỗi khi lưu tệp âm thanh.",error);}
    }

    internal void Complete(TimeSpan duration)=>Close(duration);

    public void Dispose()=>Close(null);

    private void Close(TimeSpan? duration)
    {
        if(Interlocked.Exchange(ref _closing,1)!=0)return;
        try{
            _recorder.Dispose();
            if(duration is {} end)
                _writer?.Complete((long)Math.Round(end.TotalSeconds*Rate));
        }catch(Exception error){Report("chưa hoàn tất tệp âm thanh.",error);}
        finally{
            try{_writer?.Dispose();}
            catch(Exception error){Report("chưa hoàn tất tệp âm thanh.",error);}
            _device.Dispose();
        }
    }
}
