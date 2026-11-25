using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Captovix;

public partial class MainWindow
{
    private readonly Dictionary<VideoMarkKind,VideoMark> _videoToolStyles=[];
    private string? _videoFavoriteKey,_videoCalloutGalleryKey;
    private VideoMark? _videoFavoriteMark;
    private List<StyleFavorite>? _videoShownFavorites;
    private List<string>? _videoShownUtility;
    private string? _videoShownTool;

    private static VideoMarkKind? VideoKindForTool(string? tool)=>
        Enum.TryParse<VideoMarkKind>(tool=="Shape"?"Frame":tool,out var kind)?kind:null;

    private static string VideoToolForKind(VideoMarkKind kind)=>kind==VideoMarkKind.Frame?"Shape":kind.ToString();

    private VideoMark? CurrentVideoInk()=>SelectedVideoTrack?.Mark
        ??(VideoKindForTool(_videoTool) is {} kind?DefaultVideoInk(kind):null);

    private VideoMark DefaultVideoInk(VideoMarkKind kind)
    {
        if(_videoToolStyles.TryGetValue(kind,out var stored))return stored;
        var mark=new VideoMark(kind,0,0,.25,.15,"",kind==VideoMarkKind.Step?1:0);
        if(kind==VideoMarkKind.Title)
            return mark with{Color="#FFFFFF",FontSize=44,TextFrame=true,
                Style=VideoInkStyle.Default with{Bold=true,Alignment=1,Vertical=1,FillEnabled=false}};
        var saved=_favorites.LastOrDefault(item=>VideoMarkRendering.KindFor(item.Style.Kind)==kind);
        if(saved!=null)return VideoMarkRendering.WithImageStyle(mark,saved.Style);
        var annotationKind=VideoMarkRendering.AnnotationFor(mark,960,540).Kind;
        var annotation=(FocusStudio.Supports(annotationKind)?Surface.FocusDefaultsFor(annotationKind):
            TextStudio.IsText(annotationKind)?Surface.TextDefaultsFor(annotationKind):Surface.ShapeDefaults).Copy();
        annotation.Kind=annotationKind;
        annotation.Color=kind==VideoMarkKind.Spotlight?"#000000":Surface.InkColor;
        annotation.Stroke=Surface.StrokeSize;
        annotation.FontSize=Surface.TextSize;
        if(kind==VideoMarkKind.Frame)annotation.Kind=Surface.ShapeKind;
        if(kind==VideoMarkKind.Arrow)annotation.Route=Surface.ArrowRoute;
        return VideoMarkRendering.WithImageStyle(mark,annotation);
    }

    private void SyncVideoFavorites(VideoMark? mark)
    {
        if(!_videoStudio)return;
        if(mark==_videoFavoriteMark&&ReferenceEquals(_videoShownFavorites,_favorites)
            &&ReferenceEquals(_videoShownUtility,_utilityStars)&&_videoShownTool==_videoTool)return;
        _videoFavoriteMark=mark;_videoShownFavorites=_favorites;_videoShownUtility=_utilityStars;_videoShownTool=_videoTool;
        var current=mark==null?null:VideoMarkRendering.AnnotationFor(mark,960,540);
        string? signature=current==null?null:StudioStyle.Signature(current);
        string utility=_videoTool??"Select";
        string listKey=string.Join(",",_favorites.Select(item=>item.Id));
        string key=utility+"|"+signature+"|"+listKey+"|"+string.Join(",",_utilityStars);
        if(_videoFavoriteKey!=key){
            _videoFavoriteKey=key;
            VideoFavoriteTiles.Children.Clear();
            foreach(var saved in _favorites.Where(item=>VideoMarkRendering.KindFor(item.Style.Kind)!=null))
                AddStyleTile(VideoFavoriteTiles,saved.Style,saved.Label,()=>ChooseVideoFavorite(saved.Style));
            foreach(string tool in _utilityStars.Where(tool=>tool is "Crop" or "Select")){
                var button=new Button{Content=AppIcons.Content(tool,EditorToolLabel(tool)),ToolTip=EditorToolLabel(tool)};
                button.Click+=(_,_)=>{ClearVideoTrackSelection();SetVideoTool(tool=="Select"?null:tool);};
                VideoFavoriteTiles.Children.Add(button);
            }
            bool canSave=current!=null?!_favoritesReadOnly:!_utilityStarsReadOnly;
            bool isSaved=signature==null?_utilityStars.Contains(utility):_favorites.Any(item=>item.Id==signature);
            VideoSaveToolStar.IsEnabled=canSave;
            VideoSaveToolStar.Content=AppIcons.Content("Star",isSaved?"Bỏ lưu công cụ này":"Lưu công cụ hiện tại");
        }
        string? galleryKey=mark?.Kind==VideoMarkKind.Callout?signature+"|"+listKey:null;
        if(_videoCalloutGalleryKey==galleryKey)return;
        _videoCalloutGalleryKey=galleryKey;
        VideoCalloutTiles.Children.Clear();
        if(galleryKey==null||current==null)return;
        foreach(var style in Enum.GetValues<CalloutStyle>()){
            var sample=current.Copy();
            sample.CalloutStyle=style;
            AddStyleTile(VideoCalloutTiles,sample,CalloutLabel(style),()=>ChooseVideoFavorite(sample));
        }
    }

