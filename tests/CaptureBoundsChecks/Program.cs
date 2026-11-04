using Captovix;
int count=0;void Check(bool value,string label){if(!value)throw new Exception(label);count++;Console.WriteLine("PASS: "+label);}
var bounds=new PixelRect(-300,-120,640,480);bool Inside(PixelRect r)=>r.IsEmpty||r.X>=bounds.X&&r.Y>=bounds.Y&&(long)r.X+r.Width<=(long)bounds.X+bounds.Width&&(long)r.Y+r.Height<=(long)bounds.Y+bounds.Height;
var rawWindow=new PixelRect(-308,-128,656,496);var selected=CaptureSelection.Clip(rawWindow,bounds);Check(selected==bounds,"Maximized raw window extending eight pixels beyond all edges clips to frame");
var bytes=new byte[bounds.Width*bounds.Height*4];for(int y=0;y<bounds.Height;y++)for(int x=0;x<bounds.Width;x++){int n=(y*bounds.Width+x)*4;bytes[n]=(byte)x;bytes[n+1]=(byte)y;bytes[n+2]=(byte)(x^y);bytes[n+3]=255;}
var frame=new FrozenFrame(bounds,bytes);Check(frame.Crop(selected).SequenceEqual(bytes),"Window suggestion can save exact retained pixels without capturing overlays again");
Check(CaptureSelection.Clip(new(bounds.Right,bounds.Y,8,8),bounds).IsEmpty,"Fully outside region rejected rather than creating invented pixels");
Check(CaptureSelection.Clip(new(bounds.Right-3,bounds.Bottom-3,100,100),bounds)==new PixelRect(bounds.Right-3,bounds.Bottom-3,3,3),"Partial final crop has correct actual dimensions");
Check(!CaptureSelection.Valid(CaptureSelection.Clip(new(bounds.Right-3,bounds.Bottom-3,100,100),bounds)),"Too-small clipped region cannot confirm");
Check(CaptureSelection.Clip(new(int.MaxValue-4,0,20,20),bounds).IsEmpty,"Intersection avoids right-edge integer overflow");
var before=new PixelRect(-180,-20,200,100);
foreach(var delta in new[]{(-999,-999),(999,999),(-999,999),(999,-999)}){
 var wanted=before with{X=before.X+delta.Item1,Y=before.Y+delta.Item2};var moved=CaptureSelection.MoveInside(wanted,bounds);Check(Inside(moved)&&moved.Width==before.Width&&moved.Height==before.Height,"Dragging to edge preserves region size");
 var gesture=new CaptureGesture(before,CaptureHandle.Move,-100,0);var output=gesture.Apply(-100+delta.Item1,delta.Item2,false,bounds);Check(output==moved,"Mouse movement uses constrained physical rectangle");
 var key=CaptureSelection.MoveInside(CaptureSelection.Nudge(before,delta.Item1,delta.Item2,false),bounds);Check(key==moved,"Keyboard movement shares boundary behavior");
}
foreach(var handle in Enum.GetValues<CaptureHandle>().Where(x=>x is not CaptureHandle.New and not CaptureHandle.Move)){
 foreach(var point in new[]{(-900,-800),(900,800),(-900,800),(900,-800)}){
  var gesture=new CaptureGesture(before,handle,before.X,before.Y);var result=gesture.Apply(point.Item1,point.Item2,false,bounds);Check(Inside(result),"Resize stays inside retained frame: "+handle);
 }
}
foreach(var point in new[]{(-900,-800),(900,800),(-900,800),(900,-800)}){
 var gesture=new CaptureGesture(default,CaptureHandle.New,-100,40);var square=gesture.Apply(point.Item1,point.Item2,true,bounds);Check(Inside(square)&&square.Width==square.Height,"Shift new-region remains square at physical frame boundary");
}
foreach(var handle in new[]{CaptureHandle.TopLeft,CaptureHandle.TopRight,CaptureHandle.BottomLeft,CaptureHandle.BottomRight}){
 var gesture=new CaptureGesture(before,handle,before.X,before.Y);var result=gesture.Apply(900,800,true,bounds);Check(Inside(result)&&Math.Abs((long)result.Width*before.Height-(long)result.Height*before.Width)<=before.Width+before.Height,"Shift corner resize retains aspect within pixel rounding: "+handle);
}
var click=new CaptureGesture(selected,CaptureHandle.Move,0,0,true);Check(click.Apply(0,0,false,bounds)==selected,"Click to lock retains already-clipped window");
var manual=click.Apply(200,200,false,bounds);Check(manual==new PixelRect(0,0,200,200),"Dragging suggestion switches to bounded manual region");
var random=new Random(1372);for(int i=0;i<20000;i++){
 var r=new PixelRect(random.Next(-2000,2000),random.Next(-2000,2000),random.Next(1,1500),random.Next(1,1500));var clipped=CaptureSelection.Clip(r,bounds);if(!Inside(clipped))throw new Exception("Random clip escaped frame");var moved=CaptureSelection.MoveInside(r,bounds);if(!Inside(moved))throw new Exception("Random movement escaped frame");
 if(!clipped.IsEmpty&&i%197==0){var data=frame.Crop(clipped);int x=clipped.X-bounds.X,y=clipped.Y-bounds.Y;Check(data[0]==(byte)x&&data[1]==(byte)y&&data.Length==clipped.Width*clipped.Height*4,"Clipped output dimensions and physical first pixel match");}
}Check(true,"20000 randomized clips and moves bounded; valid samples save retained pixels");
try{frame.Crop(rawWindow);throw new Exception("Crop guard removed");}catch(ArgumentOutOfRangeException ex){Check(ex.Message.Contains("ảnh đã giữ:")&&ex.ActualValue is PixelRect,"Final frame guard remains strict and logs actual region/frame coordinates");}
Console.WriteLine(count+" capture bounds checks passed.");
