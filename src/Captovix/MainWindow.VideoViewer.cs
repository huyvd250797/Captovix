using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
namespace Captovix;
public partial class MainWindow
{
    private string? _lastVideoExport;
    private Slider? _fullscreenSeek;
    private TextBlock? _fullscreenTime;
    private Button? _fullscreenPlay;
    private readonly DispatcherTimer _viewerIdle=new(){Interval=TimeSpan.FromSeconds(2)};
    private void VideoBack10_Click(object sender,RoutedEventArgs e)=>SkipVideo(-10);
    private void VideoForward10_Click(object sender,RoutedEventArgs e)=>SkipVideo(10);
    private void SkipVideo(double seconds){if(VideoCanEdit&&_timeline is {} timeline)SeekVideoProject(TimeSpan.FromSeconds(Math.Clamp(timeline.Project.Position.TotalSeconds+seconds,0,timeline.Project.Duration.TotalSeconds)));}
    private void OpenVideoExportFolder_Click(object sender,RoutedEventArgs e){if(_lastVideoExport==null)return;try{var info=new ProcessStartInfo("explorer.exe"){UseShellExecute=true};info.ArgumentList.Add("/select,");info.ArgumentList.Add(_lastVideoExport);Process.Start(info);}catch(Exception error){App.Log(error);SetStatus("Chưa mở được thư mục: "+error.Message);}}
    private void InitializeViewerControls(){
        void Reveal(){VideoTools.Opacity=1;VideoTools.IsHitTestVisible=true;VideoViewport.Cursor=Cursors.Arrow;_viewerIdle.Stop();if(!_videoStudio&&_videoFullscreen==null&&_videoItem!=null)_viewerIdle.Start();}
        PreviewMouseMove+=(_,_)=>{if(_videoItem!=null&&!_videoStudio)Reveal();};VideoHost.PreviewMouseMove+=(_,_)=>Reveal();VideoHost.PreviewMouseDown+=(_,_)=>Reveal();VideoTools.PreviewMouseMove+=(_,_)=>Reveal();VideoTools.PreviewKeyDown+=(_,_)=>Reveal();
        _viewerIdle.Tick+=(_,_)=>{_viewerIdle.Stop();if(_videoStudio||_videoFullscreen!=null )return;if(Mouse.Captured!=null){_viewerIdle.Start();return;}VideoTools.Opacity=0;VideoTools.IsHitTestVisible=false;VideoViewport.Cursor=Cursors.None;};
        InitializeVideoButtons();
        VideoTools.IsVisibleChanged+=(_,_)=>Reveal();
    }
    private FrameworkElement CreateFullscreenControls(Window window){
        var body=new StackPanel();var buttons=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Center};body.Children.Add(buttons);
        void Add(string icon,string label,RoutedEventHandler action){var button=new Button{Content=AppIcons.Content(icon,"",20),ToolTip=label,Width=46,Height=40,Margin=new(4),Padding=new(8),Foreground=Brushes.White,Background=new SolidColorBrush(Color.FromRgb(43,46,54)),BorderBrush=new SolidColorBrush(Color.FromRgb(78,82,94))};System.Windows.Automation.AutomationProperties.SetName(button,label);button.Click+=action;buttons.Children.Add(button);}
        Add("VideoBack10","Lùi 10 giây",VideoBack10_Click);Add("RecordingPlay","Phát",ToggleVideoPlayback_Click);_fullscreenPlay=(Button)buttons.Children[1];Add("VideoForward10","Tiến 10 giây",VideoForward10_Click);Add("FullscreenExit","Thoát toàn màn hình",(_,_)=>window.Close());
        _fullscreenSeek=new Slider{Minimum=0,IsMoveToPointEnabled=true,Margin=new(12,6,12,6),Style=(Style)FindResource("PercentageSlider")};
        _fullscreenSeek.ValueChanged+=(_,_)=>{if(!_syncVideoSeek)SeekVideoProject(TimeSpan.FromSeconds(_fullscreenSeek.Value));};body.Children.Add(_fullscreenSeek);
        _fullscreenTime=new TextBlock{Foreground=Brushes.White,HorizontalAlignment=HorizontalAlignment.Center};body.Children.Add(_fullscreenTime);
        var border=new Border{Background=new SolidColorBrush(Color.FromArgb(225,28,30,36)),CornerRadius=new(10),Padding=new(12),Margin=new(24),MaxWidth=1000,HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Bottom,Child=body};
        SyncFullscreenControls();return border;
    }
    private void InitializeVideoButtons(){
        VideoBackButton.Content=AppIcons.Content("VideoBack10","",20);VideoForwardButton.Content=AppIcons.Content("VideoForward10","",20);
        foreach(var button in new[]{VideoViewerExportButton,VideoExportButton}){button.Content=AppIcons.Content("Export","Xuất MP4",18);button.Style=(Style)FindResource("Primary");button.MinHeight=40;button.Padding=new(16,8,16,8);}
    }
    private void SyncFullscreenControls(){if(_timeline is not {} timeline)return;bool old=_syncVideoSeek;_syncVideoSeek=true;try{if(_fullscreenSeek is {} slider){slider.Maximum=timeline.Project.Duration.TotalSeconds;slider.Value=timeline.Project.Position.TotalSeconds;}if(_fullscreenTime!=null)_fullscreenTime.Text=VideoTimecode.Format(timeline.Project.Position)+" / "+VideoTimecode.Format(timeline.Project.Duration);if(_fullscreenPlay!=null){_fullscreenPlay.Content=AppIcons.Content(_playing?"RecordingPause":"RecordingPlay","",20);_fullscreenPlay.ToolTip=_playing?"Tạm dừng":"Phát";System.Windows.Automation.AutomationProperties.SetName(_fullscreenPlay,_playing?"Tạm dừng":"Phát");}}finally{_syncVideoSeek=old;}}
}
