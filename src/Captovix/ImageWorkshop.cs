using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
namespace Captovix;
public sealed class ImageWorkshop : Window
{
    private readonly int _w,_h;
    private readonly byte[] _original;
    private byte[] _working,_preview;
    private readonly Image _image=new();private readonly Canvas _overlay=new(){Background=Brushes.Transparent};
    private readonly Grid _picture=new();private readonly ScrollViewer _scroll=new(){HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
    private readonly ComboBox _mode=new(){ItemsSource=new[]{"Màu sắc & làm nét","Clone · sao chép vùng","Chỉnh phối cảnh","Chỉnh nghiêng","Tách nền theo màu","Lấy màu trên ảnh"},SelectedIndex=0};
    private readonly StackPanel _color=new(),_brush=new(),_perspective=new(),_rotation=new();private readonly TextBlock _status=new(){TextWrapping=TextWrapping.Wrap,Margin=new(4,10,4,10)};
    private readonly List<Slider> _adjust=[];private readonly Slider _angle=new(){Minimum=-45,Maximum=45,TickFrequency=.1},_radius=new(){Minimum=1,Maximum=150,Value=20},_tolerance=new(){Minimum=0,Maximum=150,Value=30};
    private readonly CheckBox _restore=new(){Content="Cọ giữ / khôi phục vùng gốc"},_before=new(){Content="Xem trước / ảnh ban đầu"};
    private readonly Stack<byte[]> _undo=new();private readonly (double X,double Y)[] _corners;private int _corner=-1,_version;
    private (int X,int Y)? _source,_last;private byte[]? _cloneSource;private bool _drawing,_closed,_running;private CancellationTokenSource? _cts;
    private bool _brushDirty,_previewValid=true;
    private readonly DispatcherTimer _brushFrame=new(){Interval=TimeSpan.FromMilliseconds(40)};
    private readonly DispatcherTimer _debounce=new(){Interval=TimeSpan.FromMilliseconds(180)};
    public string? PickedColor {get;private set;}
    public BitmapSource? Result {get;private set;}
    public ImageWorkshop(BitmapSource source,int mode=0)
    {
        _w=source.PixelWidth;_h=source.PixelHeight;if((long)_w*_h>40_000_000)throw new ArgumentException("Công cụ hỗ trợ tối đa40 MP.");_original=ImageTransfer.Bgra(source);_working=(byte[])_original.Clone();_preview=_working;_corners=[(0,0),(_w-1,0),(_w-1,_h-1),(0,_h-1)];
        Title="Captovix · Chỉnh ảnh";Width=Math.Min(1180,SystemParameters.WorkArea.Width-32);Height=Math.Min(820,SystemParameters.WorkArea.Height-32);WindowStartupLocation=WindowStartupLocation.CenterOwner;
        var root=new Grid{Margin=new(14)};root.ColumnDefinitions.Add(new());root.ColumnDefinitions.Add(new(){Width=new(285)});Content=root;var side=new StackPanel{Margin=new(14,0,0,0)};var sidebar=new ScrollViewer{Content=side,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};Grid.SetColumn(sidebar,1);root.Children.Add(sidebar);side.Children.Add(_mode);
        void Button(Panel parent,string text,Action action){var b=new Button{Content=text};b.Click+=(_,_)=>action();parent.Children.Add(b);}
        void Slider(Panel parent,string label,Slider value){var text=new TextBlock{Text=label,Margin=new(3,9,3,0)};parent.Children.Add(text);parent.Children.Add(value);value.ValueChanged+=(_,_)=>text.Text=label+" · "+value.Value.ToString("0.#");}
        foreach(var label in new[]{"Độ sáng","Tương phản","Nhiệt độ màu","Độ bão hòa","Làm nét"}){var v=new Slider{Minimum=label=="Làm nét"?0:-100,Maximum=label=="Làm nét"?2:100,Value=0};_adjust.Add(v);Slider(_color,label,v);v.ValueChanged+=(_,_)=>Schedule();}side.Children.Add(_color);
        Slider(_brush,"Bán kính cọ · px",_radius);Slider(_brush,"Dung sai màu nền",_tolerance);_brush.Children.Add(_restore);side.Children.Add(_brush);
        _perspective.Children.Add(new TextBlock{Text="Kéo bốn góc trên ảnh, thứ tự: trái trên → phải trên → phải dưới → trái dưới. Lưới giúp căn. Giữ nguyên kích thước pixel đầu ra.",TextWrapping=TextWrapping.Wrap});Button(_perspective,"Xem trước phối cảnh",()=>Schedule());side.Children.Add(_perspective);
        Slider(_rotation,"Góc nghiêng · độ",_angle);_angle.ValueChanged+=(_,_)=>Schedule();side.Children.Add(_rotation);
        _before.Checked+=(_,_)=>Show();_before.Unchecked+=(_,_)=>Show();side.Children.Add(_before);
        Button(side,"Chốt thay đổi đang xem",CommitPreview);Button(side,"Hoàn tác bước trong cửa sổ",()=>{if(_running)return;if(_undo.Count>0){_working=_undo.Pop();_preview=_working;ResetSliders();Show();}});
        Button(side,"Khôi phục ảnh khi mở",()=>{CancelJob();Push();_working=(byte[])_original.Clone();_preview=_working;ResetSliders();Show();});
        Button(side,"Áp dụng vào Editor",async()=>{if(_debounce.IsEnabled){_debounce.Stop();await Preview();}if(_running){_status.Text="Chờ dựng xong xem trước.";return;}if(!_previewValid){_status.Text="Sửa thiết lập chưa hợp lệ trước khi áp dụng.";return;}Result=ImageTransfer.Bitmap(_w,_h,_preview);DialogResult=true;});Button(side,"Hủy",()=>Close());side.Children.Add(_status);
        var preview=new DockPanel();root.Children.Add(preview);var zoom=new Slider{Minimum=.05,Maximum=4,Value=1,Width=160};var top=new WrapPanel();DockPanel.SetDock(top,Dock.Top);preview.Children.Add(top);top.Children.Add(new TextBlock{Text="Zoom",VerticalAlignment=VerticalAlignment.Center});top.Children.Add(zoom);Button(top,"100%",()=>zoom.Value=1);Button(top,"Vừa khung",()=>zoom.Value=Math.Clamp(Math.Min(Math.Max(40,_scroll.ActualWidth-20)/_w,Math.Max(40,_scroll.ActualHeight-20)/_h),.05,4));preview.Children.Add(_scroll);
        _picture.Width=_w;_picture.Height=_h;_image.Stretch=Stretch.Fill;_picture.Children.Add(_image);_picture.Children.Add(_overlay);_scroll.Content=_picture;zoom.ValueChanged+=(_,_)=>{_picture.LayoutTransform=new ScaleTransform(zoom.Value,zoom.Value);DrawCorners();};
        _mode.SelectionChanged+=(_,_)=>{CancelJob();_preview=_working;ResetSliders();UpdateMode();Show();};_mode.SelectedIndex=mode;UpdateMode();Show();Loaded+=(_,_)=>zoom.Value=Math.Clamp(Math.Min(Math.Max(40,_scroll.ActualWidth-20)/_w,Math.Max(40,_scroll.ActualHeight-20)/_h),.05,4);
        _overlay.MouseLeftButtonDown+=async(_,e)=>{if(_running)return;var p=e.GetPosition(_overlay);int x=Math.Clamp((int)p.X,0,_w-1),y=Math.Clamp((int)p.Y,0,_h-1);if(_mode.SelectedIndex==5){int at=(y*_w+x)*4;PickedColor=$"#{_working[at+2]:X2}{_working[at+1]:X2}{_working[at]:X2}";DialogResult=true;return;}if(_mode.SelectedIndex==2){_image.Source=ImageTransfer.Bitmap(_w,_h,_working);_corner=Enumerable.Range(0,4).OrderBy(i=>Math.Pow(_corners[i].X-p.X,2)+Math.Pow(_corners[i].Y-p.Y,2)).First();_overlay.CaptureMouse();return;}
            if(_mode.SelectedIndex==1&&(Keyboard.Modifiers&ModifierKeys.Alt)!=0){_source=(x,y);_status.Text=$"Nguồn Clone: {x}, {y}. Kéo để tô.";return;}
            if(_mode.SelectedIndex==4&&!_restore.IsChecked.GetValueOrDefault()){int tolerance=(int)_tolerance.Value;await Process(token=>ImageStudioPixels.RemoveConnected(_working,_w,_h,x,y,tolerance,token),true);return;}
            if(_mode.SelectedIndex!=1&&!(_mode.SelectedIndex==4&&_restore.IsChecked==true))return;if(_mode.SelectedIndex==1&&_source==null){_status.Text="Alt + click chọn nguồn, rồi kéo để tô.";return;}Push();_cloneSource=(byte[])_working.Clone();_last=(x,y);_drawing=true;_overlay.CaptureMouse();Brush(x,y,x,y);};
        _overlay.MouseMove+=(_,e)=>{var p=e.GetPosition(_overlay);int x=Math.Clamp((int)p.X,0,_w-1),y=Math.Clamp((int)p.Y,0,_h-1);if(_corner>=0){_corners[_corner]=(x,y);DrawCorners();}else if(_drawing&&_last is {} last){int count=Math.Max(1,(int)(Math.Sqrt(Math.Pow(x-last.X,2)+Math.Pow(y-last.Y,2))/Math.Max(1,_radius.Value/3)));for(int i=1;i<=count;i++)Brush(last.X+(x-last.X)*i/count,last.Y+(y-last.Y)*i/count,_last.Value.X,_last.Value.Y);_last=(x,y);}};
        _overlay.MouseLeftButtonUp+=(_,_)=>{if(_drawing)Show();_drawing=false;_cloneSource=null;_corner=-1;_overlay.ReleaseMouseCapture();};_overlay.LostMouseCapture+=(_,_)=>{_drawing=false;_corner=-1;};
        _brushFrame.Tick+=(_,_)=>{if(_brushDirty){_brushDirty=false;Show();}};_brushFrame.Start();
        _debounce.Tick+=async(_,_)=>{_debounce.Stop();await Preview();};Closed+=(_,_)=>{_closed=true;_brushFrame.Stop();CancelJob();};
    }
    private (int X,int Y)? _strokeStart;
    private void Brush(int x,int y,int ax,int ay){if(_cloneSource==null)return;if(_strokeStart==null)_strokeStart=(ax,ay);var start=_strokeStart.Value;var source=_source??(x,y);ImageStudioPixels.CloneBrush(_working,_mode.SelectedIndex==4?_original:_cloneSource,_w,_h,x,y,source.Item1+x-start.X,source.Item2+y-start.Y,(int)_radius.Value,_mode.SelectedIndex==4);_preview=_working;_brushDirty=true;}
    private void Push(){if(_undo.Count>=8||(long)(_undo.Count+1)*_working.Length>256_000_000)_undo.Clear();_undo.Push((byte[])_working.Clone());_strokeStart=null;}
    private async void CommitPreview(){if(_debounce.IsEnabled){_debounce.Stop();await Preview();}if(_running||!_previewValid)return;Push();_working=(byte[])_preview.Clone();ResetSliders();Show();}
    private void ResetSliders(){foreach(var v in _adjust)v.Value=0;_angle.Value=0;_debounce.Stop();_previewValid=true;}
    private void Schedule(){_previewValid=false;_debounce.Stop();_debounce.Start();}
    private void CancelJob(){_running=false;_debounce.Stop();_version++;_cts?.Cancel();}
    private async Task Preview(){var values=_adjust.Select(s=>s.Value).ToArray();var angle=_angle.Value;var corners=_corners.ToArray();int mode=_mode.SelectedIndex;await Process(token=>mode==2?ImageStudioPixels.Perspective(_working,_w,_h,corners,token):mode==3?ImageStudioPixels.Rotate(_working,_w,_h,angle,token):ImageStudioPixels.Adjust(_working,_w,_h,values[0],values[1],values[2],values[3],values[4],token),false);}
    private async Task Process(Func<CancellationToken,byte[]> operation,bool commit){_cts?.Cancel();var cts=_cts=new();int version=++_version;_running=true;_status.Text="Đang dựng xem trước…";try{var result=await Task.Run(()=>operation(cts.Token));if(_closed||version!=_version)return;if(commit){Push();_working=result;}_preview=result;_previewValid=true;Show();_status.Text="Xem trước · chưa thay đổi ảnh trong Editor.";}catch(OperationCanceledException){}catch(Exception ex){_previewValid=false;if(!_closed)_status.Text=ex.Message;}finally{if(version==_version)_running=false;cts.Dispose();if(ReferenceEquals(_cts,cts))_cts=null;}}
    private void UpdateMode(){_color.Visibility=_mode.SelectedIndex==0?Visibility.Visible:Visibility.Collapsed;_brush.Visibility=_mode.SelectedIndex is 1 or 4?Visibility.Visible:Visibility.Collapsed;_perspective.Visibility=_mode.SelectedIndex==2?Visibility.Visible:Visibility.Collapsed;_rotation.Visibility=_mode.SelectedIndex==3?Visibility.Visible:Visibility.Collapsed;_status.Text=_mode.SelectedIndex==5?"Click điểm cần lấy màu. Có thể zoom để chọn đúng pixel.":_mode.SelectedIndex==1?"Alt + click chọn nguồn; kéo để sao chép. Cọ lấy từ ảnh trước nét vẽ.":_mode.SelectedIndex==4?"Click vùng nền đồng màu để xóa phần liên thông. Bật cọ khôi phục để giữ lại chi tiết. Không phải tách chủ thể AI.":"Chỉnh, xem trước, rồi áp dụng. Ctrl+Z trong Editor để hoàn tác sau áp dụng.";DrawCorners();}
    private new void Show(){_image.Source=ImageTransfer.Bitmap(_w,_h,_before.IsChecked==true?_original:_preview);DrawCorners();}
    private void DrawCorners(){_overlay.Children.Clear();if(_mode.SelectedIndex!=2&&_mode.SelectedIndex!=3)return;for(int i=1;i<3;i++){double t=i/3d;Line((0,_h*t),(_w,_h*t));Line((_w*t,0),(_w*t,_h));}if(_mode.SelectedIndex!=2)return;for(int i=0;i<4;i++){Line(_corners[i],_corners[(i+1)%4]);double scale=(_picture.LayoutTransform as ScaleTransform)?.ScaleX??1;double size=12/scale;var dot=new Ellipse{Width=size,Height=size,Fill=Brushes.White,Stroke=Brushes.Black,StrokeThickness=1/scale,IsHitTestVisible=false};Canvas.SetLeft(dot,_corners[i].X-size/2);Canvas.SetTop(dot,_corners[i].Y-size/2);_overlay.Children.Add(dot);}}
    private void Line((double X,double Y)a,(double X,double Y)b){_overlay.Children.Add(new Line{X1=a.X,Y1=a.Y,X2=b.X,Y2=b.Y,Stroke=Brushes.MediumPurple,StrokeThickness=1/Math.Max(.05,(_picture.LayoutTransform as ScaleTransform)?.ScaleX??1),IsHitTestVisible=false});}
}
