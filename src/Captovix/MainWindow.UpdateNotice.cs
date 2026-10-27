using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Media.Animation;
namespace Captovix;
public partial class MainWindow
{
    private readonly UpdatePromptPolicy _updatePromptPolicy=new();
    private bool _updatePromptQueued;
    private void QueueUpdateNotice()
    {
        if(_updatePromptQueued||_updates?.Release is not {} release||_updates.Stage!=UpdateStage.Available)return;
        bool blocked=_busy||_updateApplying||!IsVisible||_settingsDialog!=null||!IsActive||_updateUiClosed;
        if(!_updatePromptPolicy.Offer(release.Version,true,blocked))return;
        _updatePromptQueued=true;
        Dispatcher.BeginInvoke(()=>{
            _updatePromptQueued=false;if(_updateUiClosed||_updates?.Release?.Version!=release.Version||_updates.Stage!=UpdateStage.Available)return;
            if(_busy||!IsVisible||!IsActive||_settingsDialog!=null){_updatePromptPolicy.Release(release.Version);return;}
            var panel=new StackPanel{Margin=new(24)};panel.Children.Add(AppIcons.Content("Refresh","Có phiên bản Captovix mới"));panel.Children.Add(new TextBlock{Text="V"+release.Version,FontSize=28,FontWeight=FontWeights.SemiBold,Margin=new(0,16,0,12)});
            panel.Children.Add(new TextBlock{Text="Bản đang dùng: V"+VersionInfo.Current+" · có thể cập nhật hoặc để sau.",TextWrapping=TextWrapping.Wrap,Margin=new(0,0,0,12)});
            panel.Children.Add(new ScrollViewer{Content=new TextBlock{Text=release.Notes.Length>5000?release.Notes[..5000]+"…":release.Notes,TextWrapping=TextWrapping.Wrap},MaxHeight=210,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});
            var row=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new(0,18,0,0)};var later=new Button{Content="Để sau",IsCancel=true};var update=new Button{Content="Cập nhật",Style=(Style)FindResource("Primary"),IsDefault=true};row.Children.Add(later);row.Children.Add(update);panel.Children.Add(row);
            var notice=new Window{Owner=this,Title="Captovix · Cập nhật mới",Content=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto},MaxHeight=Math.Max(300,SystemParameters.WorkArea.Height-32),Width=Math.Min(540,SystemParameters.WorkArea.Width-32),SizeToContent=SizeToContent.Height,WindowStartupLocation=WindowStartupLocation.CenterOwner,ResizeMode=ResizeMode.NoResize,ShowInTaskbar=false};
            bool accepted=false;Point? start=null;later.Click+=(_,_)=>notice.Close();update.Click+=(_,_)=>{accepted=true;notice.Close();};notice.Closing+=(_,_)=>{try{start=notice.PointToScreen(new(notice.ActualWidth/2,notice.ActualHeight/2));}catch{}};
            notice.ShowDialog();if(accepted){Dispatcher.BeginInvoke(()=>{OpenSettingsAndDownload();});}else FlyUpdateBadge(start,release.Version);
        });
    }
    private void OpenSettingsAndDownload()
    {
        if(_updates?.CanApply==true&&_updates.Stage==UpdateStage.Available){OpenSettings("Updates");}
        else OpenRelease_Click(this,new RoutedEventArgs());
    }
    private void FlyUpdateBadge(Point? screen,string? version)
    {
        if(!IsVisible||_updateUiClosed||!SystemParameters.ClientAreaAnimation)return;
        try{
            Point end=UpdateFooterButton.TranslatePoint(new(UpdateFooterButton.ActualWidth/2,UpdateFooterButton.ActualHeight/2),UpdateFlightCanvas);
            Point start=screen is {} p?UpdateFlightCanvas.PointFromScreen(p):new(UpdateFlightCanvas.ActualWidth/2,UpdateFlightCanvas.ActualHeight/2);
            var badge=new Border{Width=220,Height=56,CornerRadius=new(12),BorderThickness=new(1),Padding=new(16),Child=AppIcons.Content("Refresh","Bản V"+version),RenderTransformOrigin=new(.5,.5),RenderTransform=new ScaleTransform(1,1)};
            badge.SetResourceReference(Border.BackgroundProperty,"Surface");badge.SetResourceReference(Border.BorderBrushProperty,"AccentText");badge.SetResourceReference(TextElement.ForegroundProperty,"Text");UpdateFlightCanvas.Children.Add(badge);Canvas.SetLeft(badge,start.X-110);Canvas.SetTop(badge,start.Y-28);
            Duration duration=TimeSpan.FromMilliseconds(520);DoubleAnimation Animation(double from,double to)=>new(from,to,duration){EasingFunction=new CubicEase{EasingMode=EasingMode.EaseInOut}};
            badge.BeginAnimation(Canvas.LeftProperty,Animation(start.X-110,end.X-110));badge.BeginAnimation(Canvas.TopProperty,Animation(start.Y-28,end.Y-28));badge.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty,Animation(1,.12));badge.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty,Animation(1,.12));var fade=Animation(1,0);fade.Completed+=(_,_)=>{UpdateFlightCanvas.Children.Remove(badge);var pulse=new DoubleAnimation(1,.5,TimeSpan.FromMilliseconds(160)){AutoReverse=true,RepeatBehavior=new RepeatBehavior(2)};UpdateFooterButton.BeginAnimation(OpacityProperty,pulse);};badge.BeginAnimation(OpacityProperty,fade);
        }catch(Exception ex){App.Log(ex);}
    }
}
