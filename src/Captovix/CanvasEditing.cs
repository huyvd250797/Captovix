namespace Captovix;
public static class CanvasEditing
{
    public static PixelRect SourceBounds(PixelRect view,ImageAppearance appearance)
    {
        if(!appearance.DrawOutsideImage)return view;var l=new ImageLayout(view,appearance);var c=l.Canvas;var corners=new[]{l.ToSource(c.X,c.Y),l.ToSource(c.Right,c.Y),l.ToSource(c.Right,c.Bottom),l.ToSource(c.X,c.Bottom)};int x=(int)Math.Ceiling(corners.Min(p=>p.X)-.00001),y=(int)Math.Ceiling(corners.Min(p=>p.Y)-.00001),right=(int)Math.Floor(corners.Max(p=>p.X)+.00001),bottom=(int)Math.Floor(corners.Max(p=>p.Y)+.00001);return new(x,y,Math.Max(1,right-x),Math.Max(1,bottom-y));
    }
    public static ImageAppearance Expand(PixelRect view,ImageAppearance current,int margin=160)
    {
        var l=new ImageLayout(view,current);var next=current.Copy();next.DrawOutsideImage=true;next.CanvasWidth=checked((int)l.Canvas.Width+2*margin);next.CanvasHeight=checked((int)l.Canvas.Height+2*margin);next.Anchor=4;_=new ImageLayout(view,next);return next;
    }
}
