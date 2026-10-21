using System.Runtime.InteropServices;
using System.Text;

namespace Captovix;
public record DisplayInfo(string Device, PixelRect Bounds, bool Primary, uint DpiX, uint DpiY,string Identity="")
{
    public string Label => $"{Device.Replace(@"\\.\", "")} · {Bounds.Width}×{Bounds.Height} · {DpiX * 100 / 96}%{(Primary ? " · Chính" : "")}";
}
public record WindowInfo(nint Handle, string Title,uint ProcessId=0) { public override string ToString() => Title; }
public static class Native
{
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left,Top,Right,Bottom; public readonly PixelRect Pixels => new(Left,Top,Right-Left,Bottom-Top); }
    [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X,Y; }
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] public struct MONITORINFOEX { public int Size; public RECT Monitor,Work; public uint Flags; [MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)] public string Device; }
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct DISPLAY_DEVICE
    {
        public int Size;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Name;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Description;public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string Id;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string RegistryKey;
    }
    [DllImport("user32.dll",EntryPoint="EnumDisplayDevicesW",CharSet=CharSet.Unicode)]private static extern bool EnumDisplayDevices(string device,uint number,ref DISPLAY_DEVICE info,uint flags);
    private delegate bool MonitorProc(nint monitor,nint hdc,ref RECT rect,nint data);
    private delegate bool WindowProc(nint hwnd,nint data);
    [DllImport("user32.dll")] private static extern bool EnumDisplayMonitors(nint hdc,nint clip,MonitorProc proc,nint data);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern bool GetMonitorInfo(nint monitor,ref MONITORINFOEX info);
    [DllImport("shcore.dll")] private static extern int GetDpiForMonitor(nint monitor,int type,out uint x,out uint y);
    [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT point);
    [DllImport("user32.dll")] private static extern bool EnumWindows(WindowProc proc,nint data);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(nint hwnd);
    [DllImport("user32.dll")] public static extern bool IsIconic(nint hwnd);
    [DllImport("user32.dll")] public static extern bool IsWindow(nint hwnd);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] private static extern int GetWindowText(nint hwnd,StringBuilder title,int size);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(nint hwnd,out RECT rect);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint hwnd,out uint processId);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(nint hwnd,nint after,int x,int y,int width,int height,uint flags);
    [DllImport("user32.dll")] public static extern bool SetWindowDisplayAffinity(nint hwnd,uint affinity);
    [DllImport("user32.dll")] public static extern bool RegisterHotKey(nint hwnd,int id,uint modifiers,uint key);
    [DllImport("user32.dll")] public static extern bool UnregisterHotKey(nint hwnd,int id);
    public static List<DisplayInfo> Displays()
    {
        var result=new List<DisplayInfo>();
        EnumDisplayMonitors(0,0,(nint m,nint h,ref RECT r,nint d)=>{
            var i=new MONITORINFOEX {Size=Marshal.SizeOf<MONITORINFOEX>(),Device=""};
            if(GetMonitorInfo(m,ref i)) {
                uint x=96,y=96; if(GetDpiForMonitor(m,0,out var dx,out var dy)==0){x=dx;y=dy;}
                var monitor=new DISPLAY_DEVICE{Size=Marshal.SizeOf<DISPLAY_DEVICE>(),Name="",Description="",Id="",RegistryKey=""};var identity=EnumDisplayDevices(i.Device,0,ref monitor,1)?monitor.Id:"";
                result.Add(new(i.Device,i.Monitor.Pixels,(i.Flags&1)!=0,x,y,identity));
            } return true;
        },0);
        return result.OrderByDescending(x=>x.Primary).ThenBy(x=>x.Bounds.X).ToList();
    }
    public static List<WindowInfo> Windows()
    {
        var result=new List<WindowInfo>();
        EnumWindows((h,_)=>{
            GetWindowThreadProcessId(h,out var pid);
            if(pid==Environment.ProcessId || !IsWindowVisible(h) || IsIconic(h)) return true;
            var text=new StringBuilder(512); GetWindowText(h,text,text.Capacity);
            if(text.Length>0 && GetWindowRect(h,out var r) && r.Right-r.Left>50 && r.Bottom-r.Top>50) result.Add(new(h,text.ToString(),pid));
            return true;
        },0); return result;
    }
}
