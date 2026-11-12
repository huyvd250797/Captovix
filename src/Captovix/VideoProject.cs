using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace Captovix;

internal sealed record VideoSourceIdentity(long Length,string SampleHash)
{
    internal static VideoSourceIdentity Read(string path)
    {
        using var stream=File.OpenRead(path);
        const int sample=65536;
        var bytes=new byte[(int)Math.Min(stream.Length,2L*sample)];
        if(stream.Length<=bytes.Length)stream.ReadExactly(bytes);
        else{
            stream.ReadExactly(bytes.AsSpan(0,sample));
            stream.Position=stream.Length-sample;
            stream.ReadExactly(bytes.AsSpan(sample));
        }
        return new(stream.Length,Convert.ToHexString(SHA256.HashData(bytes)));
    }
}

internal sealed record VideoInput(string Path,VideoSourceIdentity? Identity,TimeSpan Duration)
{
    internal void Validate()
    {
        if(string.IsNullOrWhiteSpace(Path)||Path.Length>32767||!System.IO.Path.IsPathFullyQualified(Path)
            ||!string.Equals(System.IO.Path.GetExtension(Path),".mp4",StringComparison.OrdinalIgnoreCase)
            ||Duration<=TimeSpan.Zero||Duration>TimeSpan.FromDays(30)
            ||Identity is {} identity&&(identity.Length<=0||identity.SampleHash is not {Length:64}||!identity.SampleHash.All(Uri.IsHexDigit)))
            throw new InvalidDataException("Nguồn video của dự án không hợp lệ.");
    }
}

