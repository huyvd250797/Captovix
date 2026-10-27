namespace Captovix;
public static class WorkAreaGeometry
{
    public static PixelRect Maximized(PixelRect monitor,PixelRect work)
    {
        var clipped=monitor.Intersect(work);if(clipped.IsEmpty)throw new ArgumentException("Vùng làm việc màn hình không hợp lệ.");return new(clipped.X-monitor.X,clipped.Y-monitor.Y,clipped.Width,clipped.Height);
    }
}
