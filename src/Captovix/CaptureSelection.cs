namespace Captovix;
// All coordinates are physical desktop pixels, including negative monitor origins.
public enum CaptureHandle { New, Move, Left, Right, Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight }
public static class CaptureSelection
{
    public static bool Valid(PixelRect r)=>r.Width>=8&&r.Height>=8;
    // Intersect in physical pixels without overflowing intermediate right/bottom values.
    public static PixelRect Clip(PixelRect value,PixelRect bounds)
    {
        if(value.IsEmpty||bounds.IsEmpty)return default;
        long left=Math.Max((long)value.X,bounds.X),top=Math.Max((long)value.Y,bounds.Y);
        long right=Math.Min((long)value.X+value.Width,(long)bounds.X+bounds.Width),bottom=Math.Min((long)value.Y+value.Height,(long)bounds.Y+bounds.Height);
        return right<=left||bottom<=top?default:new(checked((int)left),checked((int)top),checked((int)(right-left)),checked((int)(bottom-top)));
    }
    // Moving stops at the frame edge; it does not progressively shrink the region.
    public static PixelRect MoveInside(PixelRect value,PixelRect bounds)
    {
        if(value.IsEmpty||bounds.IsEmpty)return default;
        int width=Math.Min(value.Width,bounds.Width),height=Math.Min(value.Height,bounds.Height);
        return new((int)Math.Clamp((long)value.X,bounds.X,(long)bounds.X+bounds.Width-width),(int)Math.Clamp((long)value.Y,bounds.Y,(long)bounds.Y+bounds.Height-height),width,height);
    }
    public static PixelRect FitRatio(PixelRect value,PixelRect bounds,int anchorX,int anchorY)
    {
        if(value.IsEmpty||bounds.IsEmpty)return default;
        long ax=Math.Clamp((long)anchorX,bounds.X,(long)bounds.X+bounds.Width),ay=Math.Clamp((long)anchorY,bounds.Y,(long)bounds.Y+bounds.Height);
        bool left=value.X<anchorX,top=value.Y<anchorY;
        long roomX=left?ax-bounds.X:(long)bounds.X+bounds.Width-ax,roomY=top?ay-bounds.Y:(long)bounds.Y+bounds.Height-ay;
        double scale=Math.Min(1,Math.Min(roomX/(double)value.Width,roomY/(double)value.Height));
        int width=(int)Math.Floor(value.Width*scale+1e-9),height=(int)Math.Floor(value.Height*scale+1e-9);
        return Clip(new((int)(left?ax-width:ax),(int)(top?ay-height:ay),width,height),bounds);
    }
    public static CaptureHandle Hit(PixelRect r,int x,int y,int tolerance)
    {
        if(!Valid(r)||x<r.X-tolerance||x>r.Right+tolerance||y<r.Y-tolerance||y>r.Bottom+tolerance)return CaptureHandle.New;
        bool l=Math.Abs(x-r.X)<=tolerance,rr=Math.Abs(x-r.Right)<=tolerance,t=Math.Abs(y-r.Y)<=tolerance,b=Math.Abs(y-r.Bottom)<=tolerance;
        if(l&&t)return CaptureHandle.TopLeft;if(rr&&t)return CaptureHandle.TopRight;if(l&&b)return CaptureHandle.BottomLeft;if(rr&&b)return CaptureHandle.BottomRight;
        if(l)return CaptureHandle.Left;if(rr)return CaptureHandle.Right;if(t)return CaptureHandle.Top;if(b)return CaptureHandle.Bottom;
        return CaptureHandle.Move;
    }
    public static PixelRect Drag(PixelRect before,CaptureHandle handle,int sx,int sy,int x,int y,bool ratio)
    {
        if(handle==CaptureHandle.New){if(ratio){int side=Math.Max(Math.Abs(x-sx),Math.Abs(y-sy));x=sx+(x<sx?-1:1)*side;y=sy+(y<sy?-1:1)*side;}return PixelRect.FromPoints(sx,sy,x,y);}
        if(handle==CaptureHandle.Move)return before with{X=before.X+x-sx,Y=before.Y+y-sy};
        bool left=handle is CaptureHandle.Left or CaptureHandle.TopLeft or CaptureHandle.BottomLeft,right=handle is CaptureHandle.Right or CaptureHandle.TopRight or CaptureHandle.BottomRight;
        bool top=handle is CaptureHandle.Top or CaptureHandle.TopLeft or CaptureHandle.TopRight,bottom=handle is CaptureHandle.Bottom or CaptureHandle.BottomLeft or CaptureHandle.BottomRight;
        int ax=left?before.Right:before.X,ay=top?before.Bottom:before.Y;
        int bx=left||right?(left?before.X:before.Right)+x-sx:before.Right,by=top||bottom?(top?before.Y:before.Bottom)+y-sy:before.Bottom;
        if(ratio&&(left||right)&&(top||bottom)&&before.Width>0&&before.Height>0){double scale=Math.Max(Math.Abs(bx-ax)/(double)before.Width,Math.Abs(by-ay)/(double)before.Height);bx=ax+(bx<ax?-1:1)*(int)Math.Round(before.Width*scale);by=ay+(by<ay?-1:1)*(int)Math.Round(before.Height*scale);}
        return PixelRect.FromPoints(ax,ay,bx,by);
    }
    public static PixelRect Nudge(PixelRect r,int dx,int dy,bool resize)=>resize?r with{Width=Math.Max(8,r.Width+dx),Height=Math.Max(8,r.Height+dy)}:r with{X=r.X+dx,Y=r.Y+dy};
    // Prefer outside the selected content. A full-screen selection has no outside;
    // use a compact bottom strip there, hidden entirely during mouse gestures.
    public static PixelRect Toolbar(PixelRect screen,PixelRect selection,int width,int height,int gap=8)
    {
        width=Math.Min(width,screen.Width);height=Math.Min(height,screen.Height);
        int x=Math.Clamp(selection.X,screen.X,screen.Right-width),y;
        if(selection.Bottom+gap+height<=screen.Bottom)y=selection.Bottom+gap;
        else if(selection.Y-gap-height>=screen.Y)y=selection.Y-gap-height;
        else if(selection.Right+gap+width<=screen.Right){x=selection.Right+gap;y=Math.Clamp(selection.Y,screen.Y,screen.Bottom-height);}
        else if(selection.X-gap-width>=screen.X){x=selection.X-gap-width;y=Math.Clamp(selection.Y,screen.Y,screen.Bottom-height);}
        else {x=screen.Right-width;y=screen.Bottom-height;}
        return new(x,y,width,height);
    }
}
// Gesture snapshot prevents accumulated resize error and makes a press on a
// suggestion distinguish a click-to-lock from a drag-to-select manually.
public sealed class CaptureGesture(PixelRect before,CaptureHandle handle,int startX,int startY,bool liveSuggestion=false)
{
    public CaptureHandle Handle {get;private set;}=handle;
    private bool _click=liveSuggestion;
    public PixelRect Apply(int x,int y,bool ratio,PixelRect bounds)
    {
        var requested=Apply(x,y,ratio);
        if(Handle==CaptureHandle.Move)return CaptureSelection.MoveInside(requested,bounds);
        if(ratio&&(Handle==CaptureHandle.New||Handle is CaptureHandle.TopLeft or CaptureHandle.TopRight or CaptureHandle.BottomLeft or CaptureHandle.BottomRight)){
            int ax=Handle==CaptureHandle.New?startX:Handle is CaptureHandle.Left or CaptureHandle.TopLeft or CaptureHandle.BottomLeft?before.Right:before.X;
            int ay=Handle==CaptureHandle.New?startY:Handle is CaptureHandle.Top or CaptureHandle.TopLeft or CaptureHandle.TopRight?before.Bottom:before.Y;
            return CaptureSelection.FitRatio(requested,bounds,ax,ay);
        }
        return CaptureSelection.Clip(requested,bounds);
    }
    public PixelRect Apply(int x,int y,bool ratio)
    {
        if(_click){if(Math.Abs(x-startX)<4&&Math.Abs(y-startY)<4)return before;_click=false;Handle=CaptureHandle.New;}
        return CaptureSelection.Drag(before,Handle,startX,startY,x,y,ratio);
    }
}
// Hysteresis: a provider must repeat a changed rectangle before replacing the
// current suggestion. Preserve a deliberately selected parent while still inside.
public sealed class ElementSuggestion
{
    public ElementCandidate[] Candidates {get;private set;}=[];
    public int Index {get;private set;}
    public ElementCandidate? Current=>Candidates.Length==0?null:Candidates[Index];
    private PixelRect? _pending;private int _count;private bool _manual;
    public void Clear(){Candidates=[];Index=0;_pending=null;_count=0;_manual=false;}
    public void Cycle(int wheel){Index=ElementSelection.Cycle(Index,wheel,Candidates.Length);_manual=true;}
    public bool Offer(ElementCandidate[] list,int x,int y,int tolerance)
    {
        if(list.Length==0)return false;
        if(Current==null){Candidates=list;Index=ElementSelection.PreferredAt(list,x,y,tolerance);return true;}
        int retained=Current==null?-1:Array.FindIndex(list,c=>c.Bounds==Current.Bounds);
        int preferred=retained>=0&&_manual?retained:ElementSelection.PreferredAt(list,x,y,tolerance);
        var next=list[preferred];
        if(Current?.Bounds==next.Bounds){Candidates=list;Index=preferred;_pending=null;_count=0;return true;}
        if(_pending==next.Bounds)_count++;else{_pending=next.Bounds;_count=1;}
        if(_count<2)return false;
        Candidates=list;Index=preferred;_manual=false;_pending=null;_count=0;return true;
    }
}
