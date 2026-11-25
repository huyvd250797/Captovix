using Captovix;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

int checks=0;
void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS "+name);checks++;}
string ffmpeg=VideoTrimExport.FindFfmpeg()!;
string root=Path.Combine(Path.GetTempPath(),"cvx-export-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
TimeSpan S(double seconds)=>TimeSpan.FromSeconds(seconds);
byte[] Run(IEnumerable<string> args){
    var info=new ProcessStartInfo(ffmpeg){UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true,RedirectStandardOutput=true,WorkingDirectory=root};
    foreach(var arg in new[]{"-nostdin","-hide_banner","-loglevel","error"}.Concat(args))info.ArgumentList.Add(arg);
    using var process=Process.Start(info)!;var error=process.StandardError.ReadToEndAsync();
    using var output=new MemoryStream();process.StandardOutput.BaseStream.CopyTo(output);process.WaitForExit();
    if(process.ExitCode!=0)throw new Exception(error.GetAwaiter().GetResult());return output.ToArray();
}
try{
    // Real frame/audio data; edit order blue -> red -> green, with a speed change and hold.
    string input=Path.Combine(root,"nguon co khoang trang.mp4");
    Run(["-f","lavfi","-i","color=red:s=160x90:r=30:d=1","-f","lavfi","-i","sine=frequency=440:sample_rate=48000:duration=1",
        "-f","lavfi","-i","color=green:s=160x90:r=30:d=1","-f","lavfi","-i","sine=frequency=880:sample_rate=48000:duration=1",
        "-f","lavfi","-i","color=blue:s=160x90:r=30:d=1","-f","lavfi","-i","sine=frequency=1320:sample_rate=48000:duration=1",
        "-filter_complex_threads","1","-filter_complex","[0:v][1:a][2:v][3:a][4:v][5:a]concat=n=3:v=1:a=1[v][a]",
        "-map","[v]","-map","[a]","-threads","1","-c:v","libx264","-c:a","aac",input]);
    var clips=new[]{new VideoRange(S(2),S(3)){Number=3},new VideoRange(S(.2),S(.8)){Number=1},new VideoRange(S(1),S(1.7)){Number=2,Motion=new(){Speed=2,Hold=S(.2)}}};
    var project=new VideoProject(VideoProject.CurrentVersion,input,VideoSourceIdentity.Read(input),S(3),clips,S(0),0);
    TimeSpan offset=S(0);
    for(int i=0;i<clips.Length;i++){
        var clip=clips[i];long frames=VideoExportTiming.Frames(offset,clip.Duration,30);
        Run(VideoExportCommands.Normalize(input,$"part-{i}.mp4",VideoCanvasGeometry.Create(VideoCanvasLayout.Default,VideoClipLayout.Default,160,90,160,90).Filter(160,90,"#000000"),clip.Start,clip.SourceDuration,clip.Duration,clip.Timing,160,90,30,false,frames));offset+=clip.Duration;
    }
    string manifest=Path.Combine(root,"concat.txt");File.WriteAllLines(manifest,Enumerable.Range(0,3).Select(i=>$"file 'part-{i}.mp4'"));
    string picture=Path.Combine(root,"picture.mp4");Run(VideoExportCommands.Concat(manifest,picture,offset));
    Check(VideoCanvasGeometry.Create(VideoCanvasLayout.Default,VideoClipLayout.Default,1920,1080,1920,1080).Filter(1920,1080,"#000000")=="null","1080p identity canvas skips scale and chroma resampling");
    var splitIdentity=project with{Clips=[new(S(0),S(1)),new(S(1),S(2)),new(S(2),S(3))]};
    Check(VideoExportIdentity.CanCopy(splitIdentity,splitIdentity.Clips),"Contiguous Split without content changes uses original bytes");
    Check(!VideoExportIdentity.CanCopy(project,clips),"Reordered trimmed motion timeline must render");
    Check(!VideoExportIdentity.CanCopy(splitIdentity,[new(S(.1),S(3))]),"Trim cannot use whole-file copy");
    string identical=Path.Combine(root,"identical.mp4");await VideoExportIdentity.CopyAsync(input,identical,CancellationToken.None);
    Check(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(input)).SequenceEqual(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(identical))),"Production passthrough is byte-exact, preserving video and sound");
    Check(VideoCanvasGeometry.Create(new(VideoAspect.Landscape),new(Right:.5),3840,1080,1920,1080).Filter(1920,1080,"#000000")!="null","A cropped half-width source must still crop");
    var fine=new byte[1920*1080*3];for(int y=0;y<1080;y++)for(int x=0;x<1920;x++){byte shade=(byte)((x%17<2||y%29<2||((x/7+y/13)%11==0&&x%7<3))?25:235);int pos=(y*1920+x)*3;fine[pos]=fine[pos+1]=fine[pos+2]=shade;}
    string detail=Path.Combine(root,"detail.rgb");File.WriteAllBytes(detail,fine);
    Run(["-f","rawvideo","-pixel_format","rgb24","-video_size","1920x1080","-framerate","1","-i",detail,"-frames:v","1","-pix_fmt","yuv420p","-c:v","libx264","-crf","0","detail.mp4"]);
    Run(VideoExportCommands.Normalize("detail.mp4","detail-export.mp4",VideoCanvasGeometry.Create(VideoCanvasLayout.Default,VideoClipLayout.Default,1920,1080,1920,1080).Filter(1920,1080,"#000000"),S(0),S(1),S(1),VideoMotion.Default,1920,1080,1,false,1));
    var before=Run(["-i","detail.mp4","-frames:v","1","-pix_fmt","yuv420p","-f","rawvideo","pipe:1"]);var after=Run(["-i","detail-export.mp4","-frames:v","1","-pix_fmt","yuv420p","-f","rawvideo","pipe:1"]);
    double lumaError=0;for(int i=0;i<1920*1080;i++)lumaError+=Math.Abs(before[i]-after[i]);lumaError/=1920*1080;
    Check(lumaError<1.5,"1080p fine UI-line fixture luma MAE below1.5: "+lumaError.ToString("0.000"));
    var preview=await VideoPreviewFrames.ReadAsync(Path.Combine(root,"detail.mp4"),S(0),3840,2160,CancellationToken.None);
    Check(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(preview.AsSpan(16,4))==1920&&System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(preview.AsSpan(20,4))==1080,"Paused preview keeps true1080p PNG dimensions");
    var mix=await VideoAudioRender.RenderAsync(project,root,false,CancellationToken.None);
    string final=Path.Combine(root,"result.mp4");
    Run(["-i",picture,"-i",mix.Path,"-t",offset.TotalSeconds.ToString(CultureInfo.InvariantCulture),"-map","0:v:0","-map","1:a:0","-c:v","copy","-c:a","aac",final]);
    var probe=await VideoProbe.ReadAsync(final,CancellationToken.None);Check(probe.Width==160&&probe.Height==90&&Math.Abs(probe.Duration.TotalSeconds-offset.TotalSeconds)<.07,"Export verified without Windows player or MediaClip");
    RecordingContainer.Verify(final);Check(File.Exists(final),"Produces a complete MP4 after split/reorder/speed/hold");
    foreach(var sample in new[]{(At:.4,Color:2),(At:1.3,Color:0),(At:1.8,Color:1),(At:2.05,Color:1)}){
        var rgb=Run(["-ss",sample.At.ToString(CultureInfo.InvariantCulture),"-i",final,"-frames:v","1","-vf","scale=1:1","-pix_fmt","rgb24","-f","rawvideo","pipe:1"]);
        Check(rgb.Length==3&&rgb[sample.Color]>70&&rgb[sample.Color]>rgb[(sample.Color+1)%3]*2,"Correct edited color at "+sample.At);
    }
    var pcm=Run(["-i",final,"-vn","-ac","1","-ar","48000","-f","s16le","pipe:1"]);
    double Frequency(double start,double end){var values=MemoryMarshal.Cast<byte,short>(pcm);int crossings=0;for(int i=(int)(start*48000)+1;i<(int)(end*48000);i++)if(values[i-1]<0&&values[i]>=0)crossings++;return crossings/(end-start);}
    Check(Math.Abs(Frequency(.3,.5)-1320)<30,"Audio follows blue clip after reorder");
    Check(Math.Abs(Frequency(1.2,1.4)-440)<30,"Audio follows trimmed red clip");
    Check(Math.Abs(Frequency(1.68,1.85)-880)<40,"Speed change retains audio pitch");
    var holdSamples=MemoryMarshal.Cast<byte,short>(pcm).Slice((int)(2.06*48000),(int)(.05*48000)).ToArray();
    Check(holdSamples.Max(sample=>Math.Abs((int)sample))<100,"Frozen tail does not repeat source audio");
    Check(pcm.Length/2/48000d>=offset.TotalSeconds-.04&&pcm.Length/2/48000d<offset.TotalSeconds+.06,"Output audio duration matches edited timeline");
    long accumulated=0;TimeSpan time=S(0);for(int i=0;i<1000;i++){var length=S(.043);accumulated+=VideoExportTiming.Frames(time,length,29.97);time+=length;}
    Check(accumulated==VideoExportTiming.FrameAt(time,29.97),"1000 cuts accumulate no frame rounding drift");
    Console.WriteLine($"{checks} real export checks passed");
}finally{Directory.Delete(root,true);}

namespace Captovix{
    internal static class VideoTrimExport{
        internal static string? FindFfmpeg()=>Environment.GetEnvironmentVariable("FFMPEG_EXE")??(OperatingSystem.IsWindows()?Path.GetFullPath("media-engine/ffmpeg.exe"):"/usr/bin/ffmpeg");
    }
}
