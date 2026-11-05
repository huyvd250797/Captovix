using System.IO;
namespace Captovix;
// A scan is a disposable draft. Nothing is applied to an EditorSession here.
public sealed record ScanText(PixelRect Bounds,string Text,string Foreground);
public sealed class ScanElement
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public PixelRect Bounds {get;set;}
    public MockupKind Kind {get;set;}
    public string Text {get;set;}="";
    public double Confidence {get;set;}
    public string Reason {get;set;}="";
    public bool Include {get;set;}
    public string? UiSourceId {get;set;}
    public string? UiParentId {get;set;}
    public string? LabelForId {get;set;}
    public List<ScanText> TextRuns {get;set;}=[];
    public PixelRect? OcrBounds {get;set;}
    public string FontFamily {get;set;}="Segoe UI";
    public double FontSize {get;set;}
    public bool FontBold {get;set;}
    public bool FontItalic {get;set;}
    public bool FontUnderline {get;set;}
    public bool StyleVerified {get;set;}
    public bool KeepImage {get;set;}
    public string? SemanticRole {get;set;}
    public string? LayoutGroup {get;set;}
    public string BorderColor {get;set;}="#808080";
    public double BorderWidth {get;set;}
    public double Radius {get;set;}
    public MockupColors Colors {get;set;}=new("#FFFFFF","#FFFFFF","#202020",1);
    public string Label=>$"{(Include?"✓ ":"")}{ElementScan.Label(Kind)}{(SemanticRole==null?"":" / "+SemanticRole)}{(KeepImage?" · Giữ ảnh":"")} · {Bounds.Width}×{Bounds.Height} · {Confidence:P0}"+(Uncertain?" · Cần kiểm tra":"");
    public bool Uncertain=>KeepImage||Text.Length>0&&!StyleVerified||Confidence<.85||Colors.Flatness<.75||Kind is MockupKind.Input or MockupKind.Block;
}
public sealed record ScanResult(List<ScanElement> Elements,bool Truncated,string Warning="");
public static class ElementScan
{
    public const int MaxCandidates=200,MaxApply=120;
    public static string Label(MockupKind kind)=>kind switch{MockupKind.Label=>"Chữ",MockupKind.Title=>"Tiêu đề",MockupKind.Button=>"Nút",MockupKind.Input=>"Ô nhập",_=>"Khối giao diện"};
    public static PixelRect Clamp(PixelRect r,PixelRect region)=>r.Intersect(region);
    public static double Overlap(PixelRect a,PixelRect b){var i=a.Intersect(b);double v=(double)i.Width*i.Height;return v/Math.Max(1,(double)a.Width*a.Height+(double)b.Width*b.Height-v);}
    public static ScanResult Analyze(byte[] pixels,int width,int height,PixelRect region,IReadOnlyList<OcrTextLine> lines,CancellationToken token=default,IReadOnlyList<ScanElement>? visualDraft=null)
    {
        if(width<8||height<8||pixels.LongLength!=(long)width*height*4||width!=region.Width||height!=region.Height||(long)width*height>OcrContract.MaxPixels)throw new ArgumentException("Vùng quét không hợp lệ; tối đa20MP.");
        // Bound both memory and CPU. Components operate on quantized flat colors,
        // not on millions of high-resolution glyph edges.
        int step=Math.Max(1,(int)Math.Ceiling(Math.Sqrt((double)width*height/1_200_000))),w=(width+step-1)/step,h=(height+step-1)/step;
        var grid=visualDraft==null?new int[w*h]:Array.Empty<int>();var visited=new bool[grid.Length];var queue=new int[grid.Length];
        if(visualDraft==null)for(int y=0;y<h;y++){token.ThrowIfCancellationRequested();for(int x=0;x<w;x++){int p=(Math.Min(height-1,y*step)*width+Math.Min(width-1,x*step))*4;int alpha=pixels[p+3];int b=(pixels[p]*alpha+255*(255-alpha))/255,g=(pixels[p+1]*alpha+255*(255-alpha))/255,r=(pixels[p+2]*alpha+255*(255-alpha))/255;grid[y*w+x]=(r/24)<<10|(g/24)<<5|b/24;}}
        var visual=visualDraft?.Where(e=>e.Kind is MockupKind.Button or MockupKind.Input or MockupKind.Block).ToList()??new List<ScanElement>();bool truncated=false;
        for(int seed=0;seed<grid.Length;seed++){
            if(visited[seed])continue;token.ThrowIfCancellationRequested();int head=0,tail=1;queue[0]=seed;visited[seed]=true;int color=grid[seed],minX=seed%w,maxX=minX,minY=seed/w,maxY=minY;
            while(head<tail){if((head&8191)==0)token.ThrowIfCancellationRequested();int at=queue[head++],x=at%w,y=at/w;minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);
                void Visit(int n){if(!visited[n]&&grid[n]==color){visited[n]=true;queue[tail++]=n;}}
                if(x>0)Visit(at-1);if(x+1<w)Visit(at+1);if(y>0)Visit(at-w);if(y+1<h)Visit(at+w);
            }
            int cw=(maxX-minX+1)*step,ch=(maxY-minY+1)*step;double fill=tail/(double)((maxX-minX+1)*(maxY-minY+1));
            if(cw<28||ch<16||cw<ch*.7||fill<.70||(double)cw*ch>width*(double)height*.65||cw>width*.95&&ch>height*.8)continue;
            // The interior component excludes a thin outline; include it in the proposal.
            var local=new PixelRect(Math.Max(0,minX*step-1),Math.Max(0,minY*step-1),cw+2,ch+2).Intersect(new(0,0,width,height));
            if(local.Width<8||local.Height<8)continue;
            var colors=Sample(pixels,width,height,local);double colorDistance=Distance(colors.Fill,colors.Background);
            var kind=ch>=96&&cw>=120?MockupKind.Block:colorDistance>70?MockupKind.Button:MockupKind.Input;
            // A small patch indistinguishable from its exterior is not a control.
            if(kind!=MockupKind.Block&&colorDistance<12&&fill>.98&&!HasBorder(pixels,width,height,local,colors.Fill))continue;
            visual.Add(new(){Bounds=local with{X=local.X+region.X,Y=local.Y+region.Y},Kind=kind,Confidence=kind==MockupKind.Button?.78:kind==MockupKind.Block?.48:.60,Colors=colors,Reason="Gợi ý hình học/màu phẳng; cần kiểm tra loại và viền."});
            if(visual.Count>=MaxCandidates*3){truncated=true;break;}
        }
        var result=new List<ScanElement>();
        foreach(var v in visual.OrderBy(x=>(long)x.Bounds.Width*x.Bounds.Height))if(!result.Any(x=>Overlap(x.Bounds,v.Bounds)>.78))result.Add(v);
        var uniqueLines=new List<OcrTextLine>();foreach(var line in lines.Take(500).Where(l=>!string.IsNullOrWhiteSpace(l.Text)&&l.Bounds.Width>0&&l.Bounds.Height>0).OrderByDescending(l=>l.Confidence))if(!uniqueLines.Any(x=>Overlap(x.Bounds,line.Bounds)>.75))uniqueLines.Add(line);var validLines=uniqueLines.ToArray();double median=validLines.Select(l=>l.Bounds.Height).Order().ElementAtOrDefault(validLines.Length/2);
        foreach(var line in validLines){token.ThrowIfCancellationRequested();var b=line.Bounds.Intersect(new(0,0,width,height));if(b.IsEmpty)continue;
            var global=b with{X=b.X+region.X,Y=b.Y+region.Y};var holder=result.Where(v=>v.Kind is MockupKind.Button or MockupKind.Input&&Contains(v.Bounds,global,.90)).OrderBy(v=>(long)v.Bounds.Width*v.Bounds.Height).FirstOrDefault();
            if(holder!=null){holder.TextRuns.Add(new(global,line.Text.Trim(),Sample(pixels,width,height,b).Foreground));holder.Text=Limit(holder.Text.Length==0?line.Text:holder.Text+"\n"+line.Text);holder.Confidence=Math.Min(.94,Math.Max(holder.Confidence,.55+Math.Clamp(line.Confidence,0,1)*.4));holder.Reason="Chữ OCR nằm trong khung màu/viền; kiểm tra nội dung và loại control.";continue;}
            var expanded=new PixelRect(Math.Max(0,b.X-2),Math.Max(0,b.Y-2),Math.Max(8,b.Width+4),Math.Max(8,b.Height+4)).Intersect(new(0,0,width,height));if(expanded.Width<8||expanded.Height<8)continue;
            var candidate=new ScanElement{OcrBounds=global,Bounds=expanded with{X=expanded.X+region.X,Y=expanded.Y+region.Y},Text=Limit(line.Text.Trim()),Kind=b.Height>median*1.35&&b.Height>=22?MockupKind.Title:MockupKind.Label,Confidence=Math.Clamp(line.Confidence,0,1),Reason="Khung chữ từ OCR; kiểm tra dấu tiếng Việt và bao hết nét chữ.",Colors=Sample(pixels,width,height,expanded)};
            if(!result.Any(x=>x.Kind is MockupKind.Label or MockupKind.Title&&Overlap(x.Bounds,candidate.Bounds)>.75))result.Add(candidate);
        }
        foreach(var label in result.Where(x=>x.Kind is MockupKind.Label or MockupKind.Title)){
            var a=label.Bounds;var nearby=result.Where(x=>x.Kind==MockupKind.Input).Select(x=>new{Item=x,Score=PairScore(a,x.Bounds)}).Where(x=>x.Score<100).OrderBy(x=>x.Score).ToArray();
            if(nearby.Length>0){label.LabelForId=nearby[0].Item.Id;label.Reason+=" Gợi ý ghép với ô nhập gần nhất; có thể đổi/bỏ liên kết.";if(nearby.Length>1&&nearby[1].Score-nearby[0].Score<15){label.Confidence=Math.Min(label.Confidence,.7);label.Reason+=" Có nhiều ô nhập gần nhau.";}}
        }
        if(result.Count>MaxCandidates)truncated=true;
        // Prefer text/control proposals over container suggestions; apply order is separate.
        result=result.OrderBy(x=>x.Kind==MockupKind.Block?1:0).ThenBy(x=>x.Bounds.Y).ThenBy(x=>x.Bounds.X).Take(MaxCandidates).ToList();foreach(var e in result){var local=e.Bounds with{X=e.Bounds.X-region.X,Y=e.Bounds.Y-region.Y};if(e.Kind is MockupKind.Button or MockupKind.Input or MockupKind.Block)MeasureOutline(e,pixels,width,height,local);e.FontSize=Math.Clamp((e.TextRuns.Count>0?e.TextRuns.Average(t=>t.Bounds.Height):e.Bounds.Height)*.95,8,72);}
        var ids=result.Select(x=>x.Id).ToHashSet();foreach(var x in result)if(x.LabelForId!=null&&!ids.Contains(x.LabelForId))x.LabelForId=null;
        return new(result,truncated,truncated?"Đã giới hạn200khung. Quét vùng nhỏ hơn để xem phần còn lại.":"");
    }
    private static string Limit(string value)=>value.Length<=2000?value:value[..2000];
    private static bool Contains(PixelRect a,PixelRect b,double fraction){var i=a.Intersect(b);return (double)i.Width*i.Height>=b.Width*(double)b.Height*fraction;}
    public static double PairScore(PixelRect label,PixelRect control){double dy=Math.Abs(label.Y+label.Height*.5-control.Y-control.Height*.5),dx=control.X-label.Right;double below=control.Y-label.Bottom;
        double left=dx>=-2&&dx<=100&&dy<=Math.Max(12,control.Height*.6)?Math.Max(0,dx)+dy*1.5:1000;
        double top=below>=-2&&below<=48&&Math.Abs(control.X-label.X)<Math.Max(40,control.Width*.2)?Math.Max(0,below)+Math.Abs(control.X-label.X)+10:1000;return Math.Min(left,top);}
    private static bool HasBorder(byte[] p,int width,int height,PixelRect b,string fill){int reference=Convert.ToInt32(fill[1..],16),seen=0,different=0;void Test(int x,int y){int n=(y*width+x)*4;int d=Math.Abs(p[n+2]-(reference>>16&255))+Math.Abs(p[n+1]-(reference>>8&255))+Math.Abs(p[n]-(reference&255));seen++;if(d>35)different++;}for(int x=b.X;x<b.Right;x+=Math.Max(1,b.Width/40)){Test(x,b.Y);Test(x,b.Bottom-1);}for(int y=b.Y;y<b.Bottom;y+=Math.Max(1,b.Height/40)){Test(b.X,y);Test(b.Right-1,y);}return different>seen*.45;}
    private static int Distance(string a,string b){int x=Convert.ToInt32(a[1..],16),y=Convert.ToInt32(b[1..],16);return Math.Abs((x>>16&255)-(y>>16&255))+Math.Abs((x>>8&255)-(y>>8&255))+Math.Abs((x&255)-(y&255));}
    // Sample in-place: no full candidate-sized BGRA allocation per element.
    public static MockupColors Sample(byte[] pixels,int width,int height,PixelRect b){
        var ring=new PixelRect(b.X-4,b.Y-4,b.Width+8,b.Height+8).Intersect(new(0,0,width,height));
        var inside=new Dictionary<int,int>();var outside=new Dictionary<int,int>();
        int Pixel(int x,int y){int n=(y*width+x)*4,a=pixels[n+3];return ((pixels[n+2]*a+255*(255-a))/255)<<16|((pixels[n+1]*a+255*(255-a))/255)<<8|(pixels[n]*a+255*(255-a))/255;}
        int step=Math.Max(1,(int)Math.Ceiling(Math.Sqrt((double)ring.Width*ring.Height/40000)));
        for(int y=ring.Y;y<ring.Bottom;y+=step)for(int x=ring.X;x<ring.Right;x+=step){var map=x>=b.X&&x<b.Right&&y>=b.Y&&y<b.Bottom?inside:outside;int c=Pixel(x,y);map[c]=map.GetValueOrDefault(c)+1;}
        if(inside.Count==0)return new("#FFFFFF","#FFFFFF","#202020",0);
        int fill=inside.MaxBy(p=>p.Value).Key;var edge=outside.Count>0?outside:inside;int back=edge.MaxBy(p=>p.Value).Key;
        string Hex(int c)=>$"#{c:X6}";int ink=inside.Where(p=>Distance(Hex(p.Key),Hex(fill))>70).OrderByDescending(p=>p.Value).Select(p=>p.Key).FirstOrDefault(fill>0x808080?0:0xffffff);
        return new(Hex(back),Hex(fill),Hex(ink),edge[back]/(double)edge.Values.Sum());
    }
    public static void MeasureOutline(ScanElement item,byte[] p,int w,int h,PixelRect b){
        string Pixel(int x,int y){int n=(y*w+x)*4;return $"#{p[n+2]:X2}{p[n+1]:X2}{p[n]:X2}";}
        var colors=new List<string>();var depths=new List<int>();
        // Four edge midpoints avoid rounded corners and most text glyphs.
        foreach(var side in new[]{0,1,2,3}){
            int depth=0;string? color=null;
            for(int k=0;k<Math.Min(8,Math.Min(b.Width,b.Height)/3);k++){
                int x=side<2?b.X+b.Width/2:side==2?b.X+k:b.Right-1-k;
                int y=side>=2?b.Y+b.Height/2:side==0?b.Y+k:b.Bottom-1-k;
                string c=Pixel(x,y);if(Distance(c,item.Colors.Fill)<24)break;
                if(Distance(c,item.Colors.Background)>24){color??=c;depth++;}
            }
            if(color!=null){colors.Add(color);depths.Add(depth);}
        }
        item.BorderWidth=depths.Count>=3?Math.Clamp(depths.Order().ElementAt(depths.Count/2),1,6):0;
        item.BorderColor=colors.Count>0?colors.GroupBy(c=>c).MaxBy(g=>g.Count())!.Key:item.Colors.Fill;
        int radius=0,limit=Math.Min(24,Math.Min(b.Width,b.Height)/3);
        for(int k=0;k<limit;k++){int x=b.X+k,y=b.Y+k;string c=Pixel(x,y);if(Distance(c,item.Colors.Background)>30||Distance(item.Colors.Fill,item.Colors.Background)<24&&Distance(c,item.BorderColor)<24)break;radius=k+1;}
        item.Radius=radius<2?0:Math.Clamp(radius*2,0,limit);
    }
    public static List<Annotation> Prepare(EditorDocument document,IReadOnlyList<ScanElement> candidates,PixelRect scanBounds)
    {
        var selected=candidates.Where(x=>x.Include&&!x.KeepImage).ToArray();int outputCount=selected.Sum(x=>1+(x.TextRuns.Count>0&&x.Text==string.Join("\n",x.TextRuns.Select(t=>t.Text))?x.TextRuns.Count:0));if(selected.Length==0)throw new InvalidDataException("Chọn ít nhất một phần tử đã kiểm tra.");if(selected.Length>MaxApply||document.Annotations.Count+outputCount>10000||outputCount>MaxApply)throw new InvalidDataException("Tối đa120 phần tử mỗi lần áp dụng.");
        for(int i=0;i<selected.Length;i++)for(int j=i+1;j<selected.Length;j++)if(Overlap(selected[i].Bounds,selected[j].Bounds)>.90)throw new InvalidDataException("Có khung đã chọn gần trùng nhau. Bỏ chọn hoặc sửa khung trước khi áp dụng.");
        var all=candidates.ToDictionary(x=>x.Id);var map=selected.ToDictionary(x=>x.Id,_=>Guid.NewGuid().ToString("N"));var result=new List<Annotation>();
        foreach(var item in selected.OrderBy(x=>x.Kind==MockupKind.Block?0:1).ThenByDescending(x=>(long)x.Bounds.Width*x.Bounds.Height)){
            if(!Enum.IsDefined(item.Kind)||item.Bounds.Width<8||item.Bounds.Height<8||item.Bounds.Intersect(scanBounds)!=item.Bounds||!double.IsFinite(item.Confidence)||item.Confidence<0||item.Confidence>1||item.Text.Length>2000)throw new InvalidDataException("Khung/nội dung không hợp lệ; sửa trước khi áp dụng.");
            if(item.LabelForId!=null&&(!all.TryGetValue(item.LabelForId,out var control)||control.Kind!=MockupKind.Input||item.Kind is not(MockupKind.Label or MockupKind.Title)))throw new InvalidDataException("Liên kết label cần trỏ đến một ô nhập trong danh sách.");
            var a=MockupStudio.Create(item.Bounds,new(0,0,document.OriginalWidth,document.OriginalHeight),item.Colors);a.Id=map[item.Id];a.Mockup!.Type=item.Kind;a.Mockup.FromScan=true;a.Mockup.LabelForId=item.LabelForId is {} link&&map.TryGetValue(link,out var id)?id:null;a.Name="Mockup · "+Label(item.Kind);a.Text=item.Text;a.FontSize=Math.Clamp(item.Kind is MockupKind.Input or MockupKind.Button?item.Bounds.Height*.45:item.Bounds.Height*.65,8,72);a.TextPadding=item.Kind is MockupKind.Input or MockupKind.Button?4:item.OcrBounds!=null?2:0;a.TextAlignment=item.Kind==MockupKind.Button?TextAlign.Center:TextAlign.Left;a.Bold=item.Kind==MockupKind.Title;a.StrokeEnabled=item.Kind==MockupKind.Input;a.Color=item.BorderColor;a.Stroke=Math.Max(1,item.BorderWidth);a.StrokeEnabled=item.BorderWidth>0;a.CornerRadius=item.Radius;a.FontFamily=item.FontFamily;a.FontSize=item.FontSize>0?item.FontSize:a.FontSize;a.Bold=item.FontBold;a.Italic=item.FontItalic;a.Underline=item.FontUnderline;a.Mockup.SemanticRole=item.SemanticRole;
            bool split=item.TextRuns.Count>0&&item.Text==string.Join("\n",item.TextRuns.Select(t=>t.Text));
            if(split){a.Text="";string group=Guid.NewGuid().ToString("N");a.GroupId=group;result.Add(a);
                foreach(var run in item.TextRuns){var bounds=run.Bounds.Intersect(item.Bounds);if(bounds.Width<8||bounds.Height<8)continue;var text=MockupStudio.Create(bounds,new(0,0,document.OriginalWidth,document.OriginalHeight),new(item.Colors.Fill,item.Colors.Fill,run.Foreground,1));text.Text=run.Text;text.Name="Mockup · Chữ trong "+Label(item.Kind);text.Mockup!.FromScan=true;text.Mockup.EraseOriginal=false;text.FontFamily=item.FontFamily;text.FontSize=a.FontSize;text.Bold=a.Bold;text.Italic=a.Italic;text.Underline=a.Underline;text.GroupId=group;result.Add(text);}
            }else result.Add(a);
        }
        var blocks=selected.Where(x=>x.Kind==MockupKind.Block).OrderByDescending(x=>(long)x.Bounds.Width*x.Bounds.Height).ToArray();
        var blockGroups=blocks.ToDictionary(x=>x.Id,_=>Guid.NewGuid().ToString("N"));
        foreach(var a in result){var b=new PixelRect((int)a.Left,(int)a.Top,(int)a.Width,(int)a.Height);var parents=blocks.Where(x=>a.Id==map[x.Id]||Contains(x.Bounds,b,.98)&&(long)x.Bounds.Width*x.Bounds.Height>(long)b.Width*b.Height).Select(x=>blockGroups[x.Id]).ToList();parents.AddRange(GroupLayout.Chain(a));if(parents.Count>0)GroupLayout.Set(a,parents);}
        var originalChains=result.ToDictionary(a=>a.Id,GroupLayout.Chain);var rows=new Dictionary<string,string>();var controls=result.Where(a=>a.Mockup?.Type is MockupKind.Button or MockupKind.Input&&originalChains[a.Id].LastOrDefault() is {} leaf&&!blockGroups.Values.Contains(leaf)).ToDictionary(a=>originalChains[a.Id][^1]);
        foreach(var a in result){var owner=selected.FirstOrDefault(x=>map[x.Id]==a.Id);var parent=originalChains[a.Id].LastOrDefault() is {} leaf&&controls.TryGetValue(leaf,out var found)?found:null;owner??=parent==null?null:selected.FirstOrDefault(x=>map[x.Id]==parent.Id);
            if(owner?.LayoutGroup is not {} row)continue;var chain=originalChains[a.Id];bool control=parent!=null;var prefix=control&&chain.Length>0?chain[..^1]:chain;string key=string.Join("/",prefix)+":"+row;if(!rows.TryGetValue(key,out var group))rows[key]=group=Guid.NewGuid().ToString("N");GroupLayout.Set(a,prefix.Concat(new[]{group}).Concat(control?chain[^1..]:Array.Empty<string>()));}
        return result;
    }
    private static int resultVersion(List<Annotation> additions)=>additions.Any(a=>a.GroupPath?.Count>0)?21:19;
    public static void Apply(EditorDocument document,IReadOnlyList<ScanElement> candidates,PixelRect scanBounds)
    {
        var additions=Prepare(document,candidates,scanBounds);var copy=document.Snapshot();copy.Annotations.AddRange(additions);copy.Version=Math.Max(copy.Version,additions.Any(a=>a.Mockup?.SemanticRole!=null)?22:resultVersion(additions));copy.Validate(copy.OriginalWidth,copy.OriginalHeight,copy.SourceHash);document.Annotations.AddRange(additions);document.Version=copy.Version;
    }
}
