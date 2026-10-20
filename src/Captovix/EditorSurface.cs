using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Captovix;
public enum EditorTool { Select, Arrow, Rectangle, Ellipse, Text, Highlight, Step, Crop }
public sealed class EditorSurface : FrameworkElement
{
    public EditorSession? Session {get;private set;}
    public BitmapSource? Original {get;private set;}
    public string? SelectedId {get;private set;}
    public Annotation? Selected=>Session?.Document.Annotations.FirstOrDefault(a=>a.Id==SelectedId);
    public EditorTool Tool {get;set;}
    public string InkColor {get;set;}="#008B92";
    public double StrokeSize {get;set;}=4;
    public double TextSize {get;set;}=28;
    public double Zoom {get;private set;}=1;
    public event Action? SelectionChanged;
    public event Action? VisualChanged;
    public event Action? TextRequested;
    public event Action? CropFinished;
    private bool _drag;
    private Point _start;
    private Annotation? _originalDrag;
    private string? _createdId;
    private int _handle=-1;
    private Rect? _cropDraft;
    public EditorSurface(){Focusable=true;ClipToBounds=true;Cursor=Cursors.Arrow;RenderOptions.SetBitmapScalingMode(this,BitmapScalingMode.HighQuality);}
    public void Load(BitmapSource? source,EditorSession? session)
    {
        CancelInteraction();Original=source;Session=session;SelectedId=null;Refresh();SelectionChanged?.Invoke();
    }
    public void SetZoom(double zoom){Zoom=Math.Clamp(zoom,.02,8);Refresh();}
    public void Refresh()
    {
        var v=Session?.Document.Viewport;if(v!=null){Width=v.Value.Width*Zoom;Height=v.Value.Height*Zoom;}else{Width=Height=1;}
        if(Selected==null)SelectedId=null;InvalidateVisual();VisualChanged?.Invoke();
    }
    public void NotifySelection()=>SelectionChanged?.Invoke();
    public BitmapSource Flatten(int maxWidth=0)=>Original!=null&&Session!=null?EditorRenderer.Render(Original,Session.Document,maxWidth):throw new InvalidOperationException("Chưa có ảnh để xuất.");
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);if(Original==null||Session==null)return;var view=Session.Document.Viewport;
        dc.PushTransform(new ScaleTransform(Zoom,Zoom));dc.PushTransform(new TranslateTransform(-view.X,-view.Y));
        EditorRenderer.Draw(dc,Original,Session.Document);
        if(Selected is {} selected){
            var r=EditorRenderer.Bounds(selected);r.Inflate(5/Zoom,5/Zoom);dc.DrawRectangle(null,new Pen(Brushes.DodgerBlue,1/Zoom){DashStyle=DashStyles.Dash},r);
            foreach(var handle in Handles(selected)){var size=7/Zoom;dc.DrawRectangle(Brushes.White,new Pen(Brushes.DodgerBlue,1/Zoom),new(handle.X-size/2,handle.Y-size/2,size,size));}
        }
        if(_cropDraft is {} crop){
            var v=new Rect(view.X,view.Y,view.Width,view.Height);var shade=new SolidColorBrush(Color.FromArgb(120,10,20,30));
            dc.DrawRectangle(shade,null,new Rect(v.Left,v.Top,v.Width,Math.Max(0,crop.Top-v.Top)));
            dc.DrawRectangle(shade,null,new Rect(v.Left,crop.Bottom,v.Width,Math.Max(0,v.Bottom-crop.Bottom)));
            dc.DrawRectangle(shade,null,new Rect(v.Left,crop.Top,Math.Max(0,crop.Left-v.Left),crop.Height));
            dc.DrawRectangle(shade,null,new Rect(crop.Right,crop.Top,Math.Max(0,v.Right-crop.Right),crop.Height));
            dc.DrawRectangle(null,new Pen(Brushes.White,1.5/Zoom){DashStyle=DashStyles.Dash},crop);
        }
        dc.Pop();dc.Pop();
    }
    private Point SourcePoint(MouseEventArgs e)
    {
        var p=e.GetPosition(this);var v=Session!.Document.Viewport;return new(Math.Clamp(p.X/Zoom+v.X,v.X,v.Right),Math.Clamp(p.Y/Zoom+v.Y,v.Y,v.Bottom));
    }
    private static Point[] Handles(Annotation a)=>a.Kind switch{
        AnnotationKind.Arrow=>[new(a.X1,a.Y1),new(a.X2,a.Y2)],
        AnnotationKind.Rectangle or AnnotationKind.Ellipse or AnnotationKind.Highlight=>[new(a.X1,a.Y1),new(a.X2,a.Y2)],_=>[]};
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);if(Session==null)return;Focus();_start=SourcePoint(e);_createdId=null;_originalDrag=null;_handle=-1;
        if(Tool==EditorTool.Select){
            var handles=Selected is {} s?Handles(s):[];
            for(var n=0;n<handles.Length;n++)if((handles[n]-_start).Length<10/Zoom){_handle=n;break;}
            var hit=_handle>=0?Selected:Session.Document.Annotations.LastOrDefault(a=>EditorRenderer.Hit(a,_start,6/Zoom));
            SelectedId=hit?.Id;SelectionChanged?.Invoke();
            if(hit==null){InvalidateVisual();return;}_originalDrag=hit.Copy();Session.Begin();
        }else if(Tool==EditorTool.Crop){SelectedId=null;_cropDraft=new(_start,_start);SelectionChanged?.Invoke();}
        else{
            Session.Begin();var a=new Annotation{Kind=Enum.Parse<AnnotationKind>(Tool.ToString()),X1=_start.X,Y1=_start.Y,X2=_start.X,Y2=_start.Y,Color=InkColor,Stroke=StrokeSize,FontSize=TextSize};
            if(Tool==EditorTool.Step)a.Number=Session.Document.NextStep();if(Tool==EditorTool.Text)a.Text="Nhập nội dung";
            Session.Document.Annotations.Add(a);SelectedId=_createdId=a.Id;SelectionChanged?.Invoke();
            if(Tool is EditorTool.Text or EditorTool.Step){Session.Commit();InvalidateVisual();if(Tool==EditorTool.Text)TextRequested?.Invoke();return;}
        }
        _drag=true;CaptureMouse();InvalidateVisual();e.Handled=true;
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);if(!_drag||Session==null)return;var p=SourcePoint(e);
        if(Tool==EditorTool.Crop){_cropDraft=new(_start,p);}
        else if(Selected is {} a){
            if(_originalDrag is {} original){
                a.X1=original.X1;a.Y1=original.Y1;a.X2=original.X2;a.Y2=original.Y2;
                if(_handle==0){a.X1=p.X;a.Y1=p.Y;}else if(_handle==1){a.X2=p.X;a.Y2=p.Y;}else a.Translate(p.X-_start.X,p.Y-_start.Y);
            }else{a.X2=p.X;a.Y2=p.Y;}
        }
        InvalidateVisual();
    }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);if(!_drag||Session==null)return;
        OnMouseMove(e);_drag=false;ReleaseMouseCapture();
        if(Tool==EditorTool.Crop&&_cropDraft is {} crop){
            var region=new PixelRect((int)Math.Floor(crop.X),(int)Math.Floor(crop.Y),(int)Math.Ceiling(crop.Right)-(int)Math.Floor(crop.X),(int)Math.Ceiling(crop.Bottom)-(int)Math.Floor(crop.Y));
            if(region.Width>=2&&region.Height>=2)Session.Change(doc=>doc.ApplyCrop(region));_cropDraft=null;Tool=EditorTool.Select;Refresh();CropFinished?.Invoke();
        }else{
            if(_createdId!=null&&Selected is {} a&&a.Width<2&&a.Height<2){Session.Cancel();SelectedId=null;}else Session.Commit();
            Refresh();SelectionChanged?.Invoke();
        }
        e.Handled=true;
    }
    protected override void OnLostMouseCapture(MouseEventArgs e){base.OnLostMouseCapture(e);if(_drag)CancelInteraction();}
    public void CancelInteraction()
    {
        if(!_drag&&_cropDraft==null)return;_drag=false;_cropDraft=null;Session?.Cancel();ReleaseMouseCapture();_createdId=null;SelectedId=null;Refresh();SelectionChanged?.Invoke();
    }
    public void DeleteSelected(){if(Session==null||SelectedId==null)return;var id=SelectedId;Session.Change(d=>d.Annotations.RemoveAll(a=>a.Id==id));SelectedId=null;Refresh();SelectionChanged?.Invoke();}
    public void Nudge(double x,double y){if(Session==null||Selected==null)return;var id=SelectedId;Session.Change(d=>d.Annotations.First(a=>a.Id==id).Translate(x,y));Refresh();}
}
