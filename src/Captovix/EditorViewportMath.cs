namespace Captovix;

public static class EditorViewportMath
{
    public static double WheelZoom(double current,int delta)=>Math.Clamp(current*Math.Pow(1.2,Math.Clamp(delta/120d,-4,4)),.02,8);
    public static double AnchoredOffset(double currentOffset,double contentOrigin,double sourceAnchor,double newZoom,double viewportAnchor,double maximum)
        =>Math.Clamp(currentOffset+contentOrigin+sourceAnchor*newZoom-viewportAnchor,0,Math.Max(0,maximum));
    public static double PanOffset(double originalOffset,double start,double current,double maximum)=>Math.Clamp(originalOffset+start-current,0,Math.Max(0,maximum));
}

public static class CropGeometry
{
    public static bool IsValid(PixelRect rect,PixelRect bounds)=>rect.Width>=2&&rect.Height>=2&&rect.X>=bounds.X&&rect.Y>=bounds.Y&&(long)rect.X+rect.Width<=bounds.Right&&(long)rect.Y+rect.Height<=bounds.Bottom;
    public static PixelRect FromPoints(double x1,double y1,double x2,double y2,PixelRect bounds,bool square=false)
    {
        x1=Math.Clamp(x1,bounds.X,bounds.Right);y1=Math.Clamp(y1,bounds.Y,bounds.Bottom);x2=Math.Clamp(x2,bounds.X,bounds.Right);y2=Math.Clamp(y2,bounds.Y,bounds.Bottom);
        if(square){var length=Math.Min(Math.Abs(x2-x1),Math.Abs(y2-y1));x2=x1+Math.Sign(x2-x1)*length;y2=y1+Math.Sign(y2-y1)*length;}
        int left=(int)Math.Floor(Math.Min(x1,x2)),top=(int)Math.Floor(Math.Min(y1,y2));
        var rect=new PixelRect(left,top,(int)Math.Ceiling(Math.Max(x1,x2))-left,(int)Math.Ceiling(Math.Max(y1,y2))-top).Intersect(bounds);
        return square?rect with{Width=Math.Min(rect.Width,rect.Height),Height=Math.Min(rect.Width,rect.Height)}:rect;
    }
    public static PixelRect Move(PixelRect rect,double dx,double dy,PixelRect bounds)=>rect with{
        X=(int)Math.Round(Math.Clamp(rect.X+dx,bounds.X,bounds.Right-rect.Width),MidpointRounding.AwayFromZero),
        Y=(int)Math.Round(Math.Clamp(rect.Y+dy,bounds.Y,bounds.Bottom-rect.Height),MidpointRounding.AwayFromZero)};
    public static PixelRect Resize(PixelRect rect,int handle,double x,double y,PixelRect bounds,bool ratio)
    {
        x=Math.Clamp(x,bounds.X,bounds.Right);y=Math.Clamp(y,bounds.Y,bounds.Bottom);
        bool left=handle is 0 or 6 or 7,top=handle is 0 or 1 or 2,right=handle is 2 or 3 or 4,bottom=handle is 4 or 5 or 6;
        double l=left?Math.Min(x,rect.Right-2):rect.X,t=top?Math.Min(y,rect.Bottom-2):rect.Y;
        double r=right?Math.Max(x,rect.X+2):rect.Right,b=bottom?Math.Max(y,rect.Y+2):rect.Bottom;
        if(ratio&&(handle is 0 or 2 or 4 or 6)){
            double anchorX=left?rect.Right:rect.X,anchorY=top?rect.Bottom:rect.Y;
            double maxWidth=left?anchorX-bounds.X:bounds.Right-anchorX,maxHeight=top?anchorY-bounds.Y:bounds.Bottom-anchorY;
            double minimum=Math.Max(2d/rect.Width,2d/rect.Height),maximum=Math.Min(maxWidth/rect.Width,maxHeight/rect.Height);
            double scale=Math.Clamp(Math.Max((r-l)/rect.Width,(b-t)/rect.Height),Math.Min(minimum,maximum),maximum);
            int w=Math.Clamp((int)Math.Round(rect.Width*scale),2,(int)maxWidth),h=Math.Clamp((int)Math.Round(rect.Height*scale),2,(int)maxHeight);
            return new((int)(left?anchorX-w:anchorX),(int)(top?anchorY-h:anchorY),w,h);
        }
        int ix=(int)Math.Round(l),iy=(int)Math.Round(t),ir=(int)Math.Round(r),ib=(int)Math.Round(b);
        return new(ix,iy,ir-ix,ib-iy);
    }
}
