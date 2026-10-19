using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace CaptureFoundation;
public sealed class RecordingBar : Window
{
    public bool ExcludedFromCapture {get;private set;}
    private readonly TextBlock _label=new(){Foreground=Brushes.White,VerticalAlignment=VerticalAlignment.Center,Margin=new(12,0,16,0)};
    private readonly Button _pause=new(){Content="Tạm dừng"};
    private readonly System.Diagnostics.Stopwatch _clock=new();
    private readonly DispatcherTimer _timer=new(){Interval=TimeSpan.FromMilliseconds(250)};
    private readonly Action _stop;
    private bool _canClose;
    public RecordingBar(Action pause,Action stop,DisplayInfo display)
    {
        _stop=stop;Title="Đang quay • Capture Foundation";Width=440;Height=64;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;Topmost=true;ShowInTaskbar=true;Background=new SolidColorBrush(Color.FromRgb(28,42,58));
        var row=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Center};Content=row;
        _label.Text="● 00:00:00";row.Children.Add(_label);row.Children.Add(_pause);
        var end=new Button{Content="■ Kết thúc",Background=new SolidColorBrush(Color.FromRgb(185,51,63)),Foreground=Brushes.White};row.Children.Add(end);
        _pause.Click+=(_,_)=>{pause();if(_clock.IsRunning){_clock.Stop();_pause.Content="Tiếp tục";}else{_clock.Start();_pause.Content="Tạm dừng";}};
        end.Click+=(_,_)=>RequestStop();
        MouseLeftButtonDown+=(_,_)=>DragMove();
        SourceInitialized+=(_,_)=>{var hwnd=new WindowInteropHelper(this).Handle;ExcludedFromCapture=Native.SetWindowDisplayAffinity(hwnd,0x11);};
        Loaded+=(_,_)=>{var hwnd=new WindowInteropHelper(this).Handle;Native.GetWindowRect(hwnd,out var r);Native.SetWindowPos(hwnd,new nint(-1),display.Bounds.Right-r.Pixels.Width-24,display.Bounds.Y+24,r.Pixels.Width,r.Pixels.Height,0x40);_clock.Start();_timer.Start();};
        _timer.Tick+=(_,_)=>_label.Text=$"● {_clock.Elapsed:hh\\:mm\\:ss}";
        Closing+=(_,e)=>{if(!_canClose){e.Cancel=true;RequestStop();}};
    }
    public void RequestStop(){_pause.IsEnabled=false;_label.Text="Đang lưu…";_timer.Stop();_stop();}
    public void Finish(){_canClose=true;_timer.Stop();_clock.Stop();Close();}
    public void PauseFromHotkey()=>_pause.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
}
