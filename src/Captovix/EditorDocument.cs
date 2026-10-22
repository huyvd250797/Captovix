using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Captovix;

public enum AnnotationKind { Arrow, Rectangle, Ellipse, Text, Highlight, Step, Blur, Pixelate, Cover, Callout, Freehand, RoundedRectangle, Triangle, Diamond, Trapezoid, Pentagon, Hexagon, Star, Line }
public readonly record struct AnnotationPoint(double X,double Y);
public sealed class Annotation
{
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
    public double TailX {get;set;}
    public double TailY {get;set;}
    public List<AnnotationPoint> Points {get;set;}=[];
    public const int MaxPoints=4096;
    [JsonIgnore] public bool IsRedaction=>Kind is AnnotationKind.Blur or AnnotationKind.Pixelate or AnnotationKind.Cover;
    [JsonIgnore] public double Left=>Math.Min(X1,X2);
    [JsonIgnore] public double Top=>Math.Min(Y1,Y2);
    [JsonIgnore] public double Width=>Math.Abs(X2-X1);
    [JsonIgnore] public double Height=>Math.Abs(Y2-Y1);
    public Annotation Copy()=>new(){Route=Route,Control=Control,StartAnchor=StartAnchor,EndAnchor=EndAnchor,Name=Name,Locked=Locked,Visible=Visible,Rotation=Rotation,Id=Id,Kind=Kind,X1=X1,Y1=Y1,X2=X2,Y2=Y2,Color=Color,Stroke=Stroke,FillColor=FillColor,FillEnabled=FillEnabled,StrokeEnabled=StrokeEnabled,StrokeOpacity=StrokeOpacity,FillOpacity=FillOpacity,Dash=Dash,CornerRadius=CornerRadius,StarPoints=StarPoints,StarInset=StarInset,Tip=Tip,DoubleArrow=DoubleArrow,ArrowHeadSize=ArrowHeadSize,FontSize=FontSize,Text=Text,Number=Number,EffectSize=EffectSize,GroupId=GroupId,TailX=TailX,TailY=TailY,Points=[..Points]};
    public void Translate(double dx,double dy){X1+=dx;X2+=dx;Y1+=dy;Y2+=dy;if(Control is {} c)Control=new(c.X+dx,c.Y+dy);if(Kind==AnnotationKind.Callout){TailX+=dx;TailY+=dy;}for(int i=0;i<Points.Count;i++)Points[i]=new(Points[i].X+dx,Points[i].Y+dy);}
    public void AddPoint(double x,double y){if(Points.Count>=MaxPoints)return;Points.Add(new(x,y));if(Points.Count==1){X1=X2=x;Y1=Y2=y;}else{X1=Math.Min(X1,x);Y1=Math.Min(Y1,y);X2=Math.Max(X2,x);Y2=Math.Max(Y2,y);}}
}

