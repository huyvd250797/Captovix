using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Captovix;

public partial class MainWindow
{
    private (int Width,int Height)? _videoSourceSize;
    private Guid? _selectedVideoTrack;
    private VideoMarkTrack? SelectedVideoTrack=>_timeline?.Project.Tracks.FirstOrDefault(track=>track.Id==_selectedVideoTrack);
    private (VideoCanvasLayout Canvas,VideoClipLayout Layout,int Clip)? _videoInspectorLayout;
    private VideoMarkTrack? _videoInspectorTrack;
    private bool _syncVideoInspector;
    private string? _videoTool;
    private Point? _videoDragStart;
    private readonly List<VideoInkPoint> _videoInkPoints=[];
    private VideoMarkTrack? _videoDragTrack;
    private int _videoDragHandle;
    private Rect _videoBounds=Rect.Empty;
    private Matrix _videoImageTransform;
    private object? _videoCanvasKey;
    private VideoMarkTrack[]? _previousVideoTracks;
    private TimeSpan _activeTrackBoundary;
    private TimeSpan _lastTrackPosition;
    private Guid? _previousTrackSelection;
    private Rect _previousMarkBounds=Rect.Empty;
    private bool _previousPlaying;
    private Rectangle? _videoGuide;
    private TextBox? _inlineVideoText;
    private Guid? _inlineVideoTrack;

    private async void ReadPrimaryVideoSize(VideoTimeline? timeline)
    {
        if(timeline==null||!System.IO.File.Exists(timeline.Project.SourcePath))return;
        try{
            var size=await VideoTrimExport.ReadDimensionsAsync(timeline.Project.SourcePath);
            if(!ReferenceEquals(timeline,_timeline)||_videoSourceSize!=null)return;
            _videoSourceSize=size;
            UpdateVideoMarks();
        }catch(Exception error){App.Log(error);}
    }

    private void SetVideoTool(string? tool)
    {
        CancelVideoInlineText();
        _videoTool=tool;
        ClearVideoDrag();
        if(VideoOverlay==null)return;
        VideoOverlay.Cursor=tool==null?Cursors.Arrow:Cursors.Cross;
        foreach(var button in VideoMarkTools.Children.OfType<Button>())
            button.SetResourceReference(Control.BackgroundProperty,(string?)button.Tag==(tool??"Select")?"Soft":"Surface");
    }