internal sealed record VideoProject(int Version,string SourcePath,VideoSourceIdentity? Source,
    TimeSpan SourceDuration,VideoRange[] Clips,TimeSpan Position,int SelectedClip)
{
    internal const int CurrentVersion=4,MaximumClips=1000,MaximumMarks=64,MaximumTracks=256;
    public VideoAudioTrack[] AudioTracks {get;init;}=[];
    public VideoCanvasLayout Canvas {get;init;}=VideoCanvasLayout.Default;
    public VideoMarkTrack[] Tracks {get;init;}=[];
    public int NextClipNumber {get;init;}=1;
    internal VideoProject Upgrade()
    {
        if(Version>=3)return this with{Version=CurrentVersion};
        var tracks=new List<VideoMarkTrack>();
        var offset=TimeSpan.Zero;
        var clips=new VideoRange[Clips.Length];
        for(int i=0;i<Clips.Length;i++){
            var clip=Clips[i];
            var end=offset+clip.Duration;
            foreach(var mark in clip.Marks??[])tracks.Add(new(Guid.NewGuid(),offset,end,mark));
            clips[i]=clip with{Number=i+1,Marks=null};
            offset=end;
        }
        return this with{Version=CurrentVersion,Clips=clips,Tracks=[..tracks],NextClipNumber=Clips.Length+1};
    }

    public VideoInput[] AdditionalSources {get;init;}=[];
    internal VideoInput Input(int index)=>index==0?new(SourcePath,Source,SourceDuration):AdditionalSources[index-1];
    internal IEnumerable<VideoInput> Inputs=>Enumerable.Range(0,AdditionalSources.Length+1).Select(Input);
    internal TimeSpan Duration=>TimeSpan.FromTicks(Clips.Sum(clip=>clip.Duration.Ticks));

    internal void Validate()
    {
        if(Version is <1 or >CurrentVersion)
            throw new NotSupportedException("Phiên bản dự án video chưa được hỗ trợ.");
        if(AdditionalSources is null||AdditionalSources.Length>=MaximumClips
            ||Version==1&&AdditionalSources.Length>0||Clips is null||Clips.Length is <1 or >MaximumClips
            ||SelectedClip<0||SelectedClip>=Clips.Length)
            throw new InvalidDataException("Dữ liệu dự án video không hợp lệ.");
        foreach(var input in Inputs){
            if(input==null)throw new InvalidDataException("Thiếu thông tin video nguồn.");
            input.Validate();
        }
        TimeSpan previousEnd=TimeSpan.Zero;
        foreach(var clip in Clips){
            if(clip.SourceIndex<0||clip.SourceIndex>AdditionalSources.Length||clip.Start<TimeSpan.Zero
                ||clip.End<=clip.Start||clip.End>Input(clip.SourceIndex).Duration
                ||Version==1&&(clip.Start<previousEnd||clip.Marks is not null)
                ||clip.Marks is {} marks&&(marks.Length>MaximumMarks||marks.Any(mark=>mark==null||!mark.IsValid())))
                throw new InvalidDataException("Mốc cắt hoặc chú thích trong dự án không hợp lệ.");
            previousEnd=clip.End;
        }
        var duration=Duration;
        if(duration>TimeSpan.FromDays(30))throw new InvalidDataException("Timeline vượt giới hạn thời lượng.");
        if(Version>=3){
            if(Canvas==null||!Canvas.IsValid()||Tracks==null||Tracks.Length>MaximumClips*MaximumMarks
                ||Clips.Any(clip=>clip.Number<=0||clip.Number>=NextClipNumber||clip.Marks!=null||clip.Layout?.IsValid()==false)
                ||Clips.Select(clip=>clip.Number).Distinct().Count()!=Clips.Length
                ||Tracks.Any(track=>track==null||track.Id==Guid.Empty||track.Start<TimeSpan.Zero
                    ||track.End<=track.Start||track.End>duration||track.Mark==null||!track.Mark.IsValid())
                ||Tracks.Select(track=>track.Id).Distinct().Count()!=Tracks.Length)
                throw new InvalidDataException("Bố cục, tên đoạn hoặc track chú thích không hợp lệ.");
        }
        if(AudioTracks is null||AudioTracks.Length>4||AudioTracks.Any(track=>track==null||!track.IsValid())
            ||AudioTracks.Select(track=>track.Lane).Distinct().Count()!=AudioTracks.Length)
            throw new InvalidDataException("Dữ liệu track âm thanh không hợp lệ.");
        var selectionStart=TimeSpan.FromTicks(Clips.Take(SelectedClip).Sum(clip=>clip.Duration.Ticks));
        if(Position<selectionStart||Position>selectionStart+Clips[SelectedClip].Duration)
            throw new InvalidDataException("Vị trí phát trong dự án không hợp lệ.");
    }

    internal static VideoProject Open(string path)
    {
        using var file=File.OpenRead(path);
        if(file.Length>32L*1024*1024)throw new InvalidDataException("Tệp dự án video vượt quá 32 MB.");
        var project=JsonSerializer.Deserialize<VideoProject>(file)??throw new InvalidDataException("Tệp dự án video trống.");
        project.Validate();
        return project;
    }

    internal static VideoProject? Recover(string path,out bool restored)
    {
        restored=false;
        try{return Open(path);}
        catch(FileNotFoundException) when(!File.Exists(path+".bak")){return null;}
        catch(DirectoryNotFoundException){return null;}
        catch(Exception error) when(error is JsonException or InvalidDataException or FileNotFoundException){
            if(!File.Exists(path+".bak"))throw;
            var backup=Open(path+".bak");
            // Restore the valid revision before the next save rotates the backup.
            backup.Save(path,keepBackup:false);
            restored=true;
            return backup;
        }
    }

    internal void Save(string path,bool keepBackup=true)
    {
        if(!string.Equals(Path.GetExtension(path),".cvxvideo",StringComparison.OrdinalIgnoreCase))
            throw new IOException("Chọn tệp dự án có đuôi .cvxvideo.");
        Validate();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temporary=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{
            using(var file=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None)){
                JsonSerializer.Serialize(file,this);
                if(file.Length>32L*1024*1024)throw new InvalidDataException("Tệp dự án video vượt quá 32 MB.");
                file.Flush(flushToDisk:true);
            }
            if(keepBackup&&File.Exists(path))File.Replace(temporary,path,path+".bak",ignoreMetadataErrors:true);
            else File.Move(temporary,path,true);
        }finally{File.Delete(temporary);}
    }
}
