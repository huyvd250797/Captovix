using Captovix;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
internal static class Program
{
 [STAThread]static int Main(){try{
  int n=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);n++;Console.WriteLine("PASS: "+label);}var app=new App();app.InitializeComponent();
  var bytes=Enumerable.Repeat((byte)255,320*180*4).ToArray();var source=ImageTransfer.Bitmap(320,180,bytes);var doc=new EditorDocument{OriginalWidth=320,OriginalHeight=180};var layout=new ImageLayout(doc.Viewport,doc.Appearance);var request=new AssistRequest(new(20,30,200,60),"Captovix","#FFFFFF","#202630",20,false);
  var type=typeof(MainWindow).Assembly.GetType("Captovix.AssistedEditing")!;var method=type.GetMethod("Objects",BindingFlags.Static|BindingFlags.NonPublic)!;var objects=(Annotation[])method.Invoke(null,[request,layout,doc,source])!;
  Check(objects.Length==2&&objects[1].Kind==AnnotationKind.Text,"Assist replacement creates editable background and text");var session=new EditorSession(doc);session.Change(d=>d.Annotations.AddRange(objects));Check(session.CanUndo&&doc.Annotations.Count==2,"Assist apply is a single transaction");session.Undo();Check(session.Document.Annotations.Count==0,"One Undo removes the complete assist change");session.Redo();Check(session.Document.Annotations.Count==2,"Redo restores both objects");
  var output=EditorRenderer.Render(source,session.Document);Check(output.PixelWidth==320&&output.PixelHeight==180,"Assist output geometry stays exact");var rendered=ImageTransfer.Bgra(output);bool unchanged=true;for(int y=0;y<180;y++)for(int x=0;x<320;x++)if(x<20||x>=220||y<30||y>=90)for(int c=0;c<4;c++)unchanged&=rendered[(y*320+x)*4+c]==bytes[(y*320+x)*4+c];Check(unchanged,"Outside assist region retains exact source pixels");
  var extracted=(Annotation[])method.Invoke(null,[request with{Extract=true},layout,doc,source])!;Check(extracted[1].Kind==AnnotationKind.ImageLayer&&extracted[1].PixelPng!=null,"Extracted UI region is an independent image layer");Check(ImageTransfer.Bgra(source).SequenceEqual(bytes),"Assist never mutates original bitmap");
  var shadeType=typeof(MainWindow).Assembly.GetType("Captovix.CaptureShade")!;var shade=(FrameworkElement)Activator.CreateInstance(shadeType,true)!;shade.Width=shade.Height=20;shadeType.GetMethod("Reveal")!.Invoke(shade,[new Rect(5,5,10,10)]);shade.Measure(new(20,20));shade.Arrange(new Rect(0,0,20,20));var bitmap=new RenderTargetBitmap(20,20,96,96,PixelFormats.Pbgra32);bitmap.Render(shade);var shadeBytes=ImageTransfer.Bgra(bitmap);Check(shadeBytes[(10*20+10)*4+3]==0,"Selected capture region is truly transparent");Check(shadeBytes[(1*20+1)*4+3]>100,"Outside capture region remains dimmed");
  var context=new ContextMenu();var menu=new MenuItem{Header="Copy đối tượng · Ctrl+C"};context.Items.Add(menu);typeof(MainWindow).Assembly.GetType("Captovix.AppMenus")!.GetMethod("Decorate",BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,[context]);Check(context.Style!=null&&menu.Style!=null&&menu.Icon!=null,"Context menu explicitly uses modern theme and icons");Check(menu.InputGestureText=="Ctrl+C"&&menu.Header.ToString()=="Copy đối tượng","Shortcut is separate from command label");menu.Measure(new(260,40));menu.Arrange(new Rect(0,0,260,40));menu.ApplyTemplate();Check(menu.Template.FindName("Chrome",menu) is Border,"Menu uses custom chrome instead of Windows gutter");
  Console.WriteLine($"{n} assisted Windows checks passed. Live capture/UIA/shortcut/update animation still need desktop acceptance.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
