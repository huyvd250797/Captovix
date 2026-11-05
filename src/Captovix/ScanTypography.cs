using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
internal static class ScanTypography
{
    // Raster/OCR cannot prove a font identity. Fit common installed UI fonts
    // to OCR ink bounds; keep the estimate editable and explicitly labelled.
    internal static void Fit(IEnumerable<ScanElement> elements,CancellationToken token,byte[]? pixels=null,int width=0,int height=0,PixelRect region=default){
        var families=new[]{"Segoe UI","Arial","Tahoma","Calibri","Times New Roman","Georgia","Cambria","Courier New"}.Where(name=>Fonts.SystemFontFamilies.Any(f=>f.Source.Equals(name,StringComparison.OrdinalIgnoreCase))).ToArray();
        foreach(var item in elements){token.ThrowIfCancellationRequested();if(string.IsNullOrWhiteSpace(item.Text)||item.StyleVerified)continue;
            var text=item.TextRuns.FirstOrDefault();string value=text?.Text??item.Text;var box=text?.Bounds??item.OcrBounds??item.Bounds;double best=double.MaxValue;
            var local=box with{X=box.X-region.X,Y=box.Y-region.Y};byte[]? target=pixels!=null&&local.Intersect(new(0,0,width,height))==local?Mask(pixels,width,local,item.Colors.Fill):null;
            foreach(var family in families)foreach(bool bold in new[]{false,true})foreach(bool italic in new[]{false,true}){token.ThrowIfCancellationRequested();
                var face=new Typeface(new FontFamily(family),italic?FontStyles.Italic:FontStyles.Normal,bold?FontWeights.Bold:FontWeights.Normal,FontStretches.Normal);
                var measure=new FormattedText(value,CultureInfo.CurrentUICulture,FlowDirection.LeftToRight,face,20,Brushes.Black,1);
                var ink=measure.BuildGeometry(new(0,0)).Bounds;if(ink.IsEmpty||ink.Height<1||ink.Width<1)continue;
                double size=Math.Clamp(Math.Min(20*box.Height/ink.Height,20*box.Width/ink.Width),8,72),score=Math.Abs(ink.Width*size/20-box.Width)/Math.Max(1,box.Width)+Math.Abs(ink.Height*size/20-box.Height)/Math.Max(1,box.Height)+(bold?.015:0);
                if(target!=null){var geometry=measure.BuildGeometry(new(0,0));var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){dc.PushTransform(new ScaleTransform(64/ink.Width,24/ink.Height));dc.PushTransform(new TranslateTransform(-ink.X,-ink.Y));dc.DrawGeometry(Brushes.Black,null,geometry);dc.Pop();dc.Pop();}var bitmap=new RenderTargetBitmap(64,24,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var bytes=new byte[64*24*4];bitmap.CopyPixels(bytes,64*4,0);double mismatch=0;for(int k=0;k<target.Length;k++)mismatch+=Math.Abs(target[k]-bytes[k*4+3])/255d;score+=mismatch/target.Length*2;}
                if(score<best){best=score;item.FontFamily=family;item.FontSize=size;item.FontBold=bold;item.FontItalic=italic;}
            }
            item.Reason+=" Font gần giống từ khung OCR và nét ảnh; chưa xác nhận danh tính font. Mở/đóng sửa không thay pixel gốc.";
        }
    }
    static byte[] Mask(byte[] p,int width,PixelRect b,string background){var c=(Color)ColorConverter.ConvertFromString(background);var values=new byte[64*24];for(int y=0;y<24;y++)for(int x=0;x<64;x++){int sx=b.X+Math.Min(b.Width-1,x*b.Width/64),sy=b.Y+Math.Min(b.Height-1,y*b.Height/24),i=(sy*width+sx)*4;int d=Math.Max(Math.Abs(p[i]-c.B),Math.Max(Math.Abs(p[i+1]-c.G),Math.Abs(p[i+2]-c.R)));values[y*64+x]=(byte)Math.Clamp(d*3,0,255);}return values;}
}
