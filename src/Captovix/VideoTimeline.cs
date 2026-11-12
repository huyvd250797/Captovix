namespace Captovix;

internal sealed class VideoTimeline(VideoProject project)
{
    private readonly List<VideoProject> _undo=[];
    private readonly List<VideoProject> _redo=[];
    internal VideoProject Project {get;private set;}=project.Upgrade();
    internal bool CanUndo=>_undo.Count>0;
    internal bool CanRedo=>_redo.Count>0;
    internal VideoRange Selected=>Project.Clips[Project.SelectedClip];

    internal (int Index,TimeSpan SourcePosition) Map(TimeSpan position)
    {
        var clips=Project.Clips;
        TimeSpan offset=TimeSpan.Zero;
        for(int i=0;i<clips.Length;i++){
            var clip=clips[i];
            if(position<offset+clip.Duration||i==clips.Length-1)
                return (i,clip.Start+Clamp(position-offset,TimeSpan.Zero,clip.Duration));
            offset+=clip.Duration;
        }
        throw new InvalidOperationException();
    }

    internal TimeSpan Offset(int index)
    {
        long ticks=0;
        for(int i=0;i<index;i++)ticks+=Project.Clips[i].Duration.Ticks;
        return TimeSpan.FromTicks(ticks);
    }

    internal void Seek(TimeSpan position)
    {
        position=Clamp(position,TimeSpan.Zero,Project.Duration);
        Project=Project with{Position=position,SelectedClip=Map(position).Index};
    }

    internal void Track(int index,TimeSpan sourcePosition)
    {
        var clip=Project.Clips[index];
        var position=Offset(index)+Clamp(sourcePosition-clip.Start,TimeSpan.Zero,clip.Duration);
        Project=Project with{Position=position,SelectedClip=index};
    }

    internal bool Split(TimeSpan minimum)
    {
        var (index,sourcePosition)=Map(Project.Position);
        if(Project.Clips.Length>=VideoProject.MaximumClips||
            !Project.Clips[index].TrySplit(sourcePosition,minimum,out var left,out var right))return false;
        if(Project.NextClipNumber==int.MaxValue)return false;
        right=right with{Number=Project.NextClipNumber};
        Change([..Project.Clips.Take(index),left,right,..Project.Clips.Skip(index+1)],index+1,Project.Position,
            Project with{NextClipNumber=Project.NextClipNumber+1});
        return true;
    }

    internal bool Trim(bool start,TimeSpan minimum)
    {
        int index=Project.SelectedClip;
        var clip=Project.Clips[index];
        var source=clip.Start+Clamp(Project.Position-Offset(index),TimeSpan.Zero,clip.Duration);
        var updated=start?clip with{Start=source}:clip with{End=source};
        if(updated==clip||updated.Duration<minimum)return false;
        var clips=(VideoRange[])Project.Clips.Clone();
        clips[index]=updated;
        Change(clips,index,start?Offset(index):Project.Position);
        return true;
    }

    internal bool Delete()
    {
        if(Project.Clips.Length<=1)return false;
        int index=Project.SelectedClip;
        var clips=Project.Clips.Where((_,i)=>i!=index).ToArray();
        int selected=Math.Min(index,clips.Length-1);
        Change(clips,selected,Offset(selected));
        return true;
    }

    internal bool Move(int from,int insertion)
    {
        var clips=Project.Clips.ToList();
        if(from<0||from>=clips.Count||insertion<0||insertion>clips.Count)return false;
        int target=insertion>from?insertion-1:insertion;
        if(target==from)return false;
        var clip=clips[from];
        var local=Project.SelectedClip==from?Project.Position-Offset(from):TimeSpan.Zero;
        clips.RemoveAt(from);
        clips.Insert(target,clip);
        var offset=TimeSpan.FromTicks(clips.Take(target).Sum(item=>item.Duration.Ticks));
        Change([..clips],target,offset+local);
        return true;
    }

