using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Captovix;
public sealed class RegionPicker : Window
{
    internal static CaptureChoice Choice {get;private set;}=CaptureChoice.Photo;
    private readonly Button _downArrow=new(){Content="↓",FontSize=26,Background=Brushes.Transparent,BorderThickness=new(0),Visibility=Visibility.Collapsed,ToolTip="Chụp cuộn dọc"};
    private readonly Button _rightArrow=new(){Content="→",FontSize=26,Background=Brushes.Transparent,BorderThickness=new(0),Visibility=Visibility.Collapsed,ToolTip="Chụp cuộn ngang"};
    private readonly DisplayInfo _display;
    private readonly Canvas _canvas = new();
    private readonly CaptureShade _shade=new();
    private readonly Rectangle _selection = new() {Stroke=Brushes.White,StrokeThickness=2,Fill=Brushes.Transparent,Visibility=Visibility.Collapsed};
    private readonly Action<PixelRect?> _finish;
    private readonly Action _move;
    private static Native.POINT? _start;
    private static PixelRect _current;
    private bool _edgeX,_edgeY;
    public static async Task<PixelRect?> PickAsync(IReadOnlyList<DisplayInfo> displays,CancellationToken cancellation=default,bool allowScroll=false)
    {
        cancellation.ThrowIfCancellationRequested();
        var done = new TaskCompletionSource<PixelRect?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var windows = new List<RegionPicker>(); _start = null;_current=default;Choice=CaptureChoice.Photo;
        void Finish(PixelRect? rect) { if(done.Task.IsCompleted)return; foreach(var w in windows)w.Close();done.TrySetResult(rect); }
        void Move() { foreach(var w in windows)w.Draw(); }
        foreach(var d in displays)windows.Add(new(d,Finish,Move,allowScroll));
        foreach(var w in windows)w.Show();
        using var registration=cancellation.Register(()=>System.Windows.Application.Current.Dispatcher.BeginInvoke(()=>Finish(null)));
        return await done.Task;
    }
    private RegionPicker(DisplayInfo d,Action<PixelRect?> finish,Action move,bool allowScroll)
    {
        _display=d;_finish=finish;_move=move;if(allowScroll){Panel.SetZIndex(_downArrow,5);Panel.SetZIndex(_rightArrow,5);_canvas.Children.Add(_downArrow);_canvas.Children.Add(_rightArrow);_downArrow.Click+=(_,e)=>{e.Handled=true;Choice=CaptureChoice.Vertical;_finish(_current);};_rightArrow.Click+=(_,e)=>{e.Handled=true;Choice=CaptureChoice.Horizontal;_finish(_current);};}
        WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;AllowsTransparency=true;
        Background=Brushes.Transparent;Topmost=true;ShowInTaskbar=false;Cursor=Cursors.Cross;
        var dash=new DoubleAnimation(0,-12,TimeSpan.FromSeconds(1.6)){RepeatBehavior=RepeatBehavior.Forever};_selection.StrokeDashArray=new DoubleCollection{4,2};_selection.BeginAnimation(Shape.StrokeDashOffsetProperty,dash);
        Content=_canvas;_canvas.Children.Add(_shade);_canvas.Children.Add(_selection);SizeChanged+=(_,_)=>{_shade.Width=ActualWidth;_shade.Height=ActualHeight;Draw();};
        var hint=new Border {Background=new SolidColorBrush(Color.FromRgb(25,42,57)),CornerRadius=new(8),Padding=new(16,11,16,11),Child=new TextBlock{Text="Kéo vùng hoặc cạnh để chỉnh • Enter chụp • Esc hủy",Foreground=Brushes.White,FontSize=16}};
        _canvas.Children.Add(hint);Canvas.SetLeft(hint,24);Canvas.SetTop(hint,24);
        SourceInitialized+=(_,_)=>Native.SetWindowPos(new WindowInteropHelper(this).Handle,new nint(-1),d.Bounds.X,d.Bounds.Y,d.Bounds.Width,d.Bounds.Height,0x40);
        Loaded+=(_,_)=>Native.SetWindowPos(new WindowInteropHelper(this).Handle,new nint(-1),d.Bounds.X,d.Bounds.Y,d.Bounds.Width,d.Bounds.Height,0x40);
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape)_finish(null);else if(e.Key==Key.Enter&&_current.Width>=8&&_current.Height>=8)_finish(_current);};
        var shoot=new Button{Foreground=Brushes.White,Background=new SolidColorBrush(Color.FromRgb(111,55,220)),Content=AppIcons.Content("Camera","Chụp ảnh · Enter"),Padding=new Thickness(14,9,14,9)};shoot.Click+=(_,e)=>{e.Handled=true;if(_current.Width>=8&&_current.Height>=8)_finish(_current);};_canvas.Children.Add(shoot);Canvas.SetRight(shoot,24);Canvas.SetTop(shoot,24);
        MouseLeftButtonDown+=(_,e)=>{Native.GetCursorPos(out var p);_edgeX=_edgeY=false;if(_current.Width>=8){int tolerance=14;bool left=Math.Abs(p.X-_current.X)<tolerance,right=Math.Abs(p.X-_current.Right)<tolerance,top=Math.Abs(p.Y-_current.Y)<tolerance,bottom=Math.Abs(p.Y-_current.Bottom)<tolerance;if(left||right||top||bottom){_start=new Native.POINT{X=left?_current.Right:_current.X,Y=top?_current.Bottom:_current.Y};if(!left&&!right)p.X=_current.Right;if(!top&&!bottom)p.Y=_current.Bottom;_edgeX=!left&&!right;_edgeY=!top&&!bottom;}else _start=p;}else _start=p;CaptureMouse();e.Handled=true;};
        MouseMove+=(_,_)=>{if(_start is { } s){Native.GetCursorPos(out var p);if(_edgeX)p.X=_current.Right;if(_edgeY)p.Y=_current.Bottom;_current=PixelRect.FromPoints(s.X,s.Y,p.X,p.Y);_move();}};
        MouseLeftButtonUp+=(_,_)=>{if(_start is { } s){Native.GetCursorPos(out var p);ReleaseMouseCapture();_start=null;_edgeX=_edgeY=false;_move();}};
        MouseRightButtonDown+=(_,_)=>_finish(null);
    }
    private void Draw()
    {
        var r=_current.Intersect(_display.Bounds);if(r.IsEmpty){_downArrow.Visibility=_rightArrow.Visibility=Visibility.Collapsed;_selection.Visibility=Visibility.Collapsed;_shade.Reveal(null);return;}
        var a=PointFromScreen(new(r.X,r.Y));var b=PointFromScreen(new(r.Right,r.Bottom));
        _shade.Reveal(new Rect(a,b));_selection.Visibility=Visibility.Visible;Canvas.SetLeft(_selection,a.X);Canvas.SetTop(_selection,a.Y);_selection.Width=b.X-a.X;_selection.Height=b.Y-a.Y;_downArrow.Visibility=_rightArrow.Visibility=Visibility.Visible;Canvas.SetLeft(_downArrow,(a.X+b.X)/2-20);Canvas.SetTop(_downArrow,Math.Min(ActualHeight-48,b.Y+4));Canvas.SetLeft(_rightArrow,Math.Min(ActualWidth-48,b.X+4));Canvas.SetTop(_rightArrow,(a.Y+b.Y)/2-20);
    }
}
