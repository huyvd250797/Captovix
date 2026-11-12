using System.IO;

namespace Captovix;

public sealed record RecordingProfile(int Preset=1,int Resolution=1080,int Fps=30)
{
    internal static RecordingProfile ForPreset(int preset)=>preset switch{
        0=>new(0,720,15),2=>new(2,0,60),_=>new()
    };

    internal PixelRect Frame(PixelRect source)
    {
        double scale=Resolution==0?1:Math.Min(1,Math.Min((16d*Resolution/9)/Math.Max(source.Width,source.Height),
            Resolution/(double)Math.Min(source.Width,source.Height)));
        if(scale>=1)return CaptureGeometry.RecordingFrame(source);
        return new(source.X,source.Y,Even(source.Width*scale),Even(source.Height*scale));
        static int Even(double size)=>Math.Max(2,(int)Math.Floor(size/2)*2);
    }

    internal int Bitrate(PixelRect frame)=>(int)Math.Clamp(
        (Preset switch{0=>4_000_000d,2=>12_000_000d,_=>8_000_000d})*
        ((double)frame.Width*frame.Height/(1920*1080))*Fps/30,500_000,40_000_000);

    internal long BytesPerMinute(int bitrate,bool mic,bool system)=>
        (long)Math.Ceiling((bitrate/8d+(mic||system?32_000:0)+(mic?96_000:0)+(system?192_000:0))*60*1.10);

    internal static long? AvailableBytes(string output)
    {
        var root=Path.GetPathRoot(Path.GetFullPath(output));
        if(string.IsNullOrEmpty(root)||root.StartsWith(@"\\",StringComparison.Ordinal))return null;
        try{return new DriveInfo(root).AvailableFreeSpace;}
        catch(IOException){return null;}
        catch(UnauthorizedAccessException){return null;}
    }
}
