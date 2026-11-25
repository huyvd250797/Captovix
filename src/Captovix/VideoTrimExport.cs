using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

namespace Captovix;

internal static class VideoTrimExport
{
    internal static async Task<VideoInput> ReadInputAsync(string source)
    {
        source=Path.GetFullPath(source);
        var file=await StorageFile.GetFileFromPathAsync(source);
        var clip=await MediaClip.CreateFromFileAsync(file);
        var video=clip.GetVideoEncodingProperties();
        if(video.Width==0||video.Height==0||clip.OriginalDuration<=TimeSpan.Zero)
            throw new InvalidDataException("MP4 không có khung hình phát được.");
        var identity=await Task.Run(()=>VideoSourceIdentity.Read(source));
        return new(source,identity,clip.OriginalDuration);
    }

    internal static async Task<(int Width,int Height)> ReadDimensionsAsync(string source)
    {
        if(VideoProbe.Executable!=null){var metadata=await VideoProbe.ReadAsync(source,CancellationToken.None);return(metadata.Width,metadata.Height);}
        var info=await RecordingMedia.VerifyAsync(source,CancellationToken.None);
        return (info.Width,info.Height);
    }

    internal static async Task ExportAsync(VideoProject project,string destination,IReadOnlyList<VideoRange> ranges,
        TimeSpan timelineStart,IProgress<double> progress,CancellationToken cancellation)
    {
        destination=Path.GetFullPath(destination);
        if(project.Inputs.Select(input=>input.Path).Concat(project.Cameras.Values.Select(camera=>camera.Path))
            .Any(path=>Path.GetFullPath(path).Equals(destination,StringComparison.OrdinalIgnoreCase)))
            throw new IOException("Chọn tên xuất khác để giữ nguyên video gốc.");

        if(ranges.Count==0)throw new InvalidDataException("Timeline không có đoạn để xuất.");
        if(VideoExportIdentity.CanCopy(project,ranges)){
            string source=project.Input(ranges[0].SourceIndex).Path;
            string copy=Path.Combine(Path.GetDirectoryName(destination)!,".captovix-copy-"+Guid.NewGuid().ToString("N")+".mp4");
            try{
                await VideoExportIdentity.CopyAsync(source,copy,cancellation);cancellation.ThrowIfCancellationRequested();File.Move(copy,destination,true);progress.Report(100);return;
            }finally{if(File.Exists(copy))File.Delete(copy);}
        }
        var requestedDuration=TimeSpan.FromTicks(ranges.Sum(range=>range.Duration.Ticks));
        var tracks=project.Tracks.Select(track=>track.Slice(timelineStart,timelineStart+requestedDuration))
            .OfType<VideoMarkTrack>().ToArray();
        var cues=project.ShowCues?VideoCuePlan.Create(project.Clips,project.Cues)
            .Where(c=>c.Cue.Kind!="marker"&&c.End>timelineStart&&c.Start<timelineStart+requestedDuration)
            .Select(c=>c with{ClipAt=c.ClipAt-timelineStart,Start=c.Start>timelineStart?c.Start-timelineStart:TimeSpan.Zero,End=c.End-timelineStart})
            .ToArray():[];
        bool needsCues=cues.Length>0;
        bool needsBlur=tracks.Any(track=>track.Mark.SamplesVideo&&track.Mark.Style.Opacity>0);
        bool needsCamera=ranges.Any(range=>project.Cameras.GetValueOrDefault(range.SourceIndex) is {Visible:true} camera&&camera.ForClip(range).Any());
        bool needsCanvas=needsCamera||project.Canvas!=VideoCanvasLayout.Default||ranges.Any(range=>range.Layout!=null||range.Motion!=null);
        bool needsAudio=VideoAudioPlan.NeedsMix(project);
        string? ffmpeg=FindFfmpeg();
        // Normalize every cut to the same codec/timebase, then concatenate in timeline order.
        // Audio uses the same source-time mapping as preview, including split/reorder/speed.
        if(ranges.Count>1&&ffmpeg==null)throw new IOException("Thiếu bộ dựng video cho timeline nhiều đoạn. Dựng lại bản cập nhật bằng Build.cmd để tự cài FFmpeg, hoặc đặt ffmpeg.exe cạnh Captovix.exe.");
        bool concatenate=ffmpeg!=null;
        needsCanvas|=concatenate;
        needsAudio|=concatenate;
        if((needsBlur||needsCanvas||needsAudio||needsCues)&&ffmpeg==null)
            throw new IOException("Xuất bố cục, camera, làm mờ hoặc âm thanh đã chỉnh cần FFmpeg có libx264. Đặt ffmpeg.exe cạnh Captovix hoặc thêm vào PATH.");

        var work=Path.Combine(Path.GetDirectoryName(destination)!,$".captovix-{Guid.NewGuid():N}");
        Directory.CreateDirectory(work);
        try{
            var composition=new MediaComposition();
            var normalizedClips=new List<string>();
            var profile=MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
            var inputFiles=new Dictionary<int,StorageFile>();
            var sourceMetadata=new Dictionary<int,VideoProbeInfo>();
            var sourceSizes=new Dictionary<int,(int Width,int Height)>();
            (int Width,int Height) sourceSize=(1920,1080);
            if(project.Canvas.Aspect==VideoAspect.Source){
                int index=File.Exists(project.SourcePath)?0:ranges[0].SourceIndex;
                sourceSize=await ReadDimensionsAsync(project.Input(index).Path);
                sourceSizes[index]=sourceSize;
            }
            var (width,height)=project.Canvas.OutputSize(sourceSize.Width,sourceSize.Height);
            profile.Video.Width=(uint)width;
            profile.Video.Height=(uint)height;

            var blurRegions=new List<BlurRegion>();
            var inkRegions=new List<InkRegion>();
            TimeSpan offset=TimeSpan.Zero;
            double outputFps=30;
            for(int i=0;i<ranges.Count;i++){
                cancellation.ThrowIfCancellationRequested();
                var range=ranges[i];
                var path=project.Input(range.SourceIndex).Path;
                MediaClip? clip=null;
                int sourceWidth,sourceHeight;double fps;TimeSpan originalDuration;
                if(concatenate&&VideoProbe.Executable!=null){
                    if(!sourceMetadata.TryGetValue(range.SourceIndex,out var metadata)){metadata=await VideoProbe.ReadAsync(path,cancellation);sourceMetadata.Add(range.SourceIndex,metadata);}
                    sourceWidth=metadata.Width;sourceHeight=metadata.Height;fps=metadata.Fps;originalDuration=metadata.Duration;
                }else{
                    if(!inputFiles.TryGetValue(range.SourceIndex,out var input)){input=await StorageFile.GetFileFromPathAsync(path).AsTask(cancellation);inputFiles.Add(range.SourceIndex,input);}
                    clip=await MediaClip.CreateFromFileAsync(input).AsTask(cancellation);
                    var properties=clip.GetVideoEncodingProperties();sourceWidth=(int)properties.Width;sourceHeight=(int)properties.Height;
                    fps=properties.FrameRate.Denominator>0?(double)properties.FrameRate.Numerator/properties.FrameRate.Denominator:30;originalDuration=clip.OriginalDuration;
                }
                if(sourceWidth<=0||sourceHeight<=0)throw new InvalidDataException("Nguồn MP4 không có hình ảnh.");
                var end=range.End>originalDuration?originalDuration:range.End;
                if(range.Start<TimeSpan.Zero||end<=range.Start||range.End-end>TimeSpan.FromSeconds(Math.Max(.12,2/fps)))
                    throw new InvalidDataException($"Mốc cắt của Đoạn {range.Number} vượt thời lượng nguồn.");
                if(i==0){
                    outputFps=Math.Clamp(fps,1,120);
                    profile.Video.FrameRate.Numerator=(uint)Math.Round(outputFps*1000);
                    profile.Video.FrameRate.Denominator=1000;
                    profile.Video.Bitrate=(uint)Math.Clamp(width*(double)height*outputFps*.16,2_000_000,55_000_000);
                }
                var duration=end-range.Start;
                if(needsCues&&!sourceSizes.ContainsKey(range.SourceIndex))
                    sourceSizes.Add(range.SourceIndex,(sourceWidth,sourceHeight));
                if(needsCanvas){
                    if(!sourceSizes.TryGetValue(range.SourceIndex,out var size)){
                        size=await ReadDimensionsAsync(path);
                        sourceSizes.Add(range.SourceIndex,size);
                    }
                    var geometry=VideoCanvasGeometry.Create(project.Canvas,range.Layout??VideoClipLayout.Default,size.Width,size.Height,width,height);
                    var normalized=Path.Combine(work,$"canvas-{i}.mp4");
                    double completed=offset.TotalSeconds;
                    var moving=range.Timing.Playback(duration);
                    var renderedDuration=moving+range.Timing.Hold;
                    long? frameCount=concatenate?VideoExportTiming.Frames(offset,range.Duration,outputFps):null;
                    if(frameCount==0){offset+=range.Duration;continue;}
                    await RenderCanvasClipAsync(ffmpeg!,path,normalized,geometry.Filter(width,height,project.Canvas.Background),
                        range.Start,duration,renderedDuration,range.Timing,width,height,outputFps,!needsAudio,
                        fraction=>progress.Report(50*(completed+fraction*renderedDuration.TotalSeconds)/requestedDuration.TotalSeconds),cancellation,frameCount,sourceMetadata.GetValueOrDefault(range.SourceIndex)?.Transfer);
                    if(project.Cameras.GetValueOrDefault(range.SourceIndex) is {Visible:true} camera&&camera.ForClip(range).Any()){
                        if(!File.Exists(camera.Path))throw new FileNotFoundException("Thiếu nguồn camera. Tắt lớp camera hoặc liên kết lại tệp.",camera.Path);
                        string picture=Path.Combine(work,$"camera-{i}.mp4");
                        await RenderCameraAsync(ffmpeg!,normalized,picture,camera,range with{End=end},width,height,renderedDuration,cancellation);
                        normalized=picture;
                    }
                    if(concatenate){offset+=range.Duration;normalizedClips.Add(normalized);continue;}
                    var transformed=await StorageFile.GetFileFromPathAsync(normalized).AsTask(cancellation);
                    clip=await MediaClip.CreateFromFileAsync(transformed).AsTask(cancellation);
                    if(clip.OriginalDuration>renderedDuration)clip.TrimTimeFromEnd=clip.OriginalDuration-renderedDuration;
                }else{
                    clip!.TrimTimeFromStart=range.Start;
                    clip.TrimTimeFromEnd=clip.OriginalDuration-end;
                }
                composition.Clips.Add(clip!);
                offset+=clip!.TrimmedDuration;
            }

            for(int i=0;i<tracks.Length;i++){
                cancellation.ThrowIfCancellationRequested();
                if(tracks[i].Slice(TimeSpan.Zero,offset) is not {} track)continue;
                var mark=track.Mark;
                if(mark.SamplesVideo){
                    if(mark.Style.Opacity<=0)continue;
                    var box=VideoMarkRendering.Region(mark,width,height);
                    int left=(int)Math.Floor(box.Left)&~1,top=(int)Math.Floor(box.Top)&~1;
                    int right=Math.Min(width,((int)Math.Ceiling(box.Right)+1)&~1);
                    int bottom=Math.Min(height,((int)Math.Ceiling(box.Bottom)+1)&~1);
                    if(right>left&&bottom>top)blurRegions.Add(new(left,top,right-left,bottom-top,track.Start,track.End,mark));
                    continue;
                }
                string png=Path.Combine(work,$"ink-{i}.png");
                if(!VideoMarkRendering.WritePng([mark],width,height,png))continue;
                if(needsBlur||concatenate){inkRegions.Add(new(png,track.Start,track.End));continue;}
                var file=await StorageFile.GetFileFromPathAsync(png).AsTask(cancellation);
                var ink=await MediaClip.CreateFromImageFileAsync(file,track.Length).AsTask(cancellation);
                var overlay=new MediaOverlay(ink,new Windows.Foundation.Rect(0,0,width,height),1){Delay=track.Start};
                var layer=new MediaOverlayLayer();
                layer.Overlays.Add(overlay);
                composition.OverlayLayers.Add(layer);
            }

            var temporary=Path.Combine(work,"composition.mp4");
            if(concatenate){
                string manifest=Path.Combine(work,"concat.txt");
                // Generated relative ASCII names avoid ffconcat quoting of user paths.
                await File.WriteAllLinesAsync(manifest,normalizedClips.Select(path=>"file '"+Path.GetFileName(path)+"'"),cancellation);
                await RunFfmpegAsync(ffmpeg!,VideoExportCommands.Concat(manifest,temporary,requestedDuration),requestedDuration,
                    value=>progress.Report(50+value*15),cancellation,"Không ghép được các đoạn thành MP4 liền mạch.",work);
            }else{
            using(File.Create(temporary)){}
            var output=await StorageFile.GetFileFromPathAsync(temporary).AsTask(cancellation);
            double begin=needsCanvas?50:0;
            double finish=blurRegions.Count>0||inkRegions.Count>0?82:99;
            var renderProgress=new Progress<double>(value=>progress.Report((begin+value/100*(finish-begin))*(needsAudio?.8:1)));
            var result=await composition.RenderToFileAsync(output,MediaTrimmingPreference.Precise,profile).AsTask(cancellation,renderProgress);
            if(result!=TranscodeFailureReason.None)
                throw new IOException($"Windows không xuất được MP4 ({result}). Hãy kiểm tra codec của video nguồn.");
            }
            if(blurRegions.Count>0||inkRegions.Count>0){
                var blurred=Path.Combine(work,"blurred.mp4");
                await ApplyTracksAsync(ffmpeg!,temporary,blurred,blurRegions,inkRegions,offset,needsAudio?new Progress<double>(value=>progress.Report(value*.8)):progress,cancellation);
                temporary=blurred;
            }
            if(needsCues){
                string script=Path.Combine(work,"cues.ass");
                await File.WriteAllTextAsync(script,VideoCueSubtitles.Build(cues,sourceSizes,project.Canvas,width,height,outputFps,offset),cancellation);
                var withCues=Path.Combine(work,"cues.mp4");
                await RunFfmpegAsync(ffmpeg!,["-i",temporary,"-vf","ass=cues.ass","-map","0:v:0","-map","0:a?",
                    "-c:v","libx264","-preset","fast","-crf","16","-c:a","copy",withCues],offset,
                    fraction=>progress.Report(82+fraction*(needsAudio?6:17)),cancellation,
                    "Không xuất được chỉ dẫn chuột. Cần FFmpeg có libass và libx264.",work);
                temporary=withCues;
            }
            if(needsAudio){
                var mix=await Task.Run(()=>VideoAudioRender.RenderAsync(project,work,false,cancellation),cancellation);
                var withAudio=Path.Combine(work,"audio.mp4");
                await RunFfmpegAsync(ffmpeg!,["-i",temporary,"-ss",Seconds(timelineStart),"-i",mix.Path,
                    "-t",Seconds(requestedDuration),"-map","0:v:0","-map","1:a:0","-c:v","copy","-c:a","aac",
                    "-b:a","256k","-movflags","+faststart",withAudio],requestedDuration,fraction=>progress.Report(82+17*fraction),cancellation,
                    "Không ghép được bản phối âm thanh vào MP4.");
                temporary=withAudio;
            }
            RecordingMediaInfo verified;
            if(concatenate&&VideoProbe.Executable!=null){
                RecordingContainer.Verify(temporary);var metadata=await VideoProbe.ReadAsync(temporary,cancellation);
                verified=new(metadata.Width,metadata.Height,metadata.Duration);
            }else verified=await RecordingMedia.VerifyAsync(temporary,cancellation);
            if(verified.Width!=width||verified.Height!=height)
                throw new InvalidDataException($"Kích thước MP4 không khớp canvas {width} × {height}.");
            if((verified.Duration-requestedDuration).Duration()>TimeSpan.FromSeconds(Math.Max(.15,2/outputFps)))
                throw new InvalidDataException("Thời lượng MP4 xuất ra không khớp timeline.");
            cancellation.ThrowIfCancellationRequested();
            File.Move(temporary,destination,true);
            progress.Report(100);
        }finally{
            try{Directory.Delete(work,true);}catch(Exception error) when(error is IOException or UnauthorizedAccessException){App.Log(error);}
        }
    }

