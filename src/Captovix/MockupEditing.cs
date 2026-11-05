namespace Captovix;
public sealed record MockupChanges(string Text,string Font,double Size,bool Bold,bool Italic,bool Underline,TextAlign Align,TextVertical Vertical,double Padding,double Spacing,string Ink,string Background,string Fill,string Stroke,double Border,double Radius,MockupKind Kind);
public static class MockupEditing
{
    public static void Apply(EditorDocument document,string id,MockupChanges c){
        var a=document.Annotations.FirstOrDefault(a=>a.Id==id)??throw new InvalidOperationException("Đối tượng không còn tồn tại.");if(a.Mockup==null||a.Locked||!a.Visible)throw new InvalidOperationException("Hiện và mở khóa Mockup trước khi sửa.");
        if(a.Mockup.RasterBacking)throw new InvalidOperationException("Chọn chữ/nút bên trong nhóm để sửa; lớp nền giữ toàn bộ nội dung ảnh gốc.");
        var next=a.Copy();next.Text=c.Text;next.FontFamily=c.Font;next.FontSize=c.Size;next.Bold=c.Bold;next.Italic=c.Italic;next.Underline=c.Underline;next.TextAlignment=c.Align;next.VerticalAlignment=c.Vertical;next.TextPadding=c.Padding;next.LineSpacing=c.Spacing;next.TextColor=c.Ink;next.Mockup!.Background=c.Background;next.FillColor=c.Fill;next.Color=c.Stroke;next.StrokeEnabled=c.Border>0;next.Stroke=Math.Max(1,c.Border);next.CornerRadius=c.Radius;next.Mockup.Type=c.Kind;
        TextStudio.Validate(next);ShapeStudio.ValidateStyle(next);next.Mockup.Validate(next,document.OriginalWidth,document.OriginalHeight);
        document.Annotations[document.Annotations.FindIndex(x=>x.Id==id)]=next;document.Validate(document.OriginalWidth,document.OriginalHeight,document.SourceHash);
    }
    public static MockupChanges Read(Annotation a)=>new(a.Text,a.FontFamily,a.FontSize,a.Bold,a.Italic,a.Underline,a.TextAlignment,a.VerticalAlignment,a.TextPadding??0,a.LineSpacing,a.TextColor??a.Color,a.Mockup!.Background,a.FillColor,a.Color,a.StrokeEnabled?a.Stroke:0,a.CornerRadius,a.Mockup.Type);
}
