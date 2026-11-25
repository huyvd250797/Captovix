using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Captovix;

public partial class MainWindow
{
    private const string VideoProjectFilter="Dự án video Captovix|*.cvxvideo";
    private MediaItem? _videoItem;
    private VideoTimeline? _timeline;
    private string? _videoSource;
    private int _previewSourceIndex;
    private bool _resumeAfterVideoOpen;
    private readonly HashSet<string> _verifiedVideoSources=new(StringComparer.OrdinalIgnoreCase);
    private string? _videoProjectPath;
    private CancellationTokenSource? _videoExportCancellation;
    private bool _videoReady,_videoProjectDirty,_syncVideoSeek,_videoProjectReadError;
    private DateTime _videoSaveAfter;
    private long? _holdClock;
    private bool _videoDecoderSuspended;
    private CancellationTokenSource? _videoPosterWork;
    private TimeSpan _holdStart;
    private TimeSpan? _queuedVideoSeek;
    private System.Windows.Threading.DispatcherOperation? _videoSeekWork;

    private static string VideoResumePath=>Path.Combine(App.DataRoot,"video-resume.json");

    private void RememberVideoSession(MediaItem? item)
    {
        try{
            if(item==null)File.Delete(VideoResumePath);
            else MediaStore.AtomicWrite(VideoResumePath,System.Text.Json.JsonSerializer.Serialize(item.Id));
        }catch(Exception error){App.Log(error);}
    }

    private void RestoreVideoSession()
    {
        if(Selected!=null||!File.Exists(VideoResumePath))return;
        try{
            string? id=System.Text.Json.JsonSerializer.Deserialize<string>(File.ReadAllText(VideoResumePath));
            if(_store.Items.FirstOrDefault(item=>item.IsVideo&&item.Id==id) is {} video)OpenHistoryItem(video);
        }catch(Exception error){App.Log(error);}
    }

    private static string VideoAutosavePath(string mediaId)
    {
        if(!Guid.TryParseExact(mediaId,"N",out _))throw new InvalidDataException("Mã video không hợp lệ.");
        return Path.Combine(App.DataRoot,"VideoProjects",mediaId+".cvxvideo");
    }

    private void OpenLibraryVideo(MediaItem item)
    {
        VideoProject? project=null;
        try{
            project=VideoProject.Recover(VideoAutosavePath(item.Id),out bool restored);
            if(restored)SetStatus("Đã phục hồi bản tự lưu trước đó.");
        }catch(Exception error){
            App.Log(error);
            _videoProjectReadError=true;
            SetStatus("Không đọc được bản tự lưu: "+error.Message);
        }
        OpenVideoProject(item,project);
    }

    private void OpenVideoProject(MediaItem item,VideoProject? project)
    {
        _videoItem=item;
        RememberVideoSession(item);
        _timeline=project==null?null:new VideoTimeline(project);
        _videoReady=false;
        _videoSource=project?.SourcePath??item.FilePath;
        if(project!=null&&!File.Exists(_videoSource)&&File.Exists(item.FilePath)
            &&SourceMatches(item.FilePath,project.Source)){
            _videoSource=item.FilePath;
            _timeline=new VideoTimeline(project with{SourcePath=_videoSource});
            MarkVideoProjectDirty();
        }
        _verifiedVideoSources.Clear();
        _videoSourceSize=null;
        _selectedVideoTrack=null;
        SetVideoTool(null);
        VideoHost.Visibility=Visibility.Visible;
        UpdateVideoWorkspace();
        ReadPrimaryVideoSize(_timeline);
        OpenVideoMedia();
        RequestVideoAudio();
    }

