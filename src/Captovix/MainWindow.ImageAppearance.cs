using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Captovix;
public partial class MainWindow
{
    private bool _syncImage;
    private string? _appearanceKey;
    private void SyncImageAppearance()
    {
        if(ImageStylePanel==null)return;
        var layout=Surface.Layout;var ready=_session!=null&&ReferenceEquals(_session,Surface.Session)&&layout!=null;
        ImageStylePanel.IsEnabled=ready;
        if(!ready){_appearanceKey=null;OutputSummary.Text="";return;}
        var d=_session!.Document;var s=d.Appearance;var l=layout!;
        OutputSummary.Text=$"Đầu ra {l.Width} × {l.Height} px · xoay {s.QuarterTurns*90}°";
        var key=(_activeItem?.Id??d.SourceHash)+d.Viewport+JsonSerializer.Serialize(s);if(key==_appearanceKey)return;_appearanceKey=key;_syncImage=true;
        try{
            ResizeW.Text=l.ImageWidth.ToString();ResizeH.Text=l.ImageHeight.ToString();
            CustomCanvas.IsChecked=s.CanvasWidth>0;CanvasW.Text=((int)l.Canvas.Width-s.ExtendLeft-s.ExtendRight).ToString();CanvasH.Text=((int)l.Canvas.Height-s.ExtendTop-s.ExtendBottom).ToString();CanvasAnchor.SelectedIndex=s.Anchor;
            FramePadding.Text=s.Padding.ToString();FrameBorder.Text=s.BorderWidth.ToString();FrameBorderColor.Text=s.BorderColor;
            TransparentBackground.IsChecked=s.Transparent;FrameBackground.Text=s.BackgroundColor;
            ShadowEnabled.IsChecked=s.Shadow;ShadowBlur.Text=s.ShadowBlur.ToString();ShadowX.Text=s.ShadowX.ToString();ShadowY.Text=s.ShadowY.ToString();ShadowOpacity.Text=s.ShadowOpacity.ToString();
        }finally{_syncImage=false;}
    }
    private void ApplyAppearance(Action<ImageAppearance> change,bool reset=false)
    {
        if(_session==null||Surface.IsInteracting)return;EndInlineText(true);Surface.CancelInteraction();_session.Commit();
        try{
            var candidate=reset?new ImageAppearance():_session.Document.Appearance.Copy();change(candidate);_=new ImageLayout(_session.Document.Viewport,candidate);
            if(Surface.Tool==EditorTool.Crop)SelectTool(EditorTool.Select);
            _session.Change(d=>d.Appearance=candidate);Surface.Refresh();SyncProperties();FitImage();SetStatus("Đã áp dụng biến đổi ảnh/khung. Ctrl+Z để hoàn tác.");
        }catch(Exception ex){SetStatus("Chưa áp dụng: "+ex.Message);}
    }
    private void AppearanceAction_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button {Tag:string tag})return;
        if(tag=="ResetAll"){
            if(MessageBox.Show(this,"Đặt lại xoay/lật, kích thước và kiểu khung? Chú thích và crop vẫn giữ nguyên.","Đặt lại trình bày ảnh",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;
            ApplyAppearance(s=>{},true);return;
        }
        ApplyAppearance(s=>{switch(tag){case "Left":s.Rotate(-1);break;case "Right":s.Rotate(1);break;case "FlipH":s.FlipHorizontal=!s.FlipHorizontal;break;case "FlipV":s.FlipVertical=!s.FlipVertical;break;case "Natural":s.ResizeWidth=s.ResizeHeight=0;break;}});
    }
    private static int ImageNumber(TextBox field,int min,int max)
    {
        if(!int.TryParse(field.Text,NumberStyles.Integer,CultureInfo.InvariantCulture,out int n)||n<min||n>max)throw new InvalidOperationException($"{field.ToolTip??"Giá trị"} cần là số nguyên từ {min} đến {max}.");return n;
    }
    private void ResizeInput_Changed(object sender,TextChangedEventArgs e)
    {
        if(_syncImage||_session==null||KeepRatio?.IsChecked!=true||ResizeW==null||ResizeH==null)return;
        if(!int.TryParse(((TextBox)sender).Text,out var n)||n<1)return;
        var l=Surface.Layout!;double ratio=(double)l.ImageWidth/l.ImageHeight;_syncImage=true;
        try{if(ReferenceEquals(sender,ResizeW))ResizeH.Text=Math.Clamp(Math.Round(n/ratio),1,32768).ToString(CultureInfo.InvariantCulture);else ResizeW.Text=Math.Clamp(Math.Round(n*ratio),1,32768).ToString(CultureInfo.InvariantCulture);}finally{_syncImage=false;}
    }
    private void ApplyResize_Click(object sender,RoutedEventArgs e)=>ApplyAppearance(s=>{s.ResizeWidth=ImageNumber(ResizeW,1,32768);s.ResizeHeight=ImageNumber(ResizeH,1,32768);});
    private void ApplyCanvas_Click(object sender,RoutedEventArgs e)=>ApplyAppearance(s=>{s.CanvasWidth=CustomCanvas.IsChecked==true?ImageNumber(CanvasW,1,32768):0;s.CanvasHeight=CustomCanvas.IsChecked==true?ImageNumber(CanvasH,1,32768):0;s.Anchor=Math.Clamp(CanvasAnchor.SelectedIndex,0,8);});
    private void ApplyFrame_Click(object sender,RoutedEventArgs e)=>ApplyAppearance(s=>{
        s.Padding=ImageNumber(FramePadding,0,512);s.BorderWidth=ImageNumber(FrameBorder,0,128);s.BorderColor=FrameBorderColor.Text.Trim().ToUpperInvariant();
        s.Transparent=TransparentBackground.IsChecked==true;s.BackgroundColor=FrameBackground.Text.Trim().ToUpperInvariant();
        s.Shadow=ShadowEnabled.IsChecked==true;s.ShadowBlur=ImageNumber(ShadowBlur,0,80);s.ShadowX=ImageNumber(ShadowX,-80,80);s.ShadowY=ImageNumber(ShadowY,-80,80);s.ShadowOpacity=ImageNumber(ShadowOpacity,0,100);
    });
    private void FramePreset_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button {Tag:string tag})return;
        ApplyAppearance(s=>{s.Padding=tag=="Clear"?0:32;s.BorderWidth=tag=="Clear"?0:1;s.BorderColor=tag=="Dark"?"#48566A":"#DCE3E9";s.Transparent=tag=="Clear";s.BackgroundColor=tag=="Dark"?"#182332":"#F4F7FA";s.Shadow=tag!="Clear";s.ShadowBlur=16;s.ShadowX=8;s.ShadowY=12;s.ShadowOpacity=25;});
    }
}
