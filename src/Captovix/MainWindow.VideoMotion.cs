using System.Windows;
using System.Windows.Controls;

namespace Captovix;

public partial class MainWindow
{
    private VideoRange? _videoMotionShownClip;
    private sealed record MotionKeyChoice(VideoMotionKey Key,string Label)
    {
        public override string ToString()=>Label;
    }

    private void SyncVideoMotion(VideoRange clip)
    {
        if(_videoMotionShownClip==clip)return;
        _videoMotionShownClip=clip;
        var motion=clip.Timing;
        VideoMotionClip.Text=$"Đoạn {clip.Number}";
        VideoMotionSpeed.Text=Number(motion.Speed);
        VideoMotionHold.Text=Number(motion.Hold.TotalSeconds);
        VideoMotionIn.Text=Number(motion.FadeIn);
        VideoMotionOut.Text=Number(motion.FadeOut);
        var key=motion.At(VideoMotionFraction());
        VideoMotionZoom.Text=Number(key.Zoom);
        VideoMotionX.Value=100*key.X;
        VideoMotionY.Value=100*key.Y;
        VideoMotionKeys.ItemsSource=motion.Keys.Select(k=>new MotionKeyChoice(k,
            $"{VideoTimecode.Format(TimeSpan.FromSeconds(k.At*clip.MovingDuration.TotalSeconds))} · {Number(k.Zoom)}×")).ToArray();
    }

    private double VideoMotionFraction()=>_timeline is {} timeline
        ?timeline.Selected.Fraction(timeline.Project.Position-timeline.Offset(timeline.Project.SelectedClip)):0;

    private void VideoMotionApply_Click(object sender,RoutedEventArgs e)
    {
        if(_timeline is not {} timeline||!VideoCanEdit||!RequireLicense())return;
        if(!TryNumber(VideoMotionSpeed,out double speed)||speed is <.25 or >4
            ||!TryNumber(VideoMotionHold,out double hold)||hold is <0 or >60
            ||!TryNumber(VideoMotionIn,out double fadeIn)||fadeIn is <0 or >10
            ||!TryNumber(VideoMotionOut,out double fadeOut)||fadeOut is <0 or >10){
            SetStatus("Tốc độ 0,25–4× · Dừng hình 0–60 giây · Chuyển cảnh 0–10 giây.");return;
        }
        ApplyVideoMotionEdit(timeline.Selected.Timing with{
            Speed=speed,Hold=TimeSpan.FromSeconds(hold),FadeIn=fadeIn,FadeOut=fadeOut});
    }

    private void VideoMotionKey_Click(object sender,RoutedEventArgs e)
    {
        if(_timeline is not {} timeline||!VideoCanEdit||!RequireLicense())return;
        if(!TryNumber(VideoMotionZoom,out double zoom)||zoom is <1 or >4){SetStatus("Zoom cần từ 1× đến 4×.");return;}
        var motion=timeline.Selected.Timing;
        double at=VideoMotionFraction();
        var keys=motion.Keys.Where(key=>Math.Abs(key.At-at)>.000001).ToList();
        if(keys.Count>=64){SetStatus("Đoạn đã có 64 keyframe.");return;}
        if(keys.Count==0&&at>0)keys.Add(VideoMotionKey.Default);
        keys.Add(new(at,zoom,VideoMotionX.Value/100,VideoMotionY.Value/100));
        ApplyVideoMotionEdit(motion with{Keys=keys.OrderBy(key=>key.At).ToArray()});
    }

    private void VideoMotionRemove_Click(object sender,RoutedEventArgs e)
    {
        if(_timeline is not {} timeline||!VideoCanEdit||!RequireLicense()
            ||VideoMotionKeys.SelectedItem is not MotionKeyChoice choice)return;
        var motion=timeline.Selected.Timing;
        ApplyVideoMotionEdit(motion with{Keys=motion.Keys.Where(key=>key!=choice.Key).ToArray()});
    }

    private void VideoMotionKeys_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(_timeline is not {} timeline||VideoMotionKeys.SelectedItem is not MotionKeyChoice choice)return;
        var key=choice.Key;
        var local=TimeSpan.FromTicks((long)Math.Round(timeline.Selected.MovingDuration.Ticks*key.At));
        if(key.At==1&&timeline.Selected.Timing.Hold==TimeSpan.Zero&&local.Ticks>0)local-=TimeSpan.FromTicks(1);
        SeekVideoProject(timeline.Offset(timeline.Project.SelectedClip)+local);
        VideoMotionZoom.Text=Number(key.Zoom);
        VideoMotionX.Value=100*key.X;
        VideoMotionY.Value=100*key.Y;
    }

    private void ApplyVideoMotionEdit(VideoMotion motion)
    {
        PauseVideoForEditing();
        if(_timeline?.SetMotion(motion)!=true)return;
        CommitVideoEdit();
        SetStatus("Đã cập nhật chuyển động của đoạn · Ctrl+Z hoàn tác.");
    }
}
