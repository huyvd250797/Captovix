using System.Diagnostics;
using System.Globalization;
using System.IO;
namespace Captovix;
internal static class VideoPreviewFrames
{
    internal static string[] Arguments(string input,TimeSpan at,int width,int height)=>[
        "-nostdin","-hide_banner","-loglevel","error","-ss",at.TotalSeconds.ToString("0.########",CultureInfo.InvariantCulture),"-i",input,
        "-frames:v","1","-vf",$"scale=w='min({width},iw)':h='min({height},ih)':force_original_aspect_ratio=decrease",
        "-f","image2pipe","-c:v","png","pipe:1"];
    internal static async Task<byte[]> ReadAsync(string input,TimeSpan at,int width,int height,CancellationToken token){
        var start=new ProcessStartInfo(VideoTrimExport.FindFfmpeg()??throw new IOException("Thiếu bộ giải mã xem trước.")){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        foreach(var value in Arguments(input,at,width,height))start.ArgumentList.Add(value);
        using var process=Process.Start(start)??throw new IOException("Không mở được bộ giải mã xem trước.");
        using var registration=token.Register(()=>{try{if(!process.HasExited)process.Kill(true);}catch(InvalidOperationException){}});
        var errors=process.StandardError.ReadToEndAsync();using var bytes=new MemoryStream();
        await process.StandardOutput.BaseStream.CopyToAsync(bytes,token);await process.WaitForExitAsync(token);
        var error=await errors;if(process.ExitCode!=0||bytes.Length==0)throw new IOException("Chưa lấy được khung xem trước: "+error);return bytes.ToArray();
    }
}
