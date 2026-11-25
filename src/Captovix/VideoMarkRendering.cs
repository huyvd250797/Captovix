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

    internal static VideoMarkKind? KindFor(AnnotationKind kind)
    {
        if(FrameKinds.Contains(kind))return VideoMarkKind.Frame;
        return kind switch{
            AnnotationKind.Text=>VideoMarkKind.Text,AnnotationKind.Callout=>VideoMarkKind.Callout,
            AnnotationKind.Arrow=>VideoMarkKind.Arrow,AnnotationKind.Step=>VideoMarkKind.Step,
            AnnotationKind.Blur or AnnotationKind.Pixelate or AnnotationKind.Cover=>VideoMarkKind.Blur,
            AnnotationKind.Highlight=>VideoMarkKind.Highlight,AnnotationKind.Spotlight=>VideoMarkKind.Spotlight,
            AnnotationKind.Magnifier=>VideoMarkKind.Magnifier,AnnotationKind.Freehand=>VideoMarkKind.Freehand,_=>null
        };
    }

    internal static VideoMark WithImageStyle(VideoMark mark,Annotation annotation)
    {
        var shadow=annotation.Shadow;
        return mark with{
            Color=annotation.Color,FontSize=Math.Clamp(annotation.FontSize,8,240),Stroke=Math.Clamp(annotation.Stroke,1,40),
            Style=new VideoInkStyle{
                Opacity=annotation.ObjectOpacity,StrokeOpacity=annotation.StrokeOpacity,FillOpacity=annotation.FillOpacity,
                StrokeEnabled=annotation.StrokeEnabled,FillEnabled=annotation.FillEnabled,Fill=annotation.FillColor,
                TextColor=TextStudio.Foreground(annotation),Font=annotation.FontFamily,Bold=annotation.Bold,Italic=annotation.Italic,Underline=annotation.Underline,
                Shape=Math.Max(0,Array.IndexOf(FrameKinds,annotation.Kind)),Dash=(int)annotation.Dash,ArrowHead=(int)annotation.Tip,
                ArrowSize=Math.Clamp(annotation.ArrowHeadSize,0,160),DoubleArrow=annotation.DoubleArrow,Callout=(int)annotation.CalloutStyle,
                Step=(int)annotation.StepStyle,StepStrokeColor=annotation.StepStrokeColor,StepFillVisible=annotation.StepFillVisible,
                Effect=annotation.Kind switch{AnnotationKind.Pixelate=>1,AnnotationKind.Cover=>2,_=>0},
                Strength=Math.Clamp(annotation.EffectSize,4,64),Radius=Math.Clamp(annotation.CornerRadius,0,1000),
                Zoom=annotation.Magnification,Ellipse=annotation.RegionShape==RegionShape.Ellipse,Alignment=(int)annotation.TextAlignment,
                Padding=TextStudio.Padding(annotation),Focus=annotation.FocusOpacity,Vertical=(int)annotation.VerticalAlignment,
                LineSpacing=annotation.LineSpacing,Route=(int)annotation.Route,StarPoints=annotation.StarPoints,StarInset=annotation.StarInset,
                ShadowEnabled=shadow.Enabled,ShadowDirection=shadow.Direction,ShadowDistance=shadow.Distance,
                ShadowBlur=shadow.Blur,ShadowOpacity=shadow.Opacity,ShadowColor=shadow.Color
            }
        };
    }

    internal static Annotation AnnotationFor(VideoMark mark,double width,double height)
    {
        var style=mark.Style;
        double scale=height/540;
        var annotation=new Annotation{
            Kind=mark.Kind switch{
                VideoMarkKind.Frame=>FrameKinds[style.Shape],VideoMarkKind.Callout=>AnnotationKind.Callout,
                VideoMarkKind.Arrow=>AnnotationKind.Arrow,VideoMarkKind.Text or VideoMarkKind.Title=>AnnotationKind.Text,
                VideoMarkKind.Highlight=>AnnotationKind.Highlight,VideoMarkKind.Spotlight=>AnnotationKind.Spotlight,
                VideoMarkKind.Magnifier=>AnnotationKind.Magnifier,VideoMarkKind.Freehand=>AnnotationKind.Freehand,
                VideoMarkKind.Blur=>style.Effect switch{1=>AnnotationKind.Pixelate,2=>AnnotationKind.Cover,_=>AnnotationKind.Blur},
                _=>AnnotationKind.Step
            },
            X1=mark.X1*width,Y1=mark.Y1*height,X2=mark.X2*width,Y2=mark.Y2*height,Text=mark.Text,
            Number=mark.Number,Color=mark.Color??"#F54856",TextColor=style.TextColor,FillColor=style.Fill,
            FillEnabled=style.FillEnabled??(mark.Kind is VideoMarkKind.Callout or VideoMarkKind.Text),
            FontSize=mark.FontSize*scale,Stroke=mark.Stroke*scale,TextPadding=style.Padding*scale,
            TailX=mark.Tail is {} tail?tail.X*width:mark.X1*width-18*scale,
            TailY=mark.Tail is {} pin?pin.Y*height:mark.Y2*height+18*scale,
            CalloutBodyPivot=mark.Tail!=null,TextFrame=mark.TextFrame,VerticalAlignment=(TextVertical)style.Vertical,
            LineSpacing=style.LineSpacing,ArrowHeadSize=style.ArrowSize*scale,EffectSize=style.Strength,
            Route=(ConnectorRoute)style.Route,StarPoints=style.StarPoints,StarInset=style.StarInset,
            Shadow=new ObjectShadow{Enabled=style.ShadowEnabled,Direction=style.ShadowDirection,Distance=style.ShadowDistance*scale,
                Blur=style.ShadowBlur*scale,Opacity=style.ShadowOpacity,Color=style.ShadowColor},
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
            annotation.StepFillVisible=style.StepFillVisible??style.FillEnabled??true;
            annotation.StepStrokeColor=style.StepStrokeColor??style.TextColor;
        }
        return annotation;
    }

    internal static DrawingImage? Draw(VideoMark[] marks,double width,double height,VideoMark? editing=null)
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
                }else EditorRenderer.DrawAnnotation(dc,annotation,ReferenceEquals(mark,editing));
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
