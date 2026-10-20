using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Captovix;

public enum AnnotationKind { Arrow, Rectangle, Ellipse, Text, Highlight, Step, Blur, Pixelate, Cover }
public sealed class Annotation
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public AnnotationKind Kind {get;set;}
    public double X1 {get;set;}
    public double Y1 {get;set;}
    public double X2 {get;set;}
    public double Y2 {get;set;}
    public string Color {get;set;}="#008B92";
    public double Stroke {get;set;}=4;
    public double FontSize {get;set;}=28;
    public string Text {get;set;}="";
    public int Number {get;set;}=1;
    public int EffectSize {get;set;}=24;
    [JsonIgnore] public bool IsRedaction=>Kind is AnnotationKind.Blur or AnnotationKind.Pixelate or AnnotationKind.Cover;
    [JsonIgnore] public double Left=>Math.Min(X1,X2);
    [JsonIgnore] public double Top=>Math.Min(Y1,Y2);
    [JsonIgnore] public double Width=>Math.Abs(X2-X1);
    [JsonIgnore] public double Height=>Math.Abs(Y2-Y1);
    public Annotation Copy()=>new(){Id=Id,Kind=Kind,X1=X1,Y1=Y1,X2=X2,Y2=Y2,Color=Color,Stroke=Stroke,FontSize=FontSize,Text=Text,Number=Number,EffectSize=EffectSize};
    public void Translate(double dx,double dy){X1+=dx;X2+=dx;Y1+=dy;Y2+=dy;}
}

public sealed class EditorDocument
{
    public int Version {get;set;}=2;
    public int OriginalWidth {get;set;}
    public int OriginalHeight {get;set;}
    public string SourceHash {get;set;}="";
    public PixelRect? Crop {get;set;}
    public List<Annotation> Annotations {get;set;}=[];
    [JsonIgnore] public PixelRect Viewport=>Crop??new PixelRect(0,0,OriginalWidth,OriginalHeight);
    [JsonIgnore] public bool HasEdits=>Crop!=null||Annotations.Count>0;
    public static readonly JsonSerializerOptions JsonOptions=new(){WriteIndented=true,Converters={new JsonStringEnumConverter()}};
    public string Serialize()=>JsonSerializer.Serialize(this,JsonOptions);
    public static EditorDocument Deserialize(string json)=>JsonSerializer.Deserialize<EditorDocument>(json,JsonOptions)??throw new InvalidDataException("Tài liệu chỉnh sửa trống.");
    public EditorDocument Clone()=>Deserialize(Serialize());
    public void Validate(int width,int height,string hash)
    {
        if(Version is not (1 or 2))throw new InvalidDataException("Phiên bản dữ liệu chỉnh sửa chưa được hỗ trợ.");
        if(OriginalWidth!=width||OriginalHeight!=height||SourceHash!=hash)throw new InvalidDataException("Ảnh gốc đã thay đổi. Bản chú thích cũ được giữ riêng, không tự gán lên ảnh khác.");
        if(Crop is {} c&&(c.IsEmpty||c.X<0||c.Y<0||c.Right>width||c.Bottom>height))throw new InvalidDataException("Vùng crop không hợp lệ.");
        if(Annotations==null||Annotations.Count>10000)throw new InvalidDataException("Số đối tượng vượt giới hạn tài liệu.");
        var ids=new HashSet<string>();
        foreach(var a in Annotations){
            if(a==null||string.IsNullOrEmpty(a.Id)||!ids.Add(a.Id)||!Enum.IsDefined(a.Kind)||!double.IsFinite(a.X1)||!double.IsFinite(a.X2)||!double.IsFinite(a.Y1)||!double.IsFinite(a.Y2)||Math.Abs(a.X1)>1000000||Math.Abs(a.X2)>1000000||Math.Abs(a.Y1)>1000000||Math.Abs(a.Y2)>1000000||!double.IsFinite(a.Stroke)||!double.IsFinite(a.FontSize)||a.Stroke<1||a.Stroke>40||a.FontSize<8||a.FontSize>240||a.EffectSize<4||a.EffectSize>64||a.Text==null||a.Text.Length>20000||a.Number<1||a.Number>9999||!ValidColor(a.Color))
                throw new InvalidDataException("Dữ liệu đối tượng chú thích không hợp lệ.");
        }
        Version=2;
    }
    private static bool ValidColor(string color)=>color is {Length:7}&&color[0]=='#'&&color.AsSpan(1).ToString().All(Uri.IsHexDigit);
    public bool ApplyCrop(PixelRect crop)
    {
        var clipped=crop.Intersect(Viewport);if(clipped.Width<2||clipped.Height<2)return false;
        Crop=clipped==new PixelRect(0,0,OriginalWidth,OriginalHeight)?null:clipped;return true;
    }
    public int NextStep()=>Math.Min(9999,Annotations.Where(a=>a.Kind==AnnotationKind.Step).Select(a=>a.Number).DefaultIfEmpty(0).Max()+1);
}

public sealed class EditorSession
{
    private readonly List<string> _undo=[];
    private readonly List<string> _redo=[];
    private string? _before;
    public EditorDocument Document {get;private set;}
    public bool Dirty {get;private set;}
    public bool CanUndo=>_undo.Count>0;
    public bool CanRedo=>_redo.Count>0;
    public bool InTransaction=>_before!=null;
    public event Action? Changed;
    public EditorSession(EditorDocument document){Document=document;}
    public void Begin(){_before??=Document.Serialize();}
    public void Commit()
    {
        if(_before==null)return;var before=_before;_before=null;
        if(before==Document.Serialize())return;
        _undo.Add(before);if(_undo.Count>100)_undo.RemoveAt(0);_redo.Clear();Dirty=true;Changed?.Invoke();
    }
    public void Cancel(){if(_before==null)return;Document=EditorDocument.Deserialize(_before);_before=null;}
    public void Change(Action<EditorDocument> change){Begin();try{change(Document);Commit();}catch{Cancel();throw;}}
    public void Undo(){Commit();if(!CanUndo)return;_redo.Add(Document.Serialize());Document=EditorDocument.Deserialize(_undo[^1]);_undo.RemoveAt(_undo.Count-1);Dirty=true;Changed?.Invoke();}
    public void Redo(){Commit();if(!CanRedo)return;_undo.Add(Document.Serialize());Document=EditorDocument.Deserialize(_redo[^1]);_redo.RemoveAt(_redo.Count-1);Dirty=true;Changed?.Invoke();}
    public void MarkSaved()=>Dirty=false;
}
