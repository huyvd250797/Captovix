using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Captovix;

public partial class MainWindow
{
    private Window? _videoFullscreen;
    private double _videoZoom=1;
    private bool _addingVideoClips;
    private void PauseVideoForEditing()
    {
        _resumeAfterVideoOpen=false;
        if(!_playing)return;
        if(_videoReady&&_holdClock==null&&_timeline is {} timeline)timeline.Track(timeline.Project.SelectedClip,VideoPreview.Position);
        VideoPreview.Pause();
        SetVideoPlaying(false);
        UpdateVideoPosition();
        MarkVideoProjectDirty(playback:true);
    }

    private void SelectVideoClip(int index)
    {
        if(!VideoCanEdit||_timeline is not {} timeline)return;
        _selectedVideoTrack=null;
        SeekVideoProject(timeline.Offset(index));
    }

    private void MoveVideoClip(int from,int insertion)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        PauseVideoForEditing();
        _selectedVideoTrack=null;
        if(timeline.Move(from,insertion))CommitVideoEdit();
    }

    private async void VideoAddClip_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_addingVideoClips||_timeline is not {} timeline)return;
        var dialog=new OpenFileDialog{Filter="Video MP4|*.mp4",Title="Thêm clip vào timeline",Multiselect=true};
        if(dialog.ShowDialog(this)!=true)return;
        PauseVideoForEditing();
        _addingVideoClips=true;
        VideoAddClipButton.IsEnabled=false;
        var failures=new List<string>();
        try{
            foreach(var path in dialog.FileNames){
                try{
                    var input=await VideoTrimExport.ReadInputAsync(path);
                    if(!ReferenceEquals(_timeline,timeline)||!VideoCanEdit)return;
                    if(!timeline.Append(input)){SetStatus("Timeline đã đạt giới hạn số clip hoặc thời lượng.");break;}
                    CommitVideoEdit();
                }catch(Exception error){
                    App.Log(error);
                    failures.Add(System.IO.Path.GetFileName(path));
                }
            }
            if(failures.Count>0)SetStatus("Không thêm được MP4: "+string.Join(", ",failures));
        }finally{_addingVideoClips=false;RefreshVideoControls();}
    }

    private bool HandleVideoKeys(KeyEventArgs e)
    {
        if(!VideoCanEdit||MediaList.IsKeyboardFocusWithin||Keyboard.FocusedElement is TextBoxBase or ComboBox or ComboBoxItem)return false;
        var modifiers=Keyboard.Modifiers;
        if(modifiers==ModifierKeys.None&&e.Key==Key.F11&&(_videoReady||_videoDecoderSuspended))VideoFullscreen_Click(this,e);
        else if(modifiers==ModifierKeys.None&&e.Key==Key.Space){ToggleVideoPlayback_Click(this,e);}
        else if(!_videoStudio)return false;
        else if(modifiers==ModifierKeys.Control&&e.Key==Key.Z)ExecuteVideoEdit("undo");
        else if(modifiers==ModifierKeys.Control&&e.Key==Key.Y||modifiers==(ModifierKeys.Control|ModifierKeys.Shift)&&e.Key==Key.Z)ExecuteVideoEdit("redo");
        else if(modifiers==ModifierKeys.None&&e.Key==Key.Delete)ExecuteVideoEdit("delete");
        else if(modifiers==ModifierKeys.None&&e.Key==Key.Escape&&(_videoTool!=null||_selectedVideoTrack!=null)){SetVideoTool(null);ClearVideoTrackSelection();}
        else if(modifiers==ModifierKeys.None&&e.Key==Key.Escape)ExitVideoStudio();
        else return false;
        e.Handled=true;
        return true;
    }

    private void VideoStrip_Menu(object sender,ContextMenuEventArgs e)
    {
        if(!VideoCanEdit){e.Handled=true;return;}
        PauseVideoForEditing();
        if(VideoStrip.ContextMenu is {} menu)AppMenus.Decorate(menu);
    }

    private void VideoStrip_Scrolled(object sender,ScrollChangedEventArgs e)
    {
        if(VideoStrip==null)return;
        if(e.ViewportWidthChange!=0)UpdateVideoZoom();
        VideoStrip.RefreshScroll();
    }

    private void VideoStrip_SizeChanged(object sender,SizeChangedEventArgs e)=>UpdateVideoZoom();

    private void VideoZoom_Click(object sender,RoutedEventArgs e)
    {
        var oldWidth=VideoStrip.ActualWidth;
        double anchor=_timeline is {} timeline?timeline.Project.Position.TotalSeconds/timeline.Project.Duration.TotalSeconds:0;
        var withinViewport=anchor*oldWidth-VideoStripScroll.HorizontalOffset;
        _videoZoom=((sender as Button)?.Tag as string) switch{
            "in"=>Math.Min(16,_videoZoom*2),"out"=>Math.Max(1,_videoZoom/2),_=>1
        };
        UpdateVideoZoom();
        VideoStripScroll.UpdateLayout();
        VideoStripScroll.ScrollToHorizontalOffset(anchor*VideoStrip.Width-Math.Clamp(withinViewport,0,VideoStripScroll.ViewportWidth));
    }

    private void UpdateVideoZoom()
    {
        if(VideoStripScroll is null||VideoStrip is null||VideoZoomReset is null)return;
        var width=VideoStripScroll.ViewportWidth>0?VideoStripScroll.ViewportWidth:VideoStripScroll.ActualWidth;
        VideoStrip.Width=Math.Max(1,width)*_videoZoom;
        VideoZoomReset.Content=$"{_videoZoom:P0}";
    }

    private void VideoFullscreen_Click(object sender,RoutedEventArgs e)
    {
        if(_videoFullscreen is {} active){active.Close();return;}
        if(!VideoCanEdit||(!_videoReady&&!_videoDecoderSuspended))return;
        SetVideoTool(null);
        ClearVideoTrackSelection();
        var window=new Window{
            Owner=this,WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,ShowInTaskbar=false,
            WindowStartupLocation=WindowStartupLocation.CenterOwner,Width=ActualWidth,Height=ActualHeight,
            Background=Brushes.Black,Title="Captovix · Xem trước video"
        };
        var container=new Grid();
        var close=new Button{Content=AppIcons.Content("FullscreenExit","",20),ToolTip="Thoát toàn màn hình · Esc",HorizontalAlignment=HorizontalAlignment.Right,
            VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(12)};
        System.Windows.Automation.AutomationProperties.SetName(close,"Thoát toàn màn hình");
        close.Click+=(_,_)=>window.Close();
        VideoHost.Children.Remove(VideoViewport);
        container.Children.Add(VideoViewport);
        container.Children.Add(close);
        var controls=CreateFullscreenControls(window);container.Children.Add(controls);
        var idle=new System.Windows.Threading.DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};
        void Reveal(){controls.Visibility=close.Visibility=Visibility.Visible;window.Cursor=VideoViewport.Cursor=Cursors.Arrow;idle.Stop();idle.Start();}
        idle.Tick+=(_,_)=>{if(Mouse.Captured!=null)return;controls.Visibility=close.Visibility=Visibility.Collapsed;window.Cursor=VideoViewport.Cursor=Cursors.None;idle.Stop();};
        Point? lastPointer=null;
        window.PreviewMouseMove+=(_,move)=>{var at=move.GetPosition(window);if(lastPointer is {} last&&(at-last).Length<1)return;lastPointer=at;Reveal();};
        window.PreviewMouseDown+=(_,click)=>{
            var point=click.GetPosition(controls);var exitPoint=click.GetPosition(close);
            bool overHud=controls.Visibility==Visibility.Visible&&point.X>=0&&point.Y>=0&&point.X<=controls.ActualWidth&&point.Y<=controls.ActualHeight;
            bool overExit=close.Visibility==Visibility.Visible&&exitPoint.X>=0&&exitPoint.Y>=0&&exitPoint.X<=close.ActualWidth&&exitPoint.Y<=close.ActualHeight;
            if(overHud||overExit){Reveal();return;}
            lastPointer=click.GetPosition(window);idle.Stop();controls.Visibility=close.Visibility=Visibility.Collapsed;window.Cursor=VideoViewport.Cursor=Cursors.None;click.Handled=true;
        };window.PreviewKeyDown+=(_,_)=>Reveal();window.Deactivated+=(_,_)=>{idle.Stop();controls.Visibility=close.Visibility=Visibility.Visible;};
        Reveal();
        window.Content=container;
        window.PreviewKeyDown+=(_,key)=>{
            if(key.Key is Key.Escape or Key.F11){window.Close();key.Handled=true;}
            else if(key.Key is Key.Left or Key.Right){SkipVideo(key.Key==Key.Left?-10:10);key.Handled=true;}
            else if(key.Key==Key.Space){ToggleVideoPlayback_Click(this,key);key.Handled=true;}
        };
        window.Closed+=(_,_)=>{
            container.Children.Remove(VideoViewport);
            VideoHost.Children.Add(VideoViewport);
            idle.Stop();VideoViewport.Cursor=Cursors.Arrow;_fullscreenSeek=null;_fullscreenTime=null;_fullscreenPlay=null;
            _videoFullscreen=null;
            UpdateVideoMarks();
        };
        _videoFullscreen=window;
        try{window.Show();window.WindowState=WindowState.Maximized;}
        catch{window.Close();throw;}
    }
}
