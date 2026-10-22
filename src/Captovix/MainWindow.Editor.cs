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
    private bool _editorReady,_syncProperties,_changingSelection,_fitMode=true,_actualPixels;
    private void InitializeEditor()
    {
        _saveTimer.Tick+=(_,_)=>{_saveTimer.Stop();if(_session?.InTransaction==true||_inlineId!=null)return;FlushEdits(false);};
        _textTimer.Tick+=(_,_)=>{_textTimer.Stop();_session?.Commit();};
        Surface.SelectionChanged+=SyncProperties;
        Surface.VisualChanged+=UpdateEditorReadout;
        Surface.TextRequested+=BeginInlineText;
        Surface.VisualChanged+=PositionInlineText;
        Surface.CropFinished+=()=>{SelectTool(EditorTool.Select);FitImage();};
        InitializeViewport();
        InitializeObjectControl();
        InitializeShapeStudio();
        InitializeAnnotationFeatures();
        // XAML raises selection events before later named controls exist.
        // Enable callbacks only after all editor subsystems have been wired.
        _editorReady=true;
        SelectTool(EditorTool.Select);
    }
    private void UpdateEditorReadout()
    {
        if(!_editorReady)return;
        // Refresh also fires while a surface is being cleared or replaced.
        // Only read dimensions from a complete, matching document/surface pair.
        if(_session is {} session&&ReferenceEquals(session,Surface.Session)&&Surface.Original!=null&&Surface.Layout is {} layout){
            OriginalSizeLabel.Text=$"Gốc: {Surface.Original.PixelWidth} × {Surface.Original.PixelHeight} px";
            ImageSizeLabel.Text=$"{layout.Width} × {layout.Height} px · {session.Document.Annotations.Count} chú thích";
            ImageFidelityHint.Text=ImageFidelity.CanCopyPixels(layout)?"PNG/Copy: giữ pixel vùng không chỉnh. Vừa khung chỉ đổi cách xem.":"Xuất/Copy có resize theo thiết lập; zoom chỉ đổi cách xem.";
            RestoreCropButton.IsEnabled=session.Document.Crop!=null;
            ZoomLabel.Text=$"{Surface.PhysicalZoom*100:0.#}%";
            ZoomLabel.ToolTip=$"Zoom hiển thị đầu ra · Windows scale {Surface.DisplayScale*100:0.#}% · không đổi file ảnh";
        }else{
            OriginalSizeLabel.Text=ImageSizeLabel.Text=ImageFidelityHint.Text="";ZoomLabel.Text="—";RestoreCropButton.IsEnabled=false;
        }
    }
    private void Session_Changed()
    {
        SavedLabel.Text="Đang lưu…";SavedLabel.Foreground=Brushes.DarkOrange;
        UndoButton.IsEnabled=_session?.CanUndo==true;RedoButton.IsEnabled=_session?.CanRedo==true;
        Surface.Refresh();SyncProperties();_saveTimer.Stop();_saveTimer.Start();
    }
    private bool FlushEdits(bool report=true)
    {
        EndInlineText(true);_textTimer.Stop();_session?.Commit();_saveTimer.Stop();
        if(_session==null||_activeItem==null||Surface.Original==null||!_session.Dirty)return true;
        try{
            DocumentStore.Save(_activeItem,_session,Surface.Original);SavedLabel.Text="✓ Đã lưu";SavedLabel.Foreground=new SolidColorBrush(Color.FromRgb(35,148,110));return true;
        }catch(Exception ex){App.Log(ex);SavedLabel.Text="Chưa lưu · Ctrl+S thử lại";SavedLabel.Foreground=Brushes.Firebrick;SetStatus("Không lưu được chú thích: "+ex.Message);if(report)MessageBox.Show(this,"Chưa lưu được phần chỉnh sửa. Nội dung vẫn đang ở Editor; hãy kiểm tra dung lượng/quyền thư mục và nhấn Ctrl+S.\n\n"+ex.Message,"Captovix",MessageBoxButton.OK,MessageBoxImage.Warning);return false;}
    }
    private void UnloadEditor()
    {
        EndInlineText(false);_textTimer.Stop();_saveTimer.Stop();if(_session!=null)_session.Changed-=Session_Changed;
        _spaceHeld=false;EndPan();Surface.CancelInteraction();
        // Load raises callbacks synchronously. Release the old session first so
        // every subscriber sees the same empty state while the canvas unloads.
        _session=null;_activeItem=null;Surface.Load(null,null);UndoButton.IsEnabled=RedoButton.IsEnabled=false;
    }
    private void OpenImage(MediaItem item)
    {
        var bitmap=MediaStore.LoadImage(item.FilePath);var session=DocumentStore.Load(item,bitmap,out var warning);
        _activeItem=item;_session=session;session.Changed+=Session_Changed;
        ImageScroll.Visibility=ImageTools.Visibility=ZoomTools.Visibility=EditorProperties.Visibility=Visibility.Visible;
        Surface.SetDisplayScale(VisualTreeHelper.GetDpi(this).DpiScaleX);Surface.Load(bitmap,session);SelectTool(EditorTool.Select);_fitMode=true;
        Dispatcher.BeginInvoke(FitImage,DispatcherPriority.Loaded);
        if(warning!=null){SetStatus("Bản chú thích cũ chưa được áp dụng; đã giữ bản sao.");MessageBox.Show(this,warning,"Kiểm tra dữ liệu chỉnh sửa",MessageBoxButton.OK,MessageBoxImage.Warning);}
    }
    private void SelectTool(EditorTool tool)
    {
        if(Surface==null)return;EndInlineText(true);Surface.CancelInteraction();
        if(Surface.Tool==EditorTool.Crop&&tool!=EditorTool.Crop)Surface.ClearCropDraft();
        if(tool is EditorTool.Shape or EditorTool.Line or EditorTool.Arrow)Surface.SelectIds([]);
        Surface.Tool=tool;if(tool==EditorTool.Crop)Surface.StartCrop();Surface.Cursor=tool==EditorTool.Select?Cursors.Arrow:Cursors.Cross;
        foreach(var button in ToolBox.Children.OfType<Button>()){
            var selected=button.Tag?.ToString()==tool.ToString();button.Background=selected?(Brush)FindResource("Accent"):Brushes.White;button.Foreground=selected?Brushes.White:new SolidColorBrush(Color.FromRgb(36,51,72));
        }
        SyncProperties();
        ToolHint.Text=tool switch{
            EditorTool.Select=>"Shift/Ctrl+click chọn nhiều • Alt+click chọn lớp dưới • Tay nắm tròn xoay • Shift khóa hướng/tỷ lệ/góc 15° • Con lăn zoom • Space kéo khung nhìn",
            EditorTool.Shape=>"Chọn hình và viền/nền ở bảng thuộc tính → kéo để vẽ • Shift giữ tỷ lệ • V để chọn và sửa lại.",
            EditorTool.Line or EditorTool.Arrow=>"Kéo để vẽ • Shift khóa hướng mỗi 45° • V để sửa đầu mút; chỉnh kiểu nét và đầu mũi tên bên phải.",
            EditorTool.Text=>"Click trên ảnh để nhập chữ trực tiếp • Double-click chữ để sửa • Ctrl+Enter hoàn tất, Esc hủy.",
            EditorTool.Callout=>"Kéo hộp để tạo callout và nhập chữ • Chọn rồi kéo tay nắm ở đầu đuôi để đổi điểm chỉ • Double-click sửa chữ.",
            EditorTool.Freehand=>"Giữ chuột trái để vẽ tự do • Thả chuột kết thúc nét • Esc hủy nét đang vẽ.",
            EditorTool.Step=>"Click để đặt số bước tự tăng. Có thể sửa số ở bảng thuộc tính.",
            EditorTool.Crop=>"Kéo vùng → kéo tay nắm hoặc nhập kích thước → Enter/Áp dụng • Esc hủy • Space kéo khung nhìn.",
            EditorTool.Highlight=>"Kéo phủ vùng cần làm nổi bật. Chọn màu ở bảng thuộc tính.",
            EditorTool.Blur or EditorTool.Pixelate=>"Kéo vùng cần xử lý • Khi kéo vùng tạm che kín; thả chuột để áp dụng • Chỉnh mức xử lý bên phải.",
            EditorTool.Cover=>"Kéo vùng để che kín bằng màu đang chọn • Xuất PNG/JPEG để chia sẻ ảnh đã che.",_=>"Kéo để vẽ đối tượng • Chọn công cụ Chọn (V) để sửa lại."};
    }
    private void Tool_Click(object sender,RoutedEventArgs e){if(sender is Button {Tag:string tool}){_session?.Commit();SelectTool(Enum.Parse<EditorTool>(tool));Surface.Focus();}}
    private void SyncProperties()
    {
        if(!_editorReady||_syncProperties)return;_syncProperties=true;
        try{
            var a=Surface.Selected;DeleteAnnotationButton.IsEnabled=a!=null;PropertyTitle.Text=a==null?"Thuộc tính chú thích":a.Kind switch{AnnotationKind.Arrow=>"Mũi tên",AnnotationKind.Rectangle=>"Khung chữ nhật",AnnotationKind.Ellipse=>"Hình ellipse",AnnotationKind.Text=>"Chữ",AnnotationKind.Highlight=>"Highlight",AnnotationKind.Blur=>"Làm mờ",AnnotationKind.Pixelate=>"Pixel hóa",AnnotationKind.Cover=>"Che kín",AnnotationKind.Callout=>"Hộp chú thích",AnnotationKind.Freehand=>"Nét vẽ tự do",_=>ShapeStudio.IsClosed(a.Kind)||a.Kind==AnnotationKind.Line?ShapeStudio.Label(a.Kind):"Số bước"};
            if(Surface.SelectedIds.Count>1)PropertyTitle.Text=$"Đã chọn {Surface.SelectedIds.Count} đối tượng";
            if(a!=null){Surface.InkColor=a.Color;Surface.StrokeSize=a.Stroke;Surface.TextSize=a.FontSize;Surface.EffectSize=a.EffectSize;}
            SelectValue(StrokePicker,Surface.StrokeSize);SelectValue(FontPicker,Surface.TextSize);
            SyncObjectControl();SyncShapeStudio();SyncAnnotationFeatures();SyncCropProperties();SyncAdvancedAnnotation();SyncImageAppearance();
            foreach(var b in ColorPalette.Children.OfType<Button>())b.BorderThickness=new Thickness(b.Tag?.ToString()==Surface.InkColor?3:1);
            TextProperties.Visibility=a?.Kind is AnnotationKind.Text or AnnotationKind.Callout&&Surface.SelectedIds.Count==1?Visibility.Visible:Visibility.Collapsed;
            StepProperties.Visibility=a?.Kind==AnnotationKind.Step&&Surface.SelectedIds.Count==1?Visibility.Visible:Visibility.Collapsed;
            if(a?.Kind is AnnotationKind.Text or AnnotationKind.Callout&&AnnotationText.Text!=a.Text)AnnotationText.Text=a.Text;
            if(a?.Kind==AnnotationKind.Step)StepNumber.Text=a.Number.ToString(CultureInfo.InvariantCulture);
            RestoreCropButton.IsEnabled=_session?.Document.Crop!=null;
        }finally{_syncProperties=false;}
    }
    private static void SelectValue(ComboBox picker,double value){
        foreach(ComboBoxItem i in picker.Items)if(double.TryParse(i.Tag?.ToString(),NumberStyles.Float,CultureInfo.InvariantCulture,out var v)&&Math.Abs(v-value)<.01){picker.SelectedItem=i;return;}
        var custom=new ComboBoxItem{Content=$"{value:0.#} px",Tag=value.ToString(CultureInfo.InvariantCulture)};picker.Items.Add(custom);picker.SelectedItem=custom;
    }
    private static double PickedValue(ComboBox picker,double fallback)=>picker.SelectedItem is ComboBoxItem i&&double.TryParse(i.Tag?.ToString(),NumberStyles.Float,CultureInfo.InvariantCulture,out var v)?v:fallback;
    private void Color_Click(object sender,RoutedEventArgs e)
    {
        if(sender is not Button {Tag:string color})return;Surface.InkColor=color;
        ChangeSelected(a=>a.Color=color);
        Surface.Refresh();SyncProperties();
    }
    private void Style_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(!_editorReady||_syncProperties)return;
        var stroke=PickedValue(StrokePicker,4);var font=PickedValue(FontPicker,28);Surface.StrokeSize=stroke;Surface.TextSize=font;
        ChangeSelected(a=>{a.Stroke=stroke;a.FontSize=font;});
        Surface.Refresh();SyncProperties();
    }
    private void AnnotationText_Changed(object sender,TextChangedEventArgs e)
    {
        if(_syncProperties||_session==null||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds)||Surface.Selected is not {Kind:AnnotationKind.Text or AnnotationKind.Callout} a)return;
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
    private void CommitStep(){if(_syncProperties||_session==null||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds)||Surface.Selected is not {Kind:AnnotationKind.Step} a)return;if(int.TryParse(StepNumber.Text,out var n)&&n>=1&&n<=9999){var id=a.Id;_session.Change(d=>d.Annotations.First(x=>x.Id==id).Number=n);}SyncProperties();}
    private void DeleteAnnotation_Click(object sender,RoutedEventArgs e)=>Surface.DeleteSelected();
    private void RestoreCrop_Click(object sender,RoutedEventArgs e){if(_session==null)return;SelectTool(EditorTool.Select);try{_=new ImageLayout(new PixelRect(0,0,_session.Document.OriginalWidth,_session.Document.OriginalHeight),_session.Document.Appearance);_session.Change(d=>d.Crop=null);Surface.Refresh();if(_fitMode)FitImage();}catch(Exception ex){SetStatus("Chưa khôi phục crop: "+ex.Message);}}
    private void Undo_Click(object sender,RoutedEventArgs e){Surface.CancelInteraction();if(Surface.Tool==EditorTool.Crop){SelectTool(EditorTool.Select);return;}_session?.Undo();Surface.Refresh();SyncProperties();if(_fitMode)FitImage();}
    private void Redo_Click(object sender,RoutedEventArgs e){Surface.CancelInteraction();if(Surface.Tool==EditorTool.Crop){SelectTool(EditorTool.Select);return;}_session?.Redo();Surface.Refresh();SyncProperties();if(_fitMode)FitImage();}
    private void FitImage()
    {
        _fitMode=true;_actualPixels=false;if(_session==null||Surface.Layout is not {} v)return;
        Surface.SetZoom(Math.Min(Math.Max(40,ImageScroll.ActualWidth-65)/v.Width,Math.Max(40,ImageScroll.ActualHeight-65)/v.Height));
    }
    private void Fit_Click(object sender,RoutedEventArgs e)=>FitImage();
    private void Actual_Click(object sender,RoutedEventArgs e){ZoomAtCenter(ImageFidelity.ActualZoom(VisualTreeHelper.GetDpi(this).DpiScaleX));_actualPixels=true;}
    private void ZoomOut_Click(object sender,RoutedEventArgs e)=>ZoomAtCenter(Surface.Zoom/1.25);
    private void ZoomIn_Click(object sender,RoutedEventArgs e)=>ZoomAtCenter(Surface.Zoom*1.25);
    protected override void OnDpiChanged(DpiScale oldDpi,DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi,newDpi);if(Surface==null)return;
        var physical=Surface.PhysicalZoom;Surface.SetDisplayScale(newDpi.DpiScaleX);if(_session==null)return;
        if(_actualPixels)Surface.SetZoom(ImageFidelity.ActualZoom(newDpi.DpiScaleX));
        else if(!_fitMode)Surface.SetZoom(physical/newDpi.DpiScaleX);
        var session=_session;Dispatcher.BeginInvoke(()=>{if(!ReferenceEquals(_session,session))return;if(_fitMode)FitImage();else Surface.Refresh();},DispatcherPriority.Loaded);
    }
    private void Viewport_Changed(object sender,SizeChangedEventArgs e){if(_fitMode&&_session!=null)FitImage();}
    private void Editor_KeyDown(object sender,KeyEventArgs e)
    {
        if(_keyEditors.Values.Any(v=>v.Key.IsKeyboardFocusWithin))return;
        var ctrl=Keyboard.Modifiers.HasFlag(ModifierKeys.Control);var typing=Keyboard.FocusedElement is TextBoxBase;
        if(ViewportKey(e,typing))return;
        if(Surface.IsMouseCaptured&&e.Key!=Key.Escape){
            if(ctrl&&e.Key==Key.Z){Surface.CancelInteraction();_session?.Undo();Surface.Refresh();SyncProperties();}
            e.Handled=true;return;
        }
        if(ctrl&&e.Key==Key.S){if(Surface.Tool==EditorTool.Crop&&!ApplyCropFromFields()){e.Handled=true;return;}if(StepNumber.IsKeyboardFocusWithin)CommitStep();_session?.Commit();FlushEdits();e.Handled=true;return;}
        if(typing||Keyboard.FocusedElement is ComboBox or ComboBoxItem||_session==null||_busy)return;
        if(!ctrl&&e.Key==Key.F2){BeginInlineText();e.Handled=true;return;}
        if(ctrl){
            if(e.Key==Key.D){ObjectCommand("Duplicate");e.Handled=true;return;}
            if(e.Key==Key.G){ObjectCommand(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)?"Ungroup":"Group");e.Handled=true;return;}
            if(e.Key is Key.OemCloseBrackets or Key.OemOpenBrackets){ObjectCommand(e.Key==Key.OemCloseBrackets?(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)?"Front":"Forward"):(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)?"Back":"Backward"));e.Handled=true;return;}
            if(e.Key==Key.Z){if(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))Redo_Click(sender,e);else Undo_Click(sender,e);e.Handled=true;}
            else if(e.Key==Key.Y){Redo_Click(sender,e);e.Handled=true;}
            else if(e.Key==Key.C){Copy_Click(sender,e);e.Handled=true;}
            else if(e.Key==Key.A){Surface.SelectAll();e.Handled=true;}
            return;
        }
        if(Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))return;
        if(e.Key==Key.Escape){Surface.CancelInteraction();SelectTool(EditorTool.Select);e.Handled=true;return;}
        if(e.Key==Key.Delete){Surface.DeleteSelected();e.Handled=true;return;}
        double step=Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)?10:1;
        if(e.Key is Key.Left or Key.Right or Key.Up or Key.Down){Surface.Nudge(e.Key==Key.Left?-step:e.Key==Key.Right?step:0,e.Key==Key.Up?-step:e.Key==Key.Down?step:0);e.Handled=true;return;}
        EditorTool? tool=e.Key switch{Key.S=>EditorTool.Shape,Key.L=>EditorTool.Line,Key.V=>EditorTool.Select,Key.A=>EditorTool.Arrow,Key.R=>EditorTool.Rectangle,Key.E=>EditorTool.Ellipse,Key.T=>EditorTool.Text,Key.H=>EditorTool.Highlight,Key.N=>EditorTool.Step,Key.C=>EditorTool.Crop,Key.B=>EditorTool.Blur,Key.P=>EditorTool.Pixelate,Key.K=>EditorTool.Cover,Key.Q=>EditorTool.Callout,Key.F=>EditorTool.Freehand,_=>null};
        if(tool!=null){SelectTool(tool.Value);Surface.Focus();e.Handled=true;}
    }
}