    private void OpenVideoMedia(bool resume=false)
    {
        CancelVideoPoster();
        _videoDecoderSuspended=false;
        _previewSourceIndex=_timeline?.Selected.SourceIndex??0;
        var input=_timeline?.Project.Input(_previewSourceIndex);
        _videoSource=input?.Path??_videoItem?.FilePath;
        _resumeAfterVideoOpen=resume;
        _videoReady=false;
        ReleaseCameraPreview();
        VideoPreview.Close();
        VideoPreview.Source=null;
        SetVideoPlaying(false);
        bool available=File.Exists(_videoSource);
        if(available&&input?.Identity is {} identity&&!_verifiedVideoSources.Contains(_videoSource!)&&!SourceMatches(_videoSource!,identity)){
            available=false;
            SetStatus("Tệp ở đường dẫn cũ đã thay đổi. Hãy tìm lại đúng video nguồn.");
        }
        if(available){
            _verifiedVideoSources.Add(_videoSource!);
            VideoPreview.Source=new Uri(Path.GetFullPath(_videoSource!));
            VideoPreview.Play();VideoPreview.Pause();
        }else{_resumeAfterVideoOpen=false;SetStatus("Thiếu hoặc sai video nguồn: "+Path.GetFileName(_videoSource)+". Bấm Liên kết nguồn để tìm lại MP4.");}
        RefreshVideoControls();
    }

    private static bool SourceMatches(string path,VideoSourceIdentity? identity)
    {
        if(identity==null)return File.Exists(path);
        try{return VideoSourceIdentity.Read(path)==identity;}
        catch(Exception error) when(error is IOException or UnauthorizedAccessException){return false;}
    }

    private void DetachVideoProject()
    {
        _videoSeekWork?.Abort();
        _videoSeekWork=null;
        _queuedVideoSeek=null;
        ExitVideoStudio();
        CancelVideoPoster();
        _videoDecoderSuspended=false;
        _cueClips=null;_cueSource=null;_cueSpans=[];
        ClearVideoAudio();
        _videoTimer.Stop();
        _videoSourceSize=null;
        _selectedVideoTrack=null;
        SetVideoTool(null);
        VideoMarkTools.Visibility=VideoInspector.Visibility=Visibility.Collapsed;
        _videoFullscreen?.Close();
        _resumeAfterVideoOpen=false;
        ReleaseCameraPreview();
        VideoPreview.Close();
        VideoPreview.Source=null;
        SetVideoPlaying(false);
        if(_videoItem!=null)RememberVideoSession(null);
        _videoItem=null;_timeline=null;_videoSource=null;_videoProjectPath=null;
        _videoReady=false;
        UpdateVideoWorkspace();
        _videoProjectDirty=false;_videoProjectReadError=false;
        _videoCanvasKey=null;_previousVideoTracks=null;ClearVideoVisuals();
        RefreshVideoControls();
    }

    private void CancelVideoPoster()
    {
        _videoPosterWork?.Cancel();
        _videoPosterWork=null;
        VideoPausedFrame.Visibility=Visibility.Collapsed;
        VideoPausedFrame.Source=null;
        CameraPausedFrame.Visibility=Visibility.Collapsed;
        CameraPausedFrame.Source=null;
        VideoPreview.Visibility=Visibility.Visible;
    }

    private async void SuspendVideoDecoder()
    {
        if(_videoPosterWork!=null||_videoStudio||!_videoReady||_playing||_timeline is not {} timeline||_videoSource is not {} path)return;
        var project=timeline.Project;
        var (_,at)=timeline.Map(project.Position);
        if(at>=timeline.Selected.End)at=LastVideoFrame(timeline.Selected);
        using var token=new CancellationTokenSource(TimeSpan.FromSeconds(8));
        _videoPosterWork?.Cancel();
        _videoPosterWork=token;
        try{
            var bytes=await ReadVideoFrameAsync(path,at,3840,2160,token.Token);
            var camera=project.Cameras.GetValueOrDefault(timeline.Selected.SourceIndex);
            byte[]? cameraBytes=null;
            if(camera is {Visible:true}&&camera.At(at) is {} cameraAt&&File.Exists(camera.Path)){
                try{cameraBytes=await ReadVideoFrameAsync(camera.Path,cameraAt,640,480,token.Token);}
                catch(Exception error) when(error is not OperationCanceledException){App.Log(error);}
            }
            if(_videoStudio||_playing||_timeline?.Project!=project||token.IsCancellationRequested)return;
            var poster=DecodePoster(bytes);
            if(cameraBytes!=null){
                CameraPausedFrame.Source=DecodePoster(cameraBytes);
                CameraPausedFrame.Visibility=Visibility.Visible;
            }
            VideoPausedFrame.Width=VideoPreview.Width;
            VideoPausedFrame.Height=VideoPreview.Height;
            VideoPausedFrame.Source=poster;
            VideoPausedFrame.Visibility=Visibility.Visible;
            VideoPreview.Visibility=Visibility.Collapsed;
            _videoDecoderSuspended=true;
            _videoReady=false;
            VideoPreview.Close();
            VideoPreview.Source=null;
            if(!_videoProjectDirty)_videoTimer.Stop();
            RefreshVideoControls();
        }catch(OperationCanceledException){}
        catch(Exception error){App.Log(error);}
        finally{if(ReferenceEquals(_videoPosterWork,token))_videoPosterWork=null;}
    }

