using System.IO;
using System.Text.Json;

namespace Captovix;
public sealed record AnnotationPreset(string Name,string Color,double Stroke,double FontSize,int EffectSize=24)
{
    public void Apply(Annotation a){a.Color=Color;a.Stroke=Stroke;a.FontSize=FontSize;a.EffectSize=EffectSize;}
    public void Validate()
    {
        if(string.IsNullOrWhiteSpace(Name)||Name.Length>40)throw new ArgumentException("Tên preset cần từ 1 đến 40 ký tự.");
        new EditorDocument{OriginalWidth=1,OriginalHeight=1,Annotations=[new(){Color=Color,Stroke=Stroke,FontSize=FontSize,EffectSize=EffectSize}]}.Validate(1,1,"");
    }
    public override string ToString()=>Name;
}
public static class AnnotationPresetStore
{
    public static IReadOnlyList<AnnotationPreset> BuiltIn {get;}=[
        new("Hướng dẫn","#008B92",4,28),new("Cảnh báo","#EA5362",6,36),new("Ghi chú","#2E5FA8",2,22),
        new("Nổi bật","#F3C746",6,28),new("Tối giản","#26364B",2,22),new("Chữ lớn","#26364B",4,48)
    ];
    public static List<AnnotationPreset> Load(string root,out string? warning)
    {
        warning=null;var path=Path.Combine(root,"annotation-presets.json");if(!File.Exists(path))return [];
        try{
            var list=JsonSerializer.Deserialize<List<AnnotationPreset>>(File.ReadAllText(path))??throw new InvalidDataException();
            Validate(list);return list;
        }catch(Exception){warning="Chưa đọc được preset riêng. File cũ được giữ; hãy đổi tên file lỗi trước khi lưu mẫu mới.";return [];}
    }
    private static void Validate(IReadOnlyCollection<AnnotationPreset> list)
    {
        if(list.Count>20)throw new ArgumentException("Tối đa 20 preset riêng.");
        var names=new HashSet<string>(BuiltIn.Select(p=>p.Name),StringComparer.OrdinalIgnoreCase);
        foreach(var p in list){if(p==null)throw new InvalidDataException();p.Validate();if(!names.Add(p.Name))throw new ArgumentException("Tên preset đã tồn tại.");}
    }
    public static void Save(string root,IReadOnlyCollection<AnnotationPreset> list)
    {
        Validate(list);Directory.CreateDirectory(root);var path=Path.Combine(root,"annotation-presets.json");var temp=path+".tmp";
        File.WriteAllText(temp,JsonSerializer.Serialize(list,new JsonSerializerOptions{WriteIndented=true}));File.Move(temp,path,true);
    }
}
