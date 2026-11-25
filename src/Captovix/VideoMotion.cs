using System.Globalization;

namespace Captovix;

internal sealed record VideoMotionKey(double At,double Zoom,double X,double Y)
{
    internal bool IsValid()=>At is >=0 and <=1&&Zoom is >=1 and <=4&&X is >=0 and <=1&&Y is >=0 and <=1;
    internal static readonly VideoMotionKey Default=new(0,1,.5,.5);
}

internal sealed record VideoMotion
{
    public double Speed {get;init;}=1;
    public TimeSpan Hold {get;init;}
    public double FadeIn {get;init;}
    public double FadeOut {get;init;}
    public VideoMotionKey[] Keys {get;init;}=[];

    internal static readonly VideoMotion Default=new();
    internal bool ChangesAudio=>Speed!=1||Hold>TimeSpan.Zero;
    internal bool IsValid()=>Speed is >=.25 and <=4&&Hold>=TimeSpan.Zero&&Hold<=TimeSpan.FromSeconds(60)
        &&FadeIn is >=0 and <=10&&FadeOut is >=0 and <=10&&Keys is {Length:<=64}
        &&Keys.All(k=>k!=null&&k.IsValid())&&Keys.Zip(Keys.Skip(1)).All(pair=>pair.First.At<pair.Second.At);
    internal TimeSpan Playback(TimeSpan source)=>TimeSpan.FromTicks((long)Math.Round(source.Ticks/Speed));
    internal TimeSpan Source(TimeSpan playback)=>TimeSpan.FromTicks((long)Math.Round(playback.Ticks*Speed));

    internal VideoMotionKey At(double fraction)
    {
        if(Keys.Length==0)return VideoMotionKey.Default;
        int right=Array.FindIndex(Keys,key=>key.At>=fraction);
        if(right<0)return Keys[^1];
        if(right==0)return Keys[0];
        var (a,b)=(Keys[right-1],Keys[right]);
        double t=(fraction-a.At)/(b.At-a.At);
        return new(fraction,a.Zoom+(b.Zoom-a.Zoom)*t,a.X+(b.X-a.X)*t,a.Y+(b.Y-a.Y)*t);
    }

    internal VideoMotion Slice(double from,double to,bool keepHold,bool keepFadeIn,bool keepFadeOut)
    {
        var keys=Keys.Length==0?[]:new[]{At(from) with{At=0}}
            .Concat(Keys.Where(k=>k.At>from&&k.At<to).Select(k=>k with{At=(k.At-from)/(to-from)}))
            .Append(At(to) with{At=1}).ToArray();
        return this with{Keys=keys,Hold=keepHold?Hold:TimeSpan.Zero,FadeIn=keepFadeIn?FadeIn:0,FadeOut=keepFadeOut?FadeOut:0};
    }

    internal static (double X,double Y) Origin(double zoom,double x,double y)
    {
        double window=1/zoom;
        return (Math.Clamp(x-window/2,0,1-window),Math.Clamp(y-window/2,0,1-window));
    }

    internal double Black(double time,double duration)
    {
        double a=FadeIn>0?1-time/Math.Min(FadeIn,duration):0;
        double b=FadeOut>0?1-(duration-time)/Math.Min(FadeOut,duration):0;
        return 1-(1-Math.Clamp(a,0,1))*(1-Math.Clamp(b,0,1));
    }

    internal string Filter(int width,int height,double fps,double movingSeconds,double seconds)
    {
        string rate=Number(fps);
        string filter="";
        if(Keys.Length>0){
            string time=$"min(1,on/({rate}*{Number(movingSeconds)}))";
            string Curve(Func<VideoMotionKey,double> value)
            {
                string expression=Number(value(Keys[^1]));
                for(int i=Keys.Length-2;i>=0;i--){
                    var (a,b)=(Keys[i],Keys[i+1]);
                    expression=$"if(lt({time},{Number(b.At)}),{Number(value(a))}+({Number(value(b)-value(a))})*max(0,({time}-{Number(a.At)})/{Number(b.At-a.At)}),{expression})";
                }
                return expression;
            }
            filter=$",format=yuv444p,zoompan=z='{Curve(k=>k.Zoom)}':x='clip(iw*({Curve(k=>k.X)}-1/(2*zoom)),0,iw-iw/zoom)':y='clip(ih*({Curve(k=>k.Y)}-1/(2*zoom)),0,ih-ih/zoom)':d=1:s={width}x{height}:fps={rate},format=yuv420p";
        }
        if(FadeIn>0)filter+=$",fade=t=in:st=0:d={Number(Math.Min(FadeIn,seconds))}";
        if(FadeOut>0){
            double fade=Math.Min(FadeOut,seconds);
            filter+=$",fade=t=out:st={Number(seconds-fade)}:d={Number(fade)}";
        }
        return filter;
    }

    internal static string Tempo(double speed)=>speed switch{
        <.5=>$"atempo=0.5,atempo={Number(speed*2)}",
        >2=>$"atempo=2,atempo={Number(speed/2)}",
        _=>$"atempo={Number(speed)}"
    };
    internal static string Number(double value)=>value.ToString("0.#########",CultureInfo.InvariantCulture);
}

internal static class VideoAnnotationLanes
{
    internal static IReadOnlyList<List<VideoMarkTrack>> Pack(IEnumerable<VideoMarkTrack> tracks)
    {
        var lanes=new List<List<VideoMarkTrack>>();
        foreach(var track in tracks.OrderBy(item=>item.Start)){
            var lane=lanes.FirstOrDefault(row=>row[^1].End<=track.Start);
            if(lane==null)lanes.Add([track]);
            else lane.Add(track);
        }
        return lanes;
    }
}
