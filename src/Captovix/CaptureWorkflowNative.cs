using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Microsoft.Win32;

namespace Captovix;
public static class StartupRegistration
{
    private const string KeyPath=@"Software\Microsoft\Windows\CurrentVersion\Run",ValueName="HuyVo.Captovix";
    public static string? Read(){using var key=Registry.CurrentUser.OpenSubKey(KeyPath);return key?.GetValue(ValueName) as string;}
    public static void Write(string? value){using var key=Registry.CurrentUser.CreateSubKey(KeyPath);if(value==null)key.DeleteValue(ValueName,false);else key.SetValue(ValueName,value,RegistryValueKind.String);}
    public static void RemoveOwned(){try{if(Read()?.EndsWith(" --captovix-autostart",StringComparison.Ordinal)==true)Write(null);}catch(Exception e){App.Log(e);}}
}
public sealed class CaptureTray : IDisposable
{
    public const int Callback=0x8000+51;
    public static readonly uint TaskbarCreated=RegisterWindowMessage("TaskbarCreated");
    public static readonly uint ShowEditorMessage=RegisterWindowMessage("HuyVo.Captovix.ShowEditor");
    private readonly nint _hwnd;private nint _icon;private bool _owned;private NotifyData _data;
    public bool Ready{get;private set;}
    public CaptureTray(nint hwnd)
    {
        _hwnd=hwnd;try{_icon=LoadImage(0,System.IO.Path.Combine(AppContext.BaseDirectory,"Assets","Captovix-brand-v117.ico"),1,32,32,0x10);_owned=_icon!=0;if(_icon==0&&ExtractIconEx(Environment.ProcessPath!,0,out var large,out var small,1)>0){_icon=small!=0?small:large;_owned=true;if(large!=0&&large!=_icon)DestroyIcon(large);}}catch{}
        if(_icon==0)_icon=LoadIcon(0,(nint)32512);
        _data=new(){Size=(uint)Marshal.SizeOf<NotifyData>(),Window=hwnd,Id=1,Flags=1|2|4|128,Callback=Callback,Icon=_icon,Tip="Captovix · Chụp ảnh / Mở Editor",Info="",InfoTitle=""};Add();AppearanceManager.Applied+=RefreshBrand;RefreshBrand();
    }
    private void RefreshBrand(){try{
        if(Application.Current.Resources["BrandMark"] is not System.Windows.Media.Imaging.BitmapSource bitmap)return;
        var next=ShellBrandIcon.Create(bitmap,32,32);if(next==0)return;var old=_icon;bool destroy=_owned;_icon=next;_owned=true;_data.Icon=next;_data.Flags=2;ShellNotify(1,ref _data);if(destroy&&old!=0)DestroyIcon(old);
    }catch(Exception ex){App.Log(ex);}}
    public bool Add(){ShellNotify(2,ref _data);_data.Flags=1|2|4|128;Ready=ShellNotify(0,ref _data);if(Ready){_data.Version=4;ShellNotify(4,ref _data);}return Ready;}
    public void Notify(string text){if(!Ready)return;_data.Flags=16;_data.InfoTitle="Captovix";_data.Info=text.Length>255?text[..255]:text;_data.InfoFlags=1;ShellNotify(1,ref _data);}
    public static void RestoreExisting()=>PostMessage((nint)0xffff,ShowEditorMessage,0,0);
    public void Foreground()=>SetForegroundWindow(_hwnd);
    public void Dispose(){AppearanceManager.Applied-=RefreshBrand;if(Ready)ShellNotify(2,ref _data);Ready=false;if(_owned&&_icon!=0)DestroyIcon(_icon);_icon=0;}
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct NotifyData
    {
        public uint Size;public nint Window;public uint Id,Flags,Callback;public nint Icon;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Tip;
        public uint State,StateMask;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=256)]public string Info;
        public uint Version;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=64)]public string InfoTitle;
        public uint InfoFlags;public Guid Guid;public nint BalloonIcon;
    }
    [DllImport("shell32.dll",EntryPoint="Shell_NotifyIconW",CharSet=CharSet.Unicode)]private static extern bool ShellNotify(uint message,ref NotifyData data);
    [DllImport("shell32.dll",EntryPoint="ExtractIconExW",CharSet=CharSet.Unicode)]private static extern uint ExtractIconEx(string file,int index,out nint large,out nint small,uint count);
    [DllImport("user32.dll",EntryPoint="LoadImageW",CharSet=CharSet.Unicode)]private static extern nint LoadImage(nint instance,string name,uint type,int width,int height,uint flags);
    [DllImport("user32.dll")]private static extern bool DestroyIcon(nint icon);
    [DllImport("user32.dll",EntryPoint="LoadIconW")]private static extern nint LoadIcon(nint instance,nint name);
    [DllImport("user32.dll",EntryPoint="RegisterWindowMessageW",CharSet=CharSet.Unicode)]private static extern uint RegisterWindowMessage(string name);
    [DllImport("user32.dll")]private static extern bool PostMessage(nint hwnd,uint message,nint w,nint l);
    [DllImport("user32.dll")]private static extern bool SetForegroundWindow(nint hwnd);
}
public static class CapturePrompts
{
    // Temporary selectors are capture overlays; all persistent configuration remains in the Editor.
    public static async Task<WindowInfo?> PickWindowAsync(CancellationToken cancellation,bool recording=false,WindowInfo? preferred=null)
    {
        if(recording)return await RecordingWindowPicker.PickAsync(cancellation,preferred);
        cancellation.ThrowIfCancellationRequested();var done=new TaskCompletionSource<WindowInfo?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var picker=new Window{Title=recording?"Captovix · Chọn cửa sổ để quay":"Captovix · Chọn cửa sổ để chụp",Width=640,Height=460,WindowStartupLocation=WindowStartupLocation.CenterScreen,Topmost=true,ShowInTaskbar=false};
        var panel=new DockPanel{Margin=new Thickness(16)};var hint=new TextBlock{Text="Chọn cửa sổ đang mở · Enter / nhấp đúp để xác nhận · Esc hủy",Margin=new Thickness(0,0,0,12)};DockPanel.SetDock(hint,Dock.Top);panel.Children.Add(hint);
        var footer=new StackPanel{Orientation=Orientation.Horizontal};DockPanel.SetDock(footer,Dock.Bottom);panel.Children.Add(footer);
        var list=new ListBox{DisplayMemberPath="Title"};void Refresh(){var windows=Native.Windows(true);list.ItemsSource=windows;list.SelectedItem=windows.FirstOrDefault(w=>preferred!=null&&w.Handle==preferred.Handle&&w.ProcessId==preferred.ProcessId)??windows.FirstOrDefault();}
        void Finish(){if(list.SelectedItem is WindowInfo item){done.TrySetResult(item);picker.Close();}}
        var refresh=new Button{Content="Làm mới"};refresh.Click+=(_,_)=>Refresh();footer.Children.Add(refresh);var accept=new Button{Content=recording?"Quay cửa sổ này":"Chụp cửa sổ này"};accept.Click+=(_,_)=>Finish();
        var cancel=new Button{Content="Hủy"};cancel.Click+=(_,_)=>picker.Close();footer.Children.Add(cancel);footer.Children.Add(accept);panel.Children.Add(list);picker.Content=panel;
        picker.PreviewKeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Escape)picker.Close();else if(e.Key==System.Windows.Input.Key.Enter)Finish();};list.MouseDoubleClick+=(_,_)=>Finish();picker.Closed+=(_,_)=>done.TrySetResult(null);
        using var registration=cancellation.Register(()=>picker.Dispatcher.BeginInvoke(()=>{if(picker.IsVisible)picker.Close();}));Refresh();picker.Show();list.Focus();return await done.Task;
    }
    private sealed record CameraOption(string? Id,string Name){public override string ToString()=>Name;}

    internal static async Task<(int Mode,DisplayInfo Display,RecordingAudioSettings Audio,RecordingProfile Profile,bool HighlightClicks,bool Hardware,string? Camera,bool Cursor)?> PickRecordingSourceAsync(
        Window owner,IReadOnlyList<DisplayInfo> displays,int initialMode,DisplayInfo initialDisplay,AppSettings preferences,CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        var dialog=new Window{Title="Captovix · Nguồn quay",Width=Math.Min(840,SystemParameters.WorkArea.Width-24),Height=Math.Min(650,SystemParameters.WorkArea.Height-24),
            WindowStartupLocation=WindowStartupLocation.CenterScreen,ResizeMode=ResizeMode.CanResizeWithGrip,ShowInTaskbar=false,Topmost=true};
        if(owner.IsVisible){dialog.Owner=owner;dialog.WindowStartupLocation=WindowStartupLocation.CenterOwner;}
        dialog.MaxHeight=Math.Max(240,SystemParameters.WorkArea.Height-16);
        dialog.MaxWidth=Math.Max(300,SystemParameters.WorkArea.Width-16);
        dialog.MinHeight=Math.Min(320,dialog.Height);dialog.MinWidth=Math.Min(420,dialog.Width);
        var root=new Grid();root.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});root.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1,GridUnitType.Star)});root.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});dialog.Content=root;
        var header=new TextBlock{Text="Chọn nguồn quay",FontSize=20,FontWeight=FontWeights.SemiBold,Margin=new Thickness(20,14,20,10)};root.Children.Add(header);
        var body=new Grid{Margin=new Thickness(16,0,16,8)};
        var left=new StackPanel();var right=new StackPanel();
        body.Children.Add(left);body.Children.Add(right);
        var scroll=new ScrollViewer{Content=body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};Grid.SetRow(scroll,1);root.Children.Add(scroll);
        bool? lastWide=null;
        void ArrangeColumns(){bool wide=dialog.ActualWidth>=690||(dialog.ActualWidth==0&&dialog.Width>=690);if(lastWide==wide)return;lastWide=wide;body.RowDefinitions.Clear();body.ColumnDefinitions.Clear();
            if(wide){body.ColumnDefinitions.Add(new ColumnDefinition());body.ColumnDefinitions.Add(new ColumnDefinition());Grid.SetRow(left,0);Grid.SetRow(right,0);Grid.SetColumn(left,0);Grid.SetColumn(right,1);left.Margin=new Thickness(4,0,12,0);right.Margin=new Thickness(12,0,4,0);}
            else{body.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});body.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});Grid.SetColumn(left,0);Grid.SetColumn(right,0);Grid.SetRow(left,0);Grid.SetRow(right,1);left.Margin=right.Margin=new Thickness(4,0,4,0);}}
        ArrangeColumns();dialog.SizeChanged+=(_,_)=>ArrangeColumns();
        var mode=new ComboBox{ItemsSource=new[]{"Vùng chọn","Cửa sổ","Toàn màn hình"},SelectedIndex=Math.Clamp(initialMode,0,2)};
        var screen=new ComboBox{ItemsSource=displays,DisplayMemberPath="Label",SelectedItem=initialDisplay};
        var info=new TextBlock{Margin=new Thickness(4,6,4,8),TextWrapping=TextWrapping.Wrap};
        left.Children.Add(mode);
        left.Children.Add(screen);
        left.Children.Add(info);
        void Update(){
            screen.Visibility=mode.SelectedIndex==2?Visibility.Visible:Visibility.Collapsed;
            info.Text=mode.SelectedIndex switch{0=>"Click chọn khung tự nhận diện hoặc kéo chọn vùng, rồi bấm Quay. Vị trí vùng quay được cố định trên màn hình.",1=>"Chọn cửa sổ đang mở ở bước tiếp theo.",_=>"Chọn màn hình cần quay và kiểm tra độ phân giải."};
        }
        mode.SelectionChanged+=(_,_)=>Update();
        Update();
        using var audio=new RecordingAudioPanel(preferences);
        left.Children.Add(audio);
        var profile=new RecordingProfilePanel(preferences.VideoProfile??new RecordingProfile(3,0,preferences.Fps));
        right.Children.Add(profile);
        var highlight=new CheckBox{Content="Làm nổi vị trí click chuột",IsChecked=preferences.HighlightRecordingClicks,Margin=new(4,6,4,6)};
        right.Children.Add(highlight);
        var hardware=new CheckBox{Content="Ưu tiên mã hóa phần cứng (H.264)",IsChecked=preferences.Hardware,Margin=new(4,4,4,6)};
        right.Children.Add(hardware);
        void UpdateEstimate()=>profile.Update((screen.SelectedItem as DisplayInfo??initialDisplay).Bounds,
            mode.SelectedIndex==2,audio.MicrophoneEnabled,audio.SystemEnabled);
        profile.Changed+=UpdateEstimate;
        audio.SourcesChanged+=UpdateEstimate;
        mode.SelectionChanged+=(_,_)=>UpdateEstimate();
        screen.SelectionChanged+=(_,_)=>UpdateEstimate();
        UpdateEstimate();
        var camera=new ComboBox{DisplayMemberPath=nameof(CameraOption.Name),SelectedValuePath=nameof(CameraOption.Id)};
        var cameraLabel=new TextBlock{Text="Webcam · ghi thành nguồn riêng",Margin=new Thickness(4,6,4,1)};
        right.Children.Add(cameraLabel);
        right.Children.Add(camera);
        camera.Items.Add(new CameraOption(null,"Không quay webcam"));
        camera.SelectedIndex=0;
        async void LoadCameras(object? sender,EventArgs e){
            try{
                var devices=await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);
                if(!dialog.IsVisible)return;
                foreach(var device in devices)camera.Items.Add(new CameraOption(device.Id,device.Name));
            }catch(Exception error){App.Log(error);cameraLabel.Text="Webcam · chưa đọc được thiết bị";}
        }
        dialog.Loaded+=LoadCameras;
        var cursor=new CheckBox{Content="Hiển thị con trỏ chuột trong video",IsChecked=preferences.RecordingCursor,Margin=new Thickness(4,10,4,8)};
        right.Children.Add(cursor);
        var warning=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new Thickness(4,4,4,0)};
        var footer=new StackPanel{Margin=new Thickness(20,5,20,12)};Grid.SetRow(footer,2);root.Children.Add(footer);footer.Children.Add(warning);
        var row=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
        var cancel=new Button{Content="Hủy"};
        var next=new Button{Content="Tiếp tục"};
        row.Children.Add(cancel);
        row.Children.Add(next);
        footer.Children.Add(row);
        var result=new TaskCompletionSource<(int Mode,DisplayInfo Display,RecordingAudioSettings Audio,RecordingProfile Profile,bool HighlightClicks,bool Hardware,string? Camera,bool Cursor)?>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Confirm(){
            var selected=screen.SelectedItem as DisplayInfo??initialDisplay;
            try{
                result.TrySetResult((mode.SelectedIndex,selected,audio.Read(),profile.Read(),highlight.IsChecked==true,hardware.IsChecked==true,camera.SelectedValue as string,cursor.IsChecked==true));
                dialog.Close();
            }catch(InvalidOperationException error){warning.Text=error.Message;}
        }
        cancel.Click+=(_,_)=>dialog.Close();
        next.Click+=(_,_)=>Confirm();
        dialog.PreviewKeyDown+=(_,e)=>{
            if(e.Key==System.Windows.Input.Key.Escape){e.Handled=true;dialog.Close();}
            else if(e.Key==System.Windows.Input.Key.Enter&&!mode.IsDropDownOpen&&!screen.IsDropDownOpen&&!camera.IsDropDownOpen&&!audio.IsKeyboardFocusWithin){e.Handled=true;Confirm();}
        };
        dialog.Closed+=(_,_)=>result.TrySetResult(null);
        using var registration=cancellation.Register(()=>dialog.Dispatcher.BeginInvoke(()=>{if(dialog.IsVisible)dialog.Close();}));
        dialog.Show();
        mode.Focus();
        return await result.Task;
    }

    public static async Task CountdownAsync(int seconds,CancellationToken cancellation)
    {
        if(seconds<=0)return;cancellation.ThrowIfCancellationRequested();
        var label=new TextBlock{FontSize=24,Foreground=System.Windows.Media.Brushes.White,Margin=new Thickness(24)};
        var window=new Window{Title="Captovix · Đếm ngược",Content=label,SizeToContent=SizeToContent.WidthAndHeight,WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,Topmost=true,ShowInTaskbar=false,WindowStartupLocation=WindowStartupLocation.CenterScreen,Background=System.Windows.Media.Brushes.DarkSlateGray};
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(cancellation);window.PreviewKeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Escape)linked.Cancel();};
        window.Closed+=(_,_)=>linked.Cancel();
        window.Show();try{for(var n=seconds;n>0;n--){label.Text=$"Chụp sau {n} giây · Esc hủy";await Task.Delay(1000,linked.Token);}}finally{if(window.IsVisible)window.Close();}
        await Task.Delay(220,cancellation);
    }
}
