using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Captovix;
internal sealed partial class ElementScanReviewWindow
{
    StackPanel BuildModelTools(){
        var host=new StackPanel();var content=new StackPanel();var expand=new Expander{Header="AI cấu trúc · tùy chọn, chỉ chạy khi bấm phân tích",Content=content};host.Children.Add(expand);
        var endpoint=new TextBox{Text="http://127.0.0.1:11434",ToolTip="Ollama API local hoặc HTTPS do bạn cấu hình"};var model=new ComboBox{IsEditable=true};var timeout=new TextBox{Text="60",Width=65};var message=new TextBlock{Text="Ảnh vùng quét sẽ gửi tới endpoint đã chọn. Model không nạp cùng app; chỉ trả gợi ý, không tự áp dụng.",TextWrapping=TextWrapping.Wrap};
        try{string path=Path.Combine(App.DataRoot,"ui-model.json");if(File.Exists(path)&&new FileInfo(path).Length<16000){var saved=JsonSerializer.Deserialize<UiModelOptions>(File.ReadAllText(path));if(saved!=null){endpoint.Text=saved.Endpoint;model.Text=saved.Model;timeout.Text=saved.TimeoutSeconds.ToString();}}}catch{}
        content.Children.Add(endpoint);content.Children.Add(model);var actions=new WrapPanel();var list=new Button{Content="Model đã cài"};var save=new Button{Content="Lưu cấu hình"};var analyze=new Button{Content="Phân tích AI"};var stop=new Button{Content="Dừng AI",IsEnabled=false};actions.Children.Add(list);actions.Children.Add(timeout);actions.Children.Add(save);actions.Children.Add(analyze);actions.Children.Add(stop);content.Children.Add(actions);content.Children.Add(message);
        UiModelOptions Options()=>new(endpoint.Text.Trim(),model.Text.Trim(),int.TryParse(timeout.Text,out var seconds)?seconds:60);
        save.Click+=(_,_)=>{try{var options=Options();options.Validate();MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"ui-model.json"),JsonSerializer.Serialize(options));message.Text="Đã lưu. Không tải hoặc nạp model khi khởi động app.";}catch(Exception e){message.Text=e.Message;}};
        list.Click+=async(_,_)=>{list.IsEnabled=false;try{var options=Options() with{Model="list"};options.Validate();using var http=new HttpClient(new HttpClientHandler{AllowAutoRedirect=false}){Timeout=TimeSpan.FromSeconds(5)};using var response=await http.GetAsync(options.Endpoint.TrimEnd('/')+"/api/tags",HttpCompletionOption.ResponseHeadersRead,_life.Token);response.EnsureSuccessStatusCode();if(response.Content.Headers.ContentLength>128000)throw new IOException("Danh sách model quá lớn.");using var input=await response.Content.ReadAsStreamAsync(_life.Token);using var bytes=new MemoryStream();var buffer=new byte[4096];int read;while((read=await input.ReadAsync(buffer,_life.Token))>0){if(bytes.Length+read>128000)throw new IOException("Danh sách model quá lớn.");bytes.Write(buffer,0,read);}using var json=JsonDocument.Parse(bytes.ToArray());model.ItemsSource=json.RootElement.GetProperty("models").EnumerateArray().Take(100).Select(m=>m.GetProperty("name").GetString()).Where(s=>s!=null&&s.Length<=120).ToArray();message.Text="Chọn model có khả năng vision đã cài trên endpoint.";}catch(Exception e){if(!_closed)message.Text="Chưa lấy được model: "+e.Message;}finally{list.IsEnabled=true;}};
        CancellationTokenSource? running=null;stop.Click+=(_,_)=>running?.Cancel();Closed+=(_,_)=>running?.Cancel();
        analyze.Click+=async(_,_)=>{if(!_ready||_snapshot==null||_pixels==null||!SaveEditor())return;using var cancel=CancellationTokenSource.CreateLinkedTokenSource(_life.Token);running=cancel;Ready(false);analyze.IsEnabled=list.IsEnabled=save.IsEnabled=false;stop.IsEnabled=true;message.Text="Đang phân tích cấu trúc trên endpoint…";
            try{var options=Options();options.Validate();byte[]? png=null;var source=_snapshot;await StaImageWorker.Run(()=>{double scale=Math.Min(1,1536d/Math.Max(source.PixelWidth,source.PixelHeight));BitmapSource image=scale<1?new TransformedBitmap(source,new ScaleTransform(scale,scale)):source;png=ImageTransfer.Png(image);});
                var proposals=await UiModelClient.AnalyzeAsync(options,png!,cancel.Token);if(_closed)return;int added=UiModelClient.Merge(_elements,proposals,_region,_pixels,_region.Width,_region.Height);ResetPreview();BuildList();message.Text=$"AI trả {proposals.Length} gợi ý, thêm {added} khung. Khung mới mặc định giữ ảnh; kiểm tra trước khi chọn chuyển đổi.";
            }catch(OperationCanceledException){if(!_closed)message.Text="Đã dừng AI; kết quả OCR/ảnh đang có giữ nguyên.";}catch(Exception e){if(!_closed)message.Text="AI chưa hoàn tất: "+e.Message+" · tiếp tục dùng ảnh/OCR.";}finally{running=null;if(!_closed){Ready(true);analyze.IsEnabled=list.IsEnabled=save.IsEnabled=true;stop.IsEnabled=false;LoadEditor();}}
        };
        return host;
    }
}
