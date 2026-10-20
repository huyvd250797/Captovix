using Captovix;

int passed=0;
void Check(bool ok,string name){if(!ok)throw new Exception("FAIL: "+name);passed++;Console.WriteLine("PASS: "+name);}
bool Near(double a,double b)=>Math.Abs(a-b)<.00001;
Check(Near(EditorViewportMath.WheelZoom(1,120),1.2),"Wheel step zooms in by 20 percent");
Check(Near(EditorViewportMath.WheelZoom(1.2,-120),1),"Opposite wheel step returns to original zoom");
Check(EditorViewportMath.WheelZoom(8,120)==8&&EditorViewportMath.WheelZoom(.02,-120)==.02,"Wheel zoom obeys upper and lower limits");
Check(EditorViewportMath.WheelZoom(1,int.MaxValue)<=Math.Pow(1.2,4)+.0001,"Extreme wheel input is bounded per event");
Check(EditorViewportMath.WheelZoom(1,0)==1,"Zero wheel delta is a no-op");
double offset=EditorViewportMath.AnchoredOffset(100,-50,150,2,100,1000);
Check(offset==250&&Near(50-offset+150*2,100),"Zoom anchor offset retains content position after scaling");
Check(EditorViewportMath.AnchoredOffset(20,40,10,.5,200,500)==0,"Zooming out clamps at beginning of scroll range");
Check(EditorViewportMath.AnchoredOffset(20,40,1000,2,0,500)==500,"Zooming in clamps at end of scroll range");
Check(EditorViewportMath.AnchoredOffset(0,200,20,1,220,0)==0,"Centered content smaller than viewport remains unscrolled");
Check(EditorViewportMath.PanOffset(100,20,50,500)==70,"Pan follows the pointer without multiplying by zoom");
Check(EditorViewportMath.PanOffset(10,0,100,500)==0&&EditorViewportMath.PanOffset(490,100,0,500)==500,"Pan is bounded on both sides");
Check(EditorViewportMath.PanOffset(0,100,0,0)==0,"Pan of fitted image has no scroll effect");
var bounds=new PixelRect(10,20,300,200);
var forward=CropGeometry.FromPoints(30.2,40.8,160.1,100.3,bounds);
Check(forward==new PixelRect(30,40,131,61),"Crop creation covers all selected physical pixels");
Check(CropGeometry.FromPoints(160.1,100.3,30.2,40.8,bounds)==forward,"Reverse crop drag has identical coordinates");
Check(CropGeometry.FromPoints(-100,-100,1000,1000,bounds)==bounds,"Crop drag is clipped to current viewport");
var fractionalSquare=CropGeometry.FromPoints(30.2,40.8,130.3,90.1,bounds,true);Check(fractionalSquare.Width==fractionalSquare.Height,"Fractional pointer positions still produce a pixel-exact square");
var square=CropGeometry.FromPoints(30,40,130,90,bounds,true);Check(square.Width==square.Height&&square.Width==50,"Shift creation produces a square");
Check(!CropGeometry.IsValid(new(10,20,1,100),bounds)&&!CropGeometry.IsValid(new(10,20,100,1),bounds),"Crop requires at least two pixels in both axes");
Check(!CropGeometry.IsValid(new(int.MaxValue,20,int.MaxValue,20),bounds),"Numeric crop bounds use overflow-safe validation");
Check(!CropGeometry.IsValid(new(9,20,100,100),bounds)&&!CropGeometry.IsValid(new(10,19,100,100),bounds),"Numeric crop rejects coordinates outside current viewport");
Check(!CropGeometry.IsValid(new(10,20,301,200),bounds),"Numeric crop never silently accepts too-large dimensions");
var original=new PixelRect(50,60,100,60);
Check(CropGeometry.Move(original,-1000,1000,bounds)==new PixelRect(10,160,100,60),"Moving crop clamps to bounds and preserves its size");
Check(CropGeometry.Move(original,.6,-.6,bounds)==new PixelRect(51,59,100,60),"Crop movement snaps to whole source pixels");
for(int i=0;i<8;i++){
    var crop=CropGeometry.Resize(original,i,25,30,bounds,false);
    Check(CropGeometry.IsValid(crop,bounds),"Crop handle remains valid: "+i);
}
for(int i=0;i<8;i++){
    var crop=CropGeometry.Resize(original,i,10000,-10000,bounds,false);
    Check(CropGeometry.IsValid(crop,bounds),"Crop handle clamps extreme pointer positions: "+i);
}
foreach(int i in new[]{0,2,4,6}){
    var crop=CropGeometry.Resize(original,i,10000,10000,bounds,true);
    Check(CropGeometry.IsValid(crop,bounds)&&Math.Abs(crop.Width*60d/100-crop.Height)<=1,"Shift corner resize keeps ratio within one pixel inside viewport: "+i);
}
// Randomized invariants exercise all handles near corners and minimum size.
var random=new Random(120);bool allValid=true;
for(int n=0;n<4000;n++){
    var current=new PixelRect(random.Next(10,250),random.Next(20,170),random.Next(2,55),random.Next(2,45));
    var crop=CropGeometry.Resize(current,n%8,random.Next(-100,500),random.Next(-100,500),bounds,n%2==0);
    allValid&=CropGeometry.IsValid(crop,bounds);
}
Check(allValid,"4000 randomized crop resize cases remain inside the viewport with positive size");
var doc=new EditorDocument{OriginalWidth=400,OriginalHeight=300};doc.Annotations.Add(new(){Kind=AnnotationKind.Rectangle,X1=30,Y1=40,X2=100,Y2=150});
var session=new EditorSession(doc);var before=doc.Serialize();var pending=new PixelRect(20,20,100,120);
Check(doc.Serialize()==before&&!session.CanUndo,"Preparing a crop does not modify the document or undo history");
session.Change(d=>d.ApplyCrop(pending));Check(session.Document.Crop==pending&&session.Document.Annotations[0].X1==30,"Applying crop preserves original annotation coordinate system");
session.Undo();Check(session.Document.Serialize()==before,"Applied crop is a single undoable transaction");
session.Redo();Check(session.Document.Crop==pending,"Crop redo restores the exact pixel rectangle");
Console.WriteLine($"{passed} viewport/crop checks passed.");
