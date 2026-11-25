using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
namespace Captovix;
public sealed record LibraryAsset(string Key,string Extension,string Metadata,string Source,string? Edit,string? Mic=null,string? SystemAudio=null,string? RecordingClicks=null,string? Camera=null,string? CameraMetadata=null);
public sealed record LibraryArchiveItem(string Key,string Extension,string Metadata,string Hash,string? EditHash,string? MicHash=null,string? SystemAudioHash=null,string? RecordingClicksHash=null,string? CameraHash=null,string? CameraMetadataHash=null);
public sealed record LibraryManifest(int Version,List<LibraryArchiveItem> Items);
public static class LibraryArchive
{
    public const long MaximumBytes=2_000_000_000;
    private const long MaxClicksBytes=2_000_000;
    private const long MaxTrackBytes=1_000_000_000;
    private static readonly HashSet<string> Extensions=new(StringComparer.OrdinalIgnoreCase){".png",".jpg",".jpeg",".bmp",".mp4"};
    public static string[] Tags(string text){var tags=text.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();if(tags.Length>32||tags.Any(t=>t.Length>60))throw new InvalidDataException("Tối đa32 tag, mỗi tag60 ký tự.");return tags;}
    private static bool Key(string key)=>Guid.TryParseExact(key,"N",out _);
    private static string Hash(Stream stream)=>Convert.ToHexString(SHA256.HashData(stream));
    public static void Save(string destination,IReadOnlyList<LibraryAsset> assets,CancellationToken cancellation=default)
    {
        if(assets.Count>2000||assets.Select(a=>a.Key).Distinct().Count()!=assets.Count)throw new InvalidDataException("Tối đa 2.000 nội dung, mã không trùng.");
        var temp=destination+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{
            using(var zip=ZipFile.Open(temp,ZipArchiveMode.Create)){
                long total=0;var items=new List<LibraryArchiveItem>();
                string Add(string source,string entry,long max){using var input=File.OpenRead(source);if(input.Length>max||(total+=input.Length)>MaximumBytes)throw new InvalidDataException("Bản sao lưu tối đa 2 GB; mỗi media tối đa 512 MB.");using(var output=zip.CreateEntry(entry,CompressionLevel.Fastest).Open()){var buffer=new byte[65536];int n;while((n=input.Read(buffer))>0){cancellation.ThrowIfCancellationRequested();output.Write(buffer,0,n);}}input.Position=0;return Hash(input);}
                foreach(var a in assets){cancellation.ThrowIfCancellationRequested();if(!Key(a.Key)||!Extensions.Contains(a.Extension)||a.Metadata.Length>32000)throw new InvalidDataException("Thông tin nội dung không hợp lệ.");string hash=Add(a.Source,$"media/{a.Key}{a.Extension}",512_000_000);string? edit=a.Edit==null?null:Add(a.Edit,$"edits/{a.Key}.json",140_000_000);if((a.Mic!=null||a.SystemAudio!=null||a.RecordingClicks!=null||a.Camera!=null||a.CameraMetadata!=null)&&!a.Extension.Equals(".mp4",StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Chỉ video mới có nguồn âm thanh riêng hoặc dữ liệu click chuột.");
                    string? mic=a.Mic==null?null:Add(a.Mic,$"audio/{a.Key}.mic.wav",MaxTrackBytes);
                    string? system=a.SystemAudio==null?null:Add(a.SystemAudio,$"audio/{a.Key}.system.wav",MaxTrackBytes);
                    string? clicks=a.RecordingClicks==null?null:Add(a.RecordingClicks,$"recording/{a.Key}.clicks.json",MaxClicksBytes);
                    if((a.Camera==null)!=(a.CameraMetadata==null))throw new InvalidDataException("Thiếu dữ liệu đồng bộ camera.");
                    string? camera=a.Camera==null?null:Add(a.Camera,$"recording/{a.Key}.camera.mp4",MaxTrackBytes);
                    string? sync=a.CameraMetadata==null?null:Add(a.CameraMetadata,$"recording/{a.Key}.camera.json",MaxClicksBytes);
                    items.Add(new(a.Key,a.Extension,a.Metadata,hash,edit,mic,system,clicks,camera,sync));}
                string json=JsonSerializer.Serialize(new LibraryManifest(items.Any(a=>a.CameraHash!=null)?4:items.Any(a=>a.RecordingClicksHash!=null)?3:items.Any(a=>a.MicHash!=null||a.SystemAudioHash!=null)?2:1,items));if(System.Text.Encoding.UTF8.GetByteCount(json)>8_000_000)throw new InvalidDataException("Manifest vượt8MB; chia nhỏ thư viện trước khi sao lưu.");using var writer=new StreamWriter(zip.CreateEntry("manifest.json").Open());writer.Write(json);
            }
            File.Move(temp,destination,true);
        }finally{File.Delete(temp);}
    }
    // Extract only enumerated names into a private staging folder; never trust paths in metadata.
    public static List<LibraryAsset> Read(string archive,string stage,CancellationToken cancellation=default)
    {
        if(Directory.Exists(stage)&&Directory.EnumerateFileSystemEntries(stage).Any())throw new IOException("Thư mục tạm cần trống.");Directory.CreateDirectory(stage);
        try{
            using var zip=ZipFile.OpenRead(archive);var manifest=zip.GetEntry("manifest.json")??throw new InvalidDataException("Thiếu manifest.");if(manifest.Length>8_000_000)throw new InvalidDataException("Manifest quá lớn.");
            var m=JsonSerializer.Deserialize<LibraryManifest>(ReadBytes(manifest,8_000_000))??throw new InvalidDataException("Manifest trống.");
            if(m.Version is not (1 or 2 or 3 or 4)||m.Items==null||m.Items.Count>2000||m.Items.Select(a=>a.Key).Distinct().Count()!=m.Items.Count)throw new InvalidDataException("Không hỗ trợ phiên bản/thư viện quá lớn.");
            var names=new HashSet<string>(StringComparer.Ordinal){"manifest.json"};foreach(var a in m.Items){cancellation.ThrowIfCancellationRequested();if(!Key(a.Key)||!Extensions.Contains(a.Extension)||a.Metadata==null||a.Metadata.Length>32000||!names.Add($"media/{a.Key}{a.Extension}"))throw new InvalidDataException("Nội dung trùng hoặc không hợp lệ.");if(a.EditHash!=null)names.Add($"edits/{a.Key}.json");
                if((a.MicHash!=null||a.SystemAudioHash!=null)&&(m.Version<2||!a.Extension.Equals(".mp4",StringComparison.OrdinalIgnoreCase)))throw new InvalidDataException("Nguồn âm thanh không hợp lệ.");
                if(a.MicHash!=null)names.Add($"audio/{a.Key}.mic.wav");
                if(a.SystemAudioHash!=null)names.Add($"audio/{a.Key}.system.wav");
                if(a.RecordingClicksHash!=null){
                    if(m.Version<3||!a.Extension.Equals(".mp4",StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Dữ liệu click chuột không hợp lệ.");
                    names.Add($"recording/{a.Key}.clicks.json");
                }
                if(a.CameraHash!=null||a.CameraMetadataHash!=null){
                    if(m.Version<4||a.CameraHash==null||a.CameraMetadataHash==null||!a.Extension.Equals(".mp4",StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Dữ liệu camera không hợp lệ.");
                    names.Add($"recording/{a.Key}.camera.mp4");
                    names.Add($"recording/{a.Key}.camera.json");
                }
            }
            if(zip.Entries.Count!=names.Count||zip.Entries.Select(e=>e.FullName).Distinct().Count()!=names.Count||zip.Entries.Any(e=>!names.Contains(e.FullName))||zip.Entries.Sum(e=>e.Length)>MaximumBytes)throw new InvalidDataException("Cấu trúc/giới hạn bản sao lưu không hợp lệ.");
            var result=new List<LibraryAsset>();
            string Extract(string name,string hash,long max,string local){var entry=zip.GetEntry(name)!;if(entry.Length>max)throw new InvalidDataException("File quá lớn.");using(var output=File.Create(local)){using var input=entry.Open();var b=new byte[65536];long size=0;int n;while((n=input.Read(b))>0){cancellation.ThrowIfCancellationRequested();size+=n;if(size>max||size>entry.Length)throw new InvalidDataException("Dữ liệu vượt giới hạn.");output.Write(b,0,n);}if(size!=entry.Length)throw new InvalidDataException("Dữ liệu chưa đủ.");}using var check=File.OpenRead(local);if(!Hash(check).Equals(hash,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Hash không khớp; bản sao lưu hỏng.");return local;}
            foreach(var a in m.Items){cancellation.ThrowIfCancellationRequested();var source=Extract($"media/{a.Key}{a.Extension}",a.Hash,512_000_000,Path.Combine(stage,a.Key+a.Extension));var edit=a.EditHash==null?null:Extract($"edits/{a.Key}.json",a.EditHash,140_000_000,Path.Combine(stage,a.Key+".json"));var mic=a.MicHash==null?null:Extract($"audio/{a.Key}.mic.wav",a.MicHash,MaxTrackBytes,Path.Combine(stage,a.Key+".mic.wav"));
                var system=a.SystemAudioHash==null?null:Extract($"audio/{a.Key}.system.wav",a.SystemAudioHash,MaxTrackBytes,Path.Combine(stage,a.Key+".system.wav"));
                var clicks=a.RecordingClicksHash==null?null:Extract($"recording/{a.Key}.clicks.json",a.RecordingClicksHash,MaxClicksBytes,Path.Combine(stage,a.Key+".clicks.json"));
                var camera=a.CameraHash==null?null:Extract($"recording/{a.Key}.camera.mp4",a.CameraHash,MaxTrackBytes,Path.Combine(stage,a.Key+".camera.mp4"));
                var sync=a.CameraMetadataHash==null?null:Extract($"recording/{a.Key}.camera.json",a.CameraMetadataHash,MaxClicksBytes,Path.Combine(stage,a.Key+".camera.json"));
                result.Add(new(a.Key,a.Extension,a.Metadata,source,edit,mic,system,clicks,camera,sync));}return result;
        }catch{Directory.Delete(stage,true);throw;}
    }
    private static byte[] ReadBytes(ZipArchiveEntry e,int max){using var input=e.Open();using var output=new MemoryStream();var b=new byte[65536];int n;while((n=input.Read(b))>0){if(output.Length+n>max)throw new InvalidDataException("Manifest vượt giới hạn.");output.Write(b,0,n);}return output.ToArray();}
}
