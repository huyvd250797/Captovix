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
        if(a.Kind==AnnotationKind.Callout){var b=new Rect(a.Left,a.Top,Math.Max(36,a.Width),Math.Max(36,a.Height));double fraction=a.CalloutStyle is CalloutStyle.EllipseSpeech or CalloutStyle.Ellipse?.146447:a.CalloutStyle==CalloutStyle.DiamondSpeech?.25:a.CalloutStyle==CalloutStyle.HexagonSpeech?.18:0;double ix=b.Width*fraction,iy=b.Height*fraction;return new(b.X+pad+ix,b.Y+pad+iy,Math.Max(1,b.Width-2*(pad+ix)),Math.Max(1,b.Height-2*(pad+iy)));}
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
        if(a.Kind==AnnotationKind.Callout)dc.PushClip(new RectangleGeometry(TextContent(a)));
        dc.DrawText(text,TextOrigin(a,text));if(a.Kind==AnnotationKind.Callout)dc.Pop();
    }
    private static Geometry StyledCalloutGeometry(Annotation a)
    {
        var b=new Rect(a.Left,a.Top,Math.Max(36,a.Width),Math.Max(36,a.Height));double radius=Math.Min(a.CornerRadius,Math.Min(b.Width,b.Height)/2);
        if(a.CalloutStyle is CalloutStyle.Pill or CalloutStyle.PillSpeech)radius=Math.Min(b.Width,b.Height)/2;
        Geometry body=a.CalloutStyle is CalloutStyle.EllipseSpeech or CalloutStyle.Ellipse?new EllipseGeometry(b):a.CalloutStyle is CalloutStyle.RoundedSpeech or CalloutStyle.RoundedRectangle or CalloutStyle.Pill or CalloutStyle.PillSpeech?new RectangleGeometry(b,radius,radius):new RectangleGeometry(b);
        if(a.CalloutStyle is CalloutStyle.DiamondSpeech or CalloutStyle.HexagonSpeech){var shape=a.Copy();shape.Kind=a.CalloutStyle==CalloutStyle.DiamondSpeech?AnnotationKind.Diamond:AnnotationKind.Hexagon;body=ShapeGeometry(shape);}
        var tail=new Point(a.TailX,a.TailY);if(a.CalloutStyle is CalloutStyle.Rectangle or CalloutStyle.RoundedRectangle or CalloutStyle.Pill or CalloutStyle.Ellipse||b.Contains(tail))return body;
        Point first,last;
        if(a.CalloutStyle==CalloutStyle.EllipseSpeech){double angle=Math.Atan2((tail.Y-(b.Top+b.Height/2))/(b.Height/2),(tail.X-(b.Left+b.Width/2))/(b.Width/2));first=new(b.Left+b.Width/2+b.Width/2*Math.Cos(angle-.16),b.Top+b.Height/2+b.Height/2*Math.Sin(angle-.16));last=new(b.Left+b.Width/2+b.Width/2*Math.Cos(angle+.16),b.Top+b.Height/2+b.Height/2*Math.Sin(angle+.16));}
        else{bool side=Math.Abs((tail.X-(b.Left+b.Width/2))/(b.Width/2))>Math.Abs((tail.Y-(b.Top+b.Height/2))/(b.Height/2));if(side){double y=Math.Clamp(tail.Y,b.Top+12,b.Bottom-12),x=tail.X<b.Left?b.Left:b.Right;first=new(x,y-8);last=new(x,y+8);}else{double x=Math.Clamp(tail.X,b.Left+12,b.Right-12),y=tail.Y<b.Top?b.Top:b.Bottom;first=new(x-8,y);last=new(x+8,y);}}
        if(a.CalloutStyle is CalloutStyle.PillSpeech or CalloutStyle.DiamondSpeech or CalloutStyle.HexagonSpeech){
            var center=new Point(b.Left+b.Width/2,b.Top+b.Height/2);double angle=Math.Atan2(tail.Y-center.Y,tail.X-center.X);
            Point Boundary(double theta){var vector=new Vector(Math.Cos(theta),Math.Sin(theta));double lo=0,hi=Math.Max(b.Width,b.Height)*2;for(int i=0;i<40;i++){double mid=(lo+hi)/2;if(body.FillContains(center+vector*mid))lo=mid;else hi=mid;}return center+vector*Math.Max(0,lo-.5);}
            first=Boundary(angle-.15);last=Boundary(angle+.15);
        }
        var triangle=new StreamGeometry();using(var c=triangle.Open()){c.BeginFigure(first,true,true);c.LineTo(tail,true,false);c.LineTo(last,true,false);}return Geometry.Combine(body,triangle,GeometryCombineMode.Union,null);
    }
    public static void FitText(Annotation a,PixelRect view)
    {
        if(!TextStudio.IsText(a.Kind)||a.Kind==AnnotationKind.Step)return;
        if(a.Kind==AnnotationKind.Callout&&a.AutoFitText){
            double pad=TextStudio.Padding(a);double fraction=a.CalloutStyle is CalloutStyle.EllipseSpeech or CalloutStyle.Ellipse?.707106:a.CalloutStyle==CalloutStyle.DiamondSpeech?.5:a.CalloutStyle==CalloutStyle.HexagonSpeech?.64:1;
            var natural=TextLayout(a,a.Text);double minContent=Math.Max(a.FontSize*5,Math.Min(natural.WidthIncludingTrailingWhitespace,420));double wanted=(minContent+pad*2)/fraction;
            double maxWidth=Math.Max(36,view.Width);double widthBefore=Math.Max(36,a.Width);double newWidth=Math.Max(widthBefore,Math.Min(maxWidth,wanted));
            double bodyLeft=a.Left;a.X1=bodyLeft;a.X2=bodyLeft+newWidth;
            var text=TextLayout(a);double padding=pad;double needed=(text.Height+Math.Max(0,text.OverhangAfter)+padding*2)/(a.CalloutStyle is CalloutStyle.EllipseSpeech or CalloutStyle.Ellipse?.707106:a.CalloutStyle==CalloutStyle.DiamondSpeech?.5:a.CalloutStyle==CalloutStyle.HexagonSpeech?.64:1);
            double height=Math.Clamp(Math.Max(a.Height,needed),36,Math.Max(36,view.Height));var top=a.Top;var left=a.Left;var width=Math.Max(36,a.Width);a.X1=left;a.Y1=top;a.X2=left+width;a.Y2=top+height;
        }
        if(a.AutoFitText){var b=LocalBounds(a);double dx=b.Width<=view.Width?Math.Clamp(b.Left,view.X,view.Right-b.Width)-b.Left:0,dy=b.Height<=view.Height?Math.Clamp(b.Top,view.Y,view.Bottom-b.Height)-b.Top:0;if(dx!=0||dy!=0)a.Translate(dx,dy);}
    }
}