    private void VideoSaveToolStar_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||!VideoCanEdit||!_videoStudio)return;
        if(_inlineVideoText!=null&&!CommitVideoInlineText())return;
        try{
            if(CurrentVideoInk() is {} mark){
                if(_favoritesReadOnly)return;
                if(ReadVideoMark(mark,SelectedVideoTrack!=null) is not {} updated){
                    SetStatus("Kiểm tra thông số công cụ trước khi lưu.");return;
                }
                var style=StudioStyle.Sample(VideoMarkRendering.AnnotationFor(updated,960,540));
                string id=StudioStyle.Signature(style);
                var list=_favorites.ToList();
                if(list.RemoveAll(item=>item.Id==id)==0)list.Add(new(id,updated.Label,style));
                StyleFavoriteStore.Save(App.DataRoot,list);
                _favorites=list;
                if(SelectedVideoTrack is {} track&&_timeline!.SetTrack(track with{Mark=updated}))CommitVideoEdit();
                else if(SelectedVideoTrack==null)_videoToolStyles[updated.Kind]=updated;
            }else{
                if(_utilityStarsReadOnly)return;
                string tool=_videoTool??"Select";
                var list=_utilityStars.ToList();
                if(!list.Remove(tool))list.Add(tool);
                MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"tool-stars.json"),JsonSerializer.Serialize(list));
                _utilityStars=list;
            }
            _favoriteTilesKey=null;
            _galleryKey=null;
            RefreshVideoInspector(force:true);
        }catch(Exception error){App.Log(error);SetStatus(error.Message);}
    }

    private void ChooseVideoFavorite(Annotation style)
    {
        if(!RequireLicense()||!VideoCanEdit||_timeline is not {} timeline||VideoMarkRendering.KindFor(style.Kind) is not {} kind)return;
        if(kind==VideoMarkKind.Text&&(SelectedVideoTrack?.Mark.Kind==VideoMarkKind.Title||_videoTool=="Title"))
            kind=VideoMarkKind.Title;
        if(_inlineVideoText!=null&&!CommitVideoInlineText())return;
        PauseVideoForEditing();
        if(SelectedVideoTrack is {} track&&track.Mark.Kind==kind){
            var updated=VideoMarkRendering.WithImageStyle(track.Mark,style);
            if(timeline.SetTrack(track with{Mark=updated}))CommitVideoEdit();
            return;
        }
        _videoToolStyles[kind]=VideoMarkRendering.WithImageStyle(DefaultVideoInk(kind),style);
        ClearVideoTrackSelection();
        SetVideoTool(VideoToolForKind(kind));
    }
}
