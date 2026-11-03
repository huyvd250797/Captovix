using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Captovix;
internal static class Program
{
 static int count;static void Check(bool value,string name){if(!value)throw new Exception(name);count++;Console.WriteLine("PASS "+name);}
 static byte[] Pixels(BitmapSource image){var data=new byte[image.PixelWidth*image.PixelHeight*4];image.CopyPixels(data,image.PixelWidth*4,0);return data;}
 [STAThread]static int Main(){try{
 var image=BitmapSource.Create(800,600,96,96,PixelFormats.Bgra32,null,Enumerable.Repeat((byte)255,800*600*4).ToArray(),3200);image.Freeze();
 var doc=new EditorDocument{OriginalWidth=800,OriginalHeight=600};var session=new EditorSession(doc);var surface=new EditorSurface();surface.Load(image,session);surface.Tool=EditorTool.Text;
 surface.PointerDown(new(100,100));surface.PointerMove(new(400,240));Check(surface.Selected?.Text=="Text"&&surface.IsInteracting,"Dragged text has live placeholder before release");surface.PointerUp(new(400,240));var text=surface.Selected!;Check(text.TextFrame&&text.Width==300&&text.Height==140,"Text keeps drawn width and height");Check(EditorRenderer.LocalBox(text)==new AnnotationBox(100,100,300,140),"Selection frame matches dragged box");Check(EditorRenderer.Hit(text,new(390,230),0),"Blank portion of text frame remains selectable");
 var font=text.FontSize;surface.PointerDown(new(400,170));surface.PointerMove(new(520,170));surface.PointerUp(new(520,170));Check(Math.Abs(text.Width-420)<1&&text.FontSize==font,"Side resize reflows text without changing font size");var newFrame=EditorRenderer.LocalBox(text);session.Undo();Check(session.Document.Annotations[0].Width==300,"Text resize undo restores original width");session.Redo();Check(EditorRenderer.LocalBox(session.Document.Annotations[0])==newFrame,"Text resize redo restores frame");
 var pixels=Pixels(surface.Flatten());surface.SetZoom(.35);Check(Pixels(surface.Flatten()).SequenceEqual(pixels),"Fixed text export is independent of editor zoom");
 // Exercise all stamp artwork with the actual WPF geometry/rasterizer.
 var stampMethod=typeof(MainWindow).GetMethod("StampImage",BindingFlags.Static|BindingFlags.NonPublic)!;
 foreach(var spec in StampCatalog.All){var stamp=(BitmapSource)stampMethod.Invoke(null,[spec])!;var data=Pixels(stamp);Check(data.Where((v,i)=>i%4==3).Any(v=>v>0),"Stamp has visible artwork: "+spec.Id);for(int i=0;i<256;i++)Check(data[(i*256)*4+3]==0&&data[(i*256+255)*4+3]==0&&data[i*4+3]==0&&data[(255*256+i)*4+3]==0,"Stamp stays inside raster canvas: "+spec.Id);}
 var stampPng=RasterPixels.Encode(image);surface.Tool=EditorTool.Stamp;surface.StampPng=stampPng;surface.StampName="Test";surface.SelectIds([]);int before=session.Document.Annotations.Count;surface.PointerDown(new(650,450));Check(session.Document.Annotations.Count==before+1&&surface.Selected?.Kind==AnnotationKind.ImageLayer,"Stamp is placed only after canvas click");surface.PointerDown(new(650,300));Check(session.Document.Annotations.Count==before+2,"Stamp remains armed for another click");session.Undo();Check(session.Document.Annotations.Count==before+1,"One undo removes one stamp");
 var arrow=new Annotation{Kind=AnnotationKind.Arrow,X1=200,Y1=350,X2=400,Y2=350};surface.Load(image,new EditorSession(new(){OriginalWidth=800,OriginalHeight=600,Annotations=[arrow]}));surface.SelectIds([arrow.Id]);var flags=BindingFlags.NonPublic|BindingFlags.Instance;var endpoints=(Point[])typeof(EditorSurface).GetMethod("ObjectHandles",flags)!.Invoke(surface,null)!;var rotate=(Point)typeof(EditorSurface).GetMethod("RotationHandle",flags)!.Invoke(surface,null)!;Check(endpoints.Length==2,"Arrow exposes exactly two endpoints plus separate rotation node");surface.PointerDown(rotate);surface.PointerMove(new(450,400));surface.PointerUp(new(450,400));Check(Math.Abs(arrow.Rotation)>1,"Arrow rotation node rotates actual geometry");
 Console.WriteLine($"{count} Windows refinement checks passed.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
