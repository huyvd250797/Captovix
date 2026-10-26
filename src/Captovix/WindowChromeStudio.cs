using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
namespace Captovix;
internal static class WindowChromeStudio
{
    private static readonly ConditionalWeakTable<Window,object> Applied=new();
    internal static void Apply(Window w)
    {
        if(!w.IsLoaded||w.WindowStyle==WindowStyle.None||w.AllowsTransparency||Applied.TryGetValue(w,out _))return;
        Applied.Add(w,new());var original=w.Content;w.Content=null;
        w.WindowStyle=WindowStyle.None;WindowChrome.SetWindowChrome(w,new WindowChrome{CaptionHeight=28,ResizeBorderThickness=w.ResizeMode==ResizeMode.NoResize?new(0):new(6),CornerRadius=new(0),GlassFrameThickness=new(0),UseAeroCaptionButtons=false});
        var root=new Grid();root.RowDefinitions.Add(new(){Height=new(28)});root.RowDefinitions.Add(new());
        var title=new Grid();title.SetResourceReference(Panel.BackgroundProperty,"Surface");title.ColumnDefinitions.Add(new());title.ColumnDefinitions.Add(new(){Width=GridLength.Auto});
        var label=new TextBlock{Margin=new(12,0,8,0),VerticalAlignment=VerticalAlignment.Center,TextTrimming=TextTrimming.CharacterEllipsis,FontSize=12};label.SetBinding(TextBlock.TextProperty,new System.Windows.Data.Binding("Title"){Source=w});label.SetResourceReference(TextBlock.ForegroundProperty,"Text");title.Children.Add(label);
        var buttons=new StackPanel{Orientation=Orientation.Horizontal};Grid.SetColumn(buttons,1);title.Children.Add(buttons);
        Button Button(string icon,string tip,Action action,bool close=false){var b=new Button{Content=AppIcons.Content(icon,"",11),ToolTip=tip,Width=32,Height=26,Padding=new(8,6,8,6),Margin=new(0),BorderThickness=new(0)};b.SetResourceReference(Control.ForegroundProperty,"Text");b.SetResourceReference(Control.BackgroundProperty,"Surface");WindowChrome.SetIsHitTestVisibleInChrome(b,true);b.Click+=(_,_)=>action();if(close){b.MouseEnter+=(_,_)=>{b.Background=System.Windows.Media.Brushes.Firebrick;b.Foreground=System.Windows.Media.Brushes.White;};b.MouseLeave+=(_,_)=>{b.SetResourceReference(Control.BackgroundProperty,"Surface");b.SetResourceReference(Control.ForegroundProperty,"Text");};}b.SetValue(System.Windows.Automation.AutomationProperties.NameProperty,tip);buttons.Children.Add(b);return b;}
        if(w is MainWindow||w.ResizeMode is ResizeMode.CanResize or ResizeMode.CanResizeWithGrip){Button("Minimize","Thu nhỏ",()=>w.WindowState=WindowState.Minimized);var maximize=Button("Maximize","Phóng to / khôi phục",()=>w.WindowState=w.WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized);void SyncState()=>maximize.Content=AppIcons.Content(w.WindowState==WindowState.Maximized?"Restore":"Maximize","",11);w.StateChanged+=(_,_)=>SyncState();SyncState();}
        Button("Close","Đóng",w.Close,true);root.Children.Add(title);var host=new ContentControl{Content=original};Grid.SetRow(host,1);root.Children.Add(host);w.Content=root;
        // Retain native caption drag, resize, double-click and system menu through WindowChrome.
    }
}
