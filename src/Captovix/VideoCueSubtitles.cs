
using System.Text;

namespace Captovix;

internal static class VideoCueSubtitles
{
    internal static string Build(VideoCueSpan[] spans,IReadOnlyDictionary<int,(int Width,int Height)> sizes,
        VideoCanvasLayout layout,int width,int height,double fps,TimeSpan duration)
    {
        var script=new StringBuilder($"""
            [Script Info]
            ScriptType: v4.00+
            PlayResX: {width}
            PlayResY: {height}
            WrapStyle: 2
            ScaledBorderAndShadow: yes
            [V4+ Styles]
            Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
            Style: Cue,Segoe UI,{Math.Max(14,height/25)},&H00FFFFFF,&H00FFFFFF,&H00211B17,&H00211B17,-1,0,0,0,100,100,0,0,3,4,0,5,0,0,0,1
            [Events]
            Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text

            """);
        var geometries=new Dictionary<VideoRange,VideoCanvasGeometry>();
        int radius=Math.Max(8,height/38);
        foreach(var span in spans){
            var cue=span.Cue;
            if(cue.Kind=="marker"||span.Start>=duration)continue;
            var end=span.End<duration?span.End:duration;
            if(cue.Kind=="key"){
                string text=cue.Text.Replace("\\","／").Replace("{","(").Replace("}",")");
                Line(span.Start,end,$"{{\\an5\\pos({width/2},{height*9/10})}}{text}");
                continue;
            }
            var size=sizes[cue.SourceIndex];
            if(!geometries.TryGetValue(span.Clip,out var geometry)){
                geometry=VideoCanvasGeometry.Create(layout,span.Clip.Layout??VideoClipLayout.Default,size.Width,size.Height,width,height);
                geometries.Add(span.Clip,geometry);
            }
            if(span.Clip.Timing.Keys.Length<=1){
                Ring(span.Start,end,span.Start);
            }else{
                // Align generated cue samples to the same frame grid as zoompan.
                long first=(long)Math.Ceiling(span.Start.TotalSeconds*fps-1e-7);
                for(long frame=first;frame/fps<end.TotalSeconds;frame++){
                    var at=TimeSpan.FromSeconds(frame/fps);
                    var next=TimeSpan.FromSeconds((frame+1)/fps);
                    Ring(at,next<end?next:end,at);
                }
            }
            void Ring(TimeSpan start,TimeSpan finish,TimeSpan at){
                if(VideoCuePlan.Point(span,at,geometry,size.Width,size.Height,width,height) is not {} point)return;
                string color=cue.Color;
                string bgr=color[5..7]+color[3..5]+color[1..3];
                int r=radius, c=(int)Math.Round(r*.5523);
                // The two contours have opposite winding so the centre remains transparent.
                int t=Math.Max(2,r/6), q=r-t, k=(int)Math.Round(q*.5523);
                string path=$"m {-r} 0 b {-r} {-c} {-c} {-r} 0 {-r} {c} {-r} {r} {-c} {r} 0 {r} {c} {c} {r} 0 {r} {-c} {r} {-r} {c} {-r} 0 " +
                    $"m {-q} 0 b {-q} {k} {-k} {q} 0 {q} {k} {q} {q} {k} {q} 0 {q} {-k} {k} {-q} 0 {-q} {-k} {-q} {-q} {-k} {-q} 0";
                Line(start,finish,$"{{\\an5\\pos({(int)Math.Round(point.X)+r},{(int)Math.Round(point.Y)+r})\\bord0\\shad0\\1c&H{bgr}&\\p1}}{path}");
            }
        }
        return script.ToString();

        void Line(TimeSpan start,TimeSpan end,string body)
        {
            string from=Stamp(start),to=Stamp(end);
            if(from!=to)script.AppendLine($"Dialogue: 0,{from},{to},Cue,,0,0,0,,{body}");
        }
    }

    private static string Stamp(TimeSpan time)
    {
        long centiseconds=time.Ticks/(TimeSpan.TicksPerMillisecond*10);
        return FormattableString.Invariant($"{centiseconds/360000}:{centiseconds/6000%60:00}:{centiseconds/100%60:00}.{centiseconds%100:00}");
    }
}
