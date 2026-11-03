using Captovix;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
internal static class Program
{
    static void Check(bool value,string label){if(!value)throw new Exception(label);Console.WriteLine("PASS "+label);}
    [STAThread]static int Main(){try{
        var pixels=Enumerable.Repeat((byte)255,400*300*4).ToArray();var bitmap=ImageTransfer.Bitmap(400,300,pixels);
        var surface=new EditorSurface();var session=new EditorSession(new(){OriginalWidth=400,OriginalHeight=300});surface.Load(bitmap,session);surface.StartCrop();
        surface.PointerDown(new(100,80));surface.PointerMove(new(250,200));surface.PointerUp(new(250,200));
        Check(surface.CropDraft==null&&surface.Tool==EditorTool.Select,"Mouse release applies crop and exits tool without Enter");Check(surface.Flatten().PixelWidth==150&&surface.Flatten().PixelHeight==120,"Direct crop exports exact selected pixels");session.Undo();Check(surface.Flatten().PixelWidth==400,"Direct crop is one undoable action");
        var app=new App();app.InitializeComponent();var window=new MainWindow();var flags=BindingFlags.NonPublic|BindingFlags.Instance;var edit=(EditorSurface)window.FindName("Surface");var step=new Annotation{Kind=AnnotationKind.Step,X1=80,Y1=80,X2=120,Y2=120};var doc=new EditorDocument{OriginalWidth=400,OriginalHeight=300,Annotations=[step]};var editing=new EditorSession(doc);typeof(MainWindow).GetField("_session",flags)!.SetValue(window,editing);edit.Load(bitmap,editing);edit.SelectIds([step.Id]);
        void Paint(string? color,bool fill)=>typeof(MainWindow).GetMethod("SetObjectPaint",flags)!.Invoke(window,[color,fill]);
        Paint("#123456",false);Paint("#159568",true);Check(step.StepStrokeColor=="#123456"&&step.Color=="#159568","Step border and fill change independently through real controls");Paint(null,true);Check(step.StepFillVisible==false,"Step supports transparent fill");Paint(null,false);Check(!step.StrokeEnabled,"Step supports transparent border");
        var factory=typeof(MainWindow).GetMethod("StampImage",BindingFlags.NonPublic|BindingFlags.Static)!;
        foreach(var stamp in StampCatalog.All){var image=(BitmapSource)factory.Invoke(null,[stamp])!;Check(image.PixelWidth==256&&ImageTransfer.Bgra(image).Where((_,i)=>i%4==3).Any(v=>v>0),"Stamp preview has visible vector pixels: "+stamp.Id);}
        var tools=(WrapPanel)window.FindName("ToolBox");Check(tools.Children.IndexOf((Button)window.FindName("RecentStudioButton"))+1==tools.Children.IndexOf((Button)window.FindName("StudioMoreButton")),"Recent tool sits immediately left of More");
        window.Close();return 0;
    }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
