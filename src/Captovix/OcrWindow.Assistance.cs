using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
internal sealed partial class OcrWindow
{
    public AssistRequest? Request {get;private set;}
    public bool CreateCopy=>_createCopy.IsChecked==true;
    private readonly CheckBox _createCopy=new(){Content="Tạo bản ảnh mới để chỉnh (giữ ảnh hiện tại)"};
    private readonly ComboBox _operation=new(){ItemsSource=new[]{"Thay chữ trên nền màu phẳng","Tách vùng thành lớp để di chuyển"},SelectedIndex=0};
    private readonly ComboBox _font=new(){ItemsSource=new[]{12,14,16,18,20,24,28,32,40,48,64},SelectedIndex=5};
    private readonly Button _applyAssist=new(){Content="Áp dụng vào Editor",IsEnabled=false};
    private readonly ListBox _suggestions=new(){MaxHeight=105,DisplayMemberPath="Text"};
    private readonly TextBlock _assistStatus=new(){TextWrapping=TextWrapping.Wrap,Margin=new(4,8,4,8)};
    private string _background="#FFFFFF",_foreground="#202630";
    private bool _assistEnabled;
    private Action? _paintColors;
    private PixelRect _recognizedRegion;
    private void BuildAssistance(StackPanel settings,bool assist,bool allow)
    {
        settings.Children.Add(new TextBlock{Text="Dòng chữ gợi ý · chọn sau khi nhận dạng",Margin=new(4,10,4,4)});settings.Children.Add(_suggestions);
        _suggestions.SelectionChanged+=(_,_)=>{if(_job!=null||_suggestions.SelectedItem is not OcrTextLine line)return;var b=line.Bounds;_region=OcrContract.Selection(_recognizedRegion.X+b.X-3,_recognizedRegion.Y+b.Y-3,_recognizedRegion.X+b.Right+3,_recognizedRegion.Y+b.Bottom+3,_snapshot.PixelWidth,_snapshot.PixelHeight);_result.Text=line.Text;if(_assistEnabled){_font.SelectedItem=_font.Items.Cast<int>().OrderBy(size=>Math.Abs(size-b.Height*1.1)).First();SuggestAssistBackground();}InvalidateAssist();DrawRegion();};
        _assistEnabled=assist;if(!assist)return;Title="Captovix · Chỉnh sửa ảnh hỗ trợ";
        settings.Children.Add(new TextBlock{Text="Chỉnh sửa hỗ trợ",FontSize=18,FontWeight=FontWeights.SemiBold,Margin=new(4,16,4,8)});
        settings.Children.Add(new TextBlock{Text="Sửa chữ ở ô kết quả bên dưới. Nền được lấp bằng màu phẳng; không tự phục hồi ảnh/gradient. Luôn xem trước trước khi áp dụng.",TextWrapping=TextWrapping.Wrap,Margin=new(4,4,4,8)});
        settings.Children.Add(_operation);settings.Children.Add(new TextBlock{Text="Cỡ chữ · font Segoe UI",Margin=new(4)});settings.Children.Add(_font);
        var colors=new WrapPanel();var background=new Button{Content="Màu nền"};var foreground=new Button{Content="Màu chữ"};var sample=new Button{Content="Lấy màu viền vùng"};colors.Children.Add(background);colors.Children.Add(foreground);colors.Children.Add(sample);settings.Children.Add(colors);
        void Paint(){background.Background=EditorRenderer.Ink(_background);background.Foreground=AppearanceSettings.WhiteText(_background)?Brushes.White:Brushes.Black;foreground.Background=EditorRenderer.Ink(_foreground);foreground.Foreground=AppearanceSettings.WhiteText(_foreground)?Brushes.White:Brushes.Black;}
        background.Click+=(_,_)=>{if(NativeColorPicker.Pick(this) is {} c){_background=c;Paint();InvalidateAssist();}};foreground.Click+=(_,_)=>{if(NativeColorPicker.Pick(this) is {} c){_foreground=c;Paint();InvalidateAssist();}};
        sample.Click+=(_,_)=>{try{OcrContract.Validate(_region.Width,_region.Height,"eng",3);var crop=new CroppedBitmap(_snapshot,new(_region.X,_region.Y,_region.Width,_region.Height));var value=AssistedPixels.Sample(ImageTransfer.Bgra(crop),crop.PixelWidth,crop.PixelHeight,new(0,0,crop.PixelWidth,crop.PixelHeight));_background=value.Color;Paint();InvalidateAssist();_assistStatus.Text=value.Spread>18?"Nền thay đổi nhiều: mẫu màu có thể không phù hợp. Chọn màu thủ công và xem trước.":"Đã lấy màu nền gợi ý. Kiểm tra bằng Xem trước.";}catch(Exception ex){_assistStatus.Text=ex.Message;}};_paintColors=Paint;Paint();
        _createCopy.IsChecked=!allow;_createCopy.IsEnabled=allow;settings.Children.Add(_createCopy);
        if(!allow)settings.Children.Add(new TextBlock{Text="Ảnh đang xoay/lật/resize: sẽ tạo bản ảnh đã ghép để chỉnh, giữ nguyên dự án cũ.",TextWrapping=TextWrapping.Wrap,Margin=new(4)});
        var actions=new WrapPanel();var preview=new Button{Content="Xem trước thay đổi"};var reset=new Button{Content="Xem ảnh trước sửa"};actions.Children.Add(preview);actions.Children.Add(reset);actions.Children.Add(_applyAssist);settings.Children.Add(actions);settings.Children.Add(_assistStatus);
        _operation.SelectionChanged+=(_,_)=>InvalidateAssist();_font.SelectionChanged+=(_,_)=>InvalidateAssist();_createCopy.Checked+=(_,_)=>InvalidateAssist();_createCopy.Unchecked+=(_,_)=>InvalidateAssist();
        preview.Click+=(_,_)=>{if(_job!=null)return;try{OcrContract.Validate(_region.Width,_region.Height,"eng",3);var request=new AssistRequest(_region,_result.Text,_background,_foreground,(int)_font.SelectedItem,_operation.SelectedIndex==1);_previewImage.Source=AssistedEditing.Preview(_snapshot,request);Request=request;_applyAssist.IsEnabled=true;_assistStatus.Text=request.Extract?"Áp dụng sẽ tạo lớp ảnh tại vùng này; kéo lớp đó trong Editor để di chuyển. Lớp nền màu ở lại vị trí cũ.":"Đang xem chữ thay thế. Kiểm tra font, cỡ chữ và nền trước khi áp dụng.";}catch(Exception ex){InvalidateAssist();_assistStatus.Text=ex.Message;}};
        reset.Click+=(_,_)=>InvalidateAssist();_applyAssist.Click+=(_,_)=>{if(_job==null&&Request!=null)DialogResult=true;};
    }
    private void SuggestAssistBackground()
    {
        if(!_assistEnabled||_region.Width<2||_region.Height<2||(long)_region.Width*_region.Height>OcrContract.MaxPixels)return;
        try{var crop=new CroppedBitmap(_snapshot,new(_region.X,_region.Y,_region.Width,_region.Height));var sampled=AssistedPixels.Sample(ImageTransfer.Bgra(crop),crop.PixelWidth,crop.PixelHeight,new(0,0,crop.PixelWidth,crop.PixelHeight));_background=sampled.Color;_foreground=AppearanceSettings.WhiteText(_background)?"#FFFFFF":"#202630";_paintColors?.Invoke();_assistStatus.Text=sampled.Spread>18?"Nền không đồng nhất: màu gợi ý có thể sai. Chọn màu và xem trước.":"Đã gợi ý màu nền/chữ. Xem trước trước khi áp dụng.";}catch(Exception ex){_assistStatus.Text=ex.Message;}
    }
    private void InvalidateAssist(){Request=null;_applyAssist.IsEnabled=false;if(_previewImage.Source!=null)_previewImage.Source=_snapshot;}
    private void SetSuggestions(OcrTextLine[]? lines)=>_suggestions.ItemsSource=lines??[];
}
public partial class MainWindow
{
    private void ApplyAssisted(AssistRequest request,BitmapSource snapshot,bool createCopy)
    {
        if(_session==null)return;
        if(createCopy){
            var doc=new EditorDocument{OriginalWidth=snapshot.PixelWidth,OriginalHeight=snapshot.PixelHeight};_=AssistedEditing.Objects(request,new ImageLayout(doc.Viewport,doc.Appearance),doc,snapshot);
            string path=_store.NewPath(false);ImageTransfer.Save(snapshot,path);var item=new MediaItem{FilePath=path,Title="Bản chỉnh sửa hỗ trợ "+DateTime.Now.ToString("dd-MM HH:mm:ss"),Width=snapshot.PixelWidth,Height=snapshot.PixelHeight,Source="Bản ảnh đã render; ảnh/dự án trước được giữ nguyên"};_store.Add(item);SearchBox.Clear();MediaList.SelectedItem=item;MediaList.ScrollIntoView(item);
        }
        var objects=AssistedEditing.Objects(request,Surface.Layout!,_session.Document,snapshot);_session.Change(d=>{d.Annotations.AddRange(objects);if(d.Annotations.Count>10000)throw new InvalidOperationException("Vượt giới hạn đối tượng.");});
        SelectTool(EditorTool.Select);Surface.SelectIds([objects[1].Id]);Surface.Refresh();SyncProperties();FlushEdits(false);SetStatus("Đã áp dụng chỉnh sửa hỗ trợ · đối tượng vẫn sửa/di chuyển được · Ctrl+Z hoàn tác.");
    }
}
