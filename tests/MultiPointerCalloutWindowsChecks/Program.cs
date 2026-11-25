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
 // New perimeter gesture uses the same native input path and history as ordinary edits.
 var fresh=new Annotation{Kind=AnnotationKind.Callout,CalloutBodyPivot=true,CalloutStyle=CalloutStyle.RoundedSpeech,X1=200,Y1=180,X2=440,Y2=300,TailX=130,TailY=350,FillEnabled=true,AutoFitText=false};
 var gestureSession=new EditorSession(new(){OriginalWidth=800,OriginalHeight=600,Annotations=[fresh]});surface.Load(image,gestureSession);surface.Tool=EditorTool.Select;surface.SelectIds([fresh.Id]);
 var start=new Point(260,180);var end=new Point(290,95);var first=new AnnotationPoint(fresh.TailX,fresh.TailY);
 int notifications=0;surface.SelectionChanged+=()=>notifications++;
 surface.PointerMove(start);surface.PointerDown(start);surface.PointerUp(start);
 Check(gestureSession.Document.Annotations[0].CalloutPointerCount==1&&!gestureSession.CanUndo,"Border click creates no pointer or undo entry");
 surface.PointerDown(start);surface.PointerMove(end);Check(gestureSession.Document.Annotations[0].CalloutPointerCount==2,"Live perimeter drag previews exactly one new pointer");
 surface.PointerMove(new Point(260,220));surface.PointerUp(new Point(260,220));Check(gestureSession.Document.Annotations[0].CalloutPointerCount==1&&!gestureSession.CanUndo,"Returning inside cancels preview");
 surface.PointerDown(start);surface.PointerMove(end);surface.CancelInteraction();Check(gestureSession.Document.Annotations[0].CalloutPointerCount==1&&!gestureSession.CanUndo,"Cancel restores original pointers");
 int notified=notifications;surface.PointerDown(start);surface.PointerMove(end);surface.PointerUp(end);var result=gestureSession.Document.Annotations[0];
 Check(result.CalloutPointerCount==2&&result.CalloutPointers().First()==first&&Near(World(result,result.CalloutPointers().Last()),end),"Drag commits one joined pointer at release point, preserving original");
 Check(notifications>notified,"Selection notification updates property count");gestureSession.Undo();surface.Refresh();Check(gestureSession.Document.Annotations[0].CalloutPointerCount==1,"Single Undo removes newly added pointer");gestureSession.Redo();surface.Refresh();Check(gestureSession.Document.Annotations[0].CalloutPointerCount==2,"Redo restores newly added pointer");
 var atLimit=gestureSession.Document.Annotations[0].Copy();atLimit.SetCalloutPointerCount(8);var limitSession=new EditorSession(new(){OriginalWidth=800,OriginalHeight=600,Annotations=[atLimit]});surface.Load(image,limitSession);surface.SelectIds([atLimit.Id]);surface.PointerDown(start);surface.PointerMove(end);surface.PointerUp(end);Check(limitSession.Document.Annotations[0].CalloutPointerCount==8,"Perimeter drag respects eight pointer limit");
 var rotated=fresh.Copy();rotated.SetCalloutPointerCount(1);rotated.Rotation=30;var rotatedSession=new EditorSession(new(){OriginalWidth=800,OriginalHeight=600,Annotations=[rotated]});surface.Load(image,rotatedSession);surface.SelectIds([rotated.Id]);surface.SetZoom(.75);var rs=EditorRenderer.WorldPoint(rotated,start);var re=EditorRenderer.WorldPoint(rotated,end);var originalWorld=World(rotated,rotated.CalloutPointers().First());surface.PointerDown(rs);surface.PointerMove(re);surface.PointerUp(re);var rr=rotatedSession.Document.Annotations[0];Check(rr.CalloutPointerCount==2&&Near(World(rr,rr.CalloutPointers().First()),originalWorld)&&Near(World(rr,rr.CalloutPointers().Last()),re),"Rotated zoomed callout creates world-aligned pointer");
 Console.WriteLine($"{n} multi-pointer checks passed. Physical mouse/DPI and deployment acceptance remain required.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
