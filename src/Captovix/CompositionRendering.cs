using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public static partial class EditorRenderer
{
    private static void DrawImageLayer(DrawingContext dc,Annotation a,Rect rect)
    {
        if(a.PixelPng==null)return;if(a.ImagePlaceholder){dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(241,245,249)),new Pen(Brushes.SlateGray,1){DashStyle=DashStyles.Dash},rect);return;}var source=RasterPixels.Decode(a.PixelPng);var style=a.ImageStyle;
        if(style==null){dc.DrawImage(source,rect);return;}
        BitmapSource pixels=source;if(style.Crop is {} c){pixels=new CroppedBitmap(source,new(c.X,c.Y,c.Width,c.Height));pixels.Freeze();}
        var placement=CompositionStudio.ImagePlacement(new(rect.X,rect.Y,rect.Width,rect.Height),source.PixelWidth,source.PixelHeight,style);var destination=new Rect(placement.X,placement.Y,placement.Width,placement.Height);
        var body=new RectangleGeometry(rect,style.Radius,style.Radius);dc.PushClip(body);dc.DrawImage(pixels,destination);dc.Pop();
        if(style.Border>0)dc.DrawGeometry(null,new Pen(Ink(style.BorderColor),style.Border),body);
    }
}
