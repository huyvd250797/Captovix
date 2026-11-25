using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Captovix;

public sealed class VideoTimelineStrip : FrameworkElement
{
    private VideoProject? _project;
    private IReadOnlyList<List<VideoMarkTrack>> _lanes=[];
    private VideoMarkTrack[]? _laneSource;
    private int _dragClip=-1,_insertion=-1;
    private const double MainHeight=57,TrackHeight=25;
    private const int AudioLanes=4;
    private const double AudioHeight=29,MarkTop=MainHeight+AudioLanes*AudioHeight;
    private static readonly string[] AudioLabels=["Âm thanh gốc","Micro","Tiếng máy","Nhạc nền"];
    private float[][]? _audioPeaks;
    private VideoAudioLane _audioSelection;
    internal event Action<VideoAudioLane>? AudioSelected;
    internal void SetAudio(float[][]? peaks,VideoAudioLane selection){_audioPeaks=peaks;SelectAudio(selection);}
    internal void SelectAudio(VideoAudioLane selection){_audioSelection=selection;InvalidateVisual();}
    private Point _dragStart;
    private Guid? _selectedTrack;
    private VideoMarkTrack? _trackDrag,_trackDraft;
    private int _trimEdge;
    internal event Action<Guid>? TrackSelected;
    internal event Action<Guid,TimeSpan,TimeSpan>? TrackChanged;
    internal event Action<TimeSpan>? SeekRequested;
    internal event Action<int>? ClipSelected;
    internal event Action<int,int>? MoveRequested;
    internal void Show(VideoProject? project,Guid? selection)
    {
        if(!ReferenceEquals(_laneSource,project?.Tracks)){
            _laneSource=project?.Tracks;
            _lanes=VideoAnnotationLanes.Pack(_laneSource??[]);
        }
        if(ReferenceEquals(_project,project)&&_selectedTrack==selection)return;
        _project=project;
        _selectedTrack=selection;
        var height=MarkTop+TrackHeight*_lanes.Count;
        if(Height!=height)Height=height;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var bounds=new Rect(RenderSize);
        dc.DrawRoundedRectangle(Resource("Workspace"),null,bounds,5,5);
        if(_project is not {} project||ActualWidth<=2||ActualHeight<28)return;
        var duration=project.Duration.TotalSeconds;
        var foreground=Resource("Muted");
        var accent=Resource("AccentText");
        var type=new Typeface("Segoe UI");
        double width=ActualWidth-2;
        var scroll=FindScroll();
        double top=scroll?.VerticalOffset??0,bottom=top+(scroll?.ViewportHeight??ActualHeight);
        dc.PushTransform(new TranslateTransform(0,top));
        dc.DrawRectangle(Resource("Workspace"),null,new Rect(0,0,ActualWidth,MainHeight));
        double step=Math.Max(1,duration/Math.Max(1,Math.Floor(width/82)));
        double magnitude=Math.Pow(10,Math.Floor(Math.Log10(step)));
        step=(step/magnitude<=1?1:step/magnitude<=2?2:step/magnitude<=5?5:10)*magnitude;
        for(double time=0;time<duration&&time/step<350;time+=step){
            double x=1+width*time/duration;
            dc.DrawLine(new Pen(foreground,1),new(x,17),new(x,22));
            if(x<ActualWidth-52)dc.DrawText(Label(VideoTimecode.Format(TimeSpan.FromSeconds(time),false),type,foreground,10),new(x+3,2));
        }
        double left=1;
        for(int i=0;i<project.Clips.Length;i++){
            double span=width*project.Clips[i].Duration.TotalSeconds/duration;
            bool selected=i==project.SelectedClip;
            var rect=new Rect(left,24,Math.Max(0,span-1),MainHeight-27);
            dc.DrawRoundedRectangle(Resource(selected?"Soft":"Surface"),selected?new Pen(accent,1):new Pen(Resource("Line"),1),rect,3,3);
            if(span>=47)
                dc.DrawText(Label($"Đoạn {project.Clips[i].Number}",type,Resource("Text"),10),new(left+4,29));
            left+=span;
        }
        dc.Pop();
        if(bottom>top+MainHeight){
            dc.PushClip(new RectangleGeometry(new Rect(0,top+MainHeight,ActualWidth,bottom-top-MainHeight)));
            for(int lane=0;lane<AudioLanes;lane++){
                double y=MainHeight+lane*AudioHeight;
                if(y+AudioHeight<top+MainHeight||y>bottom)continue;
                var track=VideoAudioPlan.Settings(project,(VideoAudioLane)lane);
                dc.DrawRectangle(Resource(_audioSelection==(VideoAudioLane)lane?"Soft":"Surface"),null,new Rect(0,y,ActualWidth,AudioHeight-1));
                if(_audioPeaks is {} peaks){
                    var values=peaks[lane];
                    double half=AudioHeight*.42,center=y+AudioHeight/2;
                    var pen=new Pen(track.Muted?foreground:accent,1);
                    double leftEdge=scroll?.HorizontalOffset??0,rightEdge=leftEdge+(scroll?.ViewportWidth??ActualWidth);
                    int firstPeak=Math.Max(0,(int)(leftEdge/width*values.Length)-1);
                    int lastPeak=Math.Min(values.Length,(int)(rightEdge/width*values.Length)+2);
                    for(int i=firstPeak;i<lastPeak;i++){
                        double x=1+(i+.5)*width/values.Length, amplitude=values[i]*half;
                        if(amplitude>0)dc.DrawLine(pen,new(x,center-amplitude),new(x,center+amplitude));
                    }
                }
                foreach(var cut in track.Cuts){
                    double a=XAt(cut.Start),b=XAt(cut.End);
                    dc.DrawRectangle(Resource("Workspace"),new Pen(foreground,.5),new Rect(a,y+3,Math.Max(0,b-a),AudioHeight-7));
                }
                double labelX=(scroll?.HorizontalOffset??0)+4;
                string label=AudioLabels[lane]+(track.Muted?" · M":track.Solo?" · S":"");
                var text=Label(label,type,Resource("Text"),10);
                dc.DrawRoundedRectangle(Resource("Workspace"),null,new Rect(labelX,y+5,text.Width+10,17),3,3);
                dc.DrawText(text,new(labelX+5,y+7));
            }
            dc.Pop();
        }
        if(_lanes.Count>0&&bottom>top+MainHeight){
            dc.PushClip(new RectangleGeometry(new Rect(0,top+MainHeight,ActualWidth,bottom-top-MainHeight)));
            int first=Math.Clamp((int)((top+MainHeight-MarkTop)/TrackHeight),0,_lanes.Count);
            int last=Math.Clamp((int)Math.Ceiling((bottom-MarkTop)/TrackHeight)+1,first,_lanes.Count);
            for(int i=first;i<last;i++){
                var y=MarkTop+i*TrackHeight;
                dc.DrawRectangle(Resource("Workspace"),null,new Rect(0,y,ActualWidth,TrackHeight));
                dc.DrawLine(new Pen(Resource("Line"),1),new(0,y),new(ActualWidth,y));
                foreach(var item in _lanes[i]){
                var track=_trackDraft?.Id==item.Id?_trackDraft:item;
                var from=1+width*track.Start.TotalSeconds/duration;
                var to=1+width*track.End.TotalSeconds/duration;
                var bar=new Rect(from,y+3,Math.Max(1,to-from),TrackHeight-5);
                bool selected=track.Id==_selectedTrack||track.Id==_trackDraft?.Id;
                var stroke=Resource(selected?"AccentText":"Line");
                dc.DrawRoundedRectangle(Resource(selected?"Soft":"Surface"),new Pen(stroke,1),bar,4,4);
                var label=track.Mark.Label;
                if(track.Mark.Kind is VideoMarkKind.Text or VideoMarkKind.Title or VideoMarkKind.Callout&&!string.IsNullOrWhiteSpace(track.Mark.Text))
                    label+=" · "+track.Mark.Text.Replace('\n',' ').Replace('\r',' ');
                if(bar.Width>18){
                    dc.PushClip(new RectangleGeometry(bar));
                    dc.DrawText(Label(label,type,Resource("Text"),10),new(from+7,y+6));
                    if(bar.Width>22){
                        dc.DrawLine(new Pen(stroke,1),new(from+3,y+9),new(from+3,y+17));
                        dc.DrawLine(new Pen(stroke,1),new(to-3,y+9),new(to-3,y+17));
                    }
                    dc.Pop();
                }
                }
            }
            dc.Pop();
        }
        if(_insertion>=0){
            double x=1+width*project.Clips.Take(_insertion).Sum(clip=>clip.Duration.TotalSeconds)/duration;
            dc.DrawLine(new Pen(accent,3),new(x,top+24),new(x,top+MainHeight));
        }
        double cursor=1+width*project.Position.TotalSeconds/duration;
        dc.DrawLine(new Pen(accent,2),new(cursor,top+13),new(cursor,ActualHeight));
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if(_project==null)return;
        Focus();
        _dragStart=e.GetPosition(this);
        if(_dragStart.Y>=Math.Max(MainHeight,(FindScroll()?.VerticalOffset??0)+MainHeight)&&_dragStart.Y<MarkTop){
            AudioSelected?.Invoke((VideoAudioLane)((int)(_dragStart.Y-MainHeight)/AudioHeight));
            SeekAt(_dragStart.X);
            e.Handled=true;
            return;
        }
        _trackDrag=FindTrack(_dragStart);
        _trimEdge=TrackEdge(_trackDrag,_dragStart.X);
        _dragClip=IsClipRow(_dragStart.Y)?FindClip(_dragStart.X,false):-1;
        CaptureMouse();
        if(_dragClip<0&&_trackDrag==null)SeekAt(_dragStart.X);
        e.Handled=true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var point=e.GetPosition(this);
        if(!IsMouseCaptured||e.LeftButton!=MouseButtonState.Pressed){
            var hover=FindTrack(point);
            Cursor=hover==null?Cursors.Arrow:TrackEdge(hover,point.X)==0?Cursors.SizeAll:Cursors.SizeWE;
            var tip=hover==null?null:$"{hover.Mark.Label} · {VideoTimecode.Format(hover.Start)} – {VideoTimecode.Format(hover.End)}";
            if(tip==null&&IsClipRow(point.Y)&&_project is {} preview&&FindClip(point.X,false) is >=0 and var clip)
                tip=$"Đoạn {preview.Clips[clip].Number}";
            if(!Equals(ToolTip,tip))ToolTip=tip;
            return;
        }
        if(_trackDrag is {} track&&_project is {} project){
            if(_trackDraft==null&&Math.Abs(point.X-_dragStart.X)<SystemParameters.MinimumHorizontalDragDistance)return;
            AutoScroll(e);
            var change=TimeSpan.FromTicks((long)((point.X-_dragStart.X)*project.Duration.Ticks/Math.Max(1,ActualWidth-2)));
            var minimum=TimeSpan.FromTicks(Math.Min(track.Length.Ticks,TimeSpan.TicksPerMillisecond*33));
            _trackDraft=_trimEdge switch{
                -1=>track with{Start=Clamp(track.Start+change,TimeSpan.Zero,track.End-minimum)},
                1=>track with{End=Clamp(track.End+change,track.Start+minimum,project.Duration)},
                _=>MoveTrack(track,change,project.Duration)
            };
            InvalidateVisual();
            return;
        }
        if(_dragClip<0){SeekAt(point.X);return;}
        if(_insertion>=0||Math.Abs(point.X-_dragStart.X)>=SystemParameters.MinimumHorizontalDragDistance){
            AutoScroll(e);
            _insertion=FindClip(point.X,true);
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if(!IsMouseCaptured)return;
        var (clip,insertion,track,draft)=(_dragClip,_insertion,_trackDrag,_trackDraft);
        ReleaseMouseCapture();
        if(draft!=null)TrackChanged?.Invoke(draft.Id,draft.Start,draft.End);
        else if(track!=null)TrackSelected?.Invoke(track.Id);
        else if(clip<0)SeekAt(e.GetPosition(this).X);
        else if(insertion<0)ClipSelected?.Invoke(clip);
        else MoveRequested?.Invoke(clip,insertion);
        e.Handled=true;
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        _dragClip=_insertion=-1;
        _trackDrag=_trackDraft=null;
        InvalidateVisual();
        base.OnLostMouseCapture(e);
    }

    private ScrollViewer? FindScroll()
    {
        var node=VisualTreeHelper.GetParent(this);
        while(node!=null&&node is not ScrollViewer)node=VisualTreeHelper.GetParent(node);
        return node as ScrollViewer;
    }

    private bool IsClipRow(double y)
    {
        double top=FindScroll()?.VerticalOffset??0;
        return y>=top+24&&y<top+MainHeight;
    }

    private void AutoScroll(MouseEventArgs e)
    {
        if(FindScroll() is not {} scroll||scroll.ScrollableWidth<=0)return;
        double x=e.GetPosition(scroll).X;
        if(x<20)scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset-24);
        else if(x>scroll.ViewportWidth-20)scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset+24);
    }

    private VideoMarkTrack? FindTrack(Point point)
    {
        if(_project is not {} project||point.Y<Math.Max(MarkTop,(FindScroll()?.VerticalOffset??0)+MainHeight))return null;
        int row=(int)((point.Y-MarkTop)/TrackHeight);
        if(row>=_lanes.Count)return null;
        return _lanes[row].FirstOrDefault(track=>point.X>=XAt(track.Start)&&point.X<XAt(track.End))
            ??_lanes[row].FirstOrDefault(track=>point.X>=XAt(track.Start)-3&&point.X<=XAt(track.End)+3);
    }

    private int TrackEdge(VideoMarkTrack? track,double x)
    {
        if(track==null)return 0;
        double left=XAt(track.Start),right=XAt(track.End);
        double edge=Math.Min(8,(right-left)*.3);
        return x<=left+edge?-1:x>=right-edge?1:0;
    }

    private double XAt(TimeSpan time)=>1+(ActualWidth-2)*time.Ticks/_project!.Duration.Ticks;
    private static TimeSpan Clamp(TimeSpan value,TimeSpan min,TimeSpan max)=>value<min?min:value>max?max:value;
    private static VideoMarkTrack MoveTrack(VideoMarkTrack track,TimeSpan delta,TimeSpan duration)
    {
        var start=Clamp(track.Start+delta,TimeSpan.Zero,duration-track.Length);
        return track with{Start=start,End=start+track.Length};
    }

    private int FindClip(double x,bool insertion)
    {
        if(_project is not {} project||ActualWidth<=2)return -1;
        double time=project.Duration.TotalSeconds*Math.Clamp((x-1)/(ActualWidth-2),0,1);
        double offset=0;
        for(int i=0;i<project.Clips.Length;i++){
            var length=project.Clips[i].Duration.TotalSeconds;
            if(time<offset+length*(insertion ? .5 : 1))return i;
            offset+=length;
        }
        return insertion?project.Clips.Length:project.Clips.Length-1;
    }

    internal void RefreshScroll()=>InvalidateVisual();

    private void SeekAt(double x)
    {
        if(_project is {} project&&ActualWidth>2)
            SeekRequested?.Invoke(TimeSpan.FromTicks((long)(project.Duration.Ticks*Math.Clamp((x-1)/(ActualWidth-2),0,1))));
    }

    private Brush Resource(string name)=>TryFindResource(name) as Brush??Brushes.Gray;
    private FormattedText Label(string text,Typeface type,Brush brush,double size)=>
        new(text,CultureInfo.CurrentUICulture,FlowDirection.LeftToRight,type,size,brush,VisualTreeHelper.GetDpi(this).PixelsPerDip);
}

internal static class VideoTimecode
{
    internal static string Format(TimeSpan time,bool milliseconds=true)=>
        $"{(long)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}"+(milliseconds?$".{time.Milliseconds:000}":"");
}
