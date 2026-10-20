using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Captovix;
public static class EditorRenderer
{
    public static Brush Ink(string color)=>new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    public static FormattedText TextLayout(Annotation a,string? text=null,Brush? ink=null)=>new(text??a.Text,CultureInfo.CurrentUICulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),a.FontSize,ink??Ink(a.Color),1.0);
    public static Rect Bounds(Annotation a)
    {
        if(a.Kind==AnnotationKind.Text){var text=TextLayout(a);return new(a.X1,a.Y1,Math.Max(10,text.WidthIncludingTrailingWhitespace),Math.Max(a.FontSize,text.Height));}
        if(a.Kind==AnnotationKind.Step){var r=a.FontSize*.8;return new(a.X1-r,a.Y1-r,r*2,r*2);}
        return new(a.Left,a.Top,Math.Max(1,a.Width),Math.Max(1,a.Height));
    }
    public static void Draw(DrawingContext dc,BitmapSource source,EditorDocument doc)
    {
        var view=doc.Viewport;
        dc.PushClip(new RectangleGeometry(new Rect(view.X,view.Y,view.Width,view.Height)));
        dc.DrawImage(source,new Rect(0,0,doc.OriginalWidth,doc.OriginalHeight));
        foreach(var a in doc.Annotations)DrawAnnotation(dc,a);
        dc.Pop();
    }
    public static void DrawAnnotation(DrawingContext dc,Annotation a)
    {
        var brush=Ink(a.Color);var pen=new Pen(brush,a.Stroke){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,LineJoin=PenLineJoin.Round};
        var p1=new Point(a.X1,a.Y1);var p2=new Point(a.X2,a.Y2);var rect=new Rect(a.Left,a.Top,Math.Max(1,a.Width),Math.Max(1,a.Height));
        switch(a.Kind){
            case AnnotationKind.Arrow:
                var delta=p2-p1;var length=delta.Length;if(length<.01)return;delta.Normalize();
                var head=Math.Min(Math.Max(12,a.Stroke*4),length*.5);var back=p2-delta*head;var side=new Vector(-delta.Y,delta.X)*head*.45;
                dc.DrawLine(pen,p1,back+delta*a.Stroke);
                var triangle=new StreamGeometry();using(var ctx=triangle.Open()){ctx.BeginFigure(p2,true,true);ctx.LineTo(back+side,true,false);ctx.LineTo(back-side,true,false);}dc.DrawGeometry(brush,null,triangle);break;
            case AnnotationKind.Rectangle:dc.DrawRectangle(null,pen,rect);break;
            case AnnotationKind.Ellipse:dc.DrawEllipse(null,pen,new(rect.Left+rect.Width/2,rect.Top+rect.Height/2),rect.Width/2,rect.Height/2);break;
            case AnnotationKind.Highlight:
                var color=(Color)ColorConverter.ConvertFromString(a.Color);color.A=78;dc.DrawRectangle(new SolidColorBrush(color),null,rect);break;
            case AnnotationKind.Text:dc.DrawText(TextLayout(a),p1);break;
            case AnnotationKind.Step:
                var radius=a.FontSize*.8;dc.DrawEllipse(brush,new Pen(Brushes.White,Math.Max(1,a.Stroke/2)),p1,radius,radius);
                var number=TextLayout(a,a.Number.ToString(CultureInfo.InvariantCulture),Brushes.White);dc.DrawText(number,new(p1.X-number.Width/2,p1.Y-number.Height/2));break;
        }
    }
    public static BitmapSource Render(BitmapSource source,EditorDocument doc,int maxWidth=0)
    {
        var view=doc.Viewport;
        if(view.IsEmpty)throw new InvalidOperationException("Vùng ảnh trống.");
        double scale=maxWidth>0?Math.Min(1,(double)maxWidth/view.Width):1;
        int width=Math.Max(1,(int)Math.Round(view.Width*scale)),height=Math.Max(1,(int)Math.Round(view.Height*scale));
        if((long)width*height>100_000_000)throw new InvalidOperationException("Ảnh xuất vượt 100 triệu pixel. Hãy crop vùng cần dùng trước.");
        var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
            dc.PushTransform(new ScaleTransform(scale,scale));dc.PushTransform(new TranslateTransform(-view.X,-view.Y));Draw(dc,source,doc);dc.Pop();dc.Pop();
        }
        var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();return bitmap;
    }
    public static bool Hit(Annotation a,Point p,double tolerance)
    {
        if(a.Kind!=AnnotationKind.Arrow){var r=Bounds(a);r.Inflate(tolerance,tolerance);return r.Contains(p);}
        var start=new Point(a.X1,a.Y1);var end=new Point(a.X2,a.Y2);var d=end-start;
        if(d.LengthSquared<.01)return (p-start).Length<=tolerance;
        var t=Math.Clamp(Vector.Multiply(p-start,d)/d.LengthSquared,0,1);return (p-(start+d*t)).Length<=tolerance+a.Stroke;
    }
}
