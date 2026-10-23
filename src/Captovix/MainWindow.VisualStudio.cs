using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
namespace Captovix;
public partial class MainWindow
{
    private List<StyleFavorite> _favorites=[];
    private bool _favoritesReadOnly;
    private int _shadowDirection=7,_lastFrameBorder=2;
    private double _frameShadowDistance=12;
    private string? _galleryKey;
    private Popup? _paintPopup;
    private static readonly string[] DirectionArrows=["↖","↑","↗","←","●","→","↙","↓","↘"];
    private void InitializeVisualStudio()
    {
        _favorites=StyleFavoriteStore.Load(App.DataRoot,out var warning);_favoritesReadOnly=warning!=null;if(warning!=null)VisualStudioError.Text=warning;
        void Directions(UniformGrid grid,bool frame){for(int n=0;n<9;n++){int direction=n;var button=new Button{Content=DirectionArrows[n],Tag=n,Padding=new(5),Margin=new(2),ToolTip=frame?"Hướng bóng toàn ảnh":"Hướng bóng đối tượng"};button.Click+=(_,_)=>{
            FlushAutoProperties();if(frame){var offset=StudioStyle.Offset(direction,_frameShadowDistance);ApplyAppearance(a=>{a.Shadow=true;a.ShadowX=(int)offset.X;a.ShadowY=(int)offset.Y;});}
            else{_shadowDirection=direction;bool old=_syncProperties;_syncProperties=true;ObjectShadowEnabled.IsChecked=true;_syncProperties=old;ApplyObjectShadow();}
        };grid.Children.Add(button);}}
        Directions(ObjectShadowDirections,false);Directions(FrameShadowDirections,true);
    }
    private Annotation StudioTarget()
    {
        if(Surface.Selected is {} selected){var copy=selected.Copy();if(copy.Kind==AnnotationKind.Line){copy.Kind=AnnotationKind.Arrow;copy.Tip=ArrowTip.None;}return copy;}
        var kind=Surface.Tool switch{EditorTool.Shape=>Surface.ShapeKind,EditorTool.Rectangle=>AnnotationKind.Rectangle,EditorTool.Ellipse=>AnnotationKind.Ellipse,EditorTool.Arrow=>AnnotationKind.Arrow,EditorTool.Line=>AnnotationKind.Line,EditorTool.Callout=>AnnotationKind.Callout,EditorTool.Text=>AnnotationKind.Text,_=>AnnotationKind.Freehand};
        var a=(TextStudio.IsText(kind)?Surface.TextDefaultsFor(kind):Surface.ShapeDefaults).Copy();a.Kind=kind;a.Color=Surface.InkColor;a.Stroke=Surface.StrokeSize;a.FontSize=Surface.TextSize;a.Route=Surface.ArrowRoute;return a;
    }
    private void SyncVisualStudio()
    {
        var a=StudioTarget();bool active=ShapeStudio.IsClosed(a.Kind)||ShapeStudio.IsLine(a.Kind)||a.Kind is AnnotationKind.Callout or AnnotationKind.Text;
        VisualStudioPanel.Visibility=active?Visibility.Visible:Visibility.Collapsed;
        PaintButton(FrameStrokePaint,"Viền ảnh",_session?.Document.Appearance is {BorderWidth:>0} frame?frame.BorderColor:null);PaintButton(FrameFillPaint,"Nền ảnh",_session?.Document.Appearance is {Transparent:false} fill?fill.BackgroundColor:null);
        if(_session?.Document.Appearance is {} appearance){if(appearance.BorderWidth>0)_lastFrameBorder=appearance.BorderWidth;if(appearance.ShadowX!=0||appearance.ShadowY!=0)_frameShadowDistance=Math.Max(Math.Abs(appearance.ShadowX),Math.Abs(appearance.ShadowY));int dir=(Math.Sign(appearance.ShadowY)+1)*3+Math.Sign(appearance.ShadowX)+1;MarkDirection(FrameShadowDirections,appearance.Shadow,dir);}
        if(!active)return;bool text=a.Kind==AnnotationKind.Text;
        PaintButton(StrokePaintButton,text?"Chữ":"Viền",text?TextStudio.Foreground(a):a.StrokeEnabled?a.Color:null);PaintButton(FillPaintButton,"Nền",a.FillEnabled?a.FillColor:null);FillPaintButton.IsEnabled=ShapeStudio.IsClosed(a.Kind)||a.Kind is AnnotationKind.Text or AnnotationKind.Callout;
        ObjectShadowPanel.Visibility=StudioStyle.SupportsShadow(a.Kind)?Visibility.Visible:Visibility.Collapsed;_shadowDirection=a.Shadow.Direction;ObjectShadowEnabled.IsChecked=a.Shadow.Enabled;ObjectShadowDistance.Text=ShapeNumber(a.Shadow.Distance);ObjectShadowBlur.Text=ShapeNumber(a.Shadow.Blur);ObjectShadowOpacity.Text=ShapeNumber(a.Shadow.Opacity);MarkDirection(ObjectShadowDirections,a.Shadow.Enabled,a.Shadow.Direction);
        string key=StudioStyle.Signature(a)+"|"+string.Join(",",_favorites.Select(f=>f.Id));if(key==_galleryKey)return;_galleryKey=key;ShapeTileGrid.Children.Clear();VariantTileGrid.Children.Clear();FavoriteTileGrid.Children.Clear();CalloutTileGrid.Children.Clear();
        if(a.Kind==AnnotationKind.Callout)foreach(var callout in Enum.GetValues<CalloutStyle>()){var preview=a.Copy();preview.CalloutStyle=callout;AddStyleTile(CalloutTileGrid,preview,CalloutLabel(callout),()=>ChooseStyle(preview));}
        if(ShapeStudio.IsClosed(a.Kind))foreach(var kind in ShapeStudio.Shapes){var preview=a.Copy();preview.Kind=kind;AddStyleTile(ShapeTileGrid,preview,ShapeStudio.Label(kind),()=>ChooseStyle(preview));}
        if(ShapeStudio.IsClosed(a.Kind)||ShapeStudio.IsLine(a.Kind))foreach(var dash in Enum.GetValues<StrokePattern>()){var preview=a.Copy();preview.Dash=dash;AddStyleTile(VariantTileGrid,preview,dash switch{StrokePattern.Solid=>"Liền",StrokePattern.Dash=>"Nét đứt",StrokePattern.Dot=>"Chấm",_=>"Đứt chấm"},()=>ChooseStyle(preview));}
        if(ShapeStudio.IsLine(a.Kind)){
            foreach(var route in Enum.GetValues<ConnectorRoute>()){var preview=a.Copy();preview.Route=route;AddStyleTile(VariantTileGrid,preview,route switch{ConnectorRoute.Straight=>"Thẳng",ConnectorRoute.Elbow=>"Gấp khúc",_=>"Cong"},()=>ChooseStyle(preview));}
            if(a.Kind==AnnotationKind.Arrow)foreach(var tip in Enum.GetValues<ArrowTip>()){var preview=a.Copy();preview.Tip=tip;AddStyleTile(VariantTileGrid,preview,tip switch{ArrowTip.None=>"Không đầu · đường",ArrowTip.Open=>"Đầu mở",ArrowTip.Diamond=>"Đầu thoi",ArrowTip.Circle=>"Đầu tròn",_=>"Tam giác"},()=>ChooseStyle(preview));}
        }
        foreach(var favorite in _favorites.Where(f=>a.Kind==AnnotationKind.Callout?f.Style.Kind==AnnotationKind.Callout:ShapeStudio.IsClosed(a.Kind)?ShapeStudio.IsClosed(f.Style.Kind):ShapeStudio.IsLine(a.Kind)&&ShapeStudio.IsLine(f.Style.Kind)))AddStyleTile(FavoriteTileGrid,favorite.Style,favorite.Label,()=>ChooseStyle(favorite.Style));
        if(ShapeTileGrid.Parent is Expander shapeGroup)shapeGroup.Visibility=ShapeTileGrid.Children.Count>0?Visibility.Visible:Visibility.Collapsed;
        if(VariantTileGrid.Parent is Expander variantGroup)variantGroup.Visibility=VariantTileGrid.Children.Count>0?Visibility.Visible:Visibility.Collapsed;
        if(!_favorites.Any(f=>CompositionStudio.SameFamily(a.Kind,f.Style.Kind)))FavoriteTileGrid.Children.Add(new TextBlock{Text="Bấm ☆ ngay trên mẫu để lưu.",TextWrapping=TextWrapping.Wrap,FontSize=11,Foreground=Brushes.SlateGray});
    }
    private static void MarkDirection(UniformGrid grid,bool enabled,int direction){foreach(var b in grid.Children.OfType<Button>()){b.SetResourceReference(Control.BackgroundProperty,enabled&&(int)b.Tag==direction?"Soft":"Surface");b.SetResourceReference(Control.BorderBrushProperty,enabled&&(int)b.Tag==direction?"AccentText":"Line");}}
    private void AddStyleTile(Panel host,Annotation style,string label,Action choose)
    {
        var snapshot=StudioStyle.Sample(style);var preview=snapshot.Copy();preview.Text=preview.Kind==AnnotationKind.Callout?"":"Aa";preview.FontSize=18;preview.TextPadding=4;preview.TextWidth=0;preview.LineSpacing=0;preview.VerticalAlignment=TextVertical.Center;preview.TextAlignment=TextAlign.Center;preview.Shadow.Enabled=false;preview.Stroke=Math.Min(3,preview.Stroke);var drawing=new DrawingGroup();using(var dc=drawing.Open()){dc.DrawRectangle(Brushes.Transparent,null,new Rect(0,0,108,84));dc.PushClip(new RectangleGeometry(new Rect(0,0,108,84)));EditorRenderer.DrawAnnotation(dc,preview);dc.Pop();}drawing.Freeze();
        var grid=new Grid{Width=99,Margin=new(2)};var content=new StackPanel();content.Children.Add(new Image{Source=new DrawingImage(drawing),Height=53,Stretch=Stretch.Uniform});content.Children.Add(new TextBlock{Text=label,FontSize=10,TextWrapping=TextWrapping.Wrap,TextAlignment=TextAlignment.Center});
        var select=new Button{Content=content,Padding=new(1),Margin=new(0),ToolTip=label};select.Click+=(_,_)=>{FlushAutoProperties();choose();};grid.Children.Add(select);
        var id=StudioStyle.Signature(snapshot);var star=new Button{Content=_favorites.Any(f=>f.Id==id)?"★":"☆",HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top,Padding=new(2,0,2,0),Margin=new(0),FontSize=15,ToolTip="Lưu / bỏ mẫu yêu thích",IsEnabled=!_favoritesReadOnly};star.Click+=(_,e)=>{try{var list=_favorites.ToList();if(list.Any(f=>f.Id==id))list.RemoveAll(f=>f.Id==id);else list.Add(new(id,label,snapshot.Copy()));StyleFavoriteStore.Save(App.DataRoot,list);_favorites=list;_galleryKey=null;SyncProperties();}catch(Exception ex){VisualStudioError.Text=ex.Message;}e.Handled=true;};grid.Children.Add(star);host.Children.Add(grid);
    }
    private static string CalloutLabel(CalloutStyle s)=>s switch{CalloutStyle.Speech=>"Hộp có đuôi",CalloutStyle.RoundedSpeech=>"Bo góc có đuôi",CalloutStyle.EllipseSpeech=>"Bầu dục có đuôi",CalloutStyle.Rectangle=>"Hộp",CalloutStyle.RoundedRectangle=>"Hộp bo góc",CalloutStyle.PillSpeech=>"Viên thuốc có đuôi",CalloutStyle.Pill=>"Viên thuốc",CalloutStyle.Ellipse=>"Bầu dục",CalloutStyle.DiamondSpeech=>"Thoi có đuôi",_=>"Lục giác có đuôi"};
    private void ChooseStyle(Annotation style)
    {
        var next=style.Copy();Surface.ShapeDefaults=next.Copy();Surface.ShapeKind=ShapeStudio.IsClosed(next.Kind)?next.Kind:Surface.ShapeKind;Surface.InkColor=next.Color;Surface.StrokeSize=next.Stroke;Surface.ArrowRoute=next.Route;
        bool Compatible(Annotation a)=>(ShapeStudio.IsClosed(a.Kind)&&ShapeStudio.IsClosed(next.Kind))||(ShapeStudio.IsLine(a.Kind)&&ShapeStudio.IsLine(next.Kind))||(a.Kind==AnnotationKind.Callout&&next.Kind==AnnotationKind.Callout);
        var ids=Surface.SelectedIds.ToArray();var primary=Surface.Selected?.Id;
        bool retain=ids.Length>0&&_session!=null&&_session.Document.Annotations.Where(a=>ids.Contains(a.Id)).All(Compatible);
        if(next.Kind==AnnotationKind.Callout){ShapeStudio.CopyStyle(next,Surface.CalloutDefaults);TextStudio.CopyStyle(next,Surface.CalloutDefaults);}
        if(retain)ChangeSelected(a=>{if(!Compatible(a))return;if(a.Kind==AnnotationKind.Callout){TextStudio.CopyStyle(next,a);ShapeStudio.CopyStyle(next,a);a.Color=next.Color;a.Stroke=next.Stroke;return;}a.Kind=next.Kind;if(ShapeStudio.IsLine(a.Kind)){a.Route=next.Route;a.Control=null;}a.Color=next.Color;a.Stroke=next.Stroke;ShapeStudio.CopyStyle(next,a);});
        SelectTool(next.Kind==AnnotationKind.Callout?EditorTool.Callout:ShapeStudio.IsClosed(next.Kind)?EditorTool.Shape:EditorTool.Arrow);
        if(retain)Surface.SelectIds(ids,primary);
        Surface.Refresh();SyncProperties();
    }
    private void ApplyObjectShadow()
    {
        try{var shadow=new ObjectShadow{Enabled=ObjectShadowEnabled.IsChecked==true,Direction=_shadowDirection,Distance=ShapeValue(ObjectShadowDistance,"Khoảng cách bóng",0,80),Blur=ShapeValue(ObjectShadowBlur,"Độ mềm",0,40),Opacity=ShapeValue(ObjectShadowOpacity,"Độ đậm bóng",0,100)};StudioStyle.Validate(new(){Shadow=shadow});var kind=StudioTarget().Kind;if(kind==AnnotationKind.Callout)Surface.CalloutDefaults.Shadow=shadow.Copy();else Surface.ShapeDefaults.Shadow=shadow.Copy();ChangeSelected(a=>{if(StudioStyle.SupportsShadow(a.Kind))a.Shadow=shadow.Copy();});Surface.Refresh();SyncProperties();}catch(Exception ex){VisualStudioError.Text=ex.Message;}
    }
    private void StrokePaint_Click(object sender,RoutedEventArgs e)=>OpenPaint(StrokePaintButton,color=>SetObjectPaint(color,false),StudioTarget().Kind!=AnnotationKind.Text);
    private void FillPaint_Click(object sender,RoutedEventArgs e)=>OpenPaint(FillPaintButton,color=>SetObjectPaint(color,true));
    private void SetObjectPaint(string? color,bool fill)
    {
        FlushAutoProperties();var kind=StudioTarget().Kind;
        void Apply(Annotation a){if(fill){a.FillEnabled=color!=null;if(color!=null)a.FillColor=color;}else if(a.Kind==AnnotationKind.Text){if(color!=null)a.TextColor=color;}else{a.StrokeEnabled=color!=null;if(color!=null)a.Color=color;}}
        var defaults=TextStudio.IsText(kind)?Surface.TextDefaultsFor(kind):Surface.ShapeDefaults;Apply(defaults);if(!fill&&color!=null)Surface.InkColor=color;ChangeSelected(Apply);Surface.Refresh();SyncProperties();
    }
    private void FrameStrokePaint_Click(object sender,RoutedEventArgs e)=>OpenPaint(FrameStrokePaint,color=>ApplyAppearance(a=>{a.BorderWidth=color==null?0:a.BorderWidth>0?a.BorderWidth:_lastFrameBorder;if(color!=null)a.BorderColor=color;}));
    private void FrameFillPaint_Click(object sender,RoutedEventArgs e)=>OpenPaint(FrameFillPaint,color=>ApplyAppearance(a=>{a.Transparent=color==null;if(color!=null)a.BackgroundColor=color;}));
    private void PaintButton(Button button,string label,string? color)
    {
        var row=new StackPanel();var patch=new Border{Height=17,Width=52,BorderBrush=Brushes.SlateGray,BorderThickness=new(1),Background=color==null?(Brush)FindResource("TransparencyGrid"):EditorRenderer.Ink(color),Margin=new(0,1,0,3)};row.Children.Add(patch);row.Children.Add(new TextBlock{Text=label+" · "+(color??"Trong suốt"),FontSize=10,TextWrapping=TextWrapping.Wrap,TextAlignment=TextAlignment.Center});button.Content=row;
    }
    private void OpenPaint(Button anchor,Action<string?> selected,bool transparent=true)
    {
        FlushAutoProperties();if(_paintPopup!=null)_paintPopup.IsOpen=false;var popup=new Popup{PlacementTarget=anchor,Placement=PlacementMode.Bottom,StaysOpen=false,AllowsTransparency=true};_paintPopup=popup;
        var stack=new StackPanel{Width=216,Margin=new(9)};if(transparent){var clear=new Button{Content="▧ Trong suốt",ToolTip="Không viền / không tô nền"};clear.Click+=(_,_)=>{selected(null);popup.IsOpen=false;};stack.Children.Add(clear);}
        var swatches=new UniformGrid{Columns=6};foreach(var c in new[]{"#000000","#FFFFFF","#26364B","#64748B","#008B92","#16A34A","#D94155","#EA5362","#F97316","#F3C746","#2E5FA8","#7C3AED","#B9F2E6","#D4E5FC","#FFE28A","#FFD3DC","#F4F7FA","#101828"}){string color=c;var b=new Button{Width=28,Height=27,Margin=new(2),Padding=new(0),Background=EditorRenderer.Ink(color),ToolTip=color};b.Click+=(_,_)=>{selected(color);popup.IsOpen=false;};swatches.Children.Add(b);}stack.Children.Add(swatches);var more=new Button{Content="Chọn màu khác…"};more.Click+=(_,_)=>{popup.IsOpen=false;var color=NativeColorPicker.Pick(_settingsDialog??this);if(color!=null)selected(color);};stack.Children.Add(more);
        stack.Children.Add(new TextBlock{Text="Màu tùy chọn · #RRGGBB",FontSize=11,Margin=new(3,6,0,1)});var hex=new TextBox{MaxLength=7};var error=new TextBlock{FontSize=10,Foreground=Brushes.Firebrick,TextWrapping=TextWrapping.Wrap};hex.TextChanged+=(_,_)=>{var color=hex.Text.Trim().ToUpperInvariant();if(StudioStyle.Hex(color)){selected(color);error.Text="Đã áp dụng";}else error.Text="Nhập đủ # và 6 ký tự màu.";};stack.Children.Add(hex);stack.Children.Add(error);
        var paintPanel=new Border{Child=stack,BorderThickness=new(1),CornerRadius=new(7)};paintPanel.SetResourceReference(Border.BackgroundProperty,"Surface");paintPanel.SetResourceReference(Border.BorderBrushProperty,"Line");popup.Child=paintPanel;popup.IsOpen=true;
    }
}
