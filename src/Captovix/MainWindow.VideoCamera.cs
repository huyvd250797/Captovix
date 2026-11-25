using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Captovix;

public partial class MainWindow
{
    private Uri? _cameraUri;
    private bool _cameraOpened,_cameraPlaying;
    private VideoCamera? _cameraShown;
    private int _cameraShownSource=-1;

    private void InitializeCameraPreview()
    {
        CameraPreview.MediaOpened+=(_,_)=>{_cameraOpened=true;UpdateCameraPreview(_videoBounds);};
        CameraPreview.MediaFailed+=(_,_)=>{
            _cameraOpened=false;
            CameraPreview.Visibility=Visibility.Collapsed;
            if(_videoStudio)SetStatus("Không mở được nguồn camera. Kiểm tra đường dẫn hoặc thay tệp.");
        };
    }

    private void ReleaseCameraPreview()
    {
        if(_cameraUri==null)return;
        _cameraUri=null;
        _cameraOpened=_cameraPlaying=false;
        CameraPreview.Close();
        CameraPreview.Source=null;
        CameraPreview.Visibility=Visibility.Collapsed;
    }

    private void UpdateCameraPreview(Rect bounds)
    {
        if(_videoDecoderSuspended){
            if(!bounds.IsEmpty&&CameraPausedFrame.Source!=null&&_timeline is {} paused
                &&paused.Project.Cameras.GetValueOrDefault(paused.Selected.SourceIndex) is {} visible)
                LayoutCamera(CameraPausedFrame,bounds,visible);
            ReleaseCameraPreview();
            return;
        }
        if(bounds.IsEmpty||_timeline is not {} timeline||
            timeline.Project.Cameras.GetValueOrDefault(timeline.Selected.SourceIndex) is not {Visible:true} camera){
            ReleaseCameraPreview();return;
        }
        var clip=timeline.Selected;
        var local=timeline.Project.Position-timeline.Offset(timeline.Project.SelectedClip);
        var time=camera.At(local>=clip.MovingDuration?LastVideoFrame(clip):clip.SourceAt(local));
        if(time==null){ReleaseCameraPreview();return;}
        if(_cameraUri==null||!string.Equals(_cameraUri.LocalPath,camera.Path,StringComparison.OrdinalIgnoreCase)){
            ReleaseCameraPreview();
            if(!File.Exists(camera.Path))return;
            _cameraUri=new Uri(camera.Path);
            CameraPreview.Source=_cameraUri;
            CameraPreview.Play();
            CameraPreview.Pause();
        }
        LayoutCamera(CameraPreview,bounds,camera);
        if(!_cameraOpened)return;
        bool play=_playing&&_holdClock==null;
        if(!play||!_cameraPlaying||(CameraPreview.Position-time.Value).Duration()>TimeSpan.FromMilliseconds(120))
            if((CameraPreview.Position-time.Value).Duration()>TimeSpan.FromMilliseconds(14))CameraPreview.Position=time.Value;
        if(play&&!_cameraPlaying){CameraPreview.SpeedRatio=clip.Timing.Speed;CameraPreview.Play();}
        if(!play&&_cameraPlaying)CameraPreview.Pause();
        if(play&&CameraPreview.SpeedRatio!=clip.Timing.Speed)CameraPreview.SpeedRatio=clip.Timing.Speed;
        _cameraPlaying=play;
        CameraPreview.Visibility=Visibility.Visible;
    }

    private static void LayoutCamera(FrameworkElement preview,Rect bounds,VideoCamera camera)
    {
        Canvas.SetLeft(preview,bounds.X+bounds.Width*camera.X);
        Canvas.SetTop(preview,bounds.Y+bounds.Height*camera.Y);
        preview.Width=bounds.Width*camera.Width;
        preview.Height=bounds.Height*camera.Height;
    }

    private void SyncCameraInspector(VideoTimeline timeline)
    {
        int source=timeline.Selected.SourceIndex;
        var camera=timeline.Project.Cameras.GetValueOrDefault(source);
        if(source==_cameraShownSource&&camera==_cameraShown)return;
        _cameraShownSource=source;
        _cameraShown=camera;
        CameraName.Text=camera==null?"Chưa có nguồn camera":Path.GetFileName(camera.Path);
        CameraVisible.IsChecked=camera?.Visible??true;
        CameraX.Text=Number(100*(camera?.X??.72));
        CameraY.Text=Number(100*(camera?.Y??.72));
        CameraWidth.Text=Number(100*(camera?.Width??.25));
        CameraHeight.Text=Number(100*(camera?.Height??.25));
        var first=camera?.Sync.FirstOrDefault();
        CameraOffset.Text=Number(first==null?0:(first.ScreenStart-first.CameraStart).TotalSeconds);
        CameraOffset.IsEnabled=camera?.Sync.Length is null or <=1;
    }

    private async void ImportCamera_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        var picker=new OpenFileDialog{Filter="Video MP4|*.mp4",Title="Chọn video trình bày"};
        if(picker.ShowDialog(this)!=true)return;
        int source=timeline.Selected.SourceIndex;
        try{
            var input=await VideoTrimExport.ReadInputAsync(picker.FileName);
            if(!_videoStudio||!VideoCanEdit||!ReferenceEquals(_timeline,timeline)||timeline.Selected.SourceIndex!=source)return;
            PauseVideoForEditing();
            if(timeline.SetCamera(source,VideoCamera.Imported(input.Path,input.Duration,TimeSpan.Zero)))CommitVideoEdit();
        }catch(Exception error){App.Log(error);SetStatus("Chưa thêm được camera: "+error.Message);}
    }

    private void ApplyCamera_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        int source=timeline.Selected.SourceIndex;
        if(timeline.Project.Cameras.GetValueOrDefault(source) is not {} camera)return;
        if(!TryNumber(CameraX,out double x)||!TryNumber(CameraY,out double y)
            ||!TryNumber(CameraWidth,out double w)||!TryNumber(CameraHeight,out double h)
            ||!TryNumber(CameraOffset,out double offset)||Math.Abs(offset)>86400){
            SetStatus("Nhập vị trí/kích thước theo %, và mốc lệch theo giây.");return;
        }
        var first=camera.Sync.FirstOrDefault();
        double previous=first==null?0:(first.ScreenStart-first.CameraStart).TotalSeconds;
        if(CameraOffset.IsEnabled&&Math.Abs(previous-offset)>.000001)
            camera=VideoCamera.Imported(camera.Path,camera.Duration,TimeSpan.FromSeconds(offset));
        camera=camera with{Visible=CameraVisible.IsChecked==true,X=x/100,Y=y/100,Width=w/100,Height=h/100};
        if(!camera.IsValid()){SetStatus("Khung camera phải nằm trong canvas, mỗi chiều từ 5% đến 100%.");return;}
        PauseVideoForEditing();
        if(timeline.SetCamera(source,camera))CommitVideoEdit();
    }

    private void CameraCorner_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button{Tag:string corner}||!TryNumber(CameraWidth,out double w)||!TryNumber(CameraHeight,out double h))return;
        CameraX.Text=Number(corner.EndsWith("R",StringComparison.Ordinal)?Math.Max(0,97-w):3);
        CameraY.Text=Number(corner.StartsWith("B",StringComparison.Ordinal)?Math.Max(0,97-h):3);
        ApplyCamera_Click(sender,e);
    }

    private void RemoveCamera_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        PauseVideoForEditing();
        if(timeline.SetCamera(timeline.Selected.SourceIndex,null))CommitVideoEdit();
    }
}
