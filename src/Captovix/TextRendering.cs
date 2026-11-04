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
    [System.ThreadStatic] private static System.Runtime.CompilerServices.ConditionalWeakTable<Annotation,LayoutEntry>? _layouts;
    private sealed class LayoutEntry { public LayoutEntry(){}  public object? Key;public FormattedText? Layout;public Rect? InkBounds; }
    private static double CacheCoordinate(double value)=>Math.Round(value,6);
    private static object LayoutKey(Annotation a)=> (a.Text,a.FontFamily,a.FontSize,a.Bold,a.Italic,a.Underline,a.LineSpacing,a.TextAlignment,TextStudio.Foreground(a),CacheCoordinate(a.Kind==AnnotationKind.Callout||a.Kind==AnnotationKind.Text&&a.TextFrame&&a.Width>=2?TextContent(a).Width:a.TextWidth),CultureInfo.CurrentUICulture.Name);
    public static FormattedText TextLayout(Annotation a,string? text=null,Brush? ink=null)
    {
        LayoutEntry? cached=null;object? key=null;
        if(text==null&&ink==null){cached=(_layouts??=new()).GetOrCreateValue(a);key=LayoutKey(a);if(Equals(cached.Key,key)&&cached.Layout!=null)return cached.Layout;}
        var typeface=new Typeface(TextFont(a),a.Italic?FontStyles.Italic:FontStyles.Normal,a.Bold?FontWeights.Bold:FontWeights.Normal,FontStretches.Normal);
        var layout=new FormattedText(text??a.Text,CultureInfo.CurrentUICulture,FlowDirection.LeftToRight,typeface,a.FontSize,ink??Ink(TextStudio.Foreground(a)),1.0){Trimming=TextTrimming.None};
        if(a.Underline)layout.SetTextDecorations(TextDecorations.Underline);
        if(a.LineSpacing>0)layout.LineHeight=a.FontSize*a.LineSpacing;
        if(text==null){
            double width=a.Kind==AnnotationKind.Callout||a.Kind==AnnotationKind.Text&&a.TextFrame&&a.Width>=2?TextContent(a).Width:a.TextWidth;
            if(width>0){layout.MaxTextWidth=Math.Max(1,width);layout.TextAlignment=WpfAlignment(a.TextAlignment);}
        }if(cached!=null){cached.Key=key;cached.Layout=layout;cached.InkBounds=null;}return layout;
    }
    public static Rect TextContent(Annotation a)
    {
        double pad=TextStudio.Padding(a);
        if(a.Kind==AnnotationKind.Callout){var b=new Rect(a.Left,a.Top,Math.Max(36,a.Width),Math.Max(36,a.Height));double fraction=(1-TextStudio.ContentFraction(a.CalloutStyle))/2;double ix=b.Width*fraction,iy=b.Height*fraction;return new(b.X+pad+ix,b.Y+pad+iy,Math.Max(1,b.Width-2*(pad+ix)),Math.Max(1,b.Height-2*(pad+iy)));}
        if(a.Kind==AnnotationKind.Text&&a.TextFrame&&a.Width>=2&&a.Height>=2)return new(a.Left+pad,a.Top+pad,Math.Max(1,a.Width-2*pad),Math.Max(1,a.Height-2*pad));
        var text=TextLayout(a,a.Text);return new(a.X1+pad,a.Y1+pad,a.TextWidth>0?a.TextWidth:Math.Max(10,text.WidthIncludingTrailingWhitespace),Math.Max(a.FontSize,text.Height));
    }
    public static Point TextOrigin(Annotation a,FormattedText layout)
    {
        var inner=TextContent(a);double offset=a.Kind==AnnotationKind.Callout||a.Kind==AnnotationKind.Text&&a.TextFrame?Math.Max(0,inner.Height-layout.Height)*(int)a.VerticalAlignment/2:0;return new(inner.X,inner.Y+offset);
    }
    public static Rect TextInkBounds(Annotation a)
    {
        var text=TextLayout(a);var origin=TextOrigin(a,text);var logical=new Rect(origin.X,origin.Y,Math.Max(10,a.TextWidth>0&&a.Kind==AnnotationKind.Text?a.TextWidth:text.WidthIncludingTrailingWhitespace),Math.Max(a.FontSize,text.Height));
        var entry=(_layouts??=new()).GetOrCreateValue(a);var ink=entry.InkBounds??=text.BuildGeometry(new Point()).Bounds;if(!ink.IsEmpty)ink.Offset(origin.X,origin.Y);return ink.IsEmpty?logical:Rect.Union(logical,ink);
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
    private static void DrawStyledText(DrawingContext dc,Annotation a,bool suppressText=false)
    {
        var text=TextLayout(a);double pad=TextStudio.Padding(a);
        if(a.Kind==AnnotationKind.Text&&a.FillEnabled){var bg=a.TextFrame?new Rect(a.Left,a.Top,a.Width,a.Height):TextInkBounds(a);if(!a.TextFrame)bg.Inflate(pad,pad);dc.DrawRectangle(ShapeInk(a.FillColor,a.FillOpacity),null,bg);}
        bool framed=a.Kind==AnnotationKind.Callout||a.Kind==AnnotationKind.Text&&a.TextFrame&&a.Width>=2&&a.Height>=2;
        if(framed)dc.PushClip(new RectangleGeometry(TextContent(a)));
        if(!suppressText)dc.DrawText(text,TextOrigin(a,text));if(framed)dc.Pop();
    }
    [System.ThreadStatic] private static System.Runtime.CompilerServices.ConditionalWeakTable<Annotation,CalloutDrawingEntry>? _calloutDrawings;
    private sealed class CalloutDrawingEntry { public CalloutDrawingEntry(){} public object? Key;public DrawingGroup? Drawing;}
    private static void DrawCachedCallout(DrawingContext dc,Annotation a,bool suppressText)
    {
        var entry=(_calloutDrawings??=new()).GetOrCreateValue(a);
        object key=(LayoutKey(a),a.VerticalAlignment,a.TextPadding,CacheCoordinate(a.Width),CacheCoordinate(a.Height),CacheCoordinate(a.TailX-a.Left),CacheCoordinate(a.TailY-a.Top),a.CalloutStyle,a.CornerRadius,a.FillEnabled,a.FillColor,a.FillOpacity,a.StrokeEnabled,a.Color,a.Stroke,a.StrokeOpacity,a.Dash,suppressText);
        if(!Equals(entry.Key,key)||entry.Drawing==null){
            var local=a.Copy();local.Translate(-a.Left,-a.Top);var drawing=new DrawingGroup();
            using(var context=drawing.Open()){context.DrawGeometry(local.FillEnabled?ShapeInk(local.FillColor,local.FillOpacity):null,local.StrokeEnabled?ShapePen(local):null,CalloutGeometry(local));if(!suppressText)DrawStyledText(context,local);}
            drawing.Freeze();entry.Drawing=drawing;entry.Key=key;
        }
        dc.PushTransform(new TranslateTransform(a.Left,a.Top));dc.DrawDrawing(entry.Drawing);dc.Pop();
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
        if(a.CalloutStyle is CalloutStyle.RoundedSpeech or CalloutStyle.PillSpeech or CalloutStyle.DiamondSpeech or CalloutStyle.HexagonSpeech){
            var center=new Point(b.Left+b.Width/2,b.Top+b.Height/2);double angle=Math.Atan2(tail.Y-center.Y,tail.X-center.X);
            Point Boundary(double theta){var vector=new Vector(Math.Cos(theta),Math.Sin(theta));double lo=0,hi=Math.Max(b.Width,b.Height)*2;for(int i=0;i<40;i++){double mid=(lo+hi)/2;if(body.FillContains(center+vector*mid))lo=mid;else hi=mid;}return center+vector*Math.Max(0,lo-.5);}
            first=Boundary(angle-.15);last=Boundary(angle+.15);
        }
        var triangle=new StreamGeometry();using(var c=triangle.Open()){c.BeginFigure(first,true,true);c.LineTo(tail,true,false);c.LineTo(last,true,false);}return Geometry.Combine(body,triangle,GeometryCombineMode.Union,null);
    }
    public static void FitText(Annotation a,PixelRect view,bool preserveTail=false)
    {
        if(!TextStudio.IsText(a.Kind)||a.Kind==AnnotationKind.Step||!a.AutoFitText)return;
        if(a.Kind==AnnotationKind.Callout){
            if(a.Text.Length==0)return; // Keep an empty drawn frame exactly as placed; grow only when typing.
            bool pin=preserveTail&&a.CalloutBodyPivot&&a.CalloutStyle is not(CalloutStyle.Rectangle or CalloutStyle.RoundedRectangle or CalloutStyle.Pill or CalloutStyle.Ellipse);var old=new Rect(a.Left,a.Top,Math.Max(36,a.Width),Math.Max(36,a.Height));var tail=WorldPoint(a,new(a.TailX,a.TailY));bool below=pin&&a.TailY>old.Bottom,right=pin&&a.TailX>old.Right;
            double width=Math.Min(100000,Math.Max(Math.Max(36,a.Width),TextStudio.MinimumBodyWidth(a)));double left=right?old.Right-width:old.Left,top=old.Top;a.X1=left;a.Y1=top;a.X2=left+width;
            var text=TextLayout(a);double height=Math.Max(old.Height,TextStudio.FittedBodyHeight(a,text.Height,text.OverhangAfter));if(below)top=old.Bottom-height;a.Y1=top;a.Y2=top+height;
            if(pin){var local=LocalPoint(a,tail);a.TailX=local.X;a.TailY=local.Y;}

        }
    }
}