    private static Task RenderCanvasClipAsync(string executable,string input,string output,string filter,TimeSpan start,
        TimeSpan sourceDuration,TimeSpan duration,VideoMotion motion,int width,int height,double fps,bool keepAudio,
        Action<double> progress,CancellationToken cancellation,long? frameCount=null,string? transfer=null)
    {
        var arguments=VideoExportCommands.Normalize(input,output,filter,start,sourceDuration,duration,motion,width,height,fps,keepAudio,frameCount,transfer);
        return RunFfmpegAsync(executable,arguments,duration,progress,cancellation,"FFmpeg không dựng được canvas hoặc chuyển động. Kiểm tra libx264.");
    }

    private static async Task RenderCameraAsync(string ffmpeg,string input,string output,VideoCamera camera,
        VideoRange range,int width,int height,TimeSpan duration,CancellationToken token)
    {
        var spans=camera.ForClip(range).ToArray();
        if(spans.Length==0){File.Copy(input,output);return;}
        string filter=VideoCameraFilter.Build(camera,range,width,height,spans);
        string script=Path.Combine(Path.GetDirectoryName(output)!,"camera-filter.txt");
        await File.WriteAllTextAsync(script,filter,token);
        await RunFfmpegAsync(ffmpeg,["-i",input,"-i",camera.Path,"-filter_complex_threads","1","-filter_complex_script",script,
            "-map","[cameraout]","-map","0:a?","-c:a","copy","-c:v","libx264","-preset","fast","-crf","16","-video_track_timescale","90000",output],
            duration,_=>{},token,"Không ghép được lớp camera. Kiểm tra nguồn MP4 và FFmpeg có libx264.");
    }

