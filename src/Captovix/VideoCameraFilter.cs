using System.Text;

namespace Captovix;

internal static class VideoCameraFilter
{
    internal static string Build(VideoCamera camera,VideoRange range,int width,int height,CameraSyncSpan[] spans)
    {
        var box=camera.Pixels(width,height);
        var graph=new StringBuilder();
        if(spans.Length>1){
            graph.Append($"[1:v]split={spans.Length}");
            for(int i=0;i<spans.Length;i++)graph.Append($"[input{i}]");
            graph.Append(';');
        }
        for(int i=0;i<spans.Length;i++){
            var span=spans[i];
            var from=range.LocalAt(span.ScreenStart);
            var to=range.LocalAt(span.ScreenStart+span.Length);
            bool freeze=span.ScreenStart+span.Length>=range.End&&range.Timing.Hold>TimeSpan.Zero;
            string input=spans.Length==1?"1:v":$"input{i}";
            graph.Append($"[{input}]trim=start={N(span.CameraStart)}:duration={N(span.Length)},setpts=(PTS-STARTPTS)/{VideoMotion.Number(range.Timing.Speed)},");
            graph.Append($"scale={box.Width}:{box.Height}:force_original_aspect_ratio=decrease:force_divisible_by=2,setsar=1");
            if(freeze){
                graph.Append($",tpad=stop_mode=clone:stop_duration={N(range.Timing.Hold)}");
                to+=range.Timing.Hold;
            }
            graph.Append($",setpts=PTS+{N(from)}/TB[part{i}];");
            string main=i==0?"0:v":$"camera{i-1}",output=i==spans.Length-1?"cameraout":$"camera{i}";
            graph.Append($"[{main}][part{i}]overlay=x='{box.X}+({box.Width}-overlay_w)/2':y='{box.Y}+({box.Height}-overlay_h)/2':eof_action=pass:repeatlast=0:enable='gte(t,{N(from)})*lt(t,{N(to)})'[{output}];");
        }
        return graph.ToString().TrimEnd(';');

        static string N(TimeSpan time)=>VideoMotion.Number(time.TotalSeconds);
    }
}
