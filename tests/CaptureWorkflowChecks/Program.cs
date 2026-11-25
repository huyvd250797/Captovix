using Captovix;
using System.IO.Compression;
using System.Text.Json;
var count=0;void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);count++;Console.WriteLine("PASS: "+name);}
void Reject(Action action,string name){bool rejected=false;try{action();}catch{rejected=true;}Check(rejected,name);}
var root=Path.Combine(Path.GetTempPath(),"Captovix-Workflow-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
try{
 var config=CaptureWorkflowStore.Load(root,out var warning);Check(warning==null&&!config.Background&&!config.AutoCopy&&!config.CloseToTray&&!config.StartWithWindows&&!config.StartInTray,"First install opens Editor, no autorun/close-to-tray");
 CaptureWorkflowStore.Validate(config);Check(config.Hotkeys.Count==6&&config.Presets.Count==3,"Six legacy action IDs remain compatible; three presets");
 config.CloseToTray=true;config.Background=true;config.AutoCopy=true;config.StartWithWindows=true;config.StartInTray=true;config.Hotkeys[1]=new(PhotoAction.Region,true,3,0x41);
 var displays=new[]{new WorkflowDisplay("A",new(-1920,0,1920,1080),96,96,"monitorA"),new WorkflowDisplay("B",new(0,0,2560,1440),144,144,"monitorB")};
 config.LastRegion=new(new(-300,100,900,500),CaptureWorkflowStore.Topology(displays));CaptureWorkflowStore.Save(root,config);var loaded=CaptureWorkflowStore.Load(root,out warning);
 Check(warning==null&&loaded.CloseToTray&&loaded.Background&&loaded.AutoCopy&&loaded.StartWithWindows&&loaded.StartInTray,"Preferences survive restart");
 Check(loaded.Hotkeys[1]==config.Hotkeys[1]&&loaded.Presets.SequenceEqual(config.Presets),"Hotkeys and presets roundtrip");Check(loaded.LastRegion==config.LastRegion,"Physical region and topology roundtrip");
 Check(!JsonSerializer.Serialize(loaded.Presets).Contains("Handle"),"Presets never serialize an HWND");
 var clone=config.Clone();clone.Presets.Clear();Check(config.Presets.Count==3,"Candidate edits isolated until commit");
 Reject(()=>{var x=config.Clone();x.SchemaVersion=2;CaptureWorkflowStore.Save(root,x);},"Reject future schema");
 Reject(()=>{var x=config.Clone();x.Hotkeys.RemoveAt(0);CaptureWorkflowStore.Validate(x);},"Reject missing actions");
 Reject(()=>{var x=config.Clone();x.Hotkeys[1]=x.Hotkeys[0];CaptureWorkflowStore.Validate(x);},"Reject duplicate action identifiers");
 Reject(()=>{var x=config.Clone();x.Hotkeys[0]=new((PhotoAction)99,true,6,0x41);CaptureWorkflowStore.Validate(x);},"Reject unknown action");
 Reject(()=>{var x=config.Clone();x.Hotkeys[3]=x.Hotkeys[3] with{Modifiers=3,Key=0x41};CaptureWorkflowStore.Validate(x);},"Reject duplicate chords");
 foreach(var key in new uint[]{0x32,0x33,0x34})Reject(()=>{var x=config.Clone();x.Hotkeys[0]=new(PhotoAction.Current,true,6,key);CaptureWorkflowStore.Validate(x);},"Reserve recording chord "+key);
 foreach(var chord in new[]{(0u,0x41u),(8u,0x41u),(2u,0x7Bu),(2u,0x1Bu),(0u,0x09u),(0u,0x20u),(0u,0x25u),(0u,0x60u),(15u,0x2Cu)}){var x=config.Clone();x.Hotkeys[0]=new(PhotoAction.Current,true,chord.Item1,chord.Item2);CaptureWorkflowStore.Validate(x);Check(true,"Accept keyboard chord; OS registration remains authoritative "+chord);}
 foreach(var key in new uint[]{0,1,0x10,0x11,0x12,0x5B,0x5C,0xA0,0xFF})Reject(()=>{var x=config.Clone();x.Hotkeys[0]=new(PhotoAction.Current,true,0,key);CaptureWorkflowStore.Validate(x);},"Reject modifier-only/non-key "+key);
 foreach(var key in new uint[]{0x70,0x87,0x2C}){var x=config.Clone();x.Hotkeys[0]=new(PhotoAction.Current,true,0,key);CaptureWorkflowStore.Validate(x);Check(true,"Allow function/PrintScreen "+key);}
 {var x=config.Clone();x.Hotkeys[0]=new(PhotoAction.Current,false,0,0);CaptureWorkflowStore.Validate(x);Check(true,"Disabled key can be empty");}
 foreach(var preset in new[]{new CapturePreset("x"," ",0,"",0,true,false,false),new("x",new string('a',41),0,"",0,true,false,false),new("x","New",4,"",0,true,false,false),new("x","New",0,"",2,true,false,false),new("x","HƯỚNG DẪN",0,"",0,true,false,false)})Reject(()=>{var x=config.Clone();x.Presets.Add(preset);CaptureWorkflowStore.Validate(x);},"Reject invalid/duplicate preset "+preset.Name);
 Reject(()=>{var x=config.Clone();x.Presets=Enumerable.Range(0,21).Select(i=>new CapturePreset(i.ToString(),i.ToString(),0,"",0,true,false,false)).ToList();CaptureWorkflowStore.Validate(x);},"Enforce preset limit");
 Check(CaptureWorkflowStore.CanRepeat(config.LastRegion,displays),"Repeat accepts unchanged multi-monitor physical coordinates");Check(CaptureWorkflowStore.Topology(displays)==CaptureWorkflowStore.Topology(displays.Reverse()),"Monitor enumeration order does not invalidate region");
 Check(!CaptureWorkflowStore.CanRepeat(null,displays),"No repeat before first successful region capture");
 foreach(var changed in new[]{displays.Take(1).ToArray(),new[]{displays[0],displays[1] with{DpiX=192}},new[]{displays[0],displays[1] with{Bounds=new(10,0,2560,1440)}},new[]{displays[0],displays[1] with{Identity="replacement"}}})Check(!CaptureWorkflowStore.CanRepeat(config.LastRegion,changed),"Invalidate repeat on monitor/DPI/topology change");
 Check(!CaptureWorkflowStore.CanRepeat(config.LastRegion with{Bounds=new(-9999,0,100,100)},displays),"Reject region outside desktop");Check(!CaptureWorkflowStore.CanRepeat(config.LastRegion with{Bounds=new(0,0,1,1)},displays),"Reject tiny saved region");
 var file=Path.Combine(root,CaptureWorkflowStore.FileName);File.WriteAllText(file,"{broken");CaptureWorkflowStore.Load(root,out warning);Check(warning!=null&&File.ReadAllText(file)=="{broken","Corrupt configuration retained for recovery");File.WriteAllText(file,"{\"SchemaVersion\":99}");CaptureWorkflowStore.Load(root,out warning);Check(warning!=null&&File.ReadAllText(file).Contains("99"),"Future configuration retained");
 Check(CaptureWorkflowStore.StartupCommand(@"D:\Captovix Test\current\Captovix.exe")=="\"D:\\Captovix Test\\current\\Captovix.exe\" --captovix-autostart","Autorun quotes path with spaces and no capture argument");
 Reject(()=>CaptureWorkflowStore.StartupCommand("bad\"path"),"Reject injected startup argument");Reject(()=>CaptureWorkflowStore.StartupCommand(new string('a',250)),"Reject overlong Run command");
 var active=new Dictionary<int,(uint,uint)>();var blocked=new HashSet<uint>();bool Register(int id,uint m,uint k){if(blocked.Contains(k)||active.ContainsKey(id))return false;active[id]=(m,k);return true;}
 var broker=new CaptureHotkeyRegistry(Register,id=>active.Remove(id));Check(broker.Start(config.Hotkeys).Count==0&&active.Count==3,"Register three active photo actions");
 var next=config.Hotkeys.Select(h=>h.Action==PhotoAction.Region?h with{Key=0x42}:h).ToList();blocked.Add(0x42);Reject(()=>broker.Replace(next),"External conflict rejects transaction");Check(active.Count==3&&active[5].Item2==0x41,"Conflict restores previous live bindings");blocked.Clear();broker.Replace(next);Check(active.Count==3&&active[5].Item2==0x42,"Successful rebind replaces old chord");
 broker.Stop();Check(active.Count==0,"Exit/update releases every photo hotkey");blocked.Add(0x42);Check(broker.Start(next).Count==1&&active.Count==2,"Startup conflict retains other available actions");broker.Stop();
 var gate=new CaptureGate();var winners=0;Parallel.For(0,1000,_=>{if(gate.TryEnter())Interlocked.Increment(ref winners);});Check(winners==1,"Repeated capture requests acquire one owner");gate.Exit();Check(gate.TryEnter(),"Gate reusable after cancelled/finished capture");gate.Exit();
 CaptureWorkflowStore.Save(root,config);var backup=UpdateBackup.Create(root,"1.4.0","1.5.0");using(var zip=ZipFile.OpenRead(backup))Check(zip.GetEntry(CaptureWorkflowStore.FileName)!=null,"Update backup includes capture settings, keys and presets");
 Check(Directory.GetFiles(root,"*.tmp").Length==0,"Atomic saves leave no temporary files");
 var odd=new PixelRect(-181,-31,403,205);
 var frame=CaptureGeometry.RecordingFrame(odd);
 Check(frame==new PixelRect(-181,-31,404,206)&&frame.Intersect(odd)==odd,"Odd recording canvas keeps all source edges and aligns output dimensions");
 var parts=CaptureGeometry.Split(odd,[("left",new PixelRect(-1920,-100,1920,1080)),("right",new PixelRect(0,-100,2560,1440))]);
 Check(parts.Count==2&&parts[0].Crop==new PixelRect(1739,69,181,205)&&parts[1].X==181&&parts.Sum(p=>p.Crop.Width)==odd.Width,"Cross-monitor recording uses exact physical pixels on both sides of the seam");

 var video=Path.Combine(root,"fixture.pending.mp4");
 byte[] Box(string type,byte value)=>[0,0,0,9,..System.Text.Encoding.ASCII.GetBytes(type),value];
 byte[] indexed=[..Box("ftyp",1),..Box("mdat",2),..Box("moov",3)];
 File.WriteAllBytes(video,indexed);RecordingContainer.Verify(video);Check(true,"Recognize complete top-level MP4 boxes");
 File.WriteAllBytes(video,indexed[..^1]);Reject(()=>RecordingContainer.Verify(video),"Reject truncated MP4");
 File.WriteAllBytes(video,[..Box("ftyp",1),..Box("mdat",2)]);Reject(()=>RecordingContainer.Verify(video),"Reject MP4 missing index");
 File.WriteAllBytes(video,[..Box("ftyp",1),0,0,0,0,(byte)'m',(byte)'d',(byte)'a',(byte)'t',2]);Reject(()=>RecordingContainer.Verify(video),"Reject unfinished length-to-EOF recording without an index");
 File.WriteAllBytes(video,[..Box("ftyp",1),..Box("moov",3),0,0,0,1,(byte)'m',(byte)'d',(byte)'a',(byte)'t',0,0,0,0,0,0,0,17,2]);
 RecordingContainer.Verify(video);Check(true,"Support extended MP4 atom lengths");

 var full=new PixelRect(-200,0,3840,2160);
 var light=RecordingProfile.ForPreset(0);
 var normal=RecordingProfile.ForPreset(1);
 var high=RecordingProfile.ForPreset(2);
 Check(light.Fps==15&&light.Frame(full)==new PixelRect(-200,0,1280,720),"Light preset fits 4K into 720p at 15 FPS");
 Check(normal.Fps==30&&normal.Frame(full)==new PixelRect(-200,0,1920,1080),"Normal preset fits 4K into 1080p at 30 FPS");
 Check(high.Fps==60&&high.Frame(full)==full&&high.Frame(odd)==frame,"High preset keeps source and odd edges");
 Check(normal.Frame(new(0,0,1080,1920))==new PixelRect(0,0,1080,1920),"Fit portrait without landscape distortion");
 Check(normal.BytesPerMinute(8_000_000,true,true)>normal.BytesPerMinute(8_000_000,false,false),"Estimate reserves space for independent WAV sources");
 byte[] Big(uint value)=>[(byte)(value>>24),(byte)(value>>16),(byte)(value>>8),(byte)value];
 byte[] Atom(string type,byte[] payload)=>[..Big((uint)(8+payload.Length)),..System.Text.Encoding.ASCII.GetBytes(type),..payload];
 var handler=Atom("hdlr",[..Big(0),..Big(0),..System.Text.Encoding.ASCII.GetBytes("vide")]);
 var header=Atom("mdhd",[..Big(0),..Big(0),..Big(0),..Big(24000)]);
 var times=Atom("stts",[..Big(0),..Big(1),..Big(120),..Big(1000)]);
 var movie=Atom("moov",Atom("trak",Atom("mdia",[..header,..handler,..Atom("minf",Atom("stbl",times))])));
 File.WriteAllBytes(video,movie);
 Check(RecordingFrameRate.Read(video)==24,"Read 24 FPS from MP4 video sample table");
 File.WriteAllBytes(video,movie[..^1]);
 Reject(()=>RecordingFrameRate.Read(video),"Reject truncated MP4 timing data");

 File.WriteAllText(Path.Combine(root,AppearanceSettings.FileName),"""{"Schema":1,"Accent":"#7C3AED","IconBackground":"#123456"}""");
 var priorLook=AppearanceSettings.Load(root,out var appearanceWarning);
 Check(appearanceWarning==null&&!priorLook.IconBackgroundGradient&&priorLook.IconBackground=="#123456","Existing solid logo background is preserved");
 var gradientLook=priorLook with{IconBackgroundGradient=true,IconBackgroundEnd="#AB1234",GradientAngle=135};
 gradientLook.Save(root);
 Check(AppearanceSettings.Load(root,out appearanceWarning)==gradientLook&&appearanceWarning==null,"Logo background gradient and shared angle survive reload");
 Reject(()=>(gradientLook with{IconBackgroundEnd="bad"}).Validate(),"Reject invalid logo background gradient color");
 var audioClock=new RecordingAudioClock();
 const long second=TimeSpan.TicksPerSecond;
 Check(!audioClock.Place(0,100,1000).Any(),"Audio preview before first video timestamp never reaches WAV");
 audioClock.Resume(second);
 Check(audioClock.ActiveElapsed(second+second/20)==second/20,"Markers and clicks use active recording time");
 Check(audioClock.Place(second-second/20,100,1000).Single()==new AudioSlice(50,50,0),"Trim a packet that starts before video");
 audioClock.Pause(second+second/10);
 Check(audioClock.Place(second+second/20,100,1000).Single()==new AudioSlice(0,50,50),"Trim packet at pause boundary");
 Check(!audioClock.Place(2*second,100,1000).Any(),"Discard packets wholly within pause");
 Check(audioClock.ActiveElapsed(2*second)==null,"Presenter excludes clicks during pause");
 audioClock.Resume(3*second);
 Check(audioClock.ActiveElapsed(3*second+second/5)==3*second/10,"Presenter resumes without wall-clock pause");
 Check(audioClock.Place(3*second,100,1000).Single()==new AudioSlice(0,100,100),"Resume continues on the same timeline without pause gap");
 Check(audioClock.Place(3*second+30*60*second,480,48000).Single().OutputFrame==86_404_800,"30-minute timestamp mapping does not accumulate per-packet rounding");
 Check(audioClock.Place(3*second+second/2,480,48000).Single().OutputFrame==28_800,"Preserve a silent gap in a loopback endpoint timeline");
 audioClock.Pause(4*second,true);
 audioClock.Resume(5*second);
 Check(!audioClock.Place(5*second,100,1000).Any(),"Completed video clock cannot resume");
 string wav=Path.Combine(root,"timed.wav");
 using(var audioFile=new RecordingWaveFile(wav,48000,1)){
   audioFile.Write(3,[1f,-1f,0.5f,1f]);
   audioFile.Write(6,[-1f,0.5f]);
   audioFile.Complete(7);
 }
 using(var wave=new BinaryReader(File.OpenRead(wav))){
   Check(wave.BaseStream.Length==44+7*2,"PCM file is trimmed to verified video duration");
   wave.BaseStream.Position=40;
   Check(wave.ReadUInt32()==14&&wave.ReadInt16()==0&&wave.ReadInt16()==0&&wave.ReadInt16()==0,"WAV header and silent gaps preserve timeline");
   Check(wave.ReadInt16()==short.MaxValue&&wave.ReadInt16()==short.MinValue&&wave.ReadInt16()==16384&&wave.ReadInt16()==short.MaxValue,"PCM conversion keeps signal and discards duplicate overlapping samples");
 }
 var selectedRange=new VideoRange(TimeSpan.FromSeconds(2),TimeSpan.FromSeconds(8));
 var cut=TimeSpan.FromSeconds(4.25);
 var minimum=TimeSpan.FromMilliseconds(40);
 Check(selectedRange.TrySplit(cut,minimum,out var leftRange,out var rightRange)&&leftRange.End==cut&&rightRange.Start==cut&&leftRange.Duration+rightRange.Duration==selectedRange.Duration,"Split preserves both sides of the selected interval");
 Check(!selectedRange.TrySplit(selectedRange.Start,minimum,out _,out _)&&!selectedRange.TrySplit(selectedRange.End+TimeSpan.FromMilliseconds(1),minimum,out _,out _),"Split rejects boundaries and out-of-range points");
 Check(!selectedRange.TrySplit(selectedRange.Start+TimeSpan.FromMilliseconds(10),minimum,out _,out _),"Split rejects sub-frame intervals");
 TimeSpan Seconds(double value)=>TimeSpan.FromSeconds(value);
 var originalPath=Path.Combine(root,"source.mp4");
 File.WriteAllBytes(originalPath,new byte[]{1,2,3,4});
 var identity=VideoSourceIdentity.Read(originalPath);
 var legacyProject=new VideoProject(1,originalPath,identity,Seconds(30),
     [new VideoRange(TimeSpan.Zero,Seconds(30))],TimeSpan.Zero,0);
 var legacy=Path.Combine(root,"legacy.cvxvideo");
 legacyProject.Save(legacy);
 var timeline=new VideoTimeline(VideoProject.Open(legacy));
 Check(VideoProject.Open(legacy).Version==1,"Projects from V1.57.0 can still be opened");
 timeline.Seek(Seconds(12));
 Check(timeline.Split(minimum)&&timeline.Project.Clips[0].End==Seconds(12)&&timeline.Project.Clips[1].Start==Seconds(12),
     "Split cuts the clip at the timeline playhead");
 timeline.Seek(Seconds(18));
 Check(timeline.Split(minimum)&&timeline.Project.Clips.Select(clip=>clip.Number).SequenceEqual(new[]{1,2,3}),
     "Splitting from left to right produces Đoạn 1, Đoạn 2 and Đoạn 3");
 var movedMiddle=new VideoTimeline(timeline.Project);
 Check(movedMiddle.Move(1,3)&&movedMiddle.Project.Clips.Select(clip=>clip.Number).SequenceEqual(new[]{1,3,2})
     &&movedMiddle.Undo()&&movedMiddle.Redo()&&movedMiddle.Project.Clips[^1].Number==2,
     "Dragging Đoạn 2 to the end and redoing preserves its name");
 timeline.Seek(Seconds(13));
 Check(timeline.Delete()&&timeline.Project.Duration==Seconds(24)&&timeline.Map(Seconds(12)).SourcePosition==Seconds(18),
     "Removing the middle clip closes the gap and maps the join to the next source range");
 Check(timeline.Undo()&&timeline.Project.Duration==Seconds(30)&&timeline.Project.Position==Seconds(13),
     "Undo restores both clips and playhead");
 timeline.Seek(Seconds(14));
 Check(timeline.Trim(true,minimum)&&timeline.Selected.Start==Seconds(14)&&timeline.Project.Position==Seconds(12),
     "Trim moves the source in point while retaining the selected clip");
 var ordered=new VideoTimeline(timeline.Project);
 Check(ordered.Move(2,0)&&ordered.Project.Clips[0].Start==Seconds(18)&&ordered.Map(Seconds(1)).SourcePosition==Seconds(19),
     "Drag insertion moves a later source interval before the first clip and remaps the ruler");
 Check(ordered.Move(0,ordered.Project.Clips.Length)&&ordered.Project.Clips[^1].Start==Seconds(18),
     "Drag insertion can move a clip to the end");
 Check(ordered.Undo()&&ordered.Project.Clips[0].Start==Seconds(18)&&ordered.Redo()&&ordered.Project.Clips[^1].Start==Seconds(18),
     "Undo and Redo restore clip order");
 var secondPath=Path.Combine(root,"second.mp4");
 File.WriteAllBytes(secondPath,new byte[]{6,7,8,9});
 var input=new VideoInput(secondPath,VideoSourceIdentity.Read(secondPath),Seconds(5.123));
 Check(ordered.Append(input)&&ordered.Selected.SourceIndex==1&&ordered.Project.Input(1)==input,
     "A second MP4 appends using its own duration and source identity");
 var mark=new VideoMark(VideoMarkKind.Blur,.25,.25,.75,.75,"",0);
 var annotation=new VideoMarkTrack(Guid.NewGuid(),Seconds(2),Seconds(4),mark);
 var other=new VideoMarkTrack(Guid.NewGuid(),Seconds(3),Seconds(6),new(VideoMarkKind.Text,.1,.2,.1,.2,"Chữ",0));
 Check(ordered.SetTrack(annotation)&&ordered.SetTrack(other)&&annotation.IsActive(Seconds(2))
     &&!annotation.IsActive(Seconds(4))&&other.IsActive(Seconds(4)),"Independent tracks use a start-inclusive, end-exclusive timeline");
 var shifted=annotation with{Start=Seconds(5),End=Seconds(8)};
 Check(ordered.SetTrack(shifted)&&ordered.Undo()&&ordered.Project.Tracks[0]==annotation
     &&ordered.Redo()&&ordered.Project.Tracks[0]==shifted,"Move and resize a track in a single undoable operation");
 Check(shifted.Slice(Seconds(7),Seconds(9)) is {Start.Ticks:0} overlap&&overlap.End==Seconds(1),
     "Exporting a selected clip intersects and rebases the track time");
 var canvas=new VideoCanvasLayout(VideoAspect.Portrait,VideoFit.Fill,"#26364B");
 var layout=new VideoClipLayout(90,.1,.05,.2,.15);
 Check(ordered.SetCanvas(canvas,layout)&&ordered.Selected.Layout==layout&&ordered.Project.Canvas==canvas,
     "Canvas is project-wide while crop and rotation belong to the selected clip");
 ordered.Seek(ordered.Offset(ordered.Project.SelectedClip)+Seconds(2.1));
 Check(ordered.Split(minimum)&&ordered.Project.Clips.TakeLast(2).All(clip=>clip.SourceIndex==1&&clip.Layout==layout)
     &&ordered.Project.Tracks[0]==shifted,"Split preserves source and layout while annotation times stay fixed");
 Check(ordered.Delete()&&ordered.Undo()&&ordered.Redo(),"Delete removes a clip and supports Undo and Redo");
 Check(ordered.Undo()&&ordered.RemoveTrack(other.Id)&&!ordered.CanRedo,"Removing a track clears the redo branch");
 var edited=Path.Combine(root,"ordered.cvxvideo");
 ordered.Project.Save(edited);
 var restored=VideoProject.Open(edited);
 Check(restored.Version==8&&restored.Input(1)==input&&restored.Canvas==canvas
     &&restored.Clips.SequenceEqual(ordered.Project.Clips)&&restored.Tracks.SequenceEqual(new[]{shifted}),
     "V8 persists clip numbers, order, canvas, clip layout and annotation timing");
 var title=new VideoMarkTrack(Guid.NewGuid(),Seconds(1),Seconds(3),
     new(VideoMarkKind.Title,.12,.18,.88,.34,"Bước 1 — Mở cửa sổ Cài đặt",0){TextFrame=true,
        Style=VideoInkStyle.Default with{Font="Segoe UI",Bold=true,LineSpacing=1.25,Vertical=1}});
 var callout=new VideoMarkTrack(Guid.NewGuid(),Seconds(3),Seconds(5),
     new(VideoMarkKind.Callout,.35,.25,.65,.45,"Chọn Công cụ",0){Tail=new(.25,.50)});
 var v4=restored with{Version=4,Tracks=[shifted]};
 v4.Save(legacy);
 Check(new VideoTimeline(VideoProject.Open(legacy)).Project.Tracks.SequenceEqual(new[]{shifted}),
     "V4 opens and preserves annotation times while upgrading to V8");
 Check(ordered.SetTrack(title)&&ordered.SetTrack(callout),"A title and a callout with an independent tail can be added");
 ordered.Project.Save(edited);
 var styled=VideoProject.Open(edited);
 Check(styled.Tracks.Contains(title)&&styled.Tracks.Contains(callout)
     &&callout.Slice(Seconds(4),Seconds(6)) is {Start.Ticks:0} croppedCallout&&croppedCallout.End==Seconds(1)
     &&croppedCallout.Mark.Tail==new VideoInkPoint(.25,.50),
     "Vietnamese content, typography and the normalized callout tail survive save and export-time slicing");
 var moving=new VideoTimeline(legacyProject);
 moving.Seek(Seconds(10));
 var motion=new VideoMotion{Speed=2,Hold=Seconds(2),FadeOut=.5,Keys=[new(0,1,.5,.5),new(1,3,.7,.4)]};
 Check(moving.SetMotion(motion)&&moving.Project.Duration==Seconds(17)&&moving.Project.Position==Seconds(5)
     &&moving.Map(Seconds(6)).SourcePosition==Seconds(12)&&moving.Map(Seconds(16)).SourcePosition==Seconds(30),
     "Speed, freeze and seek share the source-to-timeline mapping");
 Check(motion.At(.5).Zoom==2&&Math.Abs(motion.At(.5).X-.6)<1e-9,"Zoom and pan use linear interpolation");
 var audioPlan=VideoAudioPlan.Create(moving.Project,_=>false).Single();
 Check(audioPlan.Speed==2&&audioPlan.Length==Seconds(15)&&audioPlan.SourceLength==Seconds(30),
     "Mix adjusts source speed and leaves the freeze interval silent");
 var motionFile=Path.Combine(root,"motion.cvxvideo");
 moving.Project.Save(motionFile);
 var persisted=VideoProject.Open(motionFile);
 Check(persisted.Clips[0].Timing.Keys.SequenceEqual(motion.Keys)&&persisted.Clips[0].Timing.Hold==Seconds(2),
     "Motion keyframes and hold survive project save and reopen");
 var lanes=VideoAnnotationLanes.Pack([title,callout,other]);
 Check(lanes.Count==2&&lanes[0].SequenceEqual(new[]{title,callout}),"Adjacent annotations share a lane and overlaps use a separate lane");
 Check(moving.Split(minimum)&&moving.Project.Clips[0].Timing.Hold==TimeSpan.Zero
     &&moving.Project.Clips[1].Timing.Hold==Seconds(2)&&moving.Project.Duration==Seconds(17),
     "Splitting a speed-adjusted clip keeps its final freeze and total duration");
 if(Environment.GetEnvironmentVariable("CAPTOVIX_MOTION_FILTER_TEST") is {} filterPath)
     File.WriteAllText(filterPath,motion.Filter(320,180,30,2,3));
 var click=new VideoCue(0,Seconds(10),"left",.5,.5);
 var ring=new VideoCue(0,Seconds(30),"ring");
 Check(moving.SetCues([click],true)&&!moving.SetCues([ring],true),"Cue validation accepts source positions and rejects an event at the exclusive end");
 var cueSpans=VideoCuePlan.Create(moving.Project.Clips,moving.Project.Cues);
 Check(cueSpans is [{Start:var cueStart}]&&cueStart==Seconds(5),"A click at Split occurs once, on the right clip at the speed-adjusted time");
 var cueSpan=cueSpans.Single();
 var cueLayout=new VideoCanvasLayout(VideoAspect.Portrait);
 var cueGeometry=VideoCanvasGeometry.Create(cueLayout,VideoClipLayout.Default,320,180,180,320);
 Check(VideoCuePlan.Point(cueSpan,cueSpan.Start,cueGeometry,320,180,180,320) is {} cuePoint
     &&cuePoint.X>0&&cuePoint.X<180&&cuePoint.Y>0&&cuePoint.Y<320,"A click follows zoom and portrait letterboxing");
 moving.Project.Save(motionFile);
 Check(VideoProject.Open(motionFile).Cues.SequenceEqual(new[]{click}),"Source-timed cues persist in schema 7");
 var cueScript=VideoCueSubtitles.Build(cueSpans,new Dictionary<int,(int Width,int Height)>{{0,(320,180)}},cueLayout,180,320,30,Seconds(17));
 Check(cueScript.Contains("Dialogue:")&&cueScript.Contains("0:00:05.00"),"Export uses the same cue time and frame grid");
 if(Environment.GetEnvironmentVariable("CAPTOVIX_CUE_FILTER_TEST") is {} cuePath)File.WriteAllText(cuePath,cueScript);
 var cameraClock=new RecordingAudioClock();
 cameraClock.Resume(Seconds(2).Ticks);
 cameraClock.Pause(Seconds(5).Ticks);
 cameraClock.Resume(Seconds(10).Ticks);
 cameraClock.Pause(Seconds(15).Ticks,true);
 var cameraSpans=cameraClock.CameraSpans(Seconds(1).Ticks,Seconds(12),Seconds(8))
     .Select(part=>new CameraSyncSpan(part.Screen,part.Camera,part.Length)).ToArray();
 Check(cameraSpans.SequenceEqual(new[]{new CameraSyncSpan(Seconds(0),Seconds(1),Seconds(3)),new CameraSyncSpan(Seconds(3),Seconds(9),Seconds(3))}),
     "Webcam and microphone use the same active clock, removing pause and ending at camera loss");
 var camera=new VideoCamera(originalPath,Seconds(12)){Sync=cameraSpans};
 Check(camera.At(Seconds(0))==Seconds(1)&&camera.At(Seconds(5))==Seconds(11)&&camera.At(Seconds(6))==null,
     "Camera follows active time and disappears when the source ends");
 Check(moving.SetCamera(0,camera)&&moving.Undo()&&moving.Redo(),"Camera layout is undoable");
 moving.Project.Save(motionFile);
 Check(VideoProject.Open(motionFile).Cameras[0].Sync.SequenceEqual(cameraSpans),"Project schema 8 preserves camera synchronization");
 var cameraRange=new VideoRange(Seconds(2),Seconds(6)){Motion=new VideoMotion{Speed=2}};
 var sections=camera.ForClip(cameraRange).ToArray();
 Check(sections.Length==2&&sections[0].CameraStart==Seconds(3)&&cameraRange.LocalAt(sections[1].ScreenStart)==Seconds(.5),
     "Trimmed speed-adjusted camera uses the source offsets across pauses");
 if(Environment.GetEnvironmentVariable("CAPTOVIX_CAMERA_FILTER_TEST") is {} cameraPath)
     File.WriteAllText(cameraPath,VideoCameraFilter.Build(camera,cameraRange,320,180,sections));
 var legacyInk=legacyProject with{Version=2,Clips=[
     new VideoRange(Seconds(12),Seconds(18)){Marks=[mark]},
     new VideoRange(TimeSpan.Zero,Seconds(3))],Position=TimeSpan.Zero};
 legacyInk.Save(legacy);
 var converted=new VideoTimeline(VideoProject.Open(legacy)).Project;
 Check(converted.Version==8&&converted.Clips[0].Number==1&&converted.Clips[0].Marks==null
     &&converted.Tracks is [{Start.Ticks:0} migrated]&&migrated.End==Seconds(6)&&migrated.Mark==mark,
     "V2 whole-clip marks migrate to independent tracks at the correct timeline position");
 var fit=new VideoCanvasLayout(VideoAspect.Portrait,VideoFit.Fit,"#26364B");
 var fitted=VideoCanvasGeometry.Create(fit,VideoClipLayout.Default,1920,1080,1080,1920);
 Check(fitted.Image==new VideoPixelRect(0,656,1080,608),"Portrait fit centers the landscape image with even scaled dimensions");
 var filled=VideoCanvasGeometry.Create(fit with{Fit=VideoFit.Fill},VideoClipLayout.Default,1920,1080,1080,1920);
 Check(filled.Crop==new VideoPixelRect(656,0,608,1080)&&filled.Image==new VideoPixelRect(0,0,1080,1920),
     "Portrait fill crops the source at the center before scaling");
 var rotated=VideoCanvasGeometry.Create(fit,new VideoClipLayout(90,.25,0,.25,0),1920,1080,1080,1920);
 Check(rotated.Crop==new VideoPixelRect(480,0,960,1080)&&rotated.Image==new VideoPixelRect(0,480,1080,960)
     &&rotated.Filter(1080,1920,fit.Background).Contains("transpose=clock"),"Crop precedes rotation in the shared render plan");
 Check(new VideoCanvasLayout(VideoAspect.Landscape).OutputSize(1,1)==(1920,1080)
     &&fit.OutputSize(1,1)==(1080,1920)&&new VideoCanvasLayout(VideoAspect.Square).OutputSize(1,1)==(1080,1080),
     "Landscape, portrait and square use the specified output pixels");
 var shortening=new VideoTimeline(timeline.Project);
 var nearEnd=annotation with{Start=Seconds(23),End=Seconds(27)};
 shortening.SetTrack(nearEnd);
 shortening.Seek(Seconds(13));
 Check(shortening.Delete()&&shortening.Project.Tracks[0].End==Seconds(24)&&shortening.Undo()
     &&shortening.Project.Tracks[0]==nearEnd,"Deleting a main clip clamps track tails and Undo restores their full duration");
 var audioTimeline=new VideoTimeline(timeline.Project);
 var baseAudio=audioTimeline.Project;
 var systemTrack=new VideoAudioTrack(VideoAudioLane.System){Volume=25,FadeOut=Seconds(1),Cuts=[new(Seconds(2),Seconds(3))]};
 Check(audioTimeline.SetAudio(systemTrack)&&VideoAudioPlan.NeedsMix(audioTimeline.Project)
     &&audioTimeline.Undo()&&!VideoAudioPlan.NeedsMix(audioTimeline.Project)
     &&audioTimeline.Redo()&&VideoAudioPlan.Settings(audioTimeline.Project,VideoAudioLane.System).Volume==25,
     "Track-level mix participates in Undo and Redo");
 var musicPath=Path.Combine(root,"music.wav");
 var music=new VideoAudioTrack(VideoAudioLane.Music){File=musicPath,InPoint=Seconds(1),OutPoint=Seconds(4),Offset=Seconds(5)};
 Check(audioTimeline.SetAudio(music),"Music has independent timeline and source ranges");
 var spans=VideoAudioPlan.Create(audioTimeline.Project,path=>path.EndsWith(".mic.wav")||path.EndsWith(".system.wav"));
 Check(spans.Count(span=>span.Lane==VideoAudioLane.Microphone)==baseAudio.Clips.Length
     &&spans.Count(span=>span.Lane==VideoAudioLane.System)==baseAudio.Clips.Length
     &&spans.All(span=>span.Lane!=VideoAudioLane.Original),"Recording sidecars replace the embedded mix without doubling audio");
 Check(spans.Single(span=>span.Lane==VideoAudioLane.Music) is {} bed&&bed.At==Seconds(5)&&bed.InPoint==Seconds(1)&&bed.Length==Seconds(3),
     "Music trim and timeline offset remain independent");
 var audioPath=Path.Combine(root,"audio.cvxvideo");
 audioTimeline.Project.Save(audioPath);
 var loadedAudio=VideoProject.Open(audioPath);
 Check(loadedAudio.Version==8&&VideoAudioPlan.Settings(loadedAudio,VideoAudioLane.System).Cuts.SequenceEqual(systemTrack.Cuts)
     &&VideoAudioPlan.Settings(loadedAudio,VideoAudioLane.Music).File==musicPath,"V4 saves audio track settings");
 var previousAudio=Path.Combine(root,"previous-audio.cvxvideo");
 (baseAudio with {Version=3}).Save(previousAudio);
 Check(new VideoTimeline(VideoProject.Open(previousAudio)).Project.Tracks.SequenceEqual(baseAudio.Tracks),
     "Migrating V3 retains existing annotation tracks");
 var saved=timeline.Project;
 Reject(()=>saved.Save(originalPath),"Saving a project cannot replace the source MP4");
 Check(VideoSourceIdentity.Read(originalPath)==identity,"The original MP4 is unchanged after a rejected save");
 var draft=Path.Combine(root,"session.cvxvideo");
 saved.Save(draft);
 var reopened=VideoProject.Open(draft);
 Check(reopened.Clips.SequenceEqual(saved.Clips)&&reopened.Position==saved.Position&&reopened.SelectedClip==saved.SelectedClip
     &&reopened.Source==identity,"Project reload restores clips, selection, playhead and source identity");
 timeline.Seek(Seconds(15));
 timeline.Project.Save(draft);
 File.WriteAllText(draft,"{");
 Check(VideoProject.Recover(draft,out bool recovered)?.Position==saved.Position&&recovered
     &&VideoProject.Open(draft).Position==saved.Position,"Corrupt autosave restores the previous valid revision");
 File.WriteAllText(draft,System.Text.Json.JsonSerializer.Serialize(saved with{Version=999}));
 Reject(()=>VideoProject.Recover(draft,out _),"Unknown project version is preserved for a newer app");
 var relocated=Path.Combine(root,"relocated.mp4");
 File.Move(originalPath,relocated);
 Check(VideoSourceIdentity.Read(relocated)==identity,"A moved original retains its source identity");
 File.WriteAllBytes(relocated,new byte[]{4,3,2,1});
 Check(VideoSourceIdentity.Read(relocated)!=identity,"A different source with the same size fails relink identity");
 Reject(()=> (saved with{Position=Seconds(0),SelectedClip=1}).Validate(),"Invalid position for selected clip is rejected");
 var projectId=Guid.NewGuid().ToString("N");
 saved.Save(Path.Combine(root,"VideoProjects",projectId+".cvxvideo"));
 var videoBackup=UpdateBackup.Create(root,"1.58.0","1.59.0");
 Check(UpdateBackup.Inspect(videoBackup).Files.Any(file=>file.Path=="VideoProjects/"+projectId+".cvxvideo"),
     "Update backup includes video project metadata");
 Console.WriteLine($"{count} capture workflow checks passed.");
}finally{Directory.Delete(root,true);}
