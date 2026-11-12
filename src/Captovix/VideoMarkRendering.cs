using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Captovix;

internal static class VideoMarkRendering
{
    internal static Rect Region(VideoMark mark,double width,double height)=>
        new(new Point(mark.X1*width,mark.Y1*height),new Point(mark.X2*width,mark.Y2*height));

    private static readonly AnnotationKind[] FrameKinds=[AnnotationKind.Rectangle,AnnotationKind.RoundedRectangle,
        AnnotationKind.Ellipse,AnnotationKind.Triangle,AnnotationKind.Diamond,AnnotationKind.Trapezoid,
        AnnotationKind.Pentagon,AnnotationKind.Hexagon,AnnotationKind.Star,AnnotationKind.Line];

    internal static Annotation AnnotationFor(VideoMark mark,double width,double height)
    {
        var style=mark.Style;
        double scale=height/540;
        var annotation=new Annotation{
            Kind=mark.Kind switch{
                VideoMarkKind.Frame=>FrameKinds[style.Shape],VideoMarkKind.Callout=>AnnotationKind.Callout,
                VideoMarkKind.Arrow=>AnnotationKind.Arrow,VideoMarkKind.Text=>AnnotationKind.Text,
                VideoMarkKind.Highlight=>AnnotationKind.Highlight,VideoMarkKind.Spotlight=>AnnotationKind.Spotlight,
                VideoMarkKind.Magnifier=>AnnotationKind.Magnifier,VideoMarkKind.Freehand=>AnnotationKind.Freehand,_=>AnnotationKind.Step
            },
            X1=mark.X1*width,Y1=mark.Y1*height,X2=mark.X2*width,Y2=mark.Y2*height,Text=mark.Text,
            Number=mark.Number,Color=mark.Color??"#F54856",TextColor=style.TextColor,FillColor=style.Fill,
            FillEnabled=style.FillEnabled??(mark.Kind is VideoMarkKind.Callout or VideoMarkKind.Text),
            FontSize=mark.FontSize*scale,Stroke=mark.Stroke*scale,TextPadding=style.Padding*scale,
            TailX=mark.X1*width-18*scale,TailY=mark.Y2*height+18*scale,
            ObjectOpacity=style.Opacity,StrokeOpacity=style.StrokeOpacity,FillOpacity=style.FillOpacity,
            StrokeEnabled=style.StrokeEnabled,Dash=(StrokePattern)style.Dash,CornerRadius=style.Radius*scale,
            Rotation=style.Angle,FontFamily=style.Font,Bold=style.Bold,Italic=style.Italic,Underline=style.Underline,
            Tip=(ArrowTip)style.ArrowHead,DoubleArrow=style.DoubleArrow,CalloutStyle=(CalloutStyle)style.Callout,
            StepStyle=(StepStyle)style.Step,RegionShape=style.Ellipse?RegionShape.Ellipse:RegionShape.Rectangle,
            FocusOpacity=style.Focus,Magnification=style.Zoom,TextAlignment=(TextAlign)style.Alignment,
            Points=(mark.Points??[]).Select(point=>new AnnotationPoint(point.X*width,point.Y*height)).ToList()
        };
        if(mark.StepLabel is {} label&&TextStudio.TryStepNumber(label,out decimal step,out bool tenths)){
            annotation.Number=(int)step;
            annotation.StepDecimal=tenths?step:null;
        }
        if(mark.Kind==VideoMarkKind.Step&&style!=VideoInkStyle.Default){
            annotation.StepFillVisible=style.FillEnabled??true;
            annotation.StepStrokeColor=style.TextColor;
        }
        return annotation;
    }

    internal static DrawingImage? Draw(VideoMark[] marks,double width,double height)
    {
        if(!marks.Any(mark=>!mark.SamplesVideo))return null;
        var drawing=new DrawingGroup{ClipGeometry=new RectangleGeometry(new Rect(0,0,width,height))};
        using(var dc=drawing.Open()){
            dc.DrawRectangle(Brushes.Transparent,null,new Rect(0,0,width,height));
            foreach(var mark in marks.Where(mark=>!mark.SamplesVideo)){
                var annotation=AnnotationFor(mark,width,height);
                if(mark.Kind==VideoMarkKind.Spotlight){
                    var outside=new CombinedGeometry(GeometryCombineMode.Exclude,
                        new RectangleGeometry(new Rect(0,0,width,height)),EditorRenderer.RegionGeometry(annotation,true));
                    dc.PushOpacity(mark.Style.Opacity*mark.Style.Focus/10000);
                    dc.DrawGeometry(EditorRenderer.Ink(annotation.Color),null,outside);
                    dc.Pop();
                }else EditorRenderer.DrawAnnotation(dc,annotation);
            }
        }
        var image=new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    internal static bool WritePng(VideoMark[] marks,int width,int height,string path)
    {
        var image=Draw(marks,width,height);
        if(image==null)return false;
        var visual=new DrawingVisual();
        using(var dc=visual.RenderOpen())dc.DrawImage(image,new Rect(0,0,width,height));
        var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder=new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream=File.Create(path);
        encoder.Save(stream);
        return true;
    }
}
