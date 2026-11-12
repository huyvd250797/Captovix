using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Captovix;

internal sealed record VideoAudioMix(string Path,float[][] Peaks);

internal static class VideoAudioRender
{
    private const int Rate=48000,Channels=2,Block=4096,Buckets=1024;
    private static long Frame(TimeSpan time)=>(long)Math.Round(time.TotalSeconds*Rate);

    internal static async Task<VideoAudioMix> RenderAsync(VideoProject project,string directory,bool waveform,
        CancellationToken cancellation)
    {
        var ffmpeg=VideoTrimExport.FindFfmpeg()??throw new IOException("Chỉnh âm thanh và waveform cần FFmpeg. Đặt ffmpeg.exe cạnh Captovix hoặc thêm vào PATH.");
        Directory.CreateDirectory(directory);
        long frames=Frame(project.Duration);
        if(frames*Channels*2>uint.MaxValue-36)
            throw new IOException("Bản phối WAV vượt 4 GiB. Hãy chia dự án thành các phần ngắn hơn.");
        var spans=VideoAudioPlan.Create(project,File.Exists);
        var decoded=new Dictionary<string,string?>(StringComparer.OrdinalIgnoreCase);
        var readers=new Dictionary<string,FileStream>(StringComparer.OrdinalIgnoreCase);
        var settings=Enum.GetValues<VideoAudioLane>().Select(lane=>VideoAudioPlan.Settings(project,lane)).ToArray();
        var activeLanes=spans.Select(span=>span.Lane).ToHashSet();
        bool solo=settings.Any(track=>track.Solo&&!track.Muted&&activeLanes.Contains(track.Lane));
        float[][] peaks=Enumerable.Range(0,settings.Length).Select(_=>new float[Buckets]).ToArray();
        string output=Path.Combine(directory,$"mix-{Guid.NewGuid():N}.wav");
        try{
            var rendered=new List<(VideoAudioSpan Span,FileStream Reader,long Start,long End,long Source)>();
            foreach(var span in spans){
                cancellation.ThrowIfCancellationRequested();
                if(!decoded.TryGetValue(span.File,out string? pcm)){
                    pcm=await DecodeAsync(ffmpeg,span.File,directory,span.Lane==VideoAudioLane.Original,cancellation);
                    decoded.Add(span.File,pcm);
                    if(pcm!=null)readers.Add(span.File,new FileStream(pcm,FileMode.Open,FileAccess.Read,FileShare.Read));
                }
                if(pcm!=null)rendered.Add((span,readers[span.File],Frame(span.At),Frame(span.At+span.Length),Frame(span.InPoint)));
            }
            using var writer=new RecordingWaveFile(output,Rate,Channels);
            var buffer=new byte[Block*Channels*2];
            var samples=new float[Block*Channels];
            var ordered=rendered.OrderBy(span=>span.Start).ToArray();
            var active=new List<(VideoAudioSpan Span,FileStream Reader,long Start,long End,long Source)>();
            long[] laneStarts=new long[settings.Length],laneEnds=new long[settings.Length];
            for(int i=0;i<settings.Length;i++){
                var items=ordered.Where(x=>(int)x.Span.Lane==i).ToArray();
                laneStarts[i]=items.Length==0?0:items.Min(x=>x.Start);
                laneEnds[i]=items.Length==0?0:items.Max(x=>x.End);
            }
            var cuts=settings.Select(s=>s.Cuts.Select(cut=>(Start:Frame(cut.Start),End:Frame(cut.End))).ToArray()).ToArray();
            int next=0;
            for(long start=0;start<frames;start+=Block){
                cancellation.ThrowIfCancellationRequested();
                int count=(int)Math.Min(Block,frames-start);
                Array.Clear(samples,0,count*Channels);
                active.RemoveAll(item=>item.End<=start);
                while(next<ordered.Length&&ordered[next].Start<start+count)active.Add(ordered[next++]);
                foreach(var item in active){
                    int lane=(int)item.Span.Lane;
                    var track=settings[lane];
                    bool audible=!track.Muted&&(!solo||track.Solo);
                    if(!audible&&!waveform)continue;
                    long first=Math.Max(start,item.Start),last=Math.Min(start+count,item.End);
                    long source=item.Source+first-item.Start;
                    int bytes=(int)Math.Min((last-first)*Channels*2,Math.Max(0,item.Reader.Length-source*Channels*2));
                    if(bytes<=0)continue;
                    item.Reader.Position=source*Channels*2;
                    item.Reader.ReadExactly(buffer.AsSpan(0,bytes));
                    long fadeIn=Frame(track.FadeIn),fadeOut=Frame(track.FadeOut);
                    MixBlock(buffer,bytes,samples,peaks[lane],laneStarts[lane],laneEnds[lane],first,start,frames,
                        track.Volume/100,fadeIn,fadeOut,cuts[lane],audible,waveform);
                }
                writer.Write(start,samples.AsSpan(0,count*Channels));
            }
            writer.Complete(frames);
            return new(output,peaks);
        }catch{File.Delete(output);throw;}
        finally{
            foreach(var reader in readers.Values)reader.Dispose();
            foreach(var path in decoded.Values)if(path!=null)File.Delete(path);
        }
    }

