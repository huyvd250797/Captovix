using Captovix.Licensing;
namespace Captovix;
public sealed record LicensePresentation(bool Registered,bool TrialUsed,bool Perpetual,string Heading,string Details,bool CanStartTrial)
{
    public static LicensePresentation Create(Claims? claims,DateTimeOffset? historyUntil,DateTimeOffset now,bool full)
    {
        bool perpetual=claims?.Kind=="perpetual"&&claims.Status=="active";
        var until=historyUntil??(claims?.Kind=="trial"?claims.TrialUntil:null);bool used=until!=null;
        static string Time(DateTimeOffset value)=>value.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
        if(perpetual){string date=claims!.ActivatedAt is {} at?Time(at):"Chưa có dữ liệu trong chứng nhận cũ. Bấm Xác minh lại sau khi nâng dịch vụ.";return new(true,used,true,full?"✓ Đã kích hoạt":"Cần xác minh bản quyền",$"Bản quyền vĩnh viễn\nNgày giờ kích hoạt: {date}"+(full?"":"\nChưa xác minh được quyền sử dụng hiện tại."),false);}
        if(until is {} end){int days=Math.Max(0,(int)Math.Ceiling((end-now).TotalDays));return new(true,true,false,"Dùng thử 30 ngày",$"Kích hoạt: {Time(end.AddDays(-30))}\nHết hạn: {Time(end)}\nCòn lại: {days} ngày"+(days==0?" · Đã hết hạn":full?"":" · Cần xác minh bản quyền"),false);}
        return new(false,false,false,"Kích hoạt Captovix","Nhập KEY để sử dụng đầy đủ công cụ.",claims?.Kind!="perpetual");
    }
}
