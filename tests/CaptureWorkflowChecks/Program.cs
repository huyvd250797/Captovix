using Captovix;
using System.IO.Compression;
using System.Text.Json;
var count=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);count++;Console.WriteLine("PASS: "+name);}
void Reject(Action action,string name){bool rejected=false;try{action();}catch{rejected=true;}Check(rejected,name);}
var root=Path.Combine(Path.GetTempPath(),"Captovix-Workflow-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
try{
 var config=CaptureWorkflowStore.Load(root,out var warning);Check(warning==null&&!config.Background&&!config.AutoCopy&&!config.CloseToTray&&!config.StartWithWindows&&!config.StartInTray,"First install opens Editor, no autorun/close-to-tray");
 CaptureWorkflowStore.Validate(config);Check(config.Hotkeys.Count==6&&config.Presets.Count==3,"Six configurable photo actions and three presets");
 config.CloseToTray=true;config.Background=true;config.AutoCopy=true;config.StartWithWindows=true;config.StartInTray=true;config.Hotkeys[0]=new(PhotoAction.Current,true,3,0x41);
 var displays=new[]{new WorkflowDisplay("A",new(-1920,0,1920,1080),96,96,"monitorA"),new WorkflowDisplay("B",new(0,0,2560,1440),144,144,"monitorB")};
 config.LastRegion=new(new(-300,100,900,500),CaptureWorkflowStore.Topology(displays));CaptureWorkflowStore.Save(root,config);var loaded=CaptureWorkflowStore.Load(root,out warning);
 Check(warning==null&&loaded.CloseToTray&&loaded.Background&&loaded.AutoCopy&&loaded.StartWithWindows&&loaded.StartInTray,"Preferences survive restart");
 Check(loaded.Hotkeys[0]==config.Hotkeys[0]&&loaded.Presets.SequenceEqual(config.Presets),"Hotkeys and presets roundtrip");Check(loaded.LastRegion==config.LastRegion,"Physical region and topology roundtrip");
 Check(!JsonSerializer.Serialize(loaded.Presets).Contains("Handle"),"Presets never serialize an HWND");
 var clone=config.Clone();clone.Presets.Clear();Check(config.Presets.Count==3,"Candidate edits isolated until commit");
 Reject(()=>{var x=config.Clone();x.SchemaVersion=2;CaptureWorkflowStore.Save(root,x);},"Reject future schema");
 Reject(()=>{var x=config.Clone();x.Hotkeys.RemoveAt(0);CaptureWorkflowStore.Validate(x);},"Reject missing actions");
 Reject(()=>{var x=config.Clone();x.Hotkeys[1]=x.Hotkeys[0];CaptureWorkflowStore.Validate(x);},"Reject duplicate action identifiers");
 Reject(()=>{var x=config.Clone();x.Hotkeys[0]=new((PhotoAction)99,true,6,0x41);CaptureWorkflowStore.Validate(x);},"Reject unknown action");
 Reject(()=>{var x=config.Clone();x.Hotkeys[1]=x.Hotkeys[1] with{Modifiers=3,Key=0x41};CaptureWorkflowStore.Validate(x);},"Reject duplicate chords");
 foreach(var key in new uint[]{0x32,0x33,0x34})Reject(()=>{var x=config.Clone();x.Hotkeys[0]=new(PhotoAction.Current,true,6,key);CaptureWorkflowStore.Validate(x);},"Reserve recording chord "+key);
 foreach(var chord in new[]{(0u,0x41u),(8u,0x41u),(2u,0x7Bu),(2u,0x1Bu)})Reject(()=>{var x=config.Clone();x.Hotkeys[0]=new(PhotoAction.Current,true,chord.Item1,chord.Item2);CaptureWorkflowStore.Validate(x);},"Reject unsafe/unsupported chord "+chord);
 foreach(var key in new uint[]{0x70,0x87,0x2C}){var x=config.Clone();x.Hotkeys[0]=new(PhotoAction.Current,true,0,key);CaptureWorkflowStore.Validate(x);Check(true,"Allow function/PrintScreen "+key);}
 {var x=config.Clone();x.Hotkeys[0]=new(PhotoAction.Current,false,0,0);CaptureWorkflowStore.Validate(x);Check(true,"Disabled key can be empty");}
 foreach(var preset in new[]{new CapturePreset("x"," ",0,"",0,true,false,false),new("x",new string('a',41),0,"",0,true,false,false),new("x","New",4,"",0,true,false,false),new("x","New",0,"",2,true,false,false),new("x","HƯỚNG DẪN",0,"",0,true,false,false)})Reject(()=>{var x=config.Clone();x.Presets.Add(preset);CaptureWorkflowStore.Validate(x);},"Reject invalid/duplicate preset "+preset.Name);
 Reject(()=>{var x=config.Clone();x.Presets=Enumerable.Range(0,21).Select(i=>new CapturePreset(i.ToString(),i.ToString(),0,"",0,true,false,false)).ToList();CaptureWorkflowStore.Validate(x);},"Enforce preset limit");
 Check(CaptureWorkflowStore.CanRepeat(config.LastRegion,displays),"Repeat accepts unchanged multi-monitor physical coordinates");Check(CaptureWorkflowStore.Topology(displays)==CaptureWorkflowStore.Topology(displays.Reverse()),"Monitor enumeration order does not invalidate region");
 Check(!CaptureWorkflowStore.CanRepeat(null,displays),"No repeat before first successful region capture");
 foreach(var changed in new[]{displays.Take(1).ToArray(),new[]{displays[0],displays[1] with{DpiX=192}},new[]{displays[0],displays[1] with{Bounds=new(10,0,2560,1440)}},new[]{displays[0],displays[1] with{Identity="replacement"}}})Check(!CaptureWorkflowStore.CanRepeat(config.LastRegion,changed),"Invalidate repeat on monitor/DPI/topology change");
 Check(!CaptureWorkflowStore.CanRepeat(config.LastRegion with{Bounds=new(-9999,0,100,100)},displays),"Reject region outside desktop");Check(!CaptureWorkflowStore.CanRepeat(config.LastRegion with{Bounds=new(0,0,1,1)},displays),"Reject tiny saved region");
 var file=Path.Combine(root,CaptureWorkflowStore.FileName);File.WriteAllText(file,"{broken");CaptureWorkflowStore.Load(root,out warning);Check(warning!=null&&File.ReadAllText(file)=="{broken","Corrupt configuration retained for recovery");File.WriteAllText(file,"{\"SchemaVersion\":99}");CaptureWorkflowStore.Load(root,out warning);Check(warning!=null&&File.ReadAllText(file).Contains("99"),"Future configuration retained");
 Check(CaptureWorkflowStore.StartupCommand(@"D:\Captovix Test\current\Captovix.exe")=="\"D:\\Captovix Test\\current\\Captovix.exe\" --captovix-autostart","Autorun quotes path with spaces and no capture argument");
 Reject(()=>CaptureWorkflowStore.StartupCommand("bad\"path"),"Reject injected startup argument");Reject(()=>CaptureWorkflowStore.StartupCommand(new string('a',250)),"Reject overlong Run command");
 var active=new Dictionary<int,(uint,uint)>();var blocked=new HashSet<uint>();bool Register(int id,uint m,uint k){if(blocked.Contains(k)||active.ContainsKey(id))return false;active[id]=(m,k);return true;}
 var broker=new CaptureHotkeyRegistry(Register,id=>active.Remove(id));Check(broker.Start(config.Hotkeys).Count==0&&active.Count==6,"Register six actions");
 var next=config.Hotkeys.Select(h=>h.Action==PhotoAction.Current?h with{Key=0x42}:h).ToList();blocked.Add(0x42);Reject(()=>broker.Replace(next),"External conflict rejects transaction");Check(active.Count==6&&active[1].Item2==0x41,"Conflict restores previous live bindings");blocked.Clear();broker.Replace(next);Check(active.Count==6&&active[1].Item2==0x42,"Successful rebind replaces old chord");
 broker.Stop();Check(active.Count==0,"Exit/update releases every photo hotkey");blocked.Add(0x42);Check(broker.Start(next).Count==1&&active.Count==5,"Startup conflict retains other available actions");broker.Stop();
 var gate=new CaptureGate();var winners=0;Parallel.For(0,1000,_=>{if(gate.TryEnter())Interlocked.Increment(ref winners);});Check(winners==1,"Repeated capture requests acquire one owner");gate.Exit();Check(gate.TryEnter(),"Gate reusable after cancelled/finished capture");gate.Exit();
 CaptureWorkflowStore.Save(root,config);var backup=UpdateBackup.Create(root,"1.4.0","1.5.0");using(var zip=ZipFile.OpenRead(backup))Check(zip.GetEntry(CaptureWorkflowStore.FileName)!=null,"Update backup includes capture settings, keys and presets");
 Check(Directory.GetFiles(root,"*.tmp").Length==0,"Atomic saves leave no temporary files");
 Console.WriteLine($"{count} capture workflow checks passed.");
}finally{Directory.Delete(root,true);}
