using System.Windows;
using System.Windows.Media;
namespace Captovix;
internal sealed class CaptureShade:FrameworkElement
{
    public Rect? Hole {get;set;}
    public void Reveal(Rect? rect){Hole=rect;InvalidateVisual();}
    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(Brushes.Transparent,null,new Rect(0,0,ActualWidth,ActualHeight));
        var geometry=new GeometryGroup{FillRule=FillRule.EvenOdd};geometry.Children.Add(new RectangleGeometry(new Rect(0,0,ActualWidth,ActualHeight)));
        if(Hole is {} hole&&!hole.IsEmpty)geometry.Children.Add(new RectangleGeometry(hole));
        dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(125,0,0,0)),null,geometry);
    }
}