    internal bool Append(VideoInput input)
    {
        if(Project.NextClipNumber==int.MaxValue||Project.Clips.Length>=VideoProject.MaximumClips||Project.Duration+input.Duration>TimeSpan.FromDays(30))return false;
        input.Validate();
        var project=Project;
        int index=Array.FindIndex(project.Inputs.ToArray(),existing=>existing==input);
        if(index<0){
            index=project.AdditionalSources.Length+1;
            project=project with{AdditionalSources=[..project.AdditionalSources,input]};
        }
        Change([..project.Clips,new VideoRange(TimeSpan.Zero,input.Duration){SourceIndex=index,Number=project.NextClipNumber}],
            project.Clips.Length,project.Duration,project with{NextClipNumber=project.NextClipNumber+1});
        return true;
    }

    internal bool SetCanvas(VideoCanvasLayout canvas,VideoClipLayout layout)
    {
        if(!canvas.IsValid()||!layout.IsValid())return false;
        var clips=(VideoRange[])Project.Clips.Clone();
        var clipLayout=layout==VideoClipLayout.Default?null:layout;
        if(canvas==Project.Canvas&&Selected.Layout==clipLayout)return false;
        clips[Project.SelectedClip]=Selected with{Layout=clipLayout};
        Change(clips,Project.SelectedClip,Project.Position,Project with{Canvas=canvas});
        return true;
    }

    internal bool SetTrack(VideoMarkTrack track)
    {
        if(track.Id==Guid.Empty||!track.Mark.IsValid()||track.Start<TimeSpan.Zero
            ||track.End>Project.Duration||track.End<=track.Start)return false;
        int index=Array.FindIndex(Project.Tracks,item=>item.Id==track.Id);
        if(index<0&&Project.Tracks.Length>=VideoProject.MaximumTracks)return false;
        if(index>=0&&Project.Tracks[index]==track)return false;
        var tracks=index<0?[..Project.Tracks,track]:(VideoMarkTrack[])Project.Tracks.Clone();
        if(index>=0)tracks[index]=track;
        Change(Project.Clips,Project.SelectedClip,Project.Position,Project with{Tracks=tracks});
        return true;
    }

    internal bool RemoveTrack(Guid id)
    {
        var tracks=Project.Tracks.Where(track=>track.Id!=id).ToArray();
        if(tracks.Length==Project.Tracks.Length)return false;
        Change(Project.Clips,Project.SelectedClip,Project.Position,Project with{Tracks=tracks});
        return true;
    }

    internal bool SetAudio(VideoAudioTrack track)
    {
        if(!track.IsValid())return false;
        var current=VideoAudioPlan.Settings(Project,track.Lane);
        if(track==current)return false;
        var tracks=Project.AudioTracks.Where(item=>item.Lane!=track.Lane).ToList();
        if(track.Volume!=100||track.Muted||track.Solo||track.FadeIn!=TimeSpan.Zero||track.FadeOut!=TimeSpan.Zero
            ||track.File!=null||track.Cuts.Length>0)tracks.Add(track);
        Change(Project.Clips,Project.SelectedClip,Project.Position,Project with{AudioTracks=[..tracks]});
        return true;
    }

    internal bool Undo()=>Restore(_undo,_redo);
    internal bool Redo()=>Restore(_redo,_undo);

    private bool Restore(List<VideoProject> source,List<VideoProject> destination)
    {
        if(source.Count==0)return false;
        destination.Add(Project);
        Project=source[^1];
        source.RemoveAt(source.Count-1);
        return true;
    }

    private void Change(VideoRange[] clips,int selection,TimeSpan position,VideoProject? source=null)
    {
        if(_undo.Count>=100)_undo.RemoveAt(0);
        _undo.Add(Project);
        _redo.Clear();
        Project=(source??Project) with{Version=VideoProject.CurrentVersion,Clips=clips,SelectedClip=selection};
        var duration=Project.Duration;
        var selectedStart=Offset(selection);
        var tracks=Project.Tracks;
        if(tracks.Any(track=>track.End>duration))
            tracks=tracks.Select(track=>track.Slice(TimeSpan.Zero,duration)).OfType<VideoMarkTrack>().ToArray();
        Project=Project with{Position=Clamp(position,selectedStart,selectedStart+clips[selection].Duration),Tracks=tracks};
    }

    private static TimeSpan Clamp(TimeSpan value,TimeSpan min,TimeSpan max)=>value<min?min:value>max?max:value;
}
