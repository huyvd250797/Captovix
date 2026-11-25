using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Captovix;

internal sealed class StartupSplash : Window
{
    internal StartupSplash()
    {
        Title="Captovix";
        Width=390;
        Height=175;
        WindowStyle=WindowStyle.None;
        ResizeMode=ResizeMode.NoResize;
        ShowInTaskbar=false;
        ShowActivated=false;
        Topmost=true;
        WindowStartupLocation=WindowStartupLocation.CenterScreen;
        var content=new StackPanel{Margin=new(24,18,24,18)};
        var identity=new StackPanel{Orientation=Orientation.Horizontal};
        var logo=new Image{Width=40,Height=40,Margin=new(0,0,12,0)};
        logo.SetResourceReference(Image.SourceProperty,"BrandMark");
        identity.Children.Add(logo);
        identity.Children.Add(new TextBlock{Text="Captovix",FontSize=25,FontWeight=FontWeights.SemiBold,VerticalAlignment=VerticalAlignment.Center});
        content.Children.Add(identity);
        var text=new TextBlock{Text=App.StartedAutomatically?"Đang mở cùng Windows…":"Đang mở Captovix…",Margin=new(0,12,0,9)};
        text.SetResourceReference(TextBlock.ForegroundProperty,"Muted");
        content.Children.Add(text);
        var progress=new ProgressBar{Height=4,IsIndeterminate=true,Focusable=false};
        progress.SetResourceReference(Control.ForegroundProperty,"Accent");
        content.Children.Add(progress);
        var card=new Border{BorderThickness=new(1),Child=content};
        card.SetResourceReference(Border.BackgroundProperty,"Surface");
        card.SetResourceReference(Border.BorderBrushProperty,"Line");
        Content=card;
    }
}
