using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Captovix;
public sealed class ElementPicker : IDisposable
{
    private readonly TaskCompletionSource<PixelRect?> _done=new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly List<Window> _outlines=[];
    private readonly ElementProbeClient _probe=new();
    private readonly DispatcherTimer _timer=new(){Interval=TimeSpan.FromMilliseconds(100)};
    private readonly TextBlock _label=new(){Foreground=Brushes.White,FontSize=13,TextWrapping=TextWrapping.Wrap,MaxWidth=480,Margin=new Thickness(12)};
    private readonly Window _hint;
    private readonly CancellationTokenSource _cancel=new();
    private readonly MouseProc _callback;
    private nint _hook;
    private ElementCandidate[] _candidates=[];
    private int _index,_lastX=int.MinValue,_lastY=int.MinValue;
    private bool _querying,_pressed,_rightPressed,_disposed;
    private long _lastProbeTime;
    private Native.POINT _down;
    private PixelRect? _drag;
    private readonly IReadOnlyList<DisplayInfo> _displays;
    private ElementPicker(IReadOnlyList<DisplayInfo> displays)
    {
        _displays=displays;for(int i=0;i<4;i++)_outlines.Add(Overlay(Brushes.Teal));
        _hint=Overlay(new SolidColorBrush(Color.FromRgb(24,44,60)));_hint.Content=_label;
        _callback=Hook;_hook=SetWindowsHookEx(14,_callback,GetModuleHandle(null),0);if(_hook==0){Dispose();throw new InvalidOperationException("Không mở được bộ chọn phần tử. Hãy dùng Vùng chọn.");}
        _timer.Tick+=async(_,_)=>await Poll();_timer.Start();_label.Text="Di chuột vào modal / grid · Con lăn lên: khối cha, xuống: khối con\nClick chụp · Giữ và kéo: vùng thủ công · Chuột phải / Esc: hủy";
    }
    public static async Task<PixelRect?> PickAsync(IReadOnlyList<DisplayInfo> displays,CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();if(displays.Count==0)throw new InvalidOperationException("Không tìm thấy màn hình.");using var picker=new ElementPicker(displays);
        using var registration=cancellation.Register(()=>Application.Current.Dispatcher.BeginInvoke(()=>picker.Finish(null)));
        return await picker._done.Task;
    }
    private Window Overlay(Brush background)
    {
        var w=new Window{WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,AllowsTransparency=true,Background=background,ShowInTaskbar=false,ShowActivated=false,Topmost=true,Width=1,Height=1};
        w.SourceInitialized+=(_,_)=>{var h=new WindowInteropHelper(w).Handle;var style=GetWindowLong(h,-20);SetWindowLong(h,-20,style|0x20|0x80|0x8000000);Native.SetWindowDisplayAffinity(h,0x11);};w.Show();return w;
    }
    private async Task Poll()
    {
        if(_done.Task.IsCompleted||_querying||_pressed||!Native.GetCursorPos(out var p))return;
        bool stationary=Math.Abs((long)p.X-_lastX)<3&&Math.Abs((long)p.Y-_lastY)<3;
        if(stationary&&Environment.TickCount64-_lastProbeTime<700)return;
        var previous=stationary&&_candidates.Length>0?_candidates[_index]:null;
        _lastX=p.X;_lastY=p.Y;_lastProbeTime=Environment.TickCount64;_querying=true;
        if(!stationary){_candidates=[];HideOutline();}
        try{
            var target=Native.Windows().FirstOrDefault(w=>Native.GetWindowRect(w.Handle,out var rect)&&ElementSelection.Contains(rect.Pixels,p.X,p.Y));
            if(target==null){_candidates=[];HideOutline();ShowHint(p,"Không thấy cửa sổ. Giữ chuột và kéo để chọn vùng.");return;}
            if(!Native.GetWindowRect(target.Handle,out var window))return;if(!stationary)ShowHint(p,"Đang dò phần tử… Click sau khi khung xuất hiện; hoặc giữ và kéo vùng.");
            var result=await _probe.QueryAsync(new((long)target.Handle,p.X,p.Y,Environment.ProcessId),_cancel.Token);
            if(_done.Task.IsCompleted||_pressed)return;
            Native.GetCursorPos(out var now);if(Math.Abs(now.X-p.X)>8||Math.Abs(now.Y-p.Y)>8){_lastX=int.MinValue;return;}
            var desktop=new PixelRect(_displays.Min(d=>d.Bounds.X),_displays.Min(d=>d.Bounds.Y),_displays.Max(d=>d.Bounds.Right)-_displays.Min(d=>d.Bounds.X),_displays.Max(d=>d.Bounds.Bottom)-_displays.Min(d=>d.Bounds.Y));
            _candidates=ElementSelection.Normalize(result,p.X,p.Y,window.Pixels.Intersect(desktop));_index=previous==null?-1:Array.FindIndex(_candidates,c=>c.Bounds==previous.Bounds);if(_index<0)_index=ElementSelection.Preferred(_candidates);DrawCurrent(now);
        }catch(Exception e){if(!_done.Task.IsCompleted)App.Log(e);}
        finally{_querying=false;}
    }
    private nint Hook(int code,nint w,nint l)
    {
        if(code<0||_done.Task.IsCompleted)return CallNextHookEx(_hook,code,w,l);var data=Marshal.PtrToStructure<MouseData>(l);int msg=(int)w;
        if(msg==0x201){if(Math.Abs((long)data.Point.X-_lastX)>8||Math.Abs((long)data.Point.Y-_lastY)>8){_candidates=[];_lastX=int.MinValue;}_pressed=true;_down=data.Point;_drag=null;return 1;}
        if(msg==0x200&&_pressed){var r=PixelRect.FromPoints(_down.X,_down.Y,data.Point.X,data.Point.Y);if(r.Width>=4&&r.Height>=4){_drag=r;Application.Current.Dispatcher.BeginInvoke(()=>{if(!_disposed){Draw(r);ShowHint(data.Point,$"Vùng thủ công · {r.Width} × {r.Height} px");}});}return 1;}
        if(msg==0x202&&_pressed){_pressed=false;var chosen=_drag??(_candidates.Length>0&&ElementSelection.Contains(_candidates[_index].Bounds,data.Point.X,data.Point.Y)?_candidates[_index].Bounds:(PixelRect?)null);if(chosen!=null)Application.Current.Dispatcher.BeginInvoke(()=>Finish(chosen));else{_lastX=int.MinValue;Application.Current.Dispatcher.BeginInvoke(()=>{if(!_disposed)ShowHint(data.Point,"Chờ khung nhận diện rồi click, hoặc giữ và kéo vùng.");});}return 1;}
        if(msg==0x204){_rightPressed=true;return 1;}
        if(msg==0x205&&_rightPressed){_rightPressed=false;Application.Current.Dispatcher.BeginInvoke(()=>Finish(null));return 1;}
        if(msg==0x20A){_index=ElementSelection.Cycle(_index,(short)(data.MouseDataValue>>16),_candidates.Length);Application.Current.Dispatcher.BeginInvoke(()=>{if(!_disposed)DrawCurrent(data.Point);});return 1;}
        return CallNextHookEx(_hook,code,w,l);
    }
    private void DrawCurrent(Native.POINT p){if(_candidates.Length==0)return;var c=_candidates[_index];Draw(c.Bounds);ShowHint(p,$"{c.Label} · {c.Bounds.Width} × {c.Bounds.Height} px · {_index+1}/{_candidates.Length}\nCon lăn ↑ khối cha / ↓ khối con · Click chụp · Kéo chọn vùng · Esc hủy");}
    private void Draw(PixelRect r)
    {
        PixelRect[] lines=[new(r.X,r.Y,r.Width,2),new(r.X,r.Bottom-2,r.Width,2),new(r.X,r.Y,2,r.Height),new(r.Right-2,r.Y,2,r.Height)];
        for(int i=0;i<4;i++){_outlines[i].Show();var b=lines[i];Native.SetWindowPos(new WindowInteropHelper(_outlines[i]).Handle,-1,b.X,b.Y,Math.Max(2,b.Width),Math.Max(2,b.Height),0x10);}
    }
    private void ShowHint(Native.POINT p,string text)
    {
        _label.Text=text;var d=_displays.FirstOrDefault(d=>ElementSelection.Contains(d.Bounds,p.X,p.Y))??_displays[0];int width=Math.Min(520,d.Bounds.Width-16),height=Math.Min(130,d.Bounds.Height-16);
        int x=Math.Clamp(p.X+22,d.Bounds.X+8,d.Bounds.Right-width-8),y=Math.Clamp(p.Y+28,d.Bounds.Y+8,d.Bounds.Bottom-height-8);
        Native.SetWindowPos(new WindowInteropHelper(_hint).Handle,-1,x,y,width,height,0x10);
    }
    private void HideOutline(){foreach(var w in _outlines)w.Hide();}
    private void Finish(PixelRect? rect){if(_done.Task.IsCompleted)return;_done.TrySetResult(rect);Dispose();}
    public void Dispose(){if(_disposed)return;_disposed=true;_timer.Stop();if(_hook!=0){UnhookWindowsHookEx(_hook);_hook=0;}_cancel.Cancel();_probe.Dispose();foreach(var w in _outlines)w.Close();_outlines.Clear();_hint.Close();}
    private delegate nint MouseProc(int code,nint w,nint l);
    [StructLayout(LayoutKind.Sequential)]private struct MouseData {public Native.POINT Point;public uint MouseDataValue,Flags,Time;public nuint Extra;}
    [DllImport("user32.dll",SetLastError=true)]private static extern nint SetWindowsHookEx(int id,MouseProc callback,nint module,uint thread);
    [DllImport("user32.dll")]private static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("user32.dll")]private static extern nint CallNextHookEx(nint hook,int code,nint w,nint l);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]private static extern nint GetModuleHandle(string? name);
    [DllImport("user32.dll",EntryPoint="GetWindowLongW")]private static extern int GetWindowLong(nint h,int index);
    [DllImport("user32.dll",EntryPoint="SetWindowLongW")]private static extern int SetWindowLong(nint h,int index,int value);
}
