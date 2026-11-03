using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public static class BatchRendering
{
    public static BitmapSource Render(MediaItem item,BatchOptions options)
    {
        options.Validate();var source=MediaStore.LoadImage(item.FilePath);using var stream=File.OpenRead(item.FilePath);string hash=Convert.ToHexString(SHA256.HashData(stream));string path=DocumentStore.EditPath(item.Id);var doc=File.Exists(path)?EditorDocument.Deserialize(File.ReadAllText(path)):new EditorDocument{OriginalWidth=source.PixelWidth,OriginalHeight=source.PixelHeight,SourceHash=hash};doc.Validate(source.PixelWidth,source.PixelHeight,hash);RasterPixels.ValidateDecoded(doc);var layout=new ImageLayout(doc.Viewport,doc.Appearance);if((long)layout.Width*layout.Height>40_000_000)throw new InvalidDataException("Xuất hàng loạt hỗ trợ tối đa40MP/ảnh; dùng xuất riêng cho ảnh lớn hơn.");return Transform(EditorRenderer.Render(source,doc),options);
    }
    public static BitmapSource Transform(BitmapSource source,BatchOptions options)
    {
        options.Validate();var size=BatchNaming.Size(source.PixelWidth,source.PixelHeight,options.LongEdge);bool same=size==(source.PixelWidth,source.PixelHeight);bool marked=options.Watermark.Length>0||options.LogoPng!=null;if(same&&!marked)return source;
        byte[] pixels;if(same)pixels=ImageTransfer.Bgra(source);else{var baseVisual=new DrawingVisual();RenderOptions.SetBitmapScalingMode(baseVisual,BitmapScalingMode.HighQuality);using(var dc=baseVisual.RenderOpen())dc.DrawImage(source,new Rect(0,0,size.Width,size.Height));var scaled=new RenderTargetBitmap(size.Width,size.Height,96,96,PixelFormats.Pbgra32);scaled.Render(baseVisual);pixels=ImageTransfer.Bgra(scaled);}
        var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
            dc.PushOpacity(options.WatermarkOpacity);
            Point At(double w,double h){int pos=options.WatermarkPosition;return new(Math.Max(0,(size.Width-w-24)*(pos%3)/2d)+Math.Min(12,Math.Max(0,(size.Width-w)/2)),Math.Max(0,(size.Height-h-24)*(pos/3)/2d)+Math.Min(12,Math.Max(0,(size.Height-h)/2)));}
            if(options.LogoPng!=null){var logo=RasterPixels.Decode(options.LogoPng);double scale=Math.Min(1,Math.Min(size.Width*.25/logo.PixelWidth,size.Height*.25/logo.PixelHeight));double w=logo.PixelWidth*scale,h=logo.PixelHeight*scale;dc.DrawImage(logo,new Rect(At(w,h),new Size(w,h)));}
            else if(options.Watermark.Length>0){double font=options.WatermarkSize;FormattedText Text()=>new(options.Watermark,System.Globalization.CultureInfo.CurrentCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),font,Brushes.SlateGray,1);var text=Text();while(font>1&&(text.Width>size.Width*.9||text.Height>size.Height*.5)){font-=1;text=Text();}dc.DrawText(text,At(text.Width,text.Height));}
            dc.Pop();
        }
        var overlay=new RenderTargetBitmap(size.Width,size.Height,96,96,PixelFormats.Pbgra32);overlay.Render(visual);var bytes=new byte[checked(size.Width*size.Height*4)];overlay.CopyPixels(bytes,size.Width*4,0);ImageFidelity.OverlayPremultiplied(pixels,bytes);return ImageTransfer.Bitmap(size.Width,size.Height,pixels);
    }

    public static PdfImagePage Pdf(BitmapSource bitmap){var bgr=ImageFidelity.WhiteRgb(ImageTransfer.Bgra(bitmap));for(int i=0;i<bgr.Length;i+=3)(bgr[i],bgr[i+2])=(bgr[i+2],bgr[i]);return PdfImages.Encode(bitmap.PixelWidth,bitmap.PixelHeight,bgr);}
    public static Task<T> Sta<T>(Func<T> work){var done=new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);var thread=new Thread(()=>{try{done.SetResult(work());}catch(Exception ex){done.SetException(ex);}}){IsBackground=true};thread.SetApartmentState(ApartmentState.STA);thread.Start();return done.Task;}
    public static void Save(BitmapSource bitmap,string path,bool jpeg){string temp=path+"."+Guid.NewGuid().ToString("N")+".pending";try{ImageTransfer.Save(bitmap,temp,jpeg);File.Move(temp,path,false);}finally{File.Delete(temp);}}
}
