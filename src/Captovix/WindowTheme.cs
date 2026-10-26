using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
namespace Captovix;
internal static class WindowTheme
{
    internal static void Initialize()=>EventManager.RegisterClassHandler(typeof(Window),FrameworkElement.LoadedEvent,new RoutedEventHandler((sender,_)=>{if(sender is Window w){Apply(w);WindowChromeStudio.Apply(w);}}));
    internal static void ApplyAll(){foreach(Window window in Application.Current.Windows)Apply(window);}
    internal static void Apply(Window window)
    {
        var hwnd=new WindowInteropHelper(window).Handle;if(hwnd==0)return;
        try{
            bool dark=((SolidColorBrush)Application.Current.Resources["Surface"]).Color.R<100;int flag=dark?1:0;
            if(DwmSetWindowAttribute(hwnd,20,ref flag,4)!=0)DwmSetWindowAttribute(hwnd,19,ref flag,4);
            // Windows 11 supports explicit caption/text colours; Windows 10 keeps native fallback.
            if(OperatingSystem.IsWindowsVersionAtLeast(10,0,22000)){
                int ColorRef(string key){var c=((SolidColorBrush)Application.Current.Resources[key]).Color;return c.R|(c.G<<8)|(c.B<<16);}
                int caption=ColorRef("AppBackground"),text=ColorRef("Text"),border=ColorRef("Line");
                DwmSetWindowAttribute(hwnd,35,ref caption,4);DwmSetWindowAttribute(hwnd,36,ref text,4);DwmSetWindowAttribute(hwnd,34,ref border,4);
            }
        }catch(DllNotFoundException){}catch(EntryPointNotFoundException){}
    }
    [DllImport("dwmapi.dll")]private static extern int DwmSetWindowAttribute(nint hwnd,int attribute,ref int value,int bytes);
}
