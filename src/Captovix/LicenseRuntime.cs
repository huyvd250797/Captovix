using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Captovix.Licensing;
using Microsoft.Win32;
namespace Captovix;
internal sealed record LicenseConfiguration(string ApiUrl,string Issuer,string PublicKeyPem);
internal sealed record LicenseCache(Envelope? Certificate,DateTimeOffset HighWater);
internal static class LicenseRuntime
{
    static readonly SemaphoreSlim Gate=new(1,1);
    static readonly string Folder=Path.Combine(App.DataRoot,"licensing");
    static LicenseConfiguration? _config;static RSA? _device;static Claims? _claims;static Envelope? _certificate;
    static DateTimeOffset _highWater,_anchor;static long _tick;static string _deviceId="";static bool _ready,_identityReady;
    public static string LastError {get;private set;}="";
    public static Claims? Certificate=>_claims;
    public static DateTimeOffset? TrialUntil {get;private set;}
    public static DateTimeOffset? TrialStarted=>TrialUntil?.AddDays(-30);
    public static string TrialHistoryMessage=>TrialUntil is {} end?$"Máy này đã kích hoạt dùng thử 30 ngày từ {TrialStarted?.ToLocalTime():dd/MM/yyyy}. Hạn sử dụng: {end.ToLocalTime():dd/MM/yyyy HH:mm}. Không thể kích hoạt thêm 30 ngày.":"Máy đã có lịch sử kích hoạt. Hãy dùng KEY để tiếp tục.";
    public static bool Configured=>_config!=null;
    public static string DeviceId=>_deviceId;
    public static string DevicePublicHash=>_device==null?"":Protocol.Hash(_device.ExportSubjectPublicKeyInfoPem());
    public static bool Full=>_ready&&Protocol.FullAccess(_claims,Now)&&!ClockReversed;
    public static DateTimeOffset Now {get{var elapsed=Stopwatch.GetElapsedTime(_tick);var monotonic=_anchor>DateTimeOffset.MaxValue-elapsed?DateTimeOffset.MaxValue:_anchor+elapsed;return new[]{DateTimeOffset.UtcNow,_highWater,monotonic}.Max();}}
    public static bool ClockReversed=>_highWater-DateTimeOffset.UtcNow>TimeSpan.FromMinutes(5);
    public static string Status=>!Configured?"Chưa cấu hình dịch vụ bản quyền":ClockReversed?"Cần kiểm tra ngày giờ máy":_claims is {Status:"released"}?"Đã trả bản quyền máy này":_claims is {Status:"revoked",Kind:"perpetual"}?"Bản quyền đã thu hồi":_claims?.Kind=="trial"&&_claims.TrialUntil<=Now?"Đã hết thời gian dùng thử":Full?_claims?.Kind=="perpetual"?"Đã kích hoạt · Vĩnh viễn":$"Dùng thử · còn {Math.Max(1,(int)Math.Ceiling((_claims!.TrialUntil!.Value-Now).TotalDays))} ngày":_claims==null?"Chưa kích hoạt": "Cần xác minh bản quyền trực tuyến";
    public static void Initialize()
    {
        if(_ready)return;_anchor=DateTimeOffset.UtcNow;_tick=Stopwatch.GetTimestamp();
        try{
            Directory.CreateDirectory(Folder);using var configStream=typeof(LicenseRuntime).Assembly.GetManifestResourceStream("Captovix.license-config.json")??throw new InvalidDataException("Thiếu cấu hình bản quyền trong bản build.");var config=JsonSerializer.Deserialize<LicenseConfiguration>(configStream,Protocol.Json)!;
            if(!Uri.TryCreate(config.ApiUrl,UriKind.Absolute,out var uri)||uri.Scheme!="https"||uri.UserInfo!=""||uri.Query!=""||uri.Fragment!=""||config.Issuer.Length is <1 or >100)throw new InvalidDataException("Dịch vụ bản quyền chưa được cấu hình. Chỉ chụp vùng được mở.");
            if(!config.PublicKeyPem.TrimStart().StartsWith("-----BEGIN PUBLIC KEY-----")||config.PublicKeyPem.Contains("PRIVATE KEY"))throw new InvalidDataException("Cấu hình chỉ được chứa PUBLIC KEY, không dùng private key.");
            using(var rsa=RSA.Create()){rsa.ImportFromPem(config.PublicKeyPem);if(rsa.KeySize<3072)throw new InvalidDataException("Public key không hợp lệ.");}_config=config;
            using var machine=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            var guid=machine?.GetValue("MachineGuid")?.ToString();if(string.IsNullOrWhiteSpace(guid))throw new InvalidDataException("Không đọc được định danh máy. Liên hệ quản trị viên.");_deviceId=Protocol.Hash(Protocol.Product+"|"+guid);
            _device=RSA.Create();var keyPath=Path.Combine(Folder,"device.bin");if(File.Exists(keyPath))_device.ImportPkcs8PrivateKey(Protect.Read(keyPath),out _);else{_device.KeySize=2048;Protect.Write(keyPath,_device.ExportPkcs8PrivateKey());}_identityReady=true;
            var cachePath=Path.Combine(Folder,"certificate.bin");if(File.Exists(cachePath)){
                var cache=JsonSerializer.Deserialize<LicenseCache>(Protect.Read(cachePath),Protocol.Json)!;_highWater=cache.HighWater;
                if(cache.Certificate!=null){_claims=Verify(cache.Certificate);_certificate=cache.Certificate;}
            }
            // Preserve the first signed trial certificate independently of the active license.
            if(_claims?.Kind=="trial"&&_certificate!=null)RememberTrial(_certificate);
            using(var ledger=Registry.CurrentUser.CreateSubKey(@"Software\HuyVo\Captovix\Licensing")){
                if(ledger.GetValue("TrialCertificate") is byte[] savedTrial){var envelope=JsonSerializer.Deserialize<Envelope>(Protect.Unseal(savedTrial),Protocol.Json)!;var trial=Verify(envelope);if(trial.Kind=="trial")TrialUntil=trial.TrialUntil;}
            }
            // Independent DPAPI mirror survives deleting only certificate.bin.
            using var reg=Registry.CurrentUser.CreateSubKey(@"Software\HuyVo\Captovix\Licensing");if(reg.GetValue("Clock") is byte[] clock){var saved=JsonSerializer.Deserialize<DateTimeOffset>(Protect.Unseal(clock),Protocol.Json);if(saved>_highWater)_highWater=saved;}
        }catch(Exception e){_claims=null;if(!_identityReady){_device?.Dispose();_device=null;}LastError=e.Message;}
        if(_highWater>_anchor)_anchor=_highWater;
        _ready=true;
    }
    static Claims Verify(Envelope e)=>Protocol.Verify(e,_config!.PublicKeyPem,_config.Issuer,_deviceId,Protocol.Hash(_device!.ExportSubjectPublicKeyInfoPem()));
    public static async Task<string?> Request(string operation,string key="",string recovery="")
    {
        await Gate.WaitAsync();try{
            if(operation=="trial"&&(TrialUntil!=null||_claims?.Kind=="perpetual"))throw new InvalidOperationException(TrialHistoryMessage);
            if(_config==null||_device==null)throw new InvalidOperationException("Chưa cấu hình máy chủ bản quyền hoặc định danh máy chưa sẵn sàng. Xem hướng dẫn quản trị.");
            using var http=new HttpClient{Timeout=TimeSpan.FromSeconds(20)};
            var proof=Protocol.SignRequest(_device,operation,new(_deviceId,Environment.MachineName[..Math.Min(80,Environment.MachineName.Length)],key,recovery,operation is "refresh" or "release"?_claims?.ActivationId??"":""),DateTimeOffset.UtcNow);
            using var result=await http.PostAsJsonAsync(_config.ApiUrl.TrimEnd('/')+"/v1/"+operation,proof,Protocol.Json);
            var raw=await result.Content.ReadAsStringAsync();if(raw.Length>20000)throw new IOException("Phản hồi dịch vụ quá lớn.");
            if(!result.IsSuccessStatusCode){var code=result.StatusCode==System.Net.HttpStatusCode.TooManyRequests?"RATE_LIMIT":"SERVICE_UNAVAILABLE";try{using var doc=JsonDocument.Parse(raw);code=doc.RootElement.GetProperty("code").GetString()??code;}catch{}throw new InvalidOperationException(Message(code));}
            var reply=JsonSerializer.Deserialize<Reply>(raw,Protocol.Json)??throw new IOException("Phản hồi không hợp lệ.");var claims=Verify(reply.Certificate);
            if(claims.IssuedAt<_claims?.IssuedAt)throw new IOException("Từ chối chứng nhận cũ hơn chứng nhận đã lưu.");
            if(claims.Kind=="trial")RememberTrial(reply.Certificate);
            _claims=claims;_certificate=reply.Certificate;_anchor=claims.IssuedAt;_tick=Stopwatch.GetTimestamp();_highWater=claims.IssuedAt;Save();LastError="";return reply.RecoveryCode;
        }catch(Exception e){LastError=e.Message;throw;}finally{Gate.Release();}
    }
    static void RememberTrial(Envelope envelope){if(TrialUntil!=null)return;var trial=Verify(envelope);if(trial.Kind!="trial")return;using var registry=Registry.CurrentUser.CreateSubKey(@"Software\HuyVo\Captovix\Licensing");if(registry.GetValue("TrialCertificate") is byte[] previous){var saved=Verify(JsonSerializer.Deserialize<Envelope>(Protect.Unseal(previous),Protocol.Json)!);if(saved.Kind=="trial"){TrialUntil=saved.TrialUntil;return;}}registry.SetValue("TrialCertificate",Protect.Seal(JsonSerializer.SerializeToUtf8Bytes(envelope,Protocol.Json)),RegistryValueKind.Binary);TrialUntil=trial.TrialUntil;}
    public static void CheckpointClock(){if(!_ready||_device==null)return;try{_highWater=Now;Save();}catch(Exception e){LastError="Chưa lưu được trạng thái bản quyền: "+e.Message;_claims=null;}}
    static void Save(){Protect.Write(Path.Combine(Folder,"certificate.bin"),JsonSerializer.SerializeToUtf8Bytes(new LicenseCache(_certificate,_highWater),Protocol.Json));using var reg=Registry.CurrentUser.CreateSubKey(@"Software\HuyVo\Captovix\Licensing");reg.SetValue("Clock",Protect.Seal(JsonSerializer.SerializeToUtf8Bytes(_highWater,Protocol.Json)),RegistryValueKind.Binary);}
    public static void Demand(){if(!Full)throw new InvalidOperationException("Chức năng cần bản quyền: "+Status);}
    static string Message(string code)=>code switch{
        "USE_LICENSE_ACTIVATION_OR_RECOVERY"=>"Máy đã có lịch sử bản quyền. Dùng key để kích hoạt lại hoặc khôi phục, không bắt đầu trial mới.",
        "RATE_LIMIT"=>"Quá nhiều yêu cầu bản quyền. Vui lòng chờ một phút rồi thử lại.","CONFIG_INVALID"=>"Dịch vụ bản quyền chưa được cấu hình đầy đủ. Liên hệ quản trị.",
        "KEY_INVALID_OR_REVOKED"=>"Key không đúng hoặc đã bị thu hồi.","SEATS_FULL_USE_TRANSFER_OR_RECOVERY"=>"Đã đủ số máy. Trả bản quyền ở máy cũ hoặc dùng mã khôi phục.",
        "TRIAL_ALREADY_STARTED_CONTACT_SUPPORT"=>"Máy đã bắt đầu dùng thử bằng định danh cũ. Liên hệ quản trị để kiểm tra; cài lại không tạo 30 ngày mới.","RECOVERY_INVALID"=>"Mã khôi phục không đúng hoặc đã dùng.","RECOVERY_COOLDOWN"=>"Chưa hết thời gian chờ giữa hai lần khôi phục. Liên hệ quản trị nếu cần xử lý sớm.","REQUEST_INVALID_OR_CLOCK"=>"Ngày giờ máy sai hoặc yêu cầu không hợp lệ. Bật tự đồng bộ ngày giờ Windows.","DEVICE_NOT_FOUND"=>"Chưa có kích hoạt cho máy này. Chọn dùng thử hoặc nhập key.","TRIAL_CANNOT_TRANSFER"=>"Dùng thử gắn với máy và không chuyển được.",_=>"Chưa xác minh được bản quyền ("+code+"). Kiểm tra kết nối hoặc liên hệ hỗ trợ."};
}
internal static class Protect
{
    [StructLayout(LayoutKind.Sequential)]struct Blob{public int Size;public nint Data;}
    [DllImport("crypt32.dll",SetLastError=true)]static extern bool CryptProtectData(ref Blob input,nint description,nint entropy,nint reserved,nint prompt,int flags,out Blob output);
    [DllImport("crypt32.dll",SetLastError=true)]static extern bool CryptUnprotectData(ref Blob input,nint description,nint entropy,nint reserved,nint prompt,int flags,out Blob output);
    [DllImport("kernel32.dll")]static extern nint LocalFree(nint value);
    static byte[] Transform(byte[] data,bool protect){var input=new Blob{Size=data.Length,Data=Marshal.AllocHGlobal(data.Length)};Blob output=default;try{Marshal.Copy(data,0,input.Data,data.Length);bool ok=protect?CryptProtectData(ref input,0,0,0,0,1,out output):CryptUnprotectData(ref input,0,0,0,0,1,out output);if(!ok)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());var value=new byte[output.Size];Marshal.Copy(output.Data,value,0,value.Length);return value;}finally{Marshal.FreeHGlobal(input.Data);if(output.Data!=0)LocalFree(output.Data);}}
    public static byte[] Seal(byte[] b)=>Transform(b,true);public static byte[] Unseal(byte[] b)=>Transform(b,false);
    public static byte[] Read(string path)=>Unseal(File.ReadAllBytes(path));
    public static void Write(string path,byte[] data){var temp=path+".tmp";File.WriteAllBytes(temp,Seal(data));File.Move(temp,path,true);}
}
