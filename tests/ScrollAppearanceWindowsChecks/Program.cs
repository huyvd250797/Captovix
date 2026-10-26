using Captovix;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
internal static class Program
{
 static int n;static void Check(bool b,string s){if(!b)throw new Exception(s);n++;Console.WriteLine("PASS: "+s);}
 [STAThread]static int Main(){string root=Path.Combine(Path.GetTempPath(),"Captovix-114-wpf-"+Guid.NewGuid().ToString("N"));try{
    Directory.CreateDirectory(root);var app=new App();app.InitializeComponent();var current=typeof(AppearanceManager).GetProperty("Current",BindingFlags.Public|BindingFlags.Static)!;
    foreach(var mode in new[]{AppearanceMode.Light,AppearanceMode.Dark}){
        current.SetValue(null,new AppearanceSettings(mode,"#7C3AED"));AppearanceManager.Apply();
        Check(((SolidColorBrush)app.Resources["Surface"]).Color.ToString()==(mode==AppearanceMode.Dark?"#FF23262E":"#FFFFFFFF"),mode+" surface resources");
        var combo=new ComboBox{ItemsSource=new[]{"Sáng","Tối"},SelectedIndex=1};combo.Measure(new(250,100));combo.Arrange(new(0,0,250,40));combo.ApplyTemplate();Check(combo.Template.FindName("PART_Popup",combo)!=null,mode+" combo template loads");
        var button=new Button{Content="Test",Style=(Style)app.FindResource("Primary")};button.Measure(new(200,50));button.ApplyTemplate();Check(button.Foreground is SolidColorBrush b&&b.Color==Colors.White,mode+" primary text has contrast");
        double L(Color c){double C(byte v){double x=v/255d;return x<=.04045?x/12.92:Math.Pow((x+.055)/1.055,2.4);}return .2126*C(c.R)+.7152*C(c.G)+.0722*C(c.B);}
        foreach(string key in new[]{"Surface","Hover","Soft","Workspace"}){
            double text=L(((SolidColorBrush)app.Resources["Text"]).Color),bg=L(((SolidColorBrush)app.Resources[key]).Color);
            Check((Math.Max(text,bg)+.05)/(Math.Min(text,bg)+.05)>=4.5,mode+" readable text on "+key);
        }
        var menu=new ProbeMenuItem{Header="Thiết lập hệ thống",Style=(Style)app.FindResource(typeof(MenuItem))};menu.Items.Add(new MenuItem{Header="Phím tắt"});
        menu.Measure(new(260,100));menu.Arrange(new(0,0,260,40));menu.ApplyTemplate();
        Check(menu.Template.FindName("PART_Popup",menu) is System.Windows.Controls.Primitives.Popup,mode+" submenu popup template loads");
        var chrome=(Border)menu.Template.FindName("Chrome",menu);menu.Highlight();
        Check(chrome.Background==app.Resources["Hover"],mode+" hover uses theme brush rather than Windows white chrome");
        Check(menu.Foreground==app.Resources["Text"],mode+" hover preserves readable foreground");
        menu.IsEnabled=false;Check(menu.Foreground==app.Resources["Muted"],mode+" disabled menu has muted foreground");
        Check(app.Resources["BrandMark"] is System.Windows.Media.Imaging.BitmapImage,mode+" branded mark loads from pack URI");
    }
    int width=64,height=80;var random=new Random(114);var raw=new byte[width*140*4];random.NextBytes(raw);for(int i=3;i<raw.Length;i+=4)raw[i]=255;
    var session=new ScrollSession{Width=width,Height=height,Folder=root};session.Save();
    for(int i=0;i<2;i++){var pixels=new byte[width*height*4];Buffer.BlockCopy(raw,i*60*width*4,pixels,0,pixels.Length);string candidate=Path.Combine(root,"candidate-"+i+".png");ImageTransfer.Save(ImageTransfer.Bitmap(width,height,pixels),candidate);session.Append(candidate,i==0?null:new(ScrollMatchKind.Matched,20,0,"test"));}
    Check(session.Count==2&&session.Joins.Count==1&&session.Hashes.Count==2,"Session commits raw frames, join and hashes");var loaded=ScrollSession.Open(Path.Combine(root,"session.json"));var image=loaded.Compose();Check(image.PixelWidth==64&&image.PixelHeight==140,"Recovered PNG session composes exact geometry");Check(ImageTransfer.Bgra(image).SequenceEqual(raw),"Recovered stitched PNG pixels equal original bytes");
    ImageTransfer.Save(image,Path.Combine(root,"output.png"));Check(ImageTransfer.Bgra(MediaStore.LoadImage(Path.Combine(root,"output.png"))).SequenceEqual(raw),"Export roundtrip remains lossless");
    File.AppendAllText(session.FramePath(0),"changed");bool reject=false;try{loaded.Load(0);}catch(InvalidDataException){reject=true;}Check(reject,"Changed raw PNG hash rejected during recovery");
    Check(File.Exists(session.FramePath(1)),"Failed recovery preserves original files");Console.WriteLine($"{n} scrolling/appearance Windows checks passed. Live capture/input/installer still need desktop acceptance.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}finally{if(Directory.Exists(root))Directory.Delete(root,true);}}
}

internal sealed class ProbeMenuItem:MenuItem { public void Highlight()=>IsHighlighted=true; }
