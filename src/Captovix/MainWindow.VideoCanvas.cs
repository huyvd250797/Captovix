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
    private VideoMark? _videoInspectorDefault;
    private Image? _videoCalloutDraft;
    private bool _syncVideoInspector;
    private string? _videoTool;
    private Point? _videoDragStart;
    private readonly List<VideoInkPoint> _videoInkPoints=[];
    private VideoMarkTrack? _videoDragTrack;
    private int _videoDragHandle;
    private Rect _videoBounds=Rect.Empty;
    private Matrix _videoImageTransform;
    private object? _videoCanvasKey;
    private Matrix _videoLayoutMatrix;
    private (int Width,int Height) _videoDecodedSize;
    private VideoMark? _videoMovedDraft;
    private System.Windows.Threading.DispatcherOperation? _videoDragPaint;
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
        _pickVideoCue=false;
        _videoTool=tool;
        ClearVideoDrag();
        if(VideoOverlay==null)return;
        VideoOverlay.Cursor=tool==null?Cursors.Arrow:Cursors.Cross;
        foreach(var button in VideoMarkTools.Children.OfType<Button>())
            button.SetResourceReference(Control.BackgroundProperty,(string?)button.Tag==(tool??"Select")?"Soft":"Surface");
        if(_videoStudio)RefreshVideoInspector(force:true);
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
        if((!_videoReady&&!_videoDecoderSuspended)||_timeline is not {} timeline||VideoOverlay.ActualWidth<=0||VideoOverlay.ActualHeight<=0)
        {
            VideoCanvasSurface.Visibility=Visibility.Hidden;
            return Rect.Empty;
        }
        if(_videoReady)_videoDecodedSize=(VideoPreview.NaturalVideoWidth,VideoPreview.NaturalVideoHeight);
        var (sourceWidth,sourceHeight)=_videoDecodedSize;
        if(sourceWidth<=0||sourceHeight<=0)return Rect.Empty;
        var canvas=timeline.Project.Canvas;
        var layout=timeline.Selected.Layout??VideoClipLayout.Default;
        var primary=_videoSourceSize??(sourceWidth,sourceHeight);
        var (width,height)=canvas.OutputSize(primary.Width,primary.Height);
        double scale=Math.Min(VideoOverlay.ActualWidth/width,VideoOverlay.ActualHeight/height);
        var bounds=new Rect((VideoOverlay.ActualWidth-width*scale)/2,(VideoOverlay.ActualHeight-height*scale)/2,width*scale,height*scale);
        var key=(canvas,layout,sourceWidth,sourceHeight,width,height,bounds);
        VideoCanvasSurface.Visibility=Visibility.Visible;
        if(Equals(_videoCanvasKey,key)){ApplyVideoMotion(bounds);return bounds;}
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
        _videoLayoutMatrix=matrix;
        VideoCanvasSurface.Width=bounds.Width;
        VideoCanvasSurface.Height=bounds.Height;
        VideoCanvasSurface.Background=new SolidColorBrush(ParseVideoColor(canvas.Background));
        VideoCanvasSurface.HorizontalAlignment=HorizontalAlignment.Center;
        VideoCanvasSurface.VerticalAlignment=VerticalAlignment.Center;
        VideoCropSurface.Width=VideoPreview.Width=sourceWidth;
        VideoCropSurface.Height=VideoPreview.Height=sourceHeight;
        VideoCropSurface.Clip=new RectangleGeometry(new Rect(crop.X,crop.Y,crop.Width,crop.Height));
        ApplyVideoMotion(bounds);
        return bounds;
    }

    private void ApplyVideoMotion(Rect bounds)
    {
        if(_timeline is not {} timeline)return;
        var clip=timeline.Selected;
        var local=timeline.Project.Position-timeline.Offset(timeline.Project.SelectedClip);
        var timing=clip.Timing;
        var key=timing.At(clip.Fraction(local));
        var origin=VideoMotion.Origin(key.Zoom,key.X,key.Y);
        var matrix=_videoLayoutMatrix;
        matrix.Scale(key.Zoom,key.Zoom);
        matrix.Translate(-origin.X*bounds.Width*key.Zoom,-origin.Y*bounds.Height*key.Zoom);
        if(VideoCropSurface.RenderTransform is not MatrixTransform existing||existing.Matrix!=matrix)
            VideoCropSurface.RenderTransform=new MatrixTransform(matrix);
        _videoImageTransform=matrix;
        VideoFadeCurtain.Width=bounds.Width;
        VideoFadeCurtain.Height=bounds.Height;
        VideoFadeCurtain.Opacity=timing.Black(local.TotalSeconds,clip.Duration.TotalSeconds);
    }

    private static void DisconnectVideoEffect(System.Windows.Shapes.Shape shape)
    {
        if(shape.Fill is VisualBrush visual){visual.Visual=null;shape.Fill=null;}
        if(shape.Effect!=null)shape.Effect=null;
        if(shape.CacheMode!=null)shape.CacheMode=null;
    }

    private void ClearVideoVisuals(bool keepEditor=false)
    {
        for(int i=VideoOverlay.Children.Count-1;i>=0;i--){
            var element=VideoOverlay.Children[i];
            if(element==_cueLayer||keepEditor&&element==_inlineVideoText)continue;
            if(element is Canvas layer)
                foreach(var shape in layer.Children.OfType<System.Windows.Shapes.Shape>())DisconnectVideoEffect(shape);
            else if(element is System.Windows.Shapes.Shape shape)DisconnectVideoEffect(shape);
            VideoOverlay.Children.RemoveAt(i);
        }
        if(!keepEditor){_cueLayer.Children.Clear();_previousVideoTracks=null;}
    }

    private void UpdateVideoMarks()
    {
        if(VideoOverlay==null)return;
        var bounds=UpdateVideoCanvas();
        _videoBounds=bounds;
        UpdateVideoCues(bounds);
        UpdateCameraPreview(bounds);
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
        ClearVideoVisuals(keepEditor:true);
        _videoGuide=null;
        _videoCalloutDraft=null;
        if(bounds.IsEmpty||tracks==null)return;
        var marks=new List<VideoMark>();
        foreach(var track in tracks){
            if(track.IsActive(position))marks.Add(_videoDragTrack?.Id==track.Id&&_videoMovedDraft!=null?_videoMovedDraft:track.Mark);
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
        var editing=tracks.FirstOrDefault(item=>item.Id==_inlineVideoTrack)?.Mark;
        var image=VideoMarkRendering.Draw([..marks],bounds.Width,bounds.Height,editing);
        if(image!=null){
            var ink=new Image{Source=image,Width=bounds.Width,Height=bounds.Height,Stretch=Stretch.Fill,IsHitTestVisible=false};
            Canvas.SetLeft(ink,bounds.X);Canvas.SetTop(ink,bounds.Y);
            VideoOverlay.Children.Add(ink);
        }
        if(_videoStudio&&!_playing&&_videoFullscreen==null&&SelectedVideoTrack is {} selected&&selected.IsActive(position)){
            var mark=_videoDragTrack?.Id==selected.Id?_videoMovedDraft??selected.Mark:selected.Mark;
            var box=VideoMarkHitBox(mark);
            AddVideoOutline(box);
            if(mark.Kind==VideoMarkKind.Callout){
                var anchors=VideoCalloutHandles(mark);
                foreach(var (handle,point) in anchors)
                    DrawVideoHandle(point,handle==3?Brushes.Orange:handle==4?Brushes.DeepSkyBlue:Brushes.White);
            }else if(HasVideoHandles(mark.Kind)||mark.TextFrame){
                DrawVideoHandle(VideoPoint(mark.X1,mark.Y1));
                DrawVideoHandle(VideoPoint(mark.X2,mark.Y2));
            }
        }
    }

    private static bool HasVideoHandles(VideoMarkKind kind)=>kind is VideoMarkKind.Frame or VideoMarkKind.Blur or VideoMarkKind.Arrow or VideoMarkKind.Callout or VideoMarkKind.Highlight or VideoMarkKind.Spotlight or VideoMarkKind.Magnifier;

    private void DrawVideoHandle(Point point,Brush? fill=null)
    {
        var handle=new Rectangle{Width=7,Height=7,Fill=fill??Brushes.White,Stroke=Brushes.DeepSkyBlue,IsHitTestVisible=false};
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
        if(mark.Kind==VideoMarkKind.Callout||mark.TextFrame){
            var annotation=VideoMarkRendering.AnnotationFor(mark,_videoBounds.Width,_videoBounds.Height);
            var rendered=mark.Kind==VideoMarkKind.Callout
                ?Rect.Union(EditorRenderer.Bounds(annotation),EditorRenderer.ContentBounds(annotation))
                :EditorRenderer.Bounds(annotation);
            rendered.Offset(_videoBounds.X,_videoBounds.Y);
            rendered.Inflate(7,7);
            return rendered;
        }
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
        if(_inlineVideoText!=null&&!CommitVideoInlineText())return;
        PauseVideoForEditing();
        if(PlaceVideoCue(point)){e.Handled=true;return;}
        bool forceNew=Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        if(_videoTool!="Crop"&&!forceNew){
            var selected=SelectedVideoTrack;
            var active=selected!=null&&selected.IsActive(timeline.Project.Position)&&(VideoMarkHitBox(selected.Mark).Contains(point)||FindCalloutHandle(selected.Mark,point)>0)?selected:
                timeline.Project.Tracks.LastOrDefault(track=>track.IsActive(timeline.Project.Position)&&VideoMarkHitBox(track.Mark).Contains(point));
            if(active==null){
                ClearVideoTrackSelection();
                if(_videoTool==null)return;
            }else{
                SetSelectedVideoTrack(active.Id);
                var mark=active.Mark;
                if(e.ClickCount==2&&mark.HasText){
                    EditVideoInlineText(active);
                    e.Handled=true;
                    return;
                }
                _videoDragTrack=active;
                if(mark.Kind==VideoMarkKind.Callout)_videoDragHandle=FindCalloutHandle(mark,point);
                else if(HasVideoHandles(mark.Kind)||mark.TextFrame){
                    if((VideoPoint(mark.X1,mark.Y1)-point).Length<=9)_videoDragHandle=1;
                    else if((VideoPoint(mark.X2,mark.Y2)-point).Length<=9)_videoDragHandle=2;
                }
            }
        }
        _videoDragStart=point;
        if(_videoTool=="Freehand"&&_videoDragTrack==null){
            var at=VideoNormalized(point);
            _videoInkPoints.Add(new(at.X,at.Y));
        }
        VideoOverlay.CaptureMouse();
        e.Handled=true;
    }

    private VideoMark MovedVideoMark(VideoMark mark,Point from,Point to)
    {
        if(mark.Kind==VideoMarkKind.Callout)return MoveVideoCallout(mark,from,to);
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
        if(_videoDragTrack is {} moving){
            if((point-start).Length<2&&_videoMovedDraft==null)return;
            _videoMovedDraft=MovedVideoMark(moving.Mark,start,point);
            _videoDragPaint??=Dispatcher.InvokeAsync(()=>{
                _videoDragPaint=null;
                _previousVideoTracks=null;
                UpdateVideoMarks();
            },System.Windows.Threading.DispatcherPriority.Render);
            return;
        }
        if(_videoTool=="Callout"){
            if((point-start).Length<4)return;
            var callout=PlaceVideoCallout(DefaultVideoInk(VideoMarkKind.Callout),start,point);
            ShowVideoCalloutDraft(callout);
            return;
        }
        if(_videoTool=="Freehand"&&_videoDragTrack==null&&_videoInkPoints.Count is >0 and <2048){
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
        if(VideoKindForTool(_videoTool) is not {} kind)return;
        if(kind==VideoMarkKind.Callout&&(end-start).Length<4)return;
        var first=VideoNormalized(start);
        var last=VideoNormalized(end);
        if(kind is (VideoMarkKind.Frame or VideoMarkKind.Blur or VideoMarkKind.Highlight or VideoMarkKind.Spotlight or VideoMarkKind.Magnifier)&&(Math.Abs(end.X-start.X)<5||Math.Abs(end.Y-start.Y)<5))return;
        if(kind==VideoMarkKind.Arrow&&(end-start).Length<5)return;
        bool textFrame=kind is (VideoMarkKind.Text or VideoMarkKind.Title)&&Math.Abs(end.X-start.X)>=8&&Math.Abs(end.Y-start.Y)>=8;
        if(kind==VideoMarkKind.Title&&!textFrame){
            first=new(Math.Min(first.X,.75),Math.Min(first.Y,.85));
            last=new(Math.Min(1,first.X+.45),Math.Min(1,first.Y+.15));
            textFrame=true;
        }
        if(kind==VideoMarkKind.Step||kind==VideoMarkKind.Text&&!textFrame)last=first;
        if(kind==VideoMarkKind.Freehand&&points.Length>0){
            first=new(points.Min(p=>p.X),points.Min(p=>p.Y));
            last=new(points.Max(p=>p.X),points.Max(p=>p.Y));
        }
        var position=timeline.Project.Position;
        if(position>=timeline.Project.Duration)position=timeline.Project.Duration-TimeSpan.FromMilliseconds(1);
        if(position<TimeSpan.Zero)position=TimeSpan.Zero;
        var stop=position+TimeSpan.FromSeconds(3);
        if(stop>timeline.Project.Duration)stop=timeline.Project.Duration;
        int number=kind==VideoMarkKind.Step?NextVideoStep(timeline.Project):0;
        string text=kind switch{VideoMarkKind.Title=>"Tiêu đề",VideoMarkKind.Text=>"Chữ",_=>""};
        var mark=DefaultVideoInk(kind) with{X1=first.X,Y1=first.Y,X2=last.X,Y2=last.Y,Text=text,Number=number,
            TextFrame=textFrame,Points=points.Length>0?points:null,StepLabel=null};
        if(kind==VideoMarkKind.Callout)mark=PlaceVideoCallout(mark,start,end);
        var newTrack=new VideoMarkTrack(Guid.NewGuid(),position,stop,mark);
        if(!timeline.SetTrack(newTrack)){SetStatus("Timeline đã đạt giới hạn chú thích.");return;}
        if(position!=timeline.Project.Position)timeline.Seek(position);
        _selectedVideoTrack=newTrack.Id;
        CommitVideoEdit();
        VideoStripScroll.UpdateLayout();
        VideoStripScroll.ScrollToVerticalOffset(Math.Max(0,VideoStrip.Height-VideoStripScroll.ViewportHeight));
        if(kind is VideoMarkKind.Text or VideoMarkKind.Title or VideoMarkKind.Callout)EditVideoInlineText(newTrack);
    }

    private void EditVideoInlineText(VideoMarkTrack track)
    {
        CancelVideoInlineText();
        if(_videoBounds.IsEmpty)return;
        var mark=track.Mark;
        var annotation=VideoMarkRendering.AnnotationFor(mark,_videoBounds.Width,_videoBounds.Height);
        var content=EditorRenderer.TextContent(annotation);
        bool framed=mark.Kind==VideoMarkKind.Callout||mark.TextFrame;
        bool step=mark.Kind==VideoMarkKind.Step;
        var layout=EditorRenderer.TextLayout(annotation);
        var input=new TextBox{
            Text=step?mark.StepLabel??mark.Number.ToString(CultureInfo.CurrentCulture):mark.Text,
            MaxLength=step?12:500,AcceptsReturn=!step,
            Width=step?annotation.FontSize*4:framed?content.Width:Math.Min(_videoBounds.Width,Math.Max(160,layout.WidthIncludingTrailingWhitespace+40)),
            Height=framed?content.Height:Math.Max(annotation.FontSize+4,layout.Height+4),
            FontFamily=EditorRenderer.TextFont(annotation),FontSize=annotation.FontSize,
            FontWeight=annotation.Bold?FontWeights.Bold:FontWeights.Normal,FontStyle=annotation.Italic?FontStyles.Italic:FontStyles.Normal,
            Foreground=EditorRenderer.Ink(TextStudio.Foreground(annotation)),
            TextAlignment=step?TextAlignment.Center:EditorRenderer.WpfAlignment(annotation.TextAlignment),
            VerticalContentAlignment=annotation.VerticalAlignment switch{TextVertical.Center=>VerticalAlignment.Center,TextVertical.Bottom=>VerticalAlignment.Bottom,_=>VerticalAlignment.Top},
            TextWrapping=framed?TextWrapping.Wrap:TextWrapping.NoWrap,
            VerticalScrollBarVisibility=mark.TextFrame?ScrollBarVisibility.Auto:ScrollBarVisibility.Hidden,
            ToolTip=step?"Enter lưu · Esc hủy":"Ctrl+Enter lưu · Esc hủy"
        };
        input.SetResourceReference(StyleProperty,"InlineTextEditor");
        input.CaretBrush=input.Foreground;
        var local=EditorRenderer.LocalBox(annotation);
        double x=step?annotation.X1-input.Width/2:content.X,y=step?annotation.Y1-input.Height/2:content.Y;
        Canvas.SetLeft(input,_videoBounds.X+x);
        Canvas.SetTop(input,_videoBounds.Y+y);
        input.RenderTransform=new RotateTransform(annotation.Rotation,local.CenterX-x,local.CenterY-y);
        TextBlock.SetLineHeight(input,annotation.LineSpacing>0?annotation.FontSize*annotation.LineSpacing:double.NaN);
        input.SetValue(TextBlock.TextDecorationsProperty,annotation.Underline?TextDecorations.Underline:null);
        input.PreviewKeyDown+=VideoInlineText_KeyDown;
        input.LostKeyboardFocus+=VideoInlineText_LostFocus;
        _inlineVideoTrack=track.Id;
        _inlineVideoText=input;
        Panel.SetZIndex(input,10);
        VideoOverlay.Children.Add(input);
        _previousVideoTracks=null;
        UpdateVideoMarks();
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
        _previousVideoTracks=null;
        UpdateVideoMarks();
    }

    private void VideoOverlay_LostCapture(object sender,MouseEventArgs e)=>ClearVideoDrag();

    private void ClearVideoDrag()
    {
        _videoDragPaint?.Abort();
        _videoDragPaint=null;
        _videoDragStart=null;
        _videoInkPoints.Clear();
        if(_videoCalloutDraft!=null)VideoOverlay?.Children.Remove(_videoCalloutDraft);
        _videoCalloutDraft=null;
        if(VideoOverlay!=null)
            foreach(var draft in VideoOverlay.Children.OfType<Line>().Where(line=>Equals(line.Tag,"FreehandDraft")).ToArray())
                VideoOverlay.Children.Remove(draft);
        bool redraw=_videoMovedDraft!=null;
        _videoMovedDraft=null;
        _videoDragTrack=null;
        _videoDragHandle=0;
        if(VideoOverlay?.IsMouseCaptured==true)VideoOverlay.ReleaseMouseCapture();
        if(_videoGuide!=null)VideoOverlay?.Children.Remove(_videoGuide);
        _videoGuide=null;
        if(redraw){_previousVideoTracks=null;UpdateVideoMarks();}
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
        if(VideoInspector==null||!_videoStudio)return;
        if(_timeline is {} motionTimeline){SyncVideoMotion(motionTimeline.Selected);SyncCameraInspector(motionTimeline);}
        var track=SelectedVideoTrack;
        var draft=track==null?CurrentVideoInk():null;
        var displayed=track?.Mark??draft;
        VideoTrackInspector.Visibility=displayed==null?Visibility.Collapsed:Visibility.Visible;
        VideoMarkTiming.Visibility=track==null?Visibility.Collapsed:Visibility.Visible;
        VideoDeleteTrackButton.Visibility=VideoMarkTiming.Visibility;
        VideoApplyTrackButton.Content=track==null?"Áp dụng kiểu công cụ":"Áp dụng chú thích";
        SyncVideoFavorites(displayed);
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
            if(force||_videoInspectorTrack!=track||_videoInspectorDefault!=draft){
                _videoInspectorTrack=track;
                _videoInspectorDefault=draft;
                if(displayed is not {} mark)return;
                VideoTrackTitle.Text=mark.Label;
                bool text=track!=null&&mark.HasText;
                VideoTrackText.Visibility=VideoTrackTextLabel.Visibility=text?Visibility.Visible:Visibility.Collapsed;
                VideoTrackText.MaxLength=mark.Kind==VideoMarkKind.Step?12:500;
                VideoTrackText.Text=mark.Kind==VideoMarkKind.Step?mark.StepLabel??mark.Number.ToString(CultureInfo.CurrentCulture):mark.Text;
                if(track!=null){VideoTrackStart.Text=Number(track.Start.TotalSeconds);VideoTrackEnd.Text=Number(track.End.TotalSeconds);}
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
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline||CurrentVideoInk() is not {} original)return;
        var track=SelectedVideoTrack;
        if(ReadVideoMark(original,track!=null) is not {} mark){
            SetStatus("Kiểm tra màu, cỡ chữ, số bước và kiểu chú thích.");return;
        }
        if(track==null){
            _videoToolStyles[mark.Kind]=mark;
            RefreshVideoInspector(force:true);
            return;
        }
        if(!TryNumber(VideoTrackStart,out double start)||!TryNumber(VideoTrackEnd,out double end)
            ||start<0||end>timeline.Project.Duration.TotalSeconds+.00000005||end<=start){
            SetStatus("Nhập thời điểm hiện và ẩn hợp lệ trong timeline.");return;
        }
        var finish=TimeSpan.FromSeconds(end);
        if(finish>timeline.Project.Duration)finish=timeline.Project.Duration;
        var updated=track with{Mark=mark,Start=TimeSpan.FromSeconds(start),End=finish};
        PauseVideoForEditing();
        if(timeline.SetTrack(updated)){
            CommitVideoEdit();
            SelectVideoTrack(track.Id);
        }
    }

    private VideoMark? ReadVideoMark(VideoMark original,bool includeText)
    {
        if(!TryNumber(VideoTrackFont,out double font)||!TryNumber(VideoTrackStroke,out double stroke)
            ||ReadVideoInk(original.Style) is not {} style)return null;
        var mark=original with{FontSize=font,Stroke=stroke,Color=VideoTrackColor.Text.Trim().ToUpperInvariant(),Style=style};
        if(includeText&&mark.Kind==VideoMarkKind.Step){
            if(!TextStudio.TryStepNumber(VideoTrackText.Text,out var step,out bool tenths))return null;
            mark=mark with{StepLabel=step.ToString(tenths?"0.0":"0",CultureInfo.InvariantCulture)};
        }else if(includeText&&mark.HasText)mark=mark with{Text=VideoTrackText.Text};
        return mark.IsValid()?mark:null;
    }

    private void VideoUsePlayhead_Click(object sender,RoutedEventArgs e)
    {
        if(!VideoCanEdit||SelectedVideoTrack==null||_timeline is not {} timeline)return;
        var field=(sender as Button)?.Tag as string=="start"?VideoTrackStart:VideoTrackEnd;
        field.Text=Number(timeline.Project.Position.TotalSeconds);
    }

    private static int NextVideoStep(VideoProject project)
    {
        int highest=project.Tracks.Where(track=>track.Mark.Kind==VideoMarkKind.Step)
            .Select(track=>track.Mark.StepLabel is {} label&&TextStudio.TryStepNumber(label,out var value,out _)
                ?(int)Math.Floor(value):track.Mark.Number).DefaultIfEmpty().Max();
        return Math.Min(9999,highest+1);
    }

    private static string Number(double value)=>value.ToString("0.#######",CultureInfo.CurrentCulture);
    private static bool TryNumber(TextBox box,out double value)=>double.TryParse(box.Text,NumberStyles.Float,CultureInfo.CurrentCulture,out value)&&double.IsFinite(value);
    private static Color ParseVideoColor(string value)=>(Color)ColorConverter.ConvertFromString(value);
}