    private void VideoTool_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||!_videoReady||sender is not Button{Tag:string tool})return;
        PauseVideoForEditing();
        if(tool!="Select")ClearVideoTrackSelection();
        SetVideoTool(tool=="Select"||tool==_videoTool?null:tool);
    }

    private void VideoOverlay_SizeChanged(object sender,SizeChangedEventArgs e)=>UpdateVideoMarks();

    private Rect UpdateVideoCanvas()
    {
        if(!_videoReady||_timeline is not {} timeline||VideoOverlay.ActualWidth<=0||VideoOverlay.ActualHeight<=0)
        {
            VideoCanvasSurface.Visibility=Visibility.Hidden;
            return Rect.Empty;
        }
        int sourceWidth=VideoPreview.NaturalVideoWidth,sourceHeight=VideoPreview.NaturalVideoHeight;
        if(sourceWidth<=0||sourceHeight<=0)return Rect.Empty;
        var canvas=timeline.Project.Canvas;
        var layout=timeline.Selected.Layout??VideoClipLayout.Default;
        var primary=_videoSourceSize??(sourceWidth,sourceHeight);
        var (width,height)=canvas.OutputSize(primary.Width,primary.Height);
        double scale=Math.Min(VideoOverlay.ActualWidth/width,VideoOverlay.ActualHeight/height);
        var bounds=new Rect((VideoOverlay.ActualWidth-width*scale)/2,(VideoOverlay.ActualHeight-height*scale)/2,width*scale,height*scale);
        var key=(canvas,layout,sourceWidth,sourceHeight,width,height,bounds);
        VideoCanvasSurface.Visibility=Visibility.Visible;
        if(Equals(_videoCanvasKey,key))return bounds;
        _videoCanvasKey=key;
        var geometry=VideoCanvasGeometry.Create(canvas,layout,sourceWidth,sourceHeight,width,height);
        var (crop,rotation,image)=geometry;
        var matrix=Matrix.Identity;
        matrix.Translate(-crop.X,-crop.Y);
        matrix.Rotate(rotation);
        matrix.Translate(rotation is 90?crop.Height:rotation is 180?crop.Width:0,
            rotation is 180?crop.Height:rotation is 270?crop.Width:0);
        bool sideways=rotation is 90 or 270;
        matrix.Scale(image.Width*scale/(sideways?crop.Height:crop.Width),image.Height*scale/(sideways?crop.Width:crop.Height));
        matrix.Translate(image.X*scale,image.Y*scale);
        _videoImageTransform=matrix;
        VideoCanvasSurface.Width=bounds.Width;
        VideoCanvasSurface.Height=bounds.Height;
        VideoCanvasSurface.Background=new SolidColorBrush(ParseVideoColor(canvas.Background));
        VideoCanvasSurface.HorizontalAlignment=HorizontalAlignment.Center;
        VideoCanvasSurface.VerticalAlignment=VerticalAlignment.Center;
        VideoCropSurface.Width=VideoPreview.Width=sourceWidth;
        VideoCropSurface.Height=VideoPreview.Height=sourceHeight;
        VideoCropSurface.Clip=new RectangleGeometry(new Rect(crop.X,crop.Y,crop.Width,crop.Height));
        VideoCropSurface.RenderTransform=new MatrixTransform(matrix);
        return bounds;
    }

    private void UpdateVideoMarks()
    {
        if(VideoOverlay==null)return;
        var bounds=UpdateVideoCanvas();
        _videoBounds=bounds;
        var project=_timeline?.Project;
        var tracks=project?.Tracks;
        var position=project?.Position??TimeSpan.Zero;
        VideoOverlay.IsHitTestVisible=_videoStudio&&VideoCanEdit&&_videoReady&&_videoFullscreen==null;
        if(ReferenceEquals(tracks,_previousVideoTracks)&&bounds==_previousMarkBounds&&_selectedVideoTrack==_previousTrackSelection
            &&_playing==_previousPlaying&&position>=_lastTrackPosition&&position<_activeTrackBoundary)return;
        _lastTrackPosition=position;
        _previousVideoTracks=tracks;
        _previousMarkBounds=bounds;
        _previousTrackSelection=_selectedVideoTrack;
        _previousPlaying=_playing;
        _activeTrackBoundary=TimeSpan.MaxValue;
        for(int i=VideoOverlay.Children.Count-1;i>=0;i--)
            if(VideoOverlay.Children[i]!=_inlineVideoText)VideoOverlay.Children.RemoveAt(i);
        _videoGuide=null;
        if(bounds.IsEmpty||tracks==null)return;
        var marks=new List<VideoMark>();
        foreach(var track in tracks){
            if(track.IsActive(position))marks.Add(track.Mark);
            if(track.Start>position&&track.Start<_activeTrackBoundary)_activeTrackBoundary=track.Start;
            if(track.End>position&&track.End<_activeTrackBoundary)_activeTrackBoundary=track.End;
        }
        foreach(var mark in marks.Where(mark=>mark.SamplesVideo&&mark.Style.Opacity>0)){
            var region=VideoMarkRendering.Region(mark,bounds.Width,bounds.Height);
            if(region.Width<1||region.Height<1)continue;
            region.Offset(bounds.X,bounds.Y);
            var rect=new Rectangle{Width=region.Width,Height=region.Height,IsHitTestVisible=false,Opacity=mark.Style.Opacity/100};
            if(mark.Kind==VideoMarkKind.Blur&&mark.Style.Effect==2){
                rect.Fill=EditorRenderer.Ink(mark.Color??"#F54856");
                Canvas.SetLeft(rect,region.X);Canvas.SetTop(rect,region.Y);
                VideoOverlay.Children.Add(rect);
                continue;
            }
            if(mark.Kind==VideoMarkKind.Magnifier){
                double zoom=mark.Style.Zoom;
                var sample=new Rect((region.X-bounds.X+(1-1/zoom)*region.Width/2)/bounds.Width,
                    (region.Y-bounds.Y+(1-1/zoom)*region.Height/2)/bounds.Height,region.Width/bounds.Width/zoom,region.Height/bounds.Height/zoom);
                rect.Fill=new VisualBrush(VideoCanvasSurface){Viewbox=sample,ViewboxUnits=BrushMappingMode.RelativeToBoundingBox,Stretch=Stretch.Fill};
                Canvas.SetLeft(rect,region.X);Canvas.SetTop(rect,region.Y);
                VideoOverlay.Children.Add(rect);
                continue;
            }
            var layer=new Canvas{Width=VideoOverlay.ActualWidth,Height=VideoOverlay.ActualHeight,
                IsHitTestVisible=false,Clip=new RectangleGeometry(region)};
            var pixels=new Rectangle{Width=bounds.Width,Height=bounds.Height,IsHitTestVisible=false,Opacity=mark.Style.Opacity/100,
                Fill=new VisualBrush(VideoCanvasSurface){Stretch=Stretch.Fill}};
            if(mark.Style.Effect==1){
                pixels.CacheMode=new BitmapCache(1d/Math.Max(2,mark.Style.Strength/2d));
                RenderOptions.SetBitmapScalingMode(pixels,BitmapScalingMode.NearestNeighbor);
            }else pixels.Effect=new BlurEffect{Radius=Math.Min(Math.Min(region.Width,region.Height)/2,mark.Style.Strength/2d*bounds.Height/540)};
            Canvas.SetLeft(pixels,bounds.X);Canvas.SetTop(pixels,bounds.Y);
            layer.Children.Add(pixels);
            VideoOverlay.Children.Add(layer);
        }
        var image=VideoMarkRendering.Draw([..marks],bounds.Width,bounds.Height);
        if(image!=null){
            var ink=new Image{Source=image,Width=bounds.Width,Height=bounds.Height,Stretch=Stretch.Fill,IsHitTestVisible=false};
            Canvas.SetLeft(ink,bounds.X);Canvas.SetTop(ink,bounds.Y);
            VideoOverlay.Children.Add(ink);
        }
        if(_videoStudio&&!_playing&&_videoFullscreen==null&&SelectedVideoTrack is {} selected&&selected.IsActive(position)){
            var box=VideoMarkHitBox(selected.Mark);
            AddVideoOutline(box);
            if(HasVideoHandles(selected.Mark.Kind)){
                DrawVideoHandle(VideoPoint(selected.Mark.X1,selected.Mark.Y1));
                DrawVideoHandle(VideoPoint(selected.Mark.X2,selected.Mark.Y2));
            }
        }
    }

    private static bool HasVideoHandles(VideoMarkKind kind)=>kind is VideoMarkKind.Frame or VideoMarkKind.Blur or VideoMarkKind.Arrow or VideoMarkKind.Callout or VideoMarkKind.Highlight or VideoMarkKind.Spotlight or VideoMarkKind.Magnifier;

    private void DrawVideoHandle(Point point)
    {
        var handle=new Rectangle{Width=7,Height=7,Fill=Brushes.White,Stroke=Brushes.DeepSkyBlue,IsHitTestVisible=false};
        Canvas.SetLeft(handle,point.X-3.5);Canvas.SetTop(handle,point.Y-3.5);
        VideoOverlay.Children.Add(handle);
    }

    private Rectangle AddVideoOutline(Rect box)
    {
        var outline=new Rectangle{Width=box.Width,Height=box.Height,Stroke=Brushes.DeepSkyBlue,StrokeThickness=1.25,
            StrokeDashArray=new DoubleCollection{4,3},IsHitTestVisible=false};
        Canvas.SetLeft(outline,box.X);Canvas.SetTop(outline,box.Y);
        VideoOverlay.Children.Add(outline);
        return outline;
    }

    private Rect VideoMarkHitBox(VideoMark mark)
    {
        var first=VideoPoint(mark.X1,mark.Y1);
        var second=VideoPoint(mark.X2,mark.Y2);
        double scale=_videoBounds.Height/540;
        var box=mark.Kind switch{
            VideoMarkKind.Step=>new Rect(first.X-mark.FontSize*scale,first.Y-mark.FontSize*scale,mark.FontSize*scale*2,mark.FontSize*scale*2),
            VideoMarkKind.Text=>new Rect(first.X,first.Y,Math.Max(16,Math.Min(_videoBounds.Right-first.X,
                mark.FontSize*scale*(Math.Max(1,mark.Text.Split('\n').Max(line=>line.Length))*.63+1))),
                mark.FontSize*scale*(mark.Text.Count(c=>c=='\n')+1)*1.4+6),
            _=>new Rect(first,second)
        };
        box.Inflate(6,6);
        return box;
    }

    private Point VideoPoint(double x,double y)=>new(_videoBounds.X+x*_videoBounds.Width,_videoBounds.Y+y*_videoBounds.Height);
    private Point VideoNormalized(Point point)=>new(Math.Clamp((point.X-_videoBounds.X)/_videoBounds.Width,0,1),
        Math.Clamp((point.Y-_videoBounds.Y)/_videoBounds.Height,0,1));

    private void VideoMark_Down(object sender,MouseButtonEventArgs e)
    {
        var point=e.GetPosition(VideoOverlay);
        if(_inlineVideoText!=null&&e.OriginalSource is DependencyObject original
            &&(original==_inlineVideoText||_inlineVideoText.IsAncestorOf(original)))return;
        if(!VideoCanEdit||!_videoBounds.Contains(point)||!RequireLicense()||_timeline is not {} timeline)return;
        CancelVideoInlineText();
        PauseVideoForEditing();
        if(_videoTool==null){
            var selected=SelectedVideoTrack;
            var active=selected!=null&&selected.IsActive(timeline.Project.Position)&&VideoMarkHitBox(selected.Mark).Contains(point)?selected:
                timeline.Project.Tracks.LastOrDefault(track=>track.IsActive(timeline.Project.Position)&&VideoMarkHitBox(track.Mark).Contains(point));
            if(active==null){ClearVideoTrackSelection();return;}
            SetSelectedVideoTrack(active.Id);
            if(e.ClickCount==2&&active.Mark.Kind is VideoMarkKind.Text or VideoMarkKind.Callout or VideoMarkKind.Step){
                EditVideoInlineText(active);
                e.Handled=true;
                return;
            }
            _videoDragTrack=active;
            var mark=active.Mark;
            if(HasVideoHandles(mark.Kind)){
                if((VideoPoint(mark.X1,mark.Y1)-point).Length<=9)_videoDragHandle=1;
                else if((VideoPoint(mark.X2,mark.Y2)-point).Length<=9)_videoDragHandle=2;
            }
        }
        _videoDragStart=point;
        if(_videoTool=="Freehand"){
            var at=VideoNormalized(point);
            _videoInkPoints.Add(new(at.X,at.Y));
        }
        VideoOverlay.CaptureMouse();
        e.Handled=true;
    }

    private VideoMark MovedVideoMark(VideoMark mark,Point from,Point to)
    {
        var first=VideoNormalized(from);
        var last=VideoNormalized(to);
        if(_videoDragHandle==1)return mark with{X1=last.X,Y1=last.Y};
        if(_videoDragHandle==2)return mark with{X2=last.X,Y2=last.Y};
        double x=Math.Clamp(last.X-first.X,-Math.Min(mark.X1,mark.X2),1-Math.Max(mark.X1,mark.X2));
        double y=Math.Clamp(last.Y-first.Y,-Math.Min(mark.Y1,mark.Y2),1-Math.Max(mark.Y1,mark.Y2));
        return mark with{X1=mark.X1+x,X2=mark.X2+x,Y1=mark.Y1+y,Y2=mark.Y2+y,
            Points=mark.Points?.Select(point=>new VideoInkPoint(Math.Clamp(point.X+x,0,1),Math.Clamp(point.Y+y,0,1))).ToArray()};
    }

    private void VideoMark_Move(object sender,MouseEventArgs e)
    {
        if(_videoDragStart is not {} start||!VideoOverlay.IsMouseCaptured||_videoBounds.IsEmpty)return;
        var end=VideoNormalized(e.GetPosition(VideoOverlay));
        var point=VideoPoint(end.X,end.Y);
        if(_videoTool=="Freehand"&&_videoInkPoints.Count<2048){
            var last=_videoInkPoints[^1];
            var previous=VideoPoint(last.X,last.Y);
            if((point-previous).Length>=2){
                _videoInkPoints.Add(new(end.X,end.Y));
                VideoOverlay.Children.Add(new Line{X1=previous.X,Y1=previous.Y,X2=point.X,Y2=point.Y,Stroke=Brushes.DeepSkyBlue,
                    StrokeThickness=2,IsHitTestVisible=false,Tag="FreehandDraft"});
            }
            return;
        }
        var box=_videoDragTrack is {} track?VideoMarkHitBox(MovedVideoMark(track.Mark,start,point)):new Rect(start,point);
        if(_videoGuide!=null)VideoOverlay?.Children.Remove(_videoGuide);
        _videoGuide=AddVideoOutline(box);
    }

    private void VideoMark_Up(object sender,MouseButtonEventArgs e)
    {
        if(_videoDragStart is not {} start)return;
        var end=e.GetPosition(VideoOverlay);
        var track=_videoDragTrack;
        var moved=track==null||_videoBounds.IsEmpty?null:MovedVideoMark(track.Mark,start,end);
        var points=_videoInkPoints.ToArray();
        ClearVideoDrag();
        if(!VideoCanEdit||_timeline is not {} timeline||_videoBounds.IsEmpty)return;
        e.Handled=true;
        if(track!=null){
            if(moved!=null&&(end-start).Length>=2&&timeline.SetTrack(track with{Mark=moved}))CommitVideoEdit();
            return;
        }
        if(_videoTool=="Crop"){CropVideoByDrag(start,end);return;}
        var name=_videoTool=="Shape"?"Frame":_videoTool;
        if(!Enum.TryParse<VideoMarkKind>(name,out var kind))return;
        var first=VideoNormalized(start);
        var last=VideoNormalized(end);
        if(kind is (VideoMarkKind.Frame or VideoMarkKind.Blur or VideoMarkKind.Highlight or VideoMarkKind.Spotlight or VideoMarkKind.Magnifier)&&(Math.Abs(end.X-start.X)<5||Math.Abs(end.Y-start.Y)<5))return;
        if(kind==VideoMarkKind.Arrow&&(end-start).Length<5)return;
        if(kind==VideoMarkKind.Callout&&(last.X-first.X<.03||last.Y-first.Y<.03)){
            last=new(Math.Min(1,first.X+.24),Math.Min(1,first.Y+.13));
            first=new(Math.Min(first.X,last.X-.12),Math.Min(first.Y,last.Y-.08));
        }
        if(kind is VideoMarkKind.Step or VideoMarkKind.Text)last=first;
        if(kind==VideoMarkKind.Freehand&&points.Length>0){
            first=new(points.Min(p=>p.X),points.Min(p=>p.Y));
            last=new(points.Max(p=>p.X),points.Max(p=>p.Y));
        }
        var position=timeline.Project.Position;
        if(position>=timeline.Project.Duration)position=timeline.Project.Duration-TimeSpan.FromMilliseconds(1);
        if(position<TimeSpan.Zero)position=TimeSpan.Zero;
        var stop=position+TimeSpan.FromSeconds(3);
        if(stop>timeline.Project.Duration)stop=timeline.Project.Duration;
        int number=kind==VideoMarkKind.Step?1+timeline.Project.Tracks.Count(item=>item.Mark.Kind==VideoMarkKind.Step):0;
        string text=kind switch{VideoMarkKind.Text=>"Nội dung",VideoMarkKind.Callout=>"Chú thích",_=>""};
        var mark=new VideoMark(kind,first.X,first.Y,last.X,last.Y,text,number){Points=points.Length>0?points:null};
        if(kind==VideoMarkKind.Spotlight)mark=mark with{Color="#000000"};
        var newTrack=new VideoMarkTrack(Guid.NewGuid(),position,stop,mark);
        if(!timeline.SetTrack(newTrack)){SetStatus("Timeline đã đạt giới hạn chú thích.");return;}
        if(position!=timeline.Project.Position)timeline.Seek(position);
        _selectedVideoTrack=newTrack.Id;
        SetVideoTool(null);
        CommitVideoEdit();
        VideoStripScroll.UpdateLayout();
        VideoStripScroll.ScrollToVerticalOffset(Math.Max(0,VideoStrip.Height-VideoStripScroll.ViewportHeight));
        if(kind is VideoMarkKind.Text or VideoMarkKind.Callout)EditVideoInlineText(newTrack);
    }

    private void EditVideoInlineText(VideoMarkTrack track)
    {
        CancelVideoInlineText();
        if(_videoBounds.IsEmpty)return;
        var mark=track.Mark;
        var bounds=VideoMarkHitBox(mark);
        var input=new TextBox{
            Text=mark.Kind==VideoMarkKind.Step?mark.StepLabel??mark.Number.ToString(CultureInfo.CurrentCulture):mark.Text,
            MaxLength=mark.Kind==VideoMarkKind.Step?12:500,AcceptsReturn=mark.Kind!=VideoMarkKind.Step,
            Width=Math.Max(100,Math.Min(_videoBounds.Width,Math.Max(bounds.Width,160))),
            MinHeight=28,MaxHeight=Math.Max(36,_videoBounds.Height*.45),
            TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,
            ToolTip=mark.Kind==VideoMarkKind.Step?"Enter lưu · Esc hủy":"Ctrl+Enter lưu · Esc hủy"
        };
        input.PreviewKeyDown+=VideoInlineText_KeyDown;
        input.LostKeyboardFocus+=VideoInlineText_LostFocus;
        _inlineVideoTrack=track.Id;
        _inlineVideoText=input;
        Panel.SetZIndex(input,10);
        Canvas.SetLeft(input,Math.Max(_videoBounds.Left,Math.Min(bounds.Left,_videoBounds.Right-input.Width)));
        Canvas.SetTop(input,Math.Max(_videoBounds.Top,Math.Min(bounds.Top,_videoBounds.Bottom-36)));
        VideoOverlay.Children.Add(input);
        input.Focus();
        input.SelectAll();
    }

    private void VideoInlineText_KeyDown(object sender,KeyEventArgs e)
    {
        if(e.Key==Key.Escape){
            CancelVideoInlineText();
            VideoStrip.Focus();
        }else if(e.Key==Key.Enter&&(_inlineVideoText?.AcceptsReturn==false||Keyboard.Modifiers.HasFlag(ModifierKeys.Control))){
            if(CommitVideoInlineText())VideoStrip.Focus();
        }else return;
        e.Handled=true;
    }

    private void VideoInlineText_LostFocus(object sender,KeyboardFocusChangedEventArgs e)
    {
        if(!CommitVideoInlineText())CancelVideoInlineText();
    }

    private bool CommitVideoInlineText()
    {
        if(_inlineVideoText is not {} input)return true;
        if(!VideoCanEdit||_timeline is not {} timeline
            ||timeline.Project.Tracks.FirstOrDefault(track=>track.Id==_inlineVideoTrack) is not {} track){
            CancelVideoInlineText();
            return true;
        }
        var mark=track.Mark;
        if(mark.Kind==VideoMarkKind.Step){
            if(!TextStudio.TryStepNumber(input.Text,out var step,out bool tenths)){
                SetStatus("Nhập số Bước hợp lệ, ví dụ 1 hoặc 1.2.");return false;
            }
            mark=mark with{StepLabel=step.ToString(tenths?"0.0":"0",CultureInfo.InvariantCulture)};
        }else mark=mark with{Text=input.Text};
        CancelVideoInlineText();
        if(timeline.SetTrack(track with{Mark=mark}))CommitVideoEdit();
        return true;
    }

    private void CancelVideoInlineText()
    {
        if(_inlineVideoText is not {} input)return;
        _inlineVideoText=null;
        _inlineVideoTrack=null;
        input.LostKeyboardFocus-=VideoInlineText_LostFocus;
        input.PreviewKeyDown-=VideoInlineText_KeyDown;
        VideoOverlay.Children.Remove(input);
    }

    private void VideoOverlay_LostCapture(object sender,MouseEventArgs e)=>ClearVideoDrag();

    private void ClearVideoDrag()
    {
        _videoDragStart=null;
        _videoInkPoints.Clear();
        if(VideoOverlay!=null)
            foreach(var draft in VideoOverlay.Children.OfType<Line>().Where(line=>Equals(line.Tag,"FreehandDraft")).ToArray())
                VideoOverlay.Children.Remove(draft);
        _videoDragTrack=null;
        _videoDragHandle=0;
        if(VideoOverlay?.IsMouseCaptured==true)VideoOverlay.ReleaseMouseCapture();
        if(_videoGuide!=null)VideoOverlay?.Children.Remove(_videoGuide);
        _videoGuide=null;
    }

    private void SelectVideoTrack(Guid id)
    {
        if(!VideoCanEdit||_timeline is not {} timeline||timeline.Project.Tracks.FirstOrDefault(track=>track.Id==id) is not {} track)return;
        PauseVideoForEditing();
        SetVideoTool(null);
        _selectedVideoTrack=id;
        if(!track.IsActive(timeline.Project.Position))SeekVideoProject(track.Start);
        else SetSelectedVideoTrack(id);
    }

    private void SetSelectedVideoTrack(Guid? id)
    {
        _selectedVideoTrack=id;
        UpdateVideoPosition();
        RefreshVideoInspector();
    }

    private void ClearVideoTrackSelection()
    {
        if(_selectedVideoTrack!=null)SetSelectedVideoTrack(null);
    }

    private void ChangeVideoTrackTime(Guid id,TimeSpan start,TimeSpan end)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline
            ||timeline.Project.Tracks.FirstOrDefault(track=>track.Id==id) is not {} track)return;
        PauseVideoForEditing();
        _selectedVideoTrack=id;
        if(timeline.SetTrack(track with{Start=start,End=end}))CommitVideoEdit();
        SelectVideoTrack(id);
    }

    private void VideoDeleteTrack_Click(object sender,RoutedEventArgs e)
    {
        if(RequireLicense()&&VideoCanEdit)DeleteVideoTrack();
    }

    private void DeleteVideoTrack()
    {
        if(_timeline is not {} timeline||_selectedVideoTrack is not Guid id)return;
        PauseVideoForEditing();
        _selectedVideoTrack=null;
        if(timeline.RemoveTrack(id))CommitVideoEdit();
    }

    private void RefreshVideoInspector(bool force=false)
    {
        if(VideoInspector==null)return;
        var track=SelectedVideoTrack;
        VideoTrackInspector.Visibility=track==null?Visibility.Collapsed:Visibility.Visible;
        if(_timeline is not {} timeline){_videoInspectorLayout=null;_videoInspectorTrack=null;return;}
        var (canvas,layout,clip)=(timeline.Project.Canvas,timeline.Selected.Layout??VideoClipLayout.Default,timeline.Selected.Number);
        var key=(canvas,layout,clip);
        _syncVideoInspector=true;
        try{
            if(force||_videoInspectorLayout!=key){
                _videoInspectorLayout=key;
                VideoAspectPicker.SelectedIndex=(int)canvas.Aspect;
                VideoFitPicker.SelectedIndex=(int)canvas.Fit;
                VideoBackgroundField.Text=canvas.Background;
                VideoLayoutClip.Text=$"Đoạn {clip} · Crop và xoay";
                VideoRotationPicker.SelectedIndex=layout.Rotation/90;
                VideoCropLeft.Text=Number(layout.Left*100);VideoCropTop.Text=Number(layout.Top*100);
                VideoCropRight.Text=Number(layout.Right*100);VideoCropBottom.Text=Number(layout.Bottom*100);
            }
            if(force||_videoInspectorTrack!=track){
                _videoInspectorTrack=track;
                if(track==null)return;
                var mark=track.Mark;
                VideoTrackTitle.Text=mark.Label;
                bool text=mark.Kind is VideoMarkKind.Text or VideoMarkKind.Callout or VideoMarkKind.Step;
                VideoTrackText.Visibility=VideoTrackTextLabel.Visibility=text?Visibility.Visible:Visibility.Collapsed;
                VideoTrackText.MaxLength=mark.Kind==VideoMarkKind.Step?12:500;
                VideoTrackText.Text=mark.Kind==VideoMarkKind.Step?mark.StepLabel??mark.Number.ToString(CultureInfo.CurrentCulture):mark.Text;
                VideoTrackStart.Text=Number(track.Start.TotalSeconds);VideoTrackEnd.Text=Number(track.End.TotalSeconds);
                VideoTrackFont.Text=Number(mark.FontSize);VideoTrackStroke.Text=Number(mark.Stroke);
                VideoTrackColor.Text=mark.Color??"#F54856";
                SyncVideoInk(mark);
            }
        }finally{_syncVideoInspector=false;}
    }

    private void VideoCanvasChoice_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(!_syncVideoInspector&&VideoInspector?.Visibility==Visibility.Visible)ApplyVideoCanvas();
    }

    private void VideoBackground_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button{Tag:string color})return;
        VideoBackgroundField.Text=color;
        ApplyVideoCanvas();
    }

    private void VideoApplyCanvas_Click(object sender,RoutedEventArgs e)=>ApplyVideoCanvas();
    private void VideoResetCrop_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline)return;
        PauseVideoForEditing();
        if(timeline.SetCanvas(timeline.Project.Canvas,new(Rotation:timeline.Selected.Layout?.Rotation??0)))CommitVideoEdit();
        RefreshVideoInspector(force:true);
    }

    private void ApplyVideoCanvas()
    {
        if(!VideoCanEdit||_timeline is not {} timeline||!RequireLicense())return;
        var canvas=new VideoCanvasLayout((VideoAspect)VideoAspectPicker.SelectedIndex,(VideoFit)VideoFitPicker.SelectedIndex,
            VideoBackgroundField.Text.Trim().ToUpperInvariant());
        if(!TryNumber(VideoCropLeft,out double left)||!TryNumber(VideoCropTop,out double top)
            ||!TryNumber(VideoCropRight,out double right)||!TryNumber(VideoCropBottom,out double bottom)){
            SetStatus("Nhập tỷ lệ crop theo phần trăm.");return;
        }
        var layout=new VideoClipLayout(VideoRotationPicker.SelectedIndex*90,left/100,top/100,right/100,bottom/100);
        if(!canvas.IsValid()||!layout.IsValid()){
            SetStatus("Màu nền dùng #RRGGBB. Crop phải giữ ít nhất 5% chiều rộng và chiều cao.");return;
        }
        PauseVideoForEditing();
        if(timeline.SetCanvas(canvas,layout))CommitVideoEdit();
    }

    private void CropVideoByDrag(Point first,Point last)
    {
        if(_timeline is not {} timeline||(last-first).Length<8||!_videoImageTransform.HasInverse)return;
        var source=_videoImageTransform;
        source.Invert();
        var end=VideoNormalized(last);
        last=VideoPoint(end.X,end.Y);
        var origin=new Vector(_videoBounds.X,_videoBounds.Y);
        var box=new Rect(source.Transform(first-origin),source.Transform(last-origin));
        int width=VideoPreview.NaturalVideoWidth,height=VideoPreview.NaturalVideoHeight;
        var prior=timeline.Selected.Layout??VideoClipLayout.Default;
        var crop=prior.Crop(width,height);
        box.Intersect(new Rect(crop.X,crop.Y,crop.Width,crop.Height));
        if(box.IsEmpty)return;
        var layout=prior with{Left=Math.Max(0,box.Left/width),Top=Math.Max(0,box.Top/height),
            Right=Math.Max(0,1-box.Right/width),Bottom=Math.Max(0,1-box.Bottom/height)};
        if(!layout.IsValid()){SetStatus("Vùng crop phải giữ ít nhất 5% chiều rộng và chiều cao.");return;}
        if(timeline.SetCanvas(timeline.Project.Canvas,layout)){
            SetVideoTool(null);
            CommitVideoEdit();
        }
    }

    private void VideoApplyTrack_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline||SelectedVideoTrack is not {} track)return;
        if(!TryNumber(VideoTrackStart,out double start)||!TryNumber(VideoTrackEnd,out double end)
            ||!TryNumber(VideoTrackFont,out double font)||!TryNumber(VideoTrackStroke,out double stroke)
            ||start<0||end>timeline.Project.Duration.TotalSeconds+.00000005||end<=start){
            SetStatus("Nhập thời gian bắt đầu và kết thúc hợp lệ trong timeline.");return;
        }
        if(ReadVideoInk() is not {} style){SetStatus("Kiểm tra màu, thông số hình và opacity của chú thích.");return;}
        var mark=track.Mark with{FontSize=font,Stroke=stroke,Color=VideoTrackColor.Text.Trim().ToUpperInvariant(),Style=style};
        if(mark.Kind==VideoMarkKind.Step){
            if(!TextStudio.TryStepNumber(VideoTrackText.Text,out var step,out bool tenths)){
                SetStatus("Nhập số Bước hợp lệ, ví dụ 1 hoặc 1.2.");return;
            }
            mark=mark with{StepLabel=step.ToString(tenths?"0.0":"0",CultureInfo.InvariantCulture)};
        }
        else if(mark.Kind is VideoMarkKind.Text or VideoMarkKind.Callout)mark=mark with{Text=VideoTrackText.Text};
        if(!mark.IsValid()){SetStatus("Kiểm tra màu #RRGGBB, cỡ chữ 8–120 và độ dày 1–24.");return;}
        var finish=TimeSpan.FromSeconds(end);
        if(finish>timeline.Project.Duration)finish=timeline.Project.Duration;
        var updated=track with{Mark=mark,Start=TimeSpan.FromSeconds(start),End=finish};
        PauseVideoForEditing();
        if(timeline.SetTrack(updated)){
            CommitVideoEdit();
            SelectVideoTrack(track.Id);
        }
    }

    private static string Number(double value)=>value.ToString("0.#######",CultureInfo.CurrentCulture);
    private static bool TryNumber(TextBox box,out double value)=>double.TryParse(box.Text,NumberStyles.Float,CultureInfo.CurrentCulture,out value)&&double.IsFinite(value);
    private static Color ParseVideoColor(string value)=>(Color)ColorConverter.ConvertFromString(value);
}
