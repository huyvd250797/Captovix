using Captovix;
int checks=0;
void Check(bool condition,string description){checks++;if(!condition)throw new Exception("FAIL: "+description);Console.WriteLine("PASS: "+description);}
var screens=new[]{("LEFT",new PixelRect(-1920,0,1920,1080)),("MAIN",new PixelRect(0,0,2560,1440))};
var split=CaptureGeometry.Split(new(-100,100,300,500),screens);
Check(split.Count==2,"Selection crossing negative origin yields two sources");
Check(split[0].Crop==new PixelRect(1820,100,100,500),"Left monitor crop is local physical pixels");
Check(split[1].Crop==new PixelRect(0,100,200,500),"Main monitor crop starts at source origin");
Check(split[0].X==0&&split[1].X==100,"Composition keeps source positions without overlap");
Check(CaptureGeometry.Split(new(-2000,100,200,300),screens)[0].Crop==new PixelRect(0,100,120,300),"Selection outside virtual desktop is clipped");
Check(CaptureGeometry.Split(new(9000,9000,100,100),screens).Count==0,"No phantom source outside connected screens");
Check(new PixelRect(-10,-20,101,99).Even()==new PixelRect(-10,-20,100,98),"H264 even alignment preserves region origin");
Check(PixelRect.FromPoints(300,500,-100,100)==new PixelRect(-100,100,400,400),"Reverse dragging preserves coordinates");
var vertical=new[]{("TOP",new PixelRect(0,-1440,2560,1440)),("BOTTOM",new PixelRect(0,0,1920,1080))};
var v=CaptureGeometry.Split(new(500,-100,400,200),vertical);
Check(v.Count==2&&v[1].Y==100,"Vertically stacked monitors with negative Y");
Check(v[0].Crop==new PixelRect(500,1340,400,100),"Upper screen local crop uses native pixels");
var mixed=new[]{("100%",new PixelRect(0,0,1920,1080)),("150%",new PixelRect(1920,0,3840,2160))};
var m=CaptureGeometry.Split(new(1850,200,300,400),mixed);
Check(m[0].Crop.Width==70&&m[1].Crop.Width==230,"Mixed DPI geometry does not multiply by logical scale twice");
Check(m.Sum(x=>x.Crop.Width*x.Crop.Height)==300*400,"Physical pixel area is conserved");
// Photo source rectangles must never use H264 even alignment.
foreach(var size in new[]{(1467,386),(1466,387),(1467,387),(1,1),(255,192),(256,191)}){
    var region=new PixelRect(-31,-27,size.Item1,size.Item2);
    var monitors=new[]{("NEG",new PixelRect(-1920,-1080,1920,2160)),("MAIN",new PixelRect(0,-1080,2560,2160))};
    var parts=CaptureGeometry.Split(region,monitors);
    Check(parts.Sum(p=>(long)p.Crop.Width*p.Crop.Height)==(long)region.Width*region.Height,$"Photo {size}: exact odd/even area across negative origin");
    Check(parts.All(p=>p.X>=0&&p.Y>=0&&p.X+p.Crop.Width<=region.Width&&p.Y+p.Crop.Height<=region.Height),$"Photo {size}: source destinations inside requested canvas");
    Check(parts.Max(p=>p.X+p.Crop.Width)==region.Width&&parts.Max(p=>p.Y+p.Crop.Height)==region.Height,$"Photo {size}: rightmost and bottommost pixels retained");
}
Console.WriteLine($"{checks} geometry checks passed. Real Windows DPI validation remains pending.");
