namespace Captovix;
/// <summary>The first pointer pins the tail; both body dimensions follow the dragged endpoint independently.</summary>
public static class CalloutGesture
{
    public const double MinimumSize=36.001; // Avoid rounding below the persisted 36px minimum.
    public static void Place(Annotation a,double tailX,double tailY,double endX,double endY)
    {
        double dx=endX-tailX,dy=endY-tailY;
        double gapX=System.Math.Min(18,System.Math.Abs(dx)*.2),gapY=System.Math.Min(18,System.Math.Abs(dy)*.2);
        double width=System.Math.Max(MinimumSize,System.Math.Abs(dx)-gapX),height=System.Math.Max(MinimumSize,System.Math.Abs(dy)-gapY);
        a.X1=dx>=0?tailX+gapX:tailX-gapX-width;
        a.Y1=dy>=0?tailY+gapY:tailY-gapY-height;
        a.X2=a.X1+width;a.Y2=a.Y1+height;a.TailX=tailX;a.TailY=tailY;
    }
}
