using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public sealed class ElementScanSurface:FrameworkElement
{
    public BitmapSource? Image {get;set;}
    public PixelRect Region {get;set;}
    public IReadOnlyList<ScanElement> Elements {get;set;}=[];
    public ScanElement? Selected {get;set;}
    public bool AddMode {get;set;}
    public string? FixedLabel {get;set;}
    public bool Ready {get;set;}
    public double Zoom {get;private set;}=1;
    public Func<bool>? BeforeEdit {get;set;}
    public event Action<ScanElement?>? SelectRequested;
    public event Action<PixelRect>? AddRequested;
    public event Action<bool>? Edited;
    private Point _start;private PixelRect? _before;private int _corner=-1;private Rect? _draft;
    public void SetZoom(double zoom){Zoom=Math.Clamp(zoom,.02,4);Width=Math.Max(1,Region.Width*Zoom);Height=Math.Max(1,Region.Height*Zoom);InvalidateVisual();}
    protected override void OnRender(DrawingContext dc){base.OnRender(dc);dc.DrawRectangle(Brushes.DimGray,null,new Rect(0,0,ActualWidth,ActualHeight));if(Image==null)return;dc.DrawImage(Image,new Rect(0,0,Region.Width*Zoom,Region.Height*Zoom));
        int index=0;foreach(var e in Elements){index++;var r=Rect(e.Bounds);var color=e.Include?Colors.LimeGreen:e.Uncertain?Colors.DarkOrange:Colors.DodgerBlue;var brush=new SolidColorBrush(color);dc.DrawRectangle(null,new Pen(brush,ReferenceEquals(e,Selected)?3:1.5),r);if(r.Width>34&&r.Height>12){var label=new FormattedText(index+" · "+(FixedLabel??ElementScan.Label(e.Kind)),CultureInfo.CurrentCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),11,Brushes.White,1);var back=new Rect(r.X,Math.Max(0,r.Y-17),Math.Min(160,Math.Max(32,label.Width+6)),17);dc.DrawRectangle(brush,null,back);dc.PushClip(new RectangleGeometry(back));dc.DrawText(label,new(back.X+3,back.Y));dc.Pop();}if(ReferenceEquals(e,Selected))foreach(var p in Corners(r))dc.DrawRectangle(Brushes.White,new Pen(Brushes.DodgerBlue,1),new Rect(p.X-4,p.Y-4,8,8));}
        if(_draft is {} draft)dc.DrawRectangle(null,new Pen(Brushes.DodgerBlue,2),new Rect(draft.X*Zoom,draft.Y*Zoom,draft.Width*Zoom,draft.Height*Zoom));
    }
    private Rect Rect(PixelRect b)=>new((b.X-Region.X)*Zoom,(b.Y-Region.Y)*Zoom,b.Width*Zoom,b.Height*Zoom);
    private static Point[] Corners(Rect r)=>[r.TopLeft,r.TopRight,r.BottomRight,r.BottomLeft];
    private Point Local(MouseEventArgs e){var p=e.GetPosition(this);return new(Math.Clamp(p.X/Zoom,0,Region.Width),Math.Clamp(p.Y/Zoom,0,Region.Height));}
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e){base.OnMouseLeftButtonDown(e);if(!Ready||BeforeEdit?.Invoke()==false)return;Focus();_start=Local(e);_corner=-1;_before=null;
        if(AddMode){_draft=new(_start,_start);CaptureMouse();e.Handled=true;return;}
        if(Selected!=null){var corners=Corners(Rect(Selected.Bounds));var at=e.GetPosition(this);for(int n=0;n<4;n++)if((corners[n]-at).Length<=9){_corner=n;_before=Selected.Bounds;CaptureMouse();e.Handled=true;return;}}
        var hit=Elements.Where(x=>Rect(x.Bounds).Contains(e.GetPosition(this))).OrderBy(x=>(long)x.Bounds.Width*x.Bounds.Height).FirstOrDefault();SelectRequested?.Invoke(hit);if(hit!=null&&ReferenceEquals(hit,Selected)){_before=hit.Bounds;CaptureMouse();}e.Handled=true;
    }
    protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(!IsMouseCaptured||!Ready)return;var p=Local(e);if(_draft!=null){_draft=new(_start,p);InvalidateVisual();return;}if(_before is not {} b||Selected==null)return;
        int dx=(int)Math.Round(p.X-_start.X),dy=(int)Math.Round(p.Y-_start.Y);PixelRect next;
        if(_corner<0)next=b with{X=Math.Clamp(b.X+dx,Region.X,Region.Right-b.Width),Y=Math.Clamp(b.Y+dy,Region.Y,Region.Bottom-b.Height)};
        else{int left=b.X,top=b.Y,right=b.Right,bottom=b.Bottom;if(_corner is 0 or 3)left=Math.Clamp(b.X+dx,Region.X,right-8);else right=Math.Clamp(b.Right+dx,left+8,Region.Right);if(_corner is 0 or 1)top=Math.Clamp(b.Y+dy,Region.Y,bottom-8);else bottom=Math.Clamp(b.Bottom+dy,top+8,Region.Bottom);next=new(left,top,right-left,bottom-top);}
        Selected.Bounds=next;Edited?.Invoke(false);InvalidateVisual();
    }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e){base.OnMouseLeftButtonUp(e);if(!IsMouseCaptured)return;ReleaseMouseCapture();if(_draft is {} d){_draft=null;if(d.Width>=8&&d.Height>=8)AddRequested?.Invoke(new((int)Math.Floor(d.X)+Region.X,(int)Math.Floor(d.Y)+Region.Y,(int)Math.Floor(d.Width),(int)Math.Floor(d.Height)));}else if(_before!=null)Edited?.Invoke(true);_before=null;InvalidateVisual();e.Handled=true;}
    protected override void OnPreviewKeyDown(KeyEventArgs e){if(e.Key==Key.Escape&&IsMouseCaptured){if(_before is {} before&&Selected!=null)Selected.Bounds=before;_before=null;_draft=null;ReleaseMouseCapture();Edited?.Invoke(true);InvalidateVisual();e.Handled=true;}base.OnPreviewKeyDown(e);}
}
