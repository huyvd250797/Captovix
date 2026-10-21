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
        string? originalFile=null;MediaItem? item=null;
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
            DocumentStore.Delete(item);Check(!File.Exists(DocumentStore.EditPath(item.Id)),"Sidecar cleanup removes the selected item only");
            var app=new App();app.InitializeComponent();var window=new MainWindow();
            Check(window.WindowState==WindowState.Maximized,"Editor starts maximized");
            Check(window.Title.Contains("Captovix")&&window.Title.Contains("V"+VersionInfo.Current),"Window identity matches the app version");
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            object? Invoke(string name,params object[] values)=>typeof(MainWindow).GetMethod(name,flags)!.Invoke(window,values);
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
            window.Close();
            Console.WriteLine($"{count} Windows renderer/document checks passed. Screen/audio capture remains a manual test.");return 0;
        }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
        finally{if(item!=null)DocumentStore.Delete(item);if(originalFile!=null)File.Delete(originalFile);}
    }
}
