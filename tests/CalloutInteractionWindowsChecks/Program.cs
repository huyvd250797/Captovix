using Captovix;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
internal static class Program
{
 static int n;static void Check(bool value,string name){if(!value)throw new Exception(name);n++;Console.WriteLine("PASS "+name);}
 static bool Near(double x,double y)=>Math.Abs(x-y)<.0001;
 [STAThread]static int Main(){try{
 var image=ImageTransfer.Bitmap(800,600,Enumerable.Repeat((byte)255,800*600*4).ToArray());
 var doc=new EditorDocument{OriginalWidth=800,OriginalHeight=600};var session=new EditorSession(doc);var surface=new EditorSurface{Tool=EditorTool.Callout,SnapEnabled=false};surface.Load(image,session);surface.SetZoom(1);int edits=0;surface.TextRequested+=()=>edits++;
 surface.PointerDown(new(80,420));Check(doc.Annotations.Count==0&&!session.InTransaction,"Mouse down does not display or start saving a Callout");surface.PointerMove(new(82,420));Check(doc.Annotations.Count==0,"Small click jitter creates no Callout");surface.PointerUp(new(82,420));Check(doc.Annotations.Count==0&&!session.CanUndo&&edits==0,"Click creates no Callout, undo or text popup");
 surface.PointerDown(new(80,420));surface.PointerMove(new(300,220));Check(doc.Annotations.Count==1&&doc.Annotations[0].TailX==80&&doc.Annotations[0].TailY==420,"Drag creates exactly one anchored Callout");surface.PointerUp(new(300,220));Check(edits==1&&surface.Tool==EditorTool.Callout,"Only completed drag requests text and retains tool");
 int before=doc.Annotations.Count;for(int i=0;i<8;i++){surface.PointerDown(new(650,450));surface.PointerUp(new(650,450));}Check(doc.Annotations.Count==before&&edits==1,"Repeated outside clicks cannot create duplicates");
 var a=new Annotation{Kind=AnnotationKind.Callout,CalloutBodyPivot=true,CalloutStyle=CalloutStyle.RoundedSpeech,X1=220,Y1=180,X2=420,Y2=300,TailX=150,TailY=340,Rotation=35,Text="Nội dung",FillEnabled=true,AutoFitText=false};var moving=new EditorSession(new(){OriginalWidth=800,OriginalHeight=600,Annotations=[a]});surface.Load(image,moving);surface.Tool=EditorTool.Select;surface.SelectIds([a.Id]);
 var original=a.Copy();var tailBefore=EditorRenderer.WorldPoint(a,new(a.TailX,a.TailY));var center=EditorRenderer.WorldPoint(a,new(a.Left+a.Width/2,a.Top+a.Height/2));surface.PointerDown(center);surface.PointerMove(new(center.X+50,center.Y+25));surface.PointerUp(new(center.X+50,center.Y+25));var tailAfter=EditorRenderer.WorldPoint(a,new(a.TailX,a.TailY));Check(Near(tailBefore.X,tailAfter.X)&&Near(tailBefore.Y,tailAfter.Y)&&Near(a.Width,original.Width),"Center handle moves rotated body and pins tail");
 moving.Undo();surface.Refresh();surface.SelectIds([a.Id]);a=moving.Document.Annotations[0];tailBefore=EditorRenderer.WorldPoint(a,new(a.TailX,a.TailY));var edge=EditorRenderer.WorldPoint(a,new(a.Left+45,a.Top+35));surface.PointerDown(edge);surface.PointerMove(new(edge.X+30,edge.Y+20));surface.PointerUp(new(edge.X+30,edge.Y+20));tailAfter=EditorRenderer.WorldPoint(a,new(a.TailX,a.TailY));Check(Near(tailAfter.X,tailBefore.X+30)&&Near(tailAfter.Y,tailBefore.Y+20),"Dragging outside center moves the entire Callout including tail");
 var exported=ImageTransfer.Bgra(surface.Flatten());surface.SelectIds([]);Check(exported.SequenceEqual(ImageTransfer.Bgra(surface.Flatten())),"Center and selection handles never enter export");
 var app=new App();app.InitializeComponent();app.ShutdownMode=ShutdownMode.OnExplicitShutdown;var window=new MainWindow();var flags=BindingFlags.Instance|BindingFlags.NonPublic;var editor=(EditorSurface)window.FindName("Surface");var inline=(TextBox)window.FindName("InlineText");var item=new Annotation{Kind=AnnotationKind.Callout,CalloutBodyPivot=true,X1=200,Y1=100,X2=420,Y2=220,TailX=170,TailY=250,AutoFitText=false,Text="Trước"};var editing=new EditorSession(new(){OriginalWidth=800,OriginalHeight=600,Annotations=[item]});typeof(MainWindow).GetField("_session",flags)!.SetValue(window,editing);editor.Load(image,editing);editor.Tool=EditorTool.Callout;editor.SelectIds([item.Id]);inline.Text="Đã lưu tiếng Việt";editing.Begin();typeof(MainWindow).GetField("_inlineId",flags)!.SetValue(window,item.Id);editor.InlineEditingId=item.Id;inline.Visibility=Visibility.Visible;
 bool consumed=(bool)typeof(MainWindow).GetMethod("FinishInlineFromOutsideClick",flags)!.Invoke(window,[new Point(9999,9999),editor])!;Check(consumed&&item.Text=="Đã lưu tiếng Việt"&&!editing.InTransaction&&editing.Document.Annotations.Count==1&&editor.InlineEditingId==null,"Outside click commits text, consumes canvas click and creates no extra Callout");Check(editor.Tool==EditorTool.Callout,"Finishing text retains the selected Callout tool");window.Close();
 Console.WriteLine($"{n} callout interaction Windows checks passed; physical mouse/IME/DPI acceptance remains separate.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
