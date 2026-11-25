using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Captovix;

public partial class MainWindow
{
    private VideoCueSpan[] _cueSpans=[];
    private VideoRange[]? _cueClips;
    private VideoCue[]? _cueSource;
    private bool _pickVideoCue;
    private readonly Canvas _cueLayer=new(){IsHitTestVisible=false};

    private VideoCueSpan[] VideoCueSpans(VideoProject project)
    {
        if(!ReferenceEquals(_cueClips,project.Clips)||!ReferenceEquals(_cueSource,project.Cues)){
            _cueClips=project.Clips;
            _cueSource=project.Cues;
            _cueSpans=VideoCuePlan.Create(project.Clips,project.Cues);
        }
        return _cueSpans;
    }

    private async void ImportVideoCues_Click(object sender,RoutedEventArgs e)
    {
        if(!_videoStudio||!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        var project=timeline.Project;
        var markers=_videoItem?.RecordingCues??[];
        try{
            var cues=await Task.Run(async ()=>{
                var items=new List<VideoCue>();
                for(int index=0;index<project.AdditionalSources.Length+1;index++){
                    var input=project.Input(index);
                    string path=RecordingPresenter.PathFor(input.Path);
                    if(!File.Exists(path)||new FileInfo(path).Length>4_000_000)continue;
                    var size=await VideoTrimExport.ReadDimensionsAsync(input.Path);
                    var events=JsonSerializer.Deserialize<RecordingCue[]>(File.ReadAllText(path))??[];
                    foreach(var cue in events.OfType<RecordingCue>().Take(20000)){
                        if(cue.Milliseconds<0||cue.Milliseconds>=input.Duration.TotalMilliseconds||cue.X is not int x||cue.Y is not int y)continue;
                        var candidate=new VideoCue(index,TimeSpan.FromMilliseconds(cue.Milliseconds),cue.Kind,x/(double)size.Width,y/(double)size.Height);
                        if(candidate.IsValid(project.AdditionalSources.Length+1))items.Add(candidate);
                    }
                }
                foreach(var marker in markers.OfType<RecordingCue>().Where(m=>m.Kind=="marker"&&m.Milliseconds>=0&&m.Milliseconds<project.SourceDuration.TotalMilliseconds))
                    items.Add(new(0,TimeSpan.FromMilliseconds(marker.Milliseconds),"marker"));
                return items;
            });
            if(!_videoStudio||!ReferenceEquals(_timeline,timeline)||!ReferenceEquals(timeline.Project.AdditionalSources,project.AdditionalSources)||!VideoCanEdit)return;
            var merged=timeline.Project.Cues.Concat(cues).Distinct().ToArray();
            if(merged.Length>22000){SetStatus("Dự án vượt giới hạn 22.000 chỉ dẫn.");return;}
            PauseVideoForEditing();
            if(timeline.SetCues(merged,true))CommitVideoEdit();
            SetStatus(cues.Count==0?"Nguồn chưa có sự kiện ghi sẵn. Có thể thêm chỉ dẫn thủ công.":$"Đã đọc {cues.Count} sự kiện và marker từ bản quay.");
        }catch(Exception error){App.Log(error);SetStatus("Chưa nhập được chỉ dẫn: "+error.Message);}
    }

    private void VideoCueAdd_Click(object sender,RoutedEventArgs e)
    {
        if(!_videoStudio||!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        PauseVideoForEditing();
        if(VideoCueKind.SelectedIndex<2){
            SetVideoTool(null);
            _pickVideoCue=true;
            VideoOverlay.Cursor=Cursors.Cross;
            SetStatus("Bấm vị trí trên khung hình để đặt chỉ dẫn.");
        }else{
            string kind=VideoCueKind.SelectedIndex==2?"key":"marker";
            string text=kind=="key"?VideoCueText.Text.Trim():"";
            if(kind=="key"&&text.Length==0){SetStatus("Nhập tên phím hoặc tổ hợp, ví dụ Ctrl+S.");return;}
            AddVideoCue(timeline,kind,.5,.5,text);
        }
    }

    private bool PlaceVideoCue(Point point)
    {
        if(!_pickVideoCue||_timeline is not {} timeline)return false;
        _pickVideoCue=false;
        VideoOverlay.Cursor=Cursors.Arrow;
        var inverse=_videoImageTransform;
        if(!inverse.HasInverse)return true;
        inverse.Invert();
        var source=inverse.Transform(new Point(point.X-_videoBounds.X,point.Y-_videoBounds.Y));
        if(VideoCropSurface.Clip is RectangleGeometry crop&&!crop.Rect.Contains(source)){
            SetStatus("Chọn một điểm trong phần hình video đang hiển thị.");return true;
        }
        int width=VideoPreview.NaturalVideoWidth,height=VideoPreview.NaturalVideoHeight;
        if(width>0&&height>0&&source.X>=0&&source.X<width&&source.Y>=0&&source.Y<height)
            AddVideoCue(timeline,VideoCueKind.SelectedIndex==1?"ring":"left",source.X/width,source.Y/height,"");
        return true;
    }

    private void AddVideoCue(VideoTimeline timeline,string kind,double x,double y,string text)
    {
        var (index,source)=timeline.Map(timeline.Project.Position);
        var clip=timeline.Project.Clips[index];
        if(source>=clip.End)source=LastVideoFrame(clip);
        var cue=new VideoCue(clip.SourceIndex,source,kind,x,y,text);
        if(timeline.SetCues([..timeline.Project.Cues,cue],true))CommitVideoEdit();
    }

    private void ToggleVideoCues_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        PauseVideoForEditing();
        if(timeline.SetCues(timeline.Project.Cues,VideoCueVisible.IsChecked==true))CommitVideoEdit();
    }

    private void RemoveVideoCue_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        PauseVideoForEditing();
        var span=VideoCueSpans(timeline.Project).LastOrDefault(s=>s.Start<=timeline.Project.Position&&s.End>timeline.Project.Position);
        if(span!=null&&timeline.SetCues(timeline.Project.Cues.Where(c=>c!=span.Cue).ToArray(),timeline.Project.ShowCues))CommitVideoEdit();
    }

