using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Captovix;

internal static class Program
{
    private static int count;
    private static void Check(bool condition,string message){count++;if(!condition)throw new Exception(message);Console.WriteLine("PASS: "+message);}
    private static byte[] Pixels(BitmapSource bitmap){var bytes=new byte[bitmap.PixelWidth*bitmap.PixelHeight*4];bitmap.CopyPixels(bytes,bitmap.PixelWidth*4,0);return bytes;}
    [STAThread] private static int Main()
    {
        string? originalFile=null;MediaItem? item=null;string? lifecycleRoot=null;var lifecycleItems=new List<MediaItem>();
        try{
            var pixels=Enumerable.Repeat((byte)255,64*48*4).ToArray();int blue=(10*64+10)*4;pixels[blue]=255;pixels[blue+1]=pixels[blue+2]=0;
            var source=BitmapSource.Create(64,48,96,96,PixelFormats.Bgra32,null,pixels,64*4);source.Freeze();
            var doc=new EditorDocument{OriginalWidth=64,OriginalHeight=48,SourceHash="hash"};
            doc.Annotations.Add(new(){Kind=AnnotationKind.Rectangle,X1=5,Y1=15,X2=20,Y2=30,Color="#FF0000",Stroke=2});
            doc.Annotations.Add(new(){Kind=AnnotationKind.Highlight,X1=25,Y1=10,X2=35,Y2=30,Color="#F3C746"});
            var output=EditorRenderer.Render(source,doc);var bytes=Pixels(output);int red=(20*64+5)*4;int highlight=(20*64+30)*4;
            Check(output.PixelWidth==64&&output.PixelHeight==48,"Export preserves full resolution");
            Check(bytes[red+2]>230&&bytes[red]<30,"Rectangle annotation is present in flattened output");
            Check(bytes[highlight]>100&&bytes[highlight]<240&&bytes[highlight+3]==255,"Highlight is composited transparently over the image");
            Check(Pixels(source).SequenceEqual(pixels),"Rendering leaves original image pixels unchanged");
            doc.Crop=new(8,8,32,24);output=EditorRenderer.Render(source,doc);bytes=Pixels(output);int mapped=(2*32+2)*4;
            Check(output.PixelWidth==32&&output.PixelHeight==24,"Crop export uses exact pixel dimensions");
            Check(bytes[mapped]>230&&bytes[mapped+1]<30&&bytes[mapped+2]<30,"Crop translates original pixels without drift");
            var reopened=EditorDocument.Deserialize(doc.Serialize());Check(Pixels(EditorRenderer.Render(source,reopened)).SequenceEqual(bytes),"Reopened document renders identically");
            var surface=new EditorSurface();surface.Load(source,new EditorSession(doc));surface.SetZoom(.25);
            Check(Pixels(surface.Flatten()).SequenceEqual(bytes),"Export is independent of viewport zoom");
            var arrow=new Annotation{Kind=AnnotationKind.Arrow,X1=0,Y1=0,X2=40,Y2=0};Check(EditorRenderer.Hit(arrow,new(20,1),3)&&!EditorRenderer.Hit(arrow,new(20,20),3),"Arrow hit testing follows the line, not its bounding box");
            Check(EditorRenderer.Bounds(new(){Kind=AnnotationKind.Text,Text="Hướng dẫn\nBước 1",FontSize=28}).Height>28,"Vietnamese multiline text has correct editable bounds");
            var pattern=new byte[80*60*4];for(int y=0;y<60;y++)for(int x=0;x<80;x++){int i=(y*80+x)*4;pattern[i]=(byte)(x*3);pattern[i+1]=(byte)(y*4);pattern[i+2]=(byte)((x+y)%2*240);pattern[i+3]=255;}
            var patterned=BitmapSource.Create(80,60,96,96,PixelFormats.Bgra32,null,pattern,80*4);patterned.Freeze();
            foreach(var kind in new[]{AnnotationKind.Blur,AnnotationKind.Pixelate,AnnotationKind.Cover}){
                var masked=new EditorDocument{OriginalWidth=80,OriginalHeight=60};var mask=new Annotation{Kind=kind,X1=10,Y1=10,X2=50,Y2=40,Color="#26364B",EffectSize=8};masked.Annotations.Add(mask);
                var full=Pixels(EditorRenderer.Render(patterned,masked));var center=(20*80+20)*4;
                Check(!full.AsSpan(center,3).SequenceEqual(pattern.AsSpan(center,3)),"Export changes protected source pixels: "+kind);
                Check(full.AsSpan(0,4).SequenceEqual(pattern.AsSpan(0,4)),"Export leaves pixels outside mask untouched: "+kind);
                var again=Pixels(EditorRenderer.Render(patterned,EditorDocument.Deserialize(masked.Serialize())));Check(full.SequenceEqual(again),"Mask export is stable after sidecar roundtrip/cache reuse: "+kind);
                masked.Crop=new(16,12,32,24);var cropped=Pixels(EditorRenderer.Render(patterned,masked));bool matches=true;
                for(int row=0;row<24;row++)matches&=cropped.AsSpan(row*32*4,32*4).SequenceEqual(full.AsSpan(((row+12)*80+16)*4,32*4));
                Check(matches,"Cropping does not shift the mask pixel grid: "+kind);
                var maskSurface=new EditorSurface();maskSurface.Load(patterned,new(masked));maskSurface.SetZoom(.25);
                Check(Pixels(maskSurface.Flatten()).SequenceEqual(cropped),"Mask export is independent of canvas zoom: "+kind);
                maskSurface.SelectAll();maskSurface.Nudge(1,0);Check(maskSurface.SelectedIds.Count==1&&mask.X1==11,"Selection and nudge include mask objects: "+kind);
                Check(Pixels(patterned).SequenceEqual(pattern),"Masks never mutate the original bitmap: "+kind);
            }
            var overlap=new EditorDocument{OriginalWidth=80,OriginalHeight=60};
            overlap.Annotations.Add(new(){Kind=AnnotationKind.Cover,X1=10,Y1=10,X2=50,Y2=40,Color="#26364B"});
            overlap.Annotations.Add(new(){Kind=AnnotationKind.Blur,X1=10,Y1=10,X2=50,Y2=40});
            overlap.Annotations.Add(new(){Kind=AnnotationKind.Text,X1=15,Y1=15,Text="Private",Color="#FFFFFF"});
            var covered=Pixels(EditorRenderer.Render(patterned,overlap));int secure=(20*80+20)*4;
            Check(covered[secure]==75&&covered[secure+1]==54&&covered[secure+2]==38&&covered[secure+3]==255,"Solid covers dominate overlapping filters and text");
            var outline=new Annotation{Kind=AnnotationKind.Rectangle,X1=10,Y1=10,X2=50,Y2=40};
            Check(!EditorRenderer.Hit(outline,new(30,25),2)&&EditorRenderer.Hit(outline,new(10,25),2),"Outline hit testing permits selection through empty rectangle interiors");
            var cropSession=new EditorSession(new(){OriginalWidth=80,OriginalHeight=60});var cropSurface=new EditorSurface();cropSurface.Load(patterned,cropSession);
            cropSurface.StartCrop();Check(cropSurface.SetCropDraft(new(10,12,40,24)),"Numeric crop creates an editable pending rectangle");
            Check(cropSession.Document.Crop==null&&!cropSession.CanUndo,"Crop preview does not commit document/history");
            cropSurface.SetZoom(2);Check(cropSurface.CropDraft==new PixelRect(10,12,40,24)&&!cropSession.Dirty,"Zoom preserves pending crop in source pixels");
            Check(!cropSurface.SetCropDraft(new(-1,12,40,24))&&cropSurface.CropDraft==new PixelRect(10,12,40,24),"Invalid numeric crop leaves the prior preview intact");
            cropSurface.NudgeCrop(1,2);Check(cropSurface.CropDraft==new PixelRect(11,14,40,24),"Arrow nudge moves pending crop without applying it");
            Check(cropSurface.ApplyCropDraft()&&cropSurface.CropDraft==null&&cropSession.Document.Crop==new PixelRect(11,14,40,24),"Apply crop commits exact rectangle and exits draft mode");
            Check(cropSurface.Flatten().PixelWidth==40&&cropSurface.Flatten().PixelHeight==24,"Applied numeric crop exports exact dimensions");
            cropSession.Undo();Check(cropSession.Document.Crop==null,"Crop gesture is undoable once");
            cropSurface.StartCrop();cropSurface.SetCropDraft(new(5,5,20,20));cropSurface.ClearCropDraft();
            Check(cropSurface.CropDraft==null&&cropSession.Document.Crop==null,"Cancel pending crop leaves image unmodified");
            foreach(var kind in Enum.GetValues<AnnotationKind>()){
                var sample=new EditorDocument{OriginalWidth=64,OriginalHeight=48};var shape=new Annotation{Kind=kind,X1=12,Y1=8,X2=52,Y2=44,TailX=20,TailY=48,Text="Chữ",FontSize=16};if(kind==AnnotationKind.Freehand){shape.AddPoint(12,8);shape.AddPoint(52,44);}sample.Annotations.Add(shape);
                Check(EditorRenderer.Render(source,sample).PixelWidth==64,"Render tool: "+kind);
            }
            var freehand=new Annotation{Kind=AnnotationKind.Freehand,Color="#FF0000",Stroke=4};freehand.AddPoint(10,20);freehand.AddPoint(60,20);freehand.AddPoint(60,50);
            Check(EditorRenderer.Hit(freehand,new(30,20),2)&&!EditorRenderer.Hit(freehand,new(30,40),2),"Freehand hits actual segments instead of empty bounding-box interiors");
            var strokeDoc=new EditorDocument{OriginalWidth=80,OriginalHeight=60};strokeDoc.Annotations.Add(freehand);
            var strokeBytes=Pixels(EditorRenderer.Render(patterned,strokeDoc));int strokePixel=(20*80+30)*4;
            Check(strokeBytes[strokePixel+2]>230&&strokeBytes[strokePixel]<30,"Freehand is present in full-resolution export");
            var note=new Annotation{Kind=AnnotationKind.Callout,X1=10,Y1=5,X2=65,Y2=41,TailX=30,TailY=58,Text="Ghi chú",FontSize=12};
            Check(EditorRenderer.CalloutGeometry(note).FillContains(new Point(30,52))&&EditorRenderer.Hit(note,new(30,52),1),"Callout tail is drawn and selectable");
            Check(EditorRenderer.Bounds(note).Bottom>=58,"Callout selection bounds include the pointer");
            strokeDoc.Annotations.Add(note);var rendered=Pixels(EditorRenderer.Render(patterned,strokeDoc));
            Check(rendered.SequenceEqual(Pixels(EditorRenderer.Render(patterned,EditorDocument.Deserialize(strokeDoc.Serialize())))),"Callout and stroke export roundtrip is pixel-stable");
            AnnotationEditing.Group(strokeDoc,[freehand.Id,note.Id]);var groupedSurface=new EditorSurface();var groupedSession=new EditorSession(strokeDoc);groupedSurface.Load(patterned,groupedSession);groupedSurface.SelectIds([freehand.Id]);
            Check(groupedSurface.SelectedIds.Count==2,"Selecting a member on the surface expands its group");
            double oldTail=note.TailX;groupedSurface.Nudge(1,0);Check(freehand.Points[0].X==11&&note.TailX==oldTail+1,"Group nudge moves stroke points and callout pointer together");
            groupedSession.Undo();Check(groupedSession.Document.Annotations[0].Points[0].X==10&&groupedSession.Document.Annotations[1].TailX==oldTail,"Group movement has one complete undo");
            var tinyBytes=new byte[4*3*4];for(int y=0;y<3;y++)for(int x=0;x<4;x++){int n=(y*4+x)*4;tinyBytes[n]=(byte)(x*50);tinyBytes[n+1]=(byte)(y*80);tinyBytes[n+2]=100;tinyBytes[n+3]=255;}
            var tiny=BitmapSource.Create(4,3,96,96,PixelFormats.Bgra32,null,tinyBytes,4*4);tiny.Freeze();
            for(int q=0;q<4;q++)for(int fx=0;fx<2;fx++)for(int fy=0;fy<2;fy++){
                var transformDoc=new EditorDocument{OriginalWidth=4,OriginalHeight=3,Appearance=new(){QuarterTurns=q,FlipHorizontal=fx==1,FlipVertical=fy==1}};
                var result=EditorRenderer.Render(tiny,transformDoc);var resultPixels=Pixels(result);var layout=new ImageLayout(transformDoc.Viewport,transformDoc.Appearance);bool equal=true;
                for(int y=0;y<3;y++)for(int x=0;x<4;x++){var rotatedCenter=layout.ToOutput(x+.5,y+.5);int target=((int)Math.Floor(rotatedCenter.Y)*result.PixelWidth+(int)Math.Floor(rotatedCenter.X))*4;equal&=resultPixels.AsSpan(target,4).SequenceEqual(tinyBytes.AsSpan((y*4+x)*4,4));}
                Check(equal,$"Actual rendered pixel orientation: {q}/{fx}/{fy}");
            }
            var styledDoc=new EditorDocument{OriginalWidth=4,OriginalHeight=3,Appearance=new(){Padding=2,BorderWidth=1,BorderColor="#FF0000",Transparent=false,BackgroundColor="#123456"}};
            var styledBitmap=EditorRenderer.Render(tiny,styledDoc);var styledPixels=Pixels(styledBitmap);int backgroundPixel=(1*styledBitmap.PixelWidth+1)*4;
            Check(styledBitmap.PixelWidth==10&&styledBitmap.PixelHeight==9,"Export dimensions include exact border and padding");
            Check(styledPixels[backgroundPixel]==0x56&&styledPixels[backgroundPixel+1]==0x34&&styledPixels[backgroundPixel+2]==0x12,"Canvas background color is rendered in exported pixels");
            styledDoc.Appearance.Transparent=true;styledPixels=Pixels(EditorRenderer.Render(tiny,styledDoc));Check(styledPixels[backgroundPixel+3]==0,"PNG composition retains transparent padding");
            styledDoc.Appearance.Shadow=true;styledDoc.Appearance.ShadowBlur=8;styledDoc.Appearance.ShadowX=-4;styledDoc.Appearance.ShadowY=6;
            var shadowBitmap=EditorRenderer.Render(tiny,styledDoc);Check(shadowBitmap.PixelWidth==26&&shadowBitmap.PixelHeight==25&&Pixels(shadowBitmap).Where((v,index)=>index%4==3).Any(a=>a>0&&a<255),"Soft frame shadow is exported with alpha and full bounds");
            Check(Pixels(tiny).SequenceEqual(tinyBytes),"Presentation recipe never changes original bitmap bytes");
            originalFile=Path.Combine(Path.GetTempPath(),"Captovix-"+Guid.NewGuid().ToString("N")+".png");var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(source));using(var f=File.Create(originalFile))encoder.Save(f);
            item=new(){Id=Guid.NewGuid().ToString("N"),FilePath=originalFile};var session=DocumentStore.Load(item,source,out _);session.Change(d=>d.Annotations.Add(new(){Kind=AnnotationKind.Step,X1=20,Y1=20}));DocumentStore.Save(item,session,source);
            Check(!session.Dirty&&File.Exists(DocumentStore.EditPath(item.Id)),"Document saves as a sidecar and clears dirty state");
            var loaded=DocumentStore.Load(item,source,out var warning);Check(warning==null&&loaded.Document.Annotations.Count==1,"Sidecar reopens as editable objects");
            var legacySidecar=loaded.Document.Serialize().Replace("\"Version\": 7","\"Version\": 4");File.WriteAllText(DocumentStore.EditPath(item.Id),legacySidecar);
            var migrated=DocumentStore.Load(item,source,out warning);migrated.Change(d=>d.Annotations[0].Color="#123456");DocumentStore.Save(item,migrated,source);
            Check(File.ReadAllText(DocumentStore.EditPath(item.Id)+".pre-v1.9.bak")==legacySidecar,"First schema-7 save retains the exact old sidecar before replacement");
            Check(EditorDocument.Deserialize(File.ReadAllText(DocumentStore.EditPath(item.Id))).Version==7,"Sidecar migration writes current schema after making backup");
            File.Delete(DocumentStore.EditPath(item.Id)+".pre-v1.9.bak");
            DocumentStore.Delete(item);Check(!File.Exists(DocumentStore.EditPath(item.Id)),"Sidecar cleanup removes the selected item only");
            var app=new App();app.InitializeComponent();var window=new MainWindow();
            Check(window.WindowState==WindowState.Maximized,"Editor starts maximized");
            Check(window.Title.Contains("Captovix")&&window.Title.Contains("V"+VersionInfo.Current),"Window identity matches the app version");
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            object? Invoke(string name,params object[] values)=>typeof(MainWindow).GetMethod(name,flags)!.Invoke(window,values);
            // Regression: real constructor above must survive XAML SelectionChanged.
            var ready=typeof(MainWindow).GetField("_editorReady",flags)!;
            Check((bool)ready.GetValue(window)!,"Editor is ready only after complete initialization");
            ready.SetValue(window,false);
            var titleField=typeof(MainWindow).GetField("PropertyTitle",flags)!;
            var titleValue=titleField.GetValue(window);titleField.SetValue(window,null);
            try{
                Invoke("Style_Changed",window,new System.Windows.Controls.SelectionChangedEventArgs(System.Windows.Controls.Primitives.Selector.SelectionChangedEvent,Array.Empty<object>(),Array.Empty<object>()));
                Invoke("SyncProperties");Invoke("UpdateEditorReadout");
                Check(true,"Callbacks tolerate incomplete XAML initialization without accessing later controls");
            }finally{titleField.SetValue(window,titleValue);ready.SetValue(window,true);}
            var fontPicker=(System.Windows.Controls.ComboBox)window.FindName("FontPicker");
            var startupSurface=(EditorSurface)window.FindName("Surface");
            var fontChoice=fontPicker.Items.Cast<System.Windows.Controls.ComboBoxItem>().First(i=>i.Tag?.ToString()=="28");
            fontPicker.SelectedItem=fontChoice;Invoke("SyncProperties");
            Check(startupSurface.TextSize==28,"Style changes work after initialization and synchronize without recursive callbacks");
            var inlineSession=new EditorSession(new(){OriginalWidth=80,OriginalHeight=60});var textObject=new Annotation{Kind=AnnotationKind.Text,X1=10,Y1=10,Text="Trước",FontSize=16};inlineSession.Document.Annotations.Add(textObject);
            typeof(MainWindow).GetField("_session",flags)!.SetValue(window,inlineSession);
            var uiSurface=(EditorSurface)window.FindName("Surface");uiSurface.Load(patterned,inlineSession);uiSurface.SelectIds([textObject.Id]);
            var input=(System.Windows.Controls.TextBox)window.FindName("InlineText");
            Invoke("BeginInlineText");input.Text="Bỏ phần nhập";Invoke("EndInlineText",false);
            Check(inlineSession.Document.Annotations[0].Text=="Trước"&&!inlineSession.CanUndo,"Inline Escape preserves document and history");
            Invoke("BeginInlineText");input.Text="Sau\nTiếng Việt";Invoke("EndInlineText",true);
            Check(inlineSession.Document.Annotations[0].Text=="Sau\nTiếng Việt"&&inlineSession.CanUndo,"Inline completion writes the multiline buffer");
            inlineSession.Undo();Check(inlineSession.Document.Annotations[0].Text=="Trước"&&!inlineSession.CanUndo,"A complete inline edit is exactly one undo entry");inlineSession.Redo();
            Invoke("BeginInlineText");input.Text="Lưu phần đang gõ";Invoke("FlushEdits",false);
            Check(inlineSession.Document.Annotations[0].Text=="Lưu phần đang gõ"&&input.Visibility==Visibility.Collapsed,"Save preparation commits an active inline buffer");
            Invoke("BeginInlineText");input.Text="Xuất phần đang gõ";Check((bool)Invoke("PrepareCropForExport")!&&inlineSession.Document.Annotations[0].Text=="Xuất phần đang gõ","Export preparation includes the latest inline text");
            Action<ImageAppearance> styleChange=s=>{s.QuarterTurns=1;s.ResizeWidth=40;s.ResizeHeight=80;s.CanvasWidth=60;s.CanvasHeight=90;s.Padding=5;s.BorderWidth=2;s.Transparent=false;};
            Invoke("ApplyAppearance",styleChange,false);
            Check(uiSurface.Layout!.Width==74&&uiSurface.Layout.Height==104&&inlineSession.Document.Annotations[0].X1==10,"MainWindow transform changes output dimensions without moving source annotations");
            var resizeWidth=(System.Windows.Controls.TextBox)window.FindName("ResizeW");var resizeHeight=(System.Windows.Controls.TextBox)window.FindName("ResizeH");resizeWidth.Text="20";
            Check(resizeHeight.Text=="40","Resize inspector preserves current image aspect ratio");
            Invoke("ApplyResize_Click",window,new RoutedEventArgs());Check(inlineSession.Document.Appearance.ResizeWidth==20&&inlineSession.Document.Appearance.ResizeHeight==40,"Resize inspector applies numeric output dimensions");
            inlineSession.Undo();Check(inlineSession.Document.Appearance.ResizeWidth==40,"Resize inspector creates one undo transaction");uiSurface.Refresh();
            Invoke("ApplyAppearance",(Action<ImageAppearance>)(_=>{}),true);
            Check(!inlineSession.Document.Appearance.HasEdits&&inlineSession.Document.Annotations.Count==1,"Reset appearance keeps the editable annotation layer");
            Invoke("RenderWorkflow");
            Check(window.FindName("WorkflowPanel") is System.Windows.Controls.StackPanel,"Workflow configuration is part of the single Editor");
            Check(((System.Windows.Controls.StackPanel)window.FindName("HotkeyRows")).Children.Count==5,"Five photo hotkey editors are generated");
            Check(((System.Windows.Controls.ComboBox)window.FindName("AfterPhotoPicker")).SelectedIndex==0,"After-capture defaults to opening Editor");
            Check(((System.Windows.Controls.CheckBox)window.FindName("CloseToTrayToggle")).IsChecked==false&&((System.Windows.Controls.CheckBox)window.FindName("StartWindowsToggle")).IsChecked==false,"Tray closing and autorun are opt-in");
            Check(((System.Windows.Controls.ComboBox)window.FindName("CapturePresetPicker")).Items.Count==3,"Capture presets are populated independently of annotation presets");
            Check(window.FindName("PresetPicker")!=window.FindName("CapturePresetPicker"),"Capture and annotation preset controls remain distinct");
            Check(!((System.Windows.Controls.Button)window.FindName("CancelCaptureButton")).IsEnabled,"Cancel is unavailable while idle");
            Check(inlineSession.Document.Annotations.Count==1,"Rendering workflow configuration preserves edited image objects");
            // Regression for the 1.5.0 stack trace: VisualChanged fires with
            // a stale MainWindow session while the surface has no layout.
            uiSurface.Load(null,null);
            Check(((System.Windows.Controls.TextBlock)window.FindName("ImageSizeLabel")).Text==""&&((System.Windows.Controls.TextBlock)window.FindName("ZoomLabel")).Text=="—","Cleared surface tolerates a stale window session and clears image/zoom readouts");
            Check(!((System.Windows.Controls.Button)window.FindName("RestoreCropButton")).IsEnabled&&!((System.Windows.Controls.StackPanel)window.FindName("ImageStylePanel")).IsEnabled,"Empty surface disables crop restore and appearance controls");
            uiSurface.Load(patterned,inlineSession);
            Check(((System.Windows.Controls.TextBlock)window.FindName("ImageSizeLabel")).Text.StartsWith("80 × 60"),"Reload restores readout from the matching document layout");
            Invoke("UnloadEditor");Invoke("UnloadEditor");
            Check(uiSurface.Session==null&&uiSurface.Original==null&&typeof(MainWindow).GetField("_session",flags)!.GetValue(window)==null,"Repeated unload is safe and clears both document owners");
            lifecycleRoot=Path.Combine(Path.GetTempPath(),"Captovix-Lifecycle-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(lifecycleRoot);
            MediaItem Fixture(BitmapSource bitmap,string name){var path=Path.Combine(lifecycleRoot,name+".png");var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(bitmap));using(var f=File.Create(path))enc.Save(f);var media=new MediaItem{Id=Guid.NewGuid().ToString("N"),Title=name,FilePath=path,Width=bitmap.PixelWidth,Height=bitmap.PixelHeight};lifecycleItems.Add(media);return media;}
            var first=Fixture(source,"first");var second=Fixture(patterned,"second");
            var rows=new System.Collections.ObjectModel.ObservableCollection<MediaItem>{first,second};var list=(System.Windows.Controls.ListBox)window.FindName("MediaList");list.ItemsSource=rows;list.SelectedItem=first;
            var firstSession=(EditorSession)typeof(MainWindow).GetField("_session",flags)!.GetValue(window)!;
            firstSession.Change(d=>{d.Crop=new(2,2,40,30);d.Annotations.Add(new(){Kind=AnnotationKind.Rectangle,X1=4,Y1=4,X2=24,Y2=20});d.Appearance.Padding=4;});
            list.SelectedItem=second;
            Check(uiSurface.Session?.Document.OriginalWidth==80&&uiSurface.Layout?.Width==80,"Actual ListBox image-to-image transition opens the new image without a null layout failure");
            Check(File.Exists(DocumentStore.EditPath(first.Id)),"Switching commits the edited prior image sidecar");
            var readout=((System.Windows.Controls.TextBlock)window.FindName("ImageSizeLabel")).Text;
            firstSession.Change(d=>d.Annotations.Add(new(){Kind=AnnotationKind.Step,X1=5,Y1=5}));
            Check(((System.Windows.Controls.TextBlock)window.FindName("ImageSizeLabel")).Text==readout,"An unloaded session cannot update the new document UI");
            bool switching=true;for(var n=0;n<30;n++){list.SelectedItem=first;switching&=uiSurface.Layout?.Width==48&&uiSurface.Session?.Document.Annotations.Count==1;list.SelectedItem=second;switching&=uiSurface.Layout?.Width==80&&uiSurface.Session?.Document.Annotations.Count==0;}
            Check(switching,"Thirty image-switch roundtrips retain separate crop/style/annotation state");
            list.SelectedItem=null;
            Check(uiSurface.Session==null&&uiSurface.Layout==null&&((System.Windows.Controls.StackPanel)window.FindName("EmptyState")).Visibility==Visibility.Visible,"Clearing the library selection safely unloads the image");
            rows.Add(Fixture(tiny,"new-capture"));list.SelectedItem=rows[^1];
            Check(uiSurface.Layout?.Width==4&&uiSurface.Layout?.Height==3,"Selecting a newly added capture/import row after an empty selection opens correctly");
            Check(MediaStore.LoadImage(first.FilePath).PixelWidth==64&&MediaStore.LoadImage(second.FilePath).PixelWidth==80,"Switching edited media leaves both original image dimensions intact");
            Check(((System.Windows.Controls.TextBlock)window.FindName("OriginalSizeLabel")).Text.Contains("4 × 3")&&((System.Windows.Controls.TextBlock)window.FindName("ImageSizeLabel")).Text.StartsWith("4 × 3"),"Original and export dimensions are separate readouts");
            typeof(MainWindow).GetField("_actualPixels",flags)!.SetValue(window,true);typeof(MainWindow).GetField("_fitMode",flags)!.SetValue(window,false);uiSurface.SetDisplayScale(1);uiSurface.SetZoom(1);
            double previousDpi=1;bool exactZoom=true;
            foreach(var dpi in new[]{1.25,1.5,1.75,2,1d}){Invoke("OnDpiChanged",new DpiScale(previousDpi,previousDpi),new DpiScale(dpi,dpi));exactZoom&=Math.Abs(uiSurface.PhysicalZoom-1)<1e-9;previousDpi=dpi;}
            Check(exactZoom,"Simulated Editor DPI events preserve Actual Pixels across 100/125/150/175/200%");
            typeof(MainWindow).GetField("_actualPixels",flags)!.SetValue(window,false);uiSurface.SetZoom(2);bool freeZoom=true;
            foreach(var dpi in new[]{2d,1.75,1.5,1.25,1}){Invoke("OnDpiChanged",new DpiScale(previousDpi,previousDpi),new DpiScale(dpi,dpi));freeZoom&=Math.Abs(uiSurface.PhysicalZoom-2)<1e-9;previousDpi=dpi;}
            Check(freeZoom,"Simulated Editor DPI events preserve a chosen physical zoom without changing output");
            Check(uiSurface.Flatten().PixelWidth==4&&uiSurface.Flatten().PixelHeight==3,"Editor monitor transitions do not change exported pixel dimensions");
            // V1.7 inspector integration on a real MainWindow/session.
            list.SelectedItem=second;Invoke("SelectTool",EditorTool.Shape);
            var shapeSession=(EditorSession)typeof(MainWindow).GetField("_session",flags)!.GetValue(window)!;
            var styledShape=new Annotation{Kind=AnnotationKind.Rectangle,X1=10,Y1=10,X2=60,Y2=45};shapeSession.Change(d=>d.Annotations.Add(styledShape));uiSurface.SelectIds([styledShape.Id]);
            var shapePicker=(System.Windows.Controls.ComboBox)window.FindName("ShapeKindPicker");shapePicker.SelectedValue=AnnotationKind.Star;
            Check(styledShape.Kind==AnnotationKind.Star&&styledShape.X1==10&&styledShape.X2==60,"Shape picker converts the selected shape without changing its bounds");
            void Field(string name,string value)=>((System.Windows.Controls.TextBox)window.FindName(name)).Text=value;
            Field("ShapeStrokeHex","#123456");Field("ShapeFillHex","#FFE28A");Field("ShapeThickness","7.5");Field("ShapeStarPoints","7");Field("ShapeStarInset","35");
            ((System.Windows.Controls.CheckBox)window.FindName("ShapeFillEnabled")).IsChecked=true;
            ((System.Windows.Controls.ComboBox)window.FindName("ShapeDash")).SelectedIndex=3;
            Check((bool)Invoke("ApplyShapeStyle")!&&styledShape.Stroke==7.5&&styledShape.Dash==StrokePattern.DashDot&&styledShape.FillColor=="#FFE28A"&&styledShape.StarPoints==7,"Inspector commits validated shape styling");
            var styleJson=shapeSession.Document.Serialize();Field("ShapeThickness","NaN");Check(!(bool)Invoke("ApplyShapeStyle")!&&shapeSession.Document.Serialize()==styleJson,"Invalid inspector input leaves document untouched");Invoke("SyncProperties");
            shapeSession.Undo();uiSurface.Refresh();Check(shapeSession.Document.Annotations[0].Stroke==4&&shapeSession.Document.Annotations[0].Kind==AnnotationKind.Star,"Style inspector has one complete undo step");shapeSession.Redo();uiSurface.Refresh();
            Invoke("SelectTool",EditorTool.Line);Check(uiSurface.SelectedIds.Count==0&&uiSurface.Tool==EditorTool.Line,"Choosing a drawing tool prepares defaults without altering previous objects");
            var selectedPreset=(System.Windows.Controls.ComboBox)window.FindName("PresetPicker");selectedPreset.SelectedItem=selectedPreset.Items.Cast<AnnotationPreset>().First(p=>p.Name=="Khoanh lỗi đỏ");var prior=shapeSession.Document.Serialize();
            Invoke("ApplyPreset_Click",new System.Windows.Controls.Button{Tag="Defaults"},new RoutedEventArgs());Check(shapeSession.Document.Serialize()==prior&&uiSurface.Tool==EditorTool.Shape&&uiSurface.ShapeDefaults.Dash==StrokePattern.Dash,"Use-preset-for-new changes defaults without changing drawn objects");
            window.Close();
            Console.WriteLine($"{count} Windows renderer/document checks passed. Screen/audio capture remains a manual test.");return 0;
        }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
        finally{foreach(var media in lifecycleItems)DocumentStore.Delete(media);if(lifecycleRoot!=null&&Directory.Exists(lifecycleRoot))Directory.Delete(lifecycleRoot,true);if(item!=null)DocumentStore.Delete(item);if(originalFile!=null)File.Delete(originalFile);}
    }
}
