using System.IO;
namespace Captovix;
public sealed record StorageEntry(string Path,long Bytes,DateTime Modified);
public sealed record StorageCategory(string Name,bool CanClean,IReadOnlyList<StorageEntry> Files,int Unreadable=0)
{
    public long Bytes=>Files.Sum(f=>f.Bytes);
}
public sealed record CleanupResult(long Bytes,int Removed,int Skipped);
public static class StorageMaintenance
{
    public static string Size(long bytes)=>bytes>=1024L*1024*1024?$"{bytes/(1024d*1024*1024):F2} GB":bytes>=1024*1024?$"{bytes/(1024d*1024):F1} MB":$"{bytes/1024d:F1} KB";
    private static bool PlainPath(string path){
        for(string? current=Path.GetFullPath(path);current!=null;current=Path.GetDirectoryName(current))
            if((File.Exists(current)||Directory.Exists(current))&&(File.GetAttributes(current)&FileAttributes.ReparsePoint)!=0)return false;
        return true;
    }
    public static StorageCategory Inspect(string name,string path,bool clean=false,CancellationToken token=default){
        var files=new List<StorageEntry>();int unreadable=0;var pending=new Stack<string>();pending.Push(path);
        while(pending.TryPop(out var folder)){token.ThrowIfCancellationRequested();try{if(!Directory.Exists(folder)||!PlainPath(folder))continue;
            foreach(var child in Directory.EnumerateFileSystemEntries(folder)){token.ThrowIfCancellationRequested();try{var a=File.GetAttributes(child);if((a&FileAttributes.ReparsePoint)!=0)continue;if((a&FileAttributes.Directory)!=0)pending.Push(child);else{var info=new FileInfo(child);files.Add(new(Path.GetFullPath(child),info.Length,info.LastWriteTimeUtc));}}catch(IOException){unreadable++;}catch(UnauthorizedAccessException){unreadable++;}}
        }catch(IOException){unreadable++;}catch(UnauthorizedAccessException){unreadable++;}}
        return new(name,clean,files,unreadable);
    }
    public static StorageCategory Originals(IEnumerable<string> paths){var files=new List<StorageEntry>();int unreadable=0;foreach(var path in paths.Distinct(StringComparer.OrdinalIgnoreCase))try{var info=new FileInfo(path);if(info.Exists)files.Add(new(info.FullName,info.Length,info.LastWriteTimeUtc));else unreadable++;}catch(IOException){unreadable++;}catch(UnauthorizedAccessException){unreadable++;}return new("Ảnh / video gốc (giữ lại)",false,files,unreadable);}
    public static CleanupResult Clean(string dataRoot,IEnumerable<StorageCategory> selected,DateTime cutoff,CancellationToken token=default){
        var allowed=new[]{"Thumbnails","Cache","Temp","OcrJobs"}.Select(x=>Path.GetFullPath(Path.Combine(dataRoot,x))+Path.DirectorySeparatorChar).ToArray();long bytes=0;int removed=0,skipped=0;
        foreach(var group in selected){if(!group.CanClean)continue;foreach(var item in group.Files){token.ThrowIfCancellationRequested();try{
            string path=Path.GetFullPath(item.Path);if(!allowed.Any(root=>path.StartsWith(root,StringComparison.OrdinalIgnoreCase))||!PlainPath(path)){skipped++;continue;}
            var info=new FileInfo(path);if(!info.Exists||info.LastWriteTimeUtc!=item.Modified||info.Length!=item.Bytes||info.LastWriteTimeUtc>cutoff){skipped++;continue;}
            // Never recurse here: delete only files in the inspected snapshot.
            File.Delete(path);bytes+=item.Bytes;removed++;
        }catch(IOException){skipped++;}catch(UnauthorizedAccessException){skipped++;}}}
        return new(bytes,removed,skipped);
    }
}
