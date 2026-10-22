namespace Captovix;
public sealed record ElementCandidate(PixelRect Bounds,string Label,bool Container=false);
public sealed record ElementQuery(long Window,int X,int Y,int ExcludedProcess);
public static class ElementSelection
{
    public static bool Contains(PixelRect b,int x,int y)=>!b.IsEmpty&&x>=b.X&&y>=b.Y&&x<b.Right&&y<b.Bottom;
    public static ElementCandidate[] Normalize(IEnumerable<ElementCandidate> candidates,int x,int y,PixelRect window)
    {
        var result=new List<ElementCandidate>();
        foreach(var c in candidates){var b=c.Bounds.Intersect(window);if(b.Width<2||b.Height<2||!Contains(b,x,y)||result.Any(e=>e.Bounds==b))continue;
            if(result.Count>0&&result[^1].Bounds.Intersect(b)!=result[^1].Bounds)continue;
            result.Add(c with{Bounds=b,Label=c.Label.Length>100?c.Label[..100]:c.Label});
            if(result.Count==24)break;
        }
        if(Contains(window,x,y)&&!result.Any(c=>c.Bounds==window))result.Add(new(window,"Cửa sổ · kéo để chọn vùng nếu không thấy phần tử",true));
        return result.ToArray();
    }
    public static int Preferred(IReadOnlyList<ElementCandidate> list){for(int i=0;i<list.Count;i++)if(list[i].Container)return i;return 0;}
    public static int Cycle(int index,int wheel,int count)=>count==0?0:Math.Clamp(index+(wheel>0?1:-1),0,count-1);
}
