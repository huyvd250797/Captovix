using System.IO;
using System.Text.Json;
namespace Captovix;
public sealed record RecoveryState(string From,string To,string Backup,string Phase,DateTimeOffset At);
public static class ReleaseRecoveryState
{
    static string PathFor(string root)=>Path.Combine(root,"Recovery","update-session.json");
    public static RecoveryState? Read(string root){try{var p=PathFor(root);return File.Exists(p)&&new FileInfo(p).Length<16000?JsonSerializer.Deserialize<RecoveryState>(File.ReadAllText(p)):null;}catch{return null;}}
    public static void Write(string root,RecoveryState state){var path=PathFor(root);Directory.CreateDirectory(Path.GetDirectoryName(path)!);var temp=path+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(state));File.Move(temp,path,true);}
    public static bool Startup(string root,string current){var state=Read(root);if(state==null||state.Phase is "Healthy" or "Restored")return false;bool interrupted=state.Phase=="Starting"||state.Phase=="Applying"&&state.To!=current;try{Write(root,state with{Phase="Starting",At=DateTimeOffset.UtcNow});}catch{return true;}return interrupted;}
    public static void Healthy(string root,string current){var state=Read(root);if(state!=null)try{Write(root,state with{Phase="Healthy",At=DateTimeOffset.UtcNow});}catch{}}
}
