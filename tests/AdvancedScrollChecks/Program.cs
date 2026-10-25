using Captovix;
int n=0;void Check(bool value,string message){if(!value)throw new Exception(message);n++;Console.WriteLine("PASS: "+message);}
var random=new Random(115);var raw=new byte[200*64*4];random.NextBytes(raw);for(int i=3;i<raw.Length;i+=4)raw[i]=255;
ScrollFrame Frame(int x){var p=new byte[96*64*4];for(int y=0;y<64;y++)Buffer.BlockCopy(raw,(y*200+x)*4,p,y*96*4,96*4);return new(96,64,p);}
Check(AdvancedScroll.Transpose(AdvancedScroll.Transpose(Frame(0))).Pixels.SequenceEqual(Frame(0).Pixels),"Transpose roundtrip retains every pixel");
foreach(int shift in new[]{1,15,32,48,70}){var a=AdvancedScroll.Transpose(Frame(0));var b=AdvancedScroll.Transpose(Frame(shift));var match=ScrollStitching.Match(a,b);Check(match.Kind==ScrollMatchKind.Matched&&match.Overlap==96-shift,"Horizontal overlap exact "+shift);var pixels=ScrollStitching.Compose(64,96,new[]{match.Overlap},i=>i==0?a:b);var joined=AdvancedScroll.Transpose(new(64,96+shift,pixels));Check(joined.Width==96+shift&&joined.Height==64,"Horizontal dimensions "+shift);Check(Enumerable.Range(0,64).All(y=>joined.Pixels.AsSpan(y*joined.Width*4,joined.Width*4).SequenceEqual(raw.AsSpan(y*200*4,joined.Width*4))),"No repeated/missing columns "+shift);}
var frame=Frame(0);var trim=AdvancedScroll.Trim(frame,12,8);Check(trim.Height==44&&trim.Width==96,"Fixed header/footer excluded from matching");Check(trim.Pixels.SequenceEqual(frame.Pixels.Skip(12*96*4).Take(44*96*4)),"Trim copies exact source rows");
bool bad=false;try{AdvancedScroll.Trim(frame,40,40);}catch(System.IO.InvalidDataException){bad=true;}Check(bad,"Invalid fixed margins rejected");
Check(AdvancedScroll.Trim(frame,0,0).Pixels.SequenceEqual(frame.Pixels),"Zero margins preserve original");
Console.WriteLine($"{n} advanced scroll checks passed. Native scrolling needs Windows acceptance.");
