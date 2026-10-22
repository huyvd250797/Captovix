using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace Captovix;
public enum RegionShape { Rectangle, Ellipse }
public static class FocusStudio
{
    public static bool IsFocus(AnnotationKind kind)=>kind is AnnotationKind.Highlight or AnnotationKind.Spotlight or AnnotationKind.Magnifier;
    public static bool Supports(AnnotationKind kind)=>IsFocus(kind)||kind is AnnotationKind.Blur or AnnotationKind.Pixelate or AnnotationKind.Cover;
    public static void Validate(Annotation a)
    {
        if(!Enum.IsDefined(a.RegionShape)||!double.IsFinite(a.Magnification)||a.Magnification<1.25||a.Magnification>8||!double.IsFinite(a.FocusOpacity)||a.FocusOpacity<0||a.FocusOpacity>100)throw new InvalidDataException("Thuộc tính vùng nhấn/che không hợp lệ.");
    }
    public static void CopyStyle(Annotation from,Annotation to){to.RegionShape=from.RegionShape;to.Magnification=from.Magnification;to.FocusOpacity=from.FocusOpacity;}
    public static PrivacySampling Sampling(EditorDocument doc)
    {
        var covers=doc.Annotations.Where(a=>a.Visible&&a.Kind==AnnotationKind.Cover).ToArray();
        var json=JsonSerializer.Serialize(new{View=doc.Viewport,Covers=covers.Select(a=>new{a.X1,a.Y1,a.X2,a.Y2,a.Rotation,a.RegionShape,a.Color})});
        return new(doc.Viewport,covers,Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))));
    }
    public static void ClipSamples(byte[] pixels,int width,int height,int left,int top,PixelRect view)
    {
        if(width<1||height<1||pixels.Length!=checked(width*height*4))throw new ArgumentException("Vùng mẫu không hợp lệ.");
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)if(left+x<view.X||left+x>=view.Right||top+y<view.Y||top+y>=view.Bottom){int i=(y*width+x)*4;pixels[i]=pixels[i+1]=pixels[i+2]=pixels[i+3]=255;}
    }
    public static AnnotationBox LensSample(Annotation a)=>new(a.Left+a.Width/2-a.Width/a.Magnification/2,a.Top+a.Height/2-a.Height/a.Magnification/2,a.Width/a.Magnification,a.Height/a.Magnification);
    public static string Review(EditorDocument doc)
    {
        var masks=doc.Annotations.Where(a=>a.IsRedaction).ToArray();int solid=masks.Count(a=>a.Visible&&a.Kind==AnnotationKind.Cover),filters=masks.Count(a=>a.Visible&&a.Kind!=AnnotationKind.Cover),hidden=masks.Count(a=>!a.Visible);
        int outside=masks.Count(a=>{var b=ObjectControl.RotatedBounds(new(a.Left,a.Top,a.Width,a.Height),a.Rotation);var v=doc.Viewport;return a.Visible&&(b.X<v.X||b.Y<v.Y||b.Right>v.Right||b.Bottom>v.Bottom);});
        return $"Che đặc: {solid} · Blur/Pixel: {filters} · Đang ẩn: {hidden}."+(hidden>0?" Vùng đang ẩn không bảo vệ bản xuất.":"")+(filters>0?" Blur/Pixel có thể còn nhận ra nội dung; dùng Che kín cho dữ liệu cần giấu.":"")+(outside>0?$" {outside} vùng vượt ảnh/crop; kiểm tra phần giao vùng xuất.":"")+" Kiểm tra ảnh xuất ở 100%; thư viện/dự án vẫn giữ ảnh gốc.";
    }
}
public sealed record FocusPreset(RegionShape Shape,double Zoom,double Opacity,AnnotationKind Kind)
{
    public static FocusPreset From(Annotation a)=>new(a.RegionShape,a.Magnification,a.FocusOpacity,a.Kind);
    public void Validate(){if(!FocusStudio.Supports(Kind))throw new InvalidDataException("Loại preset hiệu ứng không hợp lệ.");FocusStudio.Validate(new(){RegionShape=Shape,Magnification=Zoom,FocusOpacity=Opacity});}
    public void Apply(Annotation a){a.RegionShape=Shape;a.Magnification=Zoom;a.FocusOpacity=Opacity;}
}

public sealed record PrivacySampling(PixelRect Viewport,Annotation[] Covers,string Key);
