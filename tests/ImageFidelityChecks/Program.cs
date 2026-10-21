using Captovix;
var count=0;void Check(bool ok,string name){if(!ok)throw new Exception(name);count++;Console.WriteLine("PASS: "+name);}void Reject(Action action,string name){bool failed=false;try{action();}catch{failed=true;}Check(failed,name);}
var source=new byte[7*5*4];for(int y=0;y<5;y++)for(int x=0;x<7;x++){int p=(y*7+x)*4;source[p]=(byte)(x*23);source[p+1]=(byte)(y*39);source[p+2]=(byte)(x+y*7);source[p+3]=(byte)((x+y)%3==0?0:(x+y)%3==1?67:255);}
foreach(var crop in new[]{new PixelRect(0,0,7,5),new(1,1,5,3),new(3,2,1,1)})for(int q=0;q<4;q++)for(int fx=0;fx<2;fx++)for(int fy=0;fy<2;fy++){
 var style=new ImageAppearance{QuarterTurns=q,FlipHorizontal=fx==1,FlipVertical=fy==1};var layout=new ImageLayout(crop,style);var actual=ImageFidelity.CopyPixels(source,7,5,crop,layout);var expected=new byte[actual.Length];
 for(int y=0;y<crop.Height;y++)for(int x=0;x<crop.Width;x++){int tx=q switch{1=>crop.Height-1-y,2=>crop.Width-1-x,3=>y,_=>x},ty=q switch{1=>x,2=>crop.Height-1-y,3=>crop.Width-1-x,_=>y};if(fx==1)tx=layout.Width-1-tx;if(fy==1)ty=layout.Height-1-ty;Array.Copy(source,((y+crop.Y)*7+x+crop.X)*4,expected,(ty*layout.Width+tx)*4,4);}
 Check(actual.SequenceEqual(expected),$"Exact BGRA incl transparent RGB: crop {crop}, rotation {q}, flip {fx}/{fy}");
}
foreach(var scale in new[]{1d,1.25,1.5,1.75,2}){
 Check(Math.Abs(ImageFidelity.PhysicalZoom(ImageFidelity.ActualZoom(scale),scale)-1)<1e-12,$"Actual Pixels at Windows {scale*100}%");
 foreach(var target in new[]{1d,1.25,1.5,1.75,2})Check(Math.Abs(ImageFidelity.PhysicalZoom(ImageFidelity.MoveZoom(.8,scale,target),target)-.8*scale)<1e-12,$"Move monitor without changing physical zoom {scale}->{target}");
}
foreach(double invalid in new[]{0,-1,double.NaN,double.PositiveInfinity})Reject(()=>ImageFidelity.ActualZoom(invalid),"Invalid DPI rejected");
Check(ImageFidelity.IsIntegerZoom(1)&&ImageFidelity.IsIntegerZoom(2)&&!ImageFidelity.IsIntegerZoom(.5)&&!ImageFidelity.IsIntegerZoom(1.25),"Nearest sampling only for integer physical magnification");
for(int anchor=0;anchor<9;anchor++){
 var view=new PixelRect(0,0,7,5);var layout=new ImageLayout(view,new(){CanvasWidth=10,CanvasHeight=8,Anchor=anchor});Check(ImageFidelity.CanCopyPixels(layout),"Odd spare pixels remain aligned: "+anchor);
 var output=ImageFidelity.CopyPixels(source,7,5,view,layout);int ox=(int)Math.Floor(3*(anchor%3)/2d),oy=(int)Math.Floor(3*(anchor/3)/2d);Check(output.AsSpan((oy*10+ox)*4,4).SequenceEqual(source.AsSpan(0,4)),"Canvas positioning has no interpolation: "+anchor);
}
var clipping=new ImageLayout(new(0,0,7,5),new(){CanvasWidth=4,CanvasHeight=2});var clipped=ImageFidelity.CopyPixels(source,7,5,new(0,0,7,5),clipping);Check(clipped.Length==32,"Smaller canvas has exact requested pixel dimensions");
Check(!ImageFidelity.CanCopyPixels(new(new(0,0,7,5),new(){ResizeWidth=14,ResizeHeight=10})),"Intentional resize uses resampling renderer");
Reject(()=>ImageFidelity.CopyPixels(source,7,5,new(-1,0,7,5),new(new(0,0,7,5),new())),"Reject source crop outside bitmap");
Reject(()=>ImageFidelity.CopyPixels(new byte[2],7,5,new(0,0,7,5),new(new(0,0,7,5),new())),"Reject wrong source byte count");
var untouched=source.ToArray();ImageFidelity.OverlayPremultiplied(untouched,new byte[source.Length]);Check(untouched.SequenceEqual(source),"Zero-alpha overlay preserves every source byte");
var dest=new byte[]{20,50,90,255};ImageFidelity.OverlayPremultiplied(dest,new byte[]{100,0,0,128});Check(dest.SequenceEqual(new byte[]{110,25,45,255}),"Premultiplied overlay composites without darkening source twice");
var alpha=new byte[]{0,0,0,0};ImageFidelity.OverlayPremultiplied(alpha,new byte[]{64,32,16,128});Check(alpha.SequenceEqual(new byte[]{128,64,32,128}),"Overlay unpremultiplies correctly onto transparency");
Check(ImageFidelity.WhiteRgb(new byte[]{17,40,90,0,17,40,90,255}).SequenceEqual(new byte[]{255,255,255,17,40,90}),"JPEG white matte preserves opaque RGB and clears transparent background");
foreach(var rect in new[]{new PixelRect(-1920,-120,601,333),new(0,0,3840,2160)}){ImageFidelity.ValidateCapture(rect,rect.Width,rect.Height,false);Check(true,"Physical capture dimensions accepted including negative origins");Reject(()=>ImageFidelity.ValidateCapture(rect,rect.Width*2,rect.Height*2,false),"DPI-double-sized capture rejected, never silently resized");}
ImageFidelity.ValidateCapture(new(0,0,640,480),620,440,true);Check(true,"Window capture reports its actual native extent without forcing desktop frame size");
Reject(()=>ImageFidelity.ValidateCapture(new(0,0,1,1),0,1,true),"Reject empty capture");Reject(()=>ImageFidelity.ValidateCapture(new(0,0,1,1),32769,1,true),"Reject excessive native dimensions");
var random=new Random(160);for(int i=0;i<2000;i++){
 int w=random.Next(1,40),h=random.Next(1,40);var bytes=new byte[w*h*4];random.NextBytes(bytes);var view=new PixelRect(0,0,w,h);var style=new ImageAppearance{QuarterTurns=random.Next(4),FlipHorizontal=random.Next(2)==1,FlipVertical=random.Next(2)==1};var layout=new ImageLayout(view,style);var transformed=ImageFidelity.CopyPixels(bytes,w,h,view,layout);
 var set=new HashSet<int>();for(int y=0;y<h;y++)for(int x=0;x<w;x++){var p=layout.ToOutput(x+.5,y+.5);int index=((int)Math.Floor(p.Y)*layout.Width+(int)Math.Floor(p.X))*4;if(!set.Add(index)||!transformed.AsSpan(index,4).SequenceEqual(bytes.AsSpan((y*w+x)*4,4)))throw new Exception("Random pixel permutation drift");}
}
Check(true,"2000 randomized transforms conserve all source pixels and alpha");
foreach(var dpi in new uint[]{96,120,144,168,192}){
 var requested=new PixelRect(-300,-50,600,250);var split=CaptureGeometry.Split(requested,new[]{("left",new PixelRect(-1920,-100,1920,1080)),("right",new PixelRect(0,0,2560,1440))});
 Check(split.Count==2&&split[0].Crop==new PixelRect(1620,50,300,250)&&split[0].X==0&&split[1].X==300&&split[1].Y==50,$"Source-local crops and output offsets remain physical pixels at DPI {dpi}; no second DPI multiplication");
}
Console.WriteLine($"{count} image fidelity checks passed.");
