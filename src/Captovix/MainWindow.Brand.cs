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
            AppearanceManager.Applied+=RefreshTaskbarBrand;
            RefreshTaskbarBrand();
        };
        Loaded+=(_,_)=>Dispatcher.BeginInvoke(RefreshTaskbarBrand,System.Windows.Threading.DispatcherPriority.Loaded);
        DpiChanged+=(_,_)=>Dispatcher.BeginInvoke(RefreshTaskbarBrand,System.Windows.Threading.DispatcherPriority.Loaded);
        Closed+=(_,_)=>{
            AppearanceManager.Applied-=RefreshTaskbarBrand;
            if(_brandSmall!=0)BrandDestroyIcon(_brandSmall);
            if(_brandLarge!=0)BrandDestroyIcon(_brandLarge);
        };
    }
    private void RefreshTaskbarBrand()
    {
        try{
            if(Application.Current.Resources["BrandMark"] is not BitmapSource bitmap)return;
            var hwnd=new WindowInteropHelper(this).Handle;
            if(hwnd==0)return;
            var dpi=System.Windows.Media.VisualTreeHelper.GetDpi(this);
            SetIcon(ref _brandSmall,0,16);
            SetIcon(ref _brandLarge,1,32);

            void SetIcon(ref nint owned,int slot,int size)
            {
                var next=ShellBrandIcon.Create(bitmap,(int)Math.Round(size*dpi.DpiScaleX),(int)Math.Round(size*dpi.DpiScaleY));
                if(next==0)return;
                BrandSendMessage(hwnd,0x80,slot,next); // WM_SETICON
                if(owned!=0)BrandDestroyIcon(owned);
                owned=next;
            }
        }catch(Exception ex){App.Log(ex);}
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
    [DllImport("user32.dll",EntryPoint="SendMessageW")]private static extern nint BrandSendMessage(nint hwnd,uint message,nint w,nint l);
    [DllImport("user32.dll",EntryPoint="DestroyIcon")]private static extern bool BrandDestroyIcon(nint icon);
}
