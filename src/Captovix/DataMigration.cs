using System.IO;
using System.Text.Json;

namespace Captovix;
public static class DataMigration
{
    public static bool ImportLegacy(string destination,string legacy)
    {
        var current=Path.Combine(destination,"library.json");var source=Path.Combine(legacy,"library.json");
        if(File.Exists(current)||!File.Exists(source))return false;
        var json=File.ReadAllText(source);
        using(var parsed=JsonDocument.Parse(json)){if(parsed.RootElement.ValueKind!=JsonValueKind.Array)throw new InvalidDataException("Thư viện V0.1 không hợp lệ; dữ liệu gốc vẫn được giữ tại "+legacy);}
        Directory.CreateDirectory(destination);
        var temp=current+".migration";File.WriteAllText(temp,json);File.Move(temp,current,false);
        var settings=Path.Combine(legacy,"settings.json");if(File.Exists(settings)&&!File.Exists(Path.Combine(destination,"settings.json")))File.Copy(settings,Path.Combine(destination,"settings.json"));
        File.WriteAllText(Path.Combine(destination,"migration-v01.txt"),$"Imported metadata {DateTimeOffset.Now:O}\nExisting media remain at original paths under {legacy}. Do not delete that folder while referenced.\n");
        return true;
    }
}
