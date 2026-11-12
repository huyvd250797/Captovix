using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Captovix;

public partial class MainWindow
{
    private readonly MediaPlayer _videoAudioPlayer=new();
    private CancellationTokenSource? _videoAudioCancellation;
    private VideoProject? _videoAudioSnapshot;
    private string? _videoAudioDirectory;
    private bool _videoAudioReady,_videoAudioPlaying,_videoAudioPendingPlay,_videoAudioSyncFields;
    private VideoAudioLane _audioLane;
    private VideoAudioTrack? _audioShownTrack;

    private void InitializeVideoAudio()
    {
        _videoAudioPlayer.MediaOpened+=(_,_)=>{
            _videoAudioReady=true;
            VideoAudioStatus.Text="Waveform sẵn sàng";
            if(_videoAudioPendingPlay){_videoAudioPendingPlay=false;ToggleVideoPlayback_Click(this,new());}
        };
        _videoAudioPlayer.MediaFailed+=(_,e)=>{
            ClearVideoAudio();
            VideoAudioStatus.Text="Không mở được bản phối";
            SetStatus(e.ErrorException.Message);
        };
        VideoAudioVolume.ValueChanged+=(_,_)=>{
            if(!_videoAudioSyncFields&&_timeline!=null&&_audioShownTrack!=null)
                VideoAudioStatus.Text="Bấm Áp dụng âm thanh để nghe thay đổi";
        };
    }

    private void ClearVideoAudio()
    {
        _videoAudioCancellation?.Cancel();
        _videoAudioCancellation=null;
        _videoAudioSnapshot=null;
        _videoAudioPendingPlay=_videoAudioReady=_videoAudioPlaying=false;
        _videoAudioPlayer.Close();
        VideoPreview.IsMuted=false;
        VideoStrip.SetAudio(null,_audioLane);
        var directory=_videoAudioDirectory;
        _videoAudioDirectory=null;
        if(directory!=null)DeleteVideoAudioDirectory(directory);
    }

