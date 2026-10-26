using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
namespace Captovix;
public sealed record LibraryAsset(string Key,string Extension,string Metadata,string Source,string? Edit);
public sealed record LibraryArchiveItem(string Key,string Extension,string Metadata,string Hash,string? EditHash);
public sealed record LibraryManifest(int Version,List<LibraryArchiveItem> Items);
public static class LibraryArchive
{
    public const long MaximumBytes=2_000_000_000;
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
                foreach(var a in assets){cancellation.ThrowIfCancellationRequested();if(!Key(a.Key)||!Extensions.Contains(a.Extension)||a.Metadata.Length>32000)throw new InvalidDataException("Thông tin nội dung không hợp lệ.");string hash=Add(a.Source,$"media/{a.Key}{a.Extension}",512_000_000);string? edit=a.Edit==null?null:Add(a.Edit,$"edits/{a.Key}.json",140_000_000);items.Add(new(a.Key,a.Extension,a.Metadata,hash,edit));}
                string json=JsonSerializer.Serialize(new LibraryManifest(1,items));if(System.Text.Encoding.UTF8.GetByteCount(json)>8_000_000)throw new InvalidDataException("Manifest vượt8MB; chia nhỏ thư viện trước khi sao lưu.");using var writer=new StreamWriter(zip.CreateEntry("manifest.json").Open());writer.Write(json);
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
            if(m.Version!=1||m.Items==null||m.Items.Count>2000||m.Items.Select(a=>a.Key).Distinct().Count()!=m.Items.Count)throw new InvalidDataException("Không hỗ trợ phiên bản/thư viện quá lớn.");
            var names=new HashSet<string>(StringComparer.Ordinal){"manifest.json"};foreach(var a in m.Items){cancellation.ThrowIfCancellationRequested();if(!Key(a.Key)||!Extensions.Contains(a.Extension)||a.Metadata==null||a.Metadata.Length>32000||!names.Add($"media/{a.Key}{a.Extension}"))throw new InvalidDataException("Nội dung trùng hoặc không hợp lệ.");if(a.EditHash!=null)names.Add($"edits/{a.Key}.json");}
            if(zip.Entries.Count!=names.Count||zip.Entries.Select(e=>e.FullName).Distinct().Count()!=names.Count||zip.Entries.Any(e=>!names.Contains(e.FullName))||zip.Entries.Sum(e=>e.Length)>MaximumBytes)throw new InvalidDataException("Cấu trúc/giới hạn bản sao lưu không hợp lệ.");
            var result=new List<LibraryAsset>();
            string Extract(string name,string hash,long max,string local){var entry=zip.GetEntry(name)!;if(entry.Length>max)throw new InvalidDataException("File quá lớn.");using(var output=File.Create(local)){using var input=entry.Open();var b=new byte[65536];long size=0;int n;while((n=input.Read(b))>0){cancellation.ThrowIfCancellationRequested();size+=n;if(size>max||size>entry.Length)throw new InvalidDataException("Dữ liệu vượt giới hạn.");output.Write(b,0,n);}if(size!=entry.Length)throw new InvalidDataException("Dữ liệu chưa đủ.");}using var check=File.OpenRead(local);if(!Hash(check).Equals(hash,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Hash không khớp; bản sao lưu hỏng.");return local;}
            foreach(var a in m.Items){cancellation.ThrowIfCancellationRequested();var source=Extract($"media/{a.Key}{a.Extension}",a.Hash,512_000_000,Path.Combine(stage,a.Key+a.Extension));var edit=a.EditHash==null?null:Extract($"edits/{a.Key}.json",a.EditHash,140_000_000,Path.Combine(stage,a.Key+".json"));result.Add(new(a.Key,a.Extension,a.Metadata,source,edit));}return result;
        }catch{Directory.Delete(stage,true);throw;}
    }
    private static byte[] ReadBytes(ZipArchiveEntry e,int max){using var input=e.Open();using var output=new MemoryStream();var b=new byte[65536];int n;while((n=input.Read(b))>0){if(output.Length+n>max)throw new InvalidDataException("Manifest vượt giới hạn.");output.Write(b,0,n);}return output.ToArray();}
}
