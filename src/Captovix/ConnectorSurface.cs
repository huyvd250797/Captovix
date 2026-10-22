using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
namespace Captovix;
public sealed partial class EditorSurface
{
    public ConnectorRoute ArrowRoute {get;set;}
    public bool ConnectToShapes {get;set;}=true;
    private void BeginConnector(Annotation a)
    {
        if(!ShapeStudio.IsLine(a.Kind))return;a.Route=ArrowRoute;
        if(ConnectToShapes){var found=ConnectorStudio.FindAnchor(Session!.Document,new(_start.X,_start.Y),12/InteractionScale,a.Id);a.StartAnchor=found.Link;a.X1=found.Point.X;a.Y1=found.Point.Y;}
    }
    private void EndConnector(Annotation a,Point p)
    {
        var found=ConnectToShapes?ConnectorStudio.FindAnchor(Session!.Document,new(p.X,p.Y),12/InteractionScale,a.Id):(null,new AnnotationPoint(p.X,p.Y));
        a.EndAnchor=found.Item1;a.X2=found.Item2.X;a.Y2=found.Item2.Y;
    }
    private void ResizeConnector(Annotation a,Annotation original,Point p,int handle)
    {
        ObjectControl.RestoreGeometry(a,original);
        if(handle==2){a.Control=new(p.X,a.Route==ConnectorRoute.Elbow?(a.Y1+a.Y2)/2:p.Y);return;}
        var found=ConnectToShapes?ConnectorStudio.FindAnchor(Session!.Document,new(p.X,p.Y),12/InteractionScale,a.Id):(null,new AnnotationPoint(p.X,p.Y));
        if(handle==0){a.StartAnchor=found.Item1;a.X1=found.Item2.X;a.Y1=found.Item2.Y;}
        else{a.EndAnchor=found.Item1;a.X2=found.Item2.X;a.Y2=found.Item2.Y;}
    }
    public void SetConnectorRoute(ConnectorRoute route)
    {
        ArrowRoute=route;if(Session==null||!ObjectControl.CanEdit(Session.Document,_selected))return;CancelInteraction();
        Session.Change(d=>{foreach(var a in d.Annotations.Where(a=>_selected.Contains(a.Id)&&ShapeStudio.IsLine(a.Kind))){
            if(Math.Abs(a.Rotation)>1e-8){var start=EditorRenderer.WorldPoint(a,new(a.X1,a.Y1));var end=EditorRenderer.WorldPoint(a,new(a.X2,a.Y2));a.X1=start.X;a.Y1=start.Y;a.X2=end.X;a.Y2=end.Y;a.Rotation=0;}
            a.Route=route;a.Control=null;
        }});Refresh();SelectionChanged?.Invoke();
    }
    public void DetachConnectors()
    {
        if(Session==null||!ObjectControl.CanEdit(Session.Document,_selected))return;CancelInteraction();Session.Change(d=>{foreach(var a in d.Annotations.Where(a=>_selected.Contains(a.Id))){a.StartAnchor=a.EndAnchor=null;}});Refresh();SelectionChanged?.Invoke();
    }
    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonDown(e);if(Session==null)return;CancelInteraction();Session.Commit();Focus();
        var p=SourcePoint(e);var hit=Hit(p);
        if(hit!=null&&!_selected.Contains(hit.Id))SelectIds([hit.Id],hit.Id);
        // Keep a multi-selection on its blank interior; click elsewhere clears it.
        else if(hit==null&&(_selected.Count==0||!new Rect(SelectionBox().X,SelectionBox().Y,SelectionBox().Width,SelectionBox().Height).Contains(p)))SelectIds([]);
    }
}
