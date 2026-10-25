using System.Reflection;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Captovix;
internal static class Program
{
    private static int count;
    private static void Check(bool value,string label){if(!value)throw new Exception(label);count++;Console.WriteLine("PASS: "+label);}
    private static byte[] Pixels(BitmapSource image){var p=new byte[image.PixelWidth*image.PixelHeight*4];image.CopyPixels(p,image.PixelWidth*4,0);return p;}
    private static BitmapSource Image(byte[] raw){var b=BitmapSource.Create(240,200,96,96,PixelFormats.Bgra32,null,raw,240*4);b.Freeze();return b;}
    private static (byte B,byte G,byte R,byte A) Pixel(BitmapSource image,int x,int y){var raw=Pixels(image);int i=(y*image.PixelWidth+x)*4;return(raw[i],raw[i+1],raw[i+2],raw[i+3]);}
    [STAThread]private static int Main()
    {
        try{
            var raw=new byte[240*200*4];for(int y=0;y<200;y++)for(int x=0;x<240;x++){int i=(y*240+x)*4;raw[i]=(byte)x;raw[i+1]=(byte)y;raw[i+2]=(byte)((x+y)%256);raw[i+3]=255;}
            var altered=(byte[])raw.Clone();for(int y=104;y<116;y++)for(int x=114;x<126;x++){int i=(y*240+x)*4;altered[i]=255;altered[i+1]=0;altered[i+2]=255;}
            var source=Image(raw);var secretChanged=Image(altered);
            foreach(var shape in Enum.GetValues<RegionShape>())foreach(double angle in new[]{0d,33d})foreach(double zoom in new[]{1.25,3,8})foreach(var filterKind in new[]{AnnotationKind.Blur,AnnotationKind.Pixelate}){
                var cover=new Annotation{Kind=AnnotationKind.Cover,X1=95,Y1=85,X2=145,Y2=135,Color="#123456",RegionShape=shape,Rotation=angle,FocusOpacity=0,StrokeOpacity=0,FillOpacity=0};
                var lens=new Annotation{Kind=AnnotationKind.Magnifier,X1=65,Y1=55,X2=175,Y2=165,Magnification=zoom,RegionShape=shape,Rotation=angle};
                var blur=new Annotation{Kind=filterKind,X1=90,Y1=80,X2=150,Y2=140};
                var doc=new EditorDocument{OriginalWidth=240,OriginalHeight=200,Annotations=[cover,lens,blur]};doc.Validate(240,200,"");
                var first=EditorRenderer.Render(source,doc);var second=EditorRenderer.Render(secretChanged,doc);
                Check(Pixels(first).SequenceEqual(Pixels(second)),$"Changing hidden source pixels cannot leak through magnifier/filter/order: {shape}/{angle}/{zoom}/{filterKind}");
                Check(Pixel(first,120,110)==(0x56,0x34,0x12,255),"Solid mask remains fully opaque even if generic opacity fields are zero");
                var restored=EditorDocument.Deserialize(doc.Serialize());restored.Validate(240,200,"");Check(Pixels(first).SequenceEqual(Pixels(EditorRenderer.Render(source,restored))),"Focus/mask rendering survives save/reopen");
            }
            var light=new Annotation{Kind=AnnotationKind.Spotlight,X1=40,Y1=40,X2=100,Y2=120,Color="#000000",FocusOpacity=70};var light2=new Annotation{Kind=AnnotationKind.Spotlight,X1=70,Y1=50,X2=150,Y2=130,Color="#000000",FocusOpacity=70};
            var focusDoc=new EditorDocument{OriginalWidth=240,OriginalHeight=200,Annotations=[light,light2]};var focused=EditorRenderer.Render(source,focusDoc);Check(Pixel(focused,80,80)==Pixel(source,80,80),"Overlapping spotlight holes are a union, with no double shade in overlap");Check(Pixel(focused,130,90)==Pixel(source,130,90),"Second spotlight hole remains bright");Check(Pixel(focused,200,170).R<Pixel(source,200,170).R,"Outside spotlight regions is darkened");
            light2.Visible=false;var one=EditorRenderer.Render(source,focusDoc);Check(Pixel(one,130,90).R<Pixel(source,130,90).R,"Hidden spotlight does not contribute a bright hole");
            var ellipse=new Annotation{Kind=AnnotationKind.Highlight,RegionShape=RegionShape.Ellipse,X1=30,Y1=30,X2=100,Y2=100,Color="#FFFF00",FocusOpacity=40};var highlightDoc=new EditorDocument{OriginalWidth=240,OriginalHeight=200,Annotations=[ellipse]};var high=EditorRenderer.Render(source,highlightDoc);Check(Pixel(high,31,31)==Pixel(source,31,31)&&Pixel(high,65,65)!=Pixel(source,65,65),"Ellipse highlight affects its interior and preserves bounding-box corners");Check(!EditorRenderer.Hit(ellipse,new(31,31),0)&&EditorRenderer.Hit(ellipse,new(65,65),0),"Ellipse hit test matches its visible region");
            var mask=new Annotation{Kind=AnnotationKind.Cover,X1=30,Y1=20,X2=80,Y2=70,Color="#000000"};var croppedDoc=new EditorDocument{OriginalWidth=240,OriginalHeight=200,Crop=new(20,10,180,150),Annotations=[mask,new(){Kind=AnnotationKind.Magnifier,X1=15,Y1=5,X2=100,Y2=100,Magnification=2},light]};croppedDoc.Annotations.Add(new(){Kind=AnnotationKind.Blur,X1=0,Y1=0,X2=80,Y2=80,EffectSize=24});
            var outside=(byte[])raw.Clone();for(int y=0;y<200;y++)for(int x=0;x<240;x++)if(x<20||x>=200||y<10||y>=160){int i=(y*240+x)*4;outside[i]=0;outside[i+1]=255;outside[i+2]=255;}
            Check(Pixels(EditorRenderer.Render(source,croppedDoc)).SequenceEqual(Pixels(EditorRenderer.Render(Image(outside),croppedDoc))),"Changing source outside crop cannot leak into lens or blur inside export");
            var surface=new EditorSurface();surface.Load(source,new(croppedDoc));var atOne=Pixels(surface.Flatten());surface.SetZoom(.25);Check(Pixels(surface.Flatten()).SequenceEqual(atOne)&&surface.Flatten().PixelWidth==180,"Crop and display zoom preserve exported dimensions and mask coordinates");
            foreach(int angle in new[]{90,180,270}){croppedDoc.Appearance.QuarterTurns=angle/90;var b=surface.Flatten();Check(b.PixelWidth==(angle==180?180:150),"Whole-image rotation transforms focus/masks with the image: "+angle);}croppedDoc.Appearance.QuarterTurns=0;
            var folder=Path.Combine(Path.GetTempPath(),"Captovix-focus-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);try{var output=Path.Combine(folder,"shared.png");ImageTransfer.Save(surface.Flatten(),output,false);using var stream=File.OpenRead(output);var decoder=BitmapDecoder.Create(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);Check(decoder.Frames.Count==1&&decoder.Frames[0].PixelWidth==180,"Shared PNG is one flattened image at export dimensions");}finally{Directory.Delete(folder,true);}
            var session=new EditorSession(new(){OriginalWidth=240,OriginalHeight=200});var pointer=new EditorSurface();pointer.Load(source,session);pointer.ShapeDefaults.StrokeEnabled=false;pointer.ShapeDefaults.StrokeOpacity=0;pointer.Tool=EditorTool.Magnifier;pointer.PointerDown(new(30,40));pointer.PointerMove(new(90,100));pointer.PointerUp(new(90,100));Check(pointer.Selected?.Kind==AnnotationKind.Magnifier&&pointer.Tool==EditorTool.Magnifier&&pointer.Selected.StrokeEnabled&&pointer.Selected.StrokeOpacity==100,"Native pointer path creates an editable lens and retains its tool");pointer.Tool=EditorTool.Step;pointer.SelectIds([]);pointer.PointerDown(new(60,60));pointer.PointerMove(new(80,70));pointer.PointerUp(new(80,70));Check(session.Document.Annotations.Count==1&&pointer.Selected?.Kind==AnnotationKind.Magnifier,"Smart selection can move a lens while Step is active");
            var app=new App();app.InitializeComponent();var window=new MainWindow();var flags=BindingFlags.Instance|BindingFlags.NonPublic;typeof(MainWindow).GetField("_session",flags)!.SetValue(window,session);var editor=(EditorSurface)window.FindName("Surface");editor.Load(source,session);editor.SelectIds([session.Document.Annotations[0].Id]);Check(((StackPanel)window.FindName("FocusStudioPanel")).Visibility==Visibility.Visible,"Selecting lens shows Focus & Privacy properties");((TextBox)window.FindName("FocusZoomField")).Text="4";typeof(MainWindow).GetMethod("ApplyFocus_Click",flags)!.Invoke(window,new object[]{window,new RoutedEventArgs()});Check(editor.Selected!.Magnification==4,"Focus inspector applies magnification through the actual handler");Check(window.Title.Contains("V1.15.0")&&window.WindowState==WindowState.Maximized,"Editor starts at current version, maximized, without initialization exception");window.Close();Check(Pixels(source).SequenceEqual(raw),"Focus/privacy operations do not alter original source pixels");
            Console.WriteLine($"{count} focus/privacy Windows checks passed. Real clipboard, DPI, pointer devices and installation remain desktop acceptance.");return 0;
        }catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
}
