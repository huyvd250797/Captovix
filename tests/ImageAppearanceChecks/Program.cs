using Captovix;
using System.IO;
int passed=0;
void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);passed++;Console.WriteLine("PASS: "+name);}
bool Near(double a,double b)=>Math.Abs(a-b)<.00001;
void Reject(Action action,string name){try{action();}catch(InvalidDataException){Check(true,name);return;}throw new Exception("FAIL accepted: "+name);}
var crop=new PixelRect(20,30,100,60);var style=new ImageAppearance();var l=new ImageLayout(crop,style);
Check(l.Width==100&&l.Height==60&&l.ToOutput(20,30)==new ImagePoint(0,0),"Identity layout is exact cropped source translation");
Check(!style.HasEdits,"Default appearance preserves the existing image");
foreach(int version in new[]{1,2,3,4}){var old=new EditorDocument{Version=version,OriginalWidth=200,OriginalHeight=120,SourceHash="hash"};old.Validate(200,120,"hash");Check(old.Version==6&&!old.Appearance.HasEdits,"Supported schema opens without visual styling: "+version);}
for(int turn=0;turn<4;turn++)for(int fx=0;fx<2;fx++)for(int fy=0;fy<2;fy++){
 style=new(){QuarterTurns=turn,FlipHorizontal=fx==1,FlipVertical=fy==1};l=new(crop,style);var corners=new[]{l.ToOutput(crop.X,crop.Y),l.ToOutput(crop.Right,crop.Y),l.ToOutput(crop.Right,crop.Bottom),l.ToOutput(crop.X,crop.Bottom)};
 bool correct=Near(corners.Min(p=>p.X),0)&&Near(corners.Min(p=>p.Y),0)&&Near(corners.Max(p=>p.X),l.Width)&&Near(corners.Max(p=>p.Y),l.Height);
 var mapped=l.ToOutput(43.2,68.7);var round=l.ToSource(mapped.X,mapped.Y);
 Check(correct&&Near(round.X,43.2)&&Near(round.Y,68.7),$"Rotation/flip bounds and inverse mapping: {turn}/{fx}/{fy}");
}
style=new(){QuarterTurns=1};l=new(crop,style);Check(l.Width==60&&l.Height==100&&l.ToOutput(20,30)==new ImagePoint(60,0),"Clockwise rotation swaps dimensions and maps the upper-left corner");
style=new(){QuarterTurns=1,ResizeWidth=300,ResizeHeight=200};l=new(crop,style);Check(l.ImageWidth==300&&l.ImageHeight==200,"Explicit output image size applies after orientation");
var delta=l.SourceDelta(1,0);var zero=l.ToOutput(50,50);var shifted=l.ToOutput(50+delta.X,50+delta.Y);Check(Near(shifted.X-zero.X,1)&&Near(shifted.Y-zero.Y,0),"Keyboard display direction converts to source delta after rotation/resize");
style=new(){ResizeWidth=300,ResizeHeight=200,FlipHorizontal=true};var oldLayout=new ImageLayout(crop,style);var oldPoint=oldLayout.ToOutput(42,54);style.Rotate(1);l=new(crop,style);var rotated=l.ToOutput(42,54);
Check(l.Width==200&&l.Height==300&&Near(rotated.X,200-oldPoint.Y)&&Near(rotated.Y,oldPoint.X),"Rotate acts on the visible flipped result and swaps explicit size");
style.Rotate(-1);Check(style.FlipHorizontal&&!style.FlipVertical&&style.QuarterTurns==0&&style.ResizeWidth==300,"Opposite rotation restores orientation state");
for(int anchor=0;anchor<9;anchor++){style=new(){CanvasWidth=200,CanvasHeight=140,Anchor=anchor};l=new(crop,style);var p=l.ToOutput(crop.X,crop.Y);Check(Near(p.X,100*(anchor%3)/2d)&&Near(p.Y,80*(anchor/3)/2d),"Canvas anchor placement: "+anchor);}
style=new(){CanvasWidth=50,CanvasHeight=30};l=new(crop,style);Check(l.Width==50&&l.Height==30&&l.ToOutput(20,30)==new ImagePoint(-25,-15),"Smaller canvas clips around anchor without changing source");
Check(!l.ContainsImage(crop,-1,10)&&l.ContainsImage(crop,10,10),"Canvas hit-test excludes clipped and outside regions");
style=new(){Padding=12,BorderWidth=3};l=new(crop,style);Check(l.Width==130&&l.Height==90&&l.Canvas.X==15&&l.Canvas.Y==15,"Padding and inside border contribute exact output pixels");
Check(!l.ContainsImage(crop,2,2)&&l.ContainsImage(crop,20,20),"Padding cannot receive image annotations");
style=new(){Shadow=true,ShadowBlur=16,ShadowX=8,ShadowY=-12};l=new(crop,style);Check(l.Width==132&&l.Height==92&&l.Frame.X==8&&l.Frame.Y==28,"Shadow margins handle positive and negative offsets");
style=new(){Transparent=false,BackgroundColor="#123456",BorderWidth=2,BorderColor="#ABCDEF",CanvasWidth=160,CanvasHeight=90,Padding=8,Shadow=true};
var d=new EditorDocument{OriginalWidth=200,OriginalHeight=120,SourceHash="hash",Crop=crop,Appearance=style};d.Annotations.Add(new(){Kind=AnnotationKind.Text,X1=40,Y1=50,Text="Chữ gốc"});
var before=d.Annotations[0].Copy();d.Validate(200,120,"hash");var json=d.Serialize();var reopened=EditorDocument.Deserialize(json);reopened.Validate(200,120,"hash");Check(json==reopened.Serialize()&&reopened.Appearance.BackgroundColor=="#123456","Roundtrip preserves every styling property");
Check(reopened.Annotations[0].X1==before.X1&&reopened.Crop==crop&&d.OriginalWidth==200,"Presentation leaves original coordinates and crop untouched");
var session=new EditorSession(d);session.Change(x=>x.Appearance.Rotate(1));var after=session.Document.Serialize();session.Undo();Check(session.Document.Serialize()==json,"Rotation/style recipe is one complete undo action");session.Redo();Check(session.Document.Serialize()==after,"Redo restores the complete recipe");
session.Change(x=>x.Appearance=new());Check(session.Document.Crop==crop&&session.Document.Annotations.Count==1&&!session.Document.Appearance.HasEdits,"Reset retains crop and annotations");
foreach(Action<ImageAppearance> invalid in new Action<ImageAppearance>[] {s=>s.QuarterTurns=4,s=>s.ResizeWidth=100,s=>s.CanvasHeight=40,s=>s.Padding=-1,s=>s.BorderWidth=129,s=>s.Anchor=9,s=>s.ShadowX=int.MinValue,s=>s.ShadowOpacity=101,s=>s.BackgroundColor="#12",s=>s.BorderColor="bad"}){var bad=new ImageAppearance();invalid(bad);Reject(()=>new ImageLayout(crop,bad),"Invalid recipe rejected before drawing");}
Reject(()=>new ImageLayout(crop,new(){ResizeWidth=20000,ResizeHeight=20000}),"Excessive output allocation rejected");
Reject(()=>new ImageLayout(crop,new(){ResizeWidth=32768,ResizeHeight=2,Padding=1}),"Final dimension limit includes decorations");
var random=new Random(104);
for(int i=0;i<2000;i++){var v=new PixelRect(10,20,random.Next(2,1000),random.Next(2,1000));var s=new ImageAppearance{QuarterTurns=random.Next(4),FlipHorizontal=random.Next(2)==1,FlipVertical=random.Next(2)==1,ResizeWidth=random.Next(1,1200),ResizeHeight=random.Next(1,1200),CanvasWidth=random.Next(1,1600),CanvasHeight=random.Next(1,1600),Anchor=random.Next(9),Padding=random.Next(50),BorderWidth=random.Next(10),Shadow=true,ShadowX=random.Next(-80,81),ShadowY=random.Next(-80,81),ShadowBlur=random.Next(81)};var layout=new ImageLayout(v,s);double x=v.X+random.NextDouble()*v.Width,y=v.Y+random.NextDouble()*v.Height;var screen=layout.ToOutput(x,y);var source=layout.ToSource(screen.X,screen.Y);if(!Near(x,source.X)||!Near(y,source.Y))throw new Exception("Random inverse mismatch");}
Check(true,"2000 randomized crop/orientation/scale/canvas/style mappings invert without drift");
var legacyJson=EditorDocument.Deserialize("""{"Version":3,"OriginalWidth":200,"OriginalHeight":120,"SourceHash":"hash","Annotations":[]}""");legacyJson.Validate(200,120,"hash");Check(legacyJson.Version==6&&!legacyJson.Appearance.HasEdits,"Legacy sidecar with no appearance property opens with identity layout");
var failingCrop=new EditorDocument{OriginalWidth=10000,OriginalHeight=10000,SourceHash="hash",Appearance=new(){Padding=512}};var prior=failingCrop.Serialize();Check(!failingCrop.ApplyCrop(new(0,0,9999,9999))&&failingCrop.Serialize()==prior,"Crop rejects an excessive decorated output before changing the document");
Console.WriteLine($"{passed} image appearance checks passed.");
