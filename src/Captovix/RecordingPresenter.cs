using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Captovix;

public sealed record RecordingCue(string Kind,long Milliseconds,int? X=null,int? Y=null);

internal sealed class RecordingPresenter(RecordingAudioClock clock,PixelRect source,PixelRect frame,Func<PixelRect?> controls) : IDisposable
{
    private readonly List<RecordingCue> _cues=[];
    private readonly List<RecordingCue> _markers=[];
    private MouseHook? _callback;
    private nint _hook;
    internal static string PathFor(string video)=>video+".cues.json";
    internal int MarkerCount=>_markers.Count;
    internal bool Start()
    {
        _callback=OnMouse;
        _hook=SetWindowsHookEx(14,_callback,GetModuleHandle(null),0);
        return _hook!=0;
    }

    internal bool Mark()
    {
        if(_markers.Count>=150||clock.ActiveElapsed(RecordingAudioClock.Now) is not {} elapsed)return false;
        _markers.Add(new("marker",elapsed/TimeSpan.TicksPerMillisecond));
        return true;
    }

    internal RecordingCue[] Finish(TimeSpan duration,string video)
    {
        Dispose();
        var markers=_markers.Where(c=>c.Milliseconds<duration.TotalMilliseconds).ToArray();
        var clicks=_cues.Where(c=>c.Milliseconds<duration.TotalMilliseconds).ToArray();
        if(clicks.Length>0)File.WriteAllText(PathFor(video),JsonSerializer.Serialize(clicks));
        return markers;
    }

    private nint OnMouse(int code,nint message,nint data)
    {
        var kind=(int)message switch{0x201=>"left",0x204=>"right",0x207=>"middle",_=>null};
        if(code==0&&kind!=null&&_cues.Count<20000&&clock.ActiveElapsed(RecordingAudioClock.Now) is {} elapsed){
            int x=Marshal.ReadInt32(data),y=Marshal.ReadInt32(data,4);
            if(ElementSelection.Contains(source,x,y)&&!(controls() is {} bar&&ElementSelection.Contains(bar,x,y)))
                _cues.Add(new(kind,elapsed/TimeSpan.TicksPerMillisecond,
                    (int)((long)(x-source.X)*frame.Width/source.Width),(int)((long)(y-source.Y)*frame.Height/source.Height)));
        }
        return CallNextHookEx(0,code,message,data);
    }

    public void Dispose()
    {
        if(_hook!=0){UnhookWindowsHookEx(_hook);_hook=0;}
        GC.KeepAlive(_callback);
    }

    private delegate nint MouseHook(int code,nint message,nint data);
    [DllImport("user32.dll",EntryPoint="SetWindowsHookExW")]
    private static extern nint SetWindowsHookEx(int id,MouseHook callback,nint module,uint thread);
    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(nint hook,int code,nint message,nint data);
    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]
    private static extern nint GetModuleHandle(string? name);
}
