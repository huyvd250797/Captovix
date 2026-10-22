using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace Captovix;
public partial class MainWindow
{
    private AnnotationKind? CurrentFocusKind()=>Surface.Selected is {} a?FocusStudio.Supports(a.Kind)?a.Kind:null:Enum.TryParse<AnnotationKind>(Surface.Tool.ToString(),out var kind)&&FocusStudio.Supports(kind)?kind:null;
    private void SyncFocusStudio()
    {
        PrivacyReviewPanel.Visibility=_session!=null&&(_session.Document.Annotations.Any(a=>a.IsRedaction)||Surface.Tool is EditorTool.Blur or EditorTool.Pixelate or EditorTool.Cover)?Visibility.Visible:Visibility.Collapsed;
        PrivacyReview.Text=_session==null?"":FocusStudio.Review(_session.Document);
        var kind=CurrentFocusKind();FocusStudioPanel.Visibility=kind==null?Visibility.Collapsed:Visibility.Visible;if(kind==null)return;
        EffectProperties.Visibility=Visibility.Collapsed;var defaults=Surface.FocusDefaultsFor(kind.Value);var a=Surface.Selected??defaults;
        if(Surface.Selected!=null){FocusStudio.CopyStyle(a,defaults);defaults.Color=a.Color;defaults.EffectSize=a.EffectSize;defaults.Stroke=a.Stroke;defaults.StrokeEnabled=a.StrokeEnabled;}
        FocusRegionPicker.SelectedIndex=(int)a.RegionShape;FocusOpacityField.Text=ShapeNumber(a.FocusOpacity);FocusZoomField.Text=ShapeNumber(a.Magnification);FocusColorField.Text=Surface.Selected==null?Surface.InkColor:a.Color;FocusStrength.Text=a.EffectSize.ToString(CultureInfo.InvariantCulture);FocusBorder.IsChecked=a.StrokeEnabled;FocusBorderWidth.Text=ShapeNumber(a.Stroke);
        FocusOpacityPanel.Visibility=kind is AnnotationKind.Highlight or AnnotationKind.Spotlight?Visibility.Visible:Visibility.Collapsed;
        FocusLensPanel.Visibility=kind==AnnotationKind.Magnifier?Visibility.Visible:Visibility.Collapsed;
        FocusPrivacyPanel.Visibility=a.IsRedaction?Visibility.Visible:Visibility.Collapsed;FocusMaskKind.SelectedIndex=kind==AnnotationKind.Blur?0:kind==AnnotationKind.Pixelate?1:2;
        FocusHint.Text=kind==AnnotationKind.Magnifier?"Kính tại chỗ: 1.25–8×; di chuyển kính để lấy vùng khác. Lấy mẫu ảnh gốc đã áp các vùng che đang bật; không phóng chú thích. Không tăng chi tiết thật.":kind==AnnotationKind.Spotlight?"Các vùng sáng được hợp lại. Màu/độ tối lấy Spotlight trên cùng; lớp tối nằm dưới chú thích. Alt+kéo để thêm vùng sáng chồng.":a.IsRedaction?"Che kín luôn là màu đặc 100%. Blur/Pixel 4–64; không bảo đảm giấu nội dung. Lớp che đặc nằm trên mọi hiệu ứng.":"Đánh dấu theo vùng chữ nhật/ellipse; màu và độ đậm riêng. Không dùng highlight để che thông tin.";
        FocusError.Text="";
    }
    private void ApplyFocus_Click(object sender,RoutedEventArgs e)
    {
        if(!_editorReady||_syncProperties||CurrentFocusKind() is not {} kind)return;
        try{
            EndInlineText(true);Surface.CancelInteraction();var style=Surface.FocusDefaultsFor(kind).Copy();style.Kind=kind;
            style.RegionShape=(RegionShape)FocusRegionPicker.SelectedIndex;style.Magnification=ShapeValue(FocusZoomField,"Mức phóng",1.25,8);style.FocusOpacity=ShapeValue(FocusOpacityField,"Độ đậm",0,100);style.Stroke=ShapeValue(FocusBorderWidth,"Viền kính",1,40);style.StrokeEnabled=FocusBorder.IsChecked==true;
            style.Color=FocusColorField.Text.Trim().ToUpperInvariant();if(style.Color.Length!=7||style.Color[0]!='#'||!style.Color.AsSpan(1).ToString().All(Uri.IsHexDigit))throw new ArgumentException("Màu cần dạng #RRGGBB.");
            if(!int.TryParse(FocusStrength.Text,NumberStyles.Integer,CultureInfo.InvariantCulture,out var strength)||strength<4||strength>64)throw new ArgumentException("Mức Blur/Pixel từ 4 đến 64.");style.EffectSize=strength;FocusStudio.Validate(style);
            var maskKind=FocusMaskKind.SelectedIndex switch{0=>AnnotationKind.Blur,1=>AnnotationKind.Pixelate,2=>AnnotationKind.Cover,_=>throw new ArgumentException("Chọn kiểu che.")};
            void Apply(Annotation a){if(!FocusStudio.Supports(a.Kind))return;FocusStudio.CopyStyle(style,a);a.Color=style.Color;a.EffectSize=strength;if(a.Kind==AnnotationKind.Magnifier){a.Stroke=style.Stroke;a.StrokeEnabled=style.StrokeEnabled;a.StrokeOpacity=100;a.Dash=StrokePattern.Solid;}if(a.IsRedaction)a.Kind=maskKind;}
            Apply(Surface.FocusDefaultsFor(kind));Surface.InkColor=style.Color;Surface.EffectSize=strength;ChangeSelected(Apply);if(Surface.SelectedIds.Count==0&&kind is AnnotationKind.Blur or AnnotationKind.Pixelate or AnnotationKind.Cover)SelectTool(Enum.Parse<EditorTool>(maskKind.ToString()));Surface.Refresh();SyncProperties();SetStatus("Đã áp dụng hiệu ứng · Ctrl+Z hoàn tác.");
        }catch(Exception ex){FocusError.Text=ex.Message;}
    }
    private void FocusField_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Enter){ApplyFocus_Click(sender,e);e.Handled=true;}else if(e.Key==Key.Escape){SyncProperties();e.Handled=true;}}
    private void PrivacyReview_Click(object sender,RoutedEventArgs e)
    {
        if(_session==null||!PrepareCropForExport())return;Surface.CancelInteraction();SelectTool(EditorTool.Select);Surface.SelectIds([]);Actual_Click(sender,e);SyncProperties();SetStatus(FocusStudio.Review(_session.Document));Surface.Focus();
    }
    private string PrivacyExportStatus()=>_session?.Document.Annotations.Any(a=>a.IsRedaction)==true?" · "+FocusStudio.Review(_session.Document):"";
}
