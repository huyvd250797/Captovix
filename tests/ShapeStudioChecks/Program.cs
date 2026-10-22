using Captovix;
int checks=0;
void Check(bool value,string label){if(!value)throw new Exception("FAIL: "+label);checks++;Console.WriteLine("PASS: "+label);}
void Reject(Action action,string label){try{action();}catch(Exception e)when(e is InvalidDataException or ArgumentException){Check(true,label);return;}throw new Exception("FAIL accepted: "+label);}
EditorDocument Doc()=>new(){OriginalWidth=1920,OriginalHeight=1080,SourceHash="h"};
Annotation Styled(AnnotationKind kind)=>new(){Kind=kind,X1=20,Y1=30,X2=151,Y2=132,Color="#D94155",FillColor="#B9F2E6",FillEnabled=true,StrokeEnabled=false,StrokeOpacity=62.5,FillOpacity=34,Stroke=7.5,Dash=StrokePattern.DashDot,CornerRadius=24,StarPoints=7,StarInset=.35,Tip=ArrowTip.Diamond,DoubleArrow=true,ArrowHeadSize=35};
foreach(int version in new[]{1,2,3,4}){
 var old=EditorDocument.Deserialize($$"""{"Version":{{version}},"OriginalWidth":1920,"OriginalHeight":1080,"SourceHash":"h","Annotations":[{"Kind":"Rectangle","X1":20,"Y1":30,"X2":151,"Y2":132,"Color":"#ABCDEF","Stroke":6}]}""");var id=old.Annotations[0].Id;old.Validate(1920,1080,"h");var a=old.Annotations[0];
 Check(old.Version==9&&a.Id==id&&a.X1==20&&a.X2==151&&a.Color=="#ABCDEF"&&a.Stroke==6,"Schema "+version+" migrates without moving/restyling existing objects");
 Check(!a.FillEnabled&&a.StrokeEnabled&&a.StrokeOpacity==100&&a.Dash==StrokePattern.Solid,"Legacy outline appearance is retained: "+version);
}
var future=Doc();future.Version=10;Reject(()=>future.Validate(1920,1080,"h"),"Future schema is protected");
foreach(var kind in ShapeStudio.Shapes.Append(AnnotationKind.Line).Append(AnnotationKind.Arrow)){
 var doc=Doc();var a=Styled(kind);doc.Annotations.Add(a);doc.Validate(1920,1080,"h");var json=doc.Serialize();var again=EditorDocument.Deserialize(json);again.Validate(1920,1080,"h");
 Check(again.Serialize()==json,"All shape/style fields survive reopen: "+kind);
 var copy=a.Copy();Check(System.Text.Json.JsonSerializer.Serialize(a,EditorDocument.JsonOptions)==System.Text.Json.JsonSerializer.Serialize(copy,EditorDocument.JsonOptions),"Gesture snapshot preserves complete style: "+kind);
 var id=a.Id;AnnotationEditing.Transform(a,copy,new(20,30,131,102),new(50,60,262,204));Check(a.X1==50&&a.Y1==60&&a.X2==312&&a.Y2==264&&a.Id==id&&a.StarPoints==7&&a.FillOpacity==34,"Resize retains identity and style: "+kind);
 var ids=AnnotationEditing.Duplicate(doc,[id],x=>new(x.Left,x.Top,x.Width,x.Height));var duplicate=doc.Annotations.Single(x=>ids.Contains(x.Id));Check(duplicate.Id!=id&&duplicate.Dash==a.Dash&&duplicate.FillColor==a.FillColor&&duplicate.Tip==a.Tip,"Duplicate retains style but gets independent identity: "+kind);
}
var session=new EditorSession(Doc());session.Change(d=>d.Annotations.Add(Styled(AnnotationKind.Rectangle)));session.MarkSaved();string before=session.Document.Serialize();session.Change(d=>{var a=d.Annotations[0];a.Kind=AnnotationKind.Star;a.Dash=StrokePattern.Dot;a.FillEnabled=false;});string after=session.Document.Serialize();session.Undo();Check(session.Document.Serialize()==before,"Shape and style change undo atomically");session.Redo();Check(session.Document.Serialize()==after,"Shape and style change redo atomically");
var invalidActions=new Action<Annotation>[] {a=>a.FillColor="#GGGGGG",a=>a.FillColor=null!,a=>a.StrokeOpacity=double.NaN,a=>a.StrokeOpacity=101,a=>a.FillOpacity=-1,a=>a.Dash=(StrokePattern)99,a=>a.Tip=(ArrowTip)99,a=>a.CornerRadius=double.PositiveInfinity,a=>a.CornerRadius=-1,a=>a.StarPoints=2,a=>a.StarPoints=21,a=>a.StarInset=.01,a=>a.StarInset=double.NaN,a=>a.ArrowHeadSize=161};
for(int i=0;i<invalidActions.Length;i++){var d=Doc();var a=Styled(AnnotationKind.Star);invalidActions[i](a);d.Annotations.Add(a);Reject(()=>d.Validate(1920,1080,"h"),"Invalid style rejected: "+i);}
foreach(var kind in new[]{AnnotationKind.Triangle,AnnotationKind.Diamond,AnnotationKind.Trapezoid,AnnotationKind.Pentagon,AnnotationKind.Hexagon,AnnotationKind.Star}){
 var points=ShapeStudio.Vertices(kind);Check(points.All(p=>double.IsFinite(p.X)&&double.IsFinite(p.Y)&&p.X>=0&&p.X<=1&&p.Y>=0&&p.Y<=1),"Normalized vertices remain in bounds: "+kind);
 Check(Math.Abs(points.Min(p=>p.X))<1e-9&&Math.Abs(points.Max(p=>p.X)-1)<1e-9&&Math.Abs(points.Min(p=>p.Y))<1e-9&&Math.Abs(points.Max(p=>p.Y)-1)<1e-9,"Vertices span the full resize box: "+kind);
}
for(int points=3;points<=20;points++){
 bool valid=true;foreach(double inset in new[]{.1,.45,.9}){var p=ShapeStudio.Vertices(AnnotationKind.Star,points,inset);valid&=p.Length==points*2&&p.Distinct().Count()==p.Length&&p.All(v=>v.X>=0&&v.X<=1&&v.Y>=0&&v.Y<=1);}
 Check(valid,"Star has distinct, bounded vertices at 3 inset levels: "+points);
}
foreach(var end in new[]{new AnnotationPoint(100,70),new(100,100),new(1,99),new(99,1),new(0,0),new(50,50)}){
 var start=new AnnotationPoint(50,50);var p=ShapeStudio.ConstrainedEnd(start,end,new(0,0,100,80));double dx=Math.Abs(p.X-start.X),dy=Math.Abs(p.Y-start.Y);
 Check(p.X>=0&&p.X<=100&&p.Y>=0&&p.Y<=80&&(dx<1e-8||dy<1e-8||Math.Abs(dx-dy)<1e-8),"Shift direction stays locked at viewport edge: "+end);
}
var arrow=Styled(AnnotationKind.Arrow);Check(ShapeStudio.HeadSize(arrow,10)<=4&&ShapeStudio.HeadSize(arrow,1000)==35,"Short double-arrow heads are bounded without stretching the shaft");
var root=Path.Combine(Path.GetTempPath(),"Captovix-Shapes-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
try{
 File.WriteAllText(Path.Combine(root,"annotation-presets.json"),"""[{"Name":"Mẫu cũ","Color":"#123456","Stroke":4,"FontSize":28}]""");var old=AnnotationPresetStore.Load(root,out var warning);Check(warning==null&&old.Count==1&&!old[0].FillEnabled&&old[0].StrokeEnabled,"Legacy preset file loads with appearance-preserving defaults");
 var preset=AnnotationPreset.From("Mẫu shape",Styled(AnnotationKind.Star),AnnotationKind.Star);AnnotationPresetStore.Save(root,[..old,preset]);var loaded=AnnotationPresetStore.Load(root,out warning);Check(warning==null&&loaded[1]==preset,"Full shape preset survives disk persistence");
 var a=new Annotation{Kind=AnnotationKind.Rectangle,X1=30,Y1=40,X2=110,Y2=150};var id=a.Id;preset.Apply(a);Check(a.Id==id&&a.X1==30&&a.X2==110&&a.Kind==AnnotationKind.Star&&a.FillColor=="#B9F2E6"&&a.DoubleArrow,"Preset changes shape/style while retaining object identity and bounds");
 var cover=new Annotation{Kind=AnnotationKind.Cover};preset.Apply(cover);Check(cover.Kind==AnnotationKind.Cover&&!cover.FillEnabled&&cover.StrokeOpacity==100,"Shape preset cannot convert or make a cover translucent");
 a.FillColor="#FFFFFF";Check(loaded[1].FillColor=="#B9F2E6","Editing an object never mutates stored preset");
 var saved=File.ReadAllText(Path.Combine(root,"annotation-presets.json"));Reject(()=>AnnotationPresetStore.Save(root,[preset with{FillOpacity=120}]),"Invalid preset refused before replacing disk file");Check(File.ReadAllText(Path.Combine(root,"annotation-presets.json"))==saved,"Failed preset save leaves old file intact");
 File.WriteAllText(Path.Combine(root,"annotation-presets.json"),"""[{"Name":"Khoanh lỗi đỏ","Color":"#ABCDEF","Stroke":4,"FontSize":28}]""");var collision=AnnotationPresetStore.Load(root,out warning);Check(warning==null&&collision.Single().Color=="#ABCDEF","Legacy custom name matching a new built-in retains user content");AnnotationPresetStore.Save(root,collision);Check(AnnotationPresetStore.Load(root,out warning).Single().Color=="#ABCDEF"&&warning==null,"Legacy name collision remains saveable without losing the user preset");
}finally{Directory.Delete(root,true);}
Console.WriteLine($"{checks} shape studio checks passed; WPF rendering and pointer interaction require Windows.");
