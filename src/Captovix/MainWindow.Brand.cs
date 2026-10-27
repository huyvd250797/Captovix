using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Captovix;
public partial class MainWindow
{
    private nint _brandSmall,_brandLarge;
    private void InitializeBrand()
    {
        SourceInitialized+=(_,_)=>{
            WindowTheme.Apply(this);
            var file=Path.Combine(AppContext.BaseDirectory,"Assets","Captovix-brand-v117.ico");
            var hwnd=new WindowInteropHelper(this).Handle;
            _brandSmall=BrandLoadImage(0,file,1,16,16,0x10);
            _brandLarge=BrandLoadImage(0,file,1,32,32,0x10);
            if(_brandSmall!=0)BrandSendMessage(hwnd,0x80,0,_brandSmall);
            if(_brandLarge!=0)BrandSendMessage(hwnd,0x80,1,_brandLarge);
        };
        Closed+=(_,_)=>{if(_brandSmall!=0)BrandDestroyIcon(_brandSmall);if(_brandLarge!=0)BrandDestroyIcon(_brandLarge);};
    }
    private void About_Click(object sender,RoutedEventArgs e)
    {
        var panel=new StackPanel{Margin=new(24)};
        panel.Children.Add(new Image{Source=(System.Windows.Media.ImageSource)FindResource("BrandMark"),Width=64,Height=64,HorizontalAlignment=HorizontalAlignment.Left});
        panel.Children.Add(new TextBlock{Text="Captovix · V"+VersionInfo.Current,FontSize=24,Margin=new(0,16,0,10)});
        panel.Children.Add(new TextBlock{Text="Capture + Tools + Visual + eXperience",FontWeight=FontWeights.SemiBold});
        panel.Children.Add(new TextBlock{Text="Bộ công cụ chụp, ghi màn hình và biên tập hình ảnh trực quan.",TextWrapping=TextWrapping.Wrap,Margin=new(0,10,0,12)});
        var dialog=new Window{Title="Giới thiệu Captovix",Owner=this,Width=470,SizeToContent=SizeToContent.Height,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner,Content=panel};
        panel.Children.Add(new TextBlock{Text="Ý tưởng, định hướng sản phẩm và thiết kế trải nghiệm: Võ Đức Huy (HuyVo).\n© 2026 HuyVo. All rights reserved.",TextWrapping=TextWrapping.Wrap,Margin=new(0,8,0,16),FontSize=12});
        var close=new Button{Content="Đóng",HorizontalAlignment=HorizontalAlignment.Right,IsCancel=true};close.Click+=(_,_)=>dialog.Close();panel.Children.Add(close);dialog.ShowDialog();
    }
    [DllImport("user32.dll",EntryPoint="LoadImageW",CharSet=CharSet.Unicode)]private static extern nint BrandLoadImage(nint instance,string file,uint type,int width,int height,uint flags);
    [DllImport("user32.dll",EntryPoint="SendMessageW")]private static extern nint BrandSendMessage(nint hwnd,uint message,nint w,nint l);
    [DllImport("user32.dll",EntryPoint="DestroyIcon")]private static extern bool BrandDestroyIcon(nint icon);
}
