using Captovix;
using System.Text.Json;

var passed=0;
void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);passed++;Console.WriteLine("PASS: "+name);}
void Reject(Action action,string name){try{action();}catch{Check(true,name);return;}Check(false,name);}
bool Near(double a,double b)=>Math.Abs(a-b)<.00001;
AnnotationBox Bounds(Annotation a)=>new(a.Left,a.Top,Math.Max(1,a.Width),Math.Max(1,a.Height));
EditorDocument Doc()=>new(){OriginalWidth=200,OriginalHeight=100,SourceHash="hash"};
var legacy=EditorDocument.Deserialize("""{"Version":1,"OriginalWidth":200,"OriginalHeight":100,"SourceHash":"hash","Annotations":[{"Kind":"Arrow","X1":1,"Y1":2,"X2":10,"Y2":20}]}""");
legacy.Validate(200,100,"hash");Check(legacy.Version==2&&legacy.Annotations[0].EffectSize==24,"V1 sidecars migrate without losing existing geometry or styles");
foreach(var kind in new[]{AnnotationKind.Blur,AnnotationKind.Pixelate,AnnotationKind.Cover}){
    var doc=Doc();doc.Annotations.Add(new(){Kind=kind,X1=10,Y1=12,X2=80,Y2=40,EffectSize=40});
    var reopened=EditorDocument.Deserialize(doc.Serialize());reopened.Validate(200,100,"hash");
    Check(reopened.Version==2&&reopened.Annotations[0].Kind==kind&&reopened.Annotations[0].Copy().EffectSize==40,"Redaction roundtrip and copy: "+kind);
}
foreach(int size in new[]{0,3,65,int.MaxValue}){var d=Doc();d.Annotations.Add(new(){EffectSize=size});Reject(()=>d.Validate(200,100,"hash"),"Reject effect size "+size);}
var invalid=Doc();invalid.Annotations.Add(new(){X1=double.PositiveInfinity});Reject(()=>invalid.Validate(200,100,"hash"),"Reject nonfinite mask coordinates");
invalid=Doc();invalid.Annotations.Add(new(){X1=1000001});Reject(()=>invalid.Validate(200,100,"hash"),"Reject excessive mask coordinates before integer conversion");
invalid=Doc();invalid.Version=99;Reject(()=>invalid.Validate(200,100,"hash"),"Future sidecar versions remain protected");
foreach(var align in Enum.GetValues<AnnotationAlignment>()){
    var d=Doc();d.Crop=new(20,10,160,80);var a=new Annotation{X1=40,Y1=30,X2=60,Y2=50};d.Annotations.Add(a);
    AnnotationEditing.Align(d,new[]{a.Id},align,Bounds);
    bool ok=align switch{AnnotationAlignment.Left=>a.Left==20,AnnotationAlignment.CenterX=>a.Left==90,AnnotationAlignment.Right=>a.Left==160,AnnotationAlignment.Top=>a.Top==10,AnnotationAlignment.CenterY=>a.Top==40,_=>a.Top==70};
    Check(ok,"Single selection aligns against cropped viewport: "+align);
}
foreach(var align in Enum.GetValues<AnnotationAlignment>()){
    var d=Doc();var a=new Annotation{X1=10,Y1=10,X2=30,Y2=30};var b=new Annotation{X1=70,Y1=40,X2=100,Y2=60};d.Annotations.AddRange([a,b]);
    AnnotationEditing.Align(d,new[]{a.Id,b.Id},align,Bounds);
    bool ok=align switch{AnnotationAlignment.Left=>a.Left==10&&b.Left==10,AnnotationAlignment.CenterX=>Near(a.Left+a.Width/2,b.Left+b.Width/2),AnnotationAlignment.Right=>a.Left+a.Width==100&&b.Left+b.Width==100,AnnotationAlignment.Top=>a.Top==10&&b.Top==10,AnnotationAlignment.CenterY=>Near(a.Top+a.Height/2,b.Top+b.Height/2),_=>a.Top+a.Height==60&&b.Top+b.Height==60};
    Check(ok,"Multiple selection aligns against group bounds: "+align);
}
var session=new EditorSession(Doc());session.Document.Annotations.AddRange([new(){X1=10,Y1=10,X2=30,Y2=30},new(){X1=70,Y1=40,X2=100,Y2=60}]);
var ids=session.Document.Annotations.Select(a=>a.Id).ToArray();var before=session.Document.Serialize();
session.Change(d=>AnnotationEditing.Align(d,ids,AnnotationAlignment.Left,Bounds));var after=session.Document.Serialize();session.Undo();Check(session.Document.Serialize()==before,"Group alignment is one undo transaction");session.Redo();Check(session.Document.Serialize()==after,"Group alignment redoes exactly");
var box=new AnnotationBox(20,20,80,40);
for(int i=0;i<8;i++){
    var b=AnnotationEditing.Resize(box,i,10,10,false);Check(b.Width>=2&&b.Height>=2&&double.IsFinite(b.Width),"Eight-handle resize remains valid: "+i);
}
var resized=AnnotationEditing.Resize(box,4,140,90,true);Check(Near(resized.Width/resized.Height,2)&&resized.X==20&&resized.Y==20,"Shift resize preserves ratio and opposite anchor");
resized=AnnotationEditing.Resize(box,0,200,200,false);Check(resized.Width==2&&resized.Height==2&&resized.Right==100&&resized.Bottom==60,"Dragging across opposite handle clamps to minimum size");
var original=new Annotation{Kind=AnnotationKind.Text,X1=20,Y1=20,X2=20,Y2=20,FontSize=20};var transformed=original.Copy();
AnnotationEditing.Transform(transformed,original,box,new(40,40,160,80));Check(transformed.X1==40&&transformed.Y1==40&&transformed.FontSize==40&&original.FontSize==20,"Text resizing changes font without mutating gesture snapshot");
var movement=AnnotationEditing.ClampMove(box,new(0,0,200,100),1000,-1000);Check(movement.X==100&&movement.Y==-20,"Group movement clamps to visible image");
movement=AnnotationEditing.SnapMove(new(12,12,20,20),new(0,0,100,100),-10,0,3,false);Check(movement.X==-12,"Movement snaps to image edge in source coordinates");
movement=AnnotationEditing.SnapMove(box,new(0,0,200,100),20,4,0,true);Check(movement.X==20&&movement.Y==0,"Shift move locks the dominant axis");
movement=AnnotationEditing.ClampMove(new(-10,-10,40,40),new(0,0,100,100),0,0);Check(movement==(0d,0d),"An object partially outside a crop does not jump on contact");

