using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
namespace Captovix;
internal sealed record VideoProbeInfo(int Width,int Height,TimeSpan Duration,double Fps){public string? ColorRange {get;init;}public string? Transfer {get;init;}}
internal static class VideoProbe
{
    internal static string? Executable=>VideoTrimExport.FindFfmpeg() is {} ffmpeg&&File.Exists(Path.Combine(Path.GetDirectoryName(ffmpeg)!,OperatingSystem.IsWindows()?"ffprobe.exe":"ffprobe"))?Path.Combine(Path.GetDirectoryName(ffmpeg)!,OperatingSystem.IsWindows()?"ffprobe.exe":"ffprobe"):null;
    internal static async Task<VideoProbeInfo> ReadAsync(string path,CancellationToken token){
        var start=new ProcessStartInfo(Executable??throw new IOException("Thiếu ffprobe.exe. Cài đầy đủ gói cập nhật mới.")){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        foreach(var argument in new[]{"-v","error","-select_streams","v:0","-show_entries","stream=width,height,avg_frame_rate,duration,color_range,color_transfer:format=duration","-of","json",path})start.ArgumentList.Add(argument);
        using var process=Process.Start(start)??throw new IOException("Không mở được bộ đọc video.");
        using var registration=token.Register(()=>{try{if(!process.HasExited)process.Kill(true);}catch(InvalidOperationException){}});
        var output=process.StandardOutput.ReadToEndAsync();var errors=process.StandardError.ReadToEndAsync();await process.WaitForExitAsync(token);
        string text=await output,error=await errors;if(process.ExitCode!=0)throw new IOException("Không đọc được nguồn video: "+error);
        using var doc=JsonDocument.Parse(text);var stream=doc.RootElement.GetProperty("streams").EnumerateArray().First();
        string duration=stream.TryGetProperty("duration",out var d)?d.GetString()??"": "";
        if(!double.TryParse(duration,NumberStyles.Float,CultureInfo.InvariantCulture,out double seconds))seconds=double.Parse(doc.RootElement.GetProperty("format").GetProperty("duration").GetString()!,CultureInfo.InvariantCulture);
        var parts=(stream.GetProperty("avg_frame_rate").GetString()??"30/1").Split('/');double fps=parts.Length==2&&double.TryParse(parts[0],CultureInfo.InvariantCulture,out var n)&&double.TryParse(parts[1],CultureInfo.InvariantCulture,out var den)&&den>0?n/den:30;
        var result=new VideoProbeInfo(stream.GetProperty("width").GetInt32(),stream.GetProperty("height").GetInt32(),TimeSpan.FromSeconds(seconds),fps>0?fps:30){ColorRange=stream.TryGetProperty("color_range",out var range)?range.GetString():null,Transfer=stream.TryGetProperty("color_transfer",out var transfer)?transfer.GetString():null};
        if(result.Width<=0||result.Height<=0||result.Duration<=TimeSpan.Zero)throw new InvalidDataException("Video không có hình hoặc thời lượng hợp lệ.");return result;
    }
}