public sealed class EditorDocument
{
    public int Version {get;set;}=7;
    public int OriginalWidth {get;set;}
    public int OriginalHeight {get;set;}
    public string SourceHash {get;set;}="";
    public PixelRect? Crop {get;set;}
    public ImageAppearance Appearance {get;set;}=new();
    public List<Annotation> Annotations {get;set;}=[];
    [JsonIgnore] public PixelRect Viewport=>Crop??new PixelRect(0,0,OriginalWidth,OriginalHeight);
    [JsonIgnore] public bool HasEdits=>Crop!=null||Annotations.Count>0||Appearance.HasEdits;
    public static readonly JsonSerializerOptions JsonOptions=new(){WriteIndented=true,Converters={new JsonStringEnumConverter()}};
    public string Serialize()=>JsonSerializer.Serialize(this,JsonOptions);
    public static EditorDocument Deserialize(string json)=>JsonSerializer.Deserialize<EditorDocument>(json,JsonOptions)??throw new InvalidDataException("Tài liệu chỉnh sửa trống.");
    public EditorDocument Clone()=>Deserialize(Serialize());
    public void Validate(int width,int height,string hash)
    {
        if(Version is not (1 or 2 or 3 or 4 or 5 or 6 or 7))throw new InvalidDataException("Phiên bản dữ liệu chỉnh sửa chưa được hỗ trợ.");
        if(OriginalWidth!=width||OriginalHeight!=height||SourceHash!=hash)throw new InvalidDataException("Ảnh gốc đã thay đổi. Bản chú thích cũ được giữ riêng, không tự gán lên ảnh khác.");
        if(Crop is {} c&&(c.IsEmpty||c.X<0||c.Y<0||c.Right>width||c.Bottom>height))throw new InvalidDataException("Vùng crop không hợp lệ.");
        if(Annotations==null||Annotations.Count>10000)throw new InvalidDataException("Số đối tượng vượt giới hạn tài liệu.");
        var ids=new HashSet<string>();var totalPoints=0;
        foreach(var a in Annotations){
            if(a==null||string.IsNullOrEmpty(a.Id)||!ids.Add(a.Id)||!Enum.IsDefined(a.Kind)||!double.IsFinite(a.X1)||!double.IsFinite(a.X2)||!double.IsFinite(a.Y1)||!double.IsFinite(a.Y2)||Math.Abs(a.X1)>1000000||Math.Abs(a.X2)>1000000||Math.Abs(a.Y1)>1000000||Math.Abs(a.Y2)>1000000||!double.IsFinite(a.Stroke)||!double.IsFinite(a.FontSize)||a.Stroke<1||a.Stroke>40||a.FontSize<8||a.FontSize>240||a.EffectSize<4||a.EffectSize>64||a.Text==null||a.Text.Length>20000||a.Number<1||a.Number>9999||!ValidColor(a.Color))
                throw new InvalidDataException("Dữ liệu đối tượng chú thích không hợp lệ.");
            if(a.Name==null||a.Name.Length>80||a.Name.Any(char.IsControl)||!double.IsFinite(a.Rotation)||Math.Abs(a.Rotation)>360)throw new InvalidDataException("Tên layer hoặc góc xoay không hợp lệ.");
            ShapeStudio.ValidateStyle(a);
            if(a.GroupId!=null&&!Guid.TryParseExact(a.GroupId,"N",out _))throw new InvalidDataException("Mã nhóm chú thích không hợp lệ.");
            if(!double.IsFinite(a.TailX)||!double.IsFinite(a.TailY)||Math.Abs(a.TailX)>1000000||Math.Abs(a.TailY)>1000000)throw new InvalidDataException("Đuôi callout không hợp lệ.");
            if(a.Points==null||a.Points.Count>Annotation.MaxPoints||(totalPoints+=a.Points.Count)>100000||a.Points.Any(p=>!double.IsFinite(p.X)||!double.IsFinite(p.Y)||Math.Abs(p.X)>1000000||Math.Abs(p.Y)>1000000))throw new InvalidDataException("Nét vẽ vượt giới hạn hoặc không hợp lệ.");
            if(a.Kind==AnnotationKind.Freehand&&a.Points.Count==0||a.Kind!=AnnotationKind.Freehand&&a.Points.Count!=0)throw new InvalidDataException("Dữ liệu điểm không phù hợp công cụ.");
            if(a.Kind==AnnotationKind.Callout&&(a.Width<36||a.Height<36))throw new InvalidDataException("Hộp callout quá nhỏ.");
        }
        var targets=Annotations.ToDictionary(a=>a.Id);foreach(var a in Annotations)ConnectorStudio.Validate(a,targets);
        ConnectorStudio.Resolve(this);
        if(Appearance==null)throw new InvalidDataException("Thiếu cấu hình ảnh.");
        _=new ImageLayout(Viewport,Appearance);
        Version=7;
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
    private readonly List<string> _undo=[];
    private readonly List<string> _redo=[];
    private string? _before;
    private readonly int _historyCharBudget;
    public EditorDocument Document {get;private set;}
    public bool Dirty {get;private set;}
    public bool CanUndo=>_undo.Count>0;
    public bool CanRedo=>_redo.Count>0;
    public bool InTransaction=>_before!=null;
    public event Action? Changed;
    public EditorSession(EditorDocument document,int historyCharBudget=8_000_000){Document=document;_historyCharBudget=Math.Max(1024,historyCharBudget);}
    private void TrimHistory(List<string> entries){long chars=entries.Sum(s=>(long)s.Length);while(entries.Count>1&&(entries.Count>100||chars>_historyCharBudget)){chars-=entries[0].Length;entries.RemoveAt(0);}}
    public void Begin(){ConnectorStudio.Resolve(Document);_before??=Document.Serialize();}
    public void Commit()
    {
        if(_before==null)return;ConnectorStudio.Resolve(Document);var before=_before;_before=null;
        if(before==Document.Serialize())return;
        _undo.Add(before);TrimHistory(_undo);_redo.Clear();Dirty=true;Changed?.Invoke();
    }
    public void Cancel(){if(_before==null)return;Document=EditorDocument.Deserialize(_before);_before=null;}
    public void Change(Action<EditorDocument> change){Begin();try{change(Document);Commit();}catch{Cancel();throw;}}
    public void Undo(){Commit();if(!CanUndo)return;_redo.Add(Document.Serialize());TrimHistory(_redo);Document=EditorDocument.Deserialize(_undo[^1]);_undo.RemoveAt(_undo.Count-1);Dirty=true;Changed?.Invoke();}
    public void Redo(){Commit();if(!CanRedo)return;_undo.Add(Document.Serialize());TrimHistory(_undo);Document=EditorDocument.Deserialize(_redo[^1]);_redo.RemoveAt(_redo.Count-1);Dirty=true;Changed?.Invoke();}
    public void MarkSaved()=>Dirty=false;
}
