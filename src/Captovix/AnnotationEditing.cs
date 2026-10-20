namespace Captovix;

public readonly record struct AnnotationBox(double X,double Y,double Width,double Height)
{
    public double Right=>X+Width;
    public double Bottom=>Y+Height;
    public double CenterX=>X+Width/2;
    public double CenterY=>Y+Height/2;
}
public enum AnnotationAlignment { Left, CenterX, Right, Top, CenterY, Bottom }
public static class AnnotationEditing
{
    public static AnnotationBox Union(IEnumerable<AnnotationBox> boxes)
    {
        var list=boxes.ToArray();if(list.Length==0)return default;
        var x=list.Min(b=>b.X);var y=list.Min(b=>b.Y);
        return new(x,y,list.Max(b=>b.Right)-x,list.Max(b=>b.Bottom)-y);
    }
    // Multiple objects align to their union. A single object aligns to the crop/image viewport.
    public static void Align(EditorDocument doc,IReadOnlyCollection<string> ids,AnnotationAlignment alignment,Func<Annotation,AnnotationBox> bounds)
    {
        var items=doc.Annotations.Where(a=>ids.Contains(a.Id)).ToArray();if(items.Length==0)return;
        var view=doc.Viewport;var target=items.Length>1?Union(items.Select(bounds)):new AnnotationBox(view.X,view.Y,view.Width,view.Height);
        foreach(var a in items){var b=bounds(a);var dx=0d;var dy=0d;
            switch(alignment){
                case AnnotationAlignment.Left:dx=target.X-b.X;break;
                case AnnotationAlignment.CenterX:dx=target.CenterX-b.CenterX;break;
                case AnnotationAlignment.Right:dx=target.Right-b.Right;break;
                case AnnotationAlignment.Top:dy=target.Y-b.Y;break;
                case AnnotationAlignment.CenterY:dy=target.CenterY-b.CenterY;break;
                case AnnotationAlignment.Bottom:dy=target.Bottom-b.Bottom;break;
            }
            a.Translate(dx,dy);
        }
    }
    public static (double X,double Y) ClampMove(AnnotationBox box,PixelRect view,double dx,double dy)
    {
        // Objects larger than a crop remain movable without jumping when first touched.
        if(box.Width<=view.Width)dx=Math.Clamp(dx,Math.Min(0,view.X-box.X),Math.Max(0,view.Right-box.Right));
        if(box.Height<=view.Height)dy=Math.Clamp(dy,Math.Min(0,view.Y-box.Y),Math.Max(0,view.Bottom-box.Bottom));
        return(dx,dy);
    }
    public static (double X,double Y) SnapMove(AnnotationBox b,PixelRect view,double dx,double dy,double tolerance,bool lockAxis)
    {
        if(lockAxis){if(Math.Abs(dx)>=Math.Abs(dy))dy=0;else dx=0;}
        double Snap(double start,double size,double low,double high,double delta){
            var offsets=new[]{low-(start+delta),(low+high)/2-(start+size/2+delta),high-(start+size+delta)};
            var nearest=offsets.MinBy(Math.Abs);return Math.Abs(nearest)<=tolerance?delta+nearest:delta;
        }
        if(!lockAxis||dx!=0)dx=Snap(b.X,b.Width,view.X,view.Right,dx);
        if(!lockAxis||dy!=0)dy=Snap(b.Y,b.Height,view.Y,view.Bottom,dy);
        return ClampMove(b,view,dx,dy);
    }
    // Handles clockwise from top-left: NW,N,NE,E,SE,S,SW,W.
    public static AnnotationBox Resize(AnnotationBox b,int handle,double x,double y,bool preserveRatio)
    {
        var left=b.X;var top=b.Y;var right=b.Right;var bottom=b.Bottom;
        if(handle is 0 or 6 or 7)left=Math.Min(x,right-2);
        if(handle is 2 or 3 or 4)right=Math.Max(x,left+2);
        if(handle is 0 or 1 or 2)top=Math.Min(y,bottom-2);
        if(handle is 4 or 5 or 6)bottom=Math.Max(y,top+2);
        if(preserveRatio&&(handle is 0 or 2 or 4 or 6)&&b.Width>0&&b.Height>0){
            var ratio=b.Width/b.Height;var width=right-left;var height=bottom-top;
            if(width/height>ratio)height=width/ratio;else width=height*ratio;
            if(handle is 0 or 6)left=right-width;else right=left+width;
            if(handle is 0 or 2)top=bottom-height;else bottom=top+height;
        }
        return new(left,top,right-left,bottom-top);
    }
    public static void Transform(Annotation target,Annotation original,AnnotationBox from,AnnotationBox to)
    {
        var sx=to.Width/Math.Max(1,from.Width);var sy=to.Height/Math.Max(1,from.Height);
        target.X1=to.X+(original.X1-from.X)*sx;target.X2=to.X+(original.X2-from.X)*sx;
        target.Y1=to.Y+(original.Y1-from.Y)*sy;target.Y2=to.Y+(original.Y2-from.Y)*sy;
        if(original.Kind is AnnotationKind.Text or AnnotationKind.Step)target.FontSize=Math.Clamp(original.FontSize*Math.Min(sx,sy),8,240);
    }
}
