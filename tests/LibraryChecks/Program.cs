using Captovix;
using System.IO.Compression;
using System.Text.Json;
int n=0;void Check(bool b,string s){if(!b)throw new Exception(s);n++;Console.WriteLine("PASS: "+s);}void Reject(Action a,string s){try{a();}catch{Check(true,s);return;}throw new Exception("Accepted: "+s);}
string root=Path.Combine(Path.GetTempPath(),"Captovix-library-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
try{
string source=Path.Combine(root,"original.png"),edit=Path.Combine(root,"edit.json"),zip=Path.Combine(root,"sample.cvlibrary");File.WriteAllBytes(source,Enumerable.Range(0,65536).Select(i=>(byte)i).ToArray());File.WriteAllText(edit,"{\"Version\":12,\"Layers\":[\"embedded image\"]}");var key=Guid.NewGuid().ToString("N");var asset=new LibraryAsset(key,".png","{\"Title\":\"Tiếng Việt\",\"Tags\":[\"test\"]}",source,edit);
LibraryArchive.Save(zip,[asset]);var imported=LibraryArchive.Read(zip,Path.Combine(root,"stage"));Check(imported.Count==1,"Read exported library");Check(File.ReadAllBytes(imported[0].Source).SequenceEqual(File.ReadAllBytes(source)),"Original pixel file bytes preserved exactly");Check(File.ReadAllText(imported[0].Edit!)==File.ReadAllText(edit),"All sidecar embedded layers preserved");Check(imported[0].Metadata==asset.Metadata,"Unicode metadata round trip");Check(imported[0].Source.StartsWith(root)&&!imported[0].Source.Equals(source),"Import stages separately from original");
Check(LibraryArchive.Tags(" alpha, BETA,alpha , ảnh ").SequenceEqual(new[]{"alpha","BETA","ảnh"}),"Tags trim/deduplicate without losing Unicode");Reject(()=>LibraryArchive.Tags(new string('x',61)),"Reject excessive tag length");
Reject(()=>LibraryArchive.Save(zip,[asset,asset]),"Reject duplicate keys");Check(LibraryArchive.Read(zip,Path.Combine(root,"still-valid")).Count==1,"Failed save leaves existing archive usable");
Reject(()=>LibraryArchive.Save(zip,[asset with{Key="../escape"}]),"Reject traversal key");Reject(()=>LibraryArchive.Save(zip,[asset with{Extension=".exe"}]),"Reject executable extension");Reject(()=>LibraryArchive.Save(zip,[asset with{Source=Path.Combine(root,"missing")}]),"Missing source fails explicitly");Check(!Directory.EnumerateFiles(root,"*.tmp").Any(),"Failed saves clean staging archive");
void Mutate(string name,Action<ZipArchive> action){File.Copy(zip,Path.Combine(root,name),true);using var z=ZipFile.Open(Path.Combine(root,name),ZipArchiveMode.Update);action(z);}
Mutate("extra.zip",z=>{using var w=new StreamWriter(z.CreateEntry("../escape.txt").Open());w.Write("bad");});Reject(()=>LibraryArchive.Read(Path.Combine(root,"extra.zip"),Path.Combine(root,"extra")),"Reject unlisted traversal entry");Check(!File.Exists(Path.Combine(root,"escape.txt")),"No file escaped extraction");Check(!Directory.Exists(Path.Combine(root,"extra")),"Failed import removes staging tree");
Mutate("hash.zip",z=>{var name="media/"+key+".png";z.GetEntry(name)!.Delete();using var w=new StreamWriter(z.CreateEntry(name).Open());w.Write("changed");});Reject(()=>LibraryArchive.Read(Path.Combine(root,"hash.zip"),Path.Combine(root,"hash")),"Reject corrupted media hash");
Mutate("edit.zip",z=>{var name="edits/"+key+".json";z.GetEntry(name)!.Delete();using var w=new StreamWriter(z.CreateEntry(name).Open());w.Write("changed");});Reject(()=>LibraryArchive.Read(Path.Combine(root,"edit.zip"),Path.Combine(root,"bad-edit")),"Reject corrupted edit hash");
Mutate("version.zip",z=>{var e=z.GetEntry("manifest.json")!;LibraryManifest m;using(var stream=e.Open())m=JsonSerializer.Deserialize<LibraryManifest>(stream)!;e.Delete();using var w=new StreamWriter(z.CreateEntry("manifest.json").Open());w.Write(JsonSerializer.Serialize(m with{Version=4}));});Reject(()=>LibraryArchive.Read(Path.Combine(root,"version.zip"),Path.Combine(root,"future")),"Reject future archive schema without reset");
Mutate("duplicate.zip",z=>z.CreateEntry("manifest.json"));Reject(()=>LibraryArchive.Read(Path.Combine(root,"duplicate.zip"),Path.Combine(root,"duplicate")),"Reject duplicate zip entry");
Reject(()=>LibraryArchive.Read(zip,Path.Combine(root,"stage")),"Refuse nonempty staging destination");LibraryArchive.Save(Path.Combine(root,"empty.zip"),[]);Check(LibraryArchive.Read(Path.Combine(root,"empty.zip"),Path.Combine(root,"empty")).Count==0,"Empty library supported");
Reject(()=>LibraryArchive.Tags(string.Join(',',Enumerable.Range(0,33))),"Reject tag overflow without silent loss");
using(var cancelled=new CancellationTokenSource()){cancelled.Cancel();Reject(()=>LibraryArchive.Save(Path.Combine(root,"cancel.zip"),[asset],cancelled.Token),"Cancel save before writing source");Check(!File.Exists(Path.Combine(root,"cancel.zip")),"Cancelled save never commits archive");Reject(()=>LibraryArchive.Read(zip,Path.Combine(root,"cancel-import"),cancelled.Token),"Cancel import before committing files");Check(!Directory.Exists(Path.Combine(root,"cancel-import")),"Cancelled import cleans staging");}
var list=new[]{new ElementCandidate(new(120,120,80,30),"control"),new ElementCandidate(new(100,100,400,300),"modal",true),new ElementCandidate(new(0,0,1000,800),"browser",true)};
Check(ElementSelection.PreferredAt(list,105,105)==1,"Modal top-left selects whole modal");Check(ElementSelection.PreferredAt(list,300,105)==1,"Modal top edge selects container");Check(ElementSelection.PreferredAt(list,4,4)==2,"Browser top corner selects entire window");Check(ElementSelection.PreferredAt(list,130,130)==1,"Interior falls back to smallest semantic container");Check(ElementSelection.Cycle(1,1,3)==2,"Wheel keeps explicit parent override");
string mic=Path.Combine(root,"mic.wav"),system=Path.Combine(root,"system.wav"),av=Path.Combine(root,"audio.cvlibrary");
File.WriteAllBytes(mic,[1,2,3]);File.WriteAllBytes(system,[4,5]);
var audioAsset=asset with{Extension=".mp4",Edit=null,Mic=mic,SystemAudio=system};
LibraryArchive.Save(av,[audioAsset]);
var restored=LibraryArchive.Read(av,Path.Combine(root,"audio-stage")).Single();
Check(File.ReadAllBytes(restored.Mic!).SequenceEqual(File.ReadAllBytes(mic)),"Microphone sidecar survives backup/import byte-for-byte");
Check(File.ReadAllBytes(restored.SystemAudio!).SequenceEqual(File.ReadAllBytes(system)),"System sidecar survives backup/import byte-for-byte");
Reject(()=>LibraryArchive.Save(av,[audioAsset with{Extension=".png"}]),"Reject audio sidecars on still images");
var clicks=Path.Combine(root,"recording.clicks.json");
File.WriteAllText(clicks,"[{\"Kind\":\"left\",\"Milliseconds\":150}]");
var withClicks=Path.Combine(root,"presenter.cvlibrary");
LibraryArchive.Save(withClicks,[audioAsset with{RecordingClicks=clicks}]);
var presenterAsset=LibraryArchive.Read(withClicks,Path.Combine(root,"presenter-stage")).Single();
Check(File.ReadAllBytes(presenterAsset.RecordingClicks!).SequenceEqual(File.ReadAllBytes(clicks))&&presenterAsset.Mic!=null,"Presenter clicks and WAV survive backup/import");
Reject(()=>LibraryArchive.Save(withClicks,[asset with{RecordingClicks=clicks}]),"Reject recording clicks on still images");

using(var z=ZipFile.Open(av,ZipArchiveMode.Update)){
 string name="audio/"+key+".mic.wav";z.GetEntry(name)!.Delete();
 using var entry=z.CreateEntry(name).Open();entry.WriteByte(9);
}
Reject(()=>LibraryArchive.Read(av,Path.Combine(root,"broken-audio")),"Reject corrupted microphone hash");
Console.WriteLine($"{n} library/archive/edge selection checks passed.");
}finally{Directory.Delete(root,true);}
