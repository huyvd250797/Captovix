using System.IO;
namespace Captovix;
// A scan is a disposable draft. Nothing is applied to an EditorSession here.
public sealed class ScanElement
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public PixelRect Bounds {get;set;}
    public MockupKind Kind {get;set;}
    public string Text {get;set;}="";
    public double Confidence {get;set;}
    public string Reason {get;set;}="";
    public bool Include {get;set;}
    public string? LabelForId {get;set;}
    public MockupColors Colors {get;set;}=new("#FFFFFF","#FFFFFF","#202020",1);
    public string Label=>$"{(Include?"✓ ":"")}{ElementScan.Label(Kind)} · {Bounds.Width}×{Bounds.Height} · {Confidence:P0}"+(Uncertain?" · Cần kiểm tra":"");
    public bool Uncertain=>Confidence<.85||Colors.Flatness<.75||Kind is MockupKind.Input or MockupKind.Block;
}
public sealed record ScanResult(List<ScanElement> Elements,bool Truncated,string Warning="");
public static class ElementScan
{
    public const int MaxCandidates=200,MaxApply=120;
    public static string Label(MockupKind kind)=>kind switch{MockupKind.Label=>"Chữ",MockupKind.Title=>"Tiêu đề",MockupKind.Button=>"Nút",MockupKind.Input=>"Ô nhập",_=>"Khối giao diện"};
    public static PixelRect Clamp(PixelRect r,PixelRect region)=>r.Intersect(region);
    public static double Overlap(PixelRect a,PixelRect b){var i=a.Intersect(b);double v=(double)i.Width*i.Height;return v/Math.Max(1,(double)a.Width*a.Height+(double)b.Width*b.Height-v);}
    public static ScanResult Analyze(byte[] pixels,int width,int height,PixelRect region,IReadOnlyList<OcrTextLine> lines,CancellationToken token=default)
    {
        if(width<8||height<8||pixels.LongLength!=(long)width*height*4||width!=region.Width||height!=region.Height||(long)width*height>OcrContract.MaxPixels)throw new ArgumentException("Vùng quét không hợp lệ; tối đa20MP.");
        // Bound both memory and CPU. Components operate on quantized flat colors,
        // not on millions of high-resolution glyph edges.
        int step=Math.Max(1,(int)Math.Ceiling(Math.Sqrt((double)width*height/1_200_000))),w=(width+step-1)/step,h=(height+step-1)/step;
        var grid=new int[w*h];var visited=new bool[grid.Length];var queue=new int[grid.Length];
        for(int y=0;y<h;y++){token.ThrowIfCancellationRequested();for(int x=0;x<w;x++){int p=(Math.Min(height-1,y*step)*width+Math.Min(width-1,x*step))*4;int alpha=pixels[p+3];int b=(pixels[p]*alpha+255*(255-alpha))/255,g=(pixels[p+1]*alpha+255*(255-alpha))/255,r=(pixels[p+2]*alpha+255*(255-alpha))/255;grid[y*w+x]=(r/24)<<10|(g/24)<<5|b/24;}}
        var visual=new List<ScanElement>();bool truncated=false;
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
            if(holder!=null){holder.Text=Limit(holder.Text.Length==0?line.Text:holder.Text+"\n"+line.Text);holder.Confidence=Math.Min(.94,Math.Max(holder.Confidence,.55+Math.Clamp(line.Confidence,0,1)*.4));holder.Reason="Chữ OCR nằm trong khung màu/viền; kiểm tra nội dung và loại control.";continue;}
            var expanded=new PixelRect(Math.Max(0,b.X-2),Math.Max(0,b.Y-2),Math.Max(8,b.Width+4),Math.Max(8,b.Height+4)).Intersect(new(0,0,width,height));if(expanded.Width<8||expanded.Height<8)continue;
            var candidate=new ScanElement{Bounds=expanded with{X=expanded.X+region.X,Y=expanded.Y+region.Y},Text=Limit(line.Text.Trim()),Kind=b.Height>median*1.35&&b.Height>=22?MockupKind.Title:MockupKind.Label,Confidence=Math.Clamp(line.Confidence,0,1),Reason="Khung chữ từ OCR; kiểm tra dấu tiếng Việt và bao hết nét chữ.",Colors=Sample(pixels,width,height,expanded)};
            if(!result.Any(x=>x.Kind is MockupKind.Label or MockupKind.Title&&Overlap(x.Bounds,candidate.Bounds)>.75))result.Add(candidate);
        }
        foreach(var label in result.Where(x=>x.Kind is MockupKind.Label or MockupKind.Title)){
            var a=label.Bounds;var nearby=result.Where(x=>x.Kind==MockupKind.Input).Select(x=>new{Item=x,Score=PairScore(a,x.Bounds)}).Where(x=>x.Score<100).OrderBy(x=>x.Score).ToArray();
            if(nearby.Length>0){label.LabelForId=nearby[0].Item.Id;label.Reason+=" Gợi ý ghép với ô nhập gần nhất; có thể đổi/bỏ liên kết.";if(nearby.Length>1&&nearby[1].Score-nearby[0].Score<15){label.Confidence=Math.Min(label.Confidence,.7);label.Reason+=" Có nhiều ô nhập gần nhau.";}}
        }
        if(result.Count>MaxCandidates)truncated=true;
        // Prefer text/control proposals over container suggestions; apply order is separate.
        result=result.OrderBy(x=>x.Kind==MockupKind.Block?1:0).ThenBy(x=>x.Bounds.Y).ThenBy(x=>x.Bounds.X).Take(MaxCandidates).ToList();var ids=result.Select(x=>x.Id).ToHashSet();foreach(var x in result)if(x.LabelForId!=null&&!ids.Contains(x.LabelForId))x.LabelForId=null;
        return new(result,truncated,truncated?"Đã giới hạn200khung. Quét vùng nhỏ hơn để xem phần còn lại.":"");
    }
    private static string Limit(string value)=>value.Length<=2000?value:value[..2000];
    private static bool Contains(PixelRect a,PixelRect b,double fraction){var i=a.Intersect(b);return (double)i.Width*i.Height>=b.Width*(double)b.Height*fraction;}
    private static double PairScore(PixelRect label,PixelRect control){double dy=Math.Abs(label.Y+label.Height*.5-control.Y-control.Height*.5),dx=control.X-label.Right;double below=control.Y-label.Bottom;
        double left=dx>=-2&&dx<=100&&dy<=Math.Max(12,control.Height*.6)?Math.Max(0,dx)+dy*1.5:1000;
        double top=below>=-2&&below<=48&&Math.Abs(control.X-label.X)<Math.Max(40,control.Width*.2)?Math.Max(0,below)+Math.Abs(control.X-label.X)+10:1000;return Math.Min(left,top);}
    private static bool HasBorder(byte[] p,int width,int height,PixelRect b,string fill){int reference=Convert.ToInt32(fill[1..],16),seen=0,different=0;void Test(int x,int y){int n=(y*width+x)*4;int d=Math.Abs(p[n+2]-(reference>>16&255))+Math.Abs(p[n+1]-(reference>>8&255))+Math.Abs(p[n]-(reference&255));seen++;if(d>35)different++;}for(int x=b.X;x<b.Right;x+=Math.Max(1,b.Width/40)){Test(x,b.Y);Test(x,b.Bottom-1);}for(int y=b.Y;y<b.Bottom;y+=Math.Max(1,b.Height/40)){Test(b.X,y);Test(b.Right-1,y);}return different>seen*.45;}
    private static int Distance(string a,string b){int x=Convert.ToInt32(a[1..],16),y=Convert.ToInt32(b[1..],16);return Math.Abs((x>>16&255)-(y>>16&255))+Math.Abs((x>>8&255)-(y>>8&255))+Math.Abs((x&255)-(y&255));}
    public static MockupColors Sample(byte[] pixels,int width,int height,PixelRect b){var ring=new PixelRect(b.X-4,b.Y-4,b.Width+8,b.Height+8).Intersect(new(0,0,width,height));var sample=new byte[ring.Width*ring.Height*4];for(int y=0;y<ring.Height;y++)Array.Copy(pixels,((ring.Y+y)*width+ring.X)*4,sample,y*ring.Width*4,ring.Width*4);return MockupStudio.Colors(sample,ring.Width,ring.Height,b with{X=b.X-ring.X,Y=b.Y-ring.Y});}
    public static List<Annotation> Prepare(EditorDocument document,IReadOnlyList<ScanElement> candidates,PixelRect scanBounds)
    {
        var selected=candidates.Where(x=>x.Include).ToArray();if(selected.Length==0)throw new InvalidDataException("Chọn ít nhất một phần tử đã kiểm tra.");if(selected.Length>MaxApply||document.Annotations.Count+selected.Length>10000)throw new InvalidDataException("Tối đa120 phần tử mỗi lần áp dụng.");
        for(int i=0;i<selected.Length;i++)for(int j=i+1;j<selected.Length;j++)if(Overlap(selected[i].Bounds,selected[j].Bounds)>.90)throw new InvalidDataException("Có khung đã chọn gần trùng nhau. Bỏ chọn hoặc sửa khung trước khi áp dụng.");
        var all=candidates.ToDictionary(x=>x.Id);var map=selected.ToDictionary(x=>x.Id,_=>Guid.NewGuid().ToString("N"));var result=new List<Annotation>();
        foreach(var item in selected.OrderBy(x=>x.Kind==MockupKind.Block?0:1).ThenByDescending(x=>(long)x.Bounds.Width*x.Bounds.Height)){
            if(!Enum.IsDefined(item.Kind)||item.Bounds.Width<8||item.Bounds.Height<8||item.Bounds.Intersect(scanBounds)!=item.Bounds||!double.IsFinite(item.Confidence)||item.Confidence<0||item.Confidence>1||item.Text.Length>2000)throw new InvalidDataException("Khung/nội dung không hợp lệ; sửa trước khi áp dụng.");
            if(item.LabelForId!=null&&(!all.TryGetValue(item.LabelForId,out var control)||control.Kind!=MockupKind.Input||item.Kind is not(MockupKind.Label or MockupKind.Title)))throw new InvalidDataException("Liên kết label cần trỏ đến một ô nhập trong danh sách.");
            var a=MockupStudio.Create(item.Bounds,new(0,0,document.OriginalWidth,document.OriginalHeight),item.Colors);a.Id=map[item.Id];a.Mockup!.Type=item.Kind;a.Mockup.FromScan=true;a.Mockup.LabelForId=item.LabelForId is {} link&&map.TryGetValue(link,out var id)?id:null;a.Name="Mockup · "+Label(item.Kind);a.Text=item.Text;a.FontSize=Math.Clamp(item.Kind is MockupKind.Input or MockupKind.Button?item.Bounds.Height*.45:item.Bounds.Height*.65,8,72);a.TextPadding=item.Kind is MockupKind.Input or MockupKind.Button?4:0;a.TextAlignment=item.Kind==MockupKind.Button?TextAlign.Center:TextAlign.Left;a.Bold=item.Kind==MockupKind.Title;a.StrokeEnabled=item.Kind==MockupKind.Input;a.Color="#B8C0CC";a.CornerRadius=item.Kind==MockupKind.Block?0:4;result.Add(a);
        }return result;
    }
    public static void Apply(EditorDocument document,IReadOnlyList<ScanElement> candidates,PixelRect scanBounds)
    {
        var additions=Prepare(document,candidates,scanBounds);var copy=document.Snapshot();copy.Annotations.AddRange(additions);copy.Version=19;copy.Validate(copy.OriginalWidth,copy.OriginalHeight,copy.SourceHash);document.Annotations.AddRange(additions);document.Version=19;
    }
}
