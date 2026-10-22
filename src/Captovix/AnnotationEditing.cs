namespace Captovix;

public readonly record struct AnnotationBox(double X,double Y,double Width,double Height)
{
    public double Right=>X+Width;
    public double Bottom=>Y+Height;
    public double CenterX=>X+Width/2;
    public double CenterY=>Y+Height/2;
}
public enum AnnotationAlignment { Left, CenterX, Right, Top, CenterY, Bottom }
public enum AnnotationOrder { Front, Back, Forward, Backward }
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
        if(!ObjectControl.CanEdit(doc,ids))return;
        var units=Units(doc,ids);if(units.Length==0)return;
        var view=doc.Viewport;var target=units.Length>1?Union(units.Select(u=>Union(u.Select(bounds)))):new AnnotationBox(view.X,view.Y,view.Width,view.Height);
        foreach(var unit in units){var b=Union(unit.Select(bounds));var dx=0d;var dy=0d;
            switch(alignment){
                case AnnotationAlignment.Left:dx=target.X-b.X;break;
                case AnnotationAlignment.CenterX:dx=target.CenterX-b.CenterX;break;
                case AnnotationAlignment.Right:dx=target.Right-b.Right;break;
                case AnnotationAlignment.Top:dy=target.Y-b.Y;break;
                case AnnotationAlignment.CenterY:dy=target.CenterY-b.CenterY;break;
                case AnnotationAlignment.Bottom:dy=target.Bottom-b.Bottom;break;
            }
            foreach(var a in unit)a.Translate(dx,dy);
        }
    }
    public static HashSet<string> ExpandGroups(EditorDocument doc,IEnumerable<string> ids)
    {
        var result=ids.ToHashSet();var groups=doc.Annotations.Where(a=>result.Contains(a.Id)&&a.GroupId!=null).Select(a=>a.GroupId!).ToHashSet();
        result.UnionWith(doc.Annotations.Where(a=>a.GroupId!=null&&groups.Contains(a.GroupId)).Select(a=>a.Id));return result;
    }
    public static Annotation[][] Units(EditorDocument doc,IEnumerable<string> ids)
    {
        var all=ExpandGroups(doc,ids);return doc.Annotations.Where(a=>all.Contains(a.Id)).GroupBy(a=>a.GroupId is {} g?"group:"+g:"item:"+a.Id).Select(g=>g.ToArray()).ToArray();
    }
    public static string? Group(EditorDocument doc,IEnumerable<string> ids)
    {
        if(!ObjectControl.CanEdit(doc,ids))return null;
        var all=ExpandGroups(doc,ids);if(all.Count<2)return null;
        var group=Guid.NewGuid().ToString("N");foreach(var a in doc.Annotations.Where(a=>all.Contains(a.Id)))a.GroupId=group;return group;
    }
    public static void Ungroup(EditorDocument doc,IEnumerable<string> ids){if(!ObjectControl.CanEdit(doc,ids))return;var all=ExpandGroups(doc,ids);foreach(var a in doc.Annotations.Where(a=>all.Contains(a.Id)))a.GroupId=null;}
    public static HashSet<string> Duplicate(EditorDocument doc,IEnumerable<string> ids,Func<Annotation,AnnotationBox> bounds)
    {
        if(!ObjectControl.CanEdit(doc,ids))return [];
        var all=ExpandGroups(doc,ids);var originals=doc.Annotations.Where(a=>all.Contains(a.Id)).ToArray();
        if(doc.Annotations.Count+originals.Length>10000||doc.Annotations.Sum(a=>a.Points.Count)+originals.Sum(a=>a.Points.Count)>100000)throw new InvalidOperationException("Nhân bản vượt giới hạn tài liệu.");
        var idMap=originals.ToDictionary(a=>a.Id,_=>Guid.NewGuid().ToString("N"));var groupMap=new Dictionary<string,string>();var result=new HashSet<string>();
        var delta=ClampMove(Union(originals.Select(bounds)),doc.Viewport,16,16);
        foreach(var a in originals){var copy=a.Copy();copy.Id=idMap[a.Id];
            copy.StartAnchor=copy.StartAnchor is {} start&&idMap.TryGetValue(start.TargetId,out var startId)?start with{TargetId=startId}:null;
            copy.EndAnchor=copy.EndAnchor is {} end&&idMap.TryGetValue(end.TargetId,out var endId)?end with{TargetId=endId}:null;
            if(copy.GroupId is {} group){if(!groupMap.ContainsKey(group))groupMap[group]=Guid.NewGuid().ToString("N");copy.GroupId=groupMap[group];}
            copy.Translate(delta.X,delta.Y);doc.Annotations.Add(copy);result.Add(copy.Id);
        }return result;
    }
    public static int Tier(Annotation a)=>a.IsRedaction?a.Kind==AnnotationKind.Cover?2:1:0;
    public static void Reorder(EditorDocument doc,IEnumerable<string> ids,AnnotationOrder order)
    {
        if(!ObjectControl.CanEdit(doc,ids))return;
        var all=ExpandGroups(doc,ids);var result=new List<Annotation>();
        foreach(var tier in doc.Annotations.GroupBy(Tier).OrderBy(g=>g.Key)){
            var list=tier.ToList();
            if(order is AnnotationOrder.Front or AnnotationOrder.Back){var picked=list.Where(a=>all.Contains(a.Id)).ToArray();list.RemoveAll(a=>all.Contains(a.Id));if(order==AnnotationOrder.Front)list.AddRange(picked);else list.InsertRange(0,picked);}
            else if(order==AnnotationOrder.Forward){for(int i=list.Count-2;i>=0;i--)if(all.Contains(list[i].Id)&&!all.Contains(list[i+1].Id))(list[i],list[i+1])=(list[i+1],list[i]);}
            else{for(int i=1;i<list.Count;i++)if(all.Contains(list[i].Id)&&!all.Contains(list[i-1].Id))(list[i],list[i-1])=(list[i-1],list[i]);}
            result.AddRange(list);
        }doc.Annotations=result;
    }
    public static bool Distribute(EditorDocument doc,IEnumerable<string> ids,bool horizontal,Func<Annotation,AnnotationBox> bounds)
    {
        if(!ObjectControl.CanEdit(doc,ids))return false;
        var units=Units(doc,ids).Select(u=>(Items:u,Box:Union(u.Select(bounds)))).OrderBy(u=>horizontal?u.Box.X:u.Box.Y).ToArray();
        if(units.Length<3)return false;
        var first=units[0].Box;var last=units[^1].Box;double low=horizontal?first.X:first.Y,high=horizontal?last.Right:last.Bottom;
        var gap=(high-low-units.Sum(u=>horizontal?u.Box.Width:u.Box.Height))/(units.Length-1);if(gap<0)return false;
        var pos=low;
        for(int i=0;i<units.Length;i++){var u=units[i];if(i>0&&i<units.Length-1)foreach(var a in u.Items)a.Translate(horizontal?pos-u.Box.X:0,horizontal?0:pos-u.Box.Y);pos+=(horizontal?u.Box.Width:u.Box.Height)+gap;}
        return true;
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
        target.TextWidth=original.TextWidth>0?Math.Clamp(original.TextWidth*to.Width/Math.Max(1,from.Width),1,100000):0;
        target.TextPadding=original.TextPadding is {} padding?Math.Clamp(padding*Math.Min(to.Width/Math.Max(1,from.Width),to.Height/Math.Max(1,from.Height)),0,200):null;
        target.Rotation=original.Rotation;
        var control=original.Control??(ConnectorStudio.IsPath(original)&&original.Route!=ConnectorRoute.Straight?ConnectorStudio.Control(original):(AnnotationPoint?)null);
        target.Control=control is {} c?new(to.X+(c.X-from.X)*to.Width/Math.Max(1,from.Width),to.Y+(c.Y-from.Y)*to.Height/Math.Max(1,from.Height)):null;
        var sx=to.Width/Math.Max(1,from.Width);var sy=to.Height/Math.Max(1,from.Height);
        target.X1=to.X+(original.X1-from.X)*sx;target.X2=to.X+(original.X2-from.X)*sx;
        target.Y1=to.Y+(original.Y1-from.Y)*sy;target.Y2=to.Y+(original.Y2-from.Y)*sy;
        if(original.Kind==AnnotationKind.Callout){if(target.Width<36)target.X2=target.X1+Math.Sign(original.X2-original.X1)*36;if(target.Height<36)target.Y2=target.Y1+Math.Sign(original.Y2-original.Y1)*36;target.TailX=to.X+(original.TailX-from.X)*sx;target.TailY=to.Y+(original.TailY-from.Y)*sy;}
        target.Points=original.Points.Select(p=>new AnnotationPoint(to.X+(p.X-from.X)*sx,to.Y+(p.Y-from.Y)*sy)).ToList();
        if(original.Kind is AnnotationKind.Text or AnnotationKind.Step)target.FontSize=Math.Clamp(original.FontSize*Math.Min(sx,sy),8,240);
    }
}
