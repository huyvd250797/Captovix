using System.Runtime.InteropServices;
namespace Captovix;
internal static class HybridCaptureService
{
    private delegate bool WindowProc(nint hwnd,nint data);
    internal static HybridWindow[] Windows(){
        var windows=new List<HybridWindow>();EnumWindows((h,_)=>{if(windows.Count>=256)return false;if(Native.IsWindowVisible(h)&&!Native.IsIconic(h)&&Native.GetWindowRect(h,out var r)&&HybridCapture.Valid(r.Pixels)){
            DwmGetWindowAttribute(h,14,out int cloaked,4);if(cloaked!=0)return true;GetWindowThreadProcessId(h,out uint pid);windows.Add(new((long)h,pid,r.Pixels));}return true;},0);return windows.ToArray();
    }
    internal static async Task<HybridEvidence?> ReadAsync(FrozenFrame frame,DateTimeOffset capturedAt,string topology,IReadOnlyList<DisplayInfo> displays,CancellationToken token,long window=0,HybridWindow[]? earlier=null){
        if((long)frame.Bounds.Width*frame.Bounds.Height>HybridCapture.MaxVerifyPixels)return null;
        try{
            var before=earlier??Windows();var query=new HybridQuery(before,displays.Select(d=>new HybridDisplay(d.Bounds,d.DpiX,d.DpiY)).ToArray(),Environment.ProcessId,window);
            var nodes=await new HybridProbeClient().QueryAsync(query,token);token.ThrowIfCancellationRequested();if(nodes.Length==0)return null;
            return await Task.Run(()=>{
                token.ThrowIfCancellationRequested();var after=Windows();var fresh=Native.Displays();var actual=CaptureWorkflowStore.Topology(fresh.Select(d=>new WorkflowDisplay(d.Device,d.Bounds,d.DpiX,d.DpiY,d.Identity)));
                if(actual!=topology)return null;
                var pixels=DesktopPhotoCapture.ReadPixels(frame.Bounds,displays,false); // cursor mismatches reject only affected elements
                var draft=new HybridEvidence(1,capturedAt,DateTimeOffset.UtcNow,topology,frame.Bounds,nodes);
                return HybridCapture.Verify(draft,frame.Pixels,pixels,before,after,actual,DateTimeOffset.UtcNow);
            },token);
        }catch(OperationCanceledException)when(token.IsCancellationRequested){throw;}catch(Exception ex){CaptureDiagnostics.Write("Hybrid fallback: "+ex.GetType().Name);return null;}
    }
    internal static async Task<HybridEvidence?> WindowAsync(PixelRect region,long window,string topology,IReadOnlyList<DisplayInfo> displays,CancellationToken token){
        if((long)region.Width*region.Height>HybridCapture.MaxVerifyPixels)return null;
        try{var earlier=Windows();var stamp=DateTimeOffset.UtcNow;var frame=await Task.Run(()=>new FrozenFrame(region,DesktopPhotoCapture.ReadPixels(region,displays,false)),token);return await ReadAsync(frame,stamp,topology,displays,token,window,earlier);}
        catch(OperationCanceledException)when(token.IsCancellationRequested){throw;}catch(Exception ex){CaptureDiagnostics.Write("Hybrid window fallback: "+ex.GetType().Name);return null;}
    }
    [DllImport("user32.dll")]private static extern bool EnumWindows(WindowProc callback,nint data);
    [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(nint hwnd,out uint pid);
    [DllImport("dwmapi.dll")]private static extern int DwmGetWindowAttribute(nint hwnd,int attribute,out int value,int size);
}
