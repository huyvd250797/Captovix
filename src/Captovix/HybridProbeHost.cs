using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Automation;
namespace Captovix;
public static class HybridProbeHost
{
    public static int Run()=>Task.Run(()=>{
        using var watchdog=new Timer(_=>Environment.Exit(0),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
        try{string? line=Console.ReadLine();if(line==null||line.Length>100_000)return 1;var q=JsonSerializer.Deserialize<HybridQuery>(line);if(q==null)return 1;Console.WriteLine(JsonSerializer.Serialize(Read(q)));return 0;}catch{Console.WriteLine("[]");return 0;}
    }).GetAwaiter().GetResult();
    private static HybridNode[] Read(HybridQuery query){
        nint previous=SetThreadDpiAwarenessContext(new(-4));
        try{
            var watch=Stopwatch.StartNew();var result=new List<HybridNode>();var walker=TreeWalker.ControlViewWalker;int visited=0;
            foreach(var window in query.Windows.Where(w=>w.ProcessId!=query.ExcludedProcess&&(query.TargetWindow==0||query.TargetWindow==w.Handle)).Take(8)){
                if(watch.ElapsedMilliseconds>700||result.Count>=HybridCapture.MaxNodes)break;
                uint dpi=HybridCapture.RenderDpi(window.Bounds,query.Displays,GetDpiForWindow((nint)window.Handle),GetAwarenessFromDpiAwarenessContext(GetWindowDpiAwarenessContext((nint)window.Handle))==2);
                var todo=new Queue<(AutomationElement Element,string? Parent,int Depth)>();todo.Enqueue((AutomationElement.FromHandle((nint)window.Handle),null,0));
                while(todo.TryDequeue(out var next)&&watch.ElapsedMilliseconds<700&&visited++<900&&result.Count<HybridCapture.MaxNodes){
                    try{
                        var info=next.Element.Current;if(info.IsOffscreen||info.IsPassword||info.ProcessId==query.ExcludedProcess)continue;
                        var rect=info.BoundingRectangle;if(rect.IsEmpty||!double.IsFinite(rect.X)||!double.IsFinite(rect.Y)||!double.IsFinite(rect.Width)||!double.IsFinite(rect.Height)||Math.Abs(rect.X)>1000000||Math.Abs(rect.Y)>1000000||rect.Width>32768||rect.Height>32768)continue;
                        var bounds=new PixelRect((int)Math.Floor(rect.X),(int)Math.Floor(rect.Y),(int)Math.Ceiling(rect.Right)-(int)Math.Floor(rect.X),(int)Math.Ceiling(rect.Bottom)-(int)Math.Floor(rect.Y));if(!HybridCapture.Valid(bounds))continue;
                        string id=Guid.NewGuid().ToString("N"),type=info.ControlType.ProgrammaticName.Replace("ControlType.",""),name=info.Name??"";if(name.Length>200)name=name[..200];
                        string font="";double points=0;bool bold=false,italic=false,underline=false,styleComplete=false;string? color=null;
                        // Never request ValuePattern or text content. Accessible names are
                        // only used after OCR agrees with visible pixels in the Editor.
                        if(type=="Text"&&next.Element.TryGetCurrentPattern(TextPattern.Pattern,out var value)&&value is TextPattern text){
                            var range=text.DocumentRange;var family=range.GetAttributeValue(TextPattern.FontNameAttribute);if(family is string f&&f.Length<=100)font=f;
                            if(range.GetAttributeValue(TextPattern.FontSizeAttribute) is double size&&double.IsFinite(size)&&size is >=4 and <=100)points=size;
                            var weightValue=range.GetAttributeValue(TextPattern.FontWeightAttribute);if(weightValue is int weight)bold=weight>=600;
                            var italicValue=range.GetAttributeValue(TextPattern.IsItalicAttribute);if(italicValue is bool slant)italic=slant;
                            var underlineValue=range.GetAttributeValue(TextPattern.UnderlineStyleAttribute);if(underlineValue is int line)underline=line!=0;
                            var ink=range.GetAttributeValue(TextPattern.ForegroundColorAttribute);if(ink is int rgb)color=$"#{rgb&255:X2}{rgb>>8&255:X2}{rgb>>16&255:X2}";
                            styleComplete=font.Length>0&&points>0&&weightValue is int&&italicValue is bool&&underlineValue is int&&color!=null;
                        }
                        result.Add(new(id,next.Parent,window.Handle,window.ProcessId,bounds,type,name,font,points,bold,dpi,"",italic,underline,color,styleComplete));
                        if(next.Depth>=16)continue;
                        for(var child=walker.GetFirstChild(next.Element);child!=null&&todo.Count<900&&watch.ElapsedMilliseconds<700;child=walker.GetNextSibling(child))todo.Enqueue((child,id,next.Depth+1));
                    }catch(ElementNotAvailableException){}catch(COMException){}catch(InvalidOperationException){}
                }
            }
            return result.ToArray();
        }finally{if(previous!=0)SetThreadDpiAwarenessContext(previous);}
    }
    [DllImport("user32.dll")]private static extern uint GetDpiForWindow(nint window);
    [DllImport("user32.dll")]private static extern nint GetWindowDpiAwarenessContext(nint window);
    [DllImport("user32.dll")]private static extern int GetAwarenessFromDpiAwarenessContext(nint context);
    [DllImport("user32.dll")]private static extern nint SetThreadDpiAwarenessContext(nint context);
}
