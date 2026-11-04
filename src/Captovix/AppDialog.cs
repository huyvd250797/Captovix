using System.Windows;
using System.Windows.Controls;
namespace Captovix;
public static class AppDialog
{
 public static MessageBoxResult Show(string message,string title="Captovix",MessageBoxButton buttons=MessageBoxButton.OK,MessageBoxImage image=MessageBoxImage.None)=>Show(Application.Current?.Windows.OfType<Window>().FirstOrDefault(w=>w.IsActive),message,title,buttons,image);
 public static MessageBoxResult Show(Window? owner,string message,string title="Captovix",MessageBoxButton buttons=MessageBoxButton.OK,MessageBoxImage image=MessageBoxImage.None)
 {
  if(Application.Current==null)return MessageBox.Show(message,title,buttons,image);
  var w=new Window{Title=title,Width=470,MaxHeight=Math.Max(300,SystemParameters.WorkArea.Height-80),SizeToContent=SizeToContent.Height,ResizeMode=ResizeMode.NoResize,ShowInTaskbar=owner==null,WindowStartupLocation=owner==null?WindowStartupLocation.CenterScreen:WindowStartupLocation.CenterOwner};if(owner?.IsVisible==true)w.Owner=owner;
  var panel=new DockPanel{Margin=new(20),LastChildFill=true};w.Content=panel;var actions=new DockPanel{LastChildFill=false,Margin=new(0,18,0,0)};DockPanel.SetDock(actions,Dock.Bottom);panel.Children.Add(actions);MessageBoxResult result=buttons is MessageBoxButton.YesNo?MessageBoxResult.No:buttons is MessageBoxButton.OK?MessageBoxResult.OK:MessageBoxResult.Cancel;
  void Button(string title,MessageBoxResult value,bool confirm,bool cancel){var b=new Button{Content=title,IsDefault=confirm,IsCancel=cancel,MinWidth=86};if(confirm){b.SetResourceReference(FrameworkElement.StyleProperty,"Primary");DockPanel.SetDock(b,Dock.Right);}actions.Children.Add(b);b.Click+=(_,_)=>{result=value;w.Close();};}
  if(buttons is MessageBoxButton.OKCancel or MessageBoxButton.YesNoCancel)Button("Hủy",MessageBoxResult.Cancel,false,true);
  if(buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel)Button("Không",MessageBoxResult.No,false,buttons==MessageBoxButton.YesNo);
  Button(buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel?"Đồng ý":"Đồng ý",buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel?MessageBoxResult.Yes:MessageBoxResult.OK,true,buttons==MessageBoxButton.OK);
  panel.Children.Add(new ScrollViewer{MaxHeight=Math.Max(150,SystemParameters.WorkArea.Height-210),VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Content=new TextBlock{Text=message,TextWrapping=TextWrapping.Wrap,FontSize=13}});w.ShowDialog();return result;
 }
}