    private void UpdateVideoCues(Rect bounds)
    {
        if(_cueLayer.Children.Count>0)_cueLayer.Children.Clear();
        if(bounds.IsEmpty||_timeline?.Project is not {ShowCues:true} project||project.Cues.Length==0)return;
        var position=project.Position;
        var spans=VideoCueSpans(project);
        int start=Array.BinarySearch(spans,new VideoCueSpan(null!,default,default,position-TimeSpan.FromSeconds(1.25),default),
            Comparer<VideoCueSpan>.Create((a,b)=>a.Start.CompareTo(b.Start)));
        if(start<0)start=~start;
        var (width,height)=_videoDecodedSize;
        for(int i=start;i<spans.Length&&spans[i].Start<=position;i++){
            var span=spans[i];
            if(span.End<=position||span.Cue.Kind=="marker")continue;
            FrameworkElement item;
            double x,y;
            if(span.Cue.Kind=="key"){
                item=new Border{Child=new TextBlock{Text=span.Cue.Text,Foreground=Brushes.White,FontWeight=FontWeights.Bold,
                    FontSize=Math.Max(8,bounds.Height/25)},Background=Brushes.Black,CornerRadius=new(4),Padding=new(6,2,6,2)};
                item.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
                x=bounds.Width/2-item.DesiredSize.Width/2;y=bounds.Height*.9-item.DesiredSize.Height/2;
            }else{
                var point=_videoImageTransform.Transform(new Point(span.Cue.X*width,span.Cue.Y*height));
                if(point.X<0||point.X>bounds.Width||point.Y<0||point.Y>bounds.Height)continue;
                if(VideoCropSurface.Clip is RectangleGeometry crop&&!crop.Rect.Contains(new Point(span.Cue.X*width,span.Cue.Y*height)))continue;
                double radius=bounds.Height/38;
                item=new Ellipse{Width=2*radius,Height=2*radius,Stroke=EditorRenderer.Ink(span.Cue.Color),StrokeThickness=Math.Max(1,radius/6)};
                x=point.X-radius;y=point.Y-radius;
            }
            Canvas.SetLeft(item,bounds.X+x);Canvas.SetTop(item,bounds.Y+y);
            _cueLayer.Children.Add(item);
        }
    }
}
