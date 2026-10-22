using System.Windows;
using System.Windows.Media;
namespace Captovix;
public static partial class EditorRenderer
{
    public static AnnotationBox LocalBox(Annotation a){var b=LocalBounds(a);return new(b.X,b.Y,b.Width,b.Height);}
    public static Rect Bounds(Annotation a){var b=ObjectControl.RotatedBounds(LocalBox(a),a.Rotation);return new(b.X,b.Y,b.Width,b.Height);}
    public static RotateTransform ObjectTransform(Annotation a){var b=LocalBounds(a);return new(a.Rotation,b.Left+b.Width/2,b.Top+b.Height/2);}
    public static Point WorldPoint(Annotation a,Point p){var b=LocalBox(a);var q=ObjectControl.RotatePoint(new(p.X,p.Y),new(b.CenterX,b.CenterY),a.Rotation);return new(q.X,q.Y);}
    public static Point LocalPoint(Annotation a,Point p){var b=LocalBox(a);var q=ObjectControl.RotatePoint(new(p.X,p.Y),new(b.CenterX,b.CenterY),-a.Rotation);return new(q.X,q.Y);}
    public static void DrawAnnotation(DrawingContext dc,Annotation a)
    {
        if(!a.Visible)return;
        if(Math.Abs(a.Rotation)<1e-9){DrawLocalAnnotation(dc,a);return;}
        dc.PushTransform(ObjectTransform(a));try{DrawLocalAnnotation(dc,a);}finally{dc.Pop();}
    }
    public static bool Hit(Annotation a,Point p,double tolerance)=>a.Visible&&!a.Locked&&HitLocal(a,LocalPoint(a,p),tolerance);
}
