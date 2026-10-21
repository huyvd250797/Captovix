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
        if(a.Kind==AnnotationKind.Callout)return Rect.Union(new Rect(a.Left,a.Top,a.Width,a.Height),new Rect(a.TailX,a.TailY,1,1));
        if(a.Kind==AnnotationKind.Freehand)return new(a.Left-a.Stroke/2,a.Top-a.Stroke/2,Math.Max(1,a.Width)+a.Stroke,Math.Max(1,a.Height)+a.Stroke);
        if(a.Kind==AnnotationKind.Text){var text=TextLayout(a);return new(a.X1,a.Y1,Math.Max(10,text.WidthIncludingTrailingWhitespace),Math.Max(a.FontSize,text.Height));}
        if(a.Kind==AnnotationKind.Step){var r=a.FontSize*.8;return new(a.X1-r,a.Y1-r,r*2,r*2);}
        return new(a.Left,a.Top,Math.Max(1,a.Width),Math.Max(1,a.Height));
    }
    public static AnnotationBox Box(Annotation a){var b=Bounds(a);return new(b.X,b.Y,b.Width,b.Height);}
    public static void Draw(DrawingContext dc,BitmapSource source,EditorDocument doc,IReadOnlySet<string>? draftIds=null)
    {
        var view=doc.Viewport;
        dc.PushClip(new RectangleGeometry(new Rect(view.X,view.Y,view.Width,view.Height)));
        dc.DrawImage(source,new Rect(0,0,doc.OriginalWidth,doc.OriginalHeight));
        foreach(var a in doc.Annotations.Where(a=>!a.IsRedaction))DrawAnnotation(dc,a);
        // Masks cover all annotations. Solid covers always dominate overlapping filters.
        foreach(var a in doc.Annotations.Where(a=>a.IsRedaction).OrderBy(a=>a.Kind==AnnotationKind.Cover))
            RedactionRenderer.Draw(dc,source,a,draftIds?.Contains(a.Id)==true);
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
            case AnnotationKind.Freehand:
                if(a.Points.Count==1){dc.DrawEllipse(brush,null,new(a.Points[0].X,a.Points[0].Y),a.Stroke/2,a.Stroke/2);break;}
                if(a.Points.Count>1){var path=new StreamGeometry();using(var ctx=path.Open()){ctx.BeginFigure(new(a.Points[0].X,a.Points[0].Y),false,false);ctx.PolyLineTo(a.Points.Skip(1).Select(p=>new Point(p.X,p.Y)).ToArray(),true,false);}path.Freeze();dc.DrawGeometry(null,pen,path);}break;
            case AnnotationKind.Callout:
                dc.DrawGeometry(Brushes.White,pen,CalloutGeometry(a));
                var inner=new Rect(rect.Left+12,rect.Top+12,Math.Max(1,rect.Width-24),Math.Max(1,rect.Height-24));
                var label=TextLayout(a);label.MaxTextWidth=inner.Width;
                dc.PushClip(new RectangleGeometry(inner));dc.DrawText(label,inner.TopLeft);dc.Pop();break;
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
    public static Geometry CalloutGeometry(Annotation a)
    {
        var b=new Rect(a.Left,a.Top,Math.Max(36,a.Width),Math.Max(36,a.Height));var tail=new Point(a.TailX,a.TailY);
        var vertices=new List<Point>{b.TopLeft};int edge=-1;
        if(!b.Contains(tail)){var dx=(tail.X-(b.Left+b.Width/2))/(b.Width/2);var dy=(tail.Y-(b.Top+b.Height/2))/(b.Height/2);edge=Math.Abs(dx)>Math.Abs(dy)?dx>0?1:3:dy>0?2:0;}
        double cx=Math.Clamp(tail.X,b.Left+12,b.Right-12),cy=Math.Clamp(tail.Y,b.Top+12,b.Bottom-12);
        if(edge==0)vertices.AddRange([new(cx-8,b.Top),tail,new(cx+8,b.Top)]);vertices.Add(b.TopRight);
        if(edge==1)vertices.AddRange([new(b.Right,cy-8),tail,new(b.Right,cy+8)]);vertices.Add(b.BottomRight);
        if(edge==2)vertices.AddRange([new(cx+8,b.Bottom),tail,new(cx-8,b.Bottom)]);vertices.Add(b.BottomLeft);
        if(edge==3)vertices.AddRange([new(b.Left,cy+8),tail,new(b.Left,cy-8)]);
        var g=new StreamGeometry();using(var c=g.Open()){c.BeginFigure(vertices[0],true,true);c.PolyLineTo(vertices.Skip(1).ToArray(),true,false);}g.Freeze();return g;
    }
    private static double Distance(Point p,Point start,Point end)
    {
        var delta=end-start;if(delta.LengthSquared<.001)return(p-start).Length;
        return(p-(start+delta*Math.Clamp(Vector.Multiply(p-start,delta)/delta.LengthSquared,0,1))).Length;
    }
    public static Matrix LayoutMatrix(ImageLayout l,double zoom=1)=>new(l.A*zoom,l.B*zoom,l.C*zoom,l.D*zoom,l.Tx*zoom,l.Ty*zoom);
    public static Rect RectOf(ImageRect r)=>new(r.X,r.Y,r.Width,r.Height);
    public static void DrawFrame(DrawingContext dc,ImageLayout layout,ImageAppearance style)
    {
        var frame=RectOf(layout.Frame);
        if(style.Shadow&&style.ShadowOpacity>0){
            const int layers=20;var ink=new SolidColorBrush(Color.FromArgb((byte)Math.Clamp(Math.Round(255d*style.ShadowOpacity/100/layers),1,255),0,0,0));ink.Freeze();
            for(int i=layers;i>=1;i--){var r=frame;r.Offset(style.ShadowX,style.ShadowY);r.Inflate(style.ShadowBlur*i/(double)layers,style.ShadowBlur*i/(double)layers);dc.DrawRectangle(ink,null,r);}
        }
        if(!style.Transparent)dc.DrawRectangle(Ink(style.BackgroundColor),null,frame);
        if(style.BorderWidth>0){var r=frame;double b=style.BorderWidth;r.Inflate(-b/2,-b/2);dc.DrawRectangle(null,new Pen(Ink(style.BorderColor),b),r);}
    }
    public static void Compose(DrawingContext dc,BitmapSource source,EditorDocument doc)
    {
        var l=new ImageLayout(doc.Viewport,doc.Appearance);DrawFrame(dc,l,doc.Appearance);
        dc.PushClip(new RectangleGeometry(RectOf(l.Canvas)));dc.PushTransform(new MatrixTransform(LayoutMatrix(l)));Draw(dc,source,doc);dc.Pop();dc.Pop();
    }
    public static BitmapSource Render(BitmapSource source,EditorDocument doc,int maxWidth=0)
    {
        var layout=new ImageLayout(doc.Viewport,doc.Appearance);
        double scale=maxWidth>0?Math.Min(1,(double)maxWidth/layout.Width):1;
        int width=Math.Max(1,(int)Math.Round(layout.Width*scale)),height=Math.Max(1,(int)Math.Round(layout.Height*scale));
        var visual=new DrawingVisual();RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.HighQuality);
        using(var dc=visual.RenderOpen()){dc.PushTransform(new ScaleTransform(scale,scale));Compose(dc,source,doc);dc.Pop();}
        var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();return bitmap;
    }
    public static bool Hit(Annotation a,Point p,double tolerance)
    {
        if(a.Kind==AnnotationKind.Freehand){for(int i=0;i<a.Points.Count;i++){var v=a.Points[i];var u=a.Points[Math.Max(0,i-1)];if(Distance(p,new(u.X,u.Y),new(v.X,v.Y))<=tolerance+a.Stroke/2)return true;}return false;}
        if(a.Kind==AnnotationKind.Callout)return CalloutGeometry(a).FillContains(p)||CalloutGeometry(a).StrokeContains(new Pen(Brushes.Black,tolerance*2+a.Stroke),p);
        if(a.Kind!=AnnotationKind.Arrow){
            var r=Bounds(a);r.Inflate(tolerance,tolerance);if(!r.Contains(p))return false;
            if(a.Kind==AnnotationKind.Rectangle){var inside=Bounds(a);inside.Inflate(-Math.Min(inside.Width/2,tolerance+a.Stroke/2),-Math.Min(inside.Height/2,tolerance+a.Stroke/2));return !inside.Contains(p);}
            if(a.Kind==AnnotationKind.Ellipse){var b=Bounds(a);var rx=b.Width/2;var ry=b.Height/2;var dx=p.X-(b.Left+rx);var dy=p.Y-(b.Top+ry);var pad=tolerance+a.Stroke/2;var outside=dx*dx/Math.Pow(rx+pad,2)+dy*dy/Math.Pow(ry+pad,2);var innerX=rx-pad;var innerY=ry-pad;return outside<=1&&(innerX<=0||innerY<=0||dx*dx/(innerX*innerX)+dy*dy/(innerY*innerY)>=1);}
            return true;
        }
        var start=new Point(a.X1,a.Y1);var end=new Point(a.X2,a.Y2);var d=end-start;
        if(d.LengthSquared<.01)return (p-start).Length<=tolerance;
        var t=Math.Clamp(Vector.Multiply(p-start,d)/d.LengthSquared,0,1);return (p-(start+d*t)).Length<=tolerance+a.Stroke;
    }
}
