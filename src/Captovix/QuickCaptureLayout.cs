namespace Captovix;
public static class QuickCaptureLayout
{
    public static long Overlap(PixelRect a,PixelRect b){var c=a.Intersect(b);return (long)Math.Max(0,c.Width)*Math.Max(0,c.Height);}
    public static PixelRect Toolbar(PixelRect screen,PixelRect selection,int width,int height,int pointerX,int pointerY)
    {
        var outside=CaptureSelection.Toolbar(screen,selection,width,height,6);
        if(Overlap(outside,selection)==0)return outside;
        width=Math.Clamp(width,1,screen.Width);height=Math.Clamp(height,1,screen.Height);
        PixelRect[] corners=[new(screen.X,screen.Y,width,height),new(screen.Right-width,screen.Y,width,height),new(screen.X,screen.Bottom-height,width,height),new(screen.Right-width,screen.Bottom-height,width,height)];
        return corners.OrderBy(c=>Overlap(c,selection)).ThenByDescending(c=>Math.Pow(c.X+c.Width/2d-pointerX,2)+Math.Pow(c.Y+c.Height/2d-pointerY,2)).First();
    }
    public static PixelRect KeyboardRegion(PixelRect desktop,int x,int y)=>new(Math.Clamp(x,desktop.X,desktop.Right-8),Math.Clamp(y,desktop.Y,desktop.Bottom-8),8,8);
}
