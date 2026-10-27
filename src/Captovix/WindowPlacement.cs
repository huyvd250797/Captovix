using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
namespace Captovix;
internal static class WindowPlacement
{
    private static readonly ConditionalWeakTable<Window,object> Attached=new();
    internal static void Attach(Window window)
    {
        if(window.AllowsTransparency||Attached.TryGetValue(window,out _))return;Attached.Add(window,new());
        HwndSource? source=null;bool fitting=false;
        bool Info(out MonitorInfo info){info=new(){Size=Marshal.SizeOf<MonitorInfo>()};var handle=new WindowInteropHelper(window).Handle;return handle!=0&&GetMonitorInfo(MonitorFromWindow(handle,2),ref info);}
        void Fit(){if(fitting||window.WindowState!=WindowState.Maximized||!Info(out var m))return;fitting=true;try{SetWindowPos(new WindowInteropHelper(window).Handle,0,m.Work.Left,m.Work.Top,m.Work.Right-m.Work.Left,m.Work.Bottom-m.Work.Top,0x14);}finally{fitting=false;}}
        nint Hook(nint hwnd,int message,nint wp,nint lp,ref bool handled){
            if(message==0x24&&Info(out var m)){var info=Marshal.PtrToStructure<MinMax>(lp);var area=WorkAreaGeometry.Maximized(m.Monitor.Rect,m.Work.Rect);info.MaxPosition=new(){X=area.X,Y=area.Y};info.MaxSize=new(){X=area.Width,Y=area.Height};info.MinTrack.X=Math.Min(info.MinTrack.X,area.Width);info.MinTrack.Y=Math.Min(info.MinTrack.Y,area.Height);Marshal.StructureToPtr(info,lp,false);handled=true;}
            if(message is 0x2e0 or 0x7e or 0x1a)window.Dispatcher.BeginInvoke(Fit,DispatcherPriority.Loaded);return 0;
        }
        void Connect(){if(source!=null)return;var hwnd=new WindowInteropHelper(window).Handle;if(hwnd==0)return;source=HwndSource.FromHwnd(hwnd);source?.AddHook(Hook);window.Dispatcher.BeginInvoke(Fit,DispatcherPriority.Loaded);}
        window.SourceInitialized+=(_,_)=>Connect();window.Loaded+=(_,_)=>{Connect();Fit();};window.StateChanged+=(_,_)=>window.Dispatcher.BeginInvoke(Fit,DispatcherPriority.Loaded);window.Closed+=(_,_)=>source?.RemoveHook(Hook);Connect();
    }
    [StructLayout(LayoutKind.Sequential)]private struct PointI {public int X,Y;}
    [StructLayout(LayoutKind.Sequential)]private struct RectI {public int Left,Top,Right,Bottom;public PixelRect Rect=>new(Left,Top,Right-Left,Bottom-Top);}
    [StructLayout(LayoutKind.Sequential)]private struct MonitorInfo {public int Size;public RectI Monitor,Work;public int Flags;}
    [StructLayout(LayoutKind.Sequential)]private struct MinMax {public PointI Reserved,MaxSize,MaxPosition,MinTrack,MaxTrack;}
    [DllImport("user32.dll")]private static extern nint MonitorFromWindow(nint hwnd,uint flags);
    [DllImport("user32.dll",CharSet=CharSet.Auto)] [return:MarshalAs(UnmanagedType.Bool)]private static extern bool GetMonitorInfo(nint monitor,ref MonitorInfo info);
    [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)]private static extern bool SetWindowPos(nint hwnd,nint after,int x,int y,int width,int height,uint flags);
}
