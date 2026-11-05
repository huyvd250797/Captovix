using System.IO;
using System.IO.Compression;
using System.Text;
namespace Captovix;
// Snapshot JSON stores small references; immutable PNG/mask strings are shared
// by reference between snapshots instead of serialized into every Undo entry.
public sealed class HistorySnapshot
{
    public bool HasRecovery {get;}
    readonly byte[] json;readonly string[] blobs;
    public long JsonBytes=>json.LongLength;
    public IReadOnlyList<string> Blobs=>blobs;
    HistorySnapshot(byte[] bytes,string[] data,bool recovery){json=bytes;blobs=data;HasRecovery=recovery;}
    public static HistorySnapshot Capture(EditorDocument source){
        var copy=source.Snapshot();var data=new List<string>();var map=new Dictionary<string,string>(ReferenceEqualityComparer.Instance);
        string? Intern(string? value){if(value==null||value.Length<512)return value;if(map.TryGetValue(value,out var token))return token;token="@captovix-history:"+data.Count;data.Add(value);map[value]=token;return token;}
        copy.PixelBasePng=Intern(copy.PixelBasePng);foreach(var a in copy.Annotations)a.PixelPng=Intern(a.PixelPng);foreach(var r in copy.Recoveries)r.Mask=Intern(r.Mask)!;
        using var output=new MemoryStream();using(var zip=new BrotliStream(output,CompressionLevel.Fastest,true)){var bytes=Encoding.UTF8.GetBytes(copy.Serialize());zip.Write(bytes);}
        return new(output.ToArray(),data.ToArray(),source.Recoveries.Count>0||source.Annotations.Any(a=>a.Mockup!=null));
    }
    public EditorDocument Restore(){
        using var input=new MemoryStream(json,false);using var zip=new BrotliStream(input,CompressionMode.Decompress);using var reader=new StreamReader(zip,Encoding.UTF8);var copy=EditorDocument.Deserialize(reader.ReadToEnd());
        string? Expand(string? value){const string prefix="@captovix-history:";return value!=null&&value.StartsWith(prefix,StringComparison.Ordinal)&&int.TryParse(value.AsSpan(prefix.Length),out var at)&&at>=0&&at<blobs.Length?blobs[at]:value;}
        copy.PixelBasePng=Expand(copy.PixelBasePng);foreach(var a in copy.Annotations)a.PixelPng=Expand(a.PixelPng);foreach(var r in copy.Recoveries)r.Mask=Expand(r.Mask)!;return copy;
    }
    public string ExpandJson()=>Restore().Serialize();
    public bool Same(HistorySnapshot other)=>json.AsSpan().SequenceEqual(other.json)&&blobs.Length==other.blobs.Length&&blobs.Zip(other.blobs).All(x=>ReferenceEquals(x.First,x.Second)||x.First==x.Second);
    public static long RetainedBytes(IEnumerable<HistorySnapshot> snapshots){long bytes=0;var seen=new HashSet<string>(ReferenceEqualityComparer.Instance);foreach(var s in snapshots){bytes+=s.JsonBytes;foreach(var blob in s.blobs)if(seen.Add(blob))bytes+=2L*blob.Length;}return bytes;}
}
