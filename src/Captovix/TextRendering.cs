using System.Globalization;
using System.Windows;
using System.Windows.Media;
namespace Captovix;
public static partial class EditorRenderer
{
    private static readonly Lazy<Dictionary<string,FontFamily>> InstalledFonts=new(()=>System.Windows.Media.Fonts.SystemFontFamilies.GroupBy(f=>f.Source,StringComparer.OrdinalIgnoreCase).ToDictionary(g=>g.Key,g=>g.First(),StringComparer.OrdinalIgnoreCase));
    public static FontFamily TextFont(Annotation a)=>InstalledFonts.Value.TryGetValue(a.FontFamily,out var font)?font:new FontFamily("Segoe UI");
    public static TextAlignment WpfAlignment(TextAlign align)=>align==TextAlign.Center?TextAlignment.Center:align==TextAlign.Right?TextAlignment.Right:TextAlignment.Left;
    public static bool HasTextFont(Annotation a)=>InstalledFonts.Value.ContainsKey(a.FontFamily);
    public static FormattedText TextLayout(Annotation a,string? text=null,Brush? ink=null)
    {
        var typeface=new Typeface(TextFont(a),a.Italic?FontStyles.Italic:FontStyles.Normal,a.Bold?FontWeights.Bold:FontWeights.Normal,FontStretches.Normal);
        var layout=new FormattedText(text??a.Text,CultureInfo.CurrentUICulture,FlowDirection.LeftToRight,typeface,a.FontSize,ink??Ink(TextStudio.Foreground(a)),1.0){Trimming=TextTrimming.None};
        if(a.Underline)layout.SetTextDecorations(TextDecorations.Underline);
        if(a.LineSpacing>0)layout.LineHeight=a.FontSize*a.LineSpacing;
        if(text==null){
            double width=a.Kind==AnnotationKind.Callout?TextContent(a).Width:a.TextWidth;
            if(width>0){layout.MaxTextWidth=Math.Max(1,width);layout.TextAlignment=WpfAlignment(a.TextAlignment);}
        }return layout;
    }
    public static Rect TextContent(Annotation a)
    {
        double pad=TextStudio.Padding(a);
        if(a.Kind==AnnotationKind.Callout){var b=new Rect(a.Left,a.Top,Math.Max(36,a.Width),Math.Max(36,a.Height));double ix=a.CalloutStyle==CalloutStyle.EllipseSpeech?b.Width*.146447:0,iy=a.CalloutStyle==CalloutStyle.EllipseSpeech?b.Height*.146447:0;return new(b.X+pad+ix,b.Y+pad+iy,Math.Max(1,b.Width-2*(pad+ix)),Math.Max(1,b.Height-2*(pad+iy)));}
        var text=TextLayout(a,a.Text);return new(a.X1+pad,a.Y1+pad,a.TextWidth>0?a.TextWidth:Math.Max(10,text.WidthIncludingTrailingWhitespace),Math.Max(a.FontSize,text.Height));
    }
    public static Point TextOrigin(Annotation a,FormattedText layout)
    {
        var inner=TextContent(a);double offset=a.Kind==AnnotationKind.Callout?Math.Max(0,inner.Height-layout.Height)*(int)a.VerticalAlignment/2:0;return new(inner.X,inner.Y+offset);
    }
    public static Rect TextInkBounds(Annotation a)
    {
        var text=TextLayout(a);var origin=TextOrigin(a,text);var logical=new Rect(origin.X,origin.Y,Math.Max(10,a.TextWidth>0&&a.Kind==AnnotationKind.Text?a.TextWidth:text.WidthIncludingTrailingWhitespace),Math.Max(a.FontSize,text.Height));
        var ink=text.BuildGeometry(origin).Bounds;return ink.IsEmpty?logical:Rect.Union(logical,ink);
    }
    public static double StepRadius(Annotation a)
    {
        var text=TextLayout(a,a.Number.ToString(CultureInfo.InvariantCulture));return Math.Max(a.FontSize*.8,Math.Max(text.WidthIncludingTrailingWhitespace,text.Height)/2+TextStudio.Padding(a));
    }
    public static Geometry StepGeometry(Annotation a)
    {
        double radius=StepRadius(a);var b=new Rect(a.X1-radius,a.Y1-radius,radius*2,radius*2);
        if(a.StepStyle==StepStyle.Circle)return new EllipseGeometry(b);
        if(a.StepStyle==StepStyle.Square)return new RectangleGeometry(b,4,4);
        double d=radius*Math.Sqrt(2);var g=new StreamGeometry();using(var c=g.Open()){c.BeginFigure(new(a.X1,a.Y1-d),true,true);c.PolyLineTo(new[]{new Point(a.X1+d,a.Y1),new(a.X1,a.Y1+d),new(a.X1-d,a.Y1)},true,false);}return g;
    }
    private static void DrawStyledText(DrawingContext dc,Annotation a)
    {
        var text=TextLayout(a);var inkBounds=TextInkBounds(a);double pad=TextStudio.Padding(a);
        if(a.Kind==AnnotationKind.Text&&a.FillEnabled){var bg=inkBounds;bg.Inflate(pad,pad);dc.DrawRectangle(ShapeInk(a.FillColor,a.FillOpacity),null,bg);}
        dc.DrawText(text,TextOrigin(a,text));
    }
    private static Geometry StyledCalloutGeometry(Annotation a)
    {
        var b=new Rect(a.Left,a.Top,Math.Max(36,a.Width),Math.Max(36,a.Height));double radius=Math.Min(a.CornerRadius,Math.Min(b.Width,b.Height)/2);
        Geometry body=a.CalloutStyle==CalloutStyle.EllipseSpeech?new EllipseGeometry(b):a.CalloutStyle is CalloutStyle.RoundedSpeech or CalloutStyle.RoundedRectangle?new RectangleGeometry(b,radius,radius):new RectangleGeometry(b);
        var tail=new Point(a.TailX,a.TailY);if(a.CalloutStyle is CalloutStyle.Rectangle or CalloutStyle.RoundedRectangle||b.Contains(tail))return body;
        Point first,last;
        if(a.CalloutStyle==CalloutStyle.EllipseSpeech){double angle=Math.Atan2((tail.Y-(b.Top+b.Height/2))/(b.Height/2),(tail.X-(b.Left+b.Width/2))/(b.Width/2));first=new(b.Left+b.Width/2+b.Width/2*Math.Cos(angle-.16),b.Top+b.Height/2+b.Height/2*Math.Sin(angle-.16));last=new(b.Left+b.Width/2+b.Width/2*Math.Cos(angle+.16),b.Top+b.Height/2+b.Height/2*Math.Sin(angle+.16));}
        else{bool side=Math.Abs((tail.X-(b.Left+b.Width/2))/(b.Width/2))>Math.Abs((tail.Y-(b.Top+b.Height/2))/(b.Height/2));if(side){double y=Math.Clamp(tail.Y,b.Top+12,b.Bottom-12),x=tail.X<b.Left?b.Left:b.Right;first=new(x,y-8);last=new(x,y+8);}else{double x=Math.Clamp(tail.X,b.Left+12,b.Right-12),y=tail.Y<b.Top?b.Top:b.Bottom;first=new(x-8,y);last=new(x+8,y);}}
        var triangle=new StreamGeometry();using(var c=triangle.Open()){c.BeginFigure(first,true,true);c.LineTo(tail,true,false);c.LineTo(last,true,false);}return Geometry.Combine(body,triangle,GeometryCombineMode.Union,null);
    }
    public static void FitText(Annotation a,PixelRect view)
    {
        if(!TextStudio.IsText(a.Kind)||a.Kind==AnnotationKind.Step)return;
        if(a.Kind==AnnotationKind.Callout&&a.AutoFitText){
            var text=TextLayout(a);double padding=TextStudio.Padding(a);double needed=(text.Height+Math.Max(0,text.OverhangAfter)+padding*2)/(a.CalloutStyle==CalloutStyle.EllipseSpeech?.707106:1);
            double height=Math.Clamp(Math.Max(a.Height,needed),36,100000);var top=a.Top;var left=a.Left;var width=Math.Max(36,a.Width);a.X1=left;a.Y1=top;a.X2=left+width;a.Y2=top+height;
        }
        if(a.AutoFitText){var b=LocalBounds(a);double dx=b.Width<=view.Width?Math.Clamp(b.Left,view.X,view.Right-b.Width)-b.Left:0,dy=b.Height<=view.Height?Math.Clamp(b.Top,view.Y,view.Bottom-b.Height)-b.Top:0;if(dx!=0||dy!=0)a.Translate(dx,dy);}
    }
}
