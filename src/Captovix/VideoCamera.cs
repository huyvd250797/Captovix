namespace Captovix;

internal sealed record CameraSyncSpan(TimeSpan ScreenStart,TimeSpan CameraStart,TimeSpan Length);

internal sealed record VideoCamera(string Path,TimeSpan Duration)
{
    public bool Visible {get;init;}=true;
    public double X {get;init;}=.72;
    public double Y {get;init;}=.72;
    public double Width {get;init;}=.25;
    public double Height {get;init;}=.25;
    public CameraSyncSpan[] Sync {get;init;}=[];
    internal bool IsValid()=>Path is {Length:>0 and <=32767}&&System.IO.Path.IsPathFullyQualified(Path)
        &&System.IO.Path.GetExtension(Path).Equals(".mp4",StringComparison.OrdinalIgnoreCase)
        &&Duration>TimeSpan.Zero&&Duration<=TimeSpan.FromDays(30)
        &&Width is >=.05 and <=1&&Height is >=.05 and <=1&&X>=0&&Y>=0&&X+Width<=1.00000001&&Y+Height<=1.00000001
        &&Sync is {Length:<=10000}&&Sync.All(s=>s!=null&&s.ScreenStart>=TimeSpan.Zero&&s.CameraStart>=TimeSpan.Zero
            &&s.CameraStart<Duration&&s.Length>TimeSpan.Zero&&s.Length<=Duration-s.CameraStart
            &&s.ScreenStart<=TimeSpan.FromDays(30)-s.Length)
        &&Sync.Zip(Sync.Skip(1)).All(pair=>pair.First.ScreenStart+pair.First.Length<=pair.Second.ScreenStart);

    internal static VideoCamera Imported(string path,TimeSpan duration,TimeSpan offset)
    {
        var available=duration-TimeSpan.FromTicks(Math.Max(0,-offset.Ticks));
        return new(path,duration){Sync=available>TimeSpan.Zero
            ?[new(TimeSpan.FromTicks(Math.Max(0,offset.Ticks)),duration-available,available)]:[]};
    }

    internal (int X,int Y,int Width,int Height) Pixels(int width,int height)
    {
        int w=Math.Clamp(((int)Math.Round(Width*width/2))*2,2,width);
        int h=Math.Clamp(((int)Math.Round(Height*height/2))*2,2,height);
        return (Math.Clamp(((int)Math.Round(X*width/2))*2,0,width-w),
            Math.Clamp(((int)Math.Round(Y*height/2))*2,0,height-h),w,h);
    }

    internal TimeSpan? At(TimeSpan source)
    {
        foreach(var span in Sync){
            if(source<span.ScreenStart)break;
            if(source-span.ScreenStart<span.Length)return span.CameraStart+source-span.ScreenStart;
        }
        return null;
    }

    internal IEnumerable<CameraSyncSpan> ForClip(VideoRange range)
    {
        foreach(var span in Sync){
            var from=span.ScreenStart>range.Start?span.ScreenStart:range.Start;
            var to=span.ScreenStart+span.Length<range.End?span.ScreenStart+span.Length:range.End;
            if(from<to)yield return new(from,span.CameraStart+from-span.ScreenStart,to-from);
        }
    }
}
