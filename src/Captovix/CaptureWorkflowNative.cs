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
        _data=new(){Size=(uint)Marshal.SizeOf<NotifyData>(),Window=hwnd,Id=1,Flags=1|2|4|128,Callback=Callback,Icon=_icon,Tip="Captovix · Chụp ảnh / Mở Editor",Info="",InfoTitle=""};Add();
    }
    public bool Add(){ShellNotify(2,ref _data);_data.Flags=1|2|4|128;Ready=ShellNotify(0,ref _data);if(Ready){_data.Version=4;ShellNotify(4,ref _data);}return Ready;}
    public void Notify(string text){if(!Ready)return;_data.Flags=16;_data.InfoTitle="Captovix";_data.Info=text.Length>255?text[..255]:text;_data.InfoFlags=1;ShellNotify(1,ref _data);}
    public static void RestoreExisting()=>PostMessage((nint)0xffff,ShowEditorMessage,0,0);
    public void Foreground()=>SetForegroundWindow(_hwnd);
    public void Dispose(){if(Ready)ShellNotify(2,ref _data);Ready=false;if(_owned&&_icon!=0)DestroyIcon(_icon);_icon=0;}
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
    public static async Task<WindowInfo?> PickWindowAsync(CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();var done=new TaskCompletionSource<WindowInfo?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var picker=new Window{Title="Captovix · Chọn cửa sổ để chụp",Width=640,Height=460,WindowStartupLocation=WindowStartupLocation.CenterScreen,Topmost=true,ShowInTaskbar=false};
        var panel=new DockPanel{Margin=new Thickness(16)};var hint=new TextBlock{Text="Chọn cửa sổ đang mở · Enter / nhấp đúp để xác nhận · Esc hủy",Margin=new Thickness(0,0,0,12)};DockPanel.SetDock(hint,Dock.Top);panel.Children.Add(hint);
        var footer=new StackPanel{Orientation=Orientation.Horizontal};DockPanel.SetDock(footer,Dock.Bottom);panel.Children.Add(footer);
        var list=new ListBox{DisplayMemberPath="Title"};void Refresh(){list.ItemsSource=Native.Windows();list.SelectedIndex=0;}
        void Finish(){if(list.SelectedItem is WindowInfo item){done.TrySetResult(item);picker.Close();}}
        var refresh=new Button{Content="Làm mới"};refresh.Click+=(_,_)=>Refresh();footer.Children.Add(refresh);var accept=new Button{Content="Chụp cửa sổ này"};accept.Click+=(_,_)=>Finish();footer.Children.Add(accept);
        var cancel=new Button{Content="Hủy"};cancel.Click+=(_,_)=>picker.Close();footer.Children.Add(cancel);panel.Children.Add(list);picker.Content=panel;
        picker.PreviewKeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Escape)picker.Close();else if(e.Key==System.Windows.Input.Key.Enter)Finish();};list.MouseDoubleClick+=(_,_)=>Finish();picker.Closed+=(_,_)=>done.TrySetResult(null);
        using var registration=cancellation.Register(()=>picker.Dispatcher.BeginInvoke(()=>picker.Close()));Refresh();picker.Show();list.Focus();return await done.Task;
    }
    public static async Task CountdownAsync(int seconds,CancellationToken cancellation)
    {
        if(seconds<=0)return;cancellation.ThrowIfCancellationRequested();
        var label=new TextBlock{FontSize=24,Foreground=System.Windows.Media.Brushes.White,Margin=new Thickness(24)};
        var window=new Window{Title="Captovix · Đếm ngược",Content=label,SizeToContent=SizeToContent.WidthAndHeight,WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,Topmost=true,ShowInTaskbar=false,WindowStartupLocation=WindowStartupLocation.CenterScreen,Background=System.Windows.Media.Brushes.DarkSlateGray};
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(cancellation);window.PreviewKeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Escape)linked.Cancel();};
        window.Show();try{for(var n=seconds;n>0;n--){label.Text=$"Chụp sau {n} giây · Esc hủy";await Task.Delay(1000,linked.Token);}}finally{window.Close();}
        await Task.Delay(220,cancellation);
    }
}
