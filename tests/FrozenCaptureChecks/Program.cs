using Captovix;
int count=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);count++;Console.WriteLine("PASS "+label);}
void Reject(Action action,string label){try{action();}catch(ArgumentException){Check(true,label);return;}catch(System.IO.InvalidDataException){Check(true,label);return;}throw new Exception("Accepted "+label);}
// Source has individual colored pixels, odd dimensions and a negative virtual origin.
var bounds=new PixelRect(-17,-11,41,29);var bytes=new byte[bounds.Width*bounds.Height*4];
for(int y=0;y<bounds.Height;y++)for(int x=0;x<bounds.Width;x++){int i=(y*bounds.Width+x)*4;bytes[i]=(byte)x;bytes[i+1]=(byte)y;bytes[i+2]=(byte)(x^y);bytes[i+3]=255;}
var frame=new FrozenFrame(bounds,bytes);var original=bytes.ToArray();
var crop=frame.Crop(new(-3,-2,7,5));Check(crop.Length==7*5*4&&crop[0]==14&&crop[1]==9&&crop[^4]==20&&crop[^3]==13,"Negative-origin crop keeps exact first/last physical pixels and odd dimensions");
crop[0]=99;Check(frame.Crop(new(-3,-2,7,5))[0]==14,"Editing output never mutates snapshot");
Check(frame.Crop(bounds).SequenceEqual(original),"Whole frame roundtrip exact");
Check(frame.TryColor(-17,-11,out var b,out var g,out var r)&&b==0&&g==0&&r==0,"Magnifier top-left sample");
Check(frame.TryColor(23,17,out b,out g,out r)&&b==40&&g==28&&r==(40^28),"Magnifier bottom-right sample");
Check(!frame.TryColor(24,17,out _,out _,out _)&&!frame.TryColor(-18,-11,out _,out _,out _),"Sample outside snapshot is bounded");
Reject(()=>frame.Crop(new(-18,0,4,4)),"Crop outside snapshot rejected");Reject(()=>frame.Crop(new(0,0,0,4)),"Empty crop rejected");Reject(()=>frame.Crop(new(0,0,int.MaxValue,4)),"Overflow crop rejected");
Reject(()=>new FrozenFrame(bounds,new byte[1]),"Incorrect backing buffer rejected");Reject(()=>FrozenFrame.ValidateBounds(new(0,0,32768,32768)),"Snapshot memory cap enforced without silent downscale");
var random=new Random(130);for(int n=0;n<500;n++){int x=random.Next(0,41),y=random.Next(0,29),w=random.Next(1,42-x),h=random.Next(1,30-y);var sub=frame.Crop(new(bounds.X+x,bounds.Y+y,w,h));for(int row=0;row<h;row++)for(int col=0;col<w;col++){int i=(row*w+col)*4;if(sub[i]!=x+col||sub[i+1]!=y+row||sub[i+2]!=((x+col)^(y+row))||sub[i+3]!=255)throw new Exception("Random crop differs");}}Check(true,"500 randomized crop positions retain source pixels");
var parts=CaptureGeometry.Split(new(-5,-3,10,7),new[]{("left",new PixelRect(-17,-11,17,29)),("right",new PixelRect(0,-11,24,29))});Check(parts.Count==2&&parts.Sum(p=>p.Crop.Width*p.Crop.Height)==70,"Region crossing negative/positive monitor boundary covers every pixel once");
var root=new DirectoryInfo(AppContext.BaseDirectory);while(root!=null&&!File.Exists(Path.Combine(root.FullName,"src/Captovix/MainWindow.xaml.cs")))root=root.Parent;
var main=File.ReadAllText(Path.Combine(root!.FullName,"src/Captovix/MainWindow.xaml.cs"));var picker=File.ReadAllText(Path.Combine(root.FullName,"src/Captovix/RegionPicker.cs"));
Check(main.IndexOf("frozen=FrozenDesktop.Capture")<main.IndexOf("if(!FlushEdits())"),"Snapshot precedes editor save and selector overlay");
Check(main.Contains("Task.Run(()=>frozen.Save(region,working)")&&main.Contains("frozen?.Dispose()"),"Confirmation saves retained frame and every exit releases it");
Check(picker.Contains("session.Frozen.Background(display.Bounds)")&&picker.Contains("UpdateMagnifier")&&picker.Contains("if(e.Key==Key.M"),"Opaque frozen backdrop and hideable pixel magnifier wired");
Console.WriteLine($"{count} frozen capture checks passed. No Windows desktop/DPI acceptance is implied.");
