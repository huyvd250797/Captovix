using System.Windows;
namespace Captovix;
public sealed partial class EditorSurface
{
    private void PlaceStamp(Point point)
    {
        if(Session==null||StampPng==null||Session.Document.Annotations.Count>=10000)return;
        Session.Commit();double size=Math.Min(96,Math.Max(32,Math.Min(Session.Document.Viewport.Width,Session.Document.Viewport.Height)/4d));
        var a=new Annotation{Kind=AnnotationKind.ImageLayer,Name=StampName,X1=point.X-size/2,Y1=point.Y-size/2,X2=point.X+size/2,Y2=point.Y+size/2,PixelPng=StampPng,ImageStyle=new()};
        Session.Change(d=>{d.Annotations.Add(a);d.Appearance.DrawOutsideImage=true;});SelectIds([a.Id]);FitSelectionCanvas();Refresh();
    }
}