    private static void DeleteVideoAudioDirectory(string directory)
    {
        try{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        catch(IOException){}
        catch(UnauthorizedAccessException){}
    }

    private async void RequestVideoAudio()
    {
        if(_timeline?.Project is not {} project||(!_videoStudio&&!VideoAudioPlan.NeedsMix(project)))return;
        if(_videoAudioSnapshot is {} snapshot&&ReferenceEquals(snapshot.Clips,project.Clips)
            &&ReferenceEquals(snapshot.AudioTracks,project.AudioTracks)&&ReferenceEquals(snapshot.AdditionalSources,project.AdditionalSources)
            &&snapshot.SourcePath==project.SourcePath)return;
        ClearVideoAudio();
        _videoAudioSnapshot=project;
        VideoPreview.IsMuted=VideoAudioPlan.NeedsMix(project);
        if(VideoTrimExport.FindFfmpeg()==null){
            VideoAudioStatus.Text="Cần FFmpeg để tạo waveform và phối âm";
            return;
        }
        var cancellation=new CancellationTokenSource();
        _videoAudioCancellation=cancellation;
        var directory=Path.Combine(Path.GetTempPath(),"CaptovixMix-"+Guid.NewGuid().ToString("N"));
        VideoAudioStatus.Text="Đang tạo waveform và bản phối…";
        bool keep=false;
        try{
            await Task.Delay(180,cancellation.Token);
            var mix=await Task.Run(()=>VideoAudioRender.RenderAsync(project,directory,true,cancellation.Token),cancellation.Token);
            if(!ReferenceEquals(_videoAudioCancellation,cancellation)||cancellation.IsCancellationRequested)return;
            _videoAudioDirectory=directory;
            keep=true;
            VideoStrip.SetAudio(mix.Peaks,_audioLane);
            _videoAudioPlayer.Open(new Uri(mix.Path));
        }catch(OperationCanceledException){}
        catch(Exception error){
            if(!ReferenceEquals(_videoAudioCancellation,cancellation))return;
            _videoAudioPendingPlay=false;
            VideoAudioStatus.Text="Chưa tạo được bản phối";
            if(_videoStudio||VideoAudioPlan.NeedsMix(project))SetStatus(error.Message);
        }finally{
            if(ReferenceEquals(_videoAudioCancellation,cancellation))_videoAudioCancellation=null;
            cancellation.Dispose();
            if(!keep)DeleteVideoAudioDirectory(directory);
        }
    }

    private bool PrepareVideoPlayback()
    {
        if(_timeline?.Project is not {} project||!VideoAudioPlan.NeedsMix(project))return true;
        if(_videoAudioReady)return true;
        if(VideoTrimExport.FindFfmpeg()==null){
            SetStatus("Cần FFmpeg để xem bản phối âm thanh đã chỉnh.");
            return false;
        }
        if(_videoAudioCancellation==null&&_videoAudioDirectory==null)_videoAudioSnapshot=null;
        RequestVideoAudio();
        _videoAudioPendingPlay=true;
        SetStatus("Đang chuẩn bị bản phối âm thanh…");
        return false;
    }

    private void SyncVideoAudio(bool force=false)
    {
        if(_timeline?.Project is not {} project)return;
        bool mix=VideoAudioPlan.NeedsMix(project);
        VideoPreview.IsMuted=mix;
        bool play=mix&&_playing&&_videoAudioReady;
        if(play){
            if(force||!_videoAudioPlaying||(_videoAudioPlayer.Position-project.Position).Duration()>TimeSpan.FromMilliseconds(150))
                _videoAudioPlayer.Position=project.Position;
            if(!_videoAudioPlaying)_videoAudioPlayer.Play();
        }else if(_videoAudioPlaying)_videoAudioPlayer.Pause();
        _videoAudioPlaying=play;
    }

    private void SelectVideoAudio(VideoAudioLane lane)
    {
        PauseVideoForEditing();
        _audioLane=lane;
        ClearVideoTrackSelection();
        VideoAudioInspector.IsExpanded=true;
        VideoStrip.SelectAudio(lane);
        RefreshVideoAudioInspector();
    }

    private void VideoAudioLane_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(!_videoAudioSyncFields&&VideoAudioLanePicker.SelectedIndex>=0&&_videoStudio)
            SelectVideoAudio((VideoAudioLane)VideoAudioLanePicker.SelectedIndex);
    }

    private void RefreshVideoAudioInspector()
    {
        if(!_videoStudio||_timeline?.Project is not {} project)return;
        var track=VideoAudioPlan.Settings(project,_audioLane);
        if(_audioShownTrack==track&&VideoAudioLanePicker.SelectedIndex==(int)_audioLane)return;
        _audioShownTrack=track;
        _videoAudioSyncFields=true;
        try{
            VideoAudioLanePicker.SelectedIndex=(int)_audioLane;
            VideoAudioVolume.Value=track.Volume;
            VideoAudioMute.IsChecked=track.Muted;
            VideoAudioSolo.IsChecked=track.Solo;
            VideoFadeIn.Text=Number(track.FadeIn.TotalSeconds);
            VideoFadeOut.Text=Number(track.FadeOut.TotalSeconds);
            VideoMusicOptions.Visibility=_audioLane==VideoAudioLane.Music?Visibility.Visible:Visibility.Collapsed;
            VideoMusicName.Text=track.File==null?"Chưa thêm nhạc nền":Path.GetFileName(track.File);
            VideoMusicOffset.Text=Number(track.Offset.TotalSeconds);
            VideoMusicIn.Text=Number(track.InPoint.TotalSeconds);
            VideoMusicOut.Text=Number(track.OutPoint.TotalSeconds);
            VideoAudioCuts.ItemsSource=track.Cuts.Select(cut=>$"{VideoTimecode.Format(cut.Start)} – {VideoTimecode.Format(cut.End)}").ToArray();
        }finally{_videoAudioSyncFields=false;}
    }

