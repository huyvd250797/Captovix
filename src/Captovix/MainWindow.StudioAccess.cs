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
        var tool=StudioToolCatalog.Find(_recentStudioCommand)??StudioToolCatalog.All[0];RecentStudioButton.Content=AppIcons.Content(tool.Icon,tool.Label);RecentStudioButton.ToolTip="Dùng lại · "+tool.Label+" · "+ToolShortcutLabel("Studio:"+tool.Command);System.Windows.Automation.AutomationProperties.SetName(RecentStudioButton,tool.Label);
    }
    private void RunStudioCommand(string command)
    {
        if(!RequireLicense()||_session==null||_busy)return;var tool=StudioToolCatalog.Find(command);if(tool==null)return;command=tool.Command;
        _recentStudioCommand=command;RenderRecentStudio();try{MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"last-studio-tool.json"),JsonSerializer.Serialize(command));}catch(Exception ex){App.Log(ex);}
        if(_studioPopup!=null)_studioPopup.IsOpen=false;
        if(command=="Mockup"){OpenMockupMenu(RecentStudioButton);return;}
        if(command.StartsWith("Editor:")){SelectTool(Enum.Parse<EditorTool>(command[7..]));return;}
        if(command=="Groups"){if(Ancestor<Expander>(ObjectTree) is {} expander)expander.IsExpanded=true;ObjectTree.BringIntoView();SelectTool(EditorTool.Select);Surface.Focus();return;}
        if(command=="Recovery"){RecoveryEditor_Click(this,new());return;}
        if(command=="Canvas"){ExtendCanvas_Click(this,new());return;}
        if(command=="Stamp"){OpenStampGallery(RecentStudioButton);return;}
        if(command=="CustomImage"){AddImages_Click(this,new());return;}
        ImageStudio_Click(new MenuItem{Tag=command},new());
    }
    private void RecentStudio_Click(object sender,RoutedEventArgs e)=>RunStudioCommand(_recentStudioCommand);
    private void StudioMore_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||_session==null||_busy)return;if(_studioPopup?.IsOpen==true){_studioPopup.IsOpen=false;return;}
        var body=new StackPanel{Width=372,Margin=new(8)};body.Children.Add(new TextBlock{Text="Thêm công cụ",FontWeight=FontWeights.SemiBold,Margin=new(5,3,5,9)});var grid=new System.Windows.Controls.Primitives.UniformGrid{Columns=2};body.Children.Add(grid);
        foreach(var tool in StudioToolCatalog.All){var button=new Button{Content=AppIcons.Content(tool.Icon,tool.Label,16),Height=38,Margin=new(3),ToolTip=tool.Label+" · "+ToolShortcutLabel("Studio:"+tool.Command)};button.Click+=(_,_)=>{if(_studioPopup!=null)_studioPopup.IsOpen=false;Dispatcher.BeginInvoke(()=>RunStudioCommand(tool.Command));};grid.Children.Add(button);}
        OpenStudioPopup(StudioMoreButton,body);
    }
    private void OpenStudioPopup(Button anchor,FrameworkElement content)
    {
        if(_studioPopup!=null)_studioPopup.IsOpen=false;
        var popup=_studioPopup=new Popup{PlacementTarget=anchor,Placement=PlacementMode.Bottom,StaysOpen=false,AllowsTransparency=true};
        var frame=new Border{Child=new ScrollViewer{Content=content,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,MaxHeight=Math.Max(150,Math.Min(450,SystemParameters.WorkArea.Height-100))},CornerRadius=new(9),BorderThickness=new(1.5),Margin=new(12),Effect=new System.Windows.Media.Effects.DropShadowEffect{Color=Colors.Black,BlurRadius=16,ShadowDepth=3,Opacity=.38}};frame.SetResourceReference(Border.BackgroundProperty,"Surface");frame.SetResourceReference(Border.BorderBrushProperty,"AccentText");frame.PreviewKeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Escape){popup.IsOpen=false;e.Handled=true;}};popup.Child=frame;PopupPresentation.Attach(popup);popup.IsOpen=true;
    }
    internal static BitmapSource StampImage(StampSpec spec)
    {
        var geometry=Geometry.Parse(spec.Path);var pen=new Pen(EditorRenderer.Ink(spec.Color),2.2){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,LineJoin=PenLineJoin.Round};var bounds=geometry.GetRenderBounds(pen);double scale=208/Math.Max(1,Math.Max(bounds.Width,bounds.Height));
        var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
            if(spec.Caption!=null){var ink=EditorRenderer.Ink(spec.Color);dc.DrawRoundedRectangle(null,new Pen(ink,5),new Rect(12,78,232,100),8,8);dc.DrawLine(new Pen(ink,2),new(22,90),new(234,90));dc.DrawLine(new Pen(ink,2),new(22,166),new(234,166));double labelSize=27;FormattedText Label(double size)=>new FormattedText(spec.Caption,System.Globalization.CultureInfo.GetCultureInfo("vi-VN"),FlowDirection.LeftToRight,new Typeface(new FontFamily("Segoe UI"),FontStyles.Normal,FontWeights.Bold,FontStretches.Normal),size,ink,1);var label=Label(labelSize);while(label.WidthIncludingTrailingWhitespace>208&&labelSize>12)label=Label(--labelSize);dc.DrawText(label,new((256-label.WidthIncludingTrailingWhitespace)/2,(256-label.Height)/2));}
            else{dc.PushTransform(new TranslateTransform((256-bounds.Width*scale)/2-bounds.X*scale,(256-bounds.Height*scale)/2-bounds.Y*scale));dc.PushTransform(new ScaleTransform(scale,scale));dc.DrawGeometry(null,pen,geometry);dc.Pop();dc.Pop();}
        }
        var bitmap=new RenderTargetBitmap(256,256,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();return bitmap;
    }
    private void OpenStampGallery(Button anchor)
    {
        if(!RequireLicense()||_session==null||_busy)return;
        var body=new StackPanel{Width=380,Margin=new(12)};body.Children.Add(new TextBlock{Text="Thư viện Stamp",FontWeight=FontWeights.SemiBold,FontSize=16,Margin=new(3,0,3,5)});body.Children.Add(new TextBlock{Text="Chọn mẫu, sau đó click lên ảnh để đặt. Esc kết thúc.",TextWrapping=TextWrapping.Wrap,Margin=new(3,0,3,12)});
        foreach(var group in StampCatalog.All.GroupBy(stamp=>stamp.Category)){
            body.Children.Add(new TextBlock{Text=group.Key,FontWeight=FontWeights.SemiBold,Margin=new(4,8,4,6)});var grid=new WrapPanel();body.Children.Add(grid);
            foreach(var stamp in group){var image=StampImage(stamp);var tile=new StackPanel{Margin=new(4)};tile.Children.Add(new Image{Source=image,Stretch=Stretch.Uniform,Width=48,Height=48,Margin=new(3)});tile.Children.Add(new TextBlock{Text=stamp.Label,FontSize=10,TextWrapping=TextWrapping.Wrap,TextAlignment=TextAlignment.Center,MaxWidth=76,Margin=new(0,3,0,0)});
                var button=new Button{Content=tile,Width=86,MinHeight=88,Padding=new(2),Margin=new(3),ToolTip=stamp.Label,HorizontalContentAlignment=HorizontalAlignment.Center,VerticalContentAlignment=VerticalAlignment.Center};System.Windows.Automation.AutomationProperties.SetName(button,stamp.Label);
                button.Click+=(_,_)=>{if(!RequireLicense()||_session==null||_busy||!FlushEdits())return;EndInlineText(true);Surface.CancelInteraction();if(_studioPopup!=null)_studioPopup.IsOpen=false;SelectTool(EditorTool.Select);Surface.StampPng=RasterPixels.Encode(image);Surface.StampName=stamp.Label;Surface.Tool=EditorTool.Stamp;Surface.Cursor=System.Windows.Input.Cursors.Cross;ToolHint.Text="Stamp · "+stamp.Label+" · Click để đặt · Esc kết thúc";Surface.Focus();SetStatus("Đã chọn "+stamp.Label+" · Click lên ảnh để đặt stamp, kéo tay nắm để resize.");};grid.Children.Add(button);
            }
        }
        OpenStudioPopup(anchor,body);
    }
}
