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
                var sample=new EditorDocument{OriginalWidth=64,OriginalHeight=48};sample.Annotations.Add(new(){Kind=kind,X1=12,Y1=12,X2=52,Y2=38,Text="Chữ",FontSize=16});
                Check(EditorRenderer.Render(source,sample).PixelWidth==64,"Render tool: "+kind);
            }
            originalFile=Path.Combine(Path.GetTempPath(),"Captovix-"+Guid.NewGuid().ToString("N")+".png");var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(source));using(var f=File.Create(originalFile))encoder.Save(f);
            item=new(){Id=Guid.NewGuid().ToString("N"),FilePath=originalFile};var session=DocumentStore.Load(item,source,out _);session.Change(d=>d.Annotations.Add(new(){Kind=AnnotationKind.Step,X1=20,Y1=20}));DocumentStore.Save(item,session,source);
            Check(!session.Dirty&&File.Exists(DocumentStore.EditPath(item.Id)),"Document saves as a sidecar and clears dirty state");
            var loaded=DocumentStore.Load(item,source,out var warning);Check(warning==null&&loaded.Document.Annotations.Count==1,"Sidecar reopens as editable objects");
            DocumentStore.Delete(item);Check(!File.Exists(DocumentStore.EditPath(item.Id)),"Sidecar cleanup removes the selected item only");
            var app=new App();app.InitializeComponent();var window=new MainWindow();
            Check(window.WindowState==WindowState.Maximized,"Editor starts maximized");
            Check(window.Title.Contains("Captovix")&&window.Title.Contains("V"+VersionInfo.Current),"Window identity matches the app version");window.Close();
            Console.WriteLine($"{count} Windows renderer/document checks passed. Screen/audio capture remains a manual test.");return 0;
        }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
        finally{if(item!=null)DocumentStore.Delete(item);if(originalFile!=null)File.Delete(originalFile);}
    }
}
