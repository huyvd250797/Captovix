using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;

public sealed class FrozenDesktop : IDisposable
{
    private FrozenFrame? _frame;
    private BitmapSource? _image;
    public DateTimeOffset CapturedAt {get;}
    public string Topology {get;}
    public FrozenFrame Frame=>_frame??throw new ObjectDisposedException(nameof(FrozenDesktop));
    private FrozenDesktop(FrozenFrame frame,string topology,DateTimeOffset capturedAt){_frame=frame;Topology=topology;CapturedAt=capturedAt;}
    public static FrozenDesktop Capture(IReadOnlyList<DisplayInfo> displays,bool cursor)
    {
        if(displays.Count==0)throw new InvalidOperationException("Không tìm thấy màn hình.");
        long left=displays.Min(d=>(long)d.Bounds.X),top=displays.Min(d=>(long)d.Bounds.Y),right=displays.Max(d=>(long)d.Bounds.X+d.Bounds.Width),bottom=displays.Max(d=>(long)d.Bounds.Y+d.Bounds.Height);
        var bounds=new PixelRect(checked((int)left),checked((int)top),checked((int)(right-left)),checked((int)(bottom-top)));FrozenFrame.ValidateBounds(bounds);
        // Hide Captovix first. DwmFlush waits for that composition without a fixed 220ms delay.
        int result=DwmFlush();if(result<0)Marshal.ThrowExceptionForHR(result);
        var topology=CaptureWorkflowStore.Topology(displays.Select(d=>new WorkflowDisplay(d.Device,d.Bounds,d.DpiX,d.DpiY,d.Identity)));
        var capturedAt=DateTimeOffset.UtcNow;var frame=new FrozenFrame(bounds,DesktopPhotoCapture.ReadPixels(bounds,displays,cursor));
        var fresh=Native.Displays();if(topology!=CaptureWorkflowStore.Topology(fresh.Select(d=>new WorkflowDisplay(d.Device,d.Bounds,d.DpiX,d.DpiY,d.Identity))))throw new InvalidOperationException("Màn hình/DPI thay đổi khi giữ ảnh. Hãy chụp lại.");
        return new(frame,topology,capturedAt);
    }
    public ImageBrush Background(PixelRect display)
    {
        _image??=ImageTransfer.Bitmap(Frame.Bounds.Width,Frame.Bounds.Height,Frame.Pixels);
        var crop=new CroppedBitmap(_image,new Int32Rect(display.X-Frame.Bounds.X,display.Y-Frame.Bounds.Y,display.Width,display.Height));crop.Freeze();
        var brush=new ImageBrush(crop){Stretch=Stretch.Fill,AlignmentX=AlignmentX.Left,AlignmentY=AlignmentY.Top};brush.Freeze();return brush;
    }
    public void Save(PixelRect region,string path)=>DesktopPhotoCapture.SavePixels(region.Width,region.Height,Frame.Crop(region),path);
    public void Dispose(){_frame=null;_image=null;}
    [DllImport("dwmapi.dll")]private static extern int DwmFlush();
}

internal sealed class CaptureMagnifier : FrameworkElement
{
    readonly FrozenDesktop _desktop;
    int _x,_y;
    public CaptureMagnifier(FrozenDesktop desktop){_desktop=desktop;Width=112;Height=138;IsHitTestVisible=false;SnapsToDevicePixels=true;}
    public void Update(int x,int y){if(x==_x&&y==_y&&IsVisible)return;_x=x;_y=y;InvalidateVisual();}
    protected override void OnRender(DrawingContext dc)
    {
        const double cell=7,start=3.5;var area=new Rect(3,3,106,106);
        dc.PushClip(new EllipseGeometry(area));
        for(int y=0;y<15;y++)for(int x=0;x<15;x++){
            _desktop.Frame.TryColor(_x+x-7,_y+y-7,out var b,out var g,out var r);
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(r,g,b)),null,new Rect(start+x*cell,start+y*cell,cell,cell));
        }
        var grid=new Pen(new SolidColorBrush(Color.FromArgb(100,45,45,55)),.5);
        for(int i=0;i<=15;i++){dc.DrawLine(grid,new(start+i*cell,start),new(start+i*cell,start+105));dc.DrawLine(grid,new(start,start+i*cell),new(start+105,start+i*cell));}
        dc.DrawRectangle(null,new Pen(Brushes.White,1.5),new Rect(52.5,52.5,7,7));dc.DrawRectangle(null,new Pen(Brushes.Black,.5),new Rect(52.5,52.5,7,7));
        dc.Pop();dc.DrawEllipse(null,new Pen(Brushes.Black,1.5),new(56,56),53,53);dc.DrawEllipse(null,new Pen(Brushes.White,.6),new(56,56),53,53);
        dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(25,27,35)),null,new Rect(0,114,112,22),4,4);
        var label=new FormattedText($"X {_x}  Y {_y}",System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),10,Brushes.White,VisualTreeHelper.GetDpi(this).PixelsPerDip){MaxTextWidth=106,Trimming=TextTrimming.CharacterEllipsis};dc.DrawText(label,new(3,118));
    }
}
