using Captovix;
using System.Xml.Linq;
int checks=0;
void Check(bool value,string message){if(!value)throw new Exception("FAIL: "+message);checks++;Console.WriteLine("PASS: "+message);}
void Reject(Action action,string message){try{action();}catch(Exception e)when(e is InvalidDataException or ArgumentException or OperationCanceledException){Check(true,message);return;}throw new Exception("FAIL: "+message);}
const int w=97,h=127,total=700;var random=new Random(114);var source=new byte[w*total*4];random.NextBytes(source);for(int i=3;i<source.Length;i+=4)source[i]=255;
ScrollFrame Frame(int start){var bytes=new byte[w*h*4];Buffer.BlockCopy(source,start*w*4,bytes,0,bytes.Length);return new(w,h,bytes);}
var first=Frame(0);
foreach(int shift in new[]{1,3,7,19,31,54,77,95,106}){var match=ScrollStitching.Match(first,Frame(shift));Check(match.Kind==ScrollMatchKind.Matched&&match.Overlap==h-shift,$"Exact downward shift {shift} aligns native odd-size pixels");}
var repeated=ScrollStitching.Match(first,first);Check(repeated.Kind==ScrollMatchKind.Duplicate,"Unchanged frame is rejected without appending");
var reverse=ScrollStitching.Match(Frame(70),Frame(0));Check(reverse.Kind==ScrollMatchKind.Uncertain,"Reverse scrolling is not silently accepted");
var unrelated=new byte[w*h*4];new Random(5).NextBytes(unrelated);Check(ScrollStitching.Match(first,new(w,h,unrelated)).Kind==ScrollMatchKind.Uncertain,"Unrelated frame requires explicit manual review");
var stripes=new byte[w*h*4];for(int y=0;y<h;y++)for(int x=0;x<w;x++)for(int c=0;c<4;c++)stripes[(y*w+x)*4+c]=(byte)(c==3?255:(y%11)*20);
var shifted=new byte[stripes.Length];for(int y=0;y<h;y++)for(int x=0;x<w;x++)for(int c=0;c<4;c++)shifted[(y*w+x)*4+c]=(byte)(c==3?255:((y+5)%11)*20);
Check(ScrollStitching.Match(new(w,h,stripes),new(w,h,shifted)).Kind==ScrollMatchKind.Uncertain,"Periodic content produces ambiguous join, not false certainty");
var blank=new byte[w*h*4];Check(ScrollStitching.Match(new(w,h,blank),new(w,h,blank)).Kind==ScrollMatchKind.Duplicate,"Blank repeated frame cannot fabricate new content");
int[] offsets=[0,54,113,180];int[] overlaps=offsets.Zip(offsets.Skip(1),(a,b)=>h-(b-a)).ToArray();var frames=offsets.Select(Frame).ToArray();var raw=ScrollStitching.Compose(w,h,overlaps,i=>frames[i]);
Check(raw.Length==(h+180)*w*4,"Four-frame output has exact predicted dimensions");Check(raw.SequenceEqual(source.Take(raw.Length)),"Every BGRA output byte matches unscaled source, no missing or repeated rows");
Check(frames[0].Pixels.SequenceEqual(first.Pixels),"Compose never mutates frame buffers");
Check(ScrollStitching.OutputHeight(w,h,Array.Empty<int>())==h,"Single-frame session remains usable");
var manual=ScrollStitching.Compose(w,h,new[]{50},i=>frames[i]);Check(manual.Length==(2*h-50)*w*4,"Manual overlap is honored");
Reject(()=>ScrollStitching.OutputHeight(w,h,new[]{0}),"Reject no-overlap assumption");Reject(()=>ScrollStitching.OutputHeight(w,h,new[]{h}),"Reject full duplicate overlap");Reject(()=>ScrollStitching.OutputHeight(w,h,new[]{-1}),"Reject negative overlap");Reject(()=>ScrollStitching.OutputHeight(w,h,Enumerable.Repeat(1,24).ToArray()),"Frame count is bounded");
Reject(()=>ScrollStitching.OutputHeight(2000,2000,Enumerable.Repeat(100,20).ToArray()),"Source/output pixel budget checked before allocation");Reject(()=>ScrollStitching.OutputHeight(10,10000,new[]{1,1,1}),"Output height is bounded");Reject(()=>ScrollStitching.OutputHeight(4000,2000,Array.Empty<int>()),"4MP per-frame limit");
Reject(()=>ScrollStitching.Match(first,new(w,h-1,new byte[w*(h-1)*4])),"Reject frame size changes");Reject(()=>ScrollStitching.Match(first,new(w,h,new byte[4])),"Reject incomplete buffers");Reject(()=>new ScrollFrame(7,h,new byte[7*h*4]).Validate(),"Reject narrow invalid region");Reject(()=>new ScrollFrame(w,31,new byte[w*31*4]).Validate(),"Reject too-short frame");
using(var cancel=new CancellationTokenSource()){cancel.Cancel();Reject(()=>ScrollStitching.Match(first,Frame(1),cancel.Token),"Cancellation before matching");Reject(()=>ScrollStitching.Compose(w,h,overlaps,i=>frames[i],cancel.Token),"Cancellation before composing");}
Reject(()=>ScrollStitching.Compose(w,h,new[]{50},i=>i==0?frames[0]:new(w,h-1,new byte[w*(h-1)*4])),"Compose validates every frame geometry");
Check(new AppearanceSettings().Mode==AppearanceMode.System&&new AppearanceSettings().Accent=="#7C3AED","Default uses System mode and approved violet");
foreach(var mode in Enum.GetValues<AppearanceMode>()){Check(AppearanceSettings.UseDark(mode,true)==(mode==AppearanceMode.Dark),$"{mode} with Windows light");Check(AppearanceSettings.UseDark(mode,false)==(mode!=AppearanceMode.Light),$"{mode} with Windows dark");}
foreach(var color in new[]{"#7C3AED","#2563eb","#008B92","#FFFFFF","#000000"}){new AppearanceSettings(Accent:color).Validate();Check(true,"Valid accent "+color);}
foreach(var color in new[]{"red","#fff","#FF00FF00","123456","#GG0000","",null!})Reject(()=>new AppearanceSettings(Accent:color).Validate(),"Invalid accent rejected: "+color);
Reject(()=>new AppearanceSettings(Mode:(AppearanceMode)99).Validate(),"Unknown mode rejected");Reject(()=>new AppearanceSettings(Schema:2).Validate(),"Future schema protected");
Check(AppearanceSettings.WhiteText("#7C3AED"),"Violet uses white button text");Check(!AppearanceSettings.WhiteText("#FFFFFF"),"White accent uses black text");Check(AppearanceSettings.WhiteText("#000000"),"Black accent uses white text");Check(!AppearanceSettings.WhiteText("#FFFF00"),"Bright yellow uses black text");
string root=Path.Combine(Path.GetTempPath(),"Captovix-114-check-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
try{
    var loaded=AppearanceSettings.Load(root,out var warning);Check(loaded==new AppearanceSettings()&&warning==null,"Missing settings uses safe defaults");
    var settings=new AppearanceSettings(AppearanceMode.Dark,"#123ABC");settings.Save(root);Check(AppearanceSettings.Load(root,out warning)==settings&&warning==null,"Mode and custom accent survive reload");
    var second=root+"-other-machine";try{Check(AppearanceSettings.Load(second,out _)!=settings,"Separate machine root has independent preference");}finally{if(Directory.Exists(second))Directory.Delete(second,true);}
    File.WriteAllText(Path.Combine(root,AppearanceSettings.FileName),"broken");loaded=AppearanceSettings.Load(root,out warning);Check(warning!=null&&loaded==new AppearanceSettings(),"Corrupt settings does not crash startup");Check(File.ReadAllText(Path.Combine(root,AppearanceSettings.FileName))=="broken","Load never overwrites corrupt settings");
    settings.Save(root);Check(Directory.EnumerateFiles(root,"*.backup-*").Count()==1,"Explicit edit preserves invalid old settings backup");Check(!Directory.EnumerateFiles(root,"*.tmp").Any(),"Atomic settings write cleans temporary files");
}finally{Directory.Delete(root,true);}
DirectoryInfo? project=new(AppContext.BaseDirectory);while(project!=null&&!File.Exists(Path.Combine(project.FullName,"src/Captovix/MainWindow.xaml")))project=project.Parent;if(project==null)throw new Exception("Project root not found.");
string Text(string name)=>File.ReadAllText(Path.Combine(project.FullName,"src/Captovix",name));var xaml=XDocument.Parse(Text("MainWindow.xaml"));XNamespace wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation";
Check(xaml.Descendants(wpf+"MenuItem").Any(x=>x.Attribute("Click")?.Value=="ScrollCapture_Click"),"Scrolling command integrated in capture menu");Check(xaml.Descendants(wpf+"MenuItem").Any(x=>x.Attribute("Click")?.Value=="RecoverScroll_Click"),"Session recovery discoverable in menu");Check(xaml.Descendants(wpf+"MenuItem").Any(x=>x.Attribute("Tag")?.Value=="Appearance"),"Appearance settings reachable from menu");
Check(Text("MainWindow.xaml").Contains("DynamicResource BrandMark"),"Header uses theme-aware approved mark");Check(Text("App.xaml.cs").Contains("AppearanceManager.Initialize();"),"Appearance loads before main window startup");Check(Text("AppearanceManager.cs").Contains("UserPreferenceChanged+=Changed")&&Text("AppearanceManager.cs").Contains("UserPreferenceChanged-=Changed"),"System listener registered and removed");Check(Text("AppearanceManager.cs").Contains("Current.Mode==AppearanceMode.System"),"System changes cannot override explicit Light/Dark mode");
Check(Text("UpdateBackup.cs").Contains("appearance-settings.json"),"Updater backs up personal appearance");Check(Text("MainWindow.ScrollingCapture.cs").Contains("Hide();await Task.Delay(350"),"Controller hides before capturing frame");Check(Text("MainWindow.ScrollingCapture.cs").Contains("topology!=_session.Topology"),"Display/DPI topology changes stop capture");Check(Text("MainWindow.ScrollingCapture.cs").Contains("j.Uncertain&&!j.Confirmed"),"Uncertain seams gate save until reviewed");
var themes=XDocument.Parse(Text("Themes.xaml"));Check(themes.Descendants(wpf+"Style").Any(s=>s.Attribute("TargetType")?.Value=="ComboBox"&&s.Descendants(wpf+"Popup").Any()),"Themed ComboBox has dark-compatible dropdown template");Check(!Text("AppearanceManager.cs").Contains("EditorRenderer"),"Appearance never touches authored image renderer");
Console.WriteLine($"{checks} scrolling/appearance checks passed. Windows runtime validation remains pending.");
