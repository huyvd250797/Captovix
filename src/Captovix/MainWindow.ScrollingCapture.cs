using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
namespace Captovix;
public partial class MainWindow
{
    private void ScrollCapture_Click(object sender,RoutedEventArgs e)=>_=ScrollCaptureAsync();
    private async Task ScrollCaptureAsync()
    {
        if(_busy||_updateApplying||!_captureGate.TryEnter())return;
        var previous=WindowState;ScrollSession? session=null;SetBusy(true);_settingsDialog?.Close();
        try{
            Surface.CancelInteraction();if(!FlushEdits())return;VideoPreview.Pause();_playing=false;
            _captureCancellation=new();var cancellation=_captureCancellation.Token;RegisterCaptureEscape();RefreshSources();var topology=CaptureWorkflowStore.Topology(WorkflowDisplays());
            Hide();await Task.Delay(220,cancellation);var region=await RegionPicker.PickAsync(_displays,cancellation);if(region==null)return;
            ScrollStitching.OutputHeight(region.Value.Width,region.Value.Height,Array.Empty<int>());
            session=ScrollSession.Create(region.Value,topology);
            var controller=new ScrollCaptureController(session,_displays,cancellation);bool finish=await controller.RunAsync();cancellation.ThrowIfCancellationRequested();
            Show();WindowState=previous==WindowState.Minimized?WindowState.Maximized:previous;Activate();ReleaseCaptureEscape();
            if(finish&&session.Count>0){var preview=new ScrollPreviewWindow(session){Owner=this};if(preview.ShowDialog()==true&&preview.Result is {} image)SaveScrollResult(session,image);}
            SetStatus($"Phiên chụp cuộn giữ tại {session.Folder}. Có thể khôi phục hoặc mở các PNG nguồn.");
        }catch(OperationCanceledException){SetStatus("Đã hủy chụp cuộn. Các khung đã lấy vẫn được giữ để khôi phục.");}
        catch(Exception ex){App.Log(ex);Show();MessageBox.Show(this,ex.Message+(session!=null?"\n\nKhung nguồn được giữ tại:\n"+session.Folder:""),"Chụp cuộn chưa hoàn tất",MessageBoxButton.OK,MessageBoxImage.Warning);}
        finally{ReleaseCaptureEscape();_captureCancellation?.Dispose();_captureCancellation=null;Show();WindowState=previous==WindowState.Minimized?WindowState.Maximized:previous;Activate();SetBusy(false);_captureGate.Exit();if(_exitRequested)_=Dispatcher.BeginInvoke(RequestExit);}
    }
    private void SaveScrollResult(ScrollSession session,BitmapSource image)
    {
        var path=_store.NewPath(false);var pending=path+".tmp";try{ImageTransfer.Save(image,pending);File.Move(pending,path);}finally{if(File.Exists(pending))File.Delete(pending);}
        var item=new MediaItem{FilePath=path,Title="Ảnh cuộn "+DateTime.Now.ToString("dd-MM HH:mm:ss"),Width=image.PixelWidth,Height=image.PixelHeight,Source=$"Cuộn dọc · {session.Count} khung · pixel gốc SDR8 · nguồn: {session.Folder}"};_store.Add(item);SearchBox.Clear();MediaList.SelectedItem=item;MediaList.ScrollIntoView(item);
    }
    private void RecoverScroll_Click(object sender,RoutedEventArgs e)
    {
        if(_busy||_updateApplying)return;Surface.CancelInteraction();if(!FlushEdits()||!_captureGate.TryEnter())return;SetBusy(true);_settingsDialog?.Close();
        try{var picker=new OpenFileDialog{Title="Mở session.json của phiên chụp cuộn",Filter="Phiên chụp cuộn|session.json",InitialDirectory=Directory.Exists(ScrollSession.Root)?ScrollSession.Root:App.DataRoot};if(picker.ShowDialog(this)!=true)return;var session=ScrollSession.Open(picker.FileName);var preview=new ScrollPreviewWindow(session){Owner=this};if(preview.ShowDialog()==true&&preview.Result is {} image)SaveScrollResult(session,image);}catch(Exception ex){App.Log(ex);MessageBox.Show(this,ex.Message,"Không khôi phục được phiên");}finally{SetBusy(false);_captureGate.Exit();if(_exitRequested)_=Dispatcher.BeginInvoke(RequestExit);}
    }
}
internal sealed class ScrollCaptureController : Window
{
    private readonly ScrollSession _session;
    private readonly IReadOnlyList<DisplayInfo> _displays;
    private readonly CancellationToken _cancellation;
    private readonly TaskCompletionSource<bool> _completion=new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TextBlock _status=new(){TextWrapping=TextWrapping.Wrap,Margin=new(4,8,4,8)};
    private readonly Button _take=new(){Content="Lấy khung · Ctrl+Shift+F10"};
    private readonly Button _finish=new(){Content="Ghép & xem trước · Ctrl+Shift+F11",IsEnabled=false};
    private bool _working,_closed,_finishing,_closeRequested;
    private nint _hwnd;
    private bool _takeKey,_finishKey;
    public ScrollCaptureController(ScrollSession session,IReadOnlyList<DisplayInfo> displays,CancellationToken cancellation)
    {
        SetResourceReference(StyleProperty,typeof(Window));UseLayoutRounding=true;_session=session;_displays=displays;_cancellation=cancellation;Title="Captovix · Chụp cuộn dọc";Width=400;SizeToContent=SizeToContent.Height;ResizeMode=ResizeMode.NoResize;Topmost=true;ShowInTaskbar=true;WindowStartupLocation=WindowStartupLocation.CenterScreen;ShowActivated=false;
        var panel=new StackPanel{Margin=new(12)};Content=panel;panel.Children.Add(new TextBlock{Text="Chụp nội dung cuộn xuống",FontSize=18,FontWeight=FontWeights.SemiBold});panel.Children.Add(new TextBlock{Text="1. Chọn vùng chỉ chứa nội dung, tránh thanh cố định.\n2. Lấy khung đầu; cuộn xuống khoảng nửa vùng.\n3. Chờ trang đứng yên rồi lấy khung tiếp. Không đổi zoom, kéo ngang hay đổi cửa sổ.",TextWrapping=TextWrapping.Wrap,Margin=new(4,10,4,4)});panel.Children.Add(_status);panel.Children.Add(_take);panel.Children.Add(_finish);
        var cancel=new Button{Content="Hủy · giữ khung nguồn"};panel.Children.Add(cancel);cancel.Click+=(_,_)=>Close();_take.Click+=async(_,_)=>await TakeAsync();_finish.Click+=(_,_)=>Finish();
        SourceInitialized+=(_,_)=>{_hwnd=new WindowInteropHelper(this).Handle;Native.SetWindowDisplayAffinity(_hwnd,0x11);var source=HwndSource.FromHwnd(_hwnd);source?.AddHook(Hook);_takeKey=Native.RegisterHotKey(_hwnd,1210,0x4006,0x79);_finishKey=Native.RegisterHotKey(_hwnd,1211,0x4006,0x7A);_status.Text="Sẵn sàng · 0 khung"+(!_takeKey||!_finishKey?". Có phím bị trùng; dùng nút lấy/ghép khung.":"");};
        Closing+=(_,e)=>{if(_working){_closeRequested=true;_status.Text="Đang hoàn tất khung đang lấy rồi đóng…";e.Cancel=true;}};
        Closed+=(_,_)=>{_closed=true;Native.UnregisterHotKey(_hwnd,1210);Native.UnregisterHotKey(_hwnd,1211);_completion.TrySetResult(_finishing&&_session.Count>0&&!_cancellation.IsCancellationRequested);};
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape){_finishing=false;Close();e.Handled=true;}};
    }
    public async Task<bool> RunAsync(){using var registration=_cancellation.Register(()=>Dispatcher.BeginInvoke(()=>{_finishing=false;Close();}));Show();return await _completion.Task;}
    private nint Hook(nint hwnd,int msg,nint w,nint l,ref bool handled){if(msg==0x312){handled=true;if((int)w==1210)_=TakeAsync();else if((int)w==1211)Finish();}return 0;}
    private void Finish(){if(_working||_session.Count==0)return;_finishing=true;Close();}
    private async Task TakeAsync()
    {
        if(_working||_closed)return;_working=true;_take.IsEnabled=_finish.IsEnabled=false;string message="";
        try{
            _cancellation.ThrowIfCancellationRequested();if(_session.Count>=ScrollStitching.MaxFrames||(long)_session.Width*_session.Height*(_session.Count+1)>ScrollStitching.MaxSourcePixels)throw new InvalidOperationException("Đã tới giới hạn phiên. Hãy ghép và bắt đầu phiên mới.");
            // Always hide the controller, even when display affinity succeeded. No UI is included in raw frames.
            Hide();await Task.Delay(350,_cancellation);var now=Native.Displays();var topology=CaptureWorkflowStore.Topology(now.Select(d=>new WorkflowDisplay(d.Device,d.Bounds,d.DpiX,d.DpiY,d.Identity)));if(topology!=_session.Topology)throw new InvalidOperationException("Màn hình/DPI đã đổi. Kết thúc phiên, không ghép tiếp.");
            string path=Path.Combine(_session.Folder,"candidate-"+Guid.NewGuid().ToString("N")+".png");await Task.Run(()=>DesktopPhotoCapture.Save(_session.Region,_displays,false,path),_cancellation);_cancellation.ThrowIfCancellationRequested();
            var match=_session.Count==0?null:await Task.Run(()=>ScrollStitching.Match(_session.Load(_session.Count-1),ScrollSession.ReadFrame(path,_session.Width,_session.Height),_cancellation),_cancellation);
            if(match?.Kind==ScrollMatchKind.Duplicate){File.Delete(path);message=match.Message;}
            else{
                var overlaps=_session.Joins.Select(j=>j.Overlap).ToList();if(match!=null)overlaps.Add(match.Overlap);ScrollStitching.OutputHeight(_session.Width,_session.Height,overlaps);
                _session.Append(path,match);message=match?.Message??"Đã lấy khung đầu. Cuộn xuống khoảng nửa vùng rồi lấy tiếp.";
            }
        }catch(OperationCanceledException){_finishing=false;}
        catch(Exception ex){App.Log(ex);message=ex.Message+" Khung đã ghi vẫn ở thư mục phiên.";}
        finally{_working=false;if(_cancellation.IsCancellationRequested||_finishing||_closeRequested)Close();else if(!_closed){Show();_status.Text=$"{_session.Count} / 24 khung · "+message;_take.IsEnabled=true;_finish.IsEnabled=_session.Count>0;}}
    }
}
internal sealed class ScrollPreviewWindow : Window
{
    private readonly ScrollSession _session;
    private readonly Image _image=new(){Stretch=Stretch.Uniform,VerticalAlignment=VerticalAlignment.Top};
    private readonly Canvas _seams=new(){IsHitTestVisible=false};
    private readonly ComboBox _join=new();
    private readonly TextBox _overlap=new();
    private readonly CheckBox _confirm=new(){Content="Tôi đã kiểm tra điểm nối này"};
    private readonly TextBlock _message=new(){TextWrapping=TextWrapping.Wrap,Margin=new(4,8,4,8)};
    private readonly Button _save=new(){Content="Đưa ảnh ghép vào Editor",IsEnabled=false};
    private readonly CancellationTokenSource _cancel=new();
    private BitmapSource? _full;
    private bool _rendering,_closing,_updating,_actual;
    public BitmapSource? Result {get;private set;}
    public ScrollPreviewWindow(ScrollSession session)
    {
        SetResourceReference(StyleProperty,typeof(Window));UseLayoutRounding=true;_image.SnapsToDevicePixels=true;_session=session;Title="Captovix · Kiểm tra điểm nối";Width=Math.Min(1120,SystemParameters.WorkArea.Width-32);Height=Math.Min(800,SystemParameters.WorkArea.Height-32);MinWidth=640;MinHeight=420;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        var layout=new Grid{Margin=new(12)};layout.ColumnDefinitions.Add(new());layout.ColumnDefinitions.Add(new(){Width=new(300)});Content=layout;
        var view=new Grid();view.Children.Add(_image);view.Children.Add(_seams);var scroll=new ScrollViewer{Content=view,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,Background=Brushes.DimGray,Margin=new(0,0,12,0)};layout.Children.Add(scroll);
        var panel=new StackPanel();Grid.SetColumn(panel,1);layout.Children.Add(new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto}.InColumn(1));
        panel.Children.Add(new TextBlock{Text="Ảnh dài · pixel gốc",FontSize=20,FontWeight=FontWeights.SemiBold});panel.Children.Add(new TextBlock{Text="Đường tím đánh dấu mối nối (không xuất ra ảnh). Kiểm tra chữ và đường nét, tránh thiếu/lặp hàng. Chọn overlap theo pixel nguồn.",TextWrapping=TextWrapping.Wrap,Margin=new(4,10,4,10)});
        panel.Children.Add(_join);panel.Children.Add(new TextBlock{Text="Overlap · số hàng khung sau bị bỏ"});panel.Children.Add(_overlap);panel.Children.Add(_confirm);
        var apply=new Button{Content="Cập nhật điểm nối & xem lại"};panel.Children.Add(apply);panel.Children.Add(_message);panel.Children.Add(_save);var folder=new Button{Content="Mở thư mục khung nguồn"};panel.Children.Add(folder);folder.Click+=(_,_)=>System.Diagnostics.Process.Start("explorer.exe",_session.Folder);
        panel.Children.Add(new TextBlock{Text="Ảnh xem trước có thể thu nhỏ. Khi lưu, app ghép trực tiếp hàng pixel gốc, không resize. Nguồn được giữ sau khi đóng; có thể xóa thư mục phiên khi không còn cần.",TextWrapping=TextWrapping.Wrap,Margin=new(4,12,4,4)});
        var zoom=new ComboBox{ItemsSource=new[]{"Vừa chiều ngang · xem trước","100% · kiểm tra pixel"},SelectedIndex=0};panel.Children.Add(zoom);zoom.SelectionChanged+=(_,_)=>ShowBitmap(zoom.SelectedIndex==1);
        _join.ItemsSource=Enumerable.Range(0,_session.Joins.Count).Select(i=>$"Khung {i+1} → {i+2}"+(session.Joins[i].Uncertain?" · cần kiểm tra":"")).ToArray();_join.SelectionChanged+=(_,_)=>SelectJoin();_join.SelectedIndex=_session.Joins.Count>0?0:-1;
        _overlap.TextChanged+=(_,_)=>{if(!_updating){_save.IsEnabled=false;_confirm.IsChecked=false;_message.Text="Thay đổi chưa áp dụng. Bấm cập nhật để xem lại.";}};
        apply.IsEnabled=_session.Joins.Count>0;_overlap.IsEnabled=_confirm.IsEnabled=_session.Joins.Count>0;
        apply.Click+=async(_,_)=>{if(_rendering||_join.SelectedIndex<0)return;int i=_join.SelectedIndex;try{if(!int.TryParse(_overlap.Text,out int overlap)||overlap<1||overlap>=_session.Height)throw new InvalidDataException("Overlap phải từ 1 đến "+(_session.Height-1)+" px.");var values=_session.Joins.Select(j=>j.Overlap).ToArray();values[i]=overlap;ScrollStitching.OutputHeight(_session.Width,_session.Height,values);_session.Joins[i].Overlap=overlap;_session.Joins[i].Confirmed=_confirm.IsChecked==true;_session.Save();await RenderAsync(zoom.SelectedIndex==1);}catch(Exception ex){_message.Text=ex.Message;}};
        _save.Click+=(_,_)=>{if(_rendering||_full==null)return;Result=_full;DialogResult=true;};
        Loaded+=async(_,_)=>await RenderAsync(false);DpiChanged+=(_,_)=>ShowBitmap(_actual);Closed+=(_,_)=>{_closing=true;_cancel.Cancel();};
    }
    private void SelectJoin(){int i=_join.SelectedIndex;if(i<0)return;_updating=true;_overlap.Text=_session.Joins[i].Overlap.ToString();_confirm.IsChecked=_session.Joins[i].Confirmed;_updating=false;_message.Text=_session.Joins[i].Message;}
    private async Task RenderAsync(bool actual)
    {
        if(_rendering)return;_rendering=true;_save.IsEnabled=false;_join.IsEnabled=false;_overlap.IsEnabled=false;_confirm.IsEnabled=false;_message.Text="Đang ghép hàng pixel…";
        try{_full=await Task.Run(()=>_session.Compose(_cancel.Token),_cancel.Token);if(_closing)return;ShowBitmap(actual);bool pending=_session.Joins.Any(j=>j.Uncertain&&!j.Confirmed);_save.IsEnabled=!pending;_message.Text=$"{_full.PixelWidth} × {_full.PixelHeight} px · {_session.Count} khung. "+(pending?"Còn điểm nối chưa chắc: sửa overlap, chọn xác nhận rồi cập nhật từng điểm nối.":"Hãy xem lại mối nối trước khi lưu. Khung nguồn vẫn được giữ.");}
        catch(OperationCanceledException){}catch(Exception ex){App.Log(ex);_message.Text=ex.Message;_full=null;_save.IsEnabled=false;}
        finally{_rendering=false;_join.IsEnabled=_session.Joins.Count>0;_overlap.IsEnabled=_confirm.IsEnabled=_session.Joins.Count>0;}
    }
    private void ShowBitmap(bool actual)
    {
        _actual=actual;if(_full==null)return;double width=actual?_full.PixelWidth/VisualTreeHelper.GetDpi(this).DpiScaleX:Math.Min(_full.PixelWidth,Math.Max(200,Width-360));double scale=width/_full.PixelWidth;_image.Source=_full;_image.Width=width;_image.Height=_full.PixelHeight*scale;_seams.Width=width;_seams.Height=_image.Height;_seams.Children.Clear();int y=_session.Height;
        foreach(var join in _session.Joins){var line=new System.Windows.Shapes.Line{X1=0,X2=width,Y1=y*scale,Y2=y*scale,Stroke=Brushes.MediumPurple,StrokeThickness=2};_seams.Children.Add(line);y+=_session.Height-join.Overlap;}
    }
}
internal static class ScrollLayoutExtensions { public static T InColumn<T>(this T element,int column) where T:FrameworkElement {Grid.SetColumn(element,column);return element;} }
