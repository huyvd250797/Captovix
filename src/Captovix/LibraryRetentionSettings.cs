using System.IO;
using System.Text.Json;
namespace Captovix;

internal sealed record LibraryRetentionSettings
{
    public bool Enabled {get;init;}
    public int Days {get;init;}=60;
    public DateTime? EnabledAt {get;init;}
    public DateTime? LastRunDate {get;init;}
    public string[]? RemovedSourcePaths {get;init;}
    internal bool Valid=>Days is >=1 and <=36500;
    internal static DateTime DueDate(DateTimeOffset created,int days)=>created.LocalDateTime.Date.AddDays(days);
    internal static DateTime NextRun(DateTime now)=>now.TimeOfDay<new TimeSpan(23,59,0)?now.Date.AddHours(23).AddMinutes(59):now.Date.AddDays(1).AddHours(23).AddMinutes(59);
    internal DateTime LatestDeadline(DateTime now)=>now.TimeOfDay>=new TimeSpan(23,59,0)?now.Date.AddHours(23).AddMinutes(59):now.Date.AddDays(-1).AddHours(23).AddMinutes(59);
    internal DateTime ScheduledRun(DateTime now){
        var missed=LatestDeadline(now);
        bool pending=Enabled&&(LastRunDate==null||LastRunDate.Value.Date<missed.Date)&&(EnabledAt==null||EnabledAt.Value<=missed);
        return pending?missed:NextRun(now);
    }
    internal bool IsDue(DateTimeOffset created,DateTime today)=>Enabled&&Valid&&DueDate(created,Days)<=today.Date;
    internal static LibraryRetentionSettings Load(string root){
        string path=Path.Combine(root,"library-retention.json");
        if(!File.Exists(path))return new();
        var value=JsonSerializer.Deserialize<LibraryRetentionSettings>(File.ReadAllText(path))??throw new InvalidDataException("Cấu hình tự dọn thư viện không hợp lệ.");
        return value.Valid?value:throw new InvalidDataException("Số ngày lưu thư viện phải từ 1 đến 36500.");
    }
    internal void Save(string root){if(!Valid)throw new InvalidDataException("Số ngày lưu thư viện phải từ 1 đến 36500.");MediaStore.AtomicWrite(Path.Combine(root,"library-retention.json"),JsonSerializer.Serialize(this));}
}