    private static System.Windows.Media.Imaging.BitmapFrame DecodePoster(byte[] bytes)
    {
        using var stream=new MemoryStream(bytes);
        var image=System.Windows.Media.Imaging.BitmapFrame.Create(stream,
            System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat,System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
        image.Freeze();
        return image;
    }

    private void Video_Opened(object sender,RoutedEventArgs e)
    {
        if(_videoItem is not {} item||_videoSource==null||!VideoPreview.NaturalDuration.HasTimeSpan)return;
        var duration=VideoPreview.NaturalDuration.TimeSpan;
        if(duration<=TimeSpan.Zero||VideoPreview.NaturalVideoWidth<=0){
            SetStatus("Video nguồn không có khung hình phát được.");
            return;
        }
        if(_timeline is {} timeline&&timeline.Project.Clips.Any(clip=>clip.SourceIndex==_previewSourceIndex&&clip.End>duration+TimeSpan.FromMilliseconds(120))){
            SetStatus("Thời lượng video nguồn không khớp mốc cắt. Hãy liên kết lại đúng tệp.");
            RefreshVideoControls();
            return;
        }
        _videoReady=true;
        if(_timeline==null){
            var project=new VideoProject(VideoProject.CurrentVersion,_videoSource,null,
                duration,[new VideoRange(TimeSpan.Zero,duration){Number=1}],TimeSpan.Zero,0){NextClipNumber=2};
            try{
                project=WithSourceIdentity(project);
                string metadata=RecordingCamera.MetadataFor(_videoSource),path=RecordingCamera.PathFor(_videoSource);
                if(File.Exists(path)&&File.Exists(metadata)&&new FileInfo(metadata).Length<2_000_000
                    &&System.Text.Json.JsonSerializer.Deserialize<VideoCamera>(File.ReadAllText(metadata)) is {} camera){
                    camera=camera with{Path=path};
                    if(camera.IsValid())project.Cameras[0]=camera;
                }
            }catch(Exception error){App.Log(error);}
            _timeline=new VideoTimeline(project);
            MarkVideoProjectDirty();
        }else if(_previewSourceIndex==0&&_timeline.Project.Source==null){
            try{_timeline=new VideoTimeline(WithSourceIdentity(_timeline.Project));}
            catch(Exception error){App.Log(error);}
        }
        if(_previewSourceIndex==0){
            item.Width=VideoPreview.NaturalVideoWidth;
            item.Height=VideoPreview.NaturalVideoHeight;
            item.Seconds=duration.TotalSeconds;
            item.RefreshDetails();
            UpdateMetadata(item);
        }
        if(_timeline!.Selected.SourceIndex==0)
            _videoSourceSize=(VideoPreview.NaturalVideoWidth,VideoPreview.NaturalVideoHeight);
        SeekVideoMedia();
        if(_resumeAfterVideoOpen){_resumeAfterVideoOpen=false;StartVideoPlayback();}
        RefreshVideoControls();
    }

    private void Video_Ended(object sender,RoutedEventArgs e)
    {
        if(_playing&&_holdClock==null&&_timeline is {} timeline)AdvanceVideo(timeline.Selected.End);
    }

    private void Video_Failed(object sender,ExceptionRoutedEventArgs e)
    {
        _videoReady=_resumeAfterVideoOpen=false;
        SetVideoPlaying(false);
        RefreshVideoControls();
        SetStatus("Không phát được video: "+e.ErrorException.Message);
    }

    private void TickVideoProject()
    {
        if(_videoReady&&_playing){
            if(_holdClock is long clock&&_timeline is {} timeline){
                var local=_holdStart+System.Diagnostics.Stopwatch.GetElapsedTime(clock);
                var clip=timeline.Selected;
                if(local>=clip.Duration)FinishVideoClip(timeline);
                else timeline.Seek(timeline.Offset(timeline.Project.SelectedClip)+local);
                MarkVideoProjectDirty(playback:true);
                UpdateVideoPosition();
            }else AdvanceVideo(VideoPreview.Position);
        }
        if(!_playing&&_videoProjectDirty&&DateTime.UtcNow>=_videoSaveAfter)FlushVideoProject(false);
        if(!_playing&&!_videoProjectDirty){_videoTimer.Stop();if(!_videoStudio&&_videoFullscreen==null&&_videoPosterWork==null)SuspendVideoDecoder();}
    }

    private void AdvanceVideo(TimeSpan sourcePosition)
    {
        if(_timeline is not {} timeline)return;
        var clip=timeline.Selected;
        if(sourcePosition<clip.Start)return;
        int index=timeline.Project.SelectedClip;
        if(sourcePosition>=clip.End){
            if(clip.Timing.Hold>TimeSpan.Zero){
                timeline.Seek(timeline.Offset(index)+clip.MovingDuration);
                BeginVideoHold(clip,clip.MovingDuration);
            }else FinishVideoClip(timeline);
        }else timeline.Track(index,sourcePosition);
        MarkVideoProjectDirty(playback:true);
        UpdateVideoPosition();
        RefreshVideoInspector();
    }

    private void FinishVideoClip(VideoTimeline timeline)
    {
        _holdClock=null;
        int index=timeline.Project.SelectedClip;
        var clip=timeline.Selected;
        if(index+1<timeline.Project.Clips.Length){
            var next=timeline.Project.Clips[index+1];
            timeline.Track(index+1,next.Start);
            if(next.SourceIndex!=clip.SourceIndex)OpenVideoMedia(resume:true);
            else{
                SeekVideoMedia();
                VideoPreview.Play();
                SyncVideoAudio(force:true);
            }
        }else{
            timeline.Seek(timeline.Project.Duration);
            VideoPreview.Pause();
            SetVideoPlaying(false);
            VideoPreview.Position=LastVideoFrame(clip);
        }
    }

    private static TimeSpan LastVideoFrame(VideoRange clip)=>clip.End-TimeSpan.FromTicks(Math.Min(clip.SourceDuration.Ticks/2,TimeSpan.TicksPerMillisecond));
    private void BeginVideoHold(VideoRange clip,TimeSpan local)
    {
        VideoPreview.Pause();
        VideoPreview.Position=LastVideoFrame(clip);
        _holdStart=local;
        _holdClock=System.Diagnostics.Stopwatch.GetTimestamp();
    }

    private void StartVideoPlayback()
    {
        if(_timeline is not {} timeline)return;
        var local=timeline.Project.Position-timeline.Offset(timeline.Project.SelectedClip);
        if(local>=timeline.Selected.MovingDuration&&timeline.Selected.Timing.Hold>TimeSpan.Zero)BeginVideoHold(timeline.Selected,local);
        else VideoPreview.Play();
        SetVideoPlaying(true);
    }

    private void SetVideoPlaying(bool playing)
    {
        _playing=playing;
        if(playing)_videoTimer.Start();
        if(!playing)_holdClock=null;
        SyncVideoAudio(force:true);
        var name=playing?"Tạm dừng":"Phát";
        SyncFullscreenControls();
        VideoPlaybackButton.Content=AppIcons.Content(playing?"RecordingPause":"RecordingPlay","");
        VideoPlaybackButton.ToolTip=name;
        System.Windows.Automation.AutomationProperties.SetName(VideoPlaybackButton,name);
    }

    private void ToggleVideoPlayback_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        if(_playing){
            PauseVideoForEditing();
            return;
        }
        if(_videoDecoderSuspended){
            if(timeline.Project.Position>=timeline.Project.Duration)timeline.Seek(TimeSpan.Zero);
            if(PrepareVideoPlayback())OpenVideoMedia(resume:true);
            return;
        }
        if(timeline.Project.Position>=timeline.Project.Duration)SeekVideoProject(TimeSpan.Zero);
        else SeekVideoMedia();
        if(!PrepareVideoPlayback())return;
        if(_videoReady)StartVideoPlayback();
        else if(File.Exists(_videoSource)&&VideoPreview.Source!=null)_resumeAfterVideoOpen=true;
    }

