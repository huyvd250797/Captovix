using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;

public static class ImageTransfer
{
    public static Func<bool> AuthorizeCopy {get;set;}=()=>true;
    internal static BitmapSource AsBgra(BitmapSource source)=>source.Format==PixelFormats.Bgra32
        ? source : new FormatConvertedBitmap(source,PixelFormats.Bgra32,null,0);
    internal static int StripeRows(int stride)=>Math.Max(1,(256*1024)/stride);
    public static byte[] Bgra(BitmapSource source)
    {
        var bitmap=AsBgra(source);
        var pixels=new byte[checked(source.PixelWidth*source.PixelHeight*4)];bitmap.CopyPixels(pixels,checked(source.PixelWidth*4),0);return pixels;
    }
    public static BitmapSource Bitmap(int width,int height,byte[] pixels)
    {
        var bitmap=BitmapSource.Create(width,height,96,96,PixelFormats.Bgra32,null,pixels,checked(width*4));bitmap.Freeze();return bitmap;
    }
    // Call on the owning thread before sharing decoder-backed frames. The result
    // contains only pixel data, with no BitmapDecoder/metadata dispatcher graph.
    public static BitmapSource Detach(BitmapSource source)=>Bitmap(source.PixelWidth,source.PixelHeight,Bgra(source));
    public static byte[] Png(BitmapSource source)
    {
        using var stream=new MemoryStream();
        WritePng(source,stream);
        return stream.ToArray();
    }
    private static void WritePng(BitmapSource source,Stream stream)
    {
        var normalized=Math.Abs(source.DpiX-96)<.001&&Math.Abs(source.DpiY-96)<.001?AsBgra(source):Detach(source);
        var encoder=new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(normalized,null,null,null));
        encoder.Save(stream);
    }
    internal static byte[] WhiteBgr(BitmapSource source)
    {
        var bitmap=AsBgra(source);
        int width=bitmap.PixelWidth,height=bitmap.PixelHeight,stride=checked(width*4);
        int stripe=Math.Min(height,StripeRows(stride));
        var scratch=new byte[checked(stripe*stride)];
        var output=new byte[checked(width*height*3)];
        for(int y=0;y<height;y+=stripe){
            int rows=Math.Min(stripe,height-y);
            bitmap.CopyPixels(new Int32Rect(0,y,width,rows),scratch,stride,0);
            ImageFidelity.WhiteRgb(scratch.AsSpan(0,rows*stride),output.AsSpan(y*width*3,rows*width*3));
        }
        return output;
    }
    private static bool SamePixels(BitmapSource expected,BitmapSource actual)
    {
        int width=expected.PixelWidth,height=expected.PixelHeight,stride=checked(width*4);
        int stripe=Math.Min(height,StripeRows(stride));
        var a=new byte[checked(stride*stripe)];
        var b=new byte[a.Length];
        expected=AsBgra(expected);
        actual=AsBgra(actual);
        for(int y=0;y<height;y+=stripe){
            int rows=Math.Min(stripe,height-y);
            var box=new Int32Rect(0,y,width,rows);
            expected.CopyPixels(box,a,stride,0);
            actual.CopyPixels(box,b,stride,0);
            if(!a.AsSpan(0,rows*stride).SequenceEqual(b.AsSpan(0,rows*stride)))return false;
        }
        return true;
    }
    public static void Copy(BitmapSource source)
    {
        if(!AuthorizeCopy())throw new InvalidOperationException("Copy ảnh đang khóa theo trạng thái bản quyền.");
        // PNG is the lossless, alpha-preserving representation. Bitmap remains
        // available for consumers that do not understand the registered PNG format.
        var normalized=Math.Abs(source.DpiX-96)<.001&&Math.Abs(source.DpiY-96)<.001?AsBgra(source):Detach(source);
        var data=new DataObject();data.SetImage(normalized);using var stream=new MemoryStream(Png(normalized));data.SetData("PNG",stream);Clipboard.SetDataObject(data,true);
    }
    public static void Save(BitmapSource source,string path,bool jpeg=false)
    {
        var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{
            if(jpeg){
                var pixels=WhiteBgr(source);var flat=BitmapSource.Create(source.PixelWidth,source.PixelHeight,96,96,PixelFormats.Bgr24,null,pixels,checked(source.PixelWidth*3));
                var encoder=new JpegBitmapEncoder{QualityLevel=95};encoder.Frames.Add(BitmapFrame.Create(flat));using var output=File.Create(temp);encoder.Save(output);
            }else{
                using var output=File.Create(temp);
                WritePng(source,output);
            }
            using(var check=File.OpenRead(temp)){
                var decoded=BitmapDecoder.Create(check,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];
                if(decoded.PixelWidth!=source.PixelWidth||decoded.PixelHeight!=source.PixelHeight)throw new IOException("Kích thước file xuất khác ảnh đã render.");
                if(!jpeg&&!SamePixels(source,decoded))throw new IOException("PNG khác pixel ảnh đã render. Không thay file đích.");
            }
            File.Move(temp,path,true);
        }finally{if(File.Exists(temp))File.Delete(temp);}
    }
}
