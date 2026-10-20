using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Captovix;
public partial class MainWindow
{
    private readonly DispatcherTimer _saveTimer=new(){Interval=TimeSpan.FromMilliseconds(550)};
    private readonly DispatcherTimer _textTimer=new(){Interval=TimeSpan.FromMilliseconds(700)};
    private EditorSession? _session;
    private MediaItem? _activeItem;
    private bool _syncProperties,_changingSelection,_fitMode=true,_actualPixels;
    private void InitializeEditor()
    {
        _saveTimer.Tick+=(_,_)=>{_saveTimer.Stop();if(_session?.InTransaction==true)return;FlushEdits(false);};
        _textTimer.Tick+=(_,_)=>{_textTimer.Stop();_session?.Commit();};
        Surface.SelectionChanged+=SyncProperties;
        Surface.VisualChanged+=()=>{if(_session is {} s){var v=s.Document.Viewport;ImageSizeLabel.Text=$"{v.Width} × {v.Height} px · {s.Document.Annotations.Count} chú thích";RestoreCropButton.IsEnabled=s.Document.Crop!=null;}ZoomLabel.Text=$"{Surface.Zoom*VisualTreeHelper.GetDpi(this).DpiScaleX*100:F0}%";};
        Surface.TextRequested+=()=>{AnnotationText.Focus();AnnotationText.SelectAll();};
        Surface.CropFinished+=()=>{SelectTool(EditorTool.Select);if(_fitMode)FitImage();};
        SelectTool(EditorTool.Select);
    }
    private void Session_Changed()
    {
        SavedLabel.Text="Đang lưu…";SavedLabel.Foreground=Brushes.DarkOrange;
        UndoButton.IsEnabled=_session?.CanUndo==true;RedoButton.IsEnabled=_session?.CanRedo==true;
        Surface.Refresh();SyncProperties();_saveTimer.Stop();_saveTimer.Start();
    }
    private bool FlushEdits(bool report=true)
    {
        _textTimer.Stop();_session?.Commit();_saveTimer.Stop();
        if(_session==null||_activeItem==null||Surface.Original==null||!_session.Dirty)return true;
        try{
            DocumentStore.Save(_activeItem,_session,Surface.Original);SavedLabel.Text="✓ Đã lưu";SavedLabel.Foreground=new SolidColorBrush(Color.FromRgb(35,148,110));return true;
        }catch(Exception ex){App.Log(ex);SavedLabel.Text="Chưa lưu · Ctrl+S thử lại";SavedLabel.Foreground=Brushes.Firebrick;SetStatus("Không lưu được chú thích: "+ex.Message);if(report)MessageBox.Show(this,"Chưa lưu được phần chỉnh sửa. Nội dung vẫn đang ở Editor; hãy kiểm tra dung lượng/quyền thư mục và nhấn Ctrl+S.\n\n"+ex.Message,"Captovix",MessageBoxButton.OK,MessageBoxImage.Warning);return false;}
    }
    private void UnloadEditor()
    {
        _textTimer.Stop();_saveTimer.Stop();if(_session!=null)_session.Changed-=Session_Changed;
        Surface.Load(null,null);_session=null;_activeItem=null;UndoButton.IsEnabled=RedoButton.IsEnabled=false;
    }
    private void OpenImage(MediaItem item)
    {
        var bitmap=MediaStore.LoadImage(item.FilePath);var session=DocumentStore.Load(item,bitmap,out var warning);
        _activeItem=item;_session=session;session.Changed+=Session_Changed;
        ImageScroll.Visibility=ImageTools.Visibility=ZoomTools.Visibility=EditorProperties.Visibility=Visibility.Visible;
        Surface.Load(bitmap,session);SelectTool(EditorTool.Select);_fitMode=true;
        Dispatcher.BeginInvoke(FitImage,DispatcherPriority.Loaded);
        if(warning!=null){SetStatus("Bản chú thích cũ chưa được áp dụng; đã giữ bản sao.");MessageBox.Show(this,warning,"Kiểm tra dữ liệu chỉnh sửa",MessageBoxButton.OK,MessageBoxImage.Warning);}
    }
    private void SelectTool(EditorTool tool)
    {
        if(Surface==null)return;Surface.CancelInteraction();Surface.Tool=tool;Surface.Cursor=tool==EditorTool.Select?Cursors.Arrow:Cursors.Cross;
        foreach(var button in ToolBox.Children.OfType<Button>()){
            var selected=button.Tag?.ToString()==tool.ToString();button.Background=selected?(Brush)FindResource("Accent"):Brushes.White;button.Foreground=selected?Brushes.White:new SolidColorBrush(Color.FromRgb(36,51,72));
        }
        ToolHint.Text=tool switch{
            EditorTool.Select=>"Click chọn • Kéo để di chuyển • Kéo tay nắm đổi đầu mũi tên/khung • Delete để xóa",
            EditorTool.Text=>"Click vị trí trên ảnh rồi nhập chữ ở bảng thuộc tính bên phải.",
            EditorTool.Step=>"Click để đặt số bước tự tăng. Có thể sửa số ở bảng thuộc tính.",
            EditorTool.Crop=>"Kéo vùng muốn giữ và thả chuột để crop • Ctrl+Z hoặc Khôi phục khung ảnh gốc để lấy lại.",
            EditorTool.Highlight=>"Kéo phủ vùng cần làm nổi bật. Chọn màu ở bảng thuộc tính.",_=>"Kéo để vẽ đối tượng • Chọn công cụ Chọn (V) để sửa lại."};
    }
    private void Tool_Click(object sender,RoutedEventArgs e){if(sender is Button {Tag:string tool}){_session?.Commit();SelectTool(Enum.Parse<EditorTool>(tool));Surface.Focus();}}
    private void SyncProperties()
    {
        if(Surface==null||StrokePicker==null||FontPicker==null)return;_syncProperties=true;
        try{
            var a=Surface.Selected;DeleteAnnotationButton.IsEnabled=a!=null;PropertyTitle.Text=a==null?"Thuộc tính chú thích":a.Kind switch{AnnotationKind.Arrow=>"Mũi tên",AnnotationKind.Rectangle=>"Khung chữ nhật",AnnotationKind.Ellipse=>"Hình ellipse",AnnotationKind.Text=>"Chữ",AnnotationKind.Highlight=>"Highlight",_=>"Số bước"};
            if(a!=null){Surface.InkColor=a.Color;Surface.StrokeSize=a.Stroke;Surface.TextSize=a.FontSize;SelectValue(StrokePicker,a.Stroke);SelectValue(FontPicker,a.FontSize);}
            foreach(var b in ColorPalette.Children.OfType<Button>())b.BorderThickness=new Thickness(b.Tag?.ToString()==Surface.InkColor?3:1);
            TextProperties.Visibility=a?.Kind==AnnotationKind.Text?Visibility.Visible:Visibility.Collapsed;
            StepProperties.Visibility=a?.Kind==AnnotationKind.Step?Visibility.Visible:Visibility.Collapsed;
            if(a?.Kind==AnnotationKind.Text&&AnnotationText.Text!=a.Text)AnnotationText.Text=a.Text;
            if(a?.Kind==AnnotationKind.Step)StepNumber.Text=a.Number.ToString(CultureInfo.InvariantCulture);
            RestoreCropButton.IsEnabled=_session?.Document.Crop!=null;
        }finally{_syncProperties=false;}
    }
    private static void SelectValue(ComboBox picker,double value){foreach(ComboBoxItem i in picker.Items)if(double.TryParse(i.Tag?.ToString(),out var v)&&Math.Abs(v-value)<.01){picker.SelectedItem=i;break;}}
    private static double PickedValue(ComboBox picker,double fallback)=>picker.SelectedItem is ComboBoxItem i&&double.TryParse(i.Tag?.ToString(),out var v)?v:fallback;
    private void Color_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button {Tag:string color})return;Surface.InkColor=color;
        if(_session!=null&&Surface.SelectedId is {} id)_session.Change(d=>d.Annotations.First(a=>a.Id==id).Color=color);
        Surface.Refresh();SyncProperties();
    }
    private void Style_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(_syncProperties||Surface==null||StrokePicker==null||FontPicker==null)return;
        var stroke=PickedValue(StrokePicker,4);var font=PickedValue(FontPicker,28);Surface.StrokeSize=stroke;Surface.TextSize=font;
        if(_session!=null&&Surface.SelectedId is {} id)_session.Change(d=>{var a=d.Annotations.First(a=>a.Id==id);a.Stroke=stroke;a.FontSize=font;});
        Surface.Refresh();
    }
    private void AnnotationText_Changed(object sender,TextChangedEventArgs e)
    {
        if(_syncProperties||_session==null||Surface.Selected is not {Kind:AnnotationKind.Text} a)return;
        _session.Begin();a.Text=AnnotationText.Text.Length>20000?AnnotationText.Text[..20000]:AnnotationText.Text;SavedLabel.Text="Đang chỉnh sửa…";Surface.InvalidateVisual();_textTimer.Stop();_textTimer.Start();
    }
    private void PropertyText_LostFocus(object sender,KeyboardFocusChangedEventArgs e){_textTimer.Stop();_session?.Commit();}
    private void PropertyText_KeyDown(object sender,KeyEventArgs e)
    {
        if(e.Key==Key.Escape){_textTimer.Stop();_session?.Cancel();Surface.Refresh();SyncProperties();SavedLabel.Text=_session?.Dirty==true?"Đang lưu…":"✓ Đã lưu";Surface.Focus();e.Handled=true;}
        else if(e.Key==Key.Enter&&Keyboard.Modifiers.HasFlag(ModifierKeys.Control)){_session?.Commit();Surface.Focus();e.Handled=true;}
    }
    private void StepNumber_Commit(object sender,KeyboardFocusChangedEventArgs e)=>CommitStep();
    private void StepNumber_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Enter){CommitStep();Surface.Focus();e.Handled=true;}}
    private void CommitStep(){if(_syncProperties||_session==null||Surface.Selected is not {Kind:AnnotationKind.Step} a)return;if(int.TryParse(StepNumber.Text,out var n)&&n>=1&&n<=9999){var id=a.Id;_session.Change(d=>d.Annotations.First(x=>x.Id==id).Number=n);}SyncProperties();}
    private void DeleteAnnotation_Click(object sender,RoutedEventArgs e)=>Surface.DeleteSelected();
    private void RestoreCrop_Click(object sender,RoutedEventArgs e){_session?.Change(d=>d.Crop=null);Surface.Refresh();if(_fitMode)FitImage();}
    private void Undo_Click(object sender,RoutedEventArgs e){Surface.CancelInteraction();_session?.Undo();Surface.Refresh();SyncProperties();if(_fitMode)FitImage();}
    private void Redo_Click(object sender,RoutedEventArgs e){Surface.CancelInteraction();_session?.Redo();Surface.Refresh();SyncProperties();if(_fitMode)FitImage();}
    private void FitImage()
    {
        _fitMode=true;_actualPixels=false;if(_session==null)return;var v=_session.Document.Viewport;
        Surface.SetZoom(Math.Min(Math.Max(40,ImageScroll.ActualWidth-65)/v.Width,Math.Max(40,ImageScroll.ActualHeight-65)/v.Height));
    }
    private void Fit_Click(object sender,RoutedEventArgs e)=>FitImage();
    private void Actual_Click(object sender,RoutedEventArgs e){_fitMode=false;_actualPixels=true;Surface.SetZoom(1/VisualTreeHelper.GetDpi(this).DpiScaleX);}
    private void ZoomOut_Click(object sender,RoutedEventArgs e){_fitMode=false;_actualPixels=false;Surface.SetZoom(Surface.Zoom/1.25);}
    private void ZoomIn_Click(object sender,RoutedEventArgs e){_fitMode=false;_actualPixels=false;Surface.SetZoom(Surface.Zoom*1.25);}
    protected override void OnDpiChanged(DpiScale oldDpi,DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi,newDpi);if(Surface==null||_session==null)return;
        Dispatcher.BeginInvoke(()=>{if(_actualPixels)Surface.SetZoom(1/newDpi.DpiScaleX);else if(_fitMode)FitImage();else Surface.Refresh();},DispatcherPriority.Loaded);
    }
    private void Viewport_Changed(object sender,SizeChangedEventArgs e){if(_fitMode&&_session!=null)FitImage();}
    private void Editor_KeyDown(object sender,KeyEventArgs e)
    {
        var ctrl=Keyboard.Modifiers.HasFlag(ModifierKeys.Control);var typing=Keyboard.FocusedElement is TextBoxBase;
        if(Surface.IsMouseCaptured&&e.Key!=Key.Escape){
            if(ctrl&&e.Key==Key.Z){Surface.CancelInteraction();_session?.Undo();Surface.Refresh();SyncProperties();}
            e.Handled=true;return;
        }
        if(ctrl&&e.Key==Key.S){if(StepNumber.IsKeyboardFocusWithin)CommitStep();_session?.Commit();FlushEdits();e.Handled=true;return;}
        if(typing||_session==null||_busy)return;
        if(ctrl){
            if(e.Key==Key.Z){if(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))Redo_Click(sender,e);else Undo_Click(sender,e);e.Handled=true;}
            else if(e.Key==Key.Y){Redo_Click(sender,e);e.Handled=true;}
            else if(e.Key==Key.C){Copy_Click(sender,e);e.Handled=true;}
            return;
        }
        if(Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))return;
        if(e.Key==Key.Escape){Surface.CancelInteraction();SelectTool(EditorTool.Select);e.Handled=true;return;}
        if(e.Key==Key.Delete){Surface.DeleteSelected();e.Handled=true;return;}
        double step=Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)?10:1;
        if(e.Key is Key.Left or Key.Right or Key.Up or Key.Down){Surface.Nudge(e.Key==Key.Left?-step:e.Key==Key.Right?step:0,e.Key==Key.Up?-step:e.Key==Key.Down?step:0);e.Handled=true;return;}
        EditorTool? tool=e.Key switch{Key.V=>EditorTool.Select,Key.A=>EditorTool.Arrow,Key.R=>EditorTool.Rectangle,Key.E=>EditorTool.Ellipse,Key.T=>EditorTool.Text,Key.H=>EditorTool.Highlight,Key.N=>EditorTool.Step,Key.C=>EditorTool.Crop,_=>null};
        if(tool!=null){SelectTool(tool.Value);Surface.Focus();e.Handled=true;}
    }
}
