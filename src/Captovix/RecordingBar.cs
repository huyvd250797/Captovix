using System.Diagnostics;
using System.Windows.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Captovix;

public sealed class RecordingBar : Window
{
    public bool ExcludedFromCapture {get;private set;}
    private readonly TextBlock _label=new(){VerticalAlignment=VerticalAlignment.Center,Margin=new(8,0,12,0)};
    private readonly TextBlock _audioWarning=new(){Visibility=Visibility.Collapsed,TextWrapping=TextWrapping.Wrap,Margin=new(8,4,8,0)};
    private readonly Button _pause=new(){Content="Tạm dừng"};
    private readonly Button _marker=new(){Content="Đánh dấu",ToolTip="Đánh dấu · Ctrl+Shift+5"};
    private readonly Button _end=new(){Content="Dừng"};
    private readonly Stopwatch _clock=Stopwatch.StartNew();
    private readonly DispatcherTimer _timer=new(){Interval=TimeSpan.FromMilliseconds(80)};
    private readonly Func<bool> _togglePause;
    private readonly Action _stop;
    private bool _canClose,_saving;
    private readonly RecordingAudio? _audio;
    private readonly RecordingPresenter? _presenter;
    private readonly (ProgressBar Level,TextBlock Status) _mic,_system;

    internal PixelRect? ScreenBounds()
    {
        var hwnd=new WindowInteropHelper(this).Handle;
        return hwnd!=0&&Native.GetWindowRect(hwnd,out var bounds)?bounds.Pixels:null;
    }

    internal RecordingBar(string source,Func<bool> togglePause,Action stop,DisplayInfo display,RecordingAudio? audio,RecordingPresenter presenter)
    {
        _pause.Content=AppIcons.Content("RecordingPause","Tạm dừng");
        _end.Content=AppIcons.Content("RecordingStop","Dừng");
        _end.Background=Brushes.Firebrick;
        _end.Foreground=Brushes.White;
        _togglePause=togglePause;
        _stop=stop;
        _audio=audio;
        _presenter=presenter;
        Title="Quay màn hình · Captovix";
        Width=505;
        SizeToContent=SizeToContent.Height;
        WindowStyle=WindowStyle.None;
        ResizeMode=ResizeMode.NoResize;
        Topmost=true;
        ShowInTaskbar=true;
        SetResourceReference(BackgroundProperty,"Surface");

        var panel=new StackPanel{Margin=new Thickness(8)};
        var sourceLabel=new TextBlock{Text=source,ToolTip=source,TextTrimming=TextTrimming.CharacterEllipsis,Margin=new(8,2,8,5)};
        panel.Children.Add(sourceLabel);
        var row=new DockPanel();
        var buttons=new StackPanel{Orientation=Orientation.Horizontal};
        buttons.Children.Add(_marker);
        buttons.Children.Add(_pause);
        buttons.Children.Add(_end);
        DockPanel.SetDock(buttons,Dock.Right);
        row.Children.Add(buttons);
        row.Children.Add(_label);
        panel.Children.Add(row);
        _mic=AddMeter("Micro");
        _system=AddMeter("Âm thanh máy");
        panel.Children.Add(_audioWarning);
        (ProgressBar,TextBlock) AddMeter(string label)
        {
            var line=new DockPanel{Margin=new(8,2,8,3)};
            var title=new TextBlock{Text=label,Width=110};
            var status=new TextBlock{Width=90,TextAlignment=TextAlignment.Right};
            var meter=new ProgressBar{Maximum=1,Height=6,VerticalAlignment=VerticalAlignment.Center,Focusable=false};
            AutomationProperties.SetName(meter,"Mức âm · "+label);
            DockPanel.SetDock(status,Dock.Right);
            line.Children.Add(title);line.Children.Add(status);line.Children.Add(meter);
            panel.Children.Add(line);
            return (meter,status);
        }
        Content=panel;
        _marker.Click+=(_,_)=>Mark();
        _pause.Click+=(_,_)=>PauseFromHotkey();
        _end.Click+=(_,_)=>RequestStop();
        MouseLeftButtonDown+=(_,e)=>{if(e.LeftButton==System.Windows.Input.MouseButtonState.Pressed)DragMove();};
        SourceInitialized+=(_,_)=>ExcludedFromCapture=Native.SetWindowDisplayAffinity(new WindowInteropHelper(this).Handle,0x11);
        Loaded+=(_,_)=>{
            var hwnd=new WindowInteropHelper(this).Handle;
            if(Native.GetWindowRect(hwnd,out var r))
                Native.SetWindowPos(hwnd,new nint(-1),display.Bounds.Right-r.Pixels.Width-24,display.Bounds.Y+24,r.Pixels.Width,r.Pixels.Height,0x40);
            _timer.Start();
        };
        _timer.Tick+=(_,_)=>RenderState();
        Closing+=(_,e)=>{if(!_canClose){e.Cancel=true;RequestStop();}};
        RenderState();
    }

    private void RenderState()
    {
        var state=_saving?"Đang lưu…":_clock.IsRunning?"Đang quay":"Tạm dừng";
        UpdateMeter(_mic,false);UpdateMeter(_system,true);
        _label.Text=$"{state} · {_clock.Elapsed:hh\\:mm\\:ss}";
    }

    private void UpdateMeter((ProgressBar Level,TextBlock Status) meter,bool system)
    {
        var input=_audio?.Meter(system)??(false,0d,(string?)null);
        double level=_clock.IsRunning&&!_saving&&input.Item3==null?input.Item2:0;
        meter.Level.Value=Math.Sqrt(level);
        meter.Status.Text=!input.Item1?"Tắt":input.Item3!=null?"Mất nguồn":_saving?"Đã dừng":!_clock.IsRunning?"Tạm dừng":level>0.002?"Có tín hiệu":"Chờ tín hiệu";
    }

    internal void Mark()
    {
        if(!_saving&&_presenter?.Mark()==true){
            _marker.Content=$"Mốc ({_presenter.MarkerCount})";
            if(_presenter.MarkerCount==150){_marker.IsEnabled=false;_marker.ToolTip="Đã đạt giới hạn 150 mốc.";}
        }
    }

    internal void ShowAudioWarning(string message)
    {
        _audioWarning.Text=string.IsNullOrEmpty(_audioWarning.Text)?message:_audioWarning.Text+"\n"+message;
        _audioWarning.Visibility=Visibility.Visible;
    }

    private void RequestStop(){if(!_saving)_stop();}

    internal void BeginSaving()
    {
        _saving=true;
        _marker.IsEnabled=_pause.IsEnabled=_end.IsEnabled=false;
        _timer.Stop();
        _clock.Stop();
        RenderState();
    }

    public void Finish(){_canClose=true;_timer.Stop();_clock.Stop();Close();}

    public void PauseFromHotkey()
    {
        if(_saving)return;
        try{
            if(_togglePause())_clock.Stop();else _clock.Start();
            _pause.Content=_clock.IsRunning?AppIcons.Content("RecordingPause","Tạm dừng"):AppIcons.Content("RecordingPlay","Tiếp tục");
            _marker.IsEnabled=_clock.IsRunning&&_presenter?.MarkerCount<150;
            RenderState();
        }catch(Exception error){
            App.Log(error);
            RequestStop();
        }
    }
}
