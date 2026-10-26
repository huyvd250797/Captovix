using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
namespace Captovix;
internal enum CaptureChoice{Photo,Vertical,Horizontal}
internal static class CaptureActionPicker
{
    internal static async Task<CaptureChoice?> PickAsync(PixelRect region,IReadOnlyList<DisplayInfo> displays,CancellationToken cancel)
    {
        var done=new TaskCompletionSource<CaptureChoice?>(TaskCreationOptions.RunContinuationsAsynchronously);var windows=new List<Window>();
        void Finish(CaptureChoice? result){if(done.TrySetResult(result))foreach(var w in windows.ToArray())w.Close();}
        foreach(var d in displays){var shade=new CaptureShade();var w=new Window{WindowStyle=WindowStyle.None,AllowsTransparency=true,Background=Brushes.Transparent,ShowInTaskbar=false,ShowActivated=false,Topmost=true,ResizeMode=ResizeMode.NoResize};w.Content=shade;windows.Add(w);w.SourceInitialized+=(_,_)=>{var h=new WindowInteropHelper(w).Handle;Native.SetWindowDisplayAffinity(h,0x11);Native.SetWindowPos(h,-1,d.Bounds.X,d.Bounds.Y,d.Bounds.Width,d.Bounds.Height,0x10);};w.Show();var part=region.Intersect(d.Bounds);if(!part.IsEmpty)shade.Reveal(new Rect(w.PointFromScreen(new(part.X,part.Y)),w.PointFromScreen(new(part.Right,part.Bottom))));w.PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Enter)Finish(CaptureChoice.Photo);if(e.Key==Key.Escape)Finish(null);};}
        var display=displays.FirstOrDefault(d=>ElementSelection.Contains(d.Bounds,region.X+region.Width/2,region.Y+region.Height/2))??displays[0];double scale=display.DpiX/96d;
        void Action(string label,string tooltip,int x,int y,CaptureChoice? result,int width=42){var button=new Button{Content=label,ToolTip=tooltip,Padding=new(8,5,8,5),Margin=new(0)};var w=new Window{WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,AllowsTransparency=true,Background=Brushes.Transparent,ShowInTaskbar=false,Topmost=true,Width=width,Height=38,Content=button};windows.Add(w);w.SourceInitialized+=(_,_)=>{var h=new WindowInteropHelper(w).Handle;Native.SetWindowDisplayAffinity(h,0x11);int pw=(int)Math.Round(width*scale),ph=(int)Math.Round(38*scale);Native.SetWindowPos(h,-1,Math.Clamp(x,display.Bounds.X,Math.Max(display.Bounds.X,display.Bounds.Right-pw)),Math.Clamp(y,display.Bounds.Y,Math.Max(display.Bounds.Y,display.Bounds.Bottom-ph)),pw,ph,0x40);};button.Click+=(_,_)=>Finish(result);w.PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Enter)Finish(CaptureChoice.Photo);if(e.Key==Key.Escape)Finish(null);};w.Show();}
        Action("↓","Chụp cuộn xuống · mở cấu hình và phiên ghép",region.X+region.Width/2-20,region.Bottom+4,CaptureChoice.Vertical);
        Action("→","Chụp cuộn sang phải · mở cấu hình và phiên ghép",region.Right+4,region.Y+region.Height/2-20,CaptureChoice.Horizontal);
        Action("Chụp · Enter","Chụp ảnh tĩnh vùng đã chọn",region.X,region.Y-42,CaptureChoice.Photo,118);
        Action("×","Hủy · Esc",region.Right-42,region.Y-42,null);
        using var registration=cancel.Register(()=>Application.Current.Dispatcher.BeginInvoke(()=>Finish(null)));try{return await done.Task;}finally{foreach(var w in windows.ToArray())w.Close();}
    }
}
