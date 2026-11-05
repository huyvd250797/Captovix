using System.IO;
namespace Captovix;
public static class BackgroundRecovery
{
    public const int MaxPixels=20_000_000;
    // The mask contains positions only, never copies of the original screenshot.
    public static RecoveryPatch Build(byte[] bgra,int width,int height,PixelRect sampled,PixelRect source,string color,MockupKind kind,IEnumerable<PixelRect> protectedAreas)
    {
        if(width!=sampled.Width||height!=sampled.Height||bgra.LongLength!=(long)width*height*4||source.IsEmpty||source.Intersect(sampled)!=source||(long)source.Width*source.Height>MaxPixels||!StudioStyle.Hex(color))throw new InvalidDataException("Vùng phục hồi nền không hợp lệ; tối đa20MP.");
        int rgb=Convert.ToInt32(color[1..],16),total=checked(source.Width*source.Height);var active=new bool[total];var block=new bool[total];
        var keep=protectedAreas.Select(x=>x.Intersect(source)).Where(x=>!x.IsEmpty).ToList();foreach(var r in keep)for(int y=r.Y;y<r.Bottom;y++)for(int x=r.X;x<r.Right;x++)block[(y-source.Y)*source.Width+x-source.X]=true;
        int Distance(int x,int y){int p=((y-sampled.Y)*width+x-sampled.X)*4,a=bgra[p+3];return Math.Abs((bgra[p+2]*a+255*(255-a))/255-(rgb>>16&255))+Math.Abs((bgra[p+1]*a+255*(255-a))/255-(rgb>>8&255))+Math.Abs((bgra[p]*a+255*(255-a))/255-(rgb&255));}
        int outside=0,flat=0;for(int y=sampled.Y;y<sampled.Bottom;y++)for(int x=sampled.X;x<sampled.Right;x++){bool inside=x>=source.X&&x<source.Right&&y>=source.Y&&y<source.Bottom;int d=Distance(x,y);if(inside){int i=(y-source.Y)*source.Width+x-source.X;active[i]=!block[i]&&d>2;}else{outside++;if(d<=24)flat++;}}
        bool uncertain=outside==0||flat/(double)Math.Max(1,outside)<.85;
        if(kind is MockupKind.Button or MockupKind.Input or MockupKind.Block){
            var seen=new bool[total];var queue=new int[total];int best=0;PixelRect body=default;
            for(int seed=0;seed<total;seed++){if(!active[seed]||seen[seed])continue;int head=0,tail=1;queue[0]=seed;seen[seed]=true;int left=seed%source.Width,right=left,top=seed/source.Width,bottom=top;
                while(head<tail){int at=queue[head++],x=at%source.Width,y=at/source.Width;left=Math.Min(left,x);right=Math.Max(right,x);top=Math.Min(top,y);bottom=Math.Max(bottom,y);void Visit(int n){if(active[n]&&!seen[n]){seen[n]=true;queue[tail++]=n;}}if(x>0)Visit(at-1);if(x+1<source.Width)Visit(at+1);if(y>0)Visit(at-source.Width);if(y+1<source.Height)Visit(at+source.Width);}
                if(tail>best){best=tail;body=new(left,top,right-left+1,bottom-top+1);}
            }
            if(body.Width>=source.Width*.45&&body.Height>=source.Height*.45||best>=active.Count(x=>x)*.65&&body.Width>=8&&body.Height>=8){var expanded=new PixelRect(body.X-1,body.Y-1,body.Width+2,body.Height+2).Intersect(new(0,0,source.Width,source.Height));for(int y=0;y<source.Height;y++)for(int x=0;x<source.Width;x++)if(x<expanded.X||x>=expanded.Right||y<expanded.Y||y>=expanded.Bottom)active[y*source.Width+x]=false;}
            else if(best>0)uncertain=true;
        }
        var bits=new byte[(total+7)/8];for(int i=0;i<total;i++)if(active[i])bits[i>>3]|=(byte)(1<<(i&7));
        return new(){Bounds=source,Kind=kind,Background=color,Mask=Convert.ToBase64String(bits),Protected=keep,NeedsReview=uncertain};
    }
    public static bool Covers(RecoveryPatch patch,int x,int y){if(x<patch.Bounds.X||y<patch.Bounds.Y||x>=patch.Bounds.Right||y>=patch.Bounds.Bottom)return false;var mask=Convert.FromBase64String(patch.Mask);int p=(y-patch.Bounds.Y)*patch.Bounds.Width+x-patch.Bounds.X;return (mask[p>>3]&(1<<(p&7)))!=0;}
    public static void Finalize(string beforeJson,EditorDocument document,Func<Annotation,EditorDocument,RecoveryPatch> prepare)
    {
        Finalize(EditorDocument.Deserialize(beforeJson),document,prepare);
    }
    public static void Finalize(EditorDocument before,EditorDocument document,Func<Annotation,EditorDocument,RecoveryPatch> prepare)
    {
        MockupRasterState.Reconcile(before,document,prepare);
        var previous=before.Annotations.ToDictionary(a=>a.Id);var current=document.Annotations.ToDictionary(a=>a.Id);
        var work=new List<(Annotation Source,Annotation? Current)>();
        foreach(var old in before.Annotations.Where(a=>a.Mockup is {EraseOriginal:true,RecoveryId:null}))if(!current.TryGetValue(old.Id,out var next)||old.X1!=next.X1||old.Y1!=next.Y1||old.X2!=next.X2||old.Y2!=next.Y2||old.Rotation!=next.Rotation||old.Visible!=next.Visible)work.Add((old,next));
        foreach(var a in document.Annotations.Where(a=>a.Mockup is {EraseOriginal:true,RecoveryId:null}&&!previous.ContainsKey(a.Id)))work.Add((a,a));
        if(work.Sum(x=>(long)x.Source.Mockup!.SourceBounds.Width*x.Source.Mockup.SourceBounds.Height)>MaxPixels)throw new InvalidDataException("Mỗi thao tác tách nền tối đa20MP. Chọn ít phần tử hơn.");
        foreach(var (source,next) in work){var patch=prepare(source,document);document.Recoveries.Add(patch);if(next?.Mockup is {} m){m.RecoveryId=patch.Id;m.EraseOriginal=false;}}
        PixelRect Box(Annotation a)=>new((int)Math.Floor(a.Left),(int)Math.Floor(a.Top),(int)Math.Ceiling(a.Width),(int)Math.Ceiling(a.Height));int recorded=0;
        foreach(var id in previous.Keys.Union(current.Keys)){previous.TryGetValue(id,out var old);current.TryGetValue(id,out var next);if(old?.Mockup==null&&next?.Mockup==null)continue;string? action=old==null?"Thêm / nhân bản":next==null?"Xóa":old.Width!=next.Width||old.Height!=next.Height?"Resize":old.Left!=next.Left||old.Top!=next.Top?"Di chuyển":old.Rotation!=next.Rotation?"Xoay":old.Text!=next.Text?"Sửa nội dung":old.Visible!=next.Visible?"Đổi hiển thị":null;if(action!=null){document.RecoveryHistory.Add(new(action,DateTimeOffset.UtcNow,id,next?.Mockup?.RecoveryId??old?.Mockup?.RecoveryId,old==null?null:Box(old),next==null?null:Box(next)));recorded++;}}
        if(recorded==0&&before.Recoveries.Count!=document.Recoveries.Count||recorded==0&&before.Recoveries.Zip(document.Recoveries).Any(x=>x.First.Mask!=x.Second.Mask||x.First.Background!=x.Second.Background))document.RecoveryHistory.Add(new("Sửa nền / mặt nạ",DateTimeOffset.UtcNow,"",null,null,null));
        if(document.RecoveryHistory.Count>100)document.RecoveryHistory.RemoveRange(0,document.RecoveryHistory.Count-100);
        if(document.Recoveries.Count>0||document.RecoveryHistory.Count>0)document.Version=20;
        document.Validate(document.OriginalWidth,document.OriginalHeight,document.SourceHash);
    }
}
