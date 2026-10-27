using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
namespace Captovix;
internal sealed partial class OcrWindow:Window
{
    private readonly BitmapSource _snapshot;
    private readonly Image _previewImage=new(){Stretch=Stretch.Fill};
    private readonly ScrollViewer _scroll=new(){HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
    private readonly TextBlock _zoomLabel=new(){VerticalAlignment=VerticalAlignment.Center,Margin=new(8,0,8,0)};
    private readonly CheckBox _lensToggle=new(){Content="Kính lúp 3×"};
    private readonly Border _lens=new(){Width=168,Height=168,CornerRadius=new(12),BorderThickness=new(2),BorderBrush=Brushes.MediumPurple,IsHitTestVisible=false,Visibility=Visibility.Collapsed};
    private double _zoom=1;
    private bool _space,_panning;
    private Point _panStart;
    private double _panH,_panV;
    private readonly Grid _view=new(){Background=Brushes.Transparent,ClipToBounds=true};
    private readonly Canvas _overlay=new(){Background=Brushes.Transparent,Cursor=Cursors.Cross};
    private readonly System.Windows.Shapes.Rectangle _box=new(){Stroke=Brushes.MediumPurple,StrokeThickness=2,Fill=new SolidColorBrush(Color.FromArgb(25,124,58,237)),IsHitTestVisible=false};
    private readonly TextBox _result=new(){AcceptsReturn=true,AcceptsTab=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,MinHeight=160,MaxHeight=double.PositiveInfinity};
    private readonly TextBlock _status=new(){TextWrapping=TextWrapping.Wrap,Margin=new(4,8,4,8)};
    private readonly TextBlock _regionLabel=new(){TextWrapping=TextWrapping.Wrap,Margin=new(4,8,4,8)};
    private readonly ComboBox _language=new(){ItemsSource=new[]{"Tiếng Việt + English","Tiếng Việt","English"},SelectedIndex=0};
    private readonly ComboBox _layout=new(){ItemsSource=new[]{"Tự động phân tích bố cục","Một khối văn bản","Chữ rải rác trên giao diện"},SelectedIndex=0};
    private readonly Button _run=new(){Content="Nhận chữ",Style=(Style)Application.Current.FindResource("Primary")};
    private readonly Button _cancel=new(){Content="Hủy nhận dạng",IsEnabled=false};
    private readonly Button _copy=new(){Content="Copy văn bản",IsEnabled=false};
    private readonly Button _save=new(){Content="Lưu TXT…",IsEnabled=false};
    private readonly Button _all=new(){Content="Chọn toàn ảnh"};
    private PixelRect _region;
    private Point? _start;
    private CancellationTokenSource? _job;
    private bool _closeRequested;
    public OcrWindow(BitmapSource snapshot,bool assist=false,bool allowInPlace=true)
    {
        SetResourceReference(StyleProperty,typeof(Window));_snapshot=snapshot;_region=new(0,0,0,0);
        Title="Captovix · Nhận chữ từ ảnh";Width=Math.Min(1160,SystemParameters.WorkArea.Width-32);Height=Math.Min(820,SystemParameters.WorkArea.Height-32);MinWidth=720;MinHeight=500;WindowStartupLocation=WindowStartupLocation.CenterOwner;
        var grid=new Grid{Margin=new(16)};grid.ColumnDefinitions.Add(new(){Width=new(1,GridUnitType.Star)});grid.ColumnDefinitions.Add(new(){Width=new(320)});Content=grid;
        var preview=new Grid{Margin=new(0,0,14,0)};preview.RowDefinitions.Add(new(){Height=GridLength.Auto});preview.RowDefinitions.Add(new());
        var navigation=new StackPanel();navigation.Children.Add(new TextBlock{Text="Kéo chọn vùng cần lấy chữ · Ctrl+con lăn để phóng to",TextWrapping=TextWrapping.Wrap,Margin=new(4,4,4,8)});
        var zoomBar=new WrapPanel();void ZoomButton(string text,Action action){var button=new Button{Content=text,Padding=new(9,5,9,5)};button.Click+=(_,_)=>action();zoomBar.Children.Add(button);}
        ZoomButton("−",()=>SetPreviewZoom(_zoom/1.25));zoomBar.Children.Add(_zoomLabel);ZoomButton("+",()=>SetPreviewZoom(_zoom*1.25));ZoomButton("100%",()=>SetPreviewZoom(1/VisualTreeHelper.GetDpi(this).DpiScaleX));ZoomButton("Vừa khung",FitPreview);navigation.Children.Add(zoomBar);preview.Children.Add(navigation);
        Grid.SetRow(_scroll,1);preview.Children.Add(_scroll);_scroll.Content=_view;_previewImage.Source=snapshot;_view.Children.Add(_previewImage);_view.Children.Add(_overlay);_overlay.Children.Add(_box);_overlay.Children.Add(_lens);grid.Children.Add(preview);
        _scroll.SetResourceReference(Control.BackgroundProperty,"Workspace");
        _scroll.PreviewMouseWheel+=(_,e)=>{if(!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))return;e.Handled=true;SetPreviewZoom(_zoom*(e.Delta>0?1.2:1/1.2),e.GetPosition(_scroll));};
        _scroll.PreviewMouseDown+=(_,e)=>{if(!_space&&e.ChangedButton!=MouseButton.Middle)return;_panning=true;_panStart=e.GetPosition(_scroll);_panH=_scroll.HorizontalOffset;_panV=_scroll.VerticalOffset;_scroll.CaptureMouse();e.Handled=true;};
        _scroll.PreviewMouseMove+=(_,e)=>{if(_panning){var p=e.GetPosition(_scroll);_scroll.ScrollToHorizontalOffset(_panH+_panStart.X-p.X);_scroll.ScrollToVerticalOffset(_panV+_panStart.Y-p.Y);e.Handled=true;}else ShowLens(e.GetPosition(_view));};
        _scroll.PreviewMouseUp+=(_,e)=>{if(_panning){_panning=false;_scroll.ReleaseMouseCapture();e.Handled=true;}};_scroll.LostMouseCapture+=(_,_)=>_panning=false;
        _lensToggle.Unchecked+=(_,_)=>_lens.Visibility=Visibility.Collapsed;
        var side=new Grid();side.RowDefinitions.Add(new(){Height=GridLength.Auto});side.RowDefinitions.Add(new());side.RowDefinitions.Add(new(){Height=GridLength.Auto});Grid.SetColumn(side,1);grid.Children.Add(side);
        var settings=new StackPanel();side.Children.Add(settings);settings.Children.Add(new TextBlock{Text="Nhận chữ · OCR",FontSize=22,FontWeight=FontWeights.SemiBold});
        settings.Children.Add(new TextBlock{Text="Xử lý trên máy · kiểm tra lại dấu và số trước khi dùng.",TextWrapping=TextWrapping.Wrap,Margin=new(4,10,4,8)});
        var actions=new WrapPanel();actions.Children.Add(_all);actions.Children.Add(_run);actions.Children.Add(_cancel);settings.Children.Add(actions);settings.Children.Add(_status);
        Grid.SetRow(_result,1);side.Children.Add(_result);_result.TextChanged+=(_,_)=>{_copy.IsEnabled=_save.IsEnabled=_job==null&&!string.IsNullOrWhiteSpace(_result.Text);InvalidateAssist();};
        var footer=new WrapPanel();footer.Children.Add(_copy);var close=new Button{Content="Đóng"};close.Click+=(_,_)=>Close();footer.Children.Add(close);Grid.SetRow(footer,2);side.Children.Add(footer);
        _view.SizeChanged+=(_,_)=>DrawRegion();_all.Click+=(_,_)=>{InvalidateAssist();_region=new(0,0,_snapshot.PixelWidth,_snapshot.PixelHeight);DrawRegion();};
        _overlay.MouseLeftButtonDown+=(_,e)=>{if(_job!=null||_panning)return;InvalidateAssist();var p=ToPixel(e.GetPosition(_view));if(p==null)return;_start=p;_overlay.CaptureMouse();e.Handled=true;};
        _overlay.MouseMove+=(_,e)=>{if(_start is not {} start||_job!=null)return;var p=ToPixel(e.GetPosition(_view),true)!.Value;_region=OcrContract.Selection(start.X,start.Y,p.X,p.Y,_snapshot.PixelWidth,_snapshot.PixelHeight);DrawRegion();};
        _overlay.MouseLeftButtonUp+=async(_,_)=>{bool selected=_start!=null;_start=null;_overlay.ReleaseMouseCapture();if(selected&&_region.Width>=8&&_region.Height>=8)await RecognizeAsync();};_overlay.LostMouseCapture+=(_,_)=>_start=null;
        _run.Click+=async(_,_)=>await RecognizeAsync();_cancel.Click+=(_,_)=>_job?.Cancel();
        _copy.Click+=(_,_)=>{try{Clipboard.SetText(_result.Text);_status.Text="Đã copy văn bản.";}catch(Exception ex){_status.Text="Clipboard đang bận: "+ex.Message;}};
        _save.Click+=(_,_)=>{try{var dialog=new SaveFileDialog{Title="Lưu văn bản OCR",Filter="Văn bản UTF-8|*.txt",FileName="Captovix-OCR.txt"};if(dialog.ShowDialog(this)==true)File.WriteAllText(dialog.FileName,_result.Text,new System.Text.UTF8Encoding(false));}catch(Exception ex){_status.Text="Chưa lưu được: "+ex.Message;}};
        Closing+=(_,e)=>{if(_job!=null){e.Cancel=true;_closeRequested=true;_job.Cancel();_status.Text="Đang hủy nhận dạng…";}};
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Space&&Keyboard.FocusedElement is not System.Windows.Controls.Primitives.TextBoxBase){_space=true;e.Handled=true;}if(e.Key==Key.Escape){if(_job!=null)_job.Cancel();else Close();e.Handled=true;}};
        PreviewKeyUp+=(_,e)=>{if(e.Key==Key.Space)_space=false;};Deactivated+=(_,_)=>{_space=false;_panning=false;_scroll.ReleaseMouseCapture();};
        Loaded+=(_,_)=>{FitPreview();DrawRegion();_status.Text="Kéo chọn vùng trên ảnh để tự nhận chữ.";};
    }
    private (double Scale,double X,double Y) Mapping()
    {
        return(_zoom,0,0);
    }
    private void FitPreview()=>SetPreviewZoom(Math.Min(Math.Max(80,_scroll.ViewportWidth-16)/_snapshot.PixelWidth,Math.Max(80,_scroll.ViewportHeight-16)/_snapshot.PixelHeight));
    private void SetPreviewZoom(double value,Point? anchor=null)
    {
        var at=anchor??new Point(_scroll.ViewportWidth/2,_scroll.ViewportHeight/2);double x=(_scroll.HorizontalOffset+at.X)/_zoom,y=(_scroll.VerticalOffset+at.Y)/_zoom;
        _zoom=Math.Clamp(value,.03,8);_view.Width=_snapshot.PixelWidth*_zoom;_view.Height=_snapshot.PixelHeight*_zoom;_scroll.UpdateLayout();
        _scroll.ScrollToHorizontalOffset(x*_zoom-at.X);_scroll.ScrollToVerticalOffset(y*_zoom-at.Y);_zoomLabel.Text=$"{_zoom*VisualTreeHelper.GetDpi(this).DpiScaleX*100:0}%";DrawRegion();
    }
    private void ShowLens(Point point)
    {
        if(_lensToggle.IsChecked!=true||_panning||ToPixel(point) is not {} source){_lens.Visibility=Visibility.Collapsed;return;}
        double sample=Math.Min(56*VisualTreeHelper.GetDpi(this).DpiScaleX,Math.Min(_snapshot.PixelWidth,_snapshot.PixelHeight));_lens.Background=new ImageBrush((ImageSource)_previewImage.Source){ViewboxUnits=BrushMappingMode.Absolute,Viewbox=new Rect(Math.Clamp(source.X-sample/2,0,_snapshot.PixelWidth-sample),Math.Clamp(source.Y-sample/2,0,_snapshot.PixelHeight-sample),sample,sample),Stretch=Stretch.Fill};
        Canvas.SetLeft(_lens,Math.Max(_scroll.HorizontalOffset,Math.Min(point.X+20,_scroll.HorizontalOffset+_scroll.ViewportWidth-176)));Canvas.SetTop(_lens,Math.Max(_scroll.VerticalOffset,Math.Min(point.Y+20,_scroll.VerticalOffset+_scroll.ViewportHeight-176)));_lens.Visibility=Visibility.Visible;
    }
    private Point? ToPixel(Point point,bool clamp=false)
    {
        var m=Mapping();if(m.Scale<=0)return null;double x=(point.X-m.X)/m.Scale,y=(point.Y-m.Y)/m.Scale;
        if(!clamp&&(x<0||y<0||x>_snapshot.PixelWidth||y>_snapshot.PixelHeight))return null;
        return new(Math.Clamp(x,0,_snapshot.PixelWidth),Math.Clamp(y,0,_snapshot.PixelHeight));
    }
    private void DrawRegion()
    {
        _box.Visibility=_region.Width>=8&&_region.Height>=8?Visibility.Visible:Visibility.Collapsed;if(_job==null)_run.IsEnabled=_region.Width>=8&&_region.Height>=8;var m=Mapping();Canvas.SetLeft(_box,m.X+_region.X*m.Scale);Canvas.SetTop(_box,m.Y+_region.Y*m.Scale);_box.Width=Math.Max(0,_region.Width*m.Scale);_box.Height=Math.Max(0,_region.Height*m.Scale);
        _regionLabel.Text=$"Vùng nhận chữ: {_region.Width} × {_region.Height} px · X={_region.X}, Y={_region.Y}";
    }
    private async Task RecognizeAsync()
    {
        if(_job!=null)return;
        try{OcrContract.Validate(_region.Width,_region.Height,OcrContract.Languages[_language.SelectedIndex],new[]{3,6,11}[_layout.SelectedIndex]);}catch(Exception ex){_status.Text=ex.Message;return;}
        InvalidateAssist();_recognizedRegion=_region;using var cancellation=new CancellationTokenSource();_job=cancellation;_cancel.IsEnabled=true;_run.IsEnabled=_all.IsEnabled=_language.IsEnabled=_layout.IsEnabled=_copy.IsEnabled=_save.IsEnabled=false;
        _result.IsReadOnly=true;_result.Clear();_status.Text="Đang nhận dạng trên máy… Có thể hủy.";
        try{
            var result=await OcrService.RecognizeAsync(_snapshot,_region,OcrContract.Languages[_language.SelectedIndex],new[]{3,6,11}[_layout.SelectedIndex],cancellation.Token);
            if(result.Error!=null){_status.Text=result.Error;return;}
            _result.Text=result.Text;_status.Text=string.IsNullOrWhiteSpace(result.Text)?"Chưa tìm thấy chữ. Hãy chọn vùng rõ hơn.":"Đã nhận chữ. Có thể sửa rồi sao chép.";
        }catch(OperationCanceledException){_status.Text="Đã hủy nhận dạng. Ảnh không thay đổi.";}
        catch(Exception ex){App.Log(ex);_status.Text=ex.Message;}
        finally{_job=null;_result.IsReadOnly=false;_cancel.IsEnabled=false;_run.IsEnabled=_all.IsEnabled=_language.IsEnabled=_layout.IsEnabled=true;_copy.IsEnabled=_save.IsEnabled=!string.IsNullOrWhiteSpace(_result.Text);if(_closeRequested)Close();}
    }
}
public partial class MainWindow
{
    private void Ocr_Click(object sender,RoutedEventArgs e)=>OpenOcr(true);
    private void Assist_Click(object sender,RoutedEventArgs e)=>OpenOcr(true);
    private void OpenOcr(bool assist)
    {
        if(_busy||_updateApplying||_session==null||!PrepareCropForExport()||!_captureGate.TryEnter())return;
        Surface.CancelInteraction();_session.Commit();
        SetBusy(true);
        try{var image=Surface.Flatten();var layout=Surface.Layout!;bool allow=Math.Abs(layout.A-1)<.00001&&Math.Abs(layout.D-1)<.00001&&Math.Abs(layout.B)<.00001&&Math.Abs(layout.C)<.00001;var dialog=new OcrWindow(image,assist,allow){Owner=this};if(dialog.ShowDialog()==true&&dialog.Request is {} request)ApplyAssisted(request,image,dialog.CreateCopy);}
        catch(Exception ex){App.Log(ex);MessageBox.Show(this,ex.Message,"Chưa mở được OCR");}
        finally{SetBusy(false);_captureGate.Exit();if(_exitRequested)_=Dispatcher.BeginInvoke(RequestExit);}
    }
}
