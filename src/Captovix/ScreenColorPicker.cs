using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
namespace Captovix;
internal sealed class ScreenColorPicker : Window
{
    public static bool IsPicking {get;private set;}
    private readonly Canvas _canvas=new();
    private readonly Border _hud;
    private readonly TextBlock _hex=new(){Foreground=Brushes.White,FontSize=14};
    private readonly System.Windows.Shapes.Rectangle _swatch=new(){Width=22,Height=22,Margin=new(0,0,8,0),Stroke=Brushes.White,StrokeThickness=1};
    private readonly DisplayInfo _display;
    private bool _committing;
    private Rect _hudPhysical;
    private readonly byte[] _pixels;
    private ScreenColorPicker(DisplayInfo display,Action<string?> finish)
    {
        _display=display;var frame=Snapshot(display.Bounds);_pixels=frame.Pixels;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;AllowsTransparency=true;Background=new ImageBrush(frame.Image){Stretch=Stretch.Fill};Topmost=true;ShowInTaskbar=false;Cursor=Cursors.Pen;Content=_canvas;
        var row=new StackPanel{Orientation=Orientation.Horizontal};row.Children.Add(_swatch);var info=new StackPanel();info.Children.Add(_hex);info.Children.Add(new TextBlock{Text="Click chọn · Esc hủy",Foreground=Brushes.LightGray,FontSize=11});row.Children.Add(info);
        _hud=new Border{Child=row,Padding=new(9),CornerRadius=new(6),Background=new SolidColorBrush(Color.FromRgb(26,29,37)),IsHitTestVisible=false,Visibility=Visibility.Collapsed};_canvas.Children.Add(_hud);
        void Place(){Native.SetWindowPos(new WindowInteropHelper(this).Handle,-1,display.Bounds.X,display.Bounds.Y,display.Bounds.Width,display.Bounds.Height,0x40);}
        SourceInitialized+=(_,_)=>Place();Loaded+=(_,_)=>{Place();Focus();};PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape){e.Handled=true;finish(null);}};MouseRightButtonDown+=(_,e)=>{e.Handled=true;finish(null);};
        MouseLeftButtonDown+=async(_,e)=>{e.Handled=true;if(_committing||!Native.GetCursorPos(out var point))return;try{foreach(var w in Application.Current.Windows.OfType<ScreenColorPicker>()){w._committing=true;w._hud.Visibility=Visibility.Collapsed;}await Dispatcher.InvokeAsync(()=>{},DispatcherPriority.Render);await Task.Delay(45);if(IsVisible)finish(Read(point.X,point.Y));}catch(Exception ex){App.Log(ex);finish(null);}};
    }
    private string Read(int x,int y){int px=x-_display.Bounds.X,py=y-_display.Bounds.Y;if(px<0||py<0||px>=_display.Bounds.Width||py>=_display.Bounds.Height)throw new ArgumentOutOfRangeException();int i=(py*_display.Bounds.Width+px)*4;return $"#{_pixels[i+2]:X2}{_pixels[i+1]:X2}{_pixels[i]:X2}";}
    private static (BitmapSource Image,byte[] Pixels) Snapshot(PixelRect r)
    {
        nint screen=0,dc=0,bitmap=0,old=0;
        try{screen=GetDC(0);dc=CreateCompatibleDC(screen);if(screen==0||dc==0)throw new Win32Exception("Không đọc được màn hình.");var info=new BitmapInfo{Size=(uint)Marshal.SizeOf<BitmapInfo>(),Width=r.Width,Height=-r.Height,Planes=1,Bits=32};bitmap=CreateDIBSection(screen,ref info,0,out var bits,0,0);if(bitmap==0||bits==0)throw new Win32Exception("Không tạo được bộ đệm pixel.");old=SelectObject(dc,bitmap);if(old==0||old==(nint)(-1))throw new Win32Exception("Không chọn được bộ đệm pixel.");if(!BitBlt(dc,0,0,r.Width,r.Height,screen,r.X,r.Y,0x40CC0020)||!GdiFlush())throw new Win32Exception("Không sao chép được pixel màn hình.");var pixels=new byte[checked(r.Width*r.Height*4)];Marshal.Copy(bits,pixels,0,pixels.Length);for(int i=3;i<pixels.Length;i+=4)pixels[i]=255;return(ImageTransfer.Bitmap(r.Width,r.Height,pixels),pixels);}
        finally{if(dc!=0&&old!=0&&old!=(nint)(-1))SelectObject(dc,old);if(bitmap!=0)DeleteObject(bitmap);if(dc!=0)DeleteDC(dc);if(screen!=0)ReleaseDC(0,screen);}
    }
    public static async Task<string?> PickAsync(Window owner)
    {
        if(IsPicking)return null;
        var done=new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);var windows=new List<ScreenColorPicker>();bool finished=false;var timer=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(40)};Native.POINT previous=new(){X=int.MinValue,Y=int.MinValue};
        void Finish(string? color){if(finished)return;finished=true;timer.Stop();foreach(var w in windows)w.Close();done.TrySetResult(color);}
        void Closed(object? sender,EventArgs e)=>Finish(null);
        owner.Closed+=Closed;
        try{
            IsPicking=true;var displays=Native.Displays();if(displays.Sum(d=>(long)d.Bounds.Width*d.Bounds.Height)>40_000_000)throw new InvalidOperationException("Lấy màu hỗ trợ tổng màn hình tối đa40MP.");await owner.Dispatcher.InvokeAsync(()=>{},DispatcherPriority.Render);await Task.Delay(45);if(finished)return null;foreach(var display in displays)windows.Add(new(display,Finish));if(windows.Count==0)return null;foreach(var window in windows)window.Show();
            timer.Tick+=(_,_)=>{if(finished||windows.Any(w=>w._committing)||!Native.GetCursorPos(out var p))return;try{foreach(var window in windows){if(!ElementSelection.Contains(window._display.Bounds,p.X,p.Y)){window._hud.Visibility=Visibility.Collapsed;continue;}var color=window.Read(p.X,p.Y);window._hex.Text=color;window._swatch.Fill=(Brush)new BrushConverter().ConvertFromString(color)!;var point=window.PointFromScreen(new(p.X,p.Y));window._hud.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));var size=window._hud.DesiredSize;Canvas.SetLeft(window._hud,Math.Clamp(point.X+24,0,Math.Max(0,window.ActualWidth-size.Width)));double y=point.Y+26;if(y+size.Height>window.ActualHeight)y=point.Y-size.Height-26;Canvas.SetTop(window._hud,Math.Max(0,y));var a=window.PointToScreen(new(Canvas.GetLeft(window._hud),Canvas.GetTop(window._hud)));double dpi=VisualTreeHelper.GetDpi(window).DpiScaleX;window._hudPhysical=new Rect(a,new Size(size.Width*dpi,size.Height*dpi));window._hud.Visibility=Visibility.Visible;}previous=p;}catch(Exception ex){App.Log(ex);Finish(null);}};
            timer.Start();return await done.Task;
        }finally{owner.Closed-=Closed;Finish(null);IsPicking=false;}
    }
    [DllImport("user32.dll")]private static extern nint GetDC(nint window);
    [DllImport("user32.dll")]private static extern int ReleaseDC(nint window,nint dc);
    [StructLayout(LayoutKind.Sequential)]private struct BitmapInfo{public uint Size;public int Width,Height;public ushort Planes,Bits;public uint Compression,ImageSize;public int XResolution,YResolution;public uint ColorsUsed,ColorsImportant;}
    [DllImport("gdi32.dll")]private static extern nint CreateCompatibleDC(nint dc);
    [DllImport("gdi32.dll")]private static extern nint CreateDIBSection(nint dc,ref BitmapInfo info,uint usage,out nint bits,nint section,uint offset);
    [DllImport("gdi32.dll")]private static extern nint SelectObject(nint dc,nint bitmap);
    [DllImport("gdi32.dll")]private static extern bool BitBlt(nint dest,int x,int y,int width,int height,nint source,int sx,int sy,uint operation);
    [DllImport("gdi32.dll")]private static extern bool GdiFlush();
    [DllImport("gdi32.dll")]private static extern bool DeleteObject(nint bitmap);
    [DllImport("gdi32.dll")]private static extern bool DeleteDC(nint dc);
}
