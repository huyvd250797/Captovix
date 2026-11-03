using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
public partial class MainWindow
{
    private Popup? _studioPopup;
    private static readonly string[] StampLabels=["✓ Đúng","✕ Sai","ĐÃ DUYỆT","CẦN SỬA","Ctrl + C","Ctrl + V","⌨","↖"];
    private static Annotation[] StampObjects(string text,double x,double y)
    {
        string group=Guid.NewGuid().ToString("N"),color=text.StartsWith("✕")||text=="CẦN SỬA"?"#DC3545":"#008B92";
        return [new(){Kind=AnnotationKind.RoundedRectangle,X1=x,Y1=y,X2=x+220,Y2=y+60,FillEnabled=true,FillColor="#FFFFFF",Color=color,GroupId=group},new(){Kind=AnnotationKind.Text,Text=text,TextColor=color,X1=x+12,Y1=y+10,X2=x+210,Y2=y+50,FontSize=24,Bold=true,GroupId=group}];
    }
    private void StudioMore_Click(object sender,RoutedEventArgs e)
    {if(!RequireLicense())return;
        if(_session==null||_busy)return;if(_studioPopup?.IsOpen==true){_studioPopup.IsOpen=false;return;}
        var popup=_studioPopup=new Popup{PlacementTarget=StudioMoreButton,Placement=PlacementMode.Bottom,StaysOpen=false,AllowsTransparency=true};
        var body=new StackPanel{Width=370,Margin=new(10)};
        void Heading(string text){var label=new TextBlock{Text=text,FontWeight=FontWeights.SemiBold,Margin=new(5,10,5,5)};label.SetResourceReference(TextBlock.ForegroundProperty,"Text");body.Children.Add(label);}
        void Run(string command){popup.IsOpen=false;Dispatcher.BeginInvoke(()=>ImageStudio_Click(new MenuItem{Tag=command},new()));}
        Heading("Chỉnh ảnh");var tools=new WrapPanel();body.Children.Add(tools);
        foreach(var tool in new[]{("Color","Màu & làm nét","Settings"),("Pick","Lấy màu từ ảnh","Eyedropper"),("Clone","Clone","Copy"),("Perspective","Phối cảnh","Shape"),("Rotate","Chỉnh nghiêng","Refresh"),("Background","Tách nền theo màu","PixelLasso"),("Mask","Cắt theo hình","PixelEllipse"),("Comparison","Trước / Sau","Layout"),("Watermark","Watermark","Text"),("ResetBase","Khôi phục nền gốc","Undo")}){
            var button=new Button{Content=AppIcons.Content(tool.Item3,tool.Item2,16),Width=174,Height=38,Margin=new(3),ToolTip=tool.Item2};button.Click+=(_,_)=>Run(tool.Item1);tools.Children.Add(button);
        }
        Heading("Stamp · chọn mẫu để thêm");var stamps=new WrapPanel();body.Children.Add(stamps);
        foreach(var text in StampLabels){
            var visual=new DrawingVisual();using(var dc=visual.RenderOpen())foreach(var a in StampObjects(text,10,10))EditorRenderer.DrawAnnotation(dc,a);
            var bitmap=new RenderTargetBitmap(240,80,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();
            var button=new Button{Content=new Image{Source=bitmap,Stretch=Stretch.Uniform,Margin=new(3)},Width=112,Height=48,Margin=new(3),ToolTip=text};System.Windows.Automation.AutomationProperties.SetName(button,text);stamps.Children.Add(button);
            button.Click+=(_,_)=>{popup.IsOpen=false;Dispatcher.BeginInvoke(()=>{try{if(_session==null||_busy||!FlushEdits())return;EndInlineText(true);Surface.CancelInteraction();var view=_session.Document.Viewport;AddStudioObjects(StampObjects(text,view.X+24,view.Y+24));}catch(Exception ex){App.Log(ex);SetStatus("Chưa thêm stamp: "+ex.Message);}});};
        }
        var custom=new Button{Content=AppIcons.Content("ImageLayer","Thêm logo / ảnh cá nhân…"),Margin=new(3,8,3,3)};custom.Click+=(_,_)=>{popup.IsOpen=false;Dispatcher.BeginInvoke(()=>AddImages_Click(this,new()));};body.Children.Add(custom);
        var scroll=new ScrollViewer{Content=body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,MaxHeight=Math.Max(180,Math.Min(590,SystemParameters.WorkArea.Height-100))};var frame=new Border{Child=scroll,CornerRadius=new(9),BorderThickness=new(1)};frame.SetResourceReference(Border.BackgroundProperty,"Surface");frame.SetResourceReference(Border.BorderBrushProperty,"Line");popup.Child=frame;popup.IsOpen=true;
    }
}
