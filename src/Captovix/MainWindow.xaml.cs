using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Captovix;
public partial class MainWindow : Window
{
    private readonly MediaStore _store;
    private readonly DispatcherTimer _videoTimer=new(){Interval=TimeSpan.FromMilliseconds(45)};
    private readonly List<int> _hotkeys=[];
    private List<DisplayInfo> _displays=[];
    private CaptureEngine? _engine;
    private RecordingBar? _bar;
    private bool _busy,_stopping,_playing,_loaded;
    private PixelRect? _lastRegion;
    private CancellationTokenSource? _captureCancellation;
    private MediaItem? Selected => MediaList.SelectedItem as MediaItem;
    private bool _firstFrameRendered,_startupInitialized;
    public MainWindow()
    {
        var startupClock=Stopwatch.StartNew();
        using var measurement=PerformanceTimings.Measure("window.construct");
        ContentRendered+=(_,_)=>{
            if(_firstFrameRendered)return;
            _firstFrameRendered=true;
            PerformanceTimings.Record("window.first-paint",startupClock.Elapsed.TotalMilliseconds);
            QueueStartupConfirmation();
        };
        IsVisibleChanged+=(_,_)=>QueueStartupConfirmation();
        StateChanged+=(_,_)=>QueueStartupConfirmation();
        WindowPlacement.Attach(this);InitializeComponent();Activated+=(_,_)=>QueueUpdateNotice();InitializeBrand();InitializeEditor();_store=new();RebuildRecent();MediaList.ItemsSource=_recentItems;
        CollectionViewSource.GetDefaultView(_recentItems).Filter=o=>{var i=(MediaItem)o;return LibraryMatch(i);};
        _store.Items.CollectionChanged+=(_,_)=>{if(_libraryRefreshHold==0)RebuildRecent();};CountLabel.Text=$"{_recentItems.Count}/{_store.Items.Count}";
        InitializeShell();InitializeLibraryProjects();InitializeComposition();InitializeToolOrder();InitializeIcons();InitializeVideoTools();InitializeOpacityControls();InitializeVideoAudio();InitializeStudioAccess();IsVisibleChanged+=(_,_)=>{if(IsVisible&&_fitMode&&_session!=null)Dispatcher.BeginInvoke(FitImage,DispatcherPriority.Loaded);};Loaded+=(_,_)=>Initialize();Closing+=Window_Closing;
        VideoStrip.SeekRequested+=SeekVideoProject;
        VideoStrip.ClipSelected+=SelectVideoClip;
        VideoStrip.MoveRequested+=MoveVideoClip;
        VideoStrip.TrackSelected+=SelectVideoTrack;
        VideoStrip.AudioSelected+=SelectVideoAudio;
        VideoStrip.TrackChanged+=ChangeVideoTrackTime;
        _videoTimer.Tick+=(_,_)=>TickVideoProject();
    }
    private void Initialize()
    {
        if(_loaded)return;
        RefreshSources();var s=_store.LoadSettings();SourceMode.SelectedIndex=s.SourceMode==3?0:Math.Clamp(s.SourceMode,0,2);
        DelayPicker.SelectedIndex=s.Delay==5?2:s.Delay==3?1:0;FpsPicker.SelectedIndex=s.Fps==60?2:s.Fps==15?0:1;
        Surface.KeepAspectRatio=false;_alignmentReference=(AlignmentReference)Math.Clamp(s.ObjectAlignmentReference,0,2);Surface.SnapEnabled=s.ObjectSnap;ObjectSnapToggle.IsChecked=s.ObjectSnap;
        _micDevice=s.MicDevice;_systemAudioDevice=s.SystemAudioDevice;
        _videoProfile=s.VideoProfile;_highlightRecordingClicks=s.HighlightRecordingClicks;
        CursorToggle.IsChecked=s.Cursor;MicToggle.IsChecked=s.Mic;SystemToggle.IsChecked=s.SystemAudio;HardwareToggle.IsChecked=s.Hardware;
        DisplayPicker.SelectedItem=_displays.FirstOrDefault(d=>d.Device==s.Display)??_displays.FirstOrDefault();
        _loaded=true;var hwnd=new WindowInteropHelper(this).Handle;HwndSource.FromHwnd(hwnd)?.AddHook(WindowMessage);
        InitializeCaptureWorkflow();
        if(ReleaseRecoveryState.Startup(App.DataRoot,VersionInfo.Current))SetStatus("Lần cập nhật/khởi động trước chưa hoàn tất. Trợ giúp → Cập nhật & khôi phục dữ liệu để kiểm tra bản sao lưu.");InitializeUpdates();InitializeLicensing();
        if(_store.StartupWarning!=null)AppDialog.Show(this,_store.StartupWarning,"Kiểm tra dữ liệu cũ",MessageBoxButton.OK,MessageBoxImage.Warning);
        _startupInitialized=true;
        RestoreVideoSession();
        QueueStartupConfirmation();
    }
    private void QueueStartupConfirmation()
    {
        if(!_startupInitialized||!_firstFrameRendered||!IsVisible||WindowState==WindowState.Minimized)return;
        Dispatcher.BeginInvoke(() => {
            if(IsVisible&&WindowState!=WindowState.Minimized)(Application.Current as App)?.ConfirmStartup();
        },DispatcherPriority.ContextIdle);
    }
    private void RefreshSources()
    {
        var device=(DisplayPicker.SelectedItem as DisplayInfo)?.Device;var hwnd=(WindowPicker.SelectedItem as WindowInfo)?.Handle;
        _displays=Native.Displays();DisplayPicker.ItemsSource=_displays;DisplayPicker.SelectedItem=_displays.FirstOrDefault(d=>d.Device==device)??_displays.FirstOrDefault();
        var windows=Native.Windows(true);WindowPicker.ItemsSource=windows;WindowPicker.SelectedItem=windows.FirstOrDefault(w=>w.Handle==hwnd)??windows.FirstOrDefault();
        DisplaySummary.Text=string.Join("\n\n",_displays.Select(d=>$"{d.Label}\nGốc pixel: {d.Bounds.X}, {d.Bounds.Y}"));
    }
    private nint WindowMessage(nint hwnd,int msg,nint w,nint l,ref bool handled)
    {
        if(HandleWorkflowMessage(msg,w,l)){handled=true;return 0;}
        if(msg==0x312){handled=true;switch((int)w){case 1:if(!_busy)QueueCapture(false);break;case 2:if(!_busy)QueueCapture(true);break;case 3:StopRecording();break;case 4:_bar?.PauseFromHotkey();break;case MarkerHotkeyId:_bar?.Mark();break;}}
        if(msg==0x7E){
            if(_recordingActive)Dispatcher.BeginInvoke(()=>StopRecording("Màn hình đã thay đổi."));
            else if(!_busy)Dispatcher.BeginInvoke(RefreshSources);
        }
        if(msg is 0x1A or 0x31A)Dispatcher.BeginInvoke(AppearanceManager.RefreshSystem);
        return 0;
    }
    private string? _micDevice,_systemAudioDevice;
    private RecordingProfile? _videoProfile;
    private bool _highlightRecordingClicks=true;
    private AppSettings Settings()=>new(){VideoProfile=_videoProfile,HighlightRecordingClicks=_highlightRecordingClicks,MicDevice=_micDevice,SystemAudioDevice=_systemAudioDevice,ObjectKeepRatio=Surface.KeepAspectRatio,ObjectAlignmentReference=(int)_alignmentReference,ObjectSnap=Surface.SnapEnabled,SourceMode=SourceMode.SelectedIndex,Delay=int.Parse(((ComboBoxItem)DelayPicker.SelectedItem).Tag.ToString()!),Fps=int.Parse(((ComboBoxItem)FpsPicker.SelectedItem).Tag.ToString()!),Cursor=CursorToggle.IsChecked==true,Mic=MicToggle.IsChecked==true,SystemAudio=SystemToggle.IsChecked==true,Hardware=HardwareToggle.IsChecked==true,Display=(DisplayPicker.SelectedItem as DisplayInfo)?.Device??""};
    private bool _recordingActive;
    private const int MarkerHotkeyId=105;
    private async Task CaptureAsync(bool video,PhotoAction action=PhotoAction.Current)
    {
        if(!LicenseRuntime.Full){if(video||action is not (PhotoAction.Current or PhotoAction.Region)){RequireLicense();return;}action=PhotoAction.Region;}
        if(_busy||ScreenColorPicker.IsPicking||_updateApplying||!_captureGate.TryEnter())return;
        var previousWindowState=WindowState;var wasVisible=IsVisible;bool success=false,showAfter=video,failed=false;PixelRect? scrollNext=null;bool scrollHorizontal=false;string? finalStatus=null;
        HybridEvidence? liveHybrid=null;FrozenDesktop? frozen=null;
        DispatcherTimer? sourceWatch=null;
        WindowInfo? window=null;
        RecordingAudioSettings? audioSettings=null;
        RecordingAudio? audio=null;
        RecordingPresenter? presenter=null;
        DispatcherTimer? diskWatch=null;
        bool markerHotkey=false;
        try {
            if(video){
                _recordingInterruption=null;
                _licenseDialog?.Close();
                _settingsDialog?.Close();
                SetBusy(true);
            }
            _captureCancellation=new();var cancellation=_captureCancellation.Token;RegisterCaptureEscape();
            var s=Settings();if(!video){if(action is PhotoAction.Region or PhotoAction.Repeat)s.SourceMode=0;else if(action==PhotoAction.Window)s.SourceMode=1;else if(action==PhotoAction.Display)s.SourceMode=2;else if(action==PhotoAction.Element)s.SourceMode=3;}
            if(!video&&s.SourceMode!=1){
                _trayMenu?.SetCurrentValue(ContextMenu.IsOpenProperty,false);
                if(s.Delay>0)await CapturePrompts.CountdownAsync(s.Delay,cancellation);
                _displays=Native.Displays();
                var displays=_displays;
                bool hybrid=LicenseRuntime.Full&&_workflow.HybridUi;
                bool overlay=s.SourceMode is 0 or 3;
                var retained=await Task.Run(()=>{
                    var uiWindows=hybrid?HybridCaptureService.Windows():[];
                    return (Desktop:FrozenDesktop.Capture(displays,s.Cursor),Windows:uiWindows);
                },cancellation);
                frozen=retained.Desktop;
                cancellation.ThrowIfCancellationRequested();
                var prepare=overlay?Task.Run(frozen.PrepareOverlay,cancellation):Task.CompletedTask;
                try{
                    if(hybrid){SetBusy(true);SetStatus("Đang đối chiếu cấu trúc giao diện với ảnh chụp…");frozen.Hybrid=await HybridCaptureService.ReadAsync(frozen.Frame,frozen.CapturedAt,frozen.Topology,displays,cancellation,earlier:retained.Windows);}
                }finally{await prepare;}
            }
            SetBusy(true);Surface.CancelInteraction();if(!FlushEdits())return;VideoPreview.Pause();SetVideoPlaying(false);


            if(s.SourceMode==3)s.SourceMode=0;
            _store.SaveSettings(Settings());
            bool captureLicensed=LicenseRuntime.Full;var options=_workflow.Clone();if(!captureLicensed)options.PhotoStyle.BorderEnabled=false;options.Background=AfterPhotoPicker.SelectedIndex==1;options.AutoCopy=captureLicensed&&AutoCopyPhoto.IsChecked==true;
            if(!_workflowReadOnly&&captureLicensed)SaveWorkflow(options);
            showAfter=video||!options.Background||_tray?.Ready!=true;
            RefreshSources();var topology=CaptureWorkflowStore.Topology(WorkflowDisplays());
            if(frozen!=null&&frozen.Topology!=topology)throw new InvalidOperationException("Màn hình hoặc DPI đã đổi sau lúc giữ ảnh. Hãy chụp lại.");
            var display=DisplayPicker.SelectedItem as DisplayInfo??throw new InvalidOperationException("Không tìm thấy màn hình.");
            PixelRect region=display.Bounds;
            // Freeze the photo before closing menus so a hotkey can capture the open submenu.
            CloseCaptureMenus();
            if(video){
                var choice=await CapturePrompts.PickRecordingSourceAsync(this,_displays,s.SourceMode,display,s,cancellation);
                if(choice is not {} selected){finalStatus="Đã hủy quay.";return;}
                s.SourceMode=selected.Mode;
                audioSettings=selected.Audio;
                _videoProfile=selected.Profile;_highlightRecordingClicks=selected.HighlightClicks;
                s.Fps=selected.Profile.Fps;s.Hardware=selected.Hardware;HardwareToggle.IsChecked=s.Hardware;
                s.Mic=audioSettings.Microphone!=null;MicToggle.IsChecked=s.Mic;
                s.SystemAudio=audioSettings.System!=null;SystemToggle.IsChecked=s.SystemAudio;
                _micDevice=audioSettings.Microphone?.Id??_micDevice;
                _systemAudioDevice=audioSettings.System?.Id??_systemAudioDevice;
                _store.SaveSettings(Settings());
                display=selected.Display;
                region=display.Bounds;
                Hide();
            }
            if(frozen==null)await Task.Delay(220,cancellation);
            if(s.SourceMode==3){var chosen=await ElementPicker.PickAsync(_displays,cancellation,frozen,video||_workflow.RequireEnter);if(chosen==null)return;region=chosen.Value;if(RegionPicker.Choice!=CaptureChoice.Photo){scrollNext=region;scrollHorizontal=RegionPicker.Choice==CaptureChoice.Horizontal;return;}}
            else if(s.SourceMode==0){
                if(action==PhotoAction.Repeat&&CaptureWorkflowStore.CanRepeat(_workflow.LastRegion,WorkflowDisplays()))region=_workflow.LastRegion!.Bounds;
                else{if(action==PhotoAction.Repeat)_tray?.Notify("Bố trí màn hình/DPI đã thay đổi hoặc chưa có vùng cũ. Hãy chọn vùng mới.");var chosen=await RegionPicker.PickAsync(_displays,cancellation,!video&&captureLicensed,captureLicensed,frozen,video||_workflow.RequireEnter,recording:video);if(chosen==null)return;region=chosen.Value;if(!video&&RegionPicker.Choice!=CaptureChoice.Photo){scrollNext=region;scrollHorizontal=RegionPicker.Choice==CaptureChoice.Horizontal;return;}}
            }else if(s.SourceMode==1){
                window=await CapturePrompts.PickWindowAsync(cancellation,video,WindowPicker.SelectedItem as WindowInfo);
                if(window==null)return;
                if(!Native.IsWindow(window.Handle)||Native.IsIconic(window.Handle)||!Native.GetWindowRect(window.Handle,out var wr))throw new InvalidOperationException("Cửa sổ đã đóng hoặc thu nhỏ. Hãy chọn lại.");region=wr.Pixels;
            }
            if(region.IsEmpty)throw new InvalidOperationException("Vùng chọn quá nhỏ.");
            var sourceName=window?.Title??(s.SourceMode==0?$"Vùng chọn · {region}":display.Label);
            if(video)await RecordingCountdown.ShowAsync(region,_displays,cancellation);
            else if(frozen==null){await CapturePrompts.CountdownAsync(s.Delay,cancellation);await Task.Delay(220,cancellation);}
            var now=Native.Displays().Select(d=>new WorkflowDisplay(d.Device,d.Bounds,d.DpiX,d.DpiY,d.Identity));if(CaptureWorkflowStore.Topology(now)!=topology)throw new InvalidOperationException("Màn hình hoặc DPI đã thay đổi trong phiên chụp. Hãy thử chọn lại.");
            if(window!=null){var current=Native.Windows(true).FirstOrDefault(w=>w.Handle==window.Handle&&w.ProcessId==window.ProcessId);if(current==null||!Native.GetWindowRect(window.Handle,out var wr))throw new InvalidOperationException("Cửa sổ mục tiêu đã đóng hoặc không còn khả dụng. Hãy chọn lại.");region=wr.Pixels;}
            if(frozen!=null){
                // Repeat/legacy selection is constrained to the same retained frame before
                // saving or recording dimensions. Never re-capture the selector toolbar.
                region=CaptureSelection.Clip(region,frozen.Frame.Bounds);
                if(!CaptureSelection.Valid(region))throw new InvalidOperationException("Vùng chọn không còn nằm trong ảnh màn hình đã giữ. Hãy chọn lại vùng trên màn hình.");
            }
            cancellation.ThrowIfCancellationRequested();var final=_store.NewPath(video);var working=Path.Combine(_store.Root,Path.GetFileNameWithoutExtension(final)+".pending"+(video?".mp4":".png"));
            var profile=video?_videoProfile:null;
            var frame=profile?.Frame(region)??CaptureGeometry.RecordingFrame(region);
            long perMinute=profile?.BytesPerMinute(profile.Bitrate(frame),s.Mic,s.SystemAudio)??0;
            const long stopReserve=32L*1024*1024;
            if(video&&RecordingProfile.AvailableBytes(working) is long available&&available<perMinute+stopReserve)
                throw new IOException($"Không đủ dung lượng để bắt đầu quay. Cần khoảng {(perMinute+stopReserve)/1_048_576d:0} MB trống cho phút đầu và phần hoàn tất video.");
            if(!LicenseRuntime.Full&&(video||s.SourceMode!=0||action==PhotoAction.Repeat))throw new InvalidOperationException("Quyền sử dụng đã hết trong phiên chọn. Chỉ chụp vùng thủ công còn được mở.");
            if(!video&&window!=null&&LicenseRuntime.Full&&_workflow.HybridUi)liveHybrid=await HybridCaptureService.WindowAsync(region,(long)window.Handle,topology,_displays,cancellation);
            var photoStarted=Stopwatch.GetTimestamp();
            var clock=Stopwatch.StartNew();string actual;
            if(frozen!=null){await Task.Run(()=>frozen.Save(region,working),cancellation);actual=working;}
            else{
                if(video&&audioSettings is {} selectedAudio){
                    audio=new RecordingAudio();
                    audio.Lost+=(system,message)=>Dispatcher.BeginInvoke(()=>{
                        if(!_recordingActive||_stopping)return;
                        _bar?.ShowAudioWarning(message);
                        try{_engine?.DisableAudioSource(system);}catch(Exception error){App.Log(error);}
                    });
                    audio.Start(selectedAudio,working);
                }
                _engine=new(){AudioClock=audio?.Clock,AudioDevices=audioSettings};
                if(!video)_engine.Status+=status=>Dispatcher.BeginInvoke(()=>SetStatus("Capture engine · "+status));
                var req=new CaptureRequest(video,region,window,_displays,s.Cursor,s.Mic,s.SystemAudio,s.Fps,s.Hardware);
                req=req with{Profile=profile,HighlightClicks=video&&_highlightRecordingClicks};
                try{await _engine.StartAsync(req,working).WaitAsync(cancellation);}
                catch(Exception error) when(video&&error is not OperationCanceledException){
                    throw new InvalidOperationException($"Không khởi động được bộ ghi H.264 {(s.Hardware?"phần cứng":"phần mềm")} ở {frame.Width} × {frame.Height} px / {s.Fps} FPS. Kiểm tra bộ mã hóa hoặc thử giảm chất lượng trong Chọn nguồn quay. {error.Message}",error);
                }
                if(video){
                    _recordingActive=true;
                    ReleaseCaptureEscape();
                    var barDisplay=_displays.FirstOrDefault(d=>!region.Intersect(d.Bounds).IsEmpty)??display;
                    presenter=new RecordingPresenter(audio!.Clock,region,frame,()=>_bar?.ScreenBounds());
                    _bar=new(sourceName,()=>_engine.TogglePause(),()=>StopRecording(),barDisplay,audio,presenter);
                    markerHotkey=Native.RegisterHotKey(new WindowInteropHelper(this).Handle,MarkerHotkeyId,0x4006,0x35);
                    if(!markerHotkey)_bar.ShowAudioWarning("Ctrl+Shift+5 đang được sử dụng. Bấm Đánh dấu trên thanh quay.");
                    if(!presenter.Start())_bar.ShowAudioWarning("Windows chưa cho phép ghi mốc click chuột.");
                    diskWatch=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};
                    diskWatch.Tick+=(_,_)=>{
                        if(!_stopping&&RecordingProfile.AvailableBytes(working) is long free&&free<stopReserve)
                            StopRecording("Ổ đĩa sắp hết chỗ. Captovix đã dừng để hoàn tất phần đã ghi.");
                    };
                    diskWatch.Start();
                    _bar.Show();
                    if(audio!=null){
                        foreach(var warning in audio.Warnings)_bar.ShowAudioWarning(warning);
                        foreach(var lostSource in audio.LostSources)
                            try{_engine.DisableAudioSource(lostSource);}catch(Exception error){App.Log(error);}
                    }
                    if(!_bar.ExcludedFromCapture)SetStatus("Windows chưa xác nhận ẩn thanh điều khiển khỏi bản quay.");
                    if(window is {} target){
                        sourceWatch=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(250)};
                        sourceWatch.Tick+=(_,_)=>{
                            if(!Native.IsSameWindow(target)){
                                sourceWatch.Stop();
                                StopRecording("Cửa sổ nguồn đã đóng.");
                            }
                        };
                        sourceWatch.Start();
                    }
                }
                actual=await _engine.Completion.WaitAsync(video?TimeSpan.FromDays(1):TimeSpan.FromSeconds(20),video?CancellationToken.None:cancellation);
            }
            // Drop the retained desktop before decoding the new library item.
            frozen?.Dispose();
            clock.Stop();cancellation.ThrowIfCancellationRequested();
            if(!video)PerformanceTimings.Record("capture.photo.encode",clock.Elapsed.TotalMilliseconds);
            if(!File.Exists(actual)||new FileInfo(actual).Length==0)throw new IOException("Engine chưa tạo được file hoàn chỉnh.");
            RecordingMediaInfo? recording=null;
            if(video){
                if(window is {} target&&!Native.IsSameWindow(target))
                    _recordingInterruption??="Cửa sổ nguồn đã đóng.";
                _stopping=true;
                sourceWatch?.Stop();
                _bar?.BeginSaving();
                SetStatus("Đang kiểm tra file MP4…");
                recording=await RecordingMedia.VerifyAsync(actual,cancellation);
                if(audio!=null)await Task.Run(()=>audio.Complete(recording.Duration));
                double actualFps=await Task.Run(()=>RecordingFrameRate.Read(actual),cancellation);
                if(Math.Abs(actualFps-s.Fps)>0.6)
                    throw new IOException($"MP4 thực tế {actualFps:0.##} FPS, khác cấu hình {s.Fps} FPS. Tệp tạm được giữ để kiểm tra.");
                var expected=frame;
                if(recording.Width!=expected.Width||recording.Height!=expected.Height)
                    throw new IOException($"Khung MP4 {recording.Width} × {recording.Height} px khác khung đã chọn {expected.Width} × {expected.Height} px. Tệp tạm được giữ để kiểm tra.");
            }
            BitmapSource? capturedImage=null;
            if(!video){capturedImage=MediaStore.LoadImage(actual);ImageFidelity.ValidateCapture(region,capturedImage.PixelWidth,capturedImage.PixelHeight,window!=null);}
            if(!video)File.Move(actual,final);
            var item=new MediaItem{FilePath=final,Title=(video?"Video ":"Ảnh ")+DateTime.Now.ToString("dd-MM HH:mm:ss"),IsVideo=video,Width=region.Width,Height=region.Height,Source=window?.Title??(s.SourceMode==0?$"Vùng chọn · {region}":s.SourceMode==3?$"Phần tử · {region}":display.Label),Microphone=video&&s.Mic,SystemAudio=video&&s.SystemAudio,MicrophoneName=audioSettings?.Microphone?.Name,SystemAudioName=audioSettings?.System?.Name,Fps=video?s.Fps:0,Seconds=recording?.Duration.TotalSeconds??0};
            if(recording!=null){item.Width=recording.Width;item.Height=recording.Height;item.RecordingCues=presenter?.Finish(recording.Duration,actual);}
            if(capturedImage!=null){item.Width=capturedImage.PixelWidth;item.Height=capturedImage.PixelHeight;item.Capture=new(region.Width,region.Height,item.Width,item.Height,window!=null?"Window capture (native output bounds)":frozen!=null?$"Frozen GDI · {frozen.CapturedAt:O} · physical pixels · SDR8":"GDI BitBlt · physical pixels · SDR8");}
            if(LicenseRuntime.Full&&capturedImage!=null&&options.PhotoStyle.BorderEnabled){var framed=DocumentStore.Load(item,capturedImage,out _);framed.Change(d=>{d.Appearance.Padding=0;d.Appearance.BorderWidth=options.PhotoStyle.BorderWidth;d.Appearance.BorderColor=options.PhotoStyle.BorderColor;});DocumentStore.Save(item,framed,capturedImage);}
            if(capturedImage!=null){
                try{var evidence=frozen?.Hybrid??liveHybrid;var bound=frozen!=null?HybridCapture.CropRetained(evidence,region):evidence?.Nodes.Length>0?HybridCapture.Bind(evidence,region,ImageTransfer.Bgra(capturedImage),item.Width,item.Height,DateTimeOffset.UtcNow):null;
                    if(bound?.Nodes.Length>0){HybridCapture.Save(App.DataRoot,item.Id,final,bound);liveHybrid=bound;}else liveHybrid=null;
                }catch(Exception ex){App.Log(ex);liveHybrid=null;}
            }
            if(video)_store.CommitRecording(actual,item);
            else _store.Add(item);
            success=true;
            if(!video)PerformanceTimings.Record("capture.photo.saved",Stopwatch.GetElapsedTime(photoStarted).TotalMilliseconds);finalStatus=frozen!=null?$"Đã giữ lúc {frozen.CapturedAt.ToLocalTime():HH:mm:ss.fff} · PNG gốc {item.Width} × {item.Height} px":"● Đã lưu nội dung trên máy.";
            if(!video)finalStatus+=" · "+(liveHybrid?.Status??"Ảnh/OCR");
            else finalStatus=(_recordingInterruption==null?"":_recordingInterruption+" ")+$"Đã lưu video · {item.Width} × {item.Height} px · {recording!.Duration:hh\\:mm\\:ss}";
            if(video&&audio is {} savedAudio&&savedAudio.Warnings.Count>0)finalStatus+=" · "+string.Join(" ",savedAudio.Warnings);
            if(!video&&s.SourceMode==0){_lastRegion=region;var updated=_workflow.Clone();updated.LastRegion=new(region,topology);if(!_workflowReadOnly)try{SaveWorkflow(updated);}catch(Exception ex){App.Log(ex);finalStatus+=" Chưa lưu được vùng chụp lại.";}}
            if(LicenseRuntime.Full&&!video&&options.AutoCopy)try{var original=capturedImage!;var edits=DocumentStore.Load(item,original,out _);try{ImageTransfer.Copy(EditorRenderer.Render(original,edits.Document));}finally{EditorRenderer.ReleaseImageCaches(original,clearSharedRaster:false);}finalStatus+=" Đã copy ảnh.";}catch(Exception ex){App.Log(ex);finalStatus+=" Clipboard đang bận; ảnh đã được lưu, có thể copy trong Editor.";}
            if(showAfter){SearchBox.Clear();MediaList.SelectedItem=item;MediaList.ScrollIntoView(item);}
            else{_notificationItem=item.Id;_tray?.Notify(item.Title+" · "+finalStatus+" Nhấp thông báo để mở ảnh.");}
        }catch(OperationCanceledException){finalStatus=video?"Đã hủy quay.":"Đã hủy chụp.";}
        catch(Exception e){failed=true;App.Log(e);if(audio is {} failedAudio&&failedAudio.Warnings.Count>0)_recordingInterruption=string.Join(" ",failedAudio.Warnings);if(video&&window is {} target&&!Native.IsSameWindow(target))_recordingInterruption??="Cửa sổ nguồn đã đóng.";_bar?.Finish();_bar=null;Show();Activate();var hint=e is DllNotFoundException or BadImageFormatException?"\n\nKhông nạp được thư viện native. Chạy Install.cmd để kiểm tra Visual C++ Runtime.":"\n\nChi tiết được lưu tại %LOCALAPPDATA%\\HuyVo\\Captovix\\errors.log.";AppDialog.Show(this,$"Không thể {(video?"quay màn hình":"chụp ảnh")}.\n\n{(video&&_recordingInterruption!=null?_recordingInterruption+" ":"")}{e.Message}{hint}","Captovix",MessageBoxButton.OK,MessageBoxImage.Warning);}
        finally {
            frozen?.Dispose();
            sourceWatch?.Stop();diskWatch?.Stop();presenter?.Dispose();
            if(markerHotkey)Native.UnregisterHotKey(new WindowInteropHelper(this).Handle,MarkerHotkeyId);
            try{audio?.Dispose();}catch(Exception error){App.Log(error);}
            try{if(!success&&!_stopping)_engine?.Stop();_engine?.Dispose();}catch(Exception e){App.Log(e);}_engine=null;_bar?.Finish();_bar=null;_stopping=false;_recordingActive=false;
            ReleaseCaptureEscape();_captureCancellation?.Dispose();_captureCancellation=null;
            if((success&&showAfter)||failed||(!success&&wasVisible)||_tray?.Ready!=true){Show();WindowState=previousWindowState==WindowState.Minimized?WindowState.Maximized:previousWindowState;Activate();}
            SetBusy(false);SetStatus(finalStatus??"● Sẵn sàng · Phiên đã hủy hoặc chưa hoàn thành.");_captureGate.Exit();
            if(_exitRequested)_=Dispatcher.BeginInvoke(RequestExit);else if(scrollNext is {} next)_=Dispatcher.BeginInvoke(()=>_=ScrollCaptureAsync(next,scrollHorizontal));
        }
    }
    private string? _recordingInterruption;
    private void StopRecording(string? interruption=null)
    {
        if(_engine==null||!_recordingActive||_stopping)return;
        _stopping=true;
        _recordingInterruption=interruption;
        _bar?.BeginSaving();
        SetStatus((interruption==null?"":interruption+" ")+"Đang hoàn tất file MP4…");
        _engine.Stop();
    }
    private void SetBusy(bool busy){_busy=busy;CaptureButton.IsEnabled=RecordButton.IsEnabled=ConfigPanel.IsEnabled=WorkflowPanel.IsEnabled=!busy;CancelCaptureButton.IsEnabled=busy;RenderUpdateUi();}
    private void SetStatus(string text)=>StatusText.Text=text;
    private void Capture_Click(object sender,RoutedEventArgs e)=>QueueCapture(false);
    private void Record_Click(object sender,RoutedEventArgs e){QueueCapture(true);}
    private void Refresh_Click(object sender,RoutedEventArgs e){if(!RequireLicense())return;RefreshSources();}
    private void SourceMode_Changed(object sender,SelectionChangedEventArgs e){if(DisplayPicker==null||WindowPicker==null)return;bool window=SourceMode.SelectedIndex==1;DisplayPicker.Visibility=window?Visibility.Collapsed:Visibility.Visible;WindowPicker.Visibility=window?Visibility.Visible:Visibility.Collapsed;}
    private void Search_Changed(object sender,TextChangedEventArgs e){if(_store!=null)CollectionViewSource.GetDefaultView(_recentItems).Refresh();if(LibrarySearchButton!=null)LibrarySearchButton.SetResourceReference(Control.ForegroundProperty,string.IsNullOrWhiteSpace(SearchBox.Text)?"Text":"AccentText");}
    private long _selectionRequest;
    private async void Media_Changed(object sender,SelectionChangedEventArgs e)
    {
        if (_changingSelection || !ReferenceEquals(e.OriginalSource, MediaList)) return;
        if(_videoExportCancellation!=null&&_videoItem!=null){
            _changingSelection=true;
            try{MediaList.SelectedItems.Clear();MediaList.SelectedItem=_videoItem;}
            finally{_changingSelection=false;}
            return;
        }
        var request = ++_selectionRequest;
        _imageLoadCancellation?.Cancel();
        if(MediaList.SelectedItems.Count>1){if(_playing){VideoPreview.Pause();SetVideoPlaying(false);}RefreshVideoControls();SetStatus($"Đã chọn {MediaList.SelectedItems.Count} nội dung · Delete để xóa / Xuất nhiều.");return;}
        var item = Selected;
        if (item != null && ReferenceEquals(item, _activeItem) && _session != null) return;
        Surface.CancelInteraction();
        bool IsCurrent() => request == _selectionRequest && ReferenceEquals(Selected, item) && MediaList.SelectedItems.Count <= 1 && !_updateUiClosed;
        if (!await SaveEditorAsync(IsCurrent))
        {
            if (IsCurrent()) {
                _changingSelection=true;
                try { MediaList.SelectedItem=_activeItem; }
                finally { _changingSelection=false; }
            }
            return;
        }
        if (!IsCurrent()) return;
        if(!FlushVideoProject()){
            _changingSelection=true;
            try{MediaList.SelectedItem=_videoItem;}
            finally{_changingSelection=false;}
            return;
        }
        DetachVideoProject();
        UnloadEditor();
        ImageScroll.Visibility=VideoHost.Visibility=ImageTools.Visibility=VideoTools.Visibility=ZoomTools.Visibility=EditorProperties.Visibility=Visibility.Collapsed;
        EmptyState.Visibility=item==null?Visibility.Visible:Visibility.Collapsed;
        CopyButton.IsEnabled=item is {IsVideo:false};ExportButton.IsEnabled=RenameButton.IsEnabled=DeleteButton.IsEnabled=item!=null;
        DocumentTitle.Text=item?.Title??"Không gian làm việc";SavedLabel.Text=item==null?"":"✓ Đã lưu";
        if(item==null){Metadata.Text="Chọn ảnh hoặc video.";return;}
        UpdateMetadata(item);
        try {
            if(item.IsVideo){
                OpenLibraryVideo(item);
                if(_openingVideoProject is {} opening&&opening.Id==item.Id){
                    _videoProjectPath=opening.Path;
                    _openingVideoProject=null;
                }
            }else if(File.Exists(item.FilePath))await OpenImageAsync(item);
            else{
                SetStatus("File đã được di chuyển hoặc xóa bên ngoài app.");
                CopyButton.IsEnabled=ExportButton.IsEnabled=false;
            }
        }catch(Exception ex){App.Log(ex);SetStatus("Không mở được file: "+ex.Message);CopyButton.IsEnabled=ExportButton.IsEnabled=false;AppDialog.Show(this,ex.Message,"Không mở được nội dung");}
    }
    private void UpdateMetadata(MediaItem item)=>Metadata.Text=$"{(item.IsVideo?"VIDEO MP4 / H.264":"ẢNH GỐC PNG")}\n{item.Width} × {item.Height} pixel\n{item.Created:dd/MM/yyyy HH:mm:ss}\n\nNguồn\n{item.Source}\n\n{(item.IsVideo?$"Mic: {(item.Microphone?item.MicrophoneName??"Bật":"Tắt")}\nTiếng máy: {(item.SystemAudio?item.SystemAudioName??"Bật":"Tắt")}\nFPS cấu hình: {item.Fps}\n":"")}\n{item.FilePath}";
    private void Copy_Click(object sender,RoutedEventArgs e)
    {if(!RequireLicense())return;
        if(MediaList.IsKeyboardFocusWithin||MediaList.SelectedItems.Count>1){CopyLibrarySelection();return;}
        if(_session==null||!PrepareCropForExport())return;_session.Commit();
        try{ImageTransfer.Copy(Surface.Flatten());FlushEdits(false);SetStatus($"Đã copy {Surface.Layout!.Width} × {Surface.Layout.Height} px · PNG + Bitmap gồm chú thích/crop. App nhận quyết định cách dán."+PrivacyExportStatus());}
        catch(Exception ex){App.Log(ex);AppDialog.Show(this,ex.Message,"Không copy được ảnh");}
    }
    private bool _exportInProgress, _closeAfterExport;
    private async void Export_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense() || _exportInProgress || _busy || Selected is not {} item)return;
        if(item.IsVideo){await ExportVideoProjectAsync();return;}
        if(!ReferenceEquals(item,_activeItem)||_session==null)return;
        var filename=string.Concat(item.Title.Select(c=>Path.GetInvalidFileNameChars().Contains(c)?'_':c));
        var save=new SaveFileDialog{Title="Xuất theo kích thước đầu ra · PNG lossless / JPEG 95% có mất mát",FileName=filename,Filter="PNG image|*.png|JPEG image|*.jpg",AddExtension=true};
        if(save.ShowDialog(this)!=true || !ReferenceEquals(Selected,item))return;

        try
        {
            var destination=Path.GetFullPath(save.FileName);
            if(_store.Items.Any(entry=>string.Equals(Path.GetFullPath(entry.FilePath),destination,StringComparison.OrdinalIgnoreCase)))
            {
                AppDialog.Show(this,"Hãy chọn tên hoặc thư mục xuất khác để giữ nguyên file gốc trong thư viện.","Giữ ảnh/video gốc");
                return;
            }
            if(!ReferenceEquals(item,_activeItem)||_session==null||!PrepareCropForExport()||!PrepareEditorSave())return;
            var source=Surface.Original;
            if(source==null)return;
            var snapshot=_session.Document.Snapshot();
            if(_session.Dirty)_saveTimer.Start();

            _exportInProgress=true;
            using var measurement=PerformanceTimings.Measure("editor.export");
            SetStatus("Đang xuất "+item.Title+" · vẫn có thể chỉnh sửa ảnh.");
            var jpeg=save.FilterIndex==2;
            await StaImageWorker.RunExport(()=>
            {
                try { ImageTransfer.Save(EditorRenderer.Render(source,snapshot),destination,jpeg); }
                finally { EditorRenderer.ReleaseImageCaches(source,clearSharedRaster:false); }
            });
            var privacy=snapshot?.Annotations.Any(a=>a.IsRedaction)==true?" · "+FocusStudio.Review(snapshot):"";
            SetStatus("Đã xuất file: "+destination+privacy);
        }
        catch(Exception error)
        {
            App.Log(error);
            AppDialog.Show(this,error.Message,"Không xuất được file");
        }
        finally
        {
            _exportInProgress=false;
            if(_closeAfterExport){_closeAfterExport=false;_=Dispatcher.BeginInvoke(Close);}
        }
    }
    private void Import_Click(object sender,RoutedEventArgs e)
    {if(!RequireLicense())return;
        var open=new OpenFileDialog{Filter="Ảnh và video|*.png;*.jpg;*.jpeg;*.bmp;*.mp4",Multiselect=true};if(open.ShowDialog(this)!=true)return;
        foreach(var path in open.FileNames)try{bool video=Path.GetExtension(path).Equals(".mp4",StringComparison.OrdinalIgnoreCase);var dest=_store.NewPath(video);var i=new MediaItem{Title=Path.GetFileNameWithoutExtension(path),FilePath=dest,IsVideo=video,Source="File được nhập"};
            if(video)File.Copy(path,dest);else{var b=MediaStore.LoadImage(path);i.Width=b.PixelWidth;i.Height=b.PixelHeight;var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(b));using var stream=File.Create(dest);enc.Save(stream);}
            _store.Add(i);SearchBox.Clear();MediaList.SelectedItem=i;
        }catch(Exception ex){App.Log(ex);AppDialog.Show(this,ex.Message,"Không nhập được file");}
    }
    private void Rename_Click(object sender,RoutedEventArgs e){if(!RequireLicense())return;if(Selected==null)return;RenameBox.Text=Selected.Title;RenameBox.Visibility=Visibility.Visible;RenameBox.Focus();RenameBox.SelectAll();}
    private void Rename_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Escape)RenameBox.Visibility=Visibility.Collapsed;if(e.Key==Key.Enter&&Selected is { } i&&!string.IsNullOrWhiteSpace(RenameBox.Text)){i.Title=RenameBox.Text.Trim();_store.Save();MediaList.Items.Refresh();DocumentTitle.Text=i.Title;RenameBox.Visibility=Visibility.Collapsed;}}
    private TrashReceipt? _lastDeleted;
    private void Delete_Click(object sender,RoutedEventArgs e){if(!RequireLicense())return;DeleteMediaBatch(MediaList.SelectedItems.Cast<MediaItem>().ToArray());}
    private void DeleteMediaTile_Click(object sender,RoutedEventArgs e){if(!RequireLicense())return;if(sender is Button {Tag:MediaItem item})DeleteMediaBatch(MediaList.SelectedItems.Contains(item)?MediaList.SelectedItems.Cast<MediaItem>().ToArray():[item]);e.Handled=true;}
    private void DeleteMedia(MediaItem item)=>DeleteMediaBatch([item]);
    private static void OpenFolder(string path)=>Process.Start(new ProcessStartInfo(path){UseShellExecute=true});
    private void Folder_Click(object sender,RoutedEventArgs e){if(!RequireLicense())return;OpenFolder(_store.Root);}
    private void Diagnostics_Click(object sender,RoutedEventArgs e)
    {if(!RequireLicense())return;
        RefreshSources();var file=Path.Combine(App.DataRoot,$"diagnostics-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        var data=new{App="Captovix "+VersionInfo.Current,Date=DateTimeOffset.Now,OS=RuntimeInformation.OSDescription,Runtime=RuntimeInformation.FrameworkDescription,Architecture=RuntimeInformation.ProcessArchitecture.ToString(),DpiMode="PerMonitorV2",Displays=_displays,LastRegion=_lastRegion,SelectedCapture=Selected?.Capture,Hotkeys=_hotkeys,PendingFiles=Directory.GetFiles(_store.Root,"*.pending.*"),RuntimeVerification="Chưa tự động kết luận. Ghi kết quả máy thật theo docs/WINDOWS-TEST-CHECKLIST.md."};
        File.WriteAllText(file,JsonSerializer.Serialize(data,new JsonSerializerOptions{WriteIndented=true}));Process.Start(new ProcessStartInfo("notepad.exe",$"\"{file}\""){UseShellExecute=true});SetStatus("Đã tạo báo cáo máy và tọa độ màn hình.");
    }
    private Process? _fidelityProcess;
    private async void FidelityCheck_Click(object sender,RoutedEventArgs e)
    {if(!RequireLicense())return;
        if(_busy||_updateApplying)return;Surface.CancelInteraction();if(!FlushEdits())return;SetBusy(true);SetStatus("Đang chạy kiểm chứng SDR; đóng cửa sổ kiểm chứng hoặc Hủy để kết thúc.");
        try{
            _fidelityProcess=Process.Start(new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false,ArgumentList={"--fidelity-check"}})??throw new InvalidOperationException("Không mở được tiến trình kiểm chứng.");
            await _fidelityProcess.WaitForExitAsync();SetStatus("Đã kết thúc tiến trình kiểm chứng. Xem report trong FidelityChecks nếu đã chạy phép thử.");
        }catch(Exception ex){App.Log(ex);AppDialog.Show(this,ex.Message,"Chưa chạy được kiểm chứng SDR");}
        finally{_fidelityProcess?.Dispose();_fidelityProcess=null;SetBusy(false);if(_exitRequested)_=Dispatcher.BeginInvoke(RequestExit);}
    }
    private void TestGuide_Click(object sender,RoutedEventArgs e){if(!RequireLicense())return;var p=Path.Combine(AppContext.BaseDirectory,"docs","WINDOWS-TEST-CHECKLIST.md");if(File.Exists(p))Process.Start(new ProcessStartInfo("notepad.exe",$"\"{p}\""){UseShellExecute=true});else AppDialog.Show(this,"Xem docs/WINDOWS-TEST-CHECKLIST.md trong gói source. Kiểm tra chụp vùng/cửa sổ/toàn màn hình, scale 100–200%, màn hình phụ tọa độ âm và quay mic + tiếng máy.","Kiểm thử Captovix V"+VersionInfo.Current);}
    private void Window_Closing(object? sender,CancelEventArgs e)
    {
        if(_exportInProgress){e.Cancel=true;_closeAfterExport=true;SetStatus("Đang hoàn tất tệp xuất trước khi đóng Captovix…");return;}
        if(_busy){e.Cancel=true;_exitRequested=true;_updatePreparationCancel?.Cancel();CancelCapture();SetStatus("Đang hoàn tất phiên và thoát Captovix…");return;}
        Surface.CancelInteraction();if(!FlushEdits()||!FlushVideoProject()){e.Cancel=true;_exitRequested=false;return;}
        try{if(_loaded){_store.SaveSettings(Settings());_store.Save();}}
        catch(Exception ex){App.Log(ex);e.Cancel=true;AppDialog.Show(this,"Chưa lưu được cấu hình/thư viện. Hãy kiểm tra thư mục dữ liệu trước khi đóng.\n"+ex.Message,"Captovix");return;}
        if(!_exitRequested&&!_updateApplying&&_workflow.CloseToTray&&_tray?.Ready==true){e.Cancel=true;Hide();_tray.Notify("Captovix đang chạy nền. Nhấp biểu tượng để mở Editor; chọn Thoát để đóng hoàn toàn.");return;}
        _selectionRequest++;_imageLoadGeneration++;_imageLoadCancellation?.Cancel();UnloadEditor();SuspendWorkflow();
        _updateUiClosed=true;_updateCheckTimer.Stop();_updateCancellation?.Cancel();_saveTimer.Stop();_videoTimer.Stop();ClearVideoAudio();VideoPreview.Close();var hwnd=new WindowInteropHelper(this).Handle;foreach(var i in _hotkeys)Native.UnregisterHotKey(hwnd,i);
    }
}