    private static string Seconds(TimeSpan value)=>value.TotalSeconds.ToString("0.#######",CultureInfo.InvariantCulture);

    private sealed record InkRegion(string Path,TimeSpan Start,TimeSpan End);
    private sealed record BlurRegion(int X,int Y,int Width,int Height,TimeSpan Start,TimeSpan End,VideoMark Mark);

    internal static string? FindFfmpeg()
    {
        var adjacent=Path.Combine(AppContext.BaseDirectory,"ffmpeg.exe");
        if(File.Exists(adjacent))return adjacent;
        foreach(string entry in (Environment.GetEnvironmentVariable("PATH")??"").Split(Path.PathSeparator,StringSplitOptions.RemoveEmptyEntries)){
            string directory=entry.Trim().Trim('"');
            if(!Path.IsPathFullyQualified(directory))continue;
            var candidate=Path.Combine(directory,"ffmpeg.exe");
            if(File.Exists(candidate))return candidate;
        }
        return null;
    }

    private static async Task ApplyTracksAsync(string executable,string input,string output,IReadOnlyList<BlurRegion> regions,
        IReadOnlyList<InkRegion> inks,TimeSpan duration,IProgress<double> progress,CancellationToken cancellation)
    {
        if(regions.Count+inks.Count>VideoProject.MaximumTracks)
            throw new IOException("Mỗi lần xuất có làm mờ hỗ trợ tối đa 256 track chú thích.");
        var graph=new StringBuilder();
        var arguments=new List<string>{"-i",input};
        string previous="0:v";
        for(int i=0;i<regions.Count;i++){
            var region=regions[i];
            var style=region.Mark.Style;
            string enable=$"gte(t,{Seconds(region.Start)})*lt(t,{Seconds(region.End)})";
            string opacity=(style.Opacity/100).ToString("0.####",CultureInfo.InvariantCulture);
            string next=$"effect{i}";
            if(region.Mark.Kind==VideoMarkKind.Blur&&style.Effect==2){
                string color=(region.Mark.Color??"#F54856")[1..];
                graph.Append($"[{previous}]drawbox=x={region.X}:y={region.Y}:w={region.Width}:h={region.Height}:color=0x{color}@{opacity}:t=fill:enable='{enable}'[{next}];");
                previous=next;
                continue;
            }
            string filter;
            if(region.Mark.Kind==VideoMarkKind.Magnifier){
                int w=Math.Max(2,((int)Math.Round(region.Width/style.Zoom))&~1);
                int h=Math.Max(2,((int)Math.Round(region.Height/style.Zoom))&~1);
                int x=(region.X+(region.Width-w)/2)&~1,y=(region.Y+(region.Height-h)/2)&~1;
                filter=$"crop={w}:{h}:{x}:{y},scale={region.Width}:{region.Height}:flags=bicubic";
            }else{
                filter=$"crop={region.Width}:{region.Height}:{region.X}:{region.Y}";
                if(style.Effect==1){
                    int divisor=Math.Max(2,style.Strength/2);
                    int w=Math.Max(2,(region.Width/divisor)&~1),h=Math.Max(2,(region.Height/divisor)&~1);
                    filter+=$",scale={w}:{h}:flags=area,scale={region.Width}:{region.Height}:flags=neighbor";
                }else filter+=$",gblur=sigma={Math.Clamp(Math.Min(region.Width,region.Height)/2,1,style.Strength/2)}";
            }
            if(style.Opacity<100)filter+=$",format=rgba,colorchannelmixer=aa={opacity}";
            graph.Append($"[{previous}]split[b{i}][c{i}];[c{i}]{filter}[r{i}];");
            graph.Append($"[b{i}][r{i}]overlay={region.X}:{region.Y}:enable='{enable}'[{next}];");
            previous=next;
        }
        for(int i=0;i<inks.Count;i++){
            var ink=inks[i];
            arguments.AddRange(["-i",ink.Path]);
            string next=$"ink{i}";
            graph.Append($"[{previous}][{i+1}:v]overlay=0:0:enable='gte(t,{Seconds(ink.Start)})*lt(t,{Seconds(ink.End)})'[{next}];");
            previous=next;
        }
        graph.Length--;
        string graphPath=Path.Combine(Path.GetDirectoryName(output)!,"tracks.txt");
        await File.WriteAllTextAsync(graphPath,graph.ToString(),cancellation);
        arguments.AddRange(["-filter_complex_threads","1","-filter_complex_script",graphPath,
            "-map",$"[{previous}]","-map","0:a?","-c:a","copy","-c:v","libx264","-preset","fast",
            "-crf","16","-movflags","+faststart",output]);
        await RunFfmpegAsync(executable,arguments,duration,fraction=>progress.Report(82+fraction*17),
            cancellation,"FFmpeg không xuất được track chú thích. Kiểm tra bộ mã hóa libx264.");
    }

