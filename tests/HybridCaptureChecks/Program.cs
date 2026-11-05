using Captovix;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
if(args.Length>0&&args[0]=="--fake"){
    Console.ReadLine();if(args[1]=="stall")await Task.Delay(10000);else if(args[1]=="bad")Console.WriteLine("bad-json");else if(args[1]=="ok")Console.WriteLine(JsonSerializer.Serialize(new[]{new HybridNode("test",null,1,1,new(0,0,10,10),"Button")}));else Console.WriteLine("[]");return;
}
int count=0;void Check(bool condition,string name){if(!condition)throw new Exception(name);count++;Console.WriteLine("PASS: "+name);}
const int width=640,height=360;var pixels=Enumerable.Repeat((byte)255,width*height*4).ToArray();
void Paint(PixelRect b,byte value){for(int y=b.Y;y<b.Bottom;y++)for(int x=b.X;x<b.Right;x++){int p=(y*width+x)*4;pixels[p]=value;pixels[p+1]=70;pixels[p+2]=40;}}
Paint(new(180,100,180,40),180);var frame=new PixelRect(-1920,-200,width,height);var bounds=new PixelRect(-1740,-100,180,40);var stamp=DateTimeOffset.UtcNow;var owner=new HybridWindow(1,42,frame);var node=new HybridNode("ui1",null,1,42,bounds,"Button","Save");
var draft=new HybridEvidence(1,stamp,stamp,"topology",frame,[node]);var stable=HybridCapture.Verify(draft,pixels,pixels,[owner],[owner],"topology",stamp.AddMilliseconds(300));
Check(stable.Nodes.Length==1&&stable.Nodes[0].PixelHash.Length==64,"Stable UI rectangle is bound to exact capture pixels");Check(HybridCapture.ToImage(bounds,frame)==new PixelRect(180,100,180,40),"Negative monitor origin translates once into screenshot pixels");Check(HybridCapture.FontPixels(node with{FontPoints=12,DpiY=144})==24,"Font points convert to physical pixels at 150 percent DPI");Check(HybridCapture.FontPixels(node with{FontPoints=12,DpiY=192})==32,"Font points convert at 200 percent without scaling rectangle twice");
var mixed=new[]{new HybridDisplay(new(-1920,0,1920,1080),96,96),new HybridDisplay(new(0,0,2560,1440),144,144)};
Check(HybridCapture.RenderDpi(new(-100,100,800,600),mixed,144,true)==144,"Per-monitor-aware window DPI is used even when child crosses another monitor");
Check(HybridCapture.RenderDpi(new(-100,100,800,600),mixed,96,false)==144,"Legacy DPI-unaware window uses dominant monitor scaling for physical font size");
var changed=pixels.ToArray();changed[(100*width+180)*4]++;
Check(HybridCapture.Verify(draft,pixels,changed,[owner],[owner],"topology",stamp).Nodes.Length==0,"Any changed pixel in the UI patch rejects live metadata");
Check(HybridCapture.Verify(draft,pixels,pixels,[owner],[owner],"new-topology",stamp).Nodes.Length==0,"Monitor or DPI topology change rejects metadata");
Check(HybridCapture.Verify(draft,pixels,pixels,[owner],[owner],"topology",stamp.AddSeconds(4)).Nodes.Length==0,"Late UI result cannot be attached to a screenshot");
Check(HybridCapture.Verify(draft,pixels,pixels,[owner],[owner with{Bounds=frame with{X=-1910}}],"topology",stamp).Nodes.Length==0,"Moved window rejects old UI geometry");
var overlay=new HybridWindow(2,90,bounds);Check(HybridCapture.Verify(draft,pixels,pixels,[overlay,owner],[overlay,owner],"topology",stamp).Nodes.Length==0,"Occluded controls are excluded even when hidden pixels are stable");
Check(HybridCapture.Verify(draft,pixels,pixels,[owner],[owner with{ProcessId=43}],"topology",stamp).Nodes.Length==0,"Reused HWND from another process cannot inherit capture metadata");
var crop=new PixelRect(-1770,-130,240,100);var cropped=new byte[240*100*4];for(int y=0;y<100;y++)Array.Copy(pixels,((y+70)*width+150)*4,cropped,y*240*4,240*4);
var bound=HybridCapture.Bind(stable,crop,cropped,240,100,stamp.AddMilliseconds(400));Check(bound?.Nodes.Length==1&&HybridCapture.ToImage(bound.Nodes[0].Bounds,crop)==new PixelRect(30,30,180,40),"Crop maps physical UI bounds to the correct local image coordinates");
Check(HybridCapture.Bind(stable,crop,cropped,241,100,stamp)==null,"Native window output size mismatch falls back to image scan");
Check(HybridCapture.CropRetained(stable,crop)?.Nodes.Length==1,"Retained screenshot crops reuse verified metadata without copying the full raster");
var partial=HybridCapture.CropRetained(stable,new(-1700,-100,100,40));Check(partial?.Nodes.Length==0,"Partially cropped controls are not promoted into whole controls");
var lines=new[]{new OcrTextLine(new(215,110,60,18),"Save",.94f)};var scan=ElementScan.Analyze(pixels,width,height,new(0,0,width,height),lines);var fused=HybridFusion.Merge(scan,stable,pixels,width,height,new(0,0,width,height));Check(fused.Matched==1&&fused.Scan.Elements.Any(e=>e.Kind==MockupKind.Button&&e.Bounds==new PixelRect(180,100,180,40)),"UI semantics refine the image control at verified screenshot coordinates");
var redacted=Enumerable.Repeat((byte)0,pixels.Length).ToArray();Check(HybridFusion.Merge(ElementScan.Analyze(redacted,width,height,new(0,0,width,height),[]),stable,redacted,width,height,new(0,0,width,height)).Matched==0,"Redaction or edited rendered pixels invalidate metadata before OCR fusion");
Check(HybridFusion.Merge(ElementScan.Analyze(pixels,width,height,new(0,0,width,height),[]),null,pixels,width,height,new(0,0,width,height)).Scan.Elements.Count>0,"Images without UI data retain normal image-only detection");
var textPixels=Enumerable.Repeat((byte)255,width*height*4).ToArray();var textUi=new HybridNode("text1",null,1,42,new(-1892,-172,104,24),"Text","Visible","Tahoma",12,true,144);
textUi=textUi with{PixelHash=HybridCapture.PatchHash(textPixels,width,height,new(28,28,104,24))};var textEvidence=stable with{Nodes=[textUi]};var textCandidate=new ScanElement{Bounds=new(28,28,104,24),Kind=MockupKind.Label,Text="Visible",Confidence=.8};
var fontFusion=HybridFusion.Merge(new([textCandidate],false),textEvidence,textPixels,width,height,new(0,0,width,height));Check(fontFusion.Matched==1&&textCandidate.FontFamily=="Tahoma"&&textCandidate.FontSize==24&&textCandidate.FontBold,"Visible OCR agreement enables UI font and DPI conversion");
var hidden=textUi with{Name="Secret not on screenshot"};var hiddenCandidate=new ScanElement{Bounds=textCandidate.Bounds,Kind=MockupKind.Label,Text="Visible"};Check(HybridFusion.Merge(new([hiddenCandidate],false),textEvidence with{Nodes=[hidden]},textPixels,width,height,new(0,0,width,height)).Matched==0&&hiddenCandidate.Text=="Visible","Unmatched accessible names never inject text missing from screenshot/OCR");
var folder=Path.Combine(Path.GetTempPath(),"CaptovixHybrid-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);try{
    string source=Path.Combine(folder,"source.png"),id=Guid.NewGuid().ToString("N");File.WriteAllBytes(source,[1,2,3]);HybridCapture.Save(folder,id,source,stable);Check(HybridCapture.Load(folder,id,source,width,height)?.Nodes.Length==1,"Capture UI sidecar persists independently from editor schema");File.WriteAllBytes(source,[4,5,6]);Check(HybridCapture.Load(folder,id,source,width,height)==null,"Replacing original pixels invalidates sidecar identity");Check(HybridCapture.MetadataPath(folder,"../../outside")==null,"Invalid image IDs cannot escape metadata directory");File.WriteAllText(HybridCapture.MetadataPath(folder,id)!,"invalid");Check(HybridCapture.Load(folder,id,source,width,height)==null,"Corrupt sidecar falls back without blocking image open");
}finally{Directory.Delete(folder,true);}
var q=new HybridQuery([owner],[],0);string exe=Environment.ProcessPath!;var prefix=Path.GetFileNameWithoutExtension(exe)=="dotnet"?new[]{Assembly.GetExecutingAssembly().Location}:Array.Empty<string>();
HybridProbeClient Fake(string mode)=>new(exe,prefix.Concat(new[]{"--fake",mode}).ToArray());
Check((await Fake("ok").QueryAsync(q,default)).Length==1,"Real helper subprocess protocol returns structured UI nodes");
var timer=Stopwatch.StartNew();Check((await Fake("stall").QueryAsync(q,default)).Length==0&&timer.Elapsed<TimeSpan.FromSeconds(4),"Stalled provider helper is killed within bounded timeout");Check((await Fake("bad").QueryAsync(q,default)).Length==0,"Malformed helper output falls back safely");Check((await Fake("empty").QueryAsync(q,default)).Length==0,"Unsupported application produces normal image-only fallback");
using(var stop=new CancellationTokenSource()){stop.Cancel();Check((await Fake("stall").QueryAsync(q,stop.Token)).Length==0,"Cancellation terminates pending UI probe");}
Console.WriteLine($"{count} Hybrid capture/fusion/process checks passed.");
