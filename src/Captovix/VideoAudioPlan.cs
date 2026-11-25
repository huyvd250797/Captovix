namespace Captovix;

internal enum VideoAudioLane { Original, Microphone, System, Music }

internal sealed record VideoAudioCut(TimeSpan Start,TimeSpan End);

internal sealed record VideoAudioTrack(VideoAudioLane Lane)
{
    public double Volume {get;init;}=100;
    public bool Muted {get;init;}
    public bool Solo {get;init;}
    public TimeSpan FadeIn {get;init;}
    public TimeSpan FadeOut {get;init;}
    public VideoAudioCut[] Cuts {get;init;}=[];
    public string? File {get;init;}
    public TimeSpan Offset {get;init;}
    public TimeSpan InPoint {get;init;}
    public TimeSpan OutPoint {get;init;}
    public TimeSpan MusicDuration {get;init;}
    internal bool IsValid()=>Enum.IsDefined(Lane)&&Volume is >=0 and <=200
        &&ValidTime(FadeIn)&&ValidTime(FadeOut)&&ValidTime(Offset)&&ValidTime(InPoint)&&ValidTime(OutPoint)&&ValidTime(MusicDuration)
        &&(MusicDuration==TimeSpan.Zero||OutPoint<=MusicDuration)
        &&(File==null||Lane==VideoAudioLane.Music&&File.Length<=32767&&System.IO.Path.IsPathFullyQualified(File)&&OutPoint>InPoint)
        &&Cuts is {Length:<=128}&&Cuts.All(cut=>cut!=null&&ValidTime(cut.Start)&&ValidTime(cut.End)&&cut.Start<cut.End);

    private static bool ValidTime(TimeSpan value)=>value>=TimeSpan.Zero&&value<=TimeSpan.FromDays(30);
}

internal sealed record VideoAudioSpan(VideoAudioLane Lane,string File,TimeSpan At,TimeSpan InPoint,TimeSpan Length,double Speed=1,TimeSpan? SourceLength=null);

internal static class VideoAudioPlan
{
    internal static VideoAudioTrack Settings(VideoProject project,VideoAudioLane lane)=>
        project.AudioTracks.FirstOrDefault(track=>track.Lane==lane)??new(lane);

    internal static IReadOnlyList<VideoAudioSpan> Create(VideoProject project,Func<string,bool> exists)
    {
        var spans=new List<VideoAudioSpan>();
        var offset=TimeSpan.Zero;
        foreach(var clip in project.Clips){
            var video=project.Input(clip.SourceIndex).Path;
            string mic=video+".mic.wav",system=video+".system.wav";
            bool hasMic=exists(mic),hasSystem=exists(system);
            if(hasMic)spans.Add(new(VideoAudioLane.Microphone,mic,offset,clip.Start,clip.MovingDuration,clip.Timing.Speed,clip.SourceDuration));
            if(hasSystem)spans.Add(new(VideoAudioLane.System,system,offset,clip.Start,clip.MovingDuration,clip.Timing.Speed,clip.SourceDuration));
            if(!hasMic&&!hasSystem)spans.Add(new(VideoAudioLane.Original,video,offset,clip.Start,clip.MovingDuration,clip.Timing.Speed,clip.SourceDuration));
            offset+=clip.Duration;
        }
        var music=Settings(project,VideoAudioLane.Music);
        if(music.File!=null&&music.Offset<offset){
            var length=music.OutPoint-music.InPoint;
            if(length>offset-music.Offset)length=offset-music.Offset;
            spans.Add(new(VideoAudioLane.Music,music.File,music.Offset,music.InPoint,length));
        }
        return spans;
    }

    internal static bool NeedsMix(VideoProject project)=>project.AudioTracks.Length>0||project.Clips.Any(clip=>clip.Timing.ChangesAudio);
}
