using System.Runtime.InteropServices;
namespace Captovix;
internal static class AdvancedCaptureNative
{
    [DllImport("user32.dll")]internal static extern nint WindowFromPoint(Native.POINT point);
    [DllImport("user32.dll")]internal static extern nint GetAncestor(nint hwnd,uint flags);
    [DllImport("user32.dll")]internal static extern nint GetForegroundWindow();
    [DllImport("user32.dll")]internal static extern bool GetClientRect(nint hwnd,out Native.RECT rect);
    [DllImport("user32.dll")]internal static extern bool ClientToScreen(nint hwnd,ref Native.POINT point);
    [DllImport("user32.dll",SetLastError=true)]private static extern nint SendMessageTimeout(nint h,uint m,nint w,nint l,uint flags,uint timeout,out nint result);
    internal static nint Target(PixelRect region)=>WindowFromPoint(new(){X=region.X+region.Width/2,Y=region.Y+region.Height/2});
    internal static bool Scroll(nint target,bool horizontal,PixelRect region){if(!Native.IsWindow(target)||GetAncestor(GetForegroundWindow(),2)!=GetAncestor(target,2))return false;int x=region.X+region.Width/2,y=region.Y+region.Height/2;uint delta=unchecked((ushort)(horizontal?120:-120));return SendMessageTimeout(target,horizontal?0x20Eu:0x20Au,(nint)(delta<<16),(nint)((ushort)x|((uint)(ushort)y<<16)),2,500,out _)!=0;}
}
