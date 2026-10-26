using Captovix;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
internal static class Program{
 static int count;static void Check(bool value,string message){if(!value)throw new Exception(message);count++;Console.WriteLine("PASS: "+message);}
 static byte[] Pixels(BitmapSource b){var p=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(p,b.PixelWidth*4,0);return p;}
 [STAThread]static int Main(){try{
 var raw=Enumerable.Repeat((byte)255,200*160*4).ToArray();var image=BitmapSource.Create(200,160,96,96,PixelFormats.Bgra32,null,raw,800);image.Freeze();
 var shape=new Annotation{Kind=AnnotationKind.Rectangle,X1=60,Y1=40,X2=120,Y2=80,FillEnabled=true,FillColor="#FFFFFF",StrokeEnabled=false,Shadow=new(){Enabled=true,Direction=7,Distance=16,Blur=0,Opacity=100}};
 var doc=new EditorDocument{OriginalWidth=200,OriginalHeight=160,Annotations=[shape]};var rendered=Pixels(EditorRenderer.Render(image,doc));
 Check(rendered[(90*200+90)*4]==0,"Bottom middle shadow renders below rectangle");Check(rendered[(60*200+140)*4]==255,"Bottom shadow does not drift right");
 var original=Pixels(image);var framed=new EditorDocument{OriginalWidth=200,OriginalHeight=160};framed.Appearance.Padding=6;framed.Appearance.BorderWidth=6;framed.Appearance.BorderColor="#123456";var border=EditorRenderer.Render(image,framed);Check(border.PixelWidth==212&&border.PixelHeight==172,"Capture-style outer border adds exact pixels without resize");Check(Pixels(image).SequenceEqual(original),"Capture styling leaves original pixels untouched");
 var app=new App();app.InitializeComponent();var window=new MainWindow();var flags=BindingFlags.Instance|BindingFlags.NonPublic;
 object? Call(string name,params object?[] args)=>typeof(MainWindow).GetMethod(name,flags)!.Invoke(window,args);
 var session=new EditorSession(doc);typeof(MainWindow).GetField("_session",flags)!.SetValue(window,session);var surface=(EditorSurface)window.FindName("Surface");surface.Load(image,session);surface.SelectIds([shape.Id]);
 Check(window.WindowState==WindowState.Maximized&&window.Title.Contains("V1.15.1"),"Editor initializes maximized with live studio");
 Call("SetObjectPaint",null,true);Check(!surface.Selected!.FillEnabled,"Transparent fill removes existing fill");Call("SetObjectPaint","#D4E5FC",true);Check(surface.Selected!.FillEnabled&&surface.Selected.FillColor=="#D4E5FC","Palette restores chosen fill");Call("SetObjectPaint",null,false);Check(!surface.Selected!.StrokeEnabled,"Transparent border removes outline");
 ((TextBox)window.FindName("ShapeThickness")).Text="9";Call("FlushAutoProperties");Check(surface.Selected!.Stroke==9,"Thickness commits through live TextChanged without Apply");
 var style=surface.Selected!.Copy();style.Kind=AnnotationKind.RoundedRectangle;style.CornerRadius=14;Call("ChooseStyle",style);Check(surface.Selected!.Kind==AnnotationKind.RoundedRectangle&&surface.Tool==EditorTool.Shape,"Gallery switches drawing tool and preserves compatible selection");
 var directions=(System.Windows.Controls.Primitives.UniformGrid)window.FindName("ObjectShadowDirections");((Button)directions.Children[7]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(surface.Selected!.Shadow.Enabled&&surface.Selected.Shadow.Direction==7,"Nine-cell grid changes selected object shadow through actual handler");
 surface.SelectIds([]);surface.PointerDown(new(20,20));surface.PointerMove(new(45,45));surface.PointerUp(new(45,45));Check(surface.Selected?.Kind==AnnotationKind.RoundedRectangle,"Next stroke uses gallery shape instead of previous tool");
 var before=Pixels(surface.Flatten());surface.SetZoom(.25);Check(Pixels(surface.Flatten()).SequenceEqual(before),"New styling remains independent of viewport zoom");
 window.Close();Console.WriteLine($"{count} reliability Windows checks passed. Native input, install and update still require desktop acceptance.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
