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
        _videoTimer.Start();
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
        _previewSourceIndex=_timeline?.Selected.SourceIndex??0;
        var input=_timeline?.Project.Input(_previewSourceIndex);
        _videoSource=input?.Path??_videoItem?.FilePath;
        _resumeAfterVideoOpen=resume;
        _videoReady=false;
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
        ExitVideoStudio();
        ClearVideoAudio();
        _videoTimer.Stop();
        _videoSourceSize=null;
        _selectedVideoTrack=null;
        SetVideoTool(null);
        VideoMarkTools.Visibility=VideoInspector.Visibility=Visibility.Collapsed;
        _videoFullscreen?.Close();
        _resumeAfterVideoOpen=false;
        VideoPreview.Close();
        VideoPreview.Source=null;
        SetVideoPlaying(false);
        if(_videoItem!=null)RememberVideoSession(null);
        _videoItem=null;_timeline=null;_videoSource=null;_videoProjectPath=null;
        UpdateVideoWorkspace();
        _videoReady=false;_videoProjectDirty=false;_videoProjectReadError=false;
        RefreshVideoControls();
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
            try{project=WithSourceIdentity(project);}catch(Exception error){App.Log(error);}
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
        if(_resumeAfterVideoOpen){_resumeAfterVideoOpen=false;VideoPreview.Play();SetVideoPlaying(true);}
        RefreshVideoControls();
    }

    private void Video_Ended(object sender,RoutedEventArgs e)
    {
        if(_playing&&_timeline is {} timeline)AdvanceVideo(timeline.Selected.End);
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
        if(_videoReady&&_playing)AdvanceVideo(VideoPreview.Position);
        if(_videoProjectDirty&&DateTime.UtcNow>=_videoSaveAfter)FlushVideoProject(false);
    }

    private void AdvanceVideo(TimeSpan sourcePosition)
    {
        if(_timeline is not {} timeline)return;
        var clip=timeline.Selected;
        if(sourcePosition<clip.Start)return;
        int index=timeline.Project.SelectedClip;
        if(sourcePosition>=clip.End){
            if(index+1<timeline.Project.Clips.Length){
                var next=timeline.Project.Clips[index+1];
                timeline.Track(index+1,next.Start);
                if(next.SourceIndex!=clip.SourceIndex)OpenVideoMedia(resume:true);
                else if(next.Start!=clip.End||sourcePosition>=next.End){VideoPreview.Position=next.Start;SyncVideoAudio(force:true);}
                else timeline.Track(index+1,sourcePosition);
            }else{
                timeline.Track(index,clip.End);
                VideoPreview.Pause();SetVideoPlaying(false);
                VideoPreview.Position=clip.End;
            }
        }else timeline.Track(index,sourcePosition);
        MarkVideoProjectDirty(playback:true);
        UpdateVideoPosition();
        RefreshVideoInspector();
    }

    private void SetVideoPlaying(bool playing)
    {
        _playing=playing;
        SyncVideoAudio(force:true);
        var name=playing?"Tạm dừng":"Phát";
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
        if(timeline.Project.Position>=timeline.Project.Duration)SeekVideoProject(TimeSpan.Zero);
        else SeekVideoMedia();
        if(!PrepareVideoPlayback())return;
        if(_videoReady){VideoPreview.Play();SetVideoPlaying(true);}
        else if(File.Exists(_videoSource)&&VideoPreview.Source!=null)_resumeAfterVideoOpen=true;
    }

    private bool VideoCanEdit=>_timeline!=null&&_videoExportCancellation==null
        &&ReferenceEquals(Selected,_videoItem)&&MediaList.SelectedItems.Count==1;

    private void SeekVideoProject(TimeSpan position)
    {
        if(!VideoCanEdit||_timeline is not {} timeline)return;
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
        var project=timeline.Project;
        var clip=timeline.Selected;
        if(clip.SourceIndex!=_previewSourceIndex||!SamePath(project.Input(clip.SourceIndex).Path,_videoSource??"")){OpenVideoMedia();return;}
        if(!_videoReady)return;
        var local=project.Position-timeline.Offset(project.SelectedClip);
        VideoPreview.Position=clip.Start+(local<TimeSpan.Zero?TimeSpan.Zero:local>clip.Duration?clip.Duration:local);
    }

    private void VideoSeek_Changed(object sender,RoutedPropertyChangedEventArgs<double> e)
    {
        if(!_syncVideoSeek&&VideoCanEdit)SeekVideoProject(TimeSpan.FromSeconds(e.NewValue));
    }

    private void UpdateVideoPosition()
    {
        var project=_timeline?.Project;
        _syncVideoSeek=true;
        try{
            VideoSeek.Maximum=project?.Duration.TotalSeconds??0;
            VideoSeek.Value=project?.Position.TotalSeconds??0;
        }finally{_syncVideoSeek=false;}
        if(_videoStudio)VideoStrip.Show(project,_selectedVideoTrack);
        SyncVideoAudio();
        UpdateVideoMarks();
        VideoViewerTime.Text=VideoTime.Text=project==null?"":$"{VideoTimecode.Format(project.Position)} / {VideoTimecode.Format(project.Duration)} · Đoạn {project.Clips[project.SelectedClip].Number}";
    }

    private void RefreshVideoControls()
    {
        bool editable=VideoCanEdit;
        VideoPlaybackButton.IsEnabled=VideoFullscreenButton.IsEnabled=editable&&_videoReady;
        VideoStudioButton.IsEnabled=editable;
        VideoMarkTools.IsEnabled=editable&&_videoReady;
        VideoInspector.IsEnabled=VideoExportClipButton.IsEnabled=editable;
        VideoAddClipButton.IsEnabled=editable&&!_addingVideoClips;
        VideoSeek.IsEnabled=VideoStrip.IsEnabled=VideoSplitButton.IsEnabled=VideoExportButton.IsEnabled=editable;
        VideoDeleteClipButton.IsEnabled=editable&&_timeline!.Project.Clips.Length>1;
        VideoUndoButton.IsEnabled=editable&&_timeline!.CanUndo;
        VideoRedoButton.IsEnabled=editable&&_timeline!.CanRedo;
        VideoSaveProjectButton.IsEnabled=_timeline!=null&&_videoExportCancellation==null;
        VideoRelinkButton.IsEnabled=_videoItem!=null&&_videoExportCancellation==null;
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
        if(!VideoCanEdit||_videoItem?.RecordingCues is not {} cues||_timeline is not {} timeline)return;
        var menu=new ContextMenu{PlacementTarget=VideoMarkersButton,Placement=System.Windows.Controls.Primitives.PlacementMode.Bottom};
        foreach(var cue in cues.Where(c=>c?.Kind=="marker"&&c.Milliseconds>=0&&c.Milliseconds<=timeline.Project.SourceDuration.TotalMilliseconds).Take(150)){
            var sourceTime=TimeSpan.FromMilliseconds(cue.Milliseconds);
            int index=Array.FindIndex(timeline.Project.Clips,clip=>clip.SourceIndex==0&&sourceTime>=clip.Start&&sourceTime<clip.End);
            if(index<0)continue;
            var time=timeline.Offset(index)+sourceTime-timeline.Project.Clips[index].Start;
            var marker=new MenuItem{Header=$"Mốc {menu.Items.Count+1} · {VideoTimecode.Format(time)}"};
            marker.Click+=(_,_)=>SeekVideoProject(time);
            menu.Items.Add(marker);
        }
        if(menu.Items.Count>0){AppMenus.Decorate(menu);menu.IsOpen=true;}
        else SetStatus("Không có mốc trong các đoạn đang giữ.");
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
            SetStatus("Đã xuất video: "+save.FileName);
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
