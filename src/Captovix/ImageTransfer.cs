using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;

public static class ImageTransfer
{
    public static byte[] Bgra(BitmapSource source)
    {
        var bitmap=source.Format==PixelFormats.Bgra32?source:new FormatConvertedBitmap(source,PixelFormats.Bgra32,null,0);
        var pixels=new byte[checked(source.PixelWidth*source.PixelHeight*4)];bitmap.CopyPixels(pixels,checked(source.PixelWidth*4),0);return pixels;
    }
    public static BitmapSource Bitmap(int width,int height,byte[] pixels)
    {
        var bitmap=BitmapSource.Create(width,height,96,96,PixelFormats.Bgra32,null,pixels,checked(width*4));bitmap.Freeze();return bitmap;
    }
    public static byte[] Png(BitmapSource source)
    {
        var normalized=source.Format==PixelFormats.Bgra32&&Math.Abs(source.DpiX-96)<.001&&Math.Abs(source.DpiY-96)<.001?source:Bitmap(source.PixelWidth,source.PixelHeight,Bgra(source));
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(normalized));using var stream=new MemoryStream();encoder.Save(stream);return stream.ToArray();
    }
    public static void Copy(BitmapSource source)
    {
        // PNG is the lossless, alpha-preserving representation. Bitmap remains
        // available for consumers that do not understand the registered PNG format.
        var normalized=Bitmap(source.PixelWidth,source.PixelHeight,Bgra(source));
        var data=new DataObject();data.SetImage(normalized);using var stream=new MemoryStream(Png(normalized));data.SetData("PNG",stream);Clipboard.SetDataObject(data,true);
    }
    public static void Save(BitmapSource source,string path,bool jpeg=false)
    {
        var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{
            if(jpeg){
                var pixels=ImageFidelity.WhiteRgb(Bgra(source));var flat=BitmapSource.Create(source.PixelWidth,source.PixelHeight,96,96,PixelFormats.Bgr24,null,pixels,checked(source.PixelWidth*3));
                var encoder=new JpegBitmapEncoder{QualityLevel=95};encoder.Frames.Add(BitmapFrame.Create(flat));using var output=File.Create(temp);encoder.Save(output);
            }else File.WriteAllBytes(temp,Png(source));
            using(var check=File.OpenRead(temp)){var decoded=BitmapDecoder.Create(check,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad).Frames[0];if(decoded.PixelWidth!=source.PixelWidth||decoded.PixelHeight!=source.PixelHeight)throw new IOException("Kích thước file xuất khác ảnh đã render.");}
            File.Move(temp,path,true);
        }finally{if(File.Exists(temp))File.Delete(temp);}
    }
}
