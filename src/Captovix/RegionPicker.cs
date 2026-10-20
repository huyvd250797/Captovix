using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Captovix;
public sealed class RegionPicker : Window
{
    private readonly DisplayInfo _display;
    private readonly Canvas _canvas = new();
    private readonly Rectangle _selection = new() {Stroke=Brushes.White,StrokeThickness=2,Fill=new SolidColorBrush(Color.FromArgb(45,0,180,185)),Visibility=Visibility.Collapsed};
    private readonly Action<PixelRect?> _finish;
    private readonly Action _move;
    private static Native.POINT? _start;
    private static PixelRect _current;
    public static Task<PixelRect?> PickAsync(IReadOnlyList<DisplayInfo> displays)
    {
        var done = new TaskCompletionSource<PixelRect?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var windows = new List<RegionPicker>(); _start = null;
        void Finish(PixelRect? rect) { if(done.Task.IsCompleted)return; foreach(var w in windows)w.Close();done.TrySetResult(rect); }
        void Move() { foreach(var w in windows)w.Draw(); }
        foreach(var d in displays)windows.Add(new(d,Finish,Move));
        foreach(var w in windows)w.Show();
        return done.Task;
    }
    private RegionPicker(DisplayInfo d,Action<PixelRect?> finish,Action move)
    {
        _display=d;_finish=finish;_move=move;
        WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;AllowsTransparency=true;
        Background=new SolidColorBrush(Color.FromArgb(65,10,25,40));Topmost=true;ShowInTaskbar=false;Cursor=Cursors.Cross;
        Content=_canvas;_canvas.Children.Add(_selection);
        var hint=new Border {Background=new SolidColorBrush(Color.FromRgb(25,42,57)),CornerRadius=new(8),Padding=new(16,11,16,11),Child=new TextBlock{Text="Kéo để chọn vùng • Esc để hủy",Foreground=Brushes.White,FontSize=16}};
        _canvas.Children.Add(hint);Canvas.SetLeft(hint,24);Canvas.SetTop(hint,24);
        SourceInitialized+=(_,_)=>Native.SetWindowPos(new WindowInteropHelper(this).Handle,new nint(-1),d.Bounds.X,d.Bounds.Y,d.Bounds.Width,d.Bounds.Height,0x40);
        Loaded+=(_,_)=>Native.SetWindowPos(new WindowInteropHelper(this).Handle,new nint(-1),d.Bounds.X,d.Bounds.Y,d.Bounds.Width,d.Bounds.Height,0x40);
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape)_finish(null);};
        MouseLeftButtonDown+=(_,e)=>{Native.GetCursorPos(out var p);_start=p;CaptureMouse();e.Handled=true;};
        MouseMove+=(_,_)=>{if(_start is { } s){Native.GetCursorPos(out var p);_current=PixelRect.FromPoints(s.X,s.Y,p.X,p.Y);_move();}};
        MouseLeftButtonUp+=(_,_)=>{if(_start is { } s){Native.GetCursorPos(out var p);ReleaseMouseCapture();var r=PixelRect.FromPoints(s.X,s.Y,p.X,p.Y);_finish(r.Width>=8&&r.Height>=8?r:null);}};
        MouseRightButtonDown+=(_,_)=>_finish(null);
    }
    private void Draw()
    {
        var r=_current.Intersect(_display.Bounds);if(r.IsEmpty){_selection.Visibility=Visibility.Collapsed;return;}
        var a=PointFromScreen(new(r.X,r.Y));var b=PointFromScreen(new(r.Right,r.Bottom));
        _selection.Visibility=Visibility.Visible;Canvas.SetLeft(_selection,a.X);Canvas.SetTop(_selection,a.Y);_selection.Width=b.X-a.X;_selection.Height=b.Y-a.Y;
    }
}
