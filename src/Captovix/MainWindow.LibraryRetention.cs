using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Captovix;
public partial class MainWindow
{
    private readonly DispatcherTimer _retentionTimer=new(DispatcherPriority.Background);
    private LibraryRetentionSettings _retention=new();
    private DateTime _retentionDue;
    private bool _retentionRunning;
    private void InitializeRetention(){
        try{
            _retentionTimer.Tick+=async(_,_)=>{
                try{await RunRetentionAsync();}
                catch(Exception error){
                    _retentionTimer.Stop();
                    App.Log(new InvalidOperationException("Retention tick failed; timer stopped for this session.",error));
                    SetStatus("Tạm dừng tự dọn thư viện; mở lại Captovix hoặc lưu lại lịch để thử lại.");
                }
            };
            _retention=LibraryRetentionSettings.Load(App.DataRoot);
            ScheduleRetention();
        }catch(Exception error){
            _retentionTimer.Stop();
            App.Log(new InvalidOperationException("Retention initialization failed; startup continues.",error));
            SetStatus("Tạm dừng tự dọn thư viện: "+error.Message);
        }
    }
    private bool ScheduleRetention(){
        DateTime now=DateTime.Now;
        DateTime? due=null;
        TimeSpan? delay=null;
        try{
            _retentionTimer.Stop();if(!_retention.Enabled)return true;
            // Use one clock snapshot. A missed deadline remains the expiry cutoff,
            // but the dispatcher must never receive a negative or zero interval.
            due=_retention.ScheduledRun(now);
            delay=LibraryRetentionSettings.TimerDelay(due.Value,now);
            _retentionDue=due.Value;
            _retentionTimer.Interval=delay.Value;
            _retentionTimer.Start();
            return true;
        }catch(Exception error){
            _retentionTimer.Stop();
            App.Log(new InvalidOperationException($"Retention scheduling failed; now={now:O}; due={due:O}; delay={delay}; timer stopped.",error));
            SetStatus("Tạm dừng tự dọn thư viện; lưu lại lịch hoặc mở lại Captovix để thử lại.");
            return false;
        }
    }
    private FrameworkElement BuildRetentionSettings(){
        var panel=new StackPanel{Margin=new(0,20,0,8)};
        panel.Children.Add(new TextBlock{Text="Tự dọn thư viện",FontSize=19,FontWeight=FontWeights.SemiBold});
        var enabled=new CheckBox{Content="Tự chuyển ảnh / video hết hạn vào Thùng rác Windows",IsChecked=_retention.Enabled,Margin=new(0,12,0,8)};panel.Children.Add(enabled);
        var row=new DockPanel();row.Children.Add(new TextBlock{Text="Số ngày giữ kể từ ngày chụp / quay",VerticalAlignment=VerticalAlignment.Center});
        var days=new TextBox{Text=_retention.Days.ToString(),Width=90,HorizontalAlignment=HorizontalAlignment.Right};row.Children.Add(days);panel.Children.Add(row);
        panel.Children.Add(new TextBlock{Text="Chạy một lần mỗi ngày lúc 23:59 theo giờ Windows khi Captovix đang mở hoặc chạy nền. Áp dụng cả nội dung cũ. Nếu lỡ lịch do tắt máy/thoát app, lần mở app tiếp theo sẽ gom và dọn bù nội dung đã đủ hạn tại lịch bị lỡ khi app rảnh. Video liên kết chỉ gỡ khỏi thư viện; tệp ngoài app được giữ. Không xóa bản sao lưu.",TextWrapping=TextWrapping.Wrap,Margin=new(0,10,0,8)});
        var status=new TextBlock{TextWrapping=TextWrapping.Wrap};panel.Children.Add(status);
        var save=new Button{Content=AppIcons.Content("Save","Lưu lịch tự dọn"),HorizontalAlignment=HorizontalAlignment.Right,Margin=new(0,10,0,0)};panel.Children.Add(save);
        save.Click+=(_,_)=>{
            if(_retentionRunning){status.Text="Đang hoàn tất phiên tự dọn. Vui lòng chờ.";return;}
            if(!int.TryParse(days.Text,out int count)||count is <1 or >36500){status.Text="Nhập số ngày nguyên từ 1 đến 36500.";return;}
            try{
                var value=_retention with{Enabled=enabled.IsChecked==true,Days=count,EnabledAt=enabled.IsChecked==true&&!_retention.Enabled?DateTime.Now:_retention.EnabledAt};value.Save(App.DataRoot);_retention=value;
                if(!ScheduleRetention()){status.Text="Đã lưu cấu hình nhưng chưa chạy được lịch tự dọn. Vui lòng thử lại.";return;}
                status.Text=value.Enabled?$"Đã lưu · chạy lúc {_retentionDue:dd/MM/yyyy HH:mm}. Nội dung đủ {count} ngày sẽ được chuyển vào Thùng rác Windows.":"Đã tắt tự dọn thư viện.";
            }catch(Exception error){App.Log(error);status.Text="Chưa lưu được: "+error.Message;}
        };
        return panel;
    }
    private async Task RunRetentionAsync(){
        _retentionTimer.Stop();
        var now=DateTime.Now;
        if(!_retention.Enabled||_retentionRunning)return;
        // An overdue timer after sleep is handled once, using the scheduled day's expiry dates.
        if(now<_retentionDue){ScheduleRetention();return;}
        if(_retention.LastRunDate==_retentionDue.Date){ScheduleRetention();return;}
        if(_busy||_exportInProgress||_libraryTransferBusy||_updateApplying||_settingsDialog!=null||_bar!=null||_session?.Dirty==true){
            _retentionTimer.Interval=TimeSpan.FromMinutes(1);_retentionTimer.Start();return;
        }
        if(!FlushEdits()||!FlushVideoProject()){ScheduleRetention();return;}
        var expired=_store.Items.Where(item=>_retention.IsDue(item.Created,_retentionDue)).ToArray();
        var selectionBefore=Selected;
        _retentionRunning=true;SetBusy(true);
        RetentionBatch? batch=null;bool metadataSaved=false;
        _libraryRefreshHold++;bool changing=_changingSelection;_changingSelection=true;
        try{
            if(expired.Length>0){
                if(expired.Contains(_activeItem)||expired.Contains(_videoItem)){DetachVideoProject();UnloadEditor();VideoPreview.Close();}
                var kept=_store.Items.Except(expired).ToArray();
                var paths=await Task.Run(()=>RetentionPaths(expired,kept));
                batch=await Task.Run(()=>RetentionBatch.Stage(expired,paths));
                foreach(var item in expired)_store.Items.Remove(item);
                _store.Save();metadataSaved=true;
                batch=batch with{Committed=true};batch.Save();
                // A retained project may still own the source; do not resurrect its expired library tile.
                _retention=_retention with{RemovedSourcePaths=(_retention.RemovedSourcePaths??[]).Concat(expired.Select(item=>item.FilePath)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()};
                _retention.Save(App.DataRoot);
                await StaImageWorker.RunExport(batch.Recycle);
                _store.ClearThumbnailCache();
            }
            _retention=_retention with{LastRunDate=_retentionDue.Date};_retention.Save(App.DataRoot);
            SetStatus($"Tự dọn thư viện: đã chuyển {expired.Length} nội dung hết hạn vào Thùng rác Windows.");
        }catch(Exception error){
            App.Log(error);
            if(batch!=null&&Directory.Exists(batch.Folder)){
                try{await Task.Run(()=>batch.Rollback(false));foreach(var item in expired)if(!_store.Items.Contains(item))_store.Items.Add(item);_store.Save();RetentionBatch.FinishRollback();}
                catch(Exception restore){App.Log(restore);}
            }else if(batch==null&&!metadataSaved){foreach(var item in expired)if(!_store.Items.Contains(item))_store.Items.Add(item);}
            // Do not rescan/delete continuously on a locked file or failed recycle operation.
            _retention=_retention with{LastRunDate=_retentionDue.Date};try{_retention.Save(App.DataRoot);}catch(Exception save){App.Log(save);}
            SetStatus("Chưa tự dọn được thư viện; sẽ thử lại trong lịch ngày tiếp theo. "+error.Message);
        }finally{
            _libraryRefreshHold--;_changingSelection=changing;_retentionRunning=false;SetBusy(false);RebuildRecent();RefreshLibrary();
            MediaList.SelectedItem=selectionBefore!=null&&_store.Items.Contains(selectionBefore)?selectionBefore:_recentItems.FirstOrDefault();
            ScheduleRetention();if(_exitRequested)_=Dispatcher.BeginInvoke(RequestExit);
        }
    }
    private static string[] RetentionPaths(MediaItem[] expired,MediaItem[] kept){
        var protectedPaths=kept.Select(item=>Path.GetFullPath(item.FilePath)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        // Do not break a newer project that still refers to an older recording.
        foreach(var item in kept.Where(item=>item.IsVideo)){
            string project=VideoAutosavePath(item.Id);
            if(!File.Exists(project)&&!File.Exists(project+".bak"))continue;
            var saved=VideoProject.Recover(project,out _);
            if(saved==null)throw new InvalidDataException("Không đọc được dự án đang giữ; hoãn tự dọn để giữ nguồn.");
            foreach(var input in saved.Inputs)protectedPaths.Add(Path.GetFullPath(input.Path));
            foreach(var camera in saved.Cameras.Values)protectedPaths.Add(Path.GetFullPath(camera.Path));
        }
        var paths=new List<string>();
        foreach(var item in expired){
            if(!item.IsLinkedVideo&&!protectedPaths.Contains(Path.GetFullPath(item.FilePath))){
                paths.Add(item.FilePath);
                if(item.IsVideo){paths.AddRange(RecordingAudioFiles.Existing(item.FilePath));paths.Add(RecordingPresenter.PathFor(item.FilePath));paths.Add(RecordingCamera.PathFor(item.FilePath));paths.Add(RecordingCamera.MetadataFor(item.FilePath));}
            }
            string edit=DocumentStore.EditPath(item.Id);paths.Add(edit);paths.Add(DocumentStore.ThumbnailPath(item.Id));
            if(item.IsVideo){string project=VideoAutosavePath(item.Id);paths.Add(project);paths.Add(project+".bak");}
            if(HybridCapture.MetadataPath(App.DataRoot,item.Id) is {} hybrid)paths.Add(hybrid);
            // Older edit backups are kept; explicit update backups are never touched.
        }
        return paths.Where(path=>!protectedPaths.Contains(Path.GetFullPath(path))).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
