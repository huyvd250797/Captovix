using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;

namespace Captovix;
public static class ElementProbeHost
{
    // All cross-process UIA lives on one MTA worker in an expendable process.
    // The Editor can terminate this process if an external provider stalls.
    public static int Run()=>Task.Run(()=>{
        string? line;while((line=Console.ReadLine())!=null){
            try{var q=JsonSerializer.Deserialize<ElementQuery>(line)!;Console.WriteLine(JsonSerializer.Serialize(Find(q)));}
            catch{Console.WriteLine("[]");}
        }return 0;
    }).GetAwaiter().GetResult();
    private static bool Contains(Rect r,ElementQuery q)=>!r.IsEmpty&&double.IsFinite(r.X)&&double.IsFinite(r.Y)&&double.IsFinite(r.Width)&&double.IsFinite(r.Height)&&r.Contains(q.X,q.Y);
    private static ElementCandidate? Read(AutomationElement element,ElementQuery q)
    {
        try{var info=element.Current;var r=info.BoundingRectangle;
        if(info.ProcessId==q.ExcludedProcess||info.IsOffscreen||!Contains(r,q)||r.Width>100000||r.Height>100000)return null;
        int x=(int)Math.Floor(r.X),y=(int)Math.Floor(r.Y);var bounds=new PixelRect(x,y,(int)Math.Ceiling(r.Right)-x,(int)Math.Ceiling(r.Bottom)-y);
        var type=info.ControlType;bool container=type==ControlType.Document||type==ControlType.Window||type==ControlType.Pane||type==ControlType.Group||type==ControlType.DataGrid||type==ControlType.Table||type==ControlType.List||type==ControlType.Tree;
        return new(bounds,string.IsNullOrWhiteSpace(info.Name)?info.LocalizedControlType:info.LocalizedControlType+" · "+info.Name,container);}catch(ElementNotAvailableException){return null;}catch(System.Runtime.InteropServices.COMException){return null;}
    }
    private static ElementCandidate[] Find(ElementQuery q)
    {
        var hwnd=(nint)q.Window;if(!Native.IsWindow(hwnd))return [];var timer=Stopwatch.StartNew();var walker=TreeWalker.RawViewWalker;
        var chain=new List<ElementCandidate>();var element=AutomationElement.FromPoint(new(q.X,q.Y));if(element?.Current.ProcessId==q.ExcludedProcess)element=null;
        for(int i=0;element!=null&&i<28&&timer.ElapsedMilliseconds<500;i++){
            var c=Read(element,q);if(c!=null)chain.Add(c);
            if(element.Current.NativeWindowHandle==(int)q.Window)return chain.ToArray();element=walker.GetParent(element);
        }
        // A highlight window or incomplete provider can intercept FromPoint.
        // Descend the target root by bounding boxes with a strict node budget.
        chain.Clear();element=AutomationElement.FromHandle(hwnd);int budget=450;
        for(int depth=0;element!=null&&depth<24&&budget>0&&timer.ElapsedMilliseconds<500;depth++){
            var c=Read(element,q);if(c!=null)chain.Add(c);AutomationElement? next=null;long smallest=long.MaxValue;
            for(var child=walker.GetFirstChild(element);child!=null&&budget-->0&&timer.ElapsedMilliseconds<500;child=walker.GetNextSibling(child))if(Read(child,q) is {} candidate){long area=(long)candidate.Bounds.Width*candidate.Bounds.Height;if(area<smallest){smallest=area;next=child;}}
            element=next;
        }chain.Reverse();return chain.ToArray();
    }
}
