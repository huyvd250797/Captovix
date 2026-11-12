using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Captovix;

internal static class RecordingCountdown
{
    internal static async Task ShowAsync(PixelRect region,IReadOnlyList<DisplayInfo> displays,CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        var parts=displays.Select(d=>region.Intersect(d.Bounds)).Where(r=>!r.IsEmpty)
            .OrderByDescending(r=>(long)r.Width*r.Height).ToArray();
        if(parts.Length==0)throw new InvalidOperationException("Nguồn quay đã ra ngoài màn hình.");

        using var stop=CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        var overlays=new List<Window>();
        var digit=new TextBlock{Text="3",Foreground=Brushes.White,FontSize=144,FontWeight=FontWeights.Bold};
        bool finished=false;
        try{
            for(int i=0;i<parts.Length;i++){
                var bounds=parts[i];
                var window=new Window{WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,
                    Width=1,Height=1,Topmost=true,AllowsTransparency=true,ShowInTaskbar=false,
                    Background=new SolidColorBrush(Color.FromArgb(164,0,0,0)),ShowActivated=i==0};
                if(i==0){
                    var panel=new StackPanel{HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};
                    panel.Children.Add(digit);
                    var cancel=new Button{Content="Hủy · Esc",HorizontalAlignment=HorizontalAlignment.Center};
                    cancel.Click+=(_,_)=>stop.Cancel();
                    panel.Children.Add(cancel);
                    window.Content=new Viewbox{Child=panel,StretchDirection=StretchDirection.DownOnly,Margin=new Thickness(6)};
                }
                void Position(){
                    var hwnd=new WindowInteropHelper(window).Handle;
                    Native.SetWindowPos(hwnd,-1,bounds.X,bounds.Y,bounds.Width,bounds.Height,0x10);
                    Native.SetWindowDisplayAffinity(hwnd,0x11);
                }
                window.SourceInitialized+=(_,_)=>Position();
                window.Loaded+=(_,_)=>Position();
                window.PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape){e.Handled=true;stop.Cancel();}};
                window.Closed+=(_,_)=>{if(!finished)stop.Cancel();};
                overlays.Add(window);
                window.Show();
            }
            for(int number=3;number>0;number--){
                digit.Text=number.ToString();
                await Task.Delay(1000,stop.Token);
            }
        }finally{
            finished=true;
            foreach(var window in overlays)if(window.IsVisible)window.Close();
        }
        // Give the compositor a frame to remove the overlay before opening the video source.
        await Task.Delay(120,cancellation);
    }
}
