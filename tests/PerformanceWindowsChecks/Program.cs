using Captovix;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
internal static class Program
{
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);Console.WriteLine("PASS "+text);}
 [STAThread]static int Main(){try{
 var samples=new List<object>();
 StaImageWorker.Run(()=>{if(System.Threading.Thread.CurrentThread.GetApartmentState()!=System.Threading.ApartmentState.STA)throw new Exception("Offscreen worker must be STA");var drawing=new DrawingVisual();using(var dc=drawing.RenderOpen())dc.DrawRectangle(Brushes.White,null,new Rect(0,0,16,16));var raster=new RenderTargetBitmap(16,16,96,96,PixelFormats.Pbgra32);raster.Render(drawing);}).GetAwaiter().GetResult();Check(true,"Offscreen WPF render completes on background STA without waiting for UI");
 var image=ImageTransfer.Bitmap(800,600,Enumerable.Repeat((byte)255,800*600*4).ToArray());
 var surface=new EditorSurface();var doc=new EditorDocument{OriginalWidth=800,OriginalHeight=600};var session=new EditorSession(doc);surface.Load(image,session);surface.Tool=EditorTool.Callout;
 surface.PointerDown(new(40,300));surface.PointerMove(new(180,200));var a=surface.Selected!;Check(a.TailX==40&&a.TailY==300&&a.Width==122&&a.Height==82,"Callout starts at fixed tail and resizes during creation");surface.PointerMove(new(230,160));Check(a.Width==172&&a.Height==122&&a.TailX==40&&a.TailY==300,"Diagonal drag resizes both axes without moving tail");surface.PointerUp(new(230,160));Check(a.Width==172&&a.Height==122,"Release keeps empty drawn dimensions");Check(surface.Tool==EditorTool.Callout,"Drawing retains callout tool");
 a.Text=new string('a',20000);a.AutoFitText=true;EditorRenderer.FitText(a,new(0,0,800,600),true);surface.Refresh();
 void Paint(){surface.Measure(new Size(1200,900));surface.Arrange(new Rect(0,0,surface.DesiredSize.Width,surface.DesiredSize.Height));var target=new RenderTargetBitmap(1200,900,96,96,PixelFormats.Pbgra32);target.Render(surface);}
 Paint();long builds=surface.SceneBuilds;var watch=Stopwatch.StartNew();for(int i=0;i<30;i++){surface.SetZoom(.25+i*.01);Paint();}watch.Stop();samples.Add(new{scenario="30 WPF zoom frames,20k-char callout",ms=watch.Elapsed.TotalMilliseconds});Check(surface.SceneBuilds==builds,"Zoom reuses unchanged scene drawing");
 a.Text="Edited";surface.Refresh();Paint();Check(surface.SceneBuilds==builds+1,"Edit invalidates retained scene");
 surface.Load(image,new(new(){OriginalWidth=800,OriginalHeight=600}));Check(surface.Tool==EditorTool.Callout,"Image replacement retains chosen tool");
 var png=RasterPixels.Encode(image);RasterPixels.Decode(png);Check(RasterPixels.CacheStats.Count>0,"Decoded image cache populated");surface.Load(null,null);Check(RasterPixels.CacheStats.Count==0&&surface.Original==null&&surface.Session==null,"Closing image releases decoded cache and session");
 long first=GC.GetTotalMemory(true);for(int i=0;i<100;i++){surface.Load(image,new(new(){OriginalWidth=800,OriginalHeight=600}));RasterPixels.Decode(png);surface.Load(null,null);}long last=GC.GetTotalMemory(true);Check(RasterPixels.CacheStats.Count==0,"100 open/close cycles retain no decoded cache entries");samples.Add(new{scenario="100 lifecycle cycles, managed bytes after collection",before=first,after=last});
 Console.WriteLine(JsonSerializer.Serialize(new{version=VersionInfo.Current,os=Environment.OSVersion.ToString(),samples},new JsonSerializerOptions{WriteIndented=true}));return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
