using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Captovix;
public static class RedactionRenderer
{
    private sealed class Cache
    {
        public BitmapSource? Converted;
        public readonly Dictionary<string,(string Key,DrawingImage Image,long Bytes)> Entries=[];
        public long Bytes;
    }
    private static readonly ConditionalWeakTable<BitmapSource,Cache> Caches=new();
    private const long CacheBudget=32*1024*1024;
    public static void Draw(DrawingContext dc,BitmapSource original,Annotation a,bool draft=false)
    {
        int left=(int)Math.Clamp(Math.Floor(a.Left),0,original.PixelWidth),top=(int)Math.Clamp(Math.Floor(a.Top),0,original.PixelHeight);
        int right=(int)Math.Clamp(Math.Ceiling(a.Left+a.Width),0,original.PixelWidth),bottom=(int)Math.Clamp(Math.Ceiling(a.Top+a.Height),0,original.PixelHeight);
        if(right<=left||bottom<=top)return;var rect=new Rect(left,top,right-left,bottom-top);
        if(a.Kind==AnnotationKind.Cover||draft){dc.DrawRectangle(draft?Brushes.DarkSlateGray:EditorRenderer.Ink(a.Color),null,rect);return;}
        var cache=Caches.GetValue(original,_=>new Cache());var key=$"{left},{top},{right},{bottom}|{a.Kind}|{a.EffectSize}";
        if(cache.Entries.TryGetValue(a.Id,out var existing)&&existing.Key==key){dc.DrawImage(existing.Image,rect);return;}
        if(cache.Entries.Remove(a.Id,out var old))cache.Bytes-=old.Bytes;
        cache.Converted??=original.Format==PixelFormats.Bgra32?original:new FormatConvertedBitmap(original,PixelFormats.Bgra32,null,0);
        var image=Build(cache.Converted,left,top,right,bottom,a.EffectSize,a.Kind==AnnotationKind.Pixelate);
        long bytes=(long)(right-left)*(bottom-top)*4;
        if(bytes<=CacheBudget){
            // Bound retained pixel data even while moving/resizing many masks.
            if(cache.Bytes+bytes>CacheBudget||cache.Entries.Count>=64){cache.Entries.Clear();cache.Bytes=0;}
            cache.Entries[a.Id]=(key,image,bytes);cache.Bytes+=bytes;
        }
        dc.DrawImage(image,rect);
    }
    private static DrawingImage Build(BitmapSource source,int left,int top,int right,int bottom,int strength,bool pixelate)
    {
        var group=new DrawingGroup();int width=right-left,height=bottom-top;
        // Pixel tiles start on a block boundary. Blur tiles include a radius-wide halo,
        // so output is identical to filtering the whole mask without allocating its full area.
        int tile=pixelate?Math.Max(strength,512/strength*strength):512;
        using(var dc=group.Open())for(int y=0;y<height;y+=tile)for(int x=0;x<width;x+=tile){
            int tw=Math.Min(tile,width-x),th=Math.Min(tile,height-y),pad=pixelate?0:strength;
            int sx=Math.Max(0,x-pad),sy=Math.Max(0,y-pad),ex=Math.Min(width,x+tw+pad),ey=Math.Min(height,y+th+pad);
            int w=ex-sx,h=ey-sy;var input=new byte[checked(w*h*4)];
            source.CopyPixels(new Int32Rect(left+sx,top+sy,w,h),input,w*4,0);
            var processed=RedactionPixels.Apply(input,w,h,strength,pixelate);var output=new byte[tw*th*4];
            for(int row=0;row<th;row++)Buffer.BlockCopy(processed,((y-sy+row)*w+x-sx)*4,output,row*tw*4,tw*4);
            var bitmap=BitmapSource.Create(tw,th,96,96,PixelFormats.Bgra32,null,output,tw*4);bitmap.Freeze();
            dc.DrawImage(bitmap,new Rect(x,y,tw,th));
        }
        group.Freeze();var image=new DrawingImage(group);image.Freeze();return image;
    }
}
