using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace Captovix;
public sealed record ObjectShadow
{
    public bool Enabled {get;set;}
    public int Direction {get;set;}=7;
    public double Distance {get;set;}=8;
    public double Blur {get;set;}=8;
    public double Opacity {get;set;}=25;
    public string Color {get;set;}="#000000";
    public ObjectShadow Copy()=>new(){Enabled=Enabled,Direction=Direction,Distance=Distance,Blur=Blur,Opacity=Opacity,Color=Color};
}
public static class StudioStyle
{
    public static bool Hex(string? color)=>color is {Length:7}&&color[0]=='#'&&color.AsSpan(1).ToString().All(Uri.IsHexDigit);
    public static bool SupportsShadow(AnnotationKind kind)=>ShapeStudio.IsClosed(kind)||kind==AnnotationKind.Callout;
    public static (double X,double Y) Offset(int direction,double distance)
    {
        if(direction<0||direction>8||!double.IsFinite(distance)||distance<0||distance>80)throw new ArgumentException("Hướng bóng cần thuộc 9 ô và khoảng cách 0–80 px.");
        int x=direction%3-1,y=direction/3-1;return(x*distance,y*distance);
    }
    public static void Validate(Annotation a)
    {
        var s=a.Shadow;if(s==null||s.Direction<0||s.Direction>8||!double.IsFinite(s.Distance)||s.Distance<0||s.Distance>80||!double.IsFinite(s.Blur)||s.Blur<0||s.Blur>40||!double.IsFinite(s.Opacity)||s.Opacity<0||s.Opacity>100||!Hex(s.Color))throw new InvalidDataException("Thuộc tính bóng đổ không hợp lệ.");
    }
    public static Annotation Sample(Annotation style)
    {
        var a=style.Copy();a.Id="style";a.Name="";a.GroupId=null;a.Locked=false;a.Visible=true;a.Rotation=0;a.X1=10;a.Y1=15;a.X2=90;a.Y2=60;a.Text="Aa";a.Number=1;a.StartAnchor=a.EndAnchor=null;a.Control=null;a.TailX=30;a.TailY=75;a.Points=[];
        return a;
    }
    public static string Signature(Annotation style)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Sample(style),EditorDocument.JsonOptions))));
}
public sealed record StyleFavorite(string Id,string Label,Annotation Style);
public static class StyleFavoriteStore
{
    public static string PathFor(string root)=>Path.Combine(root,"style-favorites.json");
    public static List<StyleFavorite> Load(string root,out string? warning)
    {
        warning=null;if(!File.Exists(PathFor(root)))return [];
        try{var values=JsonSerializer.Deserialize<List<StyleFavorite>>(File.ReadAllText(PathFor(root)),EditorDocument.JsonOptions)??throw new InvalidDataException();Validate(values);return values;}catch{warning="Chưa đọc được mẫu sao. File cũ được giữ; không ghi đè mẫu lỗi.";return [];}
    }
    public static void Validate(IReadOnlyCollection<StyleFavorite> values)
    {
        if(values.Count>64||values.Any(v=>v==null||string.IsNullOrWhiteSpace(v.Label)||v.Label.Length>100||v.Style==null||v.Id!=StudioStyle.Signature(v.Style)||!ShapeStudio.IsClosed(v.Style.Kind)&&!ShapeStudio.IsLine(v.Style.Kind)))throw new InvalidDataException("Danh sách mẫu sao không hợp lệ.");
        if(values.Select(v=>v.Id).Distinct().Count()!=values.Count)throw new InvalidDataException("Mẫu sao bị trùng.");
        foreach(var v in values)new EditorDocument{OriginalWidth=200,OriginalHeight=200,Annotations=[v.Style.Copy()]}.Validate(200,200,"");
    }
    public static void Save(string root,List<StyleFavorite> values)
    {
        Validate(values);Directory.CreateDirectory(root);var path=PathFor(root);if(File.Exists(path)&&!File.Exists(path+".pre-v1.11.1.bak"))File.Copy(path,path+".pre-v1.11.1.bak");
        var temp=path+".tmp";File.WriteAllText(temp,JsonSerializer.Serialize(values,EditorDocument.JsonOptions));File.Move(temp,path,true);
    }
}