byte[] Naive(byte[] pixels,int w,int h,int radius){
    var opaque=new byte[pixels.Length];for(int i=0;i<pixels.Length;i+=4){for(int c=0;c<3;c++)opaque[i+c]=(byte)((pixels[i+c]*pixels[i+3]+255*(255-pixels[i+3])+127)/255);opaque[i+3]=255;}
    var horizontal=new byte[pixels.Length];var output=new byte[pixels.Length];
    for(int y=0;y<h;y++)for(int x=0;x<w;x++)for(int c=0;c<3;c++){int sum=0,n=0;for(int xx=Math.Max(0,x-radius);xx<=Math.Min(w-1,x+radius);xx++){sum+=opaque[(y*w+xx)*4+c];n++;}horizontal[(y*w+x)*4+c]=(byte)(sum/n);}
    for(int y=0;y<h;y++)for(int x=0;x<w;x++){for(int c=0;c<3;c++){int sum=0,n=0;for(int yy=Math.Max(0,y-radius);yy<=Math.Min(h-1,y+radius);yy++){sum+=horizontal[(yy*w+x)*4+c];n++;}output[(y*w+x)*4+c]=(byte)(sum/n);}output[(y*w+x)*4+3]=255;}
    return output;
}
var random=new Random(42);
foreach(var (w,h) in new[]{(1,1),(3,2),(17,9)})foreach(int radius in new[]{4,12,64}){
    var pixels=new byte[w*h*4];random.NextBytes(pixels);var snapshot=pixels.ToArray();var output=RedactionPixels.Apply(pixels,w,h,radius,false);
    Check(output.SequenceEqual(Naive(pixels,w,h,radius)), $"Sliding blur equals independent reference at {w}x{h}, radius {radius}");
    Check(pixels.SequenceEqual(snapshot),"Blur leaves source bytes intact");
}
var pattern=new byte[7*5*4];for(int i=0;i<35;i++){pattern[i*4]=(byte)i;pattern[i*4+1]=100;pattern[i*4+2]=200;pattern[i*4+3]=255;}
var pixelated=RedactionPixels.Apply(pattern,7,5,4,true);Check(pixelated[0]==12&&pixelated[(3*7+3)*4]==12,"Pixel blocks use average colors rather than a single sampled source pixel");
Check(pixelated[(4*7+6)*4]==33&&pixelated.Where((_,i)=>i%4==3).All(a=>a==255),"Partial edge blocks are averaged and remain opaque");
var transparent=RedactionPixels.Apply(new byte[]{1,2,3,0},1,1,4,true);Check(transparent.SequenceEqual(new byte[]{255,255,255,255}),"Transparent originals cannot show through a redaction patch");
Reject(()=>RedactionPixels.Apply(new byte[4],2,2,4,true),"Reject inconsistent pixel buffer sizes");
Reject(()=>RedactionPixels.Apply(new byte[4],1,1,0,false),"Reject invalid filter radius");
// Exercise the halo strategy with multiple tiles and compare to a whole-region pass.
foreach(bool mosaic in new[]{false,true}){
    int w=67,h=45,strength=4,tile=16;var pixels=new byte[w*h*4];random.NextBytes(pixels);var tiled=new byte[pixels.Length];
    for(int y=0;y<h;y+=tile)for(int x=0;x<w;x+=tile){int tw=Math.Min(tile,w-x),th=Math.Min(tile,h-y),pad=mosaic?0:strength;
        int sx=Math.Max(0,x-pad),sy=Math.Max(0,y-pad),ex=Math.Min(w,x+tw+pad),ey=Math.Min(h,y+th+pad);int sw=ex-sx,sh=ey-sy;var input=new byte[sw*sh*4];
        for(int row=0;row<sh;row++)Buffer.BlockCopy(pixels,((sy+row)*w+sx)*4,input,row*sw*4,sw*4);
        var output=RedactionPixels.Apply(input,sw,sh,strength,mosaic);
        for(int row=0;row<th;row++)Buffer.BlockCopy(output,((y-sy+row)*sw+x-sx)*4,tiled,((y+row)*w+x)*4,tw*4);
    }
    Check(tiled.SequenceEqual(RedactionPixels.Apply(pixels,w,h,strength,mosaic)),"Tiled output has no seam and matches whole region: "+mosaic);
}
var root=Path.Combine(Path.GetTempPath(),"captovix-annotation-"+Guid.NewGuid().ToString("N"));
try{
    foreach(var preset in AnnotationPresetStore.BuiltIn)preset.Validate();Check(AnnotationPresetStore.BuiltIn.Count==6,"All six built-in presets are valid");
    Check(AnnotationPresetStore.Load(root,out var warning).Count==0&&warning==null,"Fresh install has no custom presets or warning");
    var custom=new AnnotationPreset("Mẫu của tôi","#EA5362",6,36,40);AnnotationPresetStore.Save(root,new[]{custom});
    Check(AnnotationPresetStore.Load(root,out warning).Single()==custom&&warning==null,"Custom preset persists Unicode, styles and effect strength");
    var a=new Annotation{Kind=AnnotationKind.Blur,X1=3,Y1=4,X2=30,Y2=40};custom.Apply(a);Check(a.EffectSize==40&&a.Color=="#EA5362"&&a.X1==3&&a.Y1==4,"Preset changes style without moving geometry");
    var path=Path.Combine(root,"annotation-presets.json");var saved=File.ReadAllText(path);
    Reject(()=>AnnotationPresetStore.Save(root,new[]{custom,custom}),"Duplicate custom names are rejected");
    Check(File.ReadAllText(path)==saved,"Failed preset validation leaves saved file unchanged");
    Reject(()=>AnnotationPresetStore.Save(root,new[]{custom with{Name="Hướng dẫn"}}),"Built-in preset names cannot be overwritten");
    Reject(()=>AnnotationPresetStore.Save(root,Enumerable.Range(0,21).Select(i=>custom with{Name="Preset "+i}).ToArray()),"Custom preset count is bounded");
    Reject(()=>AnnotationPresetStore.Save(root,new[]{custom with{Color="bad"}}),"Invalid preset colors are rejected before saving");
    File.WriteAllText(path,"broken");Check(AnnotationPresetStore.Load(root,out warning).Count==0&&warning!=null&&File.ReadAllText(path)=="broken","Corrupt presets are reported and never silently overwritten during load");
    AnnotationPresetStore.Save(root,Array.Empty<AnnotationPreset>());Check(AnnotationPresetStore.Load(root,out _).Count==0,"Deleting custom presets preserves a valid store");
}finally{if(Directory.Exists(root))Directory.Delete(root,true);}
Console.WriteLine($"{passed} annotation checks passed.");
