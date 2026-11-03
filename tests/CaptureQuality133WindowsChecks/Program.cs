using Captovix;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
internal static class Program {
 [STAThread]static int Main(){try{var app=new Application();var window=new Window{Title="Captovix Self Capture Fixture",Width=360,Height=240,Left=100,Top=100,Background=Brushes.Magenta};window.Show();window.UpdateLayout();window.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ApplicationIdle);var hwnd=new WindowInteropHelper(window).Handle;if(!Native.Windows(true).Any(w=>w.Handle==hwnd))throw new Exception("Own process window missing from opt-in source list");using(var frozen=FrozenDesktop.Capture(Native.Displays(),false)){var point=window.PointToScreen(new(100,100));frozen.Frame.TryColor((int)point.X,(int)point.Y,out var b,out var g,out var r);if(r!=255||g!=0||b!=255)throw new Exception("Own window did not appear in frozen capture");}window.Close();
 var runtime=typeof(MainWindow).Assembly.GetType("Captovix.LicenseRuntime")!;var until=runtime.GetProperty("TrialUntil",BindingFlags.Public|BindingFlags.Static)!;var request=runtime.GetMethod("Request",BindingFlags.Public|BindingFlags.Static)!;
 foreach(var end in new[]{DateTimeOffset.UtcNow.AddDays(10),DateTimeOffset.UtcNow.AddDays(-10)}){until.SetValue(null,end);try{((Task<string?>)request.Invoke(null,["trial","",""])!).GetAwaiter().GetResult();throw new Exception("Repeated trial reached transport");}catch(InvalidOperationException ex){if(!ex.Message.Contains("Không thể kích hoạt thêm"))throw;}if((DateTimeOffset?)until.GetValue(null)!=end)throw new Exception("Repeated trial altered expiry");}
 Console.WriteLine("Self capture and repeated trial guard checks passed; no server request made.");return 0;}catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
