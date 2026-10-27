using System.IO;
namespace Captovix;
public enum TextAlign { Left, Center, Right }
public enum TextVertical { Top, Center, Bottom }
public enum CalloutStyle { Speech, RoundedSpeech, EllipseSpeech, Rectangle, RoundedRectangle, PillSpeech, Pill, Ellipse, DiamondSpeech, HexagonSpeech }
public enum StepStyle { Circle, Square, Diamond }
public static class TextStudio
{
    public static bool IsText(AnnotationKind kind)=>kind is AnnotationKind.Text or AnnotationKind.Callout or AnnotationKind.Step;
    public static double Padding(Annotation a)=>a.TextPadding??(a.Kind==AnnotationKind.Callout?12:a.Kind==AnnotationKind.Step?3:0);
    public static double ContentFraction(CalloutStyle style)=>style is CalloutStyle.EllipseSpeech or CalloutStyle.Ellipse?.707106:style==CalloutStyle.DiamondSpeech?.5:style==CalloutStyle.HexagonSpeech?.64:1;
    public static double MinimumBodyWidth(Annotation a)=>(Math.Max(80,a.FontSize*4)+2*Padding(a))/ContentFraction(a.CalloutStyle);
    public static double FittedBodyHeight(Annotation a,double measuredHeight,double overhang)=>Math.Clamp((measuredHeight+Math.Max(0,overhang)+2*Padding(a))/ContentFraction(a.CalloutStyle),36,100000);
    public static string Foreground(Annotation a)=>a.TextColor??(a.Kind==AnnotationKind.Step?"#FFFFFF":a.Color);
    public static void CopyStyle(Annotation from,Annotation to)
    {
        to.FontFamily=from.FontFamily;to.Bold=from.Bold;to.Italic=from.Italic;to.Underline=from.Underline;to.TextColor=from.TextColor;to.TextAlignment=from.TextAlignment;to.VerticalAlignment=from.VerticalAlignment;to.TextPadding=from.TextPadding;to.TextWidth=from.TextWidth;to.LineSpacing=from.LineSpacing;to.CalloutStyle=from.CalloutStyle;to.StepStyle=from.StepStyle;to.AutoFitText=from.AutoFitText;
    }
    public static void Validate(Annotation a)
    {
        if(string.IsNullOrWhiteSpace(a.FontFamily)||a.FontFamily.Length>80||a.FontFamily.Any(c=>!(char.IsLetterOrDigit(c)||c is ' ' or '-' or '_' or '.' or '(' or ')'))||!Enum.IsDefined(a.TextAlignment)||!Enum.IsDefined(a.VerticalAlignment)||!Enum.IsDefined(a.CalloutStyle)||!Enum.IsDefined(a.StepStyle)||a.TextPadding is {} p&&(!double.IsFinite(p)||p<0||p>200)||!double.IsFinite(a.TextWidth)||a.TextWidth<0||a.TextWidth>100000||!double.IsFinite(a.LineSpacing)||a.LineSpacing!=0&&(a.LineSpacing<.8||a.LineSpacing>3)||a.TextColor!=null&&!(a.TextColor.Length==7&&a.TextColor[0]=='#'&&a.TextColor.AsSpan(1).ToString().All(Uri.IsHexDigit)))throw new InvalidDataException("Thuộc tính chữ/callout không hợp lệ.");
    }
    public static void MigrateLegacy(Annotation a)
    {
        if(a.Kind==AnnotationKind.Callout){a.FillEnabled=true;a.FillColor="#FFFFFF";a.FillOpacity=100;a.StrokeEnabled=true;a.StrokeOpacity=100;a.Dash=StrokePattern.Solid;}
    }
    public static int Renumber(EditorDocument doc,IEnumerable<string> selection,int start)
    {
        var selected=selection.ToHashSet();var steps=doc.Annotations.Where(a=>a.Kind==AnnotationKind.Step&&(selected.Count==0?a.Visible&&!a.Locked:selected.Contains(a.Id))).ToArray();
        if(start<1||start>9999||start+steps.Length-1>9999)throw new ArgumentException("Số bước cần nằm trong 1–9999.");
        if(steps.Length>0&&!ObjectControl.CanEdit(doc,steps.Select(a=>a.Id)))throw new InvalidOperationException("Mở khóa và hiện các bước trước khi đánh lại số.");
        foreach(var a in steps)a.Number=start++;return steps.Length;
    }
}
public sealed record TypographyPreset(string FontFamily,bool Bold,bool Italic,bool Underline,string? TextColor,TextAlign Alignment,TextVertical Vertical,double? Padding,double Width,double LineSpacing,bool AutoFit,CalloutStyle Callout,StepStyle Step)
{
    public static TypographyPreset From(Annotation a)=>new(a.FontFamily,a.Bold,a.Italic,a.Underline,a.TextColor,a.TextAlignment,a.VerticalAlignment,a.TextPadding,a.TextWidth,a.LineSpacing,a.AutoFitText,a.CalloutStyle,a.StepStyle);
    public void Apply(Annotation a){a.FontFamily=FontFamily;a.Bold=Bold;a.Italic=Italic;a.Underline=Underline;a.TextColor=TextColor;a.TextAlignment=Alignment;a.VerticalAlignment=Vertical;a.TextPadding=Padding;a.TextWidth=Width;a.LineSpacing=LineSpacing;a.AutoFitText=AutoFit;a.CalloutStyle=Callout;a.StepStyle=Step;}
}
