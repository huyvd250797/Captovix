using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
namespace Captovix;
public static class RasterPixels
{
    private static readonly ConditionalWeakTable<string,BitmapSource> Cache=new();
    public static BitmapSource Decode(string data)=>Cache.GetValue(data,key=>{var expected=PixelStudio.PngSize(key);using var stream=new MemoryStream(Convert.FromBase64String(key));var image=BitmapDecoder.Create(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];if(image.PixelWidth!=expected.Width||image.PixelHeight!=expected.Height)throw new InvalidDataException("Kích thước PNG không khớp.");image.Freeze();return image;});
    public static string Encode(BitmapSource image){if((long)image.PixelWidth*image.PixelHeight>PixelStudio.MaxPixels)throw new InvalidDataException("Chỉnh pixel hỗ trợ tối đa 40 triệu pixel.");var result=Convert.ToBase64String(ImageTransfer.Png(image));PixelStudio.PngSize(result);return result;}
    public static BitmapSource Source(BitmapSource original,EditorDocument doc)=>doc.PixelBasePng==null?original:Decode(doc.PixelBasePng);
    public static void ValidateDecoded(EditorDocument doc){if(doc.PixelBasePng!=null)Decode(doc.PixelBasePng);foreach(var a in doc.Annotations)if(a.PixelPng!=null)Decode(a.PixelPng);}
}

public static partial class EditorRenderer
{
    private static string? _rasterSamplingKey;
    private static BitmapSource? _rasterSampling,_rasterSamplingOriginal;
    private static string?[] _rasterData=[];
    private static BitmapSource RasterSampling(BitmapSource source,EditorDocument doc)
    {
        var layers=doc.Annotations.Where(a=>a.Visible&&a.Kind==AnnotationKind.ImageLayer).ToArray();
        if(layers.Length==0||!doc.Annotations.Any(a=>a.Visible&&(a.IsRedaction||a.Kind==AnnotationKind.Magnifier))){_rasterSampling=null;_rasterSamplingOriginal=null;_rasterData=[];return source;}
        string key=RuntimeHelpers.GetHashCode(source)+"|"+string.Join(";",layers.Select(a=>$"{a.Id}:{a.X1:R}:{a.Y1:R}:{a.X2:R}:{a.Y2:R}:{a.Rotation:R}:{System.Text.Json.JsonSerializer.Serialize(a.ImageStyle)}:{(a.PixelPng==null?0:RuntimeHelpers.GetHashCode(a.PixelPng))}"));
        if(key==_rasterSamplingKey&&_rasterSampling!=null&&ReferenceEquals(source,_rasterSamplingOriginal)&&layers.Select(a=>a.PixelPng).SequenceEqual(_rasterData))return _rasterSampling;
        var visual=new System.Windows.Media.DrawingVisual();using(var dc=visual.RenderOpen()){dc.DrawImage(source,new System.Windows.Rect(0,0,doc.OriginalWidth,doc.OriginalHeight));foreach(var layer in layers)DrawAnnotation(dc,layer);}
        var bitmap=new RenderTargetBitmap(doc.OriginalWidth,doc.OriginalHeight,96,96,System.Windows.Media.PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();_rasterSamplingKey=key;_rasterSampling=bitmap;_rasterSamplingOriginal=source;_rasterData=layers.Select(a=>a.PixelPng).ToArray();return bitmap;
    }
}
