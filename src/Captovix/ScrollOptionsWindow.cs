using System.Windows;
using System.Windows.Controls;
namespace Captovix;
internal sealed class ScrollOptionsWindow:Window
{
    public ScrollOptionsWindow(ScrollSession session){Title="Captovix · Hướng cuộn và vùng cố định";Width=460;SizeToContent=SizeToContent.Height;WindowStartupLocation=WindowStartupLocation.CenterOwner;var panel=new StackPanel{Margin=new(20)};Content=panel;
    var direction=new ComboBox{ItemsSource=new[]{"Dọc · cuộn xuống","Ngang · cuộn sang phải"},SelectedIndex=0};panel.Children.Add(direction);panel.Children.Add(new TextBlock{Text="Header / footer cố định · pixel (chụp dọc)",Margin=new(4,12,4,4)});var top=new TextBox{Text="0"};var bottom=new TextBox{Text="0"};panel.Children.Add(top);panel.Children.Add(bottom);var keep=new CheckBox{Content="Giữ header đầu và footer cuối một lần",IsChecked=true};panel.Children.Add(keep);var error=new TextBlock{TextWrapping=TextWrapping.Wrap};panel.Children.Add(error);var save=new Button{Content="Bắt đầu phiên"};panel.Children.Add(save);save.Click+=(_,_)=>{try{if(!int.TryParse(top.Text,out int a)||!int.TryParse(bottom.Text,out int b)||a<0||b<0)throw new Exception("Nhập số pixel nguyên không âm.");bool horizontal=direction.SelectedIndex==1;if(horizontal&&(a!=0||b!=0))throw new Exception("Ghép ngang: chọn vùng không có header/footer và đặt hai giá trị bằng0.");session.Horizontal=horizontal;session.Header=a;session.Footer=b;session.KeepHeader=keep.IsChecked==true;ScrollStitching.OutputHeight(session.WorkWidth,session.WorkHeight,Array.Empty<int>());DialogResult=true;}catch(Exception ex){error.Text=ex.Message;}};}
}
