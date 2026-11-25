using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Captovix;

/// <summary>DWM supplies previews without PrintWindow, screen grabs or polling foreign processes.</summary>
internal static class RecordingWindowPicker
{
    private sealed record Tile(WindowInfo Window,Border Card,Border Preview,nint Thumbnail);
    internal static async Task<WindowInfo?> PickAsync(CancellationToken cancellation,WindowInfo? preferred)
    {
        cancellation.ThrowIfCancellationRequested();
        var done=new TaskCompletionSource<WindowInfo?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var window=new Window{Title="Captovix · Chọn cửa sổ để quay",Width=850,Height=620,MinWidth=580,MinHeight=400,
            WindowStartupLocation=WindowStartupLocation.CenterScreen,ShowInTaskbar=false,Topmost=true};
        var root=new DockPanel{Margin=new(18)};window.Content=root;
        var hint=new TextBlock{Text="Chọn cửa sổ qua ảnh xem trước · Enter / nhấp đúp để xác nhận",TextWrapping=TextWrapping.Wrap,Margin=new(0,0,0,12)};
        DockPanel.SetDock(hint,Dock.Top);root.Children.Add(hint);
        var footer=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new(0,12,0,0)};
        DockPanel.SetDock(footer,Dock.Bottom);root.Children.Add(footer);
        var refresh=new Button{Content=AppIcons.Content("Refresh","Làm mới")};
        var cancel=new Button{Content="Hủy",IsCancel=true};
        var accept=new Button{Content=AppIcons.Content("Record","Quay cửa sổ này"),IsEnabled=false};
        footer.Children.Add(refresh);footer.Children.Add(cancel);footer.Children.Add(accept);
        var grid=new WrapPanel();var scroll=new ScrollViewer{Content=grid,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};root.Children.Add(scroll);
        var tiles=new List<Tile>();WindowInfo? selected=null;bool closed=false;
        void Select(WindowInfo source){selected=source;accept.IsEnabled=true;foreach(var tile in tiles)tile.Card.BorderBrush=tile.Window.Handle==source.Handle?Brushes.DeepSkyBlue:Brushes.DimGray;}
        void DisposeThumbnails(){foreach(var tile in tiles)if(tile.Thumbnail!=0)DwmUnregisterThumbnail(tile.Thumbnail);tiles.Clear();}
        void Refresh(){
            if(closed)return;
            DisposeThumbnails();grid.Children.Clear();selected=null;accept.IsEnabled=false;
            nint destination=new WindowInteropHelper(window).Handle;
            foreach(var source in Native.Windows(false)){
                var body=new StackPanel();var preview=new Border{Width=232,Height=132,Background=Brushes.Black,
                    Child=new TextBlock{Text="Không có ảnh xem trước",Foreground=Brushes.Gray,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center}};
                body.Children.Add(preview);body.Children.Add(new TextBlock{Text=source.Title,TextTrimming=TextTrimming.CharacterEllipsis,MaxWidth=232,Margin=new(0,8,0,0),ToolTip=source.Title});
                var card=new Border{Child=body,BorderBrush=Brushes.DimGray,BorderThickness=new(2),CornerRadius=new(8),Padding=new(8),Margin=new(4),Cursor=Cursors.Hand,Focusable=true};
                grid.Children.Add(card);
                nint thumbnail=0;if(DwmRegisterThumbnail(destination,source.Handle,out thumbnail)!=0)thumbnail=0;
                if(thumbnail!=0)preview.Child=null;
                tiles.Add(new(source,card,preview,thumbnail));
                card.MouseLeftButtonDown+=(_,e)=>{Select(source);card.Focus();if(e.ClickCount==2)Finish();e.Handled=true;};
                card.KeyDown+=(_,e)=>{if(e.Key==Key.Space){Select(source);e.Handled=true;}};
            }
            if(tiles.Count==0)hint.Text="Không có cửa sổ đang mở. Mở cửa sổ cần quay rồi bấm Làm mới.";
            else hint.Text="Chọn cửa sổ qua ảnh xem trước · Enter / nhấp đúp để xác nhận";
            var first=tiles.FirstOrDefault(t=>preferred!=null&&t.Window.Handle==preferred.Handle&&t.Window.ProcessId==preferred.ProcessId)??tiles.FirstOrDefault();
            if(first!=null)Select(first.Window);
            Update();
        }
        void Finish(){if(selected is not {} source)return;if(!Native.IsSameWindow(source)){hint.Text="Cửa sổ đã đóng. Chọn lại cửa sổ cần quay.";Refresh();return;}done.TrySetResult(source);window.Close();}
        void Update(){
            if(closed||!window.IsVisible||!scroll.IsArrangeValid)return;
            var origin=new PointNative();ClientToScreen(new WindowInteropHelper(window).Handle,ref origin);
            var viewTop=scroll.PointToScreen(new(0,0));var viewBottom=scroll.PointToScreen(new(scroll.ActualWidth,scroll.ActualHeight));
            foreach(var tile in tiles){
                if(tile.Thumbnail==0||!tile.Preview.IsArrangeValid)continue;
                var a=tile.Preview.PointToScreen(new(0,0));var b=tile.Preview.PointToScreen(new(tile.Preview.ActualWidth,tile.Preview.ActualHeight));
                bool visible=a.X>=viewTop.X&&a.Y>=viewTop.Y&&b.X<=viewBottom.X&&b.Y<=viewBottom.Y;
                if(DwmQueryThumbnailSourceSize(tile.Thumbnail,out var size)==0&&size.X>0&&size.Y>0){
                    double scale=Math.Min((b.X-a.X)/size.X,(b.Y-a.Y)/size.Y),w=size.X*scale,h=size.Y*scale;
                    a=new(a.X+(b.X-a.X-w)/2,a.Y+(b.Y-a.Y-h)/2);b=new(a.X+w,a.Y+h);
                }
                var properties=new ThumbnailProperties{Flags=1|4|8|16,Destination=new(){Left=(int)Math.Round(a.X-origin.X),Top=(int)Math.Round(a.Y-origin.Y),Right=(int)Math.Round(b.X-origin.X),Bottom=(int)Math.Round(b.Y-origin.Y)},Opacity=255,Visible=visible,SourceClientOnly=false};
                DwmUpdateThumbnailProperties(tile.Thumbnail,ref properties);
            }
        }
        refresh.Click+=(_,_)=>Refresh();cancel.Click+=(_,_)=>window.Close();accept.Click+=(_,_)=>Finish();
        window.PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape){window.Close();e.Handled=true;}else if(e.Key==Key.Enter){Finish();e.Handled=true;}};
        window.Loaded+=(_,_)=>Refresh();window.LayoutUpdated+=(_,_)=>Update();scroll.ScrollChanged+=(_,_)=>Update();
        window.Closed+=(_,_)=>{closed=true;DisposeThumbnails();done.TrySetResult(null);};
        using var registration=cancellation.Register(()=>window.Dispatcher.BeginInvoke(()=>{if(!closed)window.Close();}));
        window.Show();return await done.Task;
    }
    [StructLayout(LayoutKind.Sequential)] private struct RectNative{public int Left,Top,Right,Bottom;}
    [StructLayout(LayoutKind.Sequential)] private struct PointNative{public int X,Y;}
    [StructLayout(LayoutKind.Sequential)] private struct ThumbnailProperties{
        public uint Flags;public RectNative Destination,Source;public byte Opacity;
        [MarshalAs(UnmanagedType.Bool)]public bool Visible;
        [MarshalAs(UnmanagedType.Bool)]public bool SourceClientOnly;
    }
    [DllImport("dwmapi.dll")]private static extern int DwmRegisterThumbnail(nint destination,nint source,out nint thumbnail);
    [DllImport("dwmapi.dll")]private static extern int DwmUnregisterThumbnail(nint thumbnail);
    [DllImport("dwmapi.dll")]private static extern int DwmUpdateThumbnailProperties(nint thumbnail,ref ThumbnailProperties properties);
    [DllImport("dwmapi.dll")]private static extern int DwmQueryThumbnailSourceSize(nint thumbnail,out PointNative size);
    [DllImport("user32.dll")]private static extern bool ClientToScreen(nint window,ref PointNative point);
}