    private bool VideoCanEdit=>_timeline!=null&&_videoExportCancellation==null
        &&ReferenceEquals(Selected,_videoItem)&&MediaList.SelectedItems.Count==1;

    private void SeekVideoProject(TimeSpan position)
    {
        if(!VideoCanEdit||_timeline is not {} timeline)return;
        _videoSeekWork?.Abort();
        _videoSeekWork=null;
        _queuedVideoSeek=null;
        _resumeAfterVideoOpen=false;
        if(_playing){VideoPreview.Pause();SetVideoPlaying(false);}
        timeline.Seek(position);
        SeekVideoMedia();
        UpdateVideoPosition();
        MarkVideoProjectDirty();
        RefreshVideoInspector();
    }

    private void SeekVideoMedia()
    {
        if(_timeline is not {} timeline)return;
        if(_videoDecoderSuspended){OpenVideoMedia();return;}
        var project=timeline.Project;
        var clip=timeline.Selected;
        if(clip.SourceIndex!=_previewSourceIndex||!SamePath(project.Input(clip.SourceIndex).Path,_videoSource??"")){OpenVideoMedia();return;}
        if(!_videoReady)return;
        var local=project.Position-timeline.Offset(project.SelectedClip);
        VideoPreview.SpeedRatio=clip.Timing.Speed;
        VideoPreview.Position=local>=clip.MovingDuration?LastVideoFrame(clip):clip.SourceAt(local);
    }

