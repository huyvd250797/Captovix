using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace Captovix;
internal sealed class MockupInspector:StackPanel
{
    readonly TextBox text=new(){AcceptsReturn=true,TextWrapping=TextWrapping.Wrap,MaxLength=2000,MinHeight=54,MaxHeight=120,VerticalScrollBarVisibility=ScrollBarVisibility.Auto},size=new(),padding=new(),spacing=new(),border=new(),radius=new();
    readonly ComboBox kind=new(),font=new(){IsEditable=true},align=new(),vertical=new(),language=new(),pickTarget=new();
    readonly CheckBox bold=new(){Content="B"},italic=new(){Content="I"},underline=new(){Content="U"},preview=new(){Content="So sánh gốc / sau"};
    readonly TextBox ink=new(),background=new(),fill=new(),stroke=new();readonly TextBlock error=new(){TextWrapping=TextWrapping.Wrap},status=new(){TextWrapping=TextWrapping.Wrap};
    readonly Image before=new(){MaxHeight=150},after=new(){MaxHeight=150};readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(180)};
    CancellationTokenSource? ocrJob;string? id;bool syncing;Annotation? current;long previewGeneration;
    public Action<string,MockupChanges>? Changed;public Action? EditText;public Func<int,CancellationToken,Task>? Recognize;public Func<int,Task>? PickScreen;public Func<Task<(BitmapSource Before,BitmapSource After)>>? RenderPreview;public Func<string?>? PickColor;
    public MockupInspector(){
        foreach(var k in Enum.GetValues<MockupKind>())kind.Items.Add(ElementScan.Label(k));Add("Loại",kind);Add("Nội dung",text);var edit=new Button{Content="Sửa chữ ngay trên ảnh"};edit.Click+=(_,_)=>{Flush();EditText?.Invoke();};Children.Add(edit);
        foreach(string f in new[]{"Segoe UI","Arial","Tahoma","Calibri","Times New Roman","Georgia","Cambria","Courier New"})font.Items.Add(f);Add("Font",font);Add("Cỡ chữ · px",size);
        var styles=new WrapPanel();foreach(var c in new[]{bold,italic,underline})styles.Children.Add(c);Children.Add(styles);
        foreach(string a in new[]{"Trái","Giữa","Phải"})align.Items.Add(a);foreach(string a in new[]{"Trên","Giữa","Dưới"})vertical.Items.Add(a);Add("Căn ngang",align);Add("Căn dọc",vertical);Add("Padding · px",padding);Add("Giãn dòng · 0 = mặc định",spacing);
        Color("Màu chữ",ink);Color("Nền che gốc",background);Color("Nền control",fill);Color("Viền",stroke);Add("Độ dày viền · 0 để tắt",border);Add("Bo góc · px",radius);
        foreach(string l in new[]{"Việt + Anh","Tiếng Việt","Tiếng Anh"})language.Items.Add(l);language.SelectedIndex=0;Add("OCR",language);var ocr=new Button{Content="Nhận chữ lại"};var stop=new Button{Content="Dừng OCR",IsEnabled=false};ocr.Click+=async(_,_)=>{if(!Flush())return;ocr.IsEnabled=false;stop.IsEnabled=true;using var job=new CancellationTokenSource(TimeSpan.FromSeconds(120));ocrJob=job;try{if(Recognize!=null)await Recognize(Math.Max(0,language.SelectedIndex),job.Token);}finally{if(ReferenceEquals(ocrJob,job))ocrJob=null;ocr.IsEnabled=true;stop.IsEnabled=false;}};stop.Click+=(_,_)=>ocrJob?.Cancel();Children.Add(ocr);Children.Add(stop);
        foreach(string t in new[]{"Nền che","Nền control","Chữ","Viền"})pickTarget.Items.Add(t);pickTarget.SelectedIndex=2;Add("Lấy màu trên màn hình",pickTarget);var eye=new Button{Content="Lấy màu…"};eye.Click+=async(_,_)=>{Flush();if(PickScreen!=null)await PickScreen(pickTarget.SelectedIndex);};Children.Add(eye);
        Children.Add(preview);Children.Add(before);Children.Add(after);Children.Add(status);Children.Add(error);error.SetResourceReference(TextBlock.ForegroundProperty,"Danger");
        foreach(var t in new[]{text,size,padding,spacing,border,radius,ink,background,fill,stroke})t.TextChanged+=(_,_)=>Schedule();
        font.AddHandler(TextBox.TextChangedEvent,new TextChangedEventHandler((_,_)=>Schedule()));foreach(var c in new[]{kind,font,align,vertical})c.SelectionChanged+=(_,_)=>Schedule();foreach(var c in new[]{bold,italic,underline})c.Click+=(_,_)=>Schedule();
        timer.Tick+=(_,_)=>Flush();LostKeyboardFocus+=(_,_)=>Flush();preview.Click+=async(_,_)=>await Preview();Unloaded+=(_,_)=>{ocrJob?.Cancel();Flush();ReleasePreviews();};
    }
    void Add(string label,Control control){Children.Add(new TextBlock{Text=label,Margin=new(3,7,0,1)});Children.Add(control);}
    void Color(string title,TextBox field){var row=new DockPanel();var button=new Button{Content="…",Width=34,Padding=new(2)};DockPanel.SetDock(button,Dock.Right);row.Children.Add(button);row.Children.Add(field);Add(title,new ContentControl{Content=row});button.Click+=(_,_)=>{var value=PickColor?.Invoke();if(value!=null){field.Text=value;Flush();}};}
    static string N(double n)=>n.ToString("0.###",CultureInfo.InvariantCulture);
    static double Read(TextBox t,double min,double max){if(!double.TryParse(t.Text,NumberStyles.Float,CultureInfo.CurrentCulture,out var n)&&!double.TryParse(t.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out n)||!double.IsFinite(n)||n<min||n>max)throw new ArgumentException($"Giá trị cần từ {min} đến {max}.");return n;}
    public void Load(Annotation a){if(syncing)return;if(id!=a.Id){ocrJob?.Cancel();Flush();}id=a.Id;current=a.Copy();syncing=true;try{text.Text=a.Text;font.Text=a.FontFamily;size.Text=N(a.FontSize);bold.IsChecked=a.Bold;italic.IsChecked=a.Italic;underline.IsChecked=a.Underline;kind.SelectedIndex=(int)a.Mockup!.Type;align.SelectedIndex=(int)a.TextAlignment;vertical.SelectedIndex=(int)a.VerticalAlignment;padding.Text=N(a.TextPadding??0);spacing.Text=N(a.LineSpacing);ink.Text=a.TextColor??a.Color;background.Text=a.Mockup.Background;fill.Text=a.FillColor;stroke.Text=a.Color;border.Text=N(a.StrokeEnabled?a.Stroke:0);radius.Text=N(a.CornerRadius);IsEnabled=!a.Locked&&a.Visible;error.Text="";}finally{syncing=false;}ReleasePreviews();preview.IsChecked=false;}
    public bool Flush(){timer.Stop();if(syncing||id==null||current==null)return true;try{
        var c=new MockupChanges(text.Text,font.Text.Trim(),Read(size,8,240),bold.IsChecked==true,italic.IsChecked==true,underline.IsChecked==true,(TextAlign)align.SelectedIndex,(TextVertical)vertical.SelectedIndex,Read(padding,0,200),Read(spacing,0,3),ink.Text.Trim().ToUpperInvariant(),background.Text.Trim().ToUpperInvariant(),fill.Text.Trim().ToUpperInvariant(),stroke.Text.Trim().ToUpperInvariant(),Read(border,0,40),Read(radius,0,1000),(MockupKind)kind.SelectedIndex);
        if(c!=MockupEditing.Read(current)){syncing=true;try{Changed?.Invoke(id,c);}finally{syncing=false;}current.Text=c.Text;current.FontFamily=c.Font;current.FontSize=c.Size;current.Bold=c.Bold;current.Italic=c.Italic;current.Underline=c.Underline;current.TextColor=c.Ink;current.Mockup!.Background=c.Background;current.FillColor=c.Fill;current.Color=c.Stroke;current.StrokeEnabled=c.Border>0;current.Stroke=Math.Max(1,c.Border);current.CornerRadius=c.Radius;current.TextPadding=c.Padding;current.LineSpacing=c.Spacing;current.TextAlignment=c.Align;current.VerticalAlignment=c.Vertical;current.Mockup.Type=c.Kind;}
        error.Text="";return true;
    }catch(Exception ex){error.Text=ex.Message;return false;}}
    public void Clear(){if(syncing)return;ocrJob?.Cancel();Flush();timer.Stop();id=null;current=null;ReleasePreviews();}
    void Schedule(){if(syncing)return;timer.Stop();timer.Start();}
    public void SetPickedColor(int target,string value){new[]{background,fill,ink,stroke}[Math.Clamp(target,0,3)].Text=value;Flush();}
    public void SetText(string value){text.Text=value;Flush();}
    void ReleasePreviews(){previewGeneration++;before.Source=null;after.Source=null;}
    async Task Preview(){ReleasePreviews();if(preview.IsChecked!=true||RenderPreview==null)return;Flush();long generation=previewGeneration;try{var images=await RenderPreview();if(generation!=previewGeneration)return;before.Source=images.Before;after.Source=images.After;status.Text="Gốc / sau · bản xem nhỏ; ảnh xuất giữ chất lượng gốc.";}catch(Exception ex){error.Text=ex.Message;}}
}
