using Captovix;
int n=0;void Check(bool value,string name){if(!value)throw new Exception(name);Console.WriteLine("PASS: "+name);n++;}
void Reject(Action action,string name){try{action();}catch(ArgumentException){Check(true,name);return;}throw new Exception(name);}
int w=17,h=13;var source=new byte[w*h*4];for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=(y*w+x)*4;source[i]=(byte)(x*12);source[i+1]=(byte)(y*15);source[i+2]=(byte)((x+y)*8);source[i+3]=255;}
Check(ImageStudioPixels.Adjust(source,w,h,0,0,0,0,0).SequenceEqual(source),"Neutral settings preserve every byte");
var original=(byte[])source.Clone();var bright=ImageStudioPixels.Adjust(source,w,h,10,0,0,0,0);Check(bright[0]>source[0]&&source.SequenceEqual(original),"Brightness changes output without changing input");
var warm=ImageStudioPixels.Adjust(source,w,h,0,0,50,0,0);Check(warm[6]>source[6]&&warm[4]<=source[4],"Temperature changes red and blue in opposite directions");
var grey=ImageStudioPixels.Adjust(source,w,h,0,0,0,-100,0);Check(Enumerable.Range(0,w*h).All(i=>grey[i*4]==grey[i*4+1]&&grey[i*4]==grey[i*4+2]),"Desaturate produces neutral grey");
var sharp=ImageStudioPixels.Adjust(source,w,h,0,0,0,0,1);Check(sharp.Length==source.Length&&Enumerable.Range(0,w*h).All(i=>sharp[i*4+3]==source[i*4+3]),"Sharpen preserves dimensions and alpha");
var solid=Enumerable.Repeat((byte)120,w*h*4).ToArray();Check(ImageStudioPixels.Adjust(solid,w,h,0,0,0,0,2).SequenceEqual(solid),"Sharpen does not invent edges on flat pixels");
Check(ImageStudioPixels.Rotate(source,w,h,0).SequenceEqual(source),"Zero rotation preserves bytes");
var corners=new[]{(0d,0d),(w-1d,0d),(w-1d,h-1d),(0d,h-1d)};Check(ImageStudioPixels.Perspective(source,w,h,corners).SequenceEqual(source),"Identity projective warp preserves every pixel");
var crop=new[]{(2d,2d),(14d,2d),(14d,10d),(2d,10d)};var warp=ImageStudioPixels.Perspective(source,w,h,crop);Check(warp[0]==source[(2*w+2)*4]&&warp[^4]==source[(10*w+14)*4],"Projective warp maps selected corners correctly");
Reject(()=>ImageStudioPixels.Perspective(source,w,h,new[]{(0d,0d),(16d,12d),(16d,0d),(0d,12d)}),"Self-crossing perspective is rejected");
Reject(()=>ImageStudioPixels.Rotate(source,w,h,double.NaN),"Nonfinite angle rejected");
Reject(()=>ImageStudioPixels.Adjust(source,w,h,101,0,0,0,0),"Out-of-range brightness rejected");
var cloned=(byte[])source.Clone();ImageStudioPixels.CloneBrush(cloned,source,w,h,8,8,3,3,2);Check(cloned[(8*w+8)*4]==source[(3*w+3)*4]&&cloned.Take(4).SequenceEqual(source.Take(4)),"Clone paints only circular destination from stable source");
ImageStudioPixels.CloneBrush(cloned,source,w,h,8,8,0,0,2,true);Check(cloned.AsSpan((8*w+8)*4,4).SequenceEqual(source.AsSpan((8*w+8)*4,4)),"Restore brush recovers exact source bytes");
var bg=new byte[5*3*4];for(int i=0;i<15;i++){bg[i*4]=255;bg[i*4+3]=255;}for(int y=0;y<3;y++)bg[(y*5+2)*4]=0;
var cut=ImageStudioPixels.RemoveConnected(bg,5,3,0,0,0);Check(cut[3]==0&&cut[(0*5+4)*4+3]==255&&cut[(0*5+2)*4+3]==255,"Background flood follows connectivity, does not jump across subject");
Check(bg[3]==255,"Background removal preserves source");
using var cancel=new CancellationTokenSource();cancel.Cancel();bool cancelled=false;try{ImageStudioPixels.Adjust(source,w,h,1,0,0,0,0,cancel.Token);}catch(OperationCanceledException){cancelled=true;}Check(cancelled,"Adjustment cancellation is honored");
Console.WriteLine($"{n} image studio pixel checks passed; WPF and desktop runtime not exercised.");
