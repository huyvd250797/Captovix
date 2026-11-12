using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Captovix;

public partial class MainWindow
{
    private void ListenRecordingAudio_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||Selected is not {IsVideo:true} item)return;
        bool system=(sender as Button)?.Tag as string=="System";
        string path=RecordingAudioFiles.PathFor(ReferenceEquals(item,_videoItem)?_videoSource??item.FilePath:item.FilePath,system);
        if(!File.Exists(path)){SetStatus("Không tìm thấy tệp thu âm riêng.");return;}
        VideoPreview.Pause();
        SetVideoPlaying(false);
        var player=new MediaPlayer();
        string title=system?"Âm thanh máy":"Micro";
        var window=new Window{Owner=this,Title=$"Nghe {title.ToLowerInvariant()}",Width=390,
            SizeToContent=SizeToContent.Height,WindowStartupLocation=WindowStartupLocation.CenterOwner,ResizeMode=ResizeMode.NoResize};
        var panel=new StackPanel{Margin=new Thickness(18)};
        window.Content=panel;
        panel.Children.Add(new TextBlock{Text=title,FontWeight=FontWeights.SemiBold,FontSize=18});
        var status=new TextBlock{Text=item.Title,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(4,5,4,10)};
        panel.Children.Add(status);
        var buttons=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
        var close=new Button{Content="Đóng",IsCancel=true};
        var play=new Button{Content="Phát từ đầu",IsEnabled=false};
        var pause=new Button{Content="Tạm dừng",IsEnabled=false};
        buttons.Children.Add(close);buttons.Children.Add(play);buttons.Children.Add(pause);
        panel.Children.Add(buttons);
        close.Click+=(_,_)=>window.Close();
        play.Click+=(_,_)=>{player.Position=TimeSpan.Zero;player.Play();};
        pause.Click+=(_,_)=>player.Pause();
        player.MediaOpened+=(_,_)=>{play.IsEnabled=pause.IsEnabled=true;player.Play();};
        player.MediaFailed+=(_,args)=>{status.Text="Không phát được tệp âm thanh. "+args.ErrorException?.Message;play.IsEnabled=pause.IsEnabled=false;};
        window.Loaded+=(_,_)=>player.Open(new Uri(Path.GetFullPath(path)));
        window.Closed+=(_,_)=>player.Close();
        window.ShowDialog();
    }
}
