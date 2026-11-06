using System.Xml.Linq;
using System.IO.Compression;
DirectoryInfo? current=new(AppContext.BaseDirectory);while(current!=null&&!File.Exists(Path.Combine(current.FullName,"src/Captovix/App.xaml")))current=current.Parent;
var root=current?.FullName??throw new Exception("Source root missing");XNamespace w="http://schemas.microsoft.com/winfx/2006/xaml/presentation",x="http://schemas.microsoft.com/winfx/2006/xaml";int count=0;
void Check(bool value,string title){if(!value)throw new Exception(title);count++;Console.WriteLine("PASS: "+title);}
string Key(XElement e){var value=(string?)e.Attribute(x+"Key");if(value!=null)return value.StartsWith("{x:Type ")?"type:"+value[8..^1]:"name:"+value;return e.Name==w+"Style"&&e.Attribute("TargetType") is {} t?"type:"+(t.Value.StartsWith("{x:Type ")?t.Value[8..^1]:t.Value):e.Name==w+"DataTemplate"&&e.Attribute("DataType") is {} d?"data:"+d.Value:"";}
string[] Duplicates(XDocument doc)=>doc.Root!.DescendantsAndSelf().Where(e=>e.Name==w+"ResourceDictionary"||e.Name.LocalName.EndsWith(".Resources",StringComparison.Ordinal)).SelectMany(scope=>scope.Elements().Select(Key).Where(k=>k.Length>0).GroupBy(k=>k).Where(g=>g.Count()>1).Select(g=>g.Key)).ToArray();
foreach(var file in Directory.EnumerateFiles(Path.Combine(root,"src/Captovix"),"*.xaml")){var doc=XDocument.Load(file);var dup=Duplicates(doc);Check(dup.Length==0,"Resource keys unique: "+Path.GetFileName(file)+(dup.Length>0?" · "+string.Join(",",dup):""));}
string Open(string body)=>"<ResourceDictionary xmlns='"+w+"' xmlns:x='"+x+"'>"+body+"</ResourceDictionary>";
Check(Duplicates(XDocument.Parse(Open("<Style TargetType='TreeViewItem'/><Style TargetType='TreeViewItem'/>"))).SequenceEqual(["type:TreeViewItem"]),"Guard rejects exact V1.41 duplicate implicit style");
Check(Duplicates(XDocument.Parse(Open("<Style TargetType='TreeViewItem'/><Style x:Key='{x:Type TreeViewItem}' TargetType='TreeViewItem'/>"))).Length==1,"Guard resolves explicit type keys against implicit styles");
Check(Duplicates(XDocument.Parse(Open("<Style x:Key='A' TargetType='TreeViewItem'/><Style x:Key='B' TargetType='TreeViewItem'/>"))).Length==0,"Named styles for same type remain valid");
Check(Duplicates(XDocument.Parse(Open("<Style TargetType='TreeViewItem'/><ResourceDictionary.MergedDictionaries><ResourceDictionary><Style TargetType='TreeViewItem'/></ResourceDictionary></ResourceDictionary.MergedDictionaries>"))).Length==0,"Separate dictionaries may override the same key");
Check(Duplicates(XDocument.Parse(Open("<Style x:Key='A'/><Style x:Key='A'/>"))).Length==1,"Duplicate explicit resource names are rejected");
if(args.Length>0){using var z=ZipFile.OpenRead(args[0]);using var reader=new StreamReader(z.GetEntry(z.Entries.Single(e=>e.FullName.EndsWith("/src/Captovix/Themes.xaml")).FullName)!.Open());Check(Duplicates(XDocument.Parse(reader.ReadToEnd())).Contains("type:TreeViewItem"),"Supplied V1.41 source reproduces duplicate TreeViewItem startup defect");}
var popup=File.ReadAllText(Path.Combine(root,"src/Captovix/PopupPresentation.cs"));int detach=popup.IndexOf("if(ReferenceEquals(w.Content,root))w.Content=null;",StringComparison.Ordinal);int parent=popup.IndexOf("new Border{Child=original",StringComparison.Ordinal);Check(detach>=0&&detach<parent,"Popup content is detached before assigning new logical parent");
var entry=File.ReadAllText(Path.Combine(root,"src/Captovix/Program.cs"));Check(entry.IndexOf(".Run();",StringComparison.Ordinal)<entry.IndexOf("--ocr-worker",StringComparison.Ordinal)&&entry.IndexOf("--ocr-worker",StringComparison.Ordinal)<entry.IndexOf("var app = new App();",StringComparison.Ordinal),"Updater and worker routing still precede WPF startup");
Check(entry.Contains("App.Log(new InvalidOperationException")&&entry.Contains("System.Windows.MessageBox.Show")&&entry.Contains("Environment.ExitCode=1"),"Fatal startup errors log and show native fallback without loading broken theme");
Check(popup.Contains("child is ContextMenu or ToolTip"),"Automatic context menu and tooltip popup roots are excluded from reparenting");
Check(entry.IndexOf("--hybrid-ui-probe",StringComparison.Ordinal)<entry.IndexOf("var app = new App();",StringComparison.Ordinal),"Hybrid UI probe runs before WPF and single-instance startup");
var recovery=File.ReadAllText(Path.Combine(root,"src/Captovix.Recovery/RecoveryWindow.cs"));
Check(entry.Contains("StartupHealthSignal.Probe != null || StartupRecoveryLauncher.Open"),"Probe failure does not reopen the Recovery Center");
Check(recovery.IndexOf("if (ready.WaitOne(0))",StringComparison.Ordinal)<recovery.IndexOf("Đã khôi phục.",StringComparison.Ordinal)&&recovery.Contains("if (app.HasExited)"),"Recovery requires a startup acknowledgement from a live process");
var mainWindow=File.ReadAllText(Path.Combine(root,"src","Captovix","MainWindow.xaml.cs"));
Check(mainWindow.Contains("!_startupInitialized||!_firstFrameRendered||!IsVisible||WindowState==WindowState.Minimized")
    &&recovery.Contains("!IsWindowVisible(app.MainWindowHandle)||IsIconic(app.MainWindowHandle)"),
    "Recovery success requires initialized, rendered and visible editor");
Check(recovery.IndexOf("UpdateBackup.Create",StringComparison.Ordinal)<recovery.IndexOf("backend.VerifyReadyPackage",StringComparison.Ordinal)&&recovery.Contains("updater.ExitCode != 0"),"Recovery backs up, re-verifies package, and checks updater exit");
Console.WriteLine(count+" startup/resource regression checks passed. Native WPF loading requires Windows.");
