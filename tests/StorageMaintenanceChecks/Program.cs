using Captovix;
var root=Path.Combine(Path.GetTempPath(),"Captovix-storage-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);int n=0;
void Check(bool ok,string text){if(!ok)throw new Exception(text);n++;Console.WriteLine("PASS: "+text);}
string Make(string relative,string value="data",bool old=true){var path=Path.Combine(root,relative);Directory.CreateDirectory(Path.GetDirectoryName(path)!);File.WriteAllText(path,value);if(old)File.SetLastWriteTimeUtc(path,DateTime.UtcNow.AddDays(-3));return path;}
try{
var original=Make("Media/original.png");var edit=Make("Edits/sidecar.json");var key=Make("Licensing/certificate.bin");var thumb=Make("Thumbnails/old.png");var recent=Make("Temp/new.tmp",old:false);var changed=Make("Cache/change.dat");
var originals=StorageMaintenance.Originals([original,original]);Check(originals.Files.Count==1&&originals.Bytes==4&&!originals.CanClean,"Originals counted once and protected");
var thumbnails=StorageMaintenance.Inspect("Thumb",Path.Combine(root,"Thumbnails"),true);var cache=StorageMaintenance.Inspect("Cache",Path.Combine(root,"Cache"),true);var temp=StorageMaintenance.Inspect("Temp",Path.Combine(root,"Temp"),true);
File.WriteAllText(changed,"changed after inspection");var late=Make("Thumbnails/new-after-scan.png");
var forged=new StorageCategory("forged",true,[new(original,4,File.GetLastWriteTimeUtc(original)),new(edit,4,File.GetLastWriteTimeUtc(edit)),new(key,4,File.GetLastWriteTimeUtc(key))]);
var result=StorageMaintenance.Clean(root,[thumbnails,cache,temp,forged],DateTime.UtcNow.AddDays(-1));
Check(result.Removed==1&&result.Bytes==4&&!File.Exists(thumb),"Only inspected stale thumbnail is deleted with exact reclaimed bytes");
Check(File.Exists(original)&&File.Exists(edit)&&File.Exists(key),"Even forged cleanup categories cannot remove media, edits or license");Check(File.Exists(recent)&&File.Exists(changed)&&File.Exists(late),"New, changed and uninspected files survive cleanup");
if(!OperatingSystem.IsWindows()){
var outside=Path.Combine(root,"outside");Directory.CreateDirectory(outside);File.WriteAllText(Path.Combine(outside,"keep"),"safe");var link=Path.Combine(root,"Cache/link");Directory.CreateSymbolicLink(link,outside);
Check(StorageMaintenance.Inspect("links",Path.Combine(root,"Cache"),true).Files.All(f=>!f.Path.Contains("link")),"Audit does not follow directory links");
var forgedLink=new StorageCategory("link",true,[new(Path.Combine(link,"keep"),4,File.GetLastWriteTimeUtc(Path.Combine(link,"keep")))]);StorageMaintenance.Clean(root,[forgedLink],DateTime.UtcNow.AddDays(1));Check(File.Exists(Path.Combine(outside,"keep")),"Cleanup rejects paths traversing symlink ancestors");Directory.Delete(link);
}
using var stop=new CancellationTokenSource();stop.Cancel();try{StorageMaintenance.Inspect("cancel",root,true,stop.Token);throw new Exception("ignored cancel");}catch(OperationCanceledException){Check(true,"Storage scan is cancellable");}
Check(StorageMaintenance.Inspect("missing",Path.Combine(root,"missing"),true).Bytes==0,"Missing cache folders are harmless");
Console.WriteLine($"{n} storage maintenance checks passed.");
}finally{Directory.Delete(root,true);}
