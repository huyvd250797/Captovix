using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public static partial class EditorRenderer
{
    [System.ThreadStatic]private static System.Runtime.CompilerServices.ConditionalWeakTable<RecoveryPatch,RecoveryDrawing>? _recoveryDrawings;
    private sealed class RecoveryDrawing{public RecoveryDrawing(){}public string Color="",Mask="";public PixelRect Bounds;public BitmapSource? Image;}
    public static BitmapSource RecoveryImage(RecoveryPatch p)
    {
        var cached=(_recoveryDrawings??=new()).GetOrCreateValue(p);if(cached.Color==p.Background&&cached.Mask==p.Mask&&cached.Bounds==p.Bounds&&cached.Image!=null)return cached.Image;
        var bits=Convert.FromBase64String(p.Mask);int count=checked(p.Bounds.Width*p.Bounds.Height),rgb=Convert.ToInt32(p.Background[1..],16);var pixels=new byte[count*4];for(int i=0;i<count;i++)if((bits[i>>3]&(1<<(i&7)))!=0){int at=i*4;pixels[at]=(byte)rgb;pixels[at+1]=(byte)(rgb>>8);pixels[at+2]=(byte)(rgb>>16);pixels[at+3]=255;}var image=ImageTransfer.Bitmap(p.Bounds.Width,p.Bounds.Height,pixels);cached.Color=p.Background;cached.Mask=p.Mask;cached.Bounds=p.Bounds;cached.Image=image;return image;
    }
    [System.ThreadStatic]static (BitmapSource Source,EditorDocument Document,BitmapSource Result,(RecoveryPatch Patch,string Color,string Mask)[] Patches)? _recoveredSampling;
    public static BitmapSource RecoveredSampling(BitmapSource source,EditorDocument doc){if(doc.Recoveries.Count==0)return source;if(_recoveredSampling is {} cached&&ReferenceEquals(source,cached.Source)&&ReferenceEquals(doc,cached.Document)&&cached.Patches.Length==doc.Recoveries.Count&&cached.Patches.Select((x,i)=>ReferenceEquals(x.Patch,doc.Recoveries[i])&&x.Color==x.Patch.Background&&x.Mask==x.Patch.Mask).All(x=>x))return cached.Result;
        var pixels=ImageTransfer.Bgra(source);foreach(var p in doc.Recoveries){var bits=Convert.FromBase64String(p.Mask);int rgb=Convert.ToInt32(p.Background[1..],16);for(int y=0;y<p.Bounds.Height;y++)for(int x=0;x<p.Bounds.Width;x++){int bit=y*p.Bounds.Width+x;if((bits[bit>>3]&(1<<(bit&7)))==0)continue;int at=((p.Bounds.Y+y)*source.PixelWidth+p.Bounds.X+x)*4;pixels[at]=(byte)rgb;pixels[at+1]=(byte)(rgb>>8);pixels[at+2]=(byte)(rgb>>16);pixels[at+3]=255;}}var result=ImageTransfer.Bitmap(source.PixelWidth,source.PixelHeight,pixels);_recoveredSampling=(source,doc,result,doc.Recoveries.Select(p=>(p,p.Background,p.Mask)).ToArray());return result;
    }
    public static void DrawRecoveries(DrawingContext dc,EditorDocument doc){foreach(var p in doc.Recoveries)dc.DrawImage(RecoveryImage(p),new Rect(p.Bounds.X,p.Bounds.Y,p.Bounds.Width,p.Bounds.Height));}
}
public sealed partial class EditorSurface
{
    public RecoveryPatch MakeRecoveryDraft(Annotation a)=>PrepareRecovery(a,Session!.Document);
    private RecoveryPatch PrepareRecovery(Annotation a,EditorDocument before)
    {
        var r=a.Mockup!.SourceBounds;var sample=new PixelRect(r.X-4,r.Y-4,r.Width+8,r.Height+8).Intersect(new(0,0,before.OriginalWidth,before.OriginalHeight));
        var source=RasterPixels.Source(Original!,before);var pixels=ImageTransfer.Bgra(new CroppedBitmap(source,new(sample.X,sample.Y,sample.Width,sample.Height)));
        var protection=before.Annotations.Where(x=>x.Id!=a.Id&&x.Mockup!=null&&x.Mockup.SourceBounds!=r).Select(x=>x.Mockup!.SourceBounds).Where(x=>x.Intersect(r)!=r);
        var colors=MockupStudio.Colors(pixels,sample.Width,sample.Height,r with{X=r.X-sample.X,Y=r.Y-sample.Y});return BackgroundRecovery.Build(pixels,sample.Width,sample.Height,sample,r,colors.Background,a.Mockup.Type,protection);
    }
}
