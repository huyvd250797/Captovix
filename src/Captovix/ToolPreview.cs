using System.Windows;
using System.Windows.Media;
namespace Captovix;
public static class ToolPreview
{
    // DrawingImage sizes to actual rendered bounds: step/text/callout tails cannot be clipped by an artificial origin.
    public static DrawingGroup Create(Annotation a)
    {
        var group=new DrawingGroup();using(var dc=group.Open()){
            if(a.Kind is AnnotationKind.Blur or AnnotationKind.Pixelate or AnnotationKind.Spotlight or AnnotationKind.Magnifier){var ink=EditorRenderer.Ink(a.Color);var pen=new Pen(ink,2);dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(229,233,240)),null,new Rect(0,0,80,48),6,6);
                if(a.Kind==AnnotationKind.Magnifier){dc.DrawEllipse(Brushes.White,pen,new Point(34,22),16,16);dc.DrawLine(pen,new(46,34),new(63,48));dc.DrawLine(pen,new(26,22),new(42,22));dc.DrawLine(pen,new(34,14),new(34,30));}
                else if(a.Kind==AnnotationKind.Spotlight){dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(45,50,62)),null,new Rect(0,0,80,48),6,6);dc.DrawEllipse(Brushes.White,pen,new(40,24),20,17);}
                else for(int i=0;i<4;i++){dc.PushOpacity(.25+i*.12);dc.DrawLine(new Pen(ink,3),new(10,10+i*9),new(70-i*5,10+i*9));dc.Pop();}
            }else EditorRenderer.DrawAnnotation(dc,a);
        }group.Freeze();return group;
    }
}
