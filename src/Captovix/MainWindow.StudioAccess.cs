using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public partial class MainWindow
{
    private Popup? _studioPopup;
    private string _recentStudioCommand="Stamp";
    private void InitializeStudioAccess()
    {
        try{var path=Path.Combine(App.DataRoot,"last-studio-tool.json");if(File.Exists(path)){var value=JsonSerializer.Deserialize<string>(File.ReadAllText(path));if(StudioToolCatalog.Find(value)!=null)_recentStudioCommand=value!;}}catch(Exception ex){App.Log(ex);}
        RenderRecentStudio();
    }
    private void RenderRecentStudio()
    {
        var tool=StudioToolCatalog.Find(_recentStudioCommand)??StudioToolCatalog.All[0];RecentStudioButton.Content=AppIcons.Content(tool.Icon,tool.Label);RecentStudioButton.ToolTip="Dùng lại · "+tool.Label;System.Windows.Automation.AutomationProperties.SetName(RecentStudioButton,tool.Label);
    }
    private void RunStudioCommand(string command)
    {
        if(!RequireLicense()||_session==null||_busy)return;var tool=StudioToolCatalog.Find(command);if(tool==null)return;
        _recentStudioCommand=command;RenderRecentStudio();try{MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"last-studio-tool.json"),JsonSerializer.Serialize(command));}catch(Exception ex){App.Log(ex);}
        if(_studioPopup!=null)_studioPopup.IsOpen=false;
        if(command=="Stamp"){OpenStampGallery(RecentStudioButton);return;}
        if(command=="CustomImage"){AddImages_Click(this,new());return;}
        ImageStudio_Click(new MenuItem{Tag=command},new());
    }
    private void RecentStudio_Click(object sender,RoutedEventArgs e)=>RunStudioCommand(_recentStudioCommand);
    private void StudioMore_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||_session==null||_busy)return;if(_studioPopup?.IsOpen==true){_studioPopup.IsOpen=false;return;}
        var body=new StackPanel{Width=360,Margin=new(10)};body.Children.Add(new TextBlock{Text="Thêm công cụ",FontWeight=FontWeights.SemiBold,Margin=new(5,3,5,9)});var grid=new WrapPanel();body.Children.Add(grid);
        foreach(var tool in StudioToolCatalog.All){var button=new Button{Content=AppIcons.Content(tool.Icon,tool.Label,16),Width=174,Height=38,Margin=new(3),ToolTip=tool.Label};button.Click+=(_,_)=>{if(_studioPopup!=null)_studioPopup.IsOpen=false;Dispatcher.BeginInvoke(()=>RunStudioCommand(tool.Command));};grid.Children.Add(button);}
        OpenStudioPopup(StudioMoreButton,body);
    }
    private void OpenStudioPopup(Button anchor,FrameworkElement content)
    {
        if(_studioPopup!=null)_studioPopup.IsOpen=false;
        var popup=_studioPopup=new Popup{PlacementTarget=anchor,Placement=PlacementMode.Bottom,StaysOpen=false,AllowsTransparency=true};
        var frame=new Border{Child=new ScrollViewer{Content=content,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,MaxHeight=Math.Max(150,Math.Min(450,SystemParameters.WorkArea.Height-100))},CornerRadius=new(9),BorderThickness=new(1)};frame.SetResourceReference(Border.BackgroundProperty,"Surface");frame.SetResourceReference(Border.BorderBrushProperty,"Line");frame.PreviewKeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Escape){popup.IsOpen=false;e.Handled=true;}};popup.Child=frame;popup.IsOpen=true;
    }
    internal static BitmapSource StampImage(StampSpec spec)
    {
        var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
            dc.PushTransform(new TranslateTransform(24,24));dc.PushTransform(new ScaleTransform(208d/24,208d/24));
            var pen=new Pen(EditorRenderer.Ink(spec.Color),2.2){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,LineJoin=PenLineJoin.Round};dc.DrawGeometry(null,pen,Geometry.Parse(spec.Path));dc.Pop();dc.Pop();
        }
        var bitmap=new RenderTargetBitmap(256,256,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();return bitmap;
    }
    private void OpenStampGallery(Button anchor)
    {
        if(!RequireLicense()||_session==null||_busy)return;
        var body=new StackPanel{Width=310,Margin=new(12)};body.Children.Add(new TextBlock{Text="Stamp",FontWeight=FontWeights.SemiBold,Margin=new(3,0,3,10)});var grid=new WrapPanel();body.Children.Add(grid);
        foreach(var stamp in StampCatalog.All){var image=StampImage(stamp);var button=new Button{Content=new Image{Source=image,Stretch=Stretch.Uniform,Width=42,Height=42,Margin=new(7)},Width=68,Height=64,Margin=new(3),ToolTip=stamp.Label};System.Windows.Automation.AutomationProperties.SetName(button,stamp.Label);button.Click+=(_,_)=>{
                if(!RequireLicense()||_session==null||_busy||!FlushEdits())return;if(_studioPopup!=null)_studioPopup.IsOpen=false;
                try{EndInlineText(true);Surface.CancelInteraction();var view=_session.Document.Viewport;double size=Math.Min(96,Math.Max(32,Math.Min(view.Width,view.Height)/4d));var a=new Annotation{Kind=AnnotationKind.ImageLayer,Name=stamp.Label,X1=view.X+12,Y1=view.Y+12,X2=view.X+12+size,Y2=view.Y+12+size,PixelPng=RasterPixels.Encode(image),ImageStyle=new()};AddStudioObjects(a);SetStatus("Đã thêm stamp · kéo để di chuyển, tay nắm để đổi kích thước · Ctrl+Z hoàn tác.");}catch(Exception ex){App.Log(ex);SetStatus("Chưa thêm stamp: "+ex.Message);}
            };grid.Children.Add(button);
        }
        OpenStudioPopup(anchor,body);
    }
}
