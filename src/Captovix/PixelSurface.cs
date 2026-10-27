using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
namespace Captovix;
public sealed partial class EditorSurface
{
    public PixelSelection? PixelRegion {get;private set;}
    public bool IsPixelTool=>Tool is EditorTool.PixelRectangle or EditorTool.PixelEllipse or EditorTool.PixelLasso;
    private PixelSelection? _pixelBefore;
    private bool _pixelMove;
    private int _pixelHandle=-1;
    public void ClearPixelRegion(){PixelRegion=null;InvalidateVisual();SelectionChanged?.Invoke();}
    private bool PixelDown(Point p)
    {
        if(!IsPixelTool||Session==null)return false;var v=Session.Document.Viewport;p=new(Math.Clamp(p.X,v.X,v.Right),Math.Clamp(p.Y,v.Y,v.Bottom));
        _pixelHandle=PixelRegion is {} selected&&selected.Kind!=PixelSelectionKind.Lasso?HitCropHandle(selected.Bounds,p):-1;_pixelBefore=PixelRegion?.Copy();_pixelMove=PixelRegion?.Contains(p.X,p.Y)==true;_start=p;
        if(!_pixelMove&&_pixelHandle<0)PixelRegion=new(){Kind=Tool==EditorTool.PixelEllipse?PixelSelectionKind.Ellipse:Tool==EditorTool.PixelLasso?PixelSelectionKind.Lasso:PixelSelectionKind.Rectangle,Points=[new(p.X,p.Y)],Bounds=new((int)p.X,(int)p.Y,0,0)};
        _action=DragAction.PixelRegion;CaptureMouse();InvalidateVisual();return true;
    }
    private void PixelMove(Point p)
    {
        if(Session==null||PixelRegion==null)return;var view=Session.Document.Viewport;p=new(Math.Clamp(p.X,view.X,view.Right),Math.Clamp(p.Y,view.Y,view.Bottom));
        if(_pixelHandle>=0&&_pixelBefore!=null)PixelRegion.Bounds=CropGeometry.Resize(_pixelBefore.Bounds,_pixelHandle,p.X,p.Y,view,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
        else if(_pixelMove&&_pixelBefore!=null){PixelRegion=_pixelBefore.Copy();PixelRegion.Translate(p.X-_start.X,p.Y-_start.Y,view);}
        else if(PixelRegion.Kind==PixelSelectionKind.Lasso){var last=PixelRegion.Points[^1];if(PixelRegion.Points.Count<8192&&(new Point(last.X,last.Y)-p).Length>=1)PixelRegion.Points.Add(new(p.X,p.Y));var points=PixelRegion.Points;PixelRegion.Bounds=CropGeometry.FromPoints(points.Min(x=>x.X),points.Min(x=>x.Y),points.Max(x=>x.X),points.Max(x=>x.Y),view);}
        else PixelRegion.Bounds=CropGeometry.FromPoints(_start.X,_start.Y,p.X,p.Y,view,Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
        InvalidateVisual();
    }
    private void DrawPixelRegion(DrawingContext dc)
    {
        if(!IsPixelTool||PixelRegion is not {} region)return;var b=region.Bounds;Geometry g;
        if(region.Kind==PixelSelectionKind.Lasso){var path=new StreamGeometry();if(region.Points.Count>1)using(var c=path.Open()){c.BeginFigure(new(region.Points[0].X,region.Points[0].Y),true,true);c.PolyLineTo(region.Points.Skip(1).Select(p=>new Point(p.X,p.Y)).ToArray(),true,false);}g=path;}
        else g=region.Kind==PixelSelectionKind.Ellipse?new EllipseGeometry(new Rect(b.X,b.Y,b.Width,b.Height)):new RectangleGeometry(new Rect(b.X,b.Y,b.Width,b.Height));
        if(region.Kind!=PixelSelectionKind.Lasso)foreach(var handle in CropHandles(b))dc.DrawRectangle(Brushes.White,new Pen(Brushes.Teal,1/InteractionScale),new Rect(handle.X-3/InteractionScale,handle.Y-3/InteractionScale,6/InteractionScale,6/InteractionScale));
        dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(30,0,139,146)),new Pen(Brushes.White,2/InteractionScale),g);dc.DrawGeometry(null,new Pen(Brushes.Teal,1/InteractionScale){DashStyle=DashStyles.Dash},g);
    }
}
