using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
namespace Captovix;
public partial class MainWindow
{
    private List<string> _utilityStars=[];
    private string? _favoriteKey;
    private bool _utilityStarsReadOnly;
    private void LoadUtilityStars(){try{string p=Path.Combine(App.DataRoot,"tool-stars.json");if(File.Exists(p)){var list=JsonSerializer.Deserialize<List<string>>(File.ReadAllText(p))??throw new InvalidDataException();if(list.Count>16||list.Any(t=>!Enum.TryParse<EditorTool>(t,out _))||list.Distinct().Count()!=list.Count)throw new InvalidDataException();_utilityStars=list;}}catch{_utilityStarsReadOnly=true;VisualStudioError.Text="Chưa đọc được công cụ đã lưu; giữ file cũ.";}}
    private bool UtilityTool=>Surface.Tool is EditorTool.Select or EditorTool.Crop or EditorTool.PixelRectangle or EditorTool.PixelEllipse or EditorTool.PixelLasso;
    private void SaveToolStar_Click(object sender,RoutedEventArgs e)
    {
        FlushAutoProperties();try{
            if(UtilityTool&&Surface.Selected==null){if(_utilityStarsReadOnly)return;var list=_utilityStars.ToList();string tool=Surface.Tool.ToString();if(!list.Remove(tool))list.Add(tool);MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"tool-stars.json"),JsonSerializer.Serialize(list));_utilityStars=list;}
            else{if(_favoritesReadOnly)return;var a=StudioStyle.Sample(StudioTarget());if(a.Kind==AnnotationKind.ImageLayer){SetStatus("Lớp ảnh được lưu trong dự án, không đưa dữ liệu ảnh vào mẫu công cụ.");return;}string id=StudioStyle.Signature(a);var list=_favorites.ToList();if(list.Any(f=>f.Id==id))list.RemoveAll(f=>f.Id==id);else list.Add(new(id,a.Kind.ToString(),a));StyleFavoriteStore.Save(App.DataRoot,list);_favorites=list;}
            _galleryKey=null;SyncProperties();
        }catch(Exception ex){SetStatus(ex.Message);}
    }
    private void SyncAllFavorites(Annotation current)
    {
        string? signature=current.Kind==AnnotationKind.ImageLayer?null:StudioStyle.Signature(current);string key=Surface.Tool+"|"+(signature??"ImageLayer")+"|"+string.Join(",",_favorites.Select(f=>f.Id))+"|"+string.Join(",",_utilityStars);if(key==_favoriteKey)return;_favoriteKey=key;
        FavoriteTileGrid.Children.Clear();
        // Keep favorites above properties, filtered to the active tool family.
        bool browse=Surface.Tool==EditorTool.Select&&Surface.Selected==null;
        foreach(var f in _favorites.Where(f=>browse||f.Style.Kind==current.Kind||CompositionStudio.SameFamily(current.Kind,f.Style.Kind)))AddStyleTile(FavoriteTileGrid,f.Style,f.Label,()=>ChooseStyle(f.Style));
        foreach(var name in _utilityStars.Where(t=>browse||t==Surface.Tool.ToString()).ToArray()){var tile=new Grid{Width=64,Height=38};var pick=new Button{Content=AppIcons.Content(name,""),ToolTip=name};pick.Click+=(_,_)=>SelectTool(Enum.Parse<EditorTool>(name));tile.Children.Add(pick);var remove=new Button{Content="★",Padding=new(2),HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top,ToolTip="Bỏ lưu",IsEnabled=!_utilityStarsReadOnly};remove.Click+=(_,_)=>{try{var list=_utilityStars.Where(t=>t!=name).ToList();MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"tool-stars.json"),JsonSerializer.Serialize(list));_utilityStars=list;SyncProperties();}catch(Exception ex){SetStatus(ex.Message);}};tile.Children.Add(remove);FavoriteTileGrid.Children.Add(tile);}
        SaveToolStar.IsEnabled=Surface.Selected?.Kind!=AnnotationKind.ImageLayer;
        bool saved=UtilityTool&&Surface.Selected==null?_utilityStars.Contains(Surface.Tool.ToString()):signature!=null&&_favorites.Any(f=>f.Id==signature);
        SaveToolStar.Content=AppIcons.Content("Star",saved?"Bỏ lưu công cụ này":"Lưu công cụ hiện tại");
    }
    private void ChooseOtherFavorite(Annotation style)
    {
        var next=style.Copy();if(!Enum.TryParse<EditorTool>(next.Kind.ToString(),out var tool))return;
        var ids=Surface.SelectedIds.ToArray();string? primary=Surface.SelectedId;bool retain=Surface.Selected?.Kind==next.Kind;
        var preset=AnnotationPreset.From("Mẫu đã lưu",next);preset.Apply(FocusStudio.Supports(next.Kind)?Surface.FocusDefaultsFor(next.Kind):TextStudio.IsText(next.Kind)?Surface.TextDefaultsFor(next.Kind):Surface.ShapeDefaults);
        Surface.InkColor=next.Color;Surface.StrokeSize=next.Stroke;Surface.TextSize=next.FontSize;Surface.EffectSize=next.EffectSize;
        if(retain)ChangeSelected(a=>{if(a.Kind==next.Kind)preset.Apply(a);});SelectTool(tool);if(retain)Surface.SelectIds(ids,primary);Surface.Refresh();SyncProperties();
    }
}
