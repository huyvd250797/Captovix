using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Captovix;
public enum EditorTool { Select, Arrow, Rectangle, Ellipse, Text, Highlight, Step, Crop, Blur, Pixelate, Cover, Callout, Freehand, Shape, Line, Magnifier, Spotlight, PixelRectangle,PixelEllipse,PixelLasso }
public sealed partial class EditorSurface : FrameworkElement
{
    public EditorSession? Session {get;private set;}
    public BitmapSource? Original {get;private set;}
    private readonly HashSet<string> _selected=[];
    private string? _primary;
    public IReadOnlySet<string> SelectedIds=>_selected;
    public string? SelectedId=>_primary;
    public Annotation? Selected=>Session?.Document.Annotations.FirstOrDefault(a=>a.Id==_primary);
    public EditorTool Tool {get;set;}
    public string InkColor {get;set;}="#008B92";
    public double StrokeSize {get;set;}=4;
    public Annotation ShapeDefaults {get;set;}=new(){Kind=AnnotationKind.RoundedRectangle};
    public AnnotationKind ShapeKind {get;set;}=AnnotationKind.RoundedRectangle;
    public double TextSize {get;set;}=28;
    public int EffectSize {get;set;}=24;
    public double Zoom {get;private set;}=1;
    public double DisplayScale {get;private set;}=1;
    public double PhysicalZoom=>ImageFidelity.PhysicalZoom(Zoom,DisplayScale);
    public void SetDisplayScale(double value){_=ImageFidelity.ActualZoom(value);DisplayScale=value;Refresh();}
    public bool ContentPanEnabled {get;set;}
    public bool IsInteracting=>Dragging;
    public const double WorkspacePad=24;
    public string? InlineEditingId {get;set;}
    public ImageLayout? Layout=>Session==null?null:new ImageLayout(Session.Document.Viewport,Session.Document.Appearance);
    public PixelRect EditingBounds=>Session==null?default:CanvasEditing.SourceBounds(Session.Document.Viewport,Session.Document.Appearance);
    private bool AllowsOutside=>Tool is not (EditorTool.Crop or EditorTool.PixelRectangle or EditorTool.PixelEllipse or EditorTool.PixelLasso or EditorTool.Blur or EditorTool.Pixelate or EditorTool.Cover or EditorTool.Magnifier or EditorTool.Spotlight);
    private double InteractionScale=>Zoom*(Layout?.ScaleMin??1);
    public PixelRect? CropDraft {get;private set;}
    public event Action? CropDraftChanged;
    private PixelRect? _cropBefore;
    public event Action? SelectionChanged;
    public event Action? VisualChanged;
    public event Action? TextRequested;
    public event Action? CropFinished;
    private enum DragAction { None,Move,Resize,Rotate,Create,Crop,CropMove,CropResize,Marquee,PixelRegion,ImagePan }
    private DragAction _action;
    private bool Dragging=>_action!=DragAction.None;
    private Point _start;
    private Annotation[] _originals=[];
    private AnnotationBox _dragBounds;
    private string? _createdId;
    private int _handle=-1;
    private bool _moved;
    private int _pointBudget;
    private Rect? _draft;
    private HashSet<string> _selectionBefore=[];
    public EditorSurface(){UseLayoutRounding=true;SnapsToDevicePixels=true;Focusable=true;ClipToBounds=true;Cursor=Cursors.Arrow;RenderOptions.SetBitmapScalingMode(this,BitmapScalingMode.HighQuality);}
    public void Load(BitmapSource? source,EditorSession? session)
    {
        CancelInteraction();InlineEditingId=null;_hoveredId=null;ToolTip=null;ContextSourcePoint=null;PixelRegion=null;CropDraft=null;Original=source;Session=session;_selected.Clear();_primary=null;Refresh();SelectionChanged?.Invoke();CropDraftChanged?.Invoke();
    }
    public void SetZoom(double zoom){Zoom=Math.Clamp(zoom,.02,8);Refresh();}
    public void Refresh()
    {
        RenderOptions.SetBitmapScalingMode(this,ImageFidelity.IsIntegerZoom(PhysicalZoom)?BitmapScalingMode.NearestNeighbor:BitmapScalingMode.HighQuality);
        if(Session!=null)ConnectorStudio.Resolve(Session.Document);
        var v=Layout;if(v!=null){Width=v.Width*Zoom+2*WorkspacePad;Height=v.Height*Zoom+2*WorkspacePad;}else{Width=Height=1;}
        if(Session!=null)_selected.UnionWith(AnnotationEditing.ExpandGroups(Session.Document,_selected));
        _selected.RemoveWhere(id=>Session?.Document.Annotations.Any(a=>a.Id==id)!=true);
        if(_primary==null||!_selected.Contains(_primary))_primary=_selected.LastOrDefault();
        InvalidateVisual();VisualChanged?.Invoke();
    }
    public void NotifySelection()=>SelectionChanged?.Invoke();
    public BitmapSource Flatten(int maxWidth=0)=>Original!=null&&Session!=null?EditorRenderer.Render(Original,Session.Document,maxWidth):throw new InvalidOperationException("Chưa có ảnh để xuất.");
    private AnnotationBox SelectionBox()=>AnnotationEditing.Union(Session!.Document.Annotations.Where(a=>_selected.Contains(a.Id)).Select(EditorRenderer.Box));
    private Point[] Handles()=>ObjectHandles();
    private bool ShowHandle(int i){if(i>=8||_selected.Count==1&&Selected is {} path&&ShapeStudio.IsLine(path.Kind))return true;if(_selected.Count==1&&Selected?.Kind is AnnotationKind.Text or AnnotationKind.Step)return i%2==0;var b=ObjectPrecision.Map(SelectionBox(),Layout!);return b.Width*Zoom<36||b.Height*Zoom<36?i%2==0:true;}
    private int HitHandle(Point p){if(Session==null||!ObjectControl.CanEdit(Session.Document,_selected))return -1;var handles=Handles();int best=-1;double distance=9;for(int i=0;i<handles.Length;i++)if(ShowHandle(i)){double d=ScreenDistance(handles[i],p);if(d<=distance){distance=d;best=i;}}if(best>=0)return best;return !(_selected.Count==1&&Selected is {} line&&ConnectorStudio.IsPath(line))&&ScreenDistance(RotationHandle(),p)<=9?9:-1;}
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);dc.DrawRectangle(Brushes.Transparent,null,new Rect(0,0,ActualWidth,ActualHeight));if(Original==null||Session==null)return;var view=Session.Document.Viewport;
        var layout=Layout!;dc.PushTransform(new TranslateTransform(WorkspacePad,WorkspacePad));dc.PushTransform(new ScaleTransform(Zoom,Zoom));dc.PushClip(new RectangleGeometry(new Rect(0,0,layout.Width,layout.Height)));dc.DrawRectangle(TryFindResource("TransparencyGrid") as Brush??Brushes.Transparent,null,new Rect(0,0,layout.Width,layout.Height));EditorRenderer.DrawFrame(dc,layout,Session.Document.Appearance);
        dc.PushClip(new RectangleGeometry(Dragging||InlineEditingId!=null?new Rect(-WorkspacePad/Zoom,-WorkspacePad/Zoom,layout.Width+2*WorkspacePad/Zoom,layout.Height+2*WorkspacePad/Zoom):EditorRenderer.OutputClip(layout,Session.Document.Appearance)));dc.PushTransform(new MatrixTransform(EditorRenderer.LayoutMatrix(layout)));
        var drafts=_action is DragAction.Create or DragAction.Move or DragAction.Resize or DragAction.Rotate?_selected:null;
        EditorRenderer.Draw(dc,Original,Session.Document,drafts,InlineEditingId);DrawHover(dc);
        if(_selected.Count>0){
            if(_selected.Count>1)foreach(var a in Session.Document.Annotations.Where(a=>_selected.Contains(a.Id)))dc.DrawRectangle(null,new Pen(Brushes.DodgerBlue,.7/InteractionScale),EditorRenderer.Bounds(a));
            DrawObjectFrame(dc);
            var inverse=EditorRenderer.LayoutMatrix(layout);inverse.Invert();dc.PushTransform(new MatrixTransform(inverse));var handles=Handles();for(int i=0;i<handles.Length;i++)if(ShowHandle(i)){
                var output=layout.ToOutput(handles[i].X,handles[i].Y);var point=new Point(output.X,output.Y);var size=8/Zoom;
                if(i==8&&Selected?.Kind==AnnotationKind.Callout)dc.DrawEllipse(Brushes.Orange,new Pen(Brushes.DarkOrange,1/Zoom),point,size*.65,size*.65);else dc.DrawEllipse(Brushes.White,new Pen(Brushes.DodgerBlue,1/Zoom),point,size/2,size/2);
            }dc.Pop();
        }
        DrawSnapGuides(dc,view);DrawPixelRegion(dc);
        if(Tool==EditorTool.Crop&&CropDraft is {} crop){
            dc.Pop();dc.Pop();dc.PushClip(new RectangleGeometry(new Rect(0,0,layout.Width,layout.Height)));dc.PushTransform(Transform.Identity);var v=new Rect(0,0,layout.Width,layout.Height);var r=new Rect(crop.X,crop.Y,crop.Width,crop.Height);var shade=new SolidColorBrush(Color.FromArgb(130,10,20,30));
            dc.DrawRectangle(shade,null,new Rect(v.Left,v.Top,v.Width,Math.Max(0,r.Top-v.Top)));
            dc.DrawRectangle(shade,null,new Rect(v.Left,r.Bottom,v.Width,Math.Max(0,v.Bottom-r.Bottom)));
            dc.DrawRectangle(shade,null,new Rect(v.Left,r.Top,Math.Max(0,r.Left-v.Left),r.Height));
            dc.DrawRectangle(shade,null,new Rect(r.Right,r.Top,Math.Max(0,v.Right-r.Right),r.Height));
            dc.DrawRectangle(null,new Pen(Brushes.White,1.5/Zoom),r);
            var guide=new Pen(new SolidColorBrush(Color.FromArgb(150,255,255,255)),.7/Zoom);
            for(int n=1;n<3;n++){dc.DrawLine(guide,new(r.Left+r.Width*n/3,r.Top),new(r.Left+r.Width*n/3,r.Bottom));dc.DrawLine(guide,new(r.Left,r.Top+r.Height*n/3),new(r.Right,r.Top+r.Height*n/3));}
            foreach(var point in CropHandles(crop)){double size=9/Zoom;dc.DrawRectangle(Brushes.White,new Pen(Brushes.Teal,1/Zoom),new(point.X-size/2,point.Y-size/2,size,size));}
        }
        if(_draft is {} draft)dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(25,0,139,146)),new Pen(Brushes.Teal,1/InteractionScale){DashStyle=DashStyles.Dash},draft);
        dc.Pop();dc.Pop();dc.Pop();dc.Pop();dc.Pop();
    }
    private Point SourcePoint(MouseEventArgs e)
    {
        var p=e.GetPosition(this);var v=Tool==EditorTool.Crop?CropBounds:AllowsOutside?EditingBounds:Session!.Document.Viewport;var source=Tool==EditorTool.Crop?new ImagePoint((p.X-WorkspacePad)/Zoom,(p.Y-WorkspacePad)/Zoom):Layout!.ToSource((p.X-WorkspacePad)/Zoom,(p.Y-WorkspacePad)/Zoom);return AllowsOutside||Tool==EditorTool.Select||_action is DragAction.None or DragAction.Move or DragAction.Resize or DragAction.Rotate?new(source.X,source.Y):new(Math.Clamp(source.X,v.X,v.Right),Math.Clamp(source.Y,v.Y,v.Bottom));
    }
    private Annotation? Hit(Point p,bool cycle=false){
        var visible=Layout!.ToOutput(p.X,p.Y);if(visible.X<0||visible.Y<0||visible.X>Layout.Width||visible.Y>Layout.Height)return null;
        var hits=HitStack(p);
        if(hits.Length==0)return null;int selected=Array.FindIndex(hits,a=>a.Id==_primary);return cycle&&selected>=0?hits[(selected+1)%hits.Length]:hits[0];
    }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);if(Session==null)return;Focus();
        var location=e.GetPosition(this);if(!((AllowsOutside||Tool==EditorTool.Crop)?true:Layout!.ContainsImage(Session.Document.Viewport,(location.X-WorkspacePad)/Zoom,(location.Y-WorkspacePad)/Zoom))&&Hit(SourcePoint(e))==null&&!(Tool==EditorTool.Crop&&CropDraft is {} edge&&HitCropHandle(edge,SourcePoint(e))>=0)&&!(Tool!=EditorTool.Crop&&!Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)&&HitHandle(SourcePoint(e))>=0)){
            if(Tool==EditorTool.Select){_selected.Clear();_primary=null;Refresh();SelectionChanged?.Invoke();}e.Handled=true;return;
        }
        PointerDown(SourcePoint(e),Keyboard.Modifiers,e.ClickCount);e.Handled=true;
    }
    // Shared by native mouse input and Windows integration tests, in source pixels.
    public void PointerDown(Point point,ModifierKeys modifiers=ModifierKeys.None,int clickCount=1)
    {
        if(Session==null)return;
        if(PixelDown(point))return;
        Session.Commit();_start=point;_moved=false;_createdId=null;_originals=[];_handle=-1;_selectionBefore=new(_selected);
        if(ContentPanEnabled&&Selected is {Kind:AnnotationKind.ImageLayer,ImageStyle.Fit:ImageFit.Fill} image&&ObjectControl.CanEdit(Session.Document,_selected)&&EditorRenderer.Hit(image,point,0)&&HitHandle(point)<0){Session.Begin();_originals=[image.Copy()];_action=DragAction.ImagePan;CaptureMouse();return;}
        bool forceNew=modifiers.HasFlag(ModifierKeys.Alt)&&Tool!=EditorTool.Select;
        if(Tool!=EditorTool.Crop&&UsesSelectionGesture(Hit(_start)!=null,HitHandle(_start)>=0,forceNew)){
            var additive=(modifiers&(ModifierKeys.Shift|ModifierKeys.Control))!=0;
            bool cycle=modifiers.HasFlag(ModifierKeys.Alt);
            _handle=additive||cycle?-1:HitHandle(_start);var hit=_handle>=0?Selected:Hit(_start,cycle);
            if(hit==null){
                if(!additive){_selected.Clear();_primary=null;}
                _draft=new(_start,_start);_action=DragAction.Marquee;SelectionChanged?.Invoke();
            }else{
                var hitIds=AnnotationEditing.ExpandGroups(Session.Document,[hit.Id]);
                if(additive&&_selected.Contains(hit.Id)){_selected.ExceptWith(hitIds);_primary=_selected.LastOrDefault();Refresh();SelectionChanged?.Invoke();return;}
                if(!additive&&!_selected.Contains(hit.Id))_selected.Clear();_selected.UnionWith(hitIds);_primary=hit.Id;
                SelectionChanged?.Invoke();
                if(clickCount==2&&hit.Kind is AnnotationKind.Text or AnnotationKind.Callout){TextRequested?.Invoke();return;}
                Session.Begin();
                _originals=Session.Document.Annotations.Where(a=>_selected.Contains(a.Id)).Select(a=>a.Copy()).ToArray();_dragBounds=SelectionBox();PrepareObjectGesture();
                if(_handle<0||_handle==9||_selected.Count>1)foreach(var original in _originals){if(original.StartAnchor is {} start&&!_selected.Contains(start.TargetId))original.StartAnchor=null;if(original.EndAnchor is {} end&&!_selected.Contains(end.TargetId))original.EndAnchor=null;}
                Session.Begin();_action=_handle==9?DragAction.Rotate:_handle>=0?DragAction.Resize:DragAction.Move;
            }
        }else if(Tool==EditorTool.Crop){
            _selected.Clear();_primary=null;_cropBefore=CropDraft;_handle=CropDraft is {} crop?HitCropHandle(crop,_start):-1;
            _action=_handle>=0?DragAction.CropResize:CropDraft is {} current&&new Rect(current.X,current.Y,current.Width,current.Height).Contains(_start)?DragAction.CropMove:DragAction.Crop;
            if(_action==DragAction.Crop)CropDraft=null;SelectionChanged?.Invoke();CropDraftChanged?.Invoke();
        }
        else{
            _pointBudget=100000-Session.Document.Annotations.Sum(a=>a.Points.Count);
            if(Session.Document.Annotations.Count>=10000||Tool==EditorTool.Freehand&&_pointBudget<1){return;}
            Session.Begin();if(AllowsOutside)Session.Document.Appearance.DrawOutsideImage=true;var a=new Annotation{Kind=Tool==EditorTool.Shape?ShapeKind:Enum.Parse<AnnotationKind>(Tool.ToString()),X1=_start.X,Y1=_start.Y,X2=_start.X,Y2=_start.Y,Color=InkColor,Stroke=StrokeSize,FontSize=TextSize,EffectSize=EffectSize};
            ShapeStudio.CopyStyle(ShapeDefaults,a);InitializeTextObject(a);InitializeFocusObject(a);BeginConnector(a);if(a.Kind==AnnotationKind.Callout){a.TailX=_start.X;a.TailY=_start.Y;}
            if(Tool==EditorTool.Step)a.Number=Session.Document.NextStep();if(Tool is EditorTool.Text or EditorTool.Callout)a.Text="";
            if(Tool==EditorTool.Freehand)a.AddPoint(_start.X,_start.Y);
            Session.Document.Annotations.Add(a);_selected.Clear();_selected.Add(a.Id);_primary=_createdId=a.Id;SelectionChanged?.Invoke();
            if(Tool is EditorTool.Text or EditorTool.Step){EditorRenderer.FitText(a,EditingBounds);FitSelectionCanvas();Session.Commit();Refresh();if(Tool==EditorTool.Text)TextRequested?.Invoke();return;}
            _action=DragAction.Create;
        }
        CaptureMouse();InvalidateVisual();
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);if(Session!=null)PointerMove(SourcePoint(e),Keyboard.Modifiers);
    }
    public void PointerMove(Point p,ModifierKeys modifiers=ModifierKeys.None)
    {
        if(Session==null)return;
        if(_action==DragAction.PixelRegion){PixelMove(p);return;}
        if(!Dragging){
            ContextSourcePoint=Tool==EditorTool.Crop?null:p;var hover=Tool==EditorTool.Crop?null:Hit(p);UpdateHover(hover);
            if(Tool!=EditorTool.Crop&&!modifiers.HasFlag(ModifierKeys.Alt)){int h=HitHandle(p);Cursor=h>=0?ResizeCursor(h):hover!=null?Cursors.SizeAll:Cursors.Arrow;}
            else if(Tool==EditorTool.Crop&&CropDraft is {} crop){int h=HitCropHandle(crop,p);Cursor=h>=0?(h is 1 or 5?Cursors.SizeNS:h is 3 or 7?Cursors.SizeWE:h is 0 or 4?Cursors.SizeNWSE:Cursors.SizeNESW):new Rect(crop.X,crop.Y,crop.Width,crop.Height).Contains(p)?Cursors.SizeAll:Cursors.Cross;}
            else Cursor=Cursors.Cross;return;
        }
        if(_action is DragAction.Move or DragAction.Resize or DragAction.Rotate&&!_moved){if(ScreenDistance(p,_start)<2)return;_moved=true;}
        
        bool shift=modifiers.HasFlag(ModifierKeys.Shift);
        if(_action is DragAction.Crop or DragAction.CropMove or DragAction.CropResize){
            var view=CropBounds;
            var region=_action==DragAction.Crop?CropGeometry.FromPoints(_start.X,_start.Y,p.X,p.Y,view,shift):_action==DragAction.CropMove?CropGeometry.Move(_cropBefore!.Value,p.X-_start.X,p.Y-_start.Y,view):CropGeometry.Resize(_cropBefore!.Value,_handle,p.X,p.Y,view,shift);
            CropDraft=CropGeometry.IsValid(region,view)?region:null;CropDraftChanged?.Invoke();
        }
        else if(_action==DragAction.ImagePan&&Selected is {} image){var original=_originals[0];var style=original.ImageStyle!;var source=RasterPixels.Decode(original.PixelPng!);var placement=CompositionStudio.ImagePlacement(new(original.Left,original.Top,original.Width,original.Height),source.PixelWidth,source.PixelHeight,style);var localStart=EditorRenderer.LocalPoint(original,_start);var localEnd=EditorRenderer.LocalPoint(original,p);var delta=localEnd-localStart;var current=image.ImageStyle!;double overflowX=original.Width-placement.Width,overflowY=original.Height-placement.Height;current.PanX=Math.Clamp(style.PanX+(Math.Abs(overflowX)>1e-6?delta.X/overflowX:0),0,1);current.PanY=Math.Clamp(style.PanY+(Math.Abs(overflowY)>1e-6?delta.Y/overflowY:0),0,1);}
        else if(_action==DragAction.Marquee){_draft=new(_start,p);}
        else if(_action==DragAction.Create&&Selected is {} created){
            if(created.Kind==AnnotationKind.Freehand){var last=created.Points[^1];if(created.Points.Count<_pointBudget&&(p-new Point(last.X,last.Y)).Length>=Math.Max(1,1.5/InteractionScale))created.AddPoint(p.X,p.Y);InvalidateVisual();return;}
            var end=p;
            if(shift&&ShapeStudio.IsLine(created.Kind))end=ConstrainArrow(_start,p);
            else if(shift){double side=Math.Min(Math.Abs(p.X-_start.X),Math.Abs(p.Y-_start.Y));end=new(_start.X+Math.Sign(p.X-_start.X)*side,_start.Y+Math.Sign(p.Y-_start.Y)*side);}
            created.X2=end.X;created.Y2=end.Y;if(ShapeStudio.IsLine(created.Kind))EndConnector(created,end);
            if(created.Kind==AnnotationKind.Callout){var v=EditingBounds;created.TailX=created.Left+created.Width*.25;created.TailY=created.Top+created.Height+24;}
        }else if(_action==DragAction.Move){
            double dx=p.X-_start.X,dy=p.Y-_start.Y;
            SnapObjectMove(dx,dy,shift,SnapEnabled&&!modifiers.HasFlag(ModifierKeys.Control));
        }else if(_action==DragAction.Resize){ResizeObjectSelection(p,shift);}
        else if(_action==DragAction.Rotate){RotateObjectSelection(p,shift);}

        ConnectorStudio.Resolve(Session.Document);if(_moved&&(_action is DragAction.Move or DragAction.Resize or DragAction.Rotate))ObjectPrecision.RevealOutside(Session.Document,_selected,EditorRenderer.GeometryBox);InvalidateVisual();
    }
    private Point ConstrainArrow(Point start,Point end)
    {
        var p=ShapeStudio.ConstrainedEnd(new(start.X,start.Y),new(end.X,end.Y),new(-1000000,-1000000,2000000,2000000));return new(p.X,p.Y);
    }
    private Cursor ResizeCursor(int handle)=>handle==9?Cursors.Hand:handle==8?Cursors.Cross:_selected.Count==1&&Selected?.Kind is AnnotationKind.Arrow or AnnotationKind.Line?Cursors.Cross:HandleCursor(handle);
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);if(!Dragging||Session==null)return;PointerUp(SourcePoint(e),Keyboard.Modifiers);e.Handled=true;
    }
    public void PointerUp(Point p,ModifierKeys modifiers=ModifierKeys.None)
    {
        if(!Dragging||Session==null)return;PointerMove(p,modifiers);
        if(_action==DragAction.PixelRegion){_action=DragAction.None;ReleaseMouseCapture();try{PixelRegion?.Validate(Session.Document.Viewport);}catch{PixelRegion=null;}InvalidateVisual();SelectionChanged?.Invoke();return;}
        var action=_action;_action=DragAction.None;_guideX=_guideY=null;ReleaseMouseCapture();
        if(action is DragAction.Crop or DragAction.CropMove or DragAction.CropResize){CropDraftChanged?.Invoke();
        }else if(action==DragAction.Marquee&&_draft is {} marquee){
            if(marquee.Width>=2||marquee.Height>=2)foreach(var a in ObjectControl.CanvasCandidates(Session.Document))if(marquee.IntersectsWith(EditorRenderer.Bounds(a)))_selected.Add(a.Id);
            _primary=_selected.LastOrDefault();_draft=null;
        }else{
            if(action==DragAction.Create&&Selected is {Kind:AnnotationKind.Callout} click&&click.Width<4&&click.Height<4){click.X2=click.X1+220;click.Y2=click.Y1+120;}
            var invalid=_createdId!=null&&Selected is {} a&&(a.Kind==AnnotationKind.Freehand?a.Points.Count==0:a.Kind==AnnotationKind.Callout?a.Width<36||a.Height<36:ShapeStudio.IsLine(a.Kind)?Math.Sqrt(a.Width*a.Width+a.Height*a.Height)<2:a.Width<2||a.Height<2);
            if(invalid){Session.Cancel();_selected.Clear();_primary=null;}else{if(action==DragAction.Create)foreach(var target in Session.Document.Annotations.Where(a=>_selected.Contains(a.Id))){if(target.Kind!=AnnotationKind.Callout)EditorRenderer.FitText(target,EditingBounds);else target.AutoFitText=false;if(action==DragAction.Create&&target.Kind==AnnotationKind.Callout){var view=EditingBounds;target.TailX=target.Left+target.Width*.25;target.TailY=target.Top+target.Height+24;}}if(_moved&&(action is DragAction.Move or DragAction.Resize or DragAction.Rotate))ObjectPrecision.RevealOutside(Session.Document,_selected,EditorRenderer.GeometryBox);if(action==DragAction.Create||_moved)FitSelectionCanvas();Session.Commit();}
        }
        _originals=[];Refresh();SelectionChanged?.Invoke();if(action==DragAction.Create&&Selected?.Kind==AnnotationKind.Callout)TextRequested?.Invoke();
    }
    protected override void OnLostMouseCapture(MouseEventArgs e){base.OnLostMouseCapture(e);if(Dragging)CancelInteraction();}
    public void CancelInteraction()
    {
        if(!Dragging)return;if(_action==DragAction.PixelRegion){_action=DragAction.None;PixelRegion=_pixelBefore;ReleaseMouseCapture();InvalidateVisual();SelectionChanged?.Invoke();return;}var action=_action;_action=DragAction.None;_draft=null;_guideX=_guideY=null;
        if(action is DragAction.Crop or DragAction.CropMove or DragAction.CropResize){CropDraft=_cropBefore;CropDraftChanged?.Invoke();}
        else if(action!=DragAction.Marquee)Session?.Cancel();
        _selected.Clear();_selected.UnionWith(_selectionBefore);_primary=_selected.LastOrDefault();ReleaseMouseCapture();_createdId=null;_originals=[];Refresh();SelectionChanged?.Invoke();
    }
    private Cursor HandleCursor(int i){var l=Layout;if(l==null)return Cursors.Arrow;double x=i is 0 or 6 or 7?-1:i is 2 or 3 or 4?1:0,y=i is 0 or 1 or 2?-1:i is 4 or 5 or 6?1:0;double angle=Math.Atan2(l.B*x+l.D*y,l.A*x+l.C*y);int axis=((int)Math.Round(angle/(Math.PI/4))+8)%4;return axis switch{0=>Cursors.SizeWE,1=>Cursors.SizeNWSE,2=>Cursors.SizeNS,_=>Cursors.SizeNESW};}
    private static Point[] CropHandles(PixelRect c)=>[new(c.X,c.Y),new(c.X+c.Width/2d,c.Y),new(c.Right,c.Y),new(c.Right,c.Y+c.Height/2d),new(c.Right,c.Bottom),new(c.X+c.Width/2d,c.Bottom),new(c.X,c.Bottom),new(c.X,c.Y+c.Height/2d)];
    private int HitCropHandle(PixelRect crop,Point p){var handles=CropHandles(crop);for(int i=0;i<8;i++)if((handles[i]-p).Length<=10/Zoom)return i;return -1;}
    public PixelRect CropBounds=>Layout is {} l?new(0,0,l.Width,l.Height):default;
    public event Action<double,double>? CanvasGrown;
    public event Action<string>? CanvasLimitReached;
    public event Action? CanvasCommitted;
    public bool FitSelectionCanvas()
    {
        if(Session==null)return false;
        var objects=Session.Document.Annotations.Where(a=>a.Kind is not(AnnotationKind.Blur or AnnotationKind.Pixelate or AnnotationKind.Cover or AnnotationKind.Magnifier or AnnotationKind.Spotlight)).ToArray();
        var visible=objects.Select(EditorRenderer.ContentBounds).Where(b=>!b.IsEmpty).ToArray();var bounds=visible.Length==0?new Rect(Session.Document.Viewport.X,Session.Document.Viewport.Y,0,0):visible.Aggregate(Rect.Union);var old=Session.Document.Appearance;var before=Layout!;
        try{var basis=old.Copy();basis.ExtendLeft=Math.Max(0,basis.ExtendLeft-basis.AutoLeft);basis.ExtendTop=Math.Max(0,basis.ExtendTop-basis.AutoTop);basis.ExtendRight=Math.Max(0,basis.ExtendRight-basis.AutoRight);basis.ExtendBottom=Math.Max(0,basis.ExtendBottom-basis.AutoBottom);var next=CanvasEditing.FitContent(Session.Document.Viewport,basis,new(bounds.X,bounds.Y,bounds.Width,bounds.Height));next.AutoLeft=next.ExtendLeft-basis.ExtendLeft;next.AutoTop=next.ExtendTop-basis.ExtendTop;next.AutoRight=next.ExtendRight-basis.ExtendRight;next.AutoBottom=next.ExtendBottom-basis.ExtendBottom;Session.Document.Appearance=next;var after=Layout!;if(before.Width!=after.Width||before.Height!=after.Height){var originBefore=before.ToOutput(0,0);var originAfter=after.ToOutput(0,0);Refresh();CanvasGrown?.Invoke(originAfter.X-originBefore.X,originAfter.Y-originBefore.Y);CanvasCommitted?.Invoke();}return true;}catch(Exception ex) when(ex is ArgumentException or System.IO.InvalidDataException or OverflowException){CanvasLimitReached?.Invoke(ex.Message);return false;}
    }
    public void StartCrop(){CancelInteraction();Tool=EditorTool.Crop;CropDraft=Session==null?null:CropBounds;_selected.Clear();_primary=null;Refresh();SelectionChanged?.Invoke();CropDraftChanged?.Invoke();}
    public void ClearCropDraft(){CancelInteraction();CropDraft=null;Refresh();CropDraftChanged?.Invoke();}
    public bool SetCropDraft(PixelRect rect){if(Session==null||Dragging||!CropGeometry.IsValid(rect,CropBounds))return false;CropDraft=rect;Refresh();CropDraftChanged?.Invoke();return true;}
    public void NudgeCrop(double dx,double dy){if(Session!=null&&CropDraft is {} rect)SetCropDraft(CropGeometry.Move(rect,dx,dy,CropBounds));}
    public bool ApplyCropDraft(){
        if(Session==null||Dragging||CropDraft is not {} rect||!CropGeometry.IsValid(rect,CropBounds))return false;
        Session.Commit();try{Session.Change(d=>{d.Appearance=CanvasEditing.CropOutput(d.Viewport,d.Appearance,rect);d.Version=14;});}catch{return false;}CropDraft=null;Tool=EditorTool.Select;Refresh();CropDraftChanged?.Invoke();CropFinished?.Invoke();return true;
    }
    public void SelectIds(IEnumerable<string> ids,string? primary=null){if(Session==null)return;CancelInteraction();_selected.Clear();_selected.UnionWith(AnnotationEditing.ExpandGroups(Session.Document,ids));_primary=primary!=null&&_selected.Contains(primary)?primary:_selected.LastOrDefault();Refresh();SelectionChanged?.Invoke();}
    public void SelectAll(){if(Session==null)return;CancelInteraction();_selected.Clear();_selected.UnionWith(ObjectControl.CanvasCandidates(Session.Document).Select(a=>a.Id));_primary=_selected.LastOrDefault();Refresh();SelectionChanged?.Invoke();}
    public void DeleteSelected(){if(Session==null||!ObjectControl.CanEdit(Session.Document,_selected))return;CancelInteraction();var ids=_selected.ToHashSet();Session.Commit();Session.Change(d=>{d.Annotations.RemoveAll(a=>ids.Contains(a.Id));FitSelectionCanvas();});_selected.Clear();_primary=null;Refresh();SelectionChanged?.Invoke();}
    public void Nudge(double x,double y)
    {
        if(Session==null||!ObjectControl.CanEdit(Session.Document,_selected))return;var ids=_selected.ToHashSet();var delta=Layout!.SourceDelta(x,y);var move=AnnotationEditing.ClampMove(SelectionBox(),EditingBounds,delta.X,delta.Y);
        Session.Commit();Session.Change(d=>{ConnectorStudio.DetachForTransform(d,ids);foreach(var a in d.Annotations.Where(a=>ids.Contains(a.Id)))a.Translate(move.X,move.Y);ObjectPrecision.RevealOutside(d,ids,EditorRenderer.GeometryBox);});Refresh();SelectionChanged?.Invoke();
    }
}
