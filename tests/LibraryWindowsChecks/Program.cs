using Captovix;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
internal static class Program
{
 [STAThread]static int Main(){try{
 int n=0;void Check(bool b,string label){if(!b)throw new Exception(label);n++;Console.WriteLine("PASS: "+label);}var app=new App();app.InitializeComponent();app.ShutdownMode=ShutdownMode.OnExplicitShutdown;
 var child=new TextBox{Text="Retained content"};var w=new Window{Title="Modal close contrast",Width=500,Height=300,Content=child};w.Show();w.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ApplicationIdle);
 var chrome=typeof(MainWindow).Assembly.GetType("Captovix.WindowChromeStudio")!;var apply=chrome.GetMethod("Apply",BindingFlags.Static|BindingFlags.NonPublic)!;apply.Invoke(null,[w]);
 Check(w.WindowStyle==WindowStyle.None&&WindowChrome.GetWindowChrome(w)!=null,"Custom titlebar retains native drag/resize through WindowChrome");
 var root=(Grid)w.Content;var host=root.Children.OfType<ContentControl>().Single();Check(ReferenceEquals(host.Content,child),"Modal content survives titlebar composition");
 var title=(Grid)root.Children[0];var buttons=title.Children.OfType<StackPanel>().Single();var close=buttons.Children.OfType<Button>().Single(b=>b.ToolTip?.ToString()=="Đóng");
 Check(close.Foreground is SolidColorBrush foreground&&foreground.Color.A==255,"Close icon has explicit theme foreground before hover");Check(WindowChrome.GetIsHitTestVisibleInChrome(close),"Close button is clickable inside native caption");
 apply.Invoke(null,[w]);Check(ReferenceEquals(root,w.Content),"Repeated theme initialization never nests titlebars");
 close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));Check(!w.IsVisible,"Close command closes modal without hover");
 var mark=new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Captovix;component/Assets/Captovix-mark.png"));Check(mark.PixelWidth>=256,"About uses full-size artwork, not ICO thumbnail");
 var bytes=Enumerable.Repeat((byte)255,80*60*4).ToArray();var bitmap=ImageTransfer.Bitmap(80,60,bytes);var doc=new EditorDocument{OriginalWidth=80,OriginalHeight=60,Appearance=new(){BorderWidth=3,Padding=0,BorderColor="#7C3AED"}};var rendered=EditorRenderer.Render(bitmap,doc);Check(rendered.PixelWidth==86&&rendered.PixelHeight==66,"Capture border adds exact outside pixels");var p=ImageTransfer.Bgra(rendered);bool intact=true;for(int y=0;y<60;y++)for(int x=0;x<80;x++)for(int c=0;c<4;c++)intact&=p[((y+3)*86+x+3)*4+c]==bytes[(y*80+x)*4+c];Check(intact,"Border touches source without a gap or resampling");
 Console.WriteLine($"{n} library/chrome/border Windows checks passed. Desktop UIA and capture require live acceptance.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
