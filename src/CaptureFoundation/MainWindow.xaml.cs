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

namespace CaptureFoundation;
public partial class MainWindow : Window
{
    private readonly MediaStore _store;
    private readonly DispatcherTimer _videoTimer=new(){Interval=TimeSpan.FromMilliseconds(300)};
    private readonly List<int> _hotkeys=[];
    private List<DisplayInfo> _displays=[];
    private CaptureEngine? _engine;
    private RecordingBar? _bar;
    private bool _busy,_stopping,_playing,_loaded;
    private PixelRect? _lastRegion;
    private MediaItem? Selected => MediaList.SelectedItem as MediaItem;
    public MainWindow()
    {
        InitializeComponent();_store=new();MediaList.ItemsSource=_store.Items;
        CollectionViewSource.GetDefaultView(_store.Items).Filter=o=>{var i=(MediaItem)o;var q=SearchBox.Text??"";return $"{i.Title} {(i.IsVideo?"video":"ảnh")} {i.Created:dd/MM/yyyy}".Contains(q,StringComparison.CurrentCultureIgnoreCase);};
        _store.Items.CollectionChanged+=(_,_)=>CountLabel.Text=_store.Items.Count.ToString();CountLabel.Text=_store.Items.Count.ToString();
        Loaded+=(_,_)=>Initialize();Closing+=Window_Closing;
        _videoTimer.Tick+=(_,_)=>{if(VideoPreview.Visibility==Visibility.Visible&&VideoPreview.NaturalDuration.HasTimeSpan){if(Mouse.LeftButton!=MouseButtonState.Pressed)VideoSeek.Value=VideoPreview.Position.TotalSeconds;VideoTime.Text=$"{VideoPreview.Position:hh\\:mm\\:ss} / {VideoPreview.NaturalDuration.TimeSpan:hh\\:mm\\:ss}";}};
        _videoTimer.Start();
    }
    private void Initialize()
    {
        RefreshSources();var s=_store.LoadSettings();SourceMode.SelectedIndex=Math.Clamp(s.SourceMode,0,2);
        DelayPicker.SelectedIndex=s.Delay==5?2:s.Delay==3?1:0;FpsPicker.SelectedIndex=s.Fps==60?2:s.Fps==15?0:1;
        CursorToggle.IsChecked=s.Cursor;MicToggle.IsChecked=s.Mic;SystemToggle.IsChecked=s.SystemAudio;HardwareToggle.IsChecked=s.Hardware;
        DisplayPicker.SelectedItem=_displays.FirstOrDefault(d=>d.Device==s.Display)??_displays.FirstOrDefault();
        _loaded=true;var hwnd=new WindowInteropHelper(this).Handle;HwndSource.FromHwnd(hwnd)?.AddHook(WindowMessage);
        for(int i=1;i<=4;i++)if(Native.RegisterHotKey(hwnd,i,0x4006,(uint)(0x30+i)))_hotkeys.Add(i);
        if(_hotkeys.Count<4)SetStatus("Một số phím tắt đang bị ứng dụng khác sử dụng. Các nút trong Editor vẫn hoạt động.");
    }
    private void RefreshSources()
    {
        var device=(DisplayPicker.SelectedItem as DisplayInfo)?.Device;var hwnd=(WindowPicker.SelectedItem as WindowInfo)?.Handle;
        _displays=Native.Displays();DisplayPicker.ItemsSource=_displays;DisplayPicker.SelectedItem=_displays.FirstOrDefault(d=>d.Device==device)??_displays.FirstOrDefault();
        var windows=Native.Windows();WindowPicker.ItemsSource=windows;WindowPicker.SelectedItem=windows.FirstOrDefault(w=>w.Handle==hwnd)??windows.FirstOrDefault();
        DisplaySummary.Text=string.Join("\n\n",_displays.Select(d=>$"{d.Label}\nGốc pixel: {d.Bounds.X}, {d.Bounds.Y}"));
    }
    private nint WindowMessage(nint hwnd,int msg,nint w,nint l,ref bool handled)
    {
        if(msg==0x312){handled=true;switch((int)w){case 1:if(!_busy)_=CaptureAsync(false);break;case 2:if(!_busy)_=CaptureAsync(true);break;case 3:StopRecording();break;case 4:_bar?.PauseFromHotkey();break;}}
        if(msg==0x7E&&!_busy)Dispatcher.BeginInvoke(RefreshSources);
        return 0;
    }
    private AppSettings Settings()=>new(){SourceMode=SourceMode.SelectedIndex,Delay=int.Parse(((ComboBoxItem)DelayPicker.SelectedItem).Tag.ToString()!),Fps=int.Parse(((ComboBoxItem)FpsPicker.SelectedItem).Tag.ToString()!),Cursor=CursorToggle.IsChecked==true,Mic=MicToggle.IsChecked==true,SystemAudio=SystemToggle.IsChecked==true,Hardware=HardwareToggle.IsChecked==true,Display=(DisplayPicker.SelectedItem as DisplayInfo)?.Device??""};
    private async Task CaptureAsync(bool video)
    {
        if(_busy)return;SetBusy(true);VideoPreview.Pause();_playing=false;
        string? working=null;bool success=false;
        try {
            var s=Settings();_store.SaveSettings(s);RefreshSources();
            var display=DisplayPicker.SelectedItem as DisplayInfo??throw new InvalidOperationException("Không tìm thấy màn hình.");
            var window=s.SourceMode==1?WindowPicker.SelectedItem as WindowInfo:null;
            if(s.SourceMode==1&&window==null)throw new InvalidOperationException("Hãy mở và chọn một cửa sổ trong danh sách nguồn.");
            PixelRect region=display.Bounds;
            Hide();await Task.Delay(220);
            if(s.SourceMode==0){var chosen=await RegionPicker.PickAsync(_displays);if(chosen==null)return;region=chosen.Value;_lastRegion=region;}
            else if(window!=null){if(!Native.GetWindowRect(window.Handle,out var wr))throw new InvalidOperationException("Không đọc được kích thước cửa sổ.");region=wr.Pixels;}
            if(video)region=region.Even();
            if(region.IsEmpty)throw new InvalidOperationException("Vùng chọn quá nhỏ.");
            if(s.Delay>0){Show();for(var n=s.Delay;n>0;n--){SetStatus($"Bắt đầu sau {n} giây…");await Task.Delay(1000);}Hide();await Task.Delay(220);}
            var final=_store.NewPath(video);working=Path.Combine(_store.Root,Path.GetFileNameWithoutExtension(final)+".pending"+(video?".mp4":".png"));
            // Pending files are never inserted into history until the engine reports completion.
            _engine=new();_engine.Status+=status=>Dispatcher.BeginInvoke(()=>SetStatus("Capture engine · "+status));
            var req=new CaptureRequest(video,region,window,_displays,s.Cursor,s.Mic,s.SystemAudio,s.Fps,s.Hardware);
            await _engine.StartAsync(req,working);
            var clock=Stopwatch.StartNew();
            if(video){
                _bar=new(()=>_engine?.TogglePause(),StopRecording,display);_bar.Show();
                if(!_bar.ExcludedFromCapture)SetStatus("Windows chưa xác nhận ẩn thanh điều khiển khỏi bản quay. Có thể kéo thanh ra ngoài vùng quay.");
            }
            var actual=await _engine.Completion.WaitAsync(video?TimeSpan.FromDays(1):TimeSpan.FromSeconds(20));clock.Stop();
            if(!File.Exists(actual)||new FileInfo(actual).Length==0)throw new IOException("Engine chưa tạo được file hoàn chỉnh.");
            File.Move(actual,final);
            var item=new MediaItem{FilePath=final,Title=(video?"Video ":"Ảnh ")+DateTime.Now.ToString("dd-MM HH:mm:ss"),IsVideo=video,Width=region.Width,Height=region.Height,Source=window?.Title??(s.SourceMode==0?$"Vùng chọn · {region}":display.Label),Microphone=video&&s.Mic,SystemAudio=video&&s.SystemAudio,Fps=video?s.Fps:0,Seconds=video?clock.Elapsed.TotalSeconds:0};
            if(!video){var image=MediaStore.LoadImage(final);item.Width=image.PixelWidth;item.Height=image.PixelHeight;}
            _store.Add(item);SearchBox.Clear();MediaList.SelectedItem=item;MediaList.ScrollIntoView(item);success=true;
        }catch(Exception e){App.Log(e);Show();Activate();MessageBox.Show(this,$"Không thể {(video?"quay màn hình":"chụp ảnh")}.\n\n{e.Message}\n\nNếu lỗi engine/DLL: chạy Install.cmd để cài Visual C++ Runtime. Với Windows N/KN, cài Media Feature Pack. Có thể tắt Mã hóa GPU rồi thử lại.","Capture Foundation",MessageBoxButton.OK,MessageBoxImage.Warning);}
        finally {
            try{_engine?.Stop();_engine?.Dispose();}catch(Exception e){App.Log(e);}_engine=null;_bar?.Finish();_bar=null;_stopping=false;
            // Retain failed/pending recordings for diagnosis, never silently report them as successful.
            Show();WindowState=WindowState.Normal;Activate();SetBusy(false);SetStatus(success?"● Đã lưu nội dung trên máy.":"● Sẵn sàng · Phiên đã hủy hoặc chưa hoàn thành.");
        }
    }
    private void StopRecording(){if(_engine==null||_stopping)return;_stopping=true;SetStatus("Đang hoàn tất file MP4…");_engine.Stop();}
    private void SetBusy(bool busy){_busy=busy;CaptureButton.IsEnabled=RecordButton.IsEnabled=ConfigPanel.IsEnabled=!busy;}
    private void SetStatus(string text)=>StatusText.Text=text;
    private void Capture_Click(object sender,RoutedEventArgs e)=>_=CaptureAsync(false);
    private void Record_Click(object sender,RoutedEventArgs e)=>_=CaptureAsync(true);
    private void Refresh_Click(object sender,RoutedEventArgs e)=>RefreshSources();
    private void SourceMode_Changed(object sender,SelectionChangedEventArgs e){if(DisplayPicker==null||WindowPicker==null)return;bool window=SourceMode.SelectedIndex==1;DisplayPicker.Visibility=window?Visibility.Collapsed:Visibility.Visible;WindowPicker.Visibility=window?Visibility.Visible:Visibility.Collapsed;}
    private void Search_Changed(object sender,TextChangedEventArgs e){if(_store!=null)CollectionViewSource.GetDefaultView(_store.Items).Refresh();}
    private void Media_Changed(object sender,SelectionChangedEventArgs e)
    {
        VideoPreview.Stop();VideoPreview.Source=null;_playing=false;ImagePreview.Source=null;
        ImageScroll.Visibility=VideoPreview.Visibility=ImageTools.Visibility=VideoTools.Visibility=Visibility.Collapsed;
        var item=Selected;EmptyState.Visibility=item==null?Visibility.Visible:Visibility.Collapsed;
        CopyButton.IsEnabled=item is {IsVideo:false};ExportButton.IsEnabled=RenameButton.IsEnabled=DeleteButton.IsEnabled=item!=null;
        if(item==null){DocumentTitle.Text="Không gian làm việc";Metadata.Text="Chọn ảnh hoặc video.";SavedLabel.Text="";return;}
        DocumentTitle.Text=item.Title;SavedLabel.Text="✓ Đã lưu";
        Metadata.Text=$"{(item.IsVideo?"VIDEO MP4 / H.264":"ẢNH PNG")}\n{item.Width} × {item.Height} pixel\n{item.Created:dd/MM/yyyy HH:mm:ss}\n\nNguồn\n{item.Source}\n\n{(item.IsVideo?$"Mic: {(item.Microphone?"Bật":"Tắt")}\nTiếng máy: {(item.SystemAudio?"Bật":"Tắt")}\nFPS cấu hình: {item.Fps}\n":"")}\n{item.FilePath}";
        if(!File.Exists(item.FilePath)){SetStatus("File đã được di chuyển hoặc xóa bên ngoài app.");return;}
        try {
            if(item.IsVideo){VideoPreview.Visibility=VideoTools.Visibility=Visibility.Visible;VideoPreview.Source=new Uri(item.FilePath);VideoPreview.Play();VideoPreview.Pause();}
            else{ImagePreview.Source=MediaStore.LoadImage(item.FilePath);ImageScroll.Visibility=ImageTools.Visibility=Visibility.Visible;FitImage();}
        }catch(Exception ex){App.Log(ex);SetStatus("Không mở được file: "+ex.Message);}
    }
    private void FitImage(){ImagePreview.Width=double.NaN;ImagePreview.Height=double.NaN;ImagePreview.MaxWidth=Math.Max(200,ImageScroll.ActualWidth-38);ImagePreview.MaxHeight=Math.Max(150,ImageScroll.ActualHeight-38);}
    private void Fit_Click(object sender,RoutedEventArgs e)=>FitImage();
    private void Actual_Click(object sender,RoutedEventArgs e){if(ImagePreview.Source is not BitmapSource b)return;var dpi=VisualTreeHelper.GetDpi(this);ImagePreview.MaxWidth=ImagePreview.MaxHeight=double.PositiveInfinity;ImagePreview.Width=b.PixelWidth/dpi.DpiScaleX;ImagePreview.Height=b.PixelHeight/dpi.DpiScaleY;}
    private void Copy_Click(object sender,RoutedEventArgs e){if(ImagePreview.Source is BitmapSource b){Clipboard.SetImage(b);SetStatus("Đã copy ảnh vào clipboard.");}}
    private void Export_Click(object sender,RoutedEventArgs e)
    {
        if(Selected is not { } i)return;var save=new SaveFileDialog{FileName=i.Title.Replace(":","-"),Filter=i.IsVideo?"MP4 video|*.mp4":"PNG image|*.png|JPEG image|*.jpg",AddExtension=true};
        if(save.ShowDialog(this)!=true)return;
        try{if(string.Equals(Path.GetFullPath(save.FileName),Path.GetFullPath(i.FilePath),StringComparison.OrdinalIgnoreCase))return;
            if(!i.IsVideo&&save.FilterIndex==2){var encoder=new JpegBitmapEncoder{QualityLevel=95};encoder.Frames.Add(BitmapFrame.Create(MediaStore.LoadImage(i.FilePath)));using var f=File.Create(save.FileName);encoder.Save(f);}else File.Copy(i.FilePath,save.FileName,true);SetStatus("Đã xuất file: "+save.FileName);
        }catch(Exception ex){App.Log(ex);MessageBox.Show(this,ex.Message,"Không xuất được file");}
    }
    private void Import_Click(object sender,RoutedEventArgs e)
    {
        var open=new OpenFileDialog{Filter="Ảnh và video|*.png;*.jpg;*.jpeg;*.bmp;*.mp4",Multiselect=true};if(open.ShowDialog(this)!=true)return;
        foreach(var path in open.FileNames)try{bool video=Path.GetExtension(path).Equals(".mp4",StringComparison.OrdinalIgnoreCase);var dest=_store.NewPath(video);var i=new MediaItem{Title=Path.GetFileNameWithoutExtension(path),FilePath=dest,IsVideo=video,Source="File được nhập"};
            if(video)File.Copy(path,dest);else{var b=MediaStore.LoadImage(path);i.Width=b.PixelWidth;i.Height=b.PixelHeight;var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(b));using var stream=File.Create(dest);enc.Save(stream);}
            _store.Add(i);SearchBox.Clear();MediaList.SelectedItem=i;
        }catch(Exception ex){App.Log(ex);MessageBox.Show(this,ex.Message,"Không nhập được file");}
    }
    private void Rename_Click(object sender,RoutedEventArgs e){if(Selected==null)return;RenameBox.Text=Selected.Title;RenameBox.Visibility=Visibility.Visible;RenameBox.Focus();RenameBox.SelectAll();}
    private void Rename_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Escape)RenameBox.Visibility=Visibility.Collapsed;if(e.Key==Key.Enter&&Selected is { } i&&!string.IsNullOrWhiteSpace(RenameBox.Text)){i.Title=RenameBox.Text.Trim();_store.Save();MediaList.Items.Refresh();DocumentTitle.Text=i.Title;RenameBox.Visibility=Visibility.Collapsed;}}
    private void Delete_Click(object sender,RoutedEventArgs e){if(Selected is not { } i)return;if(MessageBox.Show(this,$"Xóa '{i.Title}' và file khỏi máy?","Xóa nội dung",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;VideoPreview.Stop();VideoPreview.Source=null;try{File.Delete(i.FilePath);_store.Items.Remove(i);_store.Save();}catch(Exception ex){MessageBox.Show(this,ex.Message,"Không xóa được file");}}
    private static void OpenFolder(string path)=>Process.Start(new ProcessStartInfo(path){UseShellExecute=true});
    private void Folder_Click(object sender,RoutedEventArgs e)=>OpenFolder(_store.Root);
    private void Video_Opened(object sender,RoutedEventArgs e){if(VideoPreview.NaturalDuration.HasTimeSpan){VideoSeek.Maximum=VideoPreview.NaturalDuration.TimeSpan.TotalSeconds;if(Selected is {IsVideo:true} i){i.Width=VideoPreview.NaturalVideoWidth;i.Height=VideoPreview.NaturalVideoHeight;i.Seconds=VideoSeek.Maximum;_store.Save();}}}
    private void Video_Ended(object sender,RoutedEventArgs e){VideoPreview.Pause();VideoPreview.Position=TimeSpan.Zero;_playing=false;}
    private void Video_Failed(object sender,ExceptionRoutedEventArgs e)=>SetStatus("Không phát được video: "+e.ErrorException.Message);
    private void Play_Click(object sender,RoutedEventArgs e){VideoPreview.Play();_playing=true;}
    private void Pause_Click(object sender,RoutedEventArgs e){VideoPreview.Pause();_playing=false;}
    private void Seek_Released(object sender,MouseButtonEventArgs e){VideoPreview.Position=TimeSpan.FromSeconds(VideoSeek.Value);if(!_playing){VideoPreview.Play();VideoPreview.Pause();}}
    private void Diagnostics_Click(object sender,RoutedEventArgs e)
    {
        RefreshSources();var file=Path.Combine(App.DataRoot,$"diagnostics-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        var data=new{App="Capture Foundation 0.1.0",Date=DateTimeOffset.Now,OS=RuntimeInformation.OSDescription,Runtime=RuntimeInformation.FrameworkDescription,Architecture=RuntimeInformation.ProcessArchitecture.ToString(),DpiMode="PerMonitorV2",Displays=_displays,LastRegion=_lastRegion,Hotkeys=_hotkeys,PendingFiles=Directory.GetFiles(_store.Root,"*.pending.*"),RuntimeVerification="Chưa tự động kết luận. Ghi kết quả máy thật theo docs/WINDOWS-TEST-CHECKLIST.md."};
        File.WriteAllText(file,JsonSerializer.Serialize(data,new JsonSerializerOptions{WriteIndented=true}));Process.Start(new ProcessStartInfo("notepad.exe",$"\"{file}\""){UseShellExecute=true});SetStatus("Đã tạo báo cáo máy và tọa độ màn hình.");
    }
    private void TestGuide_Click(object sender,RoutedEventArgs e){var p=Path.Combine(AppContext.BaseDirectory,"docs","WINDOWS-TEST-CHECKLIST.md");if(File.Exists(p))Process.Start(new ProcessStartInfo("notepad.exe",$"\"{p}\""){UseShellExecute=true});else MessageBox.Show(this,"Xem docs/WINDOWS-TEST-CHECKLIST.md trong gói source. Kiểm tra chụp vùng/cửa sổ/toàn màn hình, scale 100–200%, màn hình phụ tọa độ âm và quay mic + tiếng máy.","Kiểm thử V0.1");}
    private void Window_Closing(object? sender,CancelEventArgs e)
    {
        if(_busy){e.Cancel=true;StopRecording();SetStatus("Đang kết thúc phiên capture. Hãy đóng app sau khi file được lưu.");return;}
        if(_loaded)_store.SaveSettings(Settings());_videoTimer.Stop();VideoPreview.Close();var hwnd=new WindowInteropHelper(this).Handle;foreach(var i in _hotkeys)Native.UnregisterHotKey(hwnd,i);
    }
}
