using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace Captovix;
public partial class MainWindow
{
    private bool _imeComposing;
    private System.Windows.Input.TextComposition? _activeComposition;
    private AnnotationKind CurrentTextKind()=>Surface.Selected is {} a&&TextStudio.IsText(a.Kind)?a.Kind:Surface.Tool==EditorTool.Callout?AnnotationKind.Callout:Surface.Tool==EditorTool.Step?AnnotationKind.Step:AnnotationKind.Text;
    private void InitializeTextStudio()
    {
        System.Windows.Input.TextCompositionManager.AddPreviewTextInputStartHandler(InlineText,(_,e)=>{_imeComposing=true;_activeComposition=e.TextComposition;});
        System.Windows.Input.TextCompositionManager.AddPreviewTextInputHandler(InlineText,(_,_)=>{_imeComposing=false;_activeComposition=null;});
        TextFontPicker.ItemsSource=Fonts.SystemFontFamilies.Select(f=>f.Source).Where(n=>n.Length<=80&&n.All(c=>char.IsLetterOrDigit(c)||c is ' ' or '-' or '_' or '.' or '(' or ')')).Distinct().OrderBy(n=>n).ToArray();
    }
    private void SyncTextStudio()
    {
        var selected=Surface.Selected;bool active=selected!=null?TextStudio.IsText(selected.Kind):Surface.Tool is EditorTool.Text or EditorTool.Callout or EditorTool.Step;
        TextStudioPanel.Visibility=active?Visibility.Visible:Visibility.Collapsed;if(!active)return;
        var kind=CurrentTextKind();var defaults=Surface.TextDefaultsFor(kind);if(selected!=null){ShapeStudio.CopyStyle(selected,defaults);TextStudio.CopyStyle(selected,defaults);defaults.FillEnabled=selected.FillEnabled;defaults.FillColor=selected.FillColor;defaults.FillOpacity=selected.FillOpacity;defaults.StrokeEnabled=selected.StrokeEnabled;}
        var a=selected??defaults;TextFontPicker.Text=a.FontFamily;TextBold.IsChecked=a.Bold;TextItalic.IsChecked=a.Italic;TextUnderline.IsChecked=a.Underline;
        TextForeground.Text=TextStudio.Foreground(a);PaintButton(TextColorPaint,"Chữ",TextStudio.Foreground(a));TextAlignmentPicker.SelectedIndex=(int)a.TextAlignment;TextVerticalPicker.SelectedIndex=(int)a.VerticalAlignment;TextPaddingField.Text=ShapeNumber(TextStudio.Padding(a));TextWrapWidth.Text=ShapeNumber(a.TextWidth);TextSpacing.Text=ShapeNumber(a.LineSpacing);TextAutoFit.IsChecked=a.AutoFitText;
        TextBackgroundToggle.IsChecked=a.FillEnabled;TextBackgroundHex.Text=a.FillColor;TextBackgroundOpacity.Text=ShapeNumber(a.FillOpacity);TextBorderToggle.IsChecked=a.StrokeEnabled;
        TextWrapPanel.Visibility=kind==AnnotationKind.Callout?Visibility.Collapsed:Visibility.Visible;TextBodyOptions.Visibility=kind==AnnotationKind.Step?Visibility.Collapsed:Visibility.Visible;CalloutOptions.Visibility=kind==AnnotationKind.Callout?Visibility.Visible:Visibility.Collapsed;StepStyleOptions.Visibility=kind==AnnotationKind.Step?Visibility.Visible:Visibility.Collapsed;
        CalloutKindPicker.SelectedIndex=(int)a.CalloutStyle;StepKindPicker.SelectedIndex=(int)a.StepStyle;
        TextStudioMessage.Text=selected==null?"Kiểu cho đối tượng mới. Chọn đối tượng đã vẽ để sửa.":$"Đang chỉnh {ShapeStudio.Label(kind)} · kéo vùng trống để vẽ bằng công cụ đang chọn.";
        if(!EditorRenderer.HasTextFont(a))TextStudioMessage.Text+=" Máy này chưa có font đã lưu; xem/xuất dùng Segoe UI.";
        if(selected!=null&&_session!=null){var b=EditorRenderer.Bounds(a);var v=Surface.EditingBounds;if(b.Left<v.X||b.Top<v.Y||b.Right>v.Right||b.Bottom>v.Bottom)TextStudioMessage.Text+=" Nội dung vượt vùng ảnh/crop; hãy di chuyển, giảm cỡ chữ hoặc tăng khung trước khi xuất.";if(kind==AnnotationKind.Callout&&EditorRenderer.TextLayout(a).Height>EditorRenderer.TextContent(a).Height+1)TextStudioMessage.Text+=" Chữ cao hơn hộp; bật giãn hộp hoặc tăng chiều cao.";}
        TextStudioError.Text="";
    }
    private bool ApplyTextStudio()
    {
        if(!_editorReady||_syncProperties)return false;
        try{
            EndInlineText(true);var kind=CurrentTextKind();var style=Surface.TextDefaultsFor(kind).Copy();style.Kind=kind;
            style.FontFamily=TextFontPicker.Text.Trim();style.Bold=TextBold.IsChecked==true;style.Italic=TextItalic.IsChecked==true;style.Underline=TextUnderline.IsChecked==true;style.TextColor=TextForeground.Text.Trim().ToUpperInvariant();style.TextAlignment=(TextAlign)TextAlignmentPicker.SelectedIndex;style.VerticalAlignment=(TextVertical)TextVerticalPicker.SelectedIndex;
            style.TextPadding=ShapeValue(TextPaddingField,"Đệm chữ",0,200);style.TextWidth=ShapeValue(TextWrapWidth,"Rộng chữ",0,100000);style.LineSpacing=ShapeValue(TextSpacing,"Giãn dòng",0,3);style.AutoFitText=TextAutoFit.IsChecked==true;
            style.CalloutStyle=(CalloutStyle)CalloutKindPicker.SelectedIndex;style.StepStyle=(StepStyle)StepKindPicker.SelectedIndex;style.FillColor=TextBackgroundHex.Text.Trim().ToUpperInvariant();style.FillOpacity=ShapeValue(TextBackgroundOpacity,"Đậm nền",0,100);style.FillEnabled=TextBackgroundToggle.IsChecked==true;style.StrokeEnabled=TextBorderToggle.IsChecked==true;
            TextStudio.Validate(style);ShapeStudio.ValidateStyle(style);
            void Apply(Annotation a){TextStudio.CopyStyle(style,a);a.FillEnabled=style.FillEnabled;a.FillColor=style.FillColor;a.FillOpacity=style.FillOpacity;if(a.Kind==AnnotationKind.Callout)a.StrokeEnabled=style.StrokeEnabled;if(_session!=null)EditorRenderer.FitText(a,Surface.EditingBounds,true);}
            Apply(Surface.TextDefaultsFor(kind));ChangeSelected(a=>{if(TextStudio.IsText(a.Kind))Apply(a);});Surface.Refresh();SyncProperties();SetStatus("Đã áp dụng kiểu chữ/callout · Ctrl+Z hoàn tác.");return true;
        }catch(Exception e){TextStudioError.Text=e.Message;return false;}
    }
    private void ApplyTextStudio_Click(object sender,RoutedEventArgs e)=>ApplyTextStudio();
    private void TextStyle_KeyDown(object sender,System.Windows.Input.KeyEventArgs e){if(e.Key==System.Windows.Input.Key.Enter){ApplyTextStudio();e.Handled=true;}else if(e.Key==System.Windows.Input.Key.Escape){SyncProperties();e.Handled=true;}}
    private void ToggleTextBackground(object sender,RoutedEventArgs e)
    {
        if(!_editorReady||_syncProperties)return;bool enabled=TextBackgroundToggle.IsChecked==true;var kind=CurrentTextKind();Surface.TextDefaultsFor(kind).FillEnabled=enabled;ChangeSelected(a=>{if(a.Kind is AnnotationKind.Text or AnnotationKind.Callout)a.FillEnabled=enabled;});Surface.Refresh();SyncProperties();
    }
    private void ToggleShapeFill(object sender,RoutedEventArgs e)
    {
        if(!_editorReady||_syncProperties)return;bool enabled=ShapeFillEnabled.IsChecked==true;Surface.ShapeDefaults.FillEnabled=enabled;ChangeSelected(a=>{if(ShapeStudio.IsClosed(a.Kind))a.FillEnabled=enabled;});Surface.Refresh();SyncProperties();
    }
    private void NoShapeFill_Click(object sender,RoutedEventArgs e){if(!_editorReady)return;ShapeFillEnabled.IsChecked=false;ToggleShapeFill(sender,e);}
    private void RenumberSteps_Click(object sender,RoutedEventArgs e)
    {
        if(_session==null)return;try{if(!int.TryParse(StepStart.Text,NumberStyles.Integer,CultureInfo.InvariantCulture,out var start))throw new ArgumentException("Nhập số bắt đầu 1–9999.");EndInlineText(true);Surface.CancelInteraction();int count=0;_session.Change(d=>count=TextStudio.Renumber(d,Surface.SelectedIds,start));Surface.Refresh();SyncProperties();SetStatus($"Đã đánh lại số {count} bước.");}catch(Exception ex){TextStudioError.Text=ex.Message;}
    }
}
