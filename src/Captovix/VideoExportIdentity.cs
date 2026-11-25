using System.IO;
namespace Captovix;
internal static class VideoExportIdentity
{
    internal static async Task CopyAsync(string source,string temporary,CancellationToken cancellation){
        await using(var input=new FileStream(source,FileMode.Open,FileAccess.Read,FileShare.Read,1024*1024,true))
        await using(var output=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None,1024*1024,true)){
            await input.CopyToAsync(output,cancellation);await output.FlushAsync(cancellation);
        }
        RecordingContainer.Verify(temporary);
    }
    internal static bool CanCopy(VideoProject project,IReadOnlyList<VideoRange> ranges){
        if(ranges.Count==0||project.Canvas!=VideoCanvasLayout.Default||project.Tracks.Length>0||VideoAudioPlan.NeedsMix(project)
            ||project.Cameras.Values.Any(camera=>camera.Visible)||project.ShowCues&&project.Cues.Any(cue=>cue.Kind!="marker"))return false;
        int source=ranges[0].SourceIndex;
        if(ranges[0].Start!=TimeSpan.Zero||ranges[^1].End!=project.Input(source).Duration)return false;
        for(int i=0;i<ranges.Count;i++){
            var clip=ranges[i];var timing=clip.Timing;
            if(clip.SourceIndex!=source||clip.Layout is {} layout&&layout!=VideoClipLayout.Default||timing.ChangesAudio||timing.Keys.Length>0||timing.FadeIn!=0||timing.FadeOut!=0)return false;
            if(i>0&&ranges[i-1].End!=clip.Start)return false;
        }
        return true;
    }
}
