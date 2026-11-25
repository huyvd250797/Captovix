using System.Globalization;

namespace Captovix;
internal static class VideoExportCommands
{
    private static string Seconds(TimeSpan value)=>value.TotalSeconds.ToString("0.#######",CultureInfo.InvariantCulture);
    internal static List<string> Normalize(string input,string output,string filter,TimeSpan start,TimeSpan sourceDuration,
        TimeSpan duration,VideoMotion motion,int width,int height,double fps,bool keepAudio,long? frames,string? transfer=null){
        var moving=motion.Playback(sourceDuration);
        filter+=$",setpts=(PTS-STARTPTS)/{VideoMotion.Number(motion.Speed)},fps={VideoMotion.Number(fps)}:round=near:eof_action=pass";
        filter+=$",tpad=stop_mode=clone:stop_duration={VideoMotion.Number(motion.Hold.TotalSeconds+1)}";
        filter+=motion.Filter(width,height,fps,moving.TotalSeconds,duration.TotalSeconds);
        filter+=",setsar=1,format=yuv420p";
        var args=new List<string>{"-ss",Seconds(start),"-t",Seconds(sourceDuration),"-i",input,"-filter_threads","1",
            "-vf",filter,"-map","0:v:0","-map_metadata","-1","-c:v","libx264","-preset","fast","-crf","14","-video_track_timescale","90000"};
        // Keep source/frame color signaling; preview appearance must not retag exported pixels.
        if(frames is {} count)args.AddRange(["-frames:v",count.ToString(CultureInfo.InvariantCulture)]);
        else args.AddRange(["-t",Seconds(duration)]);
        if(keepAudio)args.AddRange(["-map","0:a:0?","-c:a","aac","-b:a","256k"]);
        else args.Add("-an");
        args.Add(output);return args;
    }
    internal static string[] Concat(string manifest,string output,TimeSpan duration)=>[
        "-f","concat","-safe","1","-i",manifest,"-map","0:v:0","-c:v","copy","-an",
        "-t",Seconds(duration),"-movflags","+faststart",output];
}
