using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
namespace Captovix;
public static class ReleasePackageAudit
{
    public static void Validate(string path,string appId,string version){using var zip=ZipFile.OpenRead(path);if(zip.Entries.Count>5000)throw new InvalidDataException("Gói cập nhật có quá nhiều tệp.");var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);foreach(var e in zip.Entries)if(!names.Add(e.FullName)||e.FullName.Contains("..")||e.FullName.StartsWith('/')||e.FullName.Contains('\\'))throw new InvalidDataException("Gói có đường dẫn trùng hoặc không an toàn.");
        var spec=zip.Entries.SingleOrDefault(e=>e.FullName.EndsWith(".nuspec"))??throw new InvalidDataException("Thiếu thông tin gói.");if(spec.Length>1_000_000||zip.Entries.Sum(e=>e.Length)>1_000_000_000)throw new InvalidDataException("Gói vượt giới hạn kiểm tra an toàn.");using(var stream=spec.Open()){var xml=XDocument.Load(stream);string? Read(string n)=>xml.Descendants().FirstOrDefault(e=>e.Name.LocalName==n)?.Value;if(Read("id")!=appId||Read("version")!=version)throw new InvalidDataException("AppId/phiên bản bên trong gói không khớp feed.");}
        foreach(string f in new[]{"Captovix.exe","Captovix.dll","Captovix.runtimeconfig.json","Velopack.dll","x64/tesseract50.dll","tessdata/eng.traineddata","tessdata/vie.traineddata"})if(!zip.Entries.Any(e=>e.FullName.StartsWith("lib/")&&e.FullName.EndsWith('/'+f)))throw new InvalidDataException("Gói thiếu thành phần bắt buộc: "+f);
        var stamp=zip.Entries.SingleOrDefault(e=>e.FullName.EndsWith("/release-quality.json"))??throw new InvalidDataException("Gói chưa có bản đối chiếu chất lượng phát hành V1.45+. Không áp dụng.");if(stamp.Length>64000)throw new InvalidDataException("Bản đối chiếu gói quá lớn.");using var input=stamp.Open();using var data=JsonDocument.Parse(input);if(data.RootElement.GetProperty("version").GetString()!=version)throw new InvalidDataException("Phiên bản bản đối chiếu không khớp.");foreach(var required in new[]{"Captovix.exe","Captovix.dll","Licensing.Shared.dll","Captovix.runtimeconfig.json"})if(!data.RootElement.GetProperty("files").TryGetProperty(required,out _))throw new InvalidDataException("Bản đối chiếu thiếu "+required);foreach(var pair in data.RootElement.GetProperty("files").EnumerateObject()){var e=zip.Entries.SingleOrDefault(e=>e.FullName.StartsWith("lib/")&&e.FullName.EndsWith('/'+pair.Name))??throw new InvalidDataException("Thiếu "+pair.Name);using var file=e.Open();if(!Convert.ToHexString(SHA256.HashData(file)).Equals(pair.Value.GetString(),StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Tệp đóng gói khác bản đối chiếu: "+pair.Name);}
    }
}
