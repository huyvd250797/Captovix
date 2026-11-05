namespace Captovix;
public sealed record HybridFusionResult(ScanResult Scan,int Matched,string Message);
public static class HybridFusion
{
    private static string Normalize(string s)=>string.Join(" ",s.Normalize().Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
    private static bool Contains(PixelRect outer,PixelRect inner)=>outer.Intersect(inner)==inner;
    private static MockupKind? Kind(string type)=>type switch{"Button"=>MockupKind.Button,"Edit" or "ComboBox"=>MockupKind.Input,"Text"=>MockupKind.Label,"Pane" or "Group" or "Table" or "DataGrid" or "Tab" or "TabItem" or "Menu" or "MenuItem" or "Image"=>MockupKind.Block,_=>null};
    public static HybridFusionResult Merge(ScanResult scan,HybridEvidence? evidence,byte[] pixels,int width,int height,PixelRect region,CancellationToken token=default){
        if(evidence==null||evidence.Version!=1)return new(scan,0,"Ảnh/OCR · ảnh không kèm cấu trúc UI");
        var elements=scan.Elements;int matched=0;long budget=HybridCapture.MaxVerifyPixels;
        foreach(var node in evidence.Nodes.Take(HybridCapture.MaxNodes)){
            token.ThrowIfCancellationRequested();var kind=Kind(node.Type);if(kind==null||node.PixelHash.Length!=64)continue;
            var image=HybridCapture.ToImage(node.Bounds,evidence.ScreenBounds);long area=(long)image.Width*image.Height;
            if(!Contains(region,image)||image.Width<8||image.Height<8||area>budget)continue;budget-=area;
            var local=image with{X=image.X-region.X,Y=image.Y-region.Y};if(HybridCapture.PatchHash(pixels,width,height,local)!=node.PixelHash)continue;
            var candidate=elements.Where(e=>(kind==MockupKind.Label?e.Kind is MockupKind.Label or MockupKind.Title:kind==MockupKind.Block?e.Kind==MockupKind.Block:e.Kind is MockupKind.Button or MockupKind.Input)&&(ElementScan.Overlap(e.Bounds,image)>.55||kind==MockupKind.Label&&Contains(image,e.OcrBounds??e.Bounds)&&Normalize(node.Name)==Normalize(e.Text))).OrderByDescending(e=>ElementScan.Overlap(e.Bounds,image)).FirstOrDefault();
            bool nameMatches=candidate!=null&&node.Name.Length>0&&Normalize(candidate.Text)==Normalize(node.Name);
            if(kind==MockupKind.Label){if(!nameMatches)continue;}
            else if(candidate==null&&kind is MockupKind.Button or MockupKind.Input){
                // A UI-only name never creates visible text. At least one OCR line
                // must agree, in addition to the capture-time pixel proof.
                var children=elements.Where(e=>e.Kind is MockupKind.Label or MockupKind.Title&&Contains(image,e.Bounds)&&e.Text.Length>0).OrderBy(e=>e.Bounds.Y).ThenBy(e=>e.Bounds.X).ToArray();
                if(children.Length==0||Normalize(string.Join(" ",children.Select(e=>e.Text)))!=Normalize(node.Name)||elements.Count>=ElementScan.MaxCandidates)continue;
                candidate=new ScanElement{Bounds=image,Kind=kind.Value,Text=string.Join("\n",children.Select(e=>e.Text)),TextRuns=children.Select(e=>new ScanText(e.OcrBounds??e.Bounds,e.Text,e.Colors.Foreground)).ToList(),Colors=ElementScan.Sample(pixels,width,height,local),FontFamily=children[0].FontFamily,FontSize=children[0].FontSize,Confidence=.90};
                ElementScan.MeasureOutline(candidate,pixels,width,height,local);elements.RemoveAll(e=>children.Contains(e));elements.Add(candidate);nameMatches=true;
            }
            if(candidate==null&&node.Type is "Table" or "DataGrid" or "Tab" or "TabItem" or "Menu" or "MenuItem" or "Image"&&elements.Count<ElementScan.MaxCandidates){candidate=new(){Bounds=image,Kind=MockupKind.Block,Confidence=.86,KeepImage=true,Colors=ElementScan.Sample(pixels,width,height,local),Reason="Cấu trúc UI đã khớp pixel; giữ ảnh gốc hoặc dựng thủ công từng phần."};elements.Add(candidate);}
            if(candidate==null)continue;
            if(kind!=MockupKind.Label){candidate.Bounds=image;candidate.Kind=kind.Value;candidate.Colors=ElementScan.Sample(pixels,width,height,local);ElementScan.MeasureOutline(candidate,pixels,width,height,local);}
            candidate.SemanticRole=node.Type switch{"Table" or "DataGrid"=>"Table","Tab" or "TabItem"=>"Tab","Menu" or "MenuItem"=>"Menu","Image"=>"Icon",_=>candidate.SemanticRole};
            candidate.UiSourceId=node.Id;candidate.UiParentId=node.ParentId;candidate.Confidence=Math.Min(.98,Math.Max(candidate.Confidence,.88));candidate.Reason+=" Hybrid: hình học UI đã đối chiếu pixel ảnh chụp; nội dung chữ giữ từ OCR.";
            if(nameMatches&&node.Font.Length>0&&node.FontPoints is >=4 and <=100&&double.IsFinite(node.FontPoints)){candidate.FontFamily=node.Font;candidate.FontSize=HybridCapture.FontPixels(node);candidate.FontBold=node.Bold;candidate.FontItalic=node.Italic;candidate.FontUnderline=node.Underline;candidate.StyleVerified=node.StyleComplete;if(StudioStyle.Hex(node.TextColor??""))candidate.Colors=candidate.Colors with{Foreground=node.TextColor!};candidate.Reason+=" Font từ UI Automation; cỡ chữ đã quy đổi DPI.";}
            matched++;
        }
        var ids=elements.Select(e=>e.Id).ToHashSet();foreach(var e in elements){if(e.LabelForId!=null&&!ids.Contains(e.LabelForId))e.LabelForId=null;}
        foreach(var label in elements.Where(e=>e.Kind is MockupKind.Label or MockupKind.Title&&e.UiParentId!=null)){
            var input=elements.Where(e=>e.Kind==MockupKind.Input&&e.UiParentId==label.UiParentId).OrderBy(e=>ElementScan.PairScore(label.Bounds,e.Bounds)).FirstOrDefault();if(input!=null&&ElementScan.PairScore(label.Bounds,input.Bounds)<100)label.LabelForId=input.Id;
        }
        var ordered=elements.OrderBy(e=>e.Kind==MockupKind.Block?1:0).ThenBy(e=>e.Bounds.Y).ThenBy(e=>e.Bounds.X).ToList();
        return new(new(ordered,scan.Truncated,scan.Warning),matched,matched>0?$"Hybrid · {matched} khung UI khớp ảnh/OCR":"Ảnh/OCR · cấu trúc UI không khớp vùng ảnh hiện tại");
    }
}
