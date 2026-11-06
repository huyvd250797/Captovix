using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Captovix;

public sealed record RecoveryOperation(string Action,DateTimeOffset At,string ElementId,string? RecoveryId,PixelRect? Before,PixelRect? After);
public sealed record RecoveryPatch
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public PixelRect Bounds {get;set;}
    public MockupKind Kind {get;set;}
    public string Background {get;set;}="#FFFFFF";
    public string Mask {get;set;}="";
    public bool NeedsReview {get;set;}
    public List<PixelRect> Protected {get;set;}=[];
    public RecoveryPatch Copy()=>this with{Protected=Protected.ToList()};
    public void Validate(int width,int height){long pixels=(long)Bounds.Width*Bounds.Height;if(!Enum.IsDefined(Kind)||!Guid.TryParseExact(Id,"N",out _)||Bounds.X<0||Bounds.Y<0||Bounds.Width<1||Bounds.Height<1||(long)Bounds.X+Bounds.Width>width||(long)Bounds.Y+Bounds.Height>height||pixels>20_000_000||!StudioStyle.Hex(Background)||Mask==null||Mask.Length>3_333_336||Protected==null||Protected.Count>10000||Protected.Any(p=>p.IsEmpty||p.Intersect(Bounds)!=p))throw new InvalidDataException("Mặt nạ nền không hợp lệ.");try{if(Convert.FromBase64String(Mask).Length!=(pixels+7)/8)throw new InvalidDataException("Sai kích thước mặt nạ nền.");}catch(FormatException ex){throw new InvalidDataException("Mặt nạ nền lỗi.",ex);}}
}
public enum AnnotationKind { Arrow, Rectangle, Ellipse, Text, Highlight, Step, Blur, Pixelate, Cover, Callout, Freehand, RoundedRectangle, Triangle, Diamond, Trapezoid, Pentagon, Hexagon, Star, Line, Magnifier, Spotlight, ImageLayer }
public readonly record struct AnnotationPoint(double X,double Y);
public enum MockupKind { Label,Title,Button,Input,Block }
public sealed record RasterClear(string ElementId,PixelRect Bounds,string Background);
public sealed record MockupElement
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? RecoveryId {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingDefault)] public bool FromScan {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? LabelForId {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? SemanticRole {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingDefault)] public bool RasterBacking {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingDefault)] public bool PreserveRaster {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingDefault)] public bool RasterDetached {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? RasterOwnerId {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public PixelRect? RasterImageBounds {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public List<RasterClear>? RasterClears {get;set;}
    public MockupKind Type {get;set;}
    public PixelRect SourceBounds {get;set;}
    public string Background {get;set;}="#FFFFFF";
    public bool EraseOriginal {get;set;}=true;
    public MockupElement Copy()=>this with{RasterClears=RasterClears?.ToList()};
    public void Validate(Annotation a,int width,int height)
    {
        if(SemanticRole!=null&&!new[]{"Icon","Table","Tab","Menu","Panel","Row","Column"}.Contains(SemanticRole))throw new InvalidDataException("Loại cấu trúc Mockup không hợp lệ.");
        if(RecoveryId!=null&&!Guid.TryParseExact(RecoveryId,"N",out _)||LabelForId!=null&&!Guid.TryParseExact(LabelForId,"N",out _)||a.Kind!=AnnotationKind.Rectangle||!Enum.IsDefined(Type)||!StudioStyle.Hex(Background)||SourceBounds.Width<1||SourceBounds.Height<1||Math.Abs((long)SourceBounds.X)>1000000||Math.Abs((long)SourceBounds.Y)>1000000||SourceBounds.Width>32768||SourceBounds.Height>32768||a.Width<8||a.Height<8||a.Text.Length>2000||EraseOriginal&&(SourceBounds.X<0||SourceBounds.Y<0||(long)SourceBounds.X+SourceBounds.Width>width||(long)SourceBounds.Y+SourceBounds.Height>height))throw new InvalidDataException("Đối tượng Mockup hoặc vùng che nền không hợp lệ.");
    }
}
public sealed class Annotation
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public MockupElement? Mockup {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingDefault)] public bool ImagePlaceholder {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public LayerImageStyle? ImageStyle {get;set;}
    public string? PixelPng {get;set;}
    public bool CalloutBodyPivot {get;set;}
    public RegionShape RegionShape {get;set;}
    public double Magnification {get;set;}=2;
    public double FocusOpacity {get;set;}=30.58823529411765;
    public ObjectShadow Shadow {get;set;}=new();
    public string FontFamily {get;set;}="Segoe UI";
    public bool Bold {get;set;}
    public bool Italic {get;set;}
    public bool Underline {get;set;}
    public string? TextColor {get;set;}
    public TextAlign TextAlignment {get;set;}
    public TextVertical VerticalAlignment {get;set;}
    public double? TextPadding {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingDefault)] public bool TextFrame {get;set;}
    public double TextWidth {get;set;}
    public double LineSpacing {get;set;}
    public bool AutoFitText {get;set;}=true;
    public CalloutStyle CalloutStyle {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? StepStrokeColor {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public bool? StepFillVisible {get;set;}
    public StepStyle StepStyle {get;set;}
    public ConnectorRoute Route {get;set;}
    public AnnotationPoint? Control {get;set;}
    public ConnectorAnchor? StartAnchor {get;set;}
    public ConnectorAnchor? EndAnchor {get;set;}
    public string Name {get;set;}="";
    public bool Locked {get;set;}
    public bool Visible {get;set;}=true;
    public double Rotation {get;set;}
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public AnnotationKind Kind {get;set;}
    public double X1 {get;set;}
    public double Y1 {get;set;}
    public double X2 {get;set;}
    public double Y2 {get;set;}
    public string Color {get;set;}="#008B92";
    public double Stroke {get;set;}=4;
    public string FillColor {get;set;}="#FFFFFF";
    public bool FillEnabled {get;set;}
    public bool StrokeEnabled {get;set;}=true;
    public double StrokeOpacity {get;set;}=100;
    public double FillOpacity {get;set;}=100;
    public StrokePattern Dash {get;set;}=StrokePattern.Solid;
    public double CornerRadius {get;set;}=16;
    public int StarPoints {get;set;}=5;
    public double StarInset {get;set;}=.45;
    public ArrowTip Tip {get;set;}=ArrowTip.Triangle;
    public bool DoubleArrow {get;set;}
    public double ArrowHeadSize {get;set;} // zero = legacy automatic size
    public double FontSize {get;set;}=28;
    public string Text {get;set;}="";
    public int Number {get;set;}=1;
    public int EffectSize {get;set;}=24;
    public string? GroupId {get;set;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public List<string>? GroupPath {get;set;}
    public double TailX {get;set;}
    public double TailY {get;set;}
    public List<AnnotationPoint> Points {get;set;}=[];
    public const int MaxPoints=4096;
    [JsonIgnore] public bool IsRedaction=>Kind is AnnotationKind.Blur or AnnotationKind.Pixelate or AnnotationKind.Cover;
    [JsonIgnore] public double Left=>Math.Min(X1,X2);
    [JsonIgnore] public double Top=>Math.Min(Y1,Y2);
    [JsonIgnore] public double Width=>Math.Abs(X2-X1);
    [JsonIgnore] public double Height=>Math.Abs(Y2-Y1);
    public Annotation Copy()=>new(){Mockup=Mockup?.Copy(),TextFrame=TextFrame,ImagePlaceholder=ImagePlaceholder,ImageStyle=ImageStyle?.Copy(),CalloutBodyPivot=CalloutBodyPivot,PixelPng=PixelPng,Shadow=Shadow.Copy(),RegionShape=RegionShape,Magnification=Magnification,FocusOpacity=FocusOpacity,FontFamily=FontFamily,Bold=Bold,Italic=Italic,Underline=Underline,TextColor=TextColor,TextAlignment=TextAlignment,VerticalAlignment=VerticalAlignment,TextPadding=TextPadding,TextWidth=TextWidth,LineSpacing=LineSpacing,AutoFitText=AutoFitText,CalloutStyle=CalloutStyle,StepStyle=StepStyle,StepStrokeColor=StepStrokeColor,StepFillVisible=StepFillVisible,Route=Route,Control=Control,StartAnchor=StartAnchor,EndAnchor=EndAnchor,Name=Name,Locked=Locked,Visible=Visible,Rotation=Rotation,Id=Id,Kind=Kind,X1=X1,Y1=Y1,X2=X2,Y2=Y2,Color=Color,Stroke=Stroke,FillColor=FillColor,FillEnabled=FillEnabled,StrokeEnabled=StrokeEnabled,StrokeOpacity=StrokeOpacity,FillOpacity=FillOpacity,Dash=Dash,CornerRadius=CornerRadius,StarPoints=StarPoints,StarInset=StarInset,Tip=Tip,DoubleArrow=DoubleArrow,ArrowHeadSize=ArrowHeadSize,FontSize=FontSize,Text=Text,Number=Number,EffectSize=EffectSize,GroupPath=GroupPath?.ToList(),GroupId=GroupId,TailX=TailX,TailY=TailY,Points=[..Points]};
    public void Translate(double dx,double dy){X1+=dx;X2+=dx;Y1+=dy;Y2+=dy;if(Control is {} c)Control=new(c.X+dx,c.Y+dy);if(Kind==AnnotationKind.Callout){TailX+=dx;TailY+=dy;}for(int i=0;i<Points.Count;i++)Points[i]=new(Points[i].X+dx,Points[i].Y+dy);}
    public void AddPoint(double x,double y){if(Points.Count>=MaxPoints)return;Points.Add(new(x,y));if(Points.Count==1){X1=X2=x;Y1=Y2=y;}else{X1=Math.Min(X1,x);Y1=Math.Min(Y1,y);X2=Math.Max(X2,x);Y2=Math.Max(Y2,y);}}
}

public sealed class EditorDocument
{
    public const int MaximumSupportedVersion=23;
    public string? PixelBasePng {get;set;}
    public int Version {get;set;}=12;
    public int OriginalWidth {get;set;}
    public int OriginalHeight {get;set;}
    public string SourceHash {get;set;}="";
    public PixelRect? Crop {get;set;}
    public ImageAppearance Appearance {get;set;}=new();
    public List<RecoveryOperation> RecoveryHistory {get;set;}=[];
    public List<RecoveryPatch> Recoveries {get;set;}=[];
    public List<Annotation> Annotations {get;set;}=[];
    [JsonIgnore] public string? EditingGroup {get;set;}
    [JsonIgnore] public PixelRect Viewport=>Crop??new PixelRect(0,0,OriginalWidth,OriginalHeight);
    [JsonIgnore] public bool HasEdits=>Crop!=null||Recoveries.Count>0||Annotations.Count>0||PixelBasePng!=null||Appearance.HasEdits;
    public static readonly JsonSerializerOptions JsonOptions=new(){WriteIndented=true,Converters={new JsonStringEnumConverter()}};
    public string Serialize()=>JsonSerializer.Serialize(this,JsonOptions);
    public static EditorDocument Deserialize(string json)=>JsonSerializer.Deserialize<EditorDocument>(json,JsonOptions)??throw new InvalidDataException("Tài liệu chỉnh sửa trống.");
    public EditorDocument Clone()=>Deserialize(Serialize());
    public EditorDocument Snapshot()=>new(){EditingGroup=EditingGroup,Version=Version,OriginalWidth=OriginalWidth,OriginalHeight=OriginalHeight,SourceHash=SourceHash,Crop=Crop,PixelBasePng=PixelBasePng,Appearance=Appearance.Copy(),RecoveryHistory=RecoveryHistory.ToList(),Recoveries=Recoveries.Select(p=>p.Copy()).ToList(),Annotations=Annotations.Select(a=>a.Copy()).ToList()};
    public void Validate(int width,int height,string hash)
    {
        if(Version<1||Version>MaximumSupportedVersion)throw new InvalidDataException("Phiên bản dữ liệu chỉnh sửa chưa được hỗ trợ.");
        if(OriginalWidth!=width||OriginalHeight!=height||SourceHash!=hash)throw new InvalidDataException("Ảnh gốc đã thay đổi. Bản chú thích cũ được giữ riêng, không tự gán lên ảnh khác.");
        if(Crop is {} c&&(c.IsEmpty||c.X<0||c.Y<0||c.Right>width||c.Bottom>height))throw new InvalidDataException("Vùng crop không hợp lệ.");
        if(Annotations==null||Annotations.Count>10000)throw new InvalidDataException("Số đối tượng vượt giới hạn tài liệu.");
        if(RecoveryHistory==null||RecoveryHistory.Count>100||RecoveryHistory.Any(x=>x==null||string.IsNullOrWhiteSpace(x.Action)||x.Action.Length>32||x.At<DateTimeOffset.UnixEpoch))throw new InvalidDataException("Lịch sử phục hồi không hợp lệ.");
        if(Recoveries==null||Recoveries.Any(p=>p==null||p.Mask==null)||Recoveries.Count>500||Recoveries.Sum(p=>(long)p.Bounds.Width*p.Bounds.Height)>20_000_000||Recoveries.Select(p=>p.Id).Distinct().Count()!=Recoveries.Count||Recoveries.Sum(p=>(long)p.Mask.Length)>40_000_000)throw new InvalidDataException("Mặt nạ nền vượt giới hạn.");foreach(var patch in Recoveries)patch.Validate(width,height);
        var ids=new HashSet<string>();var totalPoints=0;
        foreach(var a in Annotations){
            if(a==null||string.IsNullOrEmpty(a.Id)||!ids.Add(a.Id)||!Enum.IsDefined(a.Kind)||!double.IsFinite(a.X1)||!double.IsFinite(a.X2)||!double.IsFinite(a.Y1)||!double.IsFinite(a.Y2)||Math.Abs(a.X1)>1000000||Math.Abs(a.X2)>1000000||Math.Abs(a.Y1)>1000000||Math.Abs(a.Y2)>1000000||!double.IsFinite(a.Stroke)||!double.IsFinite(a.FontSize)||a.Stroke<1||a.Stroke>40||a.FontSize<8||a.FontSize>240||a.EffectSize<4||a.EffectSize>64||a.Text==null||a.Text.Length>20000||a.Number<1||a.Number>9999||!ValidColor(a.Color))
                throw new InvalidDataException("Dữ liệu đối tượng chú thích không hợp lệ.");
            if(a.Name==null||a.Name.Length>80||a.Name.Any(char.IsControl)||!double.IsFinite(a.Rotation)||Math.Abs(a.Rotation)>360)throw new InvalidDataException("Tên layer hoặc góc xoay không hợp lệ.");
            a.Mockup?.Validate(a,width,height);if(a.Mockup?.RecoveryId is {} recovery&&!Recoveries.Any(p=>p.Id==recovery))throw new InvalidDataException("Thiếu mặt nạ phục hồi nền.");
            ShapeStudio.ValidateStyle(a);if(Version<8)TextStudio.MigrateLegacy(a);
            if(a.TextFrame&&(a.Kind!=AnnotationKind.Text||a.Width<2||a.Height<2))throw new InvalidDataException("Khung chữ phải rộng/cao ít nhất 2 px.");
            TextStudio.Validate(a);FocusStudio.Validate(a);StudioStyle.Validate(a);
            if(a.GroupId!=null&&!Guid.TryParseExact(a.GroupId,"N",out _))throw new InvalidDataException("Mã nhóm chú thích không hợp lệ.");
            if(!double.IsFinite(a.TailX)||!double.IsFinite(a.TailY)||Math.Abs(a.TailX)>1000000||Math.Abs(a.TailY)>1000000)throw new InvalidDataException("Đuôi callout không hợp lệ.");
            if(a.Points==null||a.Points.Count>Annotation.MaxPoints||(totalPoints+=a.Points.Count)>100000||a.Points.Any(p=>!double.IsFinite(p.X)||!double.IsFinite(p.Y)||Math.Abs(p.X)>1000000||Math.Abs(p.Y)>1000000))throw new InvalidDataException("Nét vẽ vượt giới hạn hoặc không hợp lệ.");
            if(a.Kind==AnnotationKind.Freehand&&a.Points.Count==0||a.Kind!=AnnotationKind.Freehand&&a.Points.Count!=0)throw new InvalidDataException("Dữ liệu điểm không phù hợp công cụ.");
            if(a.Kind==AnnotationKind.Callout&&(a.Width<36||a.Height<36))throw new InvalidDataException("Hộp callout quá nhỏ.");
        }
        var targets=Annotations.ToDictionary(a=>a.Id);foreach(var a in Annotations){if(a.Mockup?.LabelForId is {} linked&&(!targets.TryGetValue(linked,out var control)||control.Mockup?.Type!=MockupKind.Input))a.Mockup.LabelForId=null;ConnectorStudio.Validate(a,targets);}
        ConnectorStudio.Resolve(this);
        if(Appearance==null)throw new InvalidDataException("Thiếu cấu hình ảnh.");
        _=new ImageLayout(Viewport,Appearance);
        MockupRasterState.Validate(this);GroupLayout.Validate(this);PixelStudio.Validate(this);Version=Annotations.Any(a=>a.Mockup is {RasterBacking:true} or {RasterOwnerId:not null} or {PreserveRaster:true})?23:Annotations.Any(a=>a.Mockup?.SemanticRole!=null)?22:Annotations.Any(a=>a.GroupPath?.Count>0)?21:Recoveries.Count>0||RecoveryHistory.Count>0?20:Annotations.Any(a=>a.Mockup is {} m&&(m.FromScan||m.Type is MockupKind.Input or MockupKind.Block||m.LabelForId!=null))?19:Annotations.Any(a=>a.Mockup!=null)?18:Annotations.Any(a=>a.TextFrame)?17:Annotations.Any(a=>a.Kind==AnnotationKind.Step&&(a.StepStrokeColor!=null||a.StepFillVisible!=null))?16:Annotations.Any(a=>a.ImageStyle is {} style&&(style.Mask!=LayerMask.Rectangle||style.Opacity!=1))?15:Appearance.HasPrecision?14:Appearance.DrawOutsideImage?13:12;
    }
    private static bool ValidColor(string color)=>color is {Length:7}&&color[0]=='#'&&color.AsSpan(1).ToString().All(Uri.IsHexDigit);
    public bool ApplyCrop(PixelRect crop)
    {
        var clipped=crop.Intersect(Viewport);if(clipped.Width<2||clipped.Height<2)return false;
        try{_=new ImageLayout(clipped,Appearance);}catch(InvalidDataException){return false;}
        Crop=clipped==new PixelRect(0,0,OriginalWidth,OriginalHeight)?null:clipped;return true;
    }
    public int NextStep()=>Math.Min(9999,Annotations.Where(a=>a.Kind==AnnotationKind.Step).Select(a=>a.Number).DefaultIfEmpty(0).Max()+1);
}

public sealed class EditorSession
{
    public static Func<bool> AuthorizeMutation {get;set;}=()=>true;
    private static void CheckAuthorization(){if(!AuthorizeMutation())throw new InvalidOperationException("Công cụ chỉnh sửa đang khóa theo trạng thái bản quyền.");}
    private readonly List<HistorySnapshot> _undo=[];
    private readonly List<HistorySnapshot> _redo=[];
    private HistorySnapshot? _before;
    private readonly int _historyCharBudget;
    public EditorDocument Document {get;private set;}
    public bool Dirty {get;private set;}
    public long Revision {get;private set;}
    public object HistoryStats=>new{Undo=_undo.Count,Redo=_redo.Count,Bytes=HistorySnapshot.RetainedBytes(_undo.Concat(_redo)),BudgetChars=_historyCharBudget};
    public bool CanUndo=>_undo.Count>0;
    public bool CanRedo=>_redo.Count>0;
    public bool InTransaction=>_before!=null;
    public Action<string,EditorDocument>? BeforeCommit {get;set;}
    public Action<HistorySnapshot,EditorDocument>? BeforeCommitSnapshot {get;set;}
    public event Action? Changed;
    public EditorSession(EditorDocument document,int historyCharBudget=8_000_000){Document=document;_historyCharBudget=Math.Max(1024,historyCharBudget);}
    private void TrimHistory(List<HistorySnapshot> added, List<HistorySnapshot> other)
    {
        while(_undo.Count+_redo.Count>100||HistorySnapshot.RetainedBytes(_undo.Concat(_redo))>2L*_historyCharBudget){
            // Evict distant history first while retaining the adjacent steps.
            var oldest=added.Count>1||other.Count==0?added:other;
            oldest.RemoveAt(0);
        }
    }
    public void Begin(){CheckAuthorization();if(_before!=null)return;ConnectorStudio.Resolve(Document);_before=HistorySnapshot.Capture(Document);}
    public void Commit()
    {
        if(_before==null)return;ConnectorStudio.Resolve(Document);var before=_before;
        if(before.Same(HistorySnapshot.Capture(Document))){_before=null;return;}
        try{BeforeCommitSnapshot?.Invoke(before,Document);BeforeCommit?.Invoke(before.ExpandJson(),Document);}catch{Document=before.Restore();_before=null;throw;}_before=null;
        _redo.Clear();_undo.Add(before);TrimHistory(_undo,_redo);Dirty=true;Revision++;Changed?.Invoke();
    }
    public void Cancel(){if(_before==null)return;Document=_before.Restore();_before=null;}
    public void Change(Action<EditorDocument> change){Begin();try{change(Document);Commit();}catch{Cancel();throw;}}
    public void Undo(){CheckAuthorization();Commit();if(!CanUndo)return;var next=_undo[^1].Restore();_redo.Add(HistorySnapshot.Capture(Document));_undo.RemoveAt(_undo.Count-1);Document=next;TrimHistory(_redo,_undo);Dirty=true;Revision++;Changed?.Invoke();}
    public void Redo(){CheckAuthorization();Commit();if(!CanRedo)return;var next=_redo[^1].Restore();_undo.Add(HistorySnapshot.Capture(Document));_redo.RemoveAt(_redo.Count-1);Document=next;TrimHistory(_undo,_redo);Dirty=true;Revision++;Changed?.Invoke();}
    public void RequireSave(){Dirty=true;Revision++;}
    public void MarkSaved()=>Dirty=false;
    public bool MarkSaved(long revision){if(Revision!=revision)return false;Dirty=false;return true;}
}

// Lossless scan backing: one PNG per scanned region, references for editable pieces.
public static class MockupRasterState
{
    public static int RemoveLegacyScans(EditorDocument doc){var old=doc.Annotations.Where(a=>a.Mockup is {FromScan:true,RasterBacking:false,PreserveRaster:false,RasterOwnerId:null,RasterDetached:false}).ToArray();var recoveries=old.Select(a=>a.Mockup!.RecoveryId).Where(id=>id!=null).ToHashSet();var ids=old.Select(a=>a.Id).ToHashSet();doc.Annotations.RemoveAll(a=>ids.Contains(a.Id));var keep=doc.Annotations.Select(a=>a.Mockup?.RecoveryId).ToHashSet();doc.Recoveries.RemoveAll(p=>recoveries.Contains(p.Id)&&!keep.Contains(p.Id));return old.Length;}
    static PixelRect Box(Annotation a)=>new((int)Math.Round(a.Left),(int)Math.Round(a.Top),(int)Math.Round(a.Width),(int)Math.Round(a.Height));
    public static List<Annotation> Attach(List<Annotation> pieces,PixelRect region,string png){
        if(PixelStudio.PngSize(png)!=(region.Width,region.Height)||(long)region.Width*region.Height>20_000_000)throw new InvalidDataException("Ảnh quét phải giữ nguyên kích thước pixel vùng nguồn.");
        var root=new Annotation{Kind=AnnotationKind.Rectangle,Name="Ảnh nền Mockup · giữ toàn bộ nội dung",X1=region.X,Y1=region.Y,X2=region.Right,Y2=region.Bottom,PixelPng=png,StrokeEnabled=false,Mockup=new(){Type=MockupKind.Block,SourceBounds=region,EraseOriginal=false,FromScan=true,RasterBacking=true,PreserveRaster=true,RasterClears=[]}};
        string group=Guid.NewGuid().ToString("N");root.GroupId=group;
        foreach(var a in pieces){a.Mockup!.RasterOwnerId=root.Id;a.Mockup.PreserveRaster=true;a.Mockup.EraseOriginal=false;GroupLayout.Set(a,new[]{group}.Concat(GroupLayout.Chain(a)));}
        return new[]{root}.Concat(pieces).ToList();
    }
    public static void Validate(EditorDocument doc){var ids=doc.Annotations.ToDictionary(a=>a.Id);foreach(var a in doc.Annotations){if(a.Mockup is not {} m)continue;
        if(m.RasterOwnerId is {} owner&&(!ids.TryGetValue(owner,out var root)||root.Mockup?.RasterBacking!=true||root.PixelPng==null||m.SourceBounds.Intersect(root.Mockup.SourceBounds)!=m.SourceBounds))throw new InvalidDataException("Phần tử quét không còn lớp ảnh nguồn hợp lệ.");
        if(m.RasterBacking&&(a.PixelPng==null||PixelStudio.PngSize(a.PixelPng)!=(m.SourceBounds.Width,m.SourceBounds.Height)))throw new InvalidDataException("Thiếu ảnh nền Mockup nguyên pixel.");
        if(m.RasterImageBounds is {} b&&(b.Width<1||b.Height<1||m.SourceBounds.Intersect(b)!=m.SourceBounds||a.PixelPng==null||PixelStudio.PngSize(a.PixelPng)!=(b.Width,b.Height)))throw new InvalidDataException("Khung ảnh nguồn Mockup không khớp.");
        if(m.RasterClears is {} clears&&(clears.Count>10000||clears.Any(c=>c.Bounds.IsEmpty||c.Bounds.Intersect(m.SourceBounds)!=c.Bounds||c.Background.Length!=7||!c.Background.StartsWith('#')||!c.Background[1..].All(Uri.IsHexDigit))))throw new InvalidDataException("Vùng chỉnh sửa Mockup không hợp lệ.");
    }}
    static bool AppearanceChanged(Annotation a,Annotation b)=>a.Text!=b.Text||a.FontFamily!=b.FontFamily||a.FontSize!=b.FontSize||a.Bold!=b.Bold||a.Italic!=b.Italic||a.Underline!=b.Underline||a.TextColor!=b.TextColor||a.Color!=b.Color||a.FillColor!=b.FillColor||a.Stroke!=b.Stroke||a.StrokeEnabled!=b.StrokeEnabled||a.CornerRadius!=b.CornerRadius||a.TextAlignment!=b.TextAlignment||a.VerticalAlignment!=b.VerticalAlignment||a.TextPadding!=b.TextPadding||a.LineSpacing!=b.LineSpacing||a.Mockup!.Type!=b.Mockup!.Type||a.Mockup.Background!=b.Mockup.Background;
    public static void Reconcile(EditorDocument before,EditorDocument doc,Func<Annotation,EditorDocument,RecoveryPatch> prepare){
        var current=doc.Annotations.ToDictionary(a=>a.Id);
        foreach(var old in before.Annotations.Where(a=>a.Mockup?.RasterBacking==true)){
            current.TryGetValue(old.Id,out var next);
            if(next==null){foreach(var detached in doc.Annotations.Where(a=>a.Mockup?.RasterOwnerId==old.Id&&a.Mockup.RasterDetached)){detached.PixelPng=old.PixelPng;detached.Mockup!.RasterImageBounds=old.Mockup!.SourceBounds;detached.Mockup.RasterOwnerId=null;}doc.Annotations.RemoveAll(a=>a.Mockup?.RasterOwnerId==old.Id);}
            if(!old.Mockup!.RasterDetached&&old.Mockup.RecoveryId==null&&(next==null||Box(old)!=Box(next)||old.Rotation!=next.Rotation||old.Visible!=next.Visible)){
                var bounds=old.Mockup.SourceBounds;var mask=new byte[checked((bounds.Width*bounds.Height+7)/8)];Array.Fill(mask,(byte)255);var patch=new RecoveryPatch{Bounds=bounds,Kind=MockupKind.Block,Background=old.Mockup.Background,Mask=Convert.ToBase64String(mask),NeedsReview=true};doc.Recoveries.Add(patch);if(next?.Mockup is {} m)m.RecoveryId=patch.Id;
            }
        }
        foreach(var old in before.Annotations.Where(a=>a.Mockup is {PreserveRaster:true,RasterBacking:false,RasterOwnerId:null})){var next=doc.Annotations.FirstOrDefault(a=>a.Id==old.Id);if(next!=null&&AppearanceChanged(old,next)){next.Mockup!.PreserveRaster=false;next.Mockup.RasterImageBounds=null;next.PixelPng=null;}}
        current=doc.Annotations.ToDictionary(a=>a.Id);
        foreach(var old in before.Annotations.Where(a=>a.Mockup?.RasterOwnerId!=null)){
            var owner=old.Mockup!.RasterOwnerId!;if(!current.TryGetValue(owner,out var root))continue;current.TryGetValue(old.Id,out var next);
            bool appearance=next!=null&&AppearanceChanged(old,next);if(appearance&&next?.Mockup is {} m)m.PreserveRaster=false;
            bool translatedTogether=false;var previousRoot=before.Annotations.FirstOrDefault(x=>x.Id==owner);if(previousRoot!=null&&next!=null)translatedTogether=Math.Abs((old.Left-previousRoot.Left)/previousRoot.Width-(next.Left-root.Left)/root.Width)<.0001&&Math.Abs((old.Top-previousRoot.Top)/previousRoot.Height-(next.Top-root.Top)/root.Height)<.0001&&Math.Abs(old.Width/previousRoot.Width-next.Width/root.Width)<.0001&&Math.Abs(old.Height/previousRoot.Height-next.Height/root.Height)<.0001;
            bool changed=next==null||!translatedTogether&&(Box(old)!=Box(next)||old.Rotation!=next.Rotation)||old.Visible!=next?.Visible||appearance;
            if(!changed||old.Mockup.RasterDetached)continue;var holes=root.Mockup!.RasterClears??=[];if(!holes.Any(h=>h.ElementId==old.Id))holes.Add(new(old.Id,old.Mockup.SourceBounds,old.Mockup.Background));
        }
    }
    public static void DetachCopies(EditorDocument original,List<Annotation> copies){var copied=copies.Select(a=>a.Id).ToHashSet();foreach(var a in copies){if(a.Mockup is not {} m)continue;if(m.RasterBacking)m.RasterDetached=true;if(m.RasterOwnerId is {} id&&!copied.Contains(id)){var root=original.Annotations.First(x=>x.Id==id);a.PixelPng=root.PixelPng;m.RasterImageBounds=root.Mockup!.SourceBounds;m.RasterOwnerId=null;m.RasterDetached=true;}}}
    public static void Remap(MockupElement m,IReadOnlyDictionary<string,string> ids){if(m.RasterBacking||m.RasterOwnerId is {} original&&!ids.ContainsKey(original))m.RasterDetached=true;if(m.RasterOwnerId is {} owner)m.RasterOwnerId=ids.TryGetValue(owner,out var next)?next:owner;if(m.RasterClears!=null)m.RasterClears=m.RasterClears.Select(h=>h with{ElementId=ids.TryGetValue(h.ElementId,out var id)?id:h.ElementId}).ToList();}
}
