using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace Captovix;
public partial class MainWindow
{
    private bool _shapeReady;
    private sealed record ShapeChoice(AnnotationKind Kind,string Label);
    private static string ShapeNumber(double n)=>n.ToString("0.##",CultureInfo.InvariantCulture);
    private void InitializeShapeStudio()
    {
        ShapeKindPicker.ItemsSource=ShapeStudio.Shapes.Select(k=>new ShapeChoice(k,ShapeStudio.Label(k))).ToArray();_shapeReady=true;
    }
    private AnnotationKind? CurrentStrokeKind()=>Surface.Selected?.Kind??(Surface.Tool switch{EditorTool.Shape=>Surface.ShapeKind,EditorTool.Rectangle=>AnnotationKind.Rectangle,EditorTool.Ellipse=>AnnotationKind.Ellipse,EditorTool.Arrow=>AnnotationKind.Arrow,EditorTool.Line=>AnnotationKind.Line,EditorTool.Freehand=>AnnotationKind.Freehand,_=>(AnnotationKind?)null});
    private void SyncShapeStudio()
    {
        if(!_shapeReady)return;
        var selected=Surface.Selected;var kind=CurrentStrokeKind();bool active=kind is {} k&&ShapeStudio.HasStroke(k);
        ShapeStudioPanel.Visibility=active?Visibility.Visible:Visibility.Collapsed;if(!active)return;
        if(selected!=null&&ShapeStudio.HasStroke(selected.Kind)){ShapeStudio.CopyStyle(selected,Surface.ShapeDefaults);if(ShapeStudio.IsClosed(selected.Kind))Surface.ShapeKind=selected.Kind;}
        var a=Surface.ShapeDefaults;bool closed=kind is {} shape&&ShapeStudio.IsClosed(shape);
        ShapeKindPanel.Visibility=closed?Visibility.Visible:Visibility.Collapsed;ShapeKindPicker.SelectedValue=closed?kind:Surface.ShapeKind;
        ShapeTargetHint.Text=Surface.SelectedIds.Count>0?$"Sửa {Surface.SelectedIds.Count} đối tượng đã chọn; lưu tự động, có Undo.":"Kiểu cho nét vẽ tiếp theo. Đối tượng đã vẽ được giữ nguyên.";
        ShapeStrokeHex.Text=Surface.InkColor;ShapeFillHex.Text=a.FillColor;ShapeStrokeEnabled.IsChecked=a.StrokeEnabled;ShapeFillEnabled.IsChecked=a.FillEnabled;ShapeFillEnabled.IsEnabled=closed;ShapeFillHex.IsEnabled=closed;
        ShapeStrokeOpacity.Text=ShapeNumber(a.StrokeOpacity);ShapeFillOpacity.Text=ShapeNumber(a.FillOpacity);ShapeThickness.Text=ShapeNumber(Surface.StrokeSize);ShapeDash.SelectedIndex=(int)a.Dash;
        ShapeRadius.Text=ShapeNumber(a.CornerRadius);ShapeStarPoints.Text=a.StarPoints.ToString(CultureInfo.InvariantCulture);ShapeStarInset.Text=ShapeNumber(a.StarInset*100);
        ShapeArrowTip.SelectedIndex=(int)a.Tip;ShapeDoubleArrow.IsChecked=a.DoubleArrow;ShapeArrowSize.Text=ShapeNumber(a.ArrowHeadSize);
        RoundedShapePanel.Visibility=kind==AnnotationKind.RoundedRectangle?Visibility.Visible:Visibility.Collapsed;StarShapePanel.Visibility=kind==AnnotationKind.Star?Visibility.Visible:Visibility.Collapsed;ArrowShapePanel.Visibility=kind==AnnotationKind.Arrow?Visibility.Visible:Visibility.Collapsed;
        ShapeStyleError.Text="";
    }
    private void ShapeKind_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(!_shapeReady||_syncProperties||ShapeKindPicker.SelectedValue is not AnnotationKind kind||!ShapeStudio.IsClosed(kind))return;
        Surface.ShapeKind=kind;ChangeSelected(a=>{if(ShapeStudio.IsClosed(a.Kind))a.Kind=kind;});
        if(Surface.SelectedIds.Count==0)SelectTool(EditorTool.Shape);Surface.Refresh();SyncProperties();
    }
    private static double ShapeValue(TextBox box,string label,double min,double max)
    {
        if(!double.TryParse(box.Text.Trim().Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out var n)||!double.IsFinite(n)||n<min||n>max)throw new ArgumentException($"{label}: nhập từ {min} đến {max}.");return n;
    }
    private bool ApplyShapeStyle()
    {
        if(!_shapeReady||_syncProperties||Surface.Session==null)return false;
        try{
            var style=Surface.ShapeDefaults.Copy();style.Color=ShapeStrokeHex.Text.Trim().ToUpperInvariant();style.FillColor=ShapeFillHex.Text.Trim().ToUpperInvariant();style.Stroke=ShapeValue(ShapeThickness,"Độ dày",1,40);
            style.StrokeOpacity=ShapeValue(ShapeStrokeOpacity,"Đậm viền",0,100);style.FillOpacity=ShapeValue(ShapeFillOpacity,"Đậm nền",0,100);style.StrokeEnabled=ShapeStrokeEnabled.IsChecked==true;style.FillEnabled=ShapeFillEnabled.IsChecked==true;style.Dash=(StrokePattern)ShapeDash.SelectedIndex;
            style.CornerRadius=ShapeValue(ShapeRadius,"Bo góc",0,1000);double points=ShapeValue(ShapeStarPoints,"Số cánh",3,20);if(points!=Math.Truncate(points))throw new ArgumentException("Số cánh phải là số nguyên.");style.StarPoints=(int)points;style.StarInset=ShapeValue(ShapeStarInset,"Lõi sao",10,90)/100;
            style.Tip=(ArrowTip)ShapeArrowTip.SelectedIndex;style.DoubleArrow=ShapeDoubleArrow.IsChecked==true;style.ArrowHeadSize=ShapeValue(ShapeArrowSize,"Cỡ đầu mũi tên",0,160);
            AnnotationPreset.From("Validate",style).Validate();Surface.ShapeDefaults=style;Surface.InkColor=style.Color;Surface.StrokeSize=style.Stroke;
            ChangeSelected(a=>{if(ShapeStudio.HasStroke(a.Kind)){a.Color=style.Color;a.Stroke=style.Stroke;ShapeStudio.CopyStyle(style,a);}});
            Surface.Refresh();SyncProperties();SetStatus(Surface.SelectedIds.Count>0?"Đã cập nhật kiểu đối tượng. Ctrl+Z để hoàn tác.":"Đã đặt kiểu cho nét tiếp theo.");return true;
        }catch(Exception ex){ShapeStyleError.Text=ex.Message;return false;}
    }
    private void ApplyShapeStyle_Click(object sender,RoutedEventArgs e)=>ApplyShapeStyle();
    private void ShapeField_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Enter){ApplyShapeStyle();e.Handled=true;}else if(e.Key==Key.Escape){SyncProperties();e.Handled=true;}}
    private void FillSwatch_Click(object sender,RoutedEventArgs e){if(sender is Button {Tag:string color}){ShapeFillHex.Text=color;ShapeFillEnabled.IsChecked=true;ApplyShapeStyle();}}
}
