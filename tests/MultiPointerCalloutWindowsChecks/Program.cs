using Captovix;
using System.Windows;
using System.Windows.Media;
using System.Reflection;
internal static class Program
{
 static int n;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);n++;Console.WriteLine("PASS: "+name);}
 static bool Near(Point a,Point b)=>(a-b).Length<.001;
 static Point World(Annotation a,AnnotationPoint p)=>EditorRenderer.WorldPoint(a,new(p.X,p.Y));
 [STAThread] static int Main(){try{
 var a=new Annotation{Kind=AnnotationKind.Callout,CalloutBodyPivot=true,CalloutStyle=CalloutStyle.RoundedSpeech,X1=200,Y1=180,X2=400,Y2=300,TailX=150,TailY=350,FillEnabled=true,FillColor="#FFFFFF",AutoFitText=false};
 Check(a.CalloutPointerCount==1&&a.ExtraCalloutPointers==null,"Legacy/default one pointer");
 a.SetCalloutPointerCount(3);a.SetCalloutPointer(1,new(480,240));a.SetCalloutPointer(2,new(300,100));
 foreach(var style in new[]{CalloutStyle.Speech,CalloutStyle.RoundedSpeech,CalloutStyle.EllipseSpeech,CalloutStyle.PillSpeech,CalloutStyle.DiamondSpeech,CalloutStyle.HexagonSpeech}){
 a.CalloutStyle=style;var g=EditorRenderer.CalloutGeometry(a);Check(g.Bounds.Right>=479.9&&g.Bounds.Top<=100.1&&g.Bounds.Bottom>=349.9&&g.FillContains(new Point(300,240)),"Joined geometry includes all tips: "+style);
 }
 a.CalloutStyle=CalloutStyle.RoundedSpeech;
 Check(EditorRenderer.CalloutGeometry(a).FillContains(new Point(440,240)),"Second pointer is filled, not a separate line");
 a.SetCalloutPointer(1,new(510,240));Check(EditorRenderer.CalloutGeometry(a).Bounds.Right>=509.9,"Cache invalidated after moving extra tip");
 var copy=a.Copy();copy.SetCalloutPointer(1,new(490,240));Check(a.CalloutPointers().ElementAt(1).X==510,"Deep copy of pointers");
 var doc=new EditorDocument{OriginalWidth=800,OriginalHeight=600,Annotations=[a]};var round=EditorDocument.Deserialize(doc.Serialize());Check(round.Annotations[0].CalloutPointers().SequenceEqual(a.CalloutPointers()),"JSON retains all pointers");
 var single=a.Copy();single.SetCalloutPointerCount(1);Check(single.ExtraCalloutPointers==null&&single.TailX==a.TailX,"Reducing count preserves first tip");
 var limit=a.Copy();limit.SetCalloutPointerCount(100);Check(limit.CalloutPointerCount==8,"Count bounded to eight");
 var moved=a.Copy();moved.Translate(20,30);Check(moved.CalloutPointers().Zip(a.CalloutPointers()).All(pair=>pair.First.X==pair.Second.X+20&&pair.First.Y==pair.Second.Y+30),"Whole object translates all tips");
 a.Rotation=28;var old=a.Copy();var world=old.CalloutPointers().Select(t=>World(old,t)).ToArray();ObjectControl.MoveCalloutBody(a,old,40,25,EditorRenderer.LocalBox);Check(a.CalloutPointers().Select(t=>World(a,t)).Zip(world).All(pair=>Near(pair.First,pair.Second)),"Moving rotated body pins every tip");
 a=old;doc.Annotations[0]=a;a.Rotation=0;
 var image=ImageTransfer.Bitmap(800,600,Enumerable.Repeat((byte)255,800*600*4).ToArray());var session=new EditorSession(doc);var surface=new EditorSurface{Tool=EditorTool.Select,SnapEnabled=false};surface.Load(image,session);surface.SetZoom(1);surface.SelectIds([a.Id]);
 var handles=(Point[])typeof(EditorSurface).GetMethod("ObjectHandles",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(surface,null)!;Check(handles.Length==11,"Eight body handles plus three pointer handles");
 var before=a.CalloutPointers().ToArray();var tip=World(a,before[1]);var destination=tip+new Vector(45,35);surface.PointerDown(tip);surface.PointerMove(destination);surface.PointerUp(destination);
 Check(Near(World(a,a.CalloutPointers().ElementAt(1)),destination)&&a.CalloutPointers().First()==before[0]&&a.CalloutPointers().ElementAt(2)==before[2],"Drag second orange handle independently");
 session.Undo();Check(session.Document.Annotations[0].CalloutPointers().SequenceEqual(before),"Undo restores all tips");session.Redo();Check(Near(World(session.Document.Annotations[0],session.Document.Annotations[0].CalloutPointers().ElementAt(1)),destination),"Redo restores edited tip");surface.Refresh();
 var exported=ImageTransfer.Bgra(surface.Flatten());surface.SelectIds([]);Check(exported.SequenceEqual(ImageTransfer.Bgra(surface.Flatten())),"Selection handles absent from export");
 var plain=a.Copy();plain.CalloutStyle=CalloutStyle.Pill;Check(!plain.HasCalloutPointers&&EditorRenderer.CalloutGeometry(plain).Bounds.Right<=plain.Left+plain.Width+.01,"Plain styles suppress tips");
 Console.WriteLine($"{n} multi-pointer checks passed. Physical mouse/DPI and deployment acceptance remain required.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
