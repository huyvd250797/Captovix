using System.IO;
using System.Text.Json;
namespace Captovix;
internal sealed record QuickCapturePreferences(bool Magnifier=true,bool Crosshair=true)
{
    static string FileName=>Path.Combine(App.DataRoot,"quick-capture.json");
    public static QuickCapturePreferences Load(){try{return JsonSerializer.Deserialize<QuickCapturePreferences>(File.ReadAllText(FileName))??new();}catch{return new();}}
    public void Save(){try{MediaStore.AtomicWrite(FileName,JsonSerializer.Serialize(this));}catch(Exception ex){App.Log(ex);}}
}