    private static void MixBlock(byte[] buffer,int bytes,float[] output,float[] peaks,long laneStart,long laneEnd,
        long first,long blockStart,long frames,double volume,long fadeIn,long fadeOut,(long Start,long End)[] cuts,bool audible,bool waveform)
    {
        var source=MemoryMarshal.Cast<byte,short>(buffer.AsSpan(0,bytes));
        for(int i=0;i+1<source.Length;i+=Channels){
            long frame=first+i/Channels;
            if(IsCut(frame,cuts))continue;
            double gain=volume;
            if(fadeIn>0&&frame<laneStart+fadeIn)gain*=Math.Clamp((double)(frame-laneStart)/fadeIn,0,1);
            if(fadeOut>0&&frame>=laneEnd-fadeOut)gain*=Math.Clamp((double)(laneEnd-frame-1)/fadeOut,0,1);
            float left=(float)(source[i]*gain/32768),right=(float)(source[i+1]*gain/32768);
            if(waveform){
                int index=(int)Math.Min(peaks.Length-1,frame*peaks.Length/frames);
                peaks[index]=Math.Max(peaks[index],Math.Min(1,Math.Max(Math.Abs(left),Math.Abs(right))));
            }
            if(audible){
                int index=(int)(frame-blockStart)*Channels;
                output[index]+=left;
                output[index+1]+=right;
            }
        }
    }

    private static bool IsCut(long frame,(long Start,long End)[] cuts)
    {
        foreach(var cut in cuts)if(frame>=cut.Start&&frame<cut.End)return true;
        return false;
    }

    internal static async Task<TimeSpan> MusicDurationAsync(string source,CancellationToken cancellation)
    {
        var ffmpeg=VideoTrimExport.FindFfmpeg()??throw new IOException("Cần FFmpeg để đọc nhạc nền.");
        var directory=Path.Combine(Path.GetTempPath(),"CaptovixMusic-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try{
            var pcm=await DecodeAsync(ffmpeg,source,directory,false,cancellation);
            var frames=new FileInfo(pcm!).Length/(Channels*2);
            if(frames<=0)throw new InvalidDataException("Tệp nhạc không có âm thanh.");
            return TimeSpan.FromSeconds((double)frames/Rate);
        }finally{Directory.Delete(directory,true);}
    }

    private static async Task<string?> DecodeAsync(string ffmpeg,string source,string directory,bool optional,CancellationToken cancellation)
    {
        if(!File.Exists(source))throw new FileNotFoundException("Thiếu nguồn âm thanh: "+Path.GetFileName(source));
        string output=Path.Combine(directory,Guid.NewGuid().ToString("N")+".pcm");
        var start=new ProcessStartInfo(ffmpeg){UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true};
        foreach(var argument in new[]{"-nostdin","-hide_banner","-loglevel","error","-i",source,"-map","0:a:0",
            "-vn","-sn","-dn","-ac","2","-ar","48000","-c:a","pcm_s16le","-f","s16le",output})
            start.ArgumentList.Add(argument);
        using var process=Process.Start(start)??throw new IOException("Không mở được FFmpeg.");
        using var registration=cancellation.Register(()=>{
            try{if(!process.HasExited)process.Kill(entireProcessTree:true);}
            catch(InvalidOperationException){}
            catch(System.ComponentModel.Win32Exception){}
        });
        var stderr=process.StandardError.ReadToEndAsync();
        try{
            await process.WaitForExitAsync();
            string error=await stderr;
            cancellation.ThrowIfCancellationRequested();
            if(process.ExitCode==0)return output;
            if(optional&&error.Contains("matches no streams",StringComparison.OrdinalIgnoreCase))return null;
            throw new IOException("Không đọc được âm thanh từ "+Path.GetFileName(source)+". "+error.Trim());
        }finally{if(process.ExitCode!=0||cancellation.IsCancellationRequested)File.Delete(output);}
    }
}
