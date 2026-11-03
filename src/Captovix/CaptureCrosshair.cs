using System.Windows;
using System.Windows.Media;
namespace Captovix;
internal sealed class CaptureCrosshair : FrameworkElement
{
    Point _point;bool _show;
    public CaptureCrosshair(){IsHitTestVisible=false;SnapsToDevicePixels=true;}
    public void Update(Point point,bool show){_point=point;_show=show;InvalidateVisual();}
    protected override void OnRender(DrawingContext dc)
    {
        if(!_show)return;var dpi=VisualTreeHelper.GetDpi(this);double unit=1/dpi.DpiScaleX;
        void Line(Point a,Point b){dc.DrawLine(new Pen(Brushes.Black,2*unit),a,b);dc.DrawLine(new Pen(Brushes.Orange,unit),a,b);}
        // Physical desktop coordinates are transformed per monitor by the owner.
        if(_point.Y>=0&&_point.Y<=ActualHeight){Line(new(0,_point.Y),new(Math.Max(0,_point.X-5),_point.Y));Line(new(Math.Min(ActualWidth,_point.X+5),_point.Y),new(ActualWidth,_point.Y));}
        if(_point.X>=0&&_point.X<=ActualWidth){Line(new(_point.X,0),new(_point.X,Math.Max(0,_point.Y-5)));Line(new(_point.X,Math.Min(ActualHeight,_point.Y+5)),new(_point.X,ActualHeight));}
    }
}
