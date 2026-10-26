using System.Text;
namespace Captovix;
public sealed record OcrTextLine(PixelRect Bounds,string Text);
public sealed record OcrResult(string Text="",float Confidence=0,string? Error=null,OcrTextLine[]? Lines=null);
public static class OcrContract
{
    public const long MaxPixels=20_000_000;
    public static readonly string[] Languages=["vie+eng","vie","eng"];
    public static void Validate(int width,int height,string language,int mode)
    {
        if(width<2||height<2||width>32768||height>32768||(long)width*height>MaxPixels)throw new ArgumentException("OCR nhận tối đa 20 triệu pixel mỗi lần. Hãy chọn vùng nhỏ hơn.");
        if(!Languages.Contains(language)||mode is not (3 or 6 or 11))throw new ArgumentException("Ngôn ngữ hoặc bố cục OCR không hợp lệ.");
    }
    public static PixelRect Selection(double x1,double y1,double x2,double y2,int width,int height)
    {
        if(!double.IsFinite(x1)||!double.IsFinite(y1)||!double.IsFinite(x2)||!double.IsFinite(y2)||width<1||height<1)throw new ArgumentException("Vùng OCR không hợp lệ.");
        int left=(int)Math.Floor(Math.Clamp(Math.Min(x1,x2),0,width)),top=(int)Math.Floor(Math.Clamp(Math.Min(y1,y2),0,height));
        int right=(int)Math.Ceiling(Math.Clamp(Math.Max(x1,x2),0,width)),bottom=(int)Math.Ceiling(Math.Clamp(Math.Max(y1,y2),0,height));
        return new(left,top,right-left,bottom-top);
    }
    public static string Normalize(string text)=>text.Replace("\r\n","\n").Replace('\r','\n').TrimEnd('\n','\f').Normalize(NormalizationForm.FormC);
}
