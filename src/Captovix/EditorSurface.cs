using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Captovix;
public enum EditorTool { Select, Arrow, Rectangle, Ellipse, Text, Highlight, Step, Crop, Blur, Pixelate, Cover }
public sealed class EditorSurface : FrameworkElement
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
    public double TextSize {get;set;}=28;
    public int EffectSize {get;set;}=24;
    public double Zoom {get;private set;}=1;
    public event Action? SelectionChanged;
    public event Action? VisualChanged;
    public event Action? TextRequested;
    public event Action? CropFinished;
    private enum DragAction { None,Move,Resize,Create,Crop,Marquee }
    private DragAction _action;
    private bool Dragging=>_action!=DragAction.None;
    private Point _start;
    private Annotation[] _originals=[];
    private AnnotationBox _dragBounds;
    private string? _createdId;
    private int _handle=-1;
    private bool _moved;
    private Rect? _draft;
    private HashSet<string> _selectionBefore=[];
    public EditorSurface(){Focusable=true;ClipToBounds=true;Cursor=Cursors.Arrow;RenderOptions.SetBitmapScalingMode(this,BitmapScalingMode.HighQuality);}
    public void Load(BitmapSource? source,EditorSession? session)
    {
        CancelInteraction();Original=source;Session=session;_selected.Clear();_primary=null;Refresh();SelectionChanged?.Invoke();
    }
    public void SetZoom(double zoom){Zoom=Math.Clamp(zoom,.02,8);Refresh();}
    public void Refresh()
    {
        var v=Session?.Document.Viewport;if(v!=null){Width=v.Value.Width*Zoom;Height=v.Value.Height*Zoom;}else{Width=Height=1;}
        _selected.RemoveWhere(id=>Session?.Document.Annotations.Any(a=>a.Id==id)!=true);
        if(_primary==null||!_selected.Contains(_primary))_primary=_selected.LastOrDefault();
        InvalidateVisual();VisualChanged?.Invoke();
    }
    public void NotifySelection()=>SelectionChanged?.Invoke();
    public BitmapSource Flatten(int maxWidth=0)=>Original!=null&&Session!=null?EditorRenderer.Render(Original,Session.Document,maxWidth):throw new InvalidOperationException("Chưa có ảnh để xuất.");
    private AnnotationBox SelectionBox()=>AnnotationEditing.Union(Session!.Document.Annotations.Where(a=>_selected.Contains(a.Id)).Select(EditorRenderer.Box));
    private Point[] Handles()
    {
        if(_selected.Count==0)return [];
        if(_selected.Count==1&&Selected is {Kind:AnnotationKind.Arrow} arrow)return [new(arrow.X1,arrow.Y1),new(arrow.X2,arrow.Y2)];
        var b=SelectionBox();return [new(b.X,b.Y),new(b.CenterX,b.Y),new(b.Right,b.Y),new(b.Right,b.CenterY),new(b.Right,b.Bottom),new(b.CenterX,b.Bottom),new(b.X,b.Bottom),new(b.X,b.CenterY)];
    }
    private bool ShowHandle(int i)=>!(_selected.Count==1&&Selected?.Kind is AnnotationKind.Text or AnnotationKind.Step)||i%2==0;
    private int HitHandle(Point p){var handles=Handles();for(int i=0;i<handles.Length;i++)if(ShowHandle(i)&&(handles[i]-p).Length<=9/Zoom)return i;return -1;}
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);if(Original==null||Session==null)return;var view=Session.Document.Viewport;
        dc.PushTransform(new ScaleTransform(Zoom,Zoom));dc.PushTransform(new TranslateTransform(-view.X,-view.Y));
        var drafts=_action is DragAction.Create or DragAction.Move or DragAction.Resize?_selected:null;
        EditorRenderer.Draw(dc,Original,Session.Document,drafts);
        if(_selected.Count>0){
            if(_selected.Count>1)foreach(var a in Session.Document.Annotations.Where(a=>_selected.Contains(a.Id)))dc.DrawRectangle(null,new Pen(Brushes.DodgerBlue,.7/Zoom),EditorRenderer.Bounds(a));
            var box=SelectionBox();var rect=new Rect(box.X,box.Y,box.Width,box.Height);rect.Inflate(3/Zoom,3/Zoom);
            dc.DrawRectangle(null,new Pen(Brushes.DodgerBlue,1/Zoom){DashStyle=DashStyles.Dash},rect);
            var handles=Handles();for(int i=0;i<handles.Length;i++)if(ShowHandle(i)){
                var point=handles[i];var size=8/Zoom;
                dc.DrawRectangle(Brushes.White,new Pen(Brushes.DodgerBlue,1/Zoom),new(point.X-size/2,point.Y-size/2,size,size));
            }
        }
        if(_draft is {} draft){
            if(_action==DragAction.Crop){
                var v=new Rect(view.X,view.Y,view.Width,view.Height);var shade=new SolidColorBrush(Color.FromArgb(120,10,20,30));
                dc.DrawRectangle(shade,null,new Rect(v.Left,v.Top,v.Width,Math.Max(0,draft.Top-v.Top)));
                dc.DrawRectangle(shade,null,new Rect(v.Left,draft.Bottom,v.Width,Math.Max(0,v.Bottom-draft.Bottom)));
                dc.DrawRectangle(shade,null,new Rect(v.Left,draft.Top,Math.Max(0,draft.Left-v.Left),draft.Height));
                dc.DrawRectangle(shade,null,new Rect(draft.Right,draft.Top,Math.Max(0,v.Right-draft.Right),draft.Height));
            }
            dc.DrawRectangle(_action==DragAction.Marquee?new SolidColorBrush(Color.FromArgb(25,0,139,146)):null,new Pen(_action==DragAction.Crop?Brushes.White:Brushes.Teal,1/Zoom){DashStyle=DashStyles.Dash},draft);
        }
        dc.Pop();dc.Pop();
    }
    private Point SourcePoint(MouseEventArgs e)
    {
        var p=e.GetPosition(this);var v=Session!.Document.Viewport;return new(Math.Clamp(p.X/Zoom+v.X,v.X,v.Right),Math.Clamp(p.Y/Zoom+v.Y,v.Y,v.Bottom));
    }
    private Annotation? Hit(Point p)=>Session!.Document.Annotations.OrderBy(a=>a.IsRedaction? a.Kind==AnnotationKind.Cover?2:1:0).LastOrDefault(a=>EditorRenderer.Hit(a,p,5/Zoom));
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);if(Session==null)return;Focus();Session.Commit();_start=SourcePoint(e);_moved=false;_createdId=null;_originals=[];_handle=-1;_selectionBefore=new(_selected);
        if(Tool==EditorTool.Select){
            var additive=(Keyboard.Modifiers&(ModifierKeys.Shift|ModifierKeys.Control))!=0;
            _handle=additive?-1:HitHandle(_start);var hit=_handle>=0?Selected:Hit(_start);
            if(hit==null){
                if(!additive){_selected.Clear();_primary=null;}
                _draft=new(_start,_start);_action=DragAction.Marquee;SelectionChanged?.Invoke();
            }else{
                if(additive&&_selected.Remove(hit.Id)){_primary=_selected.LastOrDefault();Refresh();SelectionChanged?.Invoke();e.Handled=true;return;}
                if(!additive&&!_selected.Contains(hit.Id))_selected.Clear();_selected.Add(hit.Id);_primary=hit.Id;
                SelectionChanged?.Invoke();
                if(e.ClickCount==2&&hit.Kind==AnnotationKind.Text){TextRequested?.Invoke();e.Handled=true;return;}
                _originals=Session.Document.Annotations.Where(a=>_selected.Contains(a.Id)).Select(a=>a.Copy()).ToArray();_dragBounds=SelectionBox();
                Session.Begin();_action=_handle>=0?DragAction.Resize:DragAction.Move;
            }
        }else if(Tool==EditorTool.Crop){_selected.Clear();_primary=null;_draft=new(_start,_start);_action=DragAction.Crop;SelectionChanged?.Invoke();}
        else{
            Session.Begin();var a=new Annotation{Kind=Enum.Parse<AnnotationKind>(Tool.ToString()),X1=_start.X,Y1=_start.Y,X2=_start.X,Y2=_start.Y,Color=InkColor,Stroke=StrokeSize,FontSize=TextSize,EffectSize=EffectSize};
            if(Tool==EditorTool.Step)a.Number=Session.Document.NextStep();if(Tool==EditorTool.Text)a.Text="Nhập nội dung";
            Session.Document.Annotations.Add(a);_selected.Clear();_selected.Add(a.Id);_primary=_createdId=a.Id;SelectionChanged?.Invoke();
            if(Tool is EditorTool.Text or EditorTool.Step){Session.Commit();Refresh();if(Tool==EditorTool.Text)TextRequested?.Invoke();e.Handled=true;return;}
            _action=DragAction.Create;
        }
        CaptureMouse();InvalidateVisual();e.Handled=true;
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);if(Session==null)return;var p=SourcePoint(e);
        if(!Dragging){
            if(Tool==EditorTool.Select){int h=HitHandle(p);Cursor=h>=0?ResizeCursor(h):Hit(p)!=null?Cursors.SizeAll:Cursors.Arrow;}else Cursor=Cursors.Cross;return;
        }
        if(_action is DragAction.Move or DragAction.Resize&&!_moved){if((p-_start).Length<2/Zoom)return;_moved=true;}
        bool shift=Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        if(_action is DragAction.Crop or DragAction.Marquee){_draft=new(_start,p);}
        else if(_action==DragAction.Create&&Selected is {} created){
            var end=p;
            if(shift&&created.Kind==AnnotationKind.Arrow)end=ConstrainArrow(_start,p);
            else if(shift){double side=Math.Min(Math.Abs(p.X-_start.X),Math.Abs(p.Y-_start.Y));end=new(_start.X+Math.Sign(p.X-_start.X)*side,_start.Y+Math.Sign(p.Y-_start.Y)*side);}
            created.X2=end.X;created.Y2=end.Y;
        }else if(_action==DragAction.Move){
            double dx=p.X-_start.X,dy=p.Y-_start.Y;
            var delta=Keyboard.Modifiers.HasFlag(ModifierKeys.Control)?AnnotationEditing.ClampMove(_dragBounds,Session.Document.Viewport,shift&&Math.Abs(dy)>Math.Abs(dx)?0:dx,shift&&Math.Abs(dx)>=Math.Abs(dy)?0:dy):AnnotationEditing.SnapMove(_dragBounds,Session.Document.Viewport,dx,dy,6/Zoom,shift);
            foreach(var original in _originals){var a=Session.Document.Annotations.First(x=>x.Id==original.Id);a.X1=original.X1+delta.X;a.X2=original.X2+delta.X;a.Y1=original.Y1+delta.Y;a.Y2=original.Y2+delta.Y;}
        }else if(_action==DragAction.Resize){
            if(_originals.Length==1&&Selected is {Kind:AnnotationKind.Arrow} a){
                var original=_originals[0];var fixedPoint=_handle==0?new Point(original.X2,original.Y2):new Point(original.X1,original.Y1);var end=shift?ConstrainArrow(fixedPoint,p):p;
                if(_handle==0){a.X1=end.X;a.Y1=end.Y;}else{a.X2=end.X;a.Y2=end.Y;}
            }else{
                var target=AnnotationEditing.Resize(_dragBounds,_handle,p.X,p.Y,shift||(_originals.Length==1&&_originals[0].Kind is AnnotationKind.Text or AnnotationKind.Step));
                foreach(var original in _originals)AnnotationEditing.Transform(Session.Document.Annotations.First(x=>x.Id==original.Id),original,_dragBounds,target);
            }
        }
        InvalidateVisual();
    }
    private Point ConstrainArrow(Point start,Point end)
    {
        var delta=end-start;double angle=Math.Round(Math.Atan2(delta.Y,delta.X)/(Math.PI/4))*Math.PI/4;var view=Session!.Document.Viewport;
        return new(Math.Clamp(start.X+Math.Cos(angle)*delta.Length,view.X,view.Right),Math.Clamp(start.Y+Math.Sin(angle)*delta.Length,view.Y,view.Bottom));
    }
    private Cursor ResizeCursor(int handle)=>_selected.Count==1&&Selected?.Kind==AnnotationKind.Arrow?Cursors.Cross:handle switch{0 or 4=>Cursors.SizeNWSE,2 or 6=>Cursors.SizeNESW,1 or 5=>Cursors.SizeNS,_=>Cursors.SizeWE};
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);if(!Dragging||Session==null)return;OnMouseMove(e);var action=_action;_action=DragAction.None;ReleaseMouseCapture();
        if(action==DragAction.Crop&&_draft is {} crop){
            var region=new PixelRect((int)Math.Floor(crop.X),(int)Math.Floor(crop.Y),(int)Math.Ceiling(crop.Right)-(int)Math.Floor(crop.X),(int)Math.Ceiling(crop.Bottom)-(int)Math.Floor(crop.Y));
            if(region.Width>=2&&region.Height>=2)Session.Change(doc=>doc.ApplyCrop(region));_draft=null;Tool=EditorTool.Select;CropFinished?.Invoke();
        }else if(action==DragAction.Marquee&&_draft is {} marquee){
            if(marquee.Width>=2||marquee.Height>=2)foreach(var a in Session.Document.Annotations)if(marquee.IntersectsWith(EditorRenderer.Bounds(a)))_selected.Add(a.Id);
            _primary=_selected.LastOrDefault();_draft=null;
        }else{
            var invalid=_createdId!=null&&Selected is {} a&&(a.Kind==AnnotationKind.Arrow?Math.Sqrt(a.Width*a.Width+a.Height*a.Height)<2:a.Width<2||a.Height<2);
            if(invalid){Session.Cancel();_selected.Clear();_primary=null;}else Session.Commit();
        }
        _originals=[];Refresh();SelectionChanged?.Invoke();e.Handled=true;
    }
    protected override void OnLostMouseCapture(MouseEventArgs e){base.OnLostMouseCapture(e);if(Dragging)CancelInteraction();}
    public void CancelInteraction()
    {
        if(!Dragging)return;var action=_action;_action=DragAction.None;_draft=null;
        if(action is not (DragAction.Crop or DragAction.Marquee))Session?.Cancel();
        _selected.Clear();_selected.UnionWith(_selectionBefore);_primary=_selected.LastOrDefault();ReleaseMouseCapture();_createdId=null;_originals=[];Refresh();SelectionChanged?.Invoke();
    }
    public void SelectAll(){if(Session==null)return;CancelInteraction();_selected.UnionWith(Session.Document.Annotations.Select(a=>a.Id));_primary=_selected.LastOrDefault();Refresh();SelectionChanged?.Invoke();}
    public void DeleteSelected(){if(Session==null||_selected.Count==0)return;CancelInteraction();var ids=_selected.ToHashSet();Session.Commit();Session.Change(d=>d.Annotations.RemoveAll(a=>ids.Contains(a.Id)));_selected.Clear();_primary=null;Refresh();SelectionChanged?.Invoke();}
    public void Nudge(double x,double y)
    {
        if(Session==null||_selected.Count==0)return;var ids=_selected.ToHashSet();var move=AnnotationEditing.ClampMove(SelectionBox(),Session.Document.Viewport,x,y);
        Session.Commit();Session.Change(d=>{foreach(var a in d.Annotations.Where(a=>ids.Contains(a.Id)))a.Translate(move.X,move.Y);});Refresh();SelectionChanged?.Invoke();
    }
}
