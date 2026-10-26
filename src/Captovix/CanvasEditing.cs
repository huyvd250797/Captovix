namespace Captovix;
public static class CanvasEditing
{
    public static PixelRect SourceBounds(PixelRect view,ImageAppearance appearance)
    {
        if(!appearance.DrawOutsideImage&&appearance.OutputCrop==null)return view;var l=new ImageLayout(view,appearance);var c=new ImageRect(0,0,l.Width,l.Height);var corners=new[]{l.ToSource(c.X,c.Y),l.ToSource(c.Right,c.Y),l.ToSource(c.Right,c.Bottom),l.ToSource(c.X,c.Bottom)};int x=(int)Math.Ceiling(corners.Min(p=>p.X)-.00001),y=(int)Math.Ceiling(corners.Min(p=>p.Y)-.00001),right=(int)Math.Floor(corners.Max(p=>p.X)+.00001),bottom=(int)Math.Floor(corners.Max(p=>p.Y)+.00001);return new(x,y,Math.Max(1,right-x),Math.Max(1,bottom-y));
    }
    public static ImageAppearance Expand(PixelRect view,ImageAppearance current,int margin=160)=>Expand(view,current,margin,margin,margin,margin);
    public static ImageAppearance Expand(PixelRect view,ImageAppearance current,int left,int top,int right,int bottom)
    {
        if(new[]{left,top,right,bottom}.Any(n=>n<0||n>32768))throw new ArgumentOutOfRangeException(nameof(left));
        var next=current.Copy();next.DrawOutsideImage=true;
        next.ExtendLeft=checked(next.ExtendLeft+left);next.ExtendTop=checked(next.ExtendTop+top);next.ExtendRight=checked(next.ExtendRight+right);next.ExtendBottom=checked(next.ExtendBottom+bottom);
        if(next.OutputCrop is {} crop)next.OutputCrop=crop with{Width=checked(crop.Width+left+right),Height=checked(crop.Height+top+bottom)};
        _=new ImageLayout(view,next);return next;
    }
    public static ImageAppearance CropOutput(PixelRect view,ImageAppearance current,PixelRect crop)
    {
        var layout=new ImageLayout(view,current);if(crop.Width<2||crop.Height<2||crop.X<0||crop.Y<0||crop.Right>layout.Width||crop.Bottom>layout.Height)throw new ArgumentException("Crop phải nằm trong toàn canvas và tối thiểu 2 × 2 px.");
        var next=current.Copy();var prior=current.OutputCrop;next.OutputCrop=new((prior?.X??0)+crop.X,(prior?.Y??0)+crop.Y,crop.Width,crop.Height);next.DrawOutsideImage=true;_=new ImageLayout(view,next);return next;
    }
}
