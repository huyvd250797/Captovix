using System.IO;
using System.Text.Json;

namespace Captovix;
public sealed record AnnotationPreset(string Name,string Color,double Stroke,double FontSize,int EffectSize=24,string FillColor="#FFFFFF",bool FillEnabled=false,bool StrokeEnabled=true,double StrokeOpacity=100,double FillOpacity=100,StrokePattern Dash=StrokePattern.Solid,double CornerRadius=16,int StarPoints=5,double StarInset=.45,ArrowTip Tip=ArrowTip.Triangle,bool DoubleArrow=false,double ArrowHeadSize=0,AnnotationKind? Shape=null)
{
    public Annotation ToStyle()=>new(){Color=Color,Stroke=Stroke,FontSize=FontSize,EffectSize=EffectSize,FillColor=FillColor,FillEnabled=FillEnabled,StrokeEnabled=StrokeEnabled,StrokeOpacity=StrokeOpacity,FillOpacity=FillOpacity,Dash=Dash,CornerRadius=CornerRadius,StarPoints=StarPoints,StarInset=StarInset,Tip=Tip,DoubleArrow=DoubleArrow,ArrowHeadSize=ArrowHeadSize};
    public static AnnotationPreset From(string name,Annotation a,AnnotationKind? shape=null)=>new(name,a.Color,a.Stroke,a.FontSize,a.EffectSize,a.FillColor,a.FillEnabled,a.StrokeEnabled,a.StrokeOpacity,a.FillOpacity,a.Dash,a.CornerRadius,a.StarPoints,a.StarInset,a.Tip,a.DoubleArrow,a.ArrowHeadSize,shape);
    public void Apply(Annotation a){a.Color=Color;a.Stroke=Stroke;a.FontSize=FontSize;a.EffectSize=EffectSize;if(ShapeStudio.HasStroke(a.Kind)){ShapeStudio.CopyStyle(ToStyle(),a);if(Shape is {} kind&&ShapeStudio.IsClosed(a.Kind))a.Kind=kind;}}
    public void Validate()
    {
        if(string.IsNullOrWhiteSpace(Name)||Name.Length>40)throw new ArgumentException("Tên preset cần từ 1 đến 40 ký tự.");
        if(Shape is {} kind&&!ShapeStudio.IsClosed(kind))throw new ArgumentException("Preset shape không hợp lệ.");
        new EditorDocument{OriginalWidth=1,OriginalHeight=1,Annotations=[ToStyle()]}.Validate(1,1,"");
    }
    public override string ToString()=>Name;
}
public static class AnnotationPresetStore
{
    public static IReadOnlyList<AnnotationPreset> BuiltIn {get;}=[
        new("Hướng dẫn","#008B92",4,28),new("Cảnh báo","#EA5362",6,36),new("Ghi chú","#2E5FA8",2,22),
        new("Nổi bật","#F3C746",6,28),new("Tối giản","#26364B",2,22),new("Chữ lớn","#26364B",4,48),
        new("Khoanh lỗi đỏ","#D94155",4,28,Dash:StrokePattern.Dash,Shape:AnnotationKind.RoundedRectangle),
        new("Khung ghi chú xanh","#008B92",3,28,FillColor:"#B9F2E6",FillEnabled:true,FillOpacity:35,Shape:AnnotationKind.RoundedRectangle),
        new("Nền nhấn vàng","#B88B00",2,28,FillColor:"#FFE28A",FillEnabled:true,StrokeEnabled:false,FillOpacity:45,Shape:AnnotationKind.Rectangle)
    ];
    public static List<AnnotationPreset> Load(string root,out string? warning)
    {
        warning=null;var path=Path.Combine(root,"annotation-presets.json");if(!File.Exists(path))return [];
        try{
            var list=JsonSerializer.Deserialize<List<AnnotationPreset>>(File.ReadAllText(path),EditorDocument.JsonOptions)??throw new InvalidDataException();
            Validate(list,BuiltIn.Skip(6).Select(p=>p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase));return list;
        }catch(Exception){warning="Chưa đọc được preset riêng. File cũ được giữ; hãy đổi tên file lỗi trước khi lưu mẫu mới.";return [];}
    }
    private static void Validate(IReadOnlyCollection<AnnotationPreset> list,IReadOnlySet<string>? legacyNames=null)
    {
        if(list.Count>20)throw new ArgumentException("Tối đa 20 preset riêng.");
        var names=new HashSet<string>(BuiltIn.Where(p=>legacyNames?.Contains(p.Name)!=true).Select(p=>p.Name),StringComparer.OrdinalIgnoreCase);
        foreach(var p in list){if(p==null)throw new InvalidDataException();p.Validate();if(!names.Add(p.Name))throw new ArgumentException("Tên preset đã tồn tại.");}
    }
    public static void Save(string root,IReadOnlyCollection<AnnotationPreset> list)
    {
        var existing=Load(root,out _).Select(p=>p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allowed=BuiltIn.Skip(6).Where(p=>existing.Contains(p.Name)).Select(p=>p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Validate(list,allowed);Directory.CreateDirectory(root);var path=Path.Combine(root,"annotation-presets.json");var temp=path+".tmp";
        File.WriteAllText(temp,JsonSerializer.Serialize(list,EditorDocument.JsonOptions));File.Move(temp,path,true);
    }
}
