using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Captovix;

public partial class MainWindow
{
    private void SyncVideoInk(VideoMark mark)
    {
        var style=mark.Style;
        var kind=mark.Kind;
        bool hasText=kind is VideoMarkKind.Text or VideoMarkKind.Callout or VideoMarkKind.Step;
        bool stroke=kind is VideoMarkKind.Frame or VideoMarkKind.Callout or VideoMarkKind.Arrow or VideoMarkKind.Freehand or VideoMarkKind.Step;
        VideoOutlineOptions.Visibility=stroke?Visibility.Visible:Visibility.Collapsed;
        VideoFillOptions.Visibility=(kind is VideoMarkKind.Frame or VideoMarkKind.Callout or VideoMarkKind.Text or VideoMarkKind.Step)?Visibility.Visible:Visibility.Collapsed;
        VideoTypeOptions.Visibility=hasText?Visibility.Visible:Visibility.Collapsed;
        VideoShapeOptions.Visibility=kind==VideoMarkKind.Frame?Visibility.Visible:Visibility.Collapsed;
        VideoArrowOptions.Visibility=kind==VideoMarkKind.Arrow?Visibility.Visible:Visibility.Collapsed;
        VideoCalloutOptions.Visibility=kind==VideoMarkKind.Callout?Visibility.Visible:Visibility.Collapsed;
        VideoStepOptions.Visibility=kind==VideoMarkKind.Step?Visibility.Visible:Visibility.Collapsed;
        VideoFocusOptions.Visibility=(kind is VideoMarkKind.Highlight or VideoMarkKind.Spotlight)?Visibility.Visible:Visibility.Collapsed;
        VideoMaskOptions.Visibility=kind==VideoMarkKind.Blur?Visibility.Visible:Visibility.Collapsed;
        VideoLensOptions.Visibility=kind==VideoMarkKind.Magnifier?Visibility.Visible:Visibility.Collapsed;
        VideoAngleOptions.Visibility=mark.SamplesVideo?Visibility.Collapsed:Visibility.Visible;
        VideoInkOpacity.Value=style.Opacity;
        VideoInkStrokeOpacity.Value=style.StrokeOpacity;
        VideoInkFillOpacity.Value=style.FillOpacity;
        VideoInkFocus.Value=style.Focus;
        VideoInkStrokeEnabled.IsChecked=style.StrokeEnabled;
        VideoInkFillEnabled.IsChecked=style.FillEnabled??(kind is VideoMarkKind.Callout or VideoMarkKind.Text or VideoMarkKind.Step);
        VideoInkFillColor.Text=style.Fill;
        VideoInkTextColor.Text=style.TextColor;
        VideoInkFont.Text=style.Font;
        VideoInkBold.IsChecked=style.Bold;
        VideoInkItalic.IsChecked=style.Italic;
        VideoInkUnderline.IsChecked=style.Underline;
        VideoInkDoubleArrow.IsChecked=style.DoubleArrow;
        VideoInkEllipse.IsChecked=style.Ellipse;
        VideoInkShape.SelectedIndex=style.Shape;
        VideoInkDash.SelectedIndex=style.Dash;
        VideoInkHead.SelectedIndex=style.ArrowHead;
        VideoInkCallout.SelectedIndex=style.Callout;
        VideoInkStep.SelectedIndex=style.Step;
        VideoInkEffect.SelectedIndex=style.Effect;
        VideoInkAlign.SelectedIndex=style.Alignment;
        VideoInkRadius.Text=Number(style.Radius);
        VideoInkAngle.Text=Number(style.Angle);
        VideoInkPadding.Text=Number(style.Padding);
        VideoInkStrength.Text=Number(style.Strength);
        VideoInkZoom.Text=Number(style.Zoom);
        PaintButton(VideoInkStrokePaint,"Màu nét",mark.Color??"#F54856");
        PaintButton(VideoInkFillPaint,"Màu nền",style.Fill);
        PaintButton(VideoInkTextPaint,"Màu chữ",style.TextColor);
    }

    private VideoInkStyle? ReadVideoInk()
    {
        if(!TryNumber(VideoInkRadius,out var radius)||!TryNumber(VideoInkAngle,out var angle)
            ||!TryNumber(VideoInkPadding,out var padding)||!TryNumber(VideoInkStrength,out var strength)
            ||strength<4||strength>64||!TryNumber(VideoInkZoom,out var zoom))return null;
        var style=new VideoInkStyle{
            Opacity=VideoInkOpacity.Value,StrokeOpacity=VideoInkStrokeOpacity.Value,FillOpacity=VideoInkFillOpacity.Value,
            Focus=VideoInkFocus.Value,StrokeEnabled=VideoInkStrokeEnabled.IsChecked==true,FillEnabled=VideoInkFillEnabled.IsChecked==true,
            Fill=VideoInkFillColor.Text.Trim().ToUpperInvariant(),TextColor=VideoInkTextColor.Text.Trim().ToUpperInvariant(),
            Font=VideoInkFont.Text.Trim(),Bold=VideoInkBold.IsChecked==true,Italic=VideoInkItalic.IsChecked==true,
            Underline=VideoInkUnderline.IsChecked==true,DoubleArrow=VideoInkDoubleArrow.IsChecked==true,Ellipse=VideoInkEllipse.IsChecked==true,
            Shape=VideoInkShape.SelectedIndex,Dash=VideoInkDash.SelectedIndex,ArrowHead=VideoInkHead.SelectedIndex,
            Callout=VideoInkCallout.SelectedIndex,Step=VideoInkStep.SelectedIndex,Effect=VideoInkEffect.SelectedIndex,
            Alignment=VideoInkAlign.SelectedIndex,Radius=radius,Angle=angle,Padding=padding,Strength=(int)Math.Round(strength),Zoom=zoom
        };
        return style.IsValid()?style:null;
    }

    private void VideoInkPaint_Click(object sender,RoutedEventArgs e)
    {
        if(!VideoCanEdit||sender is not Button{Tag:string target} button)return;
        var input=target switch{"fill"=>VideoInkFillColor,"text"=>VideoInkTextColor,_=>VideoTrackColor};
        OpenPaint(button,color=>{
            if(color==null)return;
            input.Text=color;
            PaintButton(button,target switch{"fill"=>"Màu nền","text"=>"Màu chữ",_=>"Màu nét"},color);
        },false);
    }
}