    private void VideoAudioToggle_Click(object sender,RoutedEventArgs e)
    {
        if(!VideoCanEdit||_timeline?.Project is not {} project)return;
        var track=VideoAudioPlan.Settings(project,_audioLane);
        ApplyAudioTrack(track with{Muted=VideoAudioMute.IsChecked==true,Solo=VideoAudioSolo.IsChecked==true});
    }

    private void VideoApplyAudio_Click(object sender,RoutedEventArgs e)
    {
        if(!VideoCanEdit||_timeline?.Project is not {} project)return;
        if(!AudioTime(VideoFadeIn,out var fadeIn)||!AudioTime(VideoFadeOut,out var fadeOut)){
            SetStatus("Nhập thời lượng fade hợp lệ theo giây.");return;
        }
        var track=VideoAudioPlan.Settings(project,_audioLane);
        track=track with{Volume=VideoAudioVolume.Value,FadeIn=fadeIn,FadeOut=fadeOut};
        if(track.File!=null){
            if(!AudioTime(VideoMusicOffset,out var offset)||!AudioTime(VideoMusicIn,out var from)
                ||!AudioTime(VideoMusicOut,out var to)||from>=to||offset>=project.Duration){
                SetStatus("Kiểm tra mốc cắt nhạc và vị trí trên timeline.");return;
            }
            track=track with{Offset=offset,InPoint=from,OutPoint=to};
        }
        ApplyAudioTrack(track);
    }

    private static bool AudioTime(TextBox input,out TimeSpan value)
    {
        value=TimeSpan.Zero;
        if(!TryNumber(input,out var seconds)||seconds<0||seconds>TimeSpan.FromDays(30).TotalSeconds)return false;
        value=TimeSpan.FromSeconds(seconds);
        return true;
    }

    private void VideoCutAudio_Click(object sender,RoutedEventArgs e)
    {
        if(!VideoCanEdit||_timeline?.Project is not {} project)return;
        if(!AudioTime(VideoAudioCutStart,out var start)||!AudioTime(VideoAudioCutEnd,out var end)||end<=start||end>project.Duration){
            SetStatus("Nhập khoảng cắt nằm trong thời lượng timeline.");return;
        }
        var track=VideoAudioPlan.Settings(project,_audioLane);
        ApplyAudioTrack(track with{Cuts=[..track.Cuts,new(start,end)]});
    }

    private void VideoRestoreAudio_Click(object sender,RoutedEventArgs e)
    {
        if(!VideoCanEdit||_timeline?.Project is not {} project||VideoAudioCuts.SelectedIndex<0)return;
        var track=VideoAudioPlan.Settings(project,_audioLane);
        var index=VideoAudioCuts.SelectedIndex;
        ApplyAudioTrack(track with{Cuts=track.Cuts.Where((_,i)=>i!=index).ToArray()});
    }

    private void ApplyAudioTrack(VideoAudioTrack track)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        if(!track.IsValid()){SetStatus("Thông số track âm thanh không hợp lệ.");return;}
        PauseVideoForEditing();
        if(timeline.SetAudio(track))CommitVideoEdit();
    }

    private async void VideoAddMusic_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        var dialog=new OpenFileDialog{Title="Thêm hoặc thay nhạc nền",Filter="Âm thanh|*.mp3;*.wav;*.m4a;*.aac;*.flac;*.ogg;*.opus"};
        if(dialog.ShowDialog(this)!=true)return;
        PauseVideoForEditing();
        try{
            var duration=await VideoAudioRender.MusicDurationAsync(dialog.FileName,CancellationToken.None);
            if(!ReferenceEquals(timeline,_timeline)||!VideoCanEdit||!_videoStudio)return;
            SelectVideoAudio(VideoAudioLane.Music);
            var start=timeline.Project.Position;
            if(start>=timeline.Project.Duration)start=TimeSpan.Zero;
            ApplyAudioTrack(new(VideoAudioLane.Music){File=dialog.FileName,Offset=start,OutPoint=duration,MusicDuration=duration});
        }catch(Exception error){App.Log(error);SetStatus(error.Message);}
    }

    private void VideoRemoveMusic_Click(object sender,RoutedEventArgs e)=>ApplyAudioTrack(new(VideoAudioLane.Music));
}