    internal static async Task RunFfmpegAsync(string executable,IReadOnlyList<string> arguments,TimeSpan duration,
        Action<double> progress,CancellationToken cancellation,string errorMessage,string? workingDirectory=null)
    {
        cancellation.ThrowIfCancellationRequested();
        var settings=new ProcessStartInfo(executable){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        if(workingDirectory!=null)settings.WorkingDirectory=workingDirectory;
        foreach(var argument in new[]{"-nostdin","-hide_banner","-loglevel","error","-progress","pipe:1"}.Concat(arguments))
            settings.ArgumentList.Add(argument);
        using var process=Process.Start(settings)??throw new IOException("Không khởi động được FFmpeg.");
        using var registration=cancellation.Register(()=>{
            try{if(!process.HasExited)process.Kill(entireProcessTree:true);}
            catch(Exception error) when(error is InvalidOperationException or System.ComponentModel.Win32Exception){App.Log(error);}
        });
        var errors=process.StandardError.ReadToEndAsync();
        while(await process.StandardOutput.ReadLineAsync() is {} line){
            if(line.StartsWith("out_time_us=",StringComparison.Ordinal)
                &&long.TryParse(line.AsSpan(12),NumberStyles.Integer,CultureInfo.InvariantCulture,out long time))
                progress(Math.Clamp(time/(duration.TotalSeconds*1_000_000),0,1));
        }
        await process.WaitForExitAsync();
        cancellation.ThrowIfCancellationRequested();
        if(process.ExitCode!=0){
            string details=(await errors).Trim();App.Log(new IOException(details));
            throw new IOException(errorMessage+" "+(details.Length>1500?details[^1500..]:details));
        }
        await errors;
    }
}