    private void VideoSeek_Changed(object sender,RoutedPropertyChangedEventArgs<double> e)
    {
        if(_syncVideoSeek||!VideoCanEdit)return;
        _queuedVideoSeek=TimeSpan.FromSeconds(e.NewValue);
        if(_videoSeekWork!=null)return;
        _videoSeekWork=Dispatcher.InvokeAsync(()=>{
            _videoSeekWork=null;
            var pending=_queuedVideoSeek;
            _queuedVideoSeek=null;
            if(pending is {} position)SeekVideoProject(position);
        },System.Windows.Threading.DispatcherPriority.Render);
    }

    private void UpdateVideoPosition()
    {
        SyncFullscreenControls();
        var project=_timeline?.Project;
        _syncVideoSeek=true;
        try{
            VideoViewerSeek.Maximum=VideoSeek.Maximum=project?.Duration.TotalSeconds??0;
            VideoViewerSeek.Value=VideoSeek.Value=project?.Position.TotalSeconds??0;
        }finally{_syncVideoSeek=false;}
        if(_videoStudio)VideoStrip.Show(project,_selectedVideoTrack);
        SyncVideoAudio();
        UpdateVideoMarks();
        VideoViewerTime.Text=VideoTime.Text=project==null?"":$"{VideoTimecode.Format(project.Position)} / {VideoTimecode.Format(project.Duration)} · Đoạn {project.Clips[project.SelectedClip].Number}";
    }

    private void RefreshVideoControls()
    {
        bool editable=VideoCanEdit;
        VideoPlaybackButton.IsEnabled=editable&&(_videoReady||_videoDecoderSuspended);
        VideoFullscreenButton.IsEnabled=editable&&(_videoReady||_videoDecoderSuspended);
        VideoStudioButton.IsEnabled=editable;
        VideoMarkTools.IsEnabled=editable&&_videoReady;
        VideoInspector.IsEnabled=VideoExportClipButton.IsEnabled=editable;
        VideoAddClipButton.IsEnabled=editable&&!_addingVideoClips;
        VideoViewerSeek.IsEnabled=VideoSeek.IsEnabled=VideoStrip.IsEnabled=VideoSplitButton.IsEnabled=VideoExportButton.IsEnabled=editable;
        VideoDeleteClipButton.IsEnabled=editable&&_timeline!.Project.Clips.Length>1;
        VideoUndoButton.IsEnabled=editable&&_timeline!.CanUndo;
        VideoRedoButton.IsEnabled=editable&&_timeline!.CanRedo;
        VideoSaveProjectButton.IsEnabled=_timeline!=null&&_videoExportCancellation==null;
        VideoRelinkButton.IsEnabled=_videoItem!=null&&_videoExportCancellation==null;
        VideoCueVisible.IsChecked=_timeline?.Project.ShowCues??true;
        VideoMarkersButton.Visibility=_videoReady&&_videoItem?.RecordingCues?.Any(c=>c?.Kind=="marker")==true?Visibility.Visible:Visibility.Collapsed;
        ListenMicButton.Visibility=_videoSource!=null&&File.Exists(RecordingAudioFiles.PathFor(_videoSource,false))?Visibility.Visible:Visibility.Collapsed;
        ListenSystemButton.Visibility=_videoSource!=null&&File.Exists(RecordingAudioFiles.PathFor(_videoSource,true))?Visibility.Visible:Visibility.Collapsed;
        UpdateVideoPosition();
        RefreshVideoInspector();
        RefreshVideoAudioInspector();
    }

