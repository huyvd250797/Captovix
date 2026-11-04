namespace Captovix;
public sealed record ShortcutTool(string Command,string Label,uint Modifiers=0,uint Key=0);
public static class ToolShortcutCatalog
{
    public static IReadOnlyList<ShortcutTool> All=>new ShortcutTool[]{new("Tool:Shape","Khung",0,0x52),new("Tool:Callout","Callout",0,0x51),new("Tool:Arrow","Mũi tên",0,0x41),new("Tool:Text","Chữ",0,0x54),new("Tool:Crop","Crop",0,0x43),new("Tool:Blur","Làm mờ",0,0x42),new("Tool:Step","Bước",0,0x4E),new("Pixel:PixelRectangle","Vùng chữ nhật",0,0x50),new("Pixel:PixelEllipse","Vùng ellipse"),new("Pixel:PixelLasso","Vùng lasso"),new("Assist","Nhận chữ")}.Concat(StudioToolCatalog.All.Select(s=>new ShortcutTool("Studio:"+s.Command,s.Label,s.Command=="Mockup"?6u:s.Command=="Canvas"?3u:0u,s.Command switch{"Mockup"=>0x4Du,"Canvas"=>0x45u,"Editor:Select"=>0x56u,"Editor:Magnifier"=>0x4Du,"Editor:Spotlight"=>0x4Fu,"Editor:Highlight"=>0x48u,"Editor:Freehand"=>0x46u,_=>0u}))).ToArray();
    public static List<EditorShortcut> Merge(IEnumerable<EditorShortcut> saved,IEnumerable<CaptureHotkey> capture){var stored=saved.ToDictionary(x=>x.Command);var global=capture.Where(x=>x.Enabled).Select(x=>(x.Modifiers,x.Key)).ToHashSet();return All.Select(t=>stored.TryGetValue(t.Command,out var s)?s:new(t.Command,t.Key!=0&&!global.Contains((t.Modifiers,t.Key)),t.Modifiers,t.Key)).ToList();}
}
