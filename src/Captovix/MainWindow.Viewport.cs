using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Captovix;
public partial class MainWindow
{
    private bool _spaceHeld,_panActive,_syncCrop;
    private Point _panStart;
    private double _panX,_panY;
    private void InitializeViewport()
    {
        PreviewMouseLeftButtonDown+=(_,e)=>{
            if(_inlineId==null)return;var point=e.GetPosition(InlineText);
            if(new Rect(0,0,InlineText.ActualWidth,InlineText.ActualHeight).Contains(point))return;
            EndInlineText(true);
        };
        Surface.CropDraftChanged+=SyncCropProperties;Surface.CanvasCommitted+=()=>Dispatcher.BeginInvoke(()=>{if(_fitMode)FitImage();});Surface.CanvasLimitReached+=message=>SetStatus("Không thể mở rộng thêm: "+message);
        Surface.CanvasGrown+=(left,top)=>{ImageScroll.UpdateLayout();ImageScroll.ScrollToHorizontalOffset(ImageScroll.HorizontalOffset+left*Surface.Zoom);ImageScroll.ScrollToVerticalOffset(ImageScroll.VerticalOffset+top*Surface.Zoom);};
        ImageScroll.PreviewMouseWheel+=Viewport_Wheel;
        ImageScroll.PreviewMouseLeftButtonDown+=Viewport_PanDown;
        ImageScroll.PreviewMouseMove+=Viewport_PanMove;
        ImageScroll.PreviewMouseLeftButtonUp+=(_,e)=>{if(_panActive){EndPan();e.Handled=true;}};
        ImageScroll.LostMouseCapture+=(_,_)=>{if(_panActive)EndPan();};
        Deactivated+=(_,_)=>{_spaceHeld=false;EndPan();Surface.CancelInteraction();};
    }
    private void ZoomAt(double zoom,Point anchor)
    {
        if(_session==null||Surface.IsInteracting||_panActive)return;
        ImageScroll.UpdateLayout();var origin=Surface.TranslatePoint(new(0,0),ImageScroll);
        var source=new Point((anchor.X-origin.X-EditorSurface.WorkspacePad)/Surface.Zoom,(anchor.Y-origin.Y-EditorSurface.WorkspacePad)/Surface.Zoom);
        _fitMode=false;_actualPixels=false;Surface.SetZoom(zoom);ImageScroll.UpdateLayout();
        origin=Surface.TranslatePoint(new(0,0),ImageScroll);
        ImageScroll.ScrollToHorizontalOffset(EditorViewportMath.AnchoredOffset(ImageScroll.HorizontalOffset,origin.X+EditorSurface.WorkspacePad,source.X,Surface.Zoom,anchor.X,ImageScroll.ScrollableWidth));
        ImageScroll.ScrollToVerticalOffset(EditorViewportMath.AnchoredOffset(ImageScroll.VerticalOffset,origin.Y+EditorSurface.WorkspacePad,source.Y,Surface.Zoom,anchor.Y,ImageScroll.ScrollableHeight));
        ImageScroll.UpdateLayout();
    }
    private void ZoomAtCenter(double zoom)=>ZoomAt(zoom,new(ImageScroll.ViewportWidth/2,ImageScroll.ViewportHeight/2));
    private void Viewport_Wheel(object sender,MouseWheelEventArgs e)
    {
        if(_session==null||ImageScroll.Visibility!=Visibility.Visible)return;e.Handled=true;
        if(Surface.IsInteracting||_panActive)return;
        var intent=EditorViewportMath.Intent(Keyboard.Modifiers.HasFlag(ModifierKeys.Control),Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
        if(intent==WheelIntent.Zoom)ZoomAt(EditorViewportMath.WheelZoom(Surface.Zoom,e.Delta),e.GetPosition(ImageScroll));
        else if(intent==WheelIntent.HorizontalScroll)ImageScroll.ScrollToHorizontalOffset(EditorViewportMath.WheelOffset(ImageScroll.HorizontalOffset,e.Delta,ImageScroll.ScrollableWidth));
        else ImageScroll.ScrollToVerticalOffset(EditorViewportMath.WheelOffset(ImageScroll.VerticalOffset,e.Delta,ImageScroll.ScrollableHeight));
    }
    private bool ViewportKey(KeyEventArgs e,bool typing)
    {
        if(_panActive){if(e.Key==Key.Escape){_spaceHeld=false;EndPan(true);}e.Handled=true;return true;}
        if(Keyboard.FocusedElement is ComboBox or ComboBoxItem)return false;
        if(e.Key==Key.Space&&!typing&&Keyboard.FocusedElement is not (ComboBox or ComboBoxItem)&&_session!=null&&!_busy&&!Surface.IsInteracting&&(ImageScroll.IsMouseOver||ImageScroll.IsKeyboardFocusWithin)){
            _spaceHeld=true;Mouse.OverrideCursor=Cursors.Hand;e.Handled=true;return true;
        }
        if(Surface.Tool==EditorTool.Crop&&_session!=null){
            if(!typing&&e.Key==Key.A&&Keyboard.Modifiers.HasFlag(ModifierKeys.Control)){Surface.SetCropDraft(Surface.CropBounds);e.Handled=true;return true;}
            if(e.Key==Key.Escape){Surface.CancelInteraction();SelectTool(EditorTool.Select);e.Handled=true;return true;}
            if(e.Key==Key.Enter&&!typing&&!Surface.IsInteracting){ApplyCropFromFields();e.Handled=true;return true;}
            if(!typing&&e.Key is Key.Left or Key.Right or Key.Up or Key.Down){
                double n=Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)?10:1;Surface.NudgeCrop(e.Key==Key.Left?-n:e.Key==Key.Right?n:0,e.Key==Key.Up?-n:e.Key==Key.Down?n:0);e.Handled=true;return true;
            }
        }
        return false;
    }
    private void Editor_KeyUp(object sender,KeyEventArgs e)
    {
        if(e.Key!=Key.Space||!_spaceHeld)return;_spaceHeld=false;EndPan();e.Handled=true;
    }
    private void Viewport_PanDown(object sender,MouseButtonEventArgs e)
    {
        if(!_spaceHeld||_session==null||Surface.IsInteracting||_busy||IsScrollBarSource(e.OriginalSource))return;
        _panStart=e.GetPosition(ImageScroll);_panX=ImageScroll.HorizontalOffset;_panY=ImageScroll.VerticalOffset;
        if(!ImageScroll.CaptureMouse())return;_panActive=true;Surface.Focus();Mouse.OverrideCursor=Cursors.ScrollAll;
        if(ImageScroll.ScrollableWidth>0||ImageScroll.ScrollableHeight>0)_fitMode=false;e.Handled=true;
    }
    private bool IsScrollBarSource(object source){
        for(var node=source as DependencyObject;node!=null&&node!=ImageScroll;node=node is Visual?VisualTreeHelper.GetParent(node):LogicalTreeHelper.GetParent(node))if(node is ScrollBar)return true;return false;
    }
    private void Viewport_PanMove(object sender,MouseEventArgs e)
    {
        if(!_panActive)return;var p=e.GetPosition(ImageScroll);
        ImageScroll.ScrollToHorizontalOffset(EditorViewportMath.PanOffset(_panX,_panStart.X,p.X,ImageScroll.ScrollableWidth));
        ImageScroll.ScrollToVerticalOffset(EditorViewportMath.PanOffset(_panY,_panStart.Y,p.Y,ImageScroll.ScrollableHeight));e.Handled=true;
    }
    private void EndPan(bool restore=false)
    {
        var active=_panActive;_panActive=false;
        if(active){if(restore){ImageScroll.ScrollToHorizontalOffset(_panX);ImageScroll.ScrollToVerticalOffset(_panY);}ImageScroll.ReleaseMouseCapture();}
        Mouse.OverrideCursor=_spaceHeld?Cursors.Hand:null;
    }
    private void SyncCropProperties()
    {
        if(CropProperties==null)return;
        bool show=_session!=null&&Surface.Session!=null&&Surface.Tool==EditorTool.Crop;
        CropProperties.Visibility=show?Visibility.Visible:Visibility.Collapsed;if(!show)return;
        var rect=Surface.CropDraft??Surface.CropBounds;_syncCrop=true;
        try{CropX.Text=rect.X.ToString(CultureInfo.InvariantCulture);CropY.Text=rect.Y.ToString(CultureInfo.InvariantCulture);CropWidth.Text=rect.Width.ToString(CultureInfo.InvariantCulture);CropHeight.Text=rect.Height.ToString(CultureInfo.InvariantCulture);}
        finally{_syncCrop=false;}
        CropHint.Text=Surface.CropDraft==null?"Kéo tay nắm để chọn vùng. Enter hoặc Áp dụng để cắt.":$"Đang chọn {rect.Width} × {rect.Height} px. Áp dụng để hoàn tất.";
    }
    private bool PreviewCropFields()
    {
        if(_session==null||_syncCrop)return false;
        var inputs=new[]{CropX.Text,CropY.Text,CropWidth.Text,CropHeight.Text};var values=new int[4];
        for(int i=0;i<4;i++)if(!int.TryParse(inputs[i],NumberStyles.Integer,CultureInfo.InvariantCulture,out values[i])){SetStatus("Crop: hãy nhập tọa độ và kích thước bằng số nguyên.");return false;}
        if(!Surface.SetCropDraft(new(values[0],values[1],values[2],values[3]))){SetStatus("Crop cần ít nhất 2 × 2 px và nằm trong khung ảnh/crop hiện tại.");return false;}return true;
    }
    private bool ApplyCropFromFields(){if(!PreviewCropFields())return false;if(Surface.ApplyCropDraft())return true;SetStatus("Chưa áp dụng crop: kiểm tra giới hạn kích thước đầu ra/khung ảnh.");return false;}
    private bool PrepareCropForExport(){EndInlineText(true);return _session==null||Surface.Tool!=EditorTool.Crop||ApplyCropFromFields();}
    private void PreviewCrop_Click(object sender,RoutedEventArgs e)=>PreviewCropFields();
    private void ApplyCrop_Click(object sender,RoutedEventArgs e)=>ApplyCropFromFields();
    private void CancelCrop_Click(object sender,RoutedEventArgs e){Surface.CancelInteraction();SelectTool(EditorTool.Select);Surface.Focus();}
    private void RestartCrop_Click(object sender,RoutedEventArgs e){Surface.StartCrop();Surface.Focus();}
    private void CropField_KeyDown(object sender,KeyEventArgs e)
    {
        if(e.Key!=Key.Enter)return;ApplyCropFromFields();e.Handled=true;
    }
}