    private void VideoEdit_Click(object sender,RoutedEventArgs e)
    {
        ExecuteVideoEdit((sender as FrameworkElement)?.Tag as string);
    }

    private void ExecuteVideoEdit(string? command)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        PauseVideoForEditing();
        if(command=="delete"&&_selectedVideoTrack is Guid track&&timeline.Project.Tracks.Any(item=>item.Id==track)){
            DeleteVideoTrack();
            return;
        }
        int fps=timeline.Selected.SourceIndex==0?_videoItem?.Fps??0:0;
        var minimum=TimeSpan.FromSeconds(1d/(fps>0?fps:30));
        bool changed=command switch{
            "split"=>timeline.Split(minimum),"in"=>timeline.Trim(true,minimum),"out"=>timeline.Trim(false,minimum),
            "delete" or "delete-clip"=>timeline.Delete(),"undo"=>timeline.Undo(),"redo"=>timeline.Redo(),_=>false
        };
        if(!changed){
            if(command is "split" or "in" or "out")SetStatus("Chọn mốc nằm trong đoạn và giữ ít nhất một khung hình.");
            return;
        }
        if(command is "delete-clip" or "delete")_selectedVideoTrack=null;
        CommitVideoEdit();
    }

    private void CommitVideoEdit()
    {
        SeekVideoMedia();
        RefreshVideoControls();
        MarkVideoProjectDirty();
        RequestVideoAudio();
    }

    private void MarkVideoProjectDirty(bool playback=false)
    {
        if(_videoProjectReadError)return;
        if(!playback||!_videoProjectDirty)_videoSaveAfter=DateTime.UtcNow.AddMilliseconds(playback?3000:650);
        if(!_videoProjectDirty)SavedLabel.Text="Đang chờ tự lưu";
        _videoProjectDirty=true;
        _videoTimer.Start();
    }

    private bool FlushVideoProject(bool report=true)
    {
        if(!_videoProjectDirty||_videoItem is not {} item||_timeline is not {} timeline)return true;
        try{
            var project=WithSourceIdentity(timeline.Project);
            project.Save(VideoAutosavePath(item.Id));
            _videoProjectDirty=false;
            SavedLabel.Text="✓ Đã tự lưu dự án";
            return true;
        }catch(Exception error){
            App.Log(error);_videoSaveAfter=DateTime.UtcNow.AddSeconds(5);
            SavedLabel.Text="Chưa lưu được dự án";
            if(report)SetStatus("Không lưu được dự án video: "+error.Message);
            return false;
        }
    }

    private static VideoProject WithSourceIdentity(VideoProject project)=>
        project.Source!=null||!File.Exists(project.SourcePath)?project:project with{Source=VideoSourceIdentity.Read(project.SourcePath)};

    private void SaveVideoProject_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||_timeline is not {} timeline||_videoItem is not {} item||_exportInProgress)return;
        PauseVideoForEditing();
        try{
            var path=_videoProjectPath;
            if(path==null){
                var save=new SaveFileDialog{Filter=VideoProjectFilter,DefaultExt=".cvxvideo",FileName=SafeVideoName(item.Title)};
                if(save.ShowDialog(this)!=true)return;
                path=save.FileName;
            }
            WithSourceIdentity(timeline.Project).Save(path);
            _videoProjectPath=path;
            FlushVideoProject(false);
            SetStatus("Đã lưu dự án video: "+path);
        }catch(Exception error){App.Log(error);SetStatus("Không lưu được dự án video: "+error.Message);}
    }

    private void OpenVideoProject_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||_busy||_exportInProgress||!FlushEdits()||!FlushVideoProject())return;
        var open=new OpenFileDialog{Filter=VideoProjectFilter};
        if(open.ShowDialog(this)!=true)return;
        try{
            var project=VideoProject.Open(open.FileName);
            var nearby=Path.Combine(Path.GetDirectoryName(open.FileName)!,Path.GetFileName(project.SourcePath));
            if(!File.Exists(project.SourcePath)&&project.Source!=null&&SourceMatches(nearby,project.Source))
                project=project with{SourcePath=nearby};
            var item=_store.Items.FirstOrDefault(entry=>entry.IsVideo&&SamePath(entry.FilePath,project.SourcePath));
            bool newItem=item==null;
            if(item==null)
                item=new MediaItem{IsVideo=true,Title=Path.GetFileNameWithoutExtension(project.SourcePath),
                    FilePath=project.SourcePath,Source="Dự án video",Seconds=project.SourceDuration.TotalSeconds,IsLinkedVideo=true};
            string autosave=VideoAutosavePath(item.Id);
            project.Save(autosave);
            if(newItem){
                try{_store.Add(item);}catch{_store.Items.Remove(item);File.Delete(autosave);throw;}
            }
            SearchBox.Clear();
            if(ReferenceEquals(Selected,item)){
                DetachVideoProject();
                OpenVideoProject(item,project);
                _videoProjectPath=open.FileName;
            }else{
                _openingVideoProject=(item.Id,open.FileName);
                OpenHistoryItem(item);
            }
        }catch(Exception error){App.Log(error);SetStatus("Không mở được dự án video: "+error.Message);}
    }

    private (string Id,string Path)? _openingVideoProject;

    private void RelinkVideoSource_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||_videoItem is not {} item||_busy||_exportInProgress)return;
        var open=new OpenFileDialog{Filter="Video MP4|*.mp4",Title="Tìm video nguồn"};
        if(open.ShowDialog(this)!=true)return;
        try{
            var project=_timeline?.Project;
            int index=_timeline?.Selected.SourceIndex??0;
            if(!SourceMatches(open.FileName,project?.Input(index).Identity))
                throw new InvalidDataException("Tệp đã chọn không khớp với video gốc của dự án.");
            var oldPath=item.FilePath;
            bool oldLink=item.IsLinkedVideo;
            if(index==0&&!SamePath(oldPath,open.FileName)){
                item.FilePath=open.FileName;item.IsLinkedVideo=true;
                try{_store.Save();}catch{item.FilePath=oldPath;item.IsLinkedVideo=oldLink;throw;}
            }
            _videoSource=open.FileName;
            if(project!=null){
                if(index==0)project=project with{SourcePath=open.FileName};
                else{
                    var sources=(VideoInput[])project.AdditionalSources.Clone();
                    sources[index-1]=sources[index-1] with{Path=open.FileName};
                    project=project with{AdditionalSources=sources};
                }
                _timeline=new VideoTimeline(project);
                ReadPrimaryVideoSize(_timeline);
                MarkVideoProjectDirty();
                FlushVideoProject();
            }
            UpdateMetadata(item);
            _verifiedVideoSources.Clear();
            OpenVideoMedia();
        }catch(Exception error){App.Log(error);SetStatus("Không liên kết được video: "+error.Message);}
    }

    private void VideoMarkers_Click(object sender,RoutedEventArgs e)
    {
        if(!VideoCanEdit||_timeline is not {} timeline)return;
        var menu=new ContextMenu{PlacementTarget=sender as FrameworkElement??VideoMarkersButton,Placement=System.Windows.Controls.Primitives.PlacementMode.Bottom};
        var project=timeline.Project;
        var sourceMarkers=(_videoItem?.RecordingCues??[]).OfType<RecordingCue>()
            .Where(c=>c.Kind=="marker"&&c.Milliseconds>=0&&c.Milliseconds<project.SourceDuration.TotalMilliseconds)
            .Select(c=>new VideoCue(0,TimeSpan.FromMilliseconds(c.Milliseconds),"marker"));
        var markers=VideoCuePlan.Create(project.Clips,project.Cues.Where(c=>c.Kind=="marker").Concat(sourceMarkers).Distinct());
        foreach(var cue in markers.Take(300)){
            var time=cue.Start;
            var marker=new MenuItem{Header=$"Mốc {menu.Items.Count+1} · {VideoTimecode.Format(time)}"};
            marker.Click+=(_,_)=>SeekVideoProject(time);
            menu.Items.Add(marker);
        }
        if(menu.Items.Count>0){AppMenus.Decorate(menu);menu.IsOpen=true;}
        else SetStatus("Không có marker trong các đoạn đang giữ.");
    }

    private async void ExportVideo_Click(object sender,RoutedEventArgs e)
    {
        if(RequireLicense())await ExportVideoProjectAsync((sender as Button)?.Tag as string=="clip");
    }

    private async Task ExportVideoProjectAsync(bool selectedOnly=false)
    {
        if(_busy||_exportInProgress||!VideoCanEdit||_timeline is not {} timeline||_videoItem is not {} item)return;
        PauseVideoForEditing();
        var project=timeline.Project;
        VideoRange[] clips=selectedOnly?[timeline.Selected]:project.Clips;
        foreach(var index in clips.Select(clip=>clip.SourceIndex).Distinct()){
            var input=project.Input(index);
            if(!SourceMatches(input.Path,input.Identity)){
                SetStatus("Thiếu hoặc sai video nguồn: "+Path.GetFileName(input.Path)+". Chọn đoạn đó để liên kết lại.");
                return;
            }
        }
        var suffix=selectedOnly?$"-doan-{timeline.Selected.Number}":"-timeline";
        var save=new SaveFileDialog{Filter="Video MP4|*.mp4",DefaultExt=".mp4",FileName=SafeVideoName(item.Title)+suffix};
        if(save.ShowDialog(this)!=true)return;
        if(project.Inputs.Any(input=>SamePath(input.Path,save.FileName))||_store.Items.Any(entry=>SamePath(entry.FilePath,save.FileName))){
            SetStatus("Chọn tên hoặc thư mục khác để giữ video nguồn.");return;
        }
        using var cancellation=new CancellationTokenSource();
        _exportInProgress=true;_videoExportCancellation=cancellation;
        VideoExportProgress.Value=0;
        VideoExportStatus.Visibility=Visibility.Visible;
        VideoExportLabel.Text="Đang xuất MP4 · 0%";
        RefreshVideoControls();
        try{
            await VideoTrimExport.ExportAsync(project,save.FileName,clips,selectedOnly?timeline.Offset(project.SelectedClip):TimeSpan.Zero,new Progress<double>(value=>{
                if(_videoExportCancellation!=cancellation)return;
                VideoExportProgress.Value=Math.Clamp(value,0,100);
                VideoExportLabel.Text=$"Đang xuất MP4 · {VideoExportProgress.Value:0}%";
            }),cancellation.Token);
            _lastVideoExport=save.FileName;VideoOpenExportFolder.Visibility=Visibility.Visible;
            SetStatus("Đã xuất video: "+save.FileName);
            if(AppDialog.Show(this,"Đã xuất MP4 thành công. Mở thư mục chứa video?","Xuất video",MessageBoxButton.YesNo,MessageBoxImage.Information)==MessageBoxResult.Yes)OpenVideoExportFolder_Click(this,new());
        }catch(OperationCanceledException){SetStatus("Đã hủy xuất video.");}
        catch(Exception error){App.Log(error);SetStatus("Không xuất được video: "+error.Message);}
        finally{
            _videoExportCancellation=null;_exportInProgress=false;
            VideoExportStatus.Visibility=Visibility.Collapsed;
            RefreshVideoControls();
            if(_closeAfterExport){_closeAfterExport=false;_=Dispatcher.BeginInvoke(Close);}
        }
    }

    private void CancelVideoExport_Click(object sender,RoutedEventArgs e)=>_videoExportCancellation?.Cancel();

    private static string SafeVideoName(string title)=>string.Concat(title.Select(c=>Path.GetInvalidFileNameChars().Contains(c)?'_':c));
    private static bool SamePath(string left,string right)=>!string.IsNullOrWhiteSpace(left)&&!string.IsNullOrWhiteSpace(right)&&string.Equals(Path.GetFullPath(left),Path.GetFullPath(right),StringComparison.OrdinalIgnoreCase);
}
