namespace Captovix;
public static class UiReconstruction
{
    public static ScanResult Refine(ScanResult scan,byte[] pixels,int width,int height,PixelRect region,CancellationToken token=default){
        // Duplicate OCR/vision boxes are resolved once before any layout grouping.
        var list=new List<ScanElement>();foreach(var e in scan.Elements.OrderByDescending(e=>e.UiSourceId!=null).ThenByDescending(e=>e.Confidence)){
            token.ThrowIfCancellationRequested();if(list.Any(x=>x.Kind==e.Kind&&ElementScan.Overlap(x.Bounds,e.Bounds)>.85&&string.Equals(x.Text.Trim(),e.Text.Trim(),StringComparison.OrdinalIgnoreCase)))continue;list.Add(e);
        }
        foreach(var element in list){if(element.Colors.Flatness<.7){element.KeepImage=true;element.Reason+=" Nền phức tạp: mặc định giữ ảnh gốc; chỉ chuyển khi đã kiểm tra nền/mặt nạ.";}}
        // Detect grid lines on a bounded sampling grid. OCR words cannot create a
        // table by themselves: at least 3 horizontal and 2 vertical long rules.
        int step=Math.Max(1,(int)Math.Ceiling(Math.Sqrt((double)width*height/2_000_000)));var horizontal=new List<PixelRect>();var vertical=new List<PixelRect>();
        var fill=ElementScan.Sample(pixels,width,height,new(0,0,width,height)).Fill;int reference=Convert.ToInt32(fill[1..],16);
        bool Edge(int x,int y){int n=(y*width+x)*4;return Math.Abs(pixels[n+2]-(reference>>16&255))+Math.Abs(pixels[n+1]-(reference>>8&255))+Math.Abs(pixels[n]-(reference&255))>55;}
        for(int y=0;y<height&&horizontal.Count<128;y+=step){token.ThrowIfCancellationRequested();int start=-1,gap=0;for(int x=0;x<=width;x+=step){bool edge=x<width&&Edge(x,y);if(edge){if(start<0)start=x;gap=0;}else if(start>=0&&++gap>2){int end=x-gap*step;if(end-start>=80)horizontal.Add(new(start,y,end-start+step,step));start=-1;gap=0;}}}
        for(int x=0;x<width&&vertical.Count<128;x+=step){token.ThrowIfCancellationRequested();int start=-1,gap=0;for(int y=0;y<=height;y+=step){bool edge=y<height&&Edge(x,y);if(edge){if(start<0)start=y;gap=0;}else if(start>=0&&++gap>2){int end=y-gap*step;if(end-start>=60)vertical.Add(new(x,start,step,end-start+step));start=-1;gap=0;}}}
        var hs=horizontal.GroupBy(l=>l.Y/Math.Max(3,step*2)).Select(g=>g.MaxBy(l=>l.Width)).ToArray();var vs=vertical.GroupBy(l=>l.X/Math.Max(3,step*2)).Select(g=>g.MaxBy(l=>l.Height)).ToArray();
        var connected=hs.Where(h=>vs.Count(v=>v.X>=h.X-3&&v.X<=h.Right+3&&h.Y>=v.Y-3&&h.Y<=v.Bottom+3)>=2).ToArray();
        if(connected.Length>=3&&vs.Length>=2&&list.Count<ElementScan.MaxCandidates){int left=connected.Min(h=>h.X),right=connected.Max(h=>h.Right),top=connected.Min(h=>h.Y),bottom=connected.Max(h=>h.Bottom);var b=new PixelRect(left,top,right-left,bottom-top).Intersect(new(0,0,width,height));if(b.Width>=80&&b.Height>=40){var global=b with{X=b.X+region.X,Y=b.Y+region.Y};var existing=list.FirstOrDefault(e=>e.Kind==MockupKind.Block&&ElementScan.Overlap(e.Bounds,global)>.7);if(existing!=null){existing.SemanticRole="Table";existing.KeepImage=true;}else list.Add(new(){Bounds=global,Kind=MockupKind.Block,SemanticRole="Table",KeepImage=true,Confidence=.78,Colors=ElementScan.Sample(pixels,width,height,b),Reason="Bảng từ các đường lưới giao nhau; giữ ảnh nền bảng, có thể sửa riêng chữ/cell đã chọn."});}}
        // Group aligned rows without moving/resizing source geometry.
        var pending=list.Where(e=>e.Kind!=MockupKind.Block&&e.SemanticRole!="Icon").OrderBy(e=>e.Bounds.Y+e.Bounds.Height/2).ToList();
        while(pending.Count>0){var seed=pending[0];double center=seed.Bounds.Y+seed.Bounds.Height/2d;var row=pending.Where(e=>Math.Abs(e.Bounds.Y+e.Bounds.Height/2d-center)<=Math.Max(5,Math.Min(seed.Bounds.Height,e.Bounds.Height)*.35)).OrderBy(e=>e.Bounds.X).ToArray();foreach(var e in row)pending.Remove(e);if(row.Length>=2){string group=Guid.NewGuid().ToString("N");foreach(var e in row){e.LayoutGroup=group;e.Reason+=" Nhóm cùng hàng; giữ nguyên tọa độ/khoảng cách gốc.";}}}
        // Remaining aligned objects form columns; never overwrite row membership.
        var ungrouped=list.Where(e=>e.LayoutGroup==null&&e.Kind!=MockupKind.Block&&e.SemanticRole!="Icon").OrderBy(e=>e.Bounds.X).ToList();
        while(ungrouped.Count>0){var seed=ungrouped[0];var column=ungrouped.Where(e=>Math.Abs(e.Bounds.X-seed.Bounds.X)<=6&&Math.Abs(e.Bounds.Width-seed.Bounds.Width)<=Math.Max(10,seed.Bounds.Width*.2)).OrderBy(e=>e.Bounds.Y).ToArray();foreach(var e in column)ungrouped.Remove(e);if(column.Length>=2){string group=Guid.NewGuid().ToString("N");foreach(var e in column){e.LayoutGroup=group;e.Reason+=" Nhóm cùng cột; giữ nguyên tọa độ/khoảng cách gốc.";}}}
        foreach(var label in list.Where(e=>e.Kind is MockupKind.Label or MockupKind.Title)){
            var inputs=list.Where(e=>e.Kind==MockupKind.Input).OrderBy(e=>ElementScan.PairScore(label.Bounds,e.Bounds)).Take(2).ToArray();if(inputs.Length>0&&ElementScan.PairScore(label.Bounds,inputs[0].Bounds)<100&&(inputs.Length==1||ElementScan.PairScore(label.Bounds,inputs[1].Bounds)-ElementScan.PairScore(label.Bounds,inputs[0].Bounds)>12))label.LabelForId=inputs[0].Id;
        }
        var ids=list.Select(e=>e.Id).ToHashSet();foreach(var e in list)if(e.LabelForId!=null&&!ids.Contains(e.LabelForId))e.LabelForId=null;
        return new(list.OrderBy(e=>e.Kind==MockupKind.Block?1:0).ThenBy(e=>e.Bounds.Y).ThenBy(e=>e.Bounds.X).Take(ElementScan.MaxCandidates).ToList(),scan.Truncated||list.Count>ElementScan.MaxCandidates,scan.Warning);
    }
}
