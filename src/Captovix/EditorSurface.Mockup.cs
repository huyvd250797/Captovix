using System.Windows;
namespace Captovix;
public sealed partial class EditorSurface
{
    public event Action<PixelRect>? MockupRequested;
    public event Action? MockupEditRequested;
    private void BeginMockup(Point p)
    {
        if(Session==null)return;Session.Commit();_selectionBefore=new(_selected);_selected.Clear();_primary=null;
        _start=new(Math.Clamp(p.X,Session.Document.Viewport.X,Session.Document.Viewport.Right),Math.Clamp(p.Y,Session.Document.Viewport.Y,Session.Document.Viewport.Bottom));_draft=new(_start,_start);_action=DragAction.MockupRegion;CaptureMouse();InvalidateVisual();SelectionChanged?.Invoke();
    }
    private void MoveMockup(Point p)
    {
        var view=Session!.Document.Viewport;p=new(Math.Clamp(p.X,view.X,view.Right),Math.Clamp(p.Y,view.Y,view.Bottom));_draft=new Rect(_start,p);InvalidateVisual();
    }
    private void EndMockup()
    {
        var draft=_draft;_draft=null;_action=DragAction.None;ReleaseMouseCapture();InvalidateVisual();
        if(draft is {} r&&r.Width>=8&&r.Height>=8){var region=MockupStudio.Region(r.Left,r.Top,r.Right,r.Bottom,Session!.Document.Viewport);MockupRequested?.Invoke(region);}
    }
}
