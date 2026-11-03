namespace Captovix;
/// <summary>Tail-first creation. The anchor stays in source pixels in every drag quadrant.</summary>
public static class CalloutGesture
{
    public static void Place(Annotation a,double tailX,double tailY,double endX,double endY)
    {
        double dx=endX-tailX,dy=endY-tailY;
        double width=System.Math.Max(172,System.Math.Abs(dx)-24),height=System.Math.Max(96,System.Math.Abs(dy)*2);
        // Put the tail on the side of the body, vertically centered at the initial pointer.
        a.X1=dx<0?tailX-24-width:tailX+24;a.Y1=tailY-height/2;
        a.X2=a.X1+width;a.Y2=a.Y1+height;a.TailX=tailX;a.TailY=tailY;
    }
}
