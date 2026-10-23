using System.Windows;
using System.Windows.Media;
namespace Captovix;
public static partial class EditorRenderer
{
    private static void DrawObjectShadow(DrawingContext dc,Annotation a)
    {
        if(!StudioStyle.SupportsShadow(a.Kind)||!a.Shadow.Enabled||a.Shadow.Opacity<=0||a.Kind!=AnnotationKind.ImageLayer&&(!a.FillEnabled||a.FillOpacity<=0)&&(!a.StrokeEnabled||a.StrokeOpacity<=0))return;
        Geometry body=a.Kind==AnnotationKind.ImageLayer?new RectangleGeometry(new Rect(a.Left,a.Top,a.Width,a.Height),a.ImageStyle?.Radius??0,a.ImageStyle?.Radius??0):a.Kind==AnnotationKind.Callout?CalloutGeometry(a):ShapeGeometry(a);
        if(a.Kind!=AnnotationKind.ImageLayer&&(!a.FillEnabled||a.FillOpacity<=0))body=body.GetWidenedPathGeometry(new Pen(Brushes.Black,a.Stroke));
        var group=new GeometryGroup();group.Children.Add(body);group.Transform=ObjectTransform(a);
        var offset=StudioStyle.Offset(a.Shadow.Direction,a.Shadow.Distance);dc.PushTransform(new TranslateTransform(offset.X,offset.Y));
        int layers=a.Shadow.Blur>0?16:1;double alpha=100*(1-Math.Pow(1-a.Shadow.Opacity/100,1d/layers));var ink=ShapeInk(a.Shadow.Color,alpha);
        for(int n=layers;n>=1;n--){double spread=a.Shadow.Blur*n/layers;dc.DrawGeometry(ink,spread>0?new Pen(ink,spread*2){LineJoin=PenLineJoin.Round}:null,group);}
        dc.Pop();
    }
}
