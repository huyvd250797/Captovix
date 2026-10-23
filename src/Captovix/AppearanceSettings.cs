using System.Text.Json;
using System.Text.RegularExpressions;
using System.IO;
namespace Captovix;
public enum AppearanceMode { System, Light, Dark }
public sealed record AppearanceSettings(AppearanceMode Mode=AppearanceMode.System,string Accent="#7C3AED",int Schema=1)
{
    public static string FileName=>"appearance-settings.json";
    public void Validate(){if(Schema!=1||!Enum.IsDefined(Mode)||!Regex.IsMatch(Accent??"","^#[0-9a-fA-F]{6}$"))throw new InvalidDataException("Thiết lập giao diện không hợp lệ hoặc thuộc phiên bản mới hơn.");}
    public static AppearanceSettings Load(string root,out string? warning)
    {
        warning=null;var path=Path.Combine(root,FileName);if(!File.Exists(path))return new();
        try{var value=JsonSerializer.Deserialize<AppearanceSettings>(File.ReadAllText(path))??throw new InvalidDataException();value.Validate();return value;}
        catch(Exception ex){warning="Chưa đọc được giao diện đã lưu; dùng mặc định. File cũ được giữ nguyên. "+ex.Message;return new();}
    }
    public void Save(string root){Validate();Directory.CreateDirectory(root);var path=Path.Combine(root,FileName);if(File.Exists(path)){try{var old=JsonSerializer.Deserialize<AppearanceSettings>(File.ReadAllText(path));old?.Validate();}catch{File.Copy(path,path+".backup-"+Guid.NewGuid().ToString("N"));}}var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllText(temp,JsonSerializer.Serialize(this));File.Move(temp,path,true);}finally{if(File.Exists(temp))File.Delete(temp);}}
    public static bool UseDark(AppearanceMode mode,bool systemLight)=>mode==AppearanceMode.Dark||(mode==AppearanceMode.System&&!systemLight);
    public static bool WhiteText(string hex)
    {
        if(!Regex.IsMatch(hex,"^#[0-9a-fA-F]{6}$"))throw new ArgumentException(nameof(hex));
        double Channel(int start){double v=Convert.ToInt32(hex.Substring(start,2),16)/255d;return v<=.04045?v/12.92:Math.Pow((v+.055)/1.055,2.4);}
        double l=.2126*Channel(1)+.7152*Channel(3)+.0722*Channel(5);return 1.05/(l+.05)>=(l+.05)/.05;
    }
}
