using System.Globalization;
using System.Windows;
using System.Windows.Media;
namespace Captovix;
internal static class ScanTypography
{
    // Raster/OCR cannot prove a font identity. Fit common installed UI fonts
    // to OCR ink bounds; keep the estimate editable and explicitly labelled.
    internal static void Fit(IEnumerable<ScanElement> elements,CancellationToken token){
        var families=new[]{"Segoe UI","Arial","Tahoma","Calibri"}.Where(name=>Fonts.SystemFontFamilies.Any(f=>f.Source.Equals(name,StringComparison.OrdinalIgnoreCase))).ToArray();
        foreach(var item in elements){token.ThrowIfCancellationRequested();if(string.IsNullOrWhiteSpace(item.Text))continue;
            var text=item.TextRuns.FirstOrDefault();string value=text?.Text??item.Text;var box=text?.Bounds??item.OcrBounds??item.Bounds;double best=double.MaxValue;
            foreach(var family in families)foreach(bool bold in new[]{false,true}){
                var face=new Typeface(new FontFamily(family),FontStyles.Normal,bold?FontWeights.Bold:FontWeights.Normal,FontStretches.Normal);
                var measure=new FormattedText(value,CultureInfo.CurrentUICulture,FlowDirection.LeftToRight,face,20,Brushes.Black,1);
                var ink=measure.BuildGeometry(new(0,0)).Bounds;if(ink.IsEmpty||ink.Height<1||ink.Width<1)continue;
                double size=Math.Clamp(Math.Min(20*box.Height/ink.Height,20*box.Width/ink.Width),8,72),score=Math.Abs(ink.Width*size/20-box.Width)/Math.Max(1,box.Width)+Math.Abs(ink.Height*size/20-box.Height)/Math.Max(1,box.Height)+(bold?.015:0);
                if(score<best){best=score;item.FontFamily=family;item.FontSize=size;item.FontBold=bold;}
            }
            item.Reason+=" Font/cỡ chữ ước lượng theo khung OCR; có thể chỉnh sau áp dụng.";
        }
    }
}
