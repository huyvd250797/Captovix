using System.IO;
using System.Text.Json;
namespace Captovix;
public sealed class AnnotationClipboardPacket
{
    public int Version {get;set;}=1;
    public int SourceWidth {get;set;}
    public int SourceHeight {get;set;}
    public List<Annotation> Objects {get;set;}=[];
}
public static class AnnotationClipboard
{
    public const string Format="Captovix.AnnotationSet.v1";
    public const int MaxChars=4_000_000;
    public static string Copy(EditorDocument doc,IEnumerable<string> selection)
    {
        ConnectorStudio.Resolve(doc);var ids=AnnotationEditing.ExpandGroups(doc,selection);var objects=doc.Annotations.Where(a=>ids.Contains(a.Id)).Select(a=>a.Copy()).ToList();
        if(objects.Count==0)throw new InvalidOperationException("Chọn đối tượng để copy.");
        foreach(var a in objects){if(a.StartAnchor is {} start&&!ids.Contains(start.TargetId))a.StartAnchor=null;if(a.EndAnchor is {} end&&!ids.Contains(end.TargetId))a.EndAnchor=null;}
        var packet=new AnnotationClipboardPacket{SourceWidth=doc.OriginalWidth,SourceHeight=doc.OriginalHeight,Objects=objects};var json=JsonSerializer.Serialize(packet,EditorDocument.JsonOptions);Read(json);return json;
    }
    public static AnnotationClipboardPacket Read(string json)
    {
        if(json.Length>MaxChars)throw new InvalidDataException("Dữ liệu copy quá lớn.");
        var packet=JsonSerializer.Deserialize<AnnotationClipboardPacket>(json,EditorDocument.JsonOptions)??throw new InvalidDataException("Clipboard trống.");
        if(packet.Version!=1||packet.SourceWidth<1||packet.SourceHeight<1||packet.Objects==null||packet.Objects.Count==0)throw new InvalidDataException("Clipboard không phải đối tượng Captovix hợp lệ.");
        new EditorDocument{OriginalWidth=packet.SourceWidth,OriginalHeight=packet.SourceHeight,Annotations=packet.Objects}.Validate(packet.SourceWidth,packet.SourceHeight,"");return packet;
    }
    public static HashSet<string> Paste(EditorDocument doc,string json,double offset,Func<Annotation,AnnotationBox> bounds,PixelRect? movementBounds=null)
    {
        if(!double.IsFinite(offset)||Math.Abs(offset)>100000)throw new ArgumentException("Vị trí dán không hợp lệ.");
        var packet=Read(json);if(doc.Annotations.Count+packet.Objects.Count>10000||doc.Annotations.Sum(a=>a.Points.Count)+packet.Objects.Sum(a=>a.Points.Count)>100000)throw new InvalidDataException("Dán vượt giới hạn tài liệu.");
        var objects=packet.Objects;var idMap=objects.ToDictionary(a=>a.Id,_=>Guid.NewGuid().ToString("N"));var groups=new Dictionary<string,string>();
        var box=AnnotationEditing.Union(objects.Select(bounds));var view=movementBounds??doc.Viewport;
        double left=Math.Clamp(box.X+offset,view.X,view.Right-Math.Min(view.Width,box.Width)),top=Math.Clamp(box.Y+offset,view.Y,view.Bottom-Math.Min(view.Height,box.Height));
        foreach(var a in objects){
            a.Id=idMap[a.Id];a.Locked=false;a.Visible=true;
            if(a.GroupId is {} group){if(!groups.TryGetValue(group,out var mapped))groups[group]=mapped=Guid.NewGuid().ToString("N");a.GroupId=mapped;}
            a.StartAnchor=a.StartAnchor is {} start&&idMap.TryGetValue(start.TargetId,out var sid)?start with{TargetId=sid}:null;
            a.EndAnchor=a.EndAnchor is {} end&&idMap.TryGetValue(end.TargetId,out var eid)?end with{TargetId=eid}:null;
            a.Translate(left-box.X,top-box.Y);
        }
        doc.Annotations.AddRange(objects);ConnectorStudio.Resolve(doc);return objects.Select(a=>a.Id).ToHashSet();
    }
}
