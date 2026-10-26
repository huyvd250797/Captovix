using System.IO;
namespace Captovix;
internal static class CaptureDiagnostics
{
    private static readonly object Gate=new();
    internal static void Write(string message)
    {
        try{lock(Gate){Directory.CreateDirectory(App.DataRoot);var path=Path.Combine(App.DataRoot,"capture.log");if(File.Exists(path)&&new FileInfo(path).Length>2_000_000)File.Move(path,path+".previous",true);File.AppendAllText(path,$"{DateTimeOffset.Now:O} V{VersionInfo.Current} {message.Replace('\r',' ').Replace('\n',' ')}\n");}}catch{/* Diagnostics must never interrupt capture. */}
    }
}
