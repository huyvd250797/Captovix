using System.IO;
using System.Windows;
using System.Windows.Controls;
namespace Captovix;
public partial class MainWindow
{
    private FrameworkElement BuildStorageSettings(){
        var panel=new StackPanel{Margin=new(12)};panel.Children.Add(new TextBlock{Text="Dung lượng & bộ nhớ",FontSize=22,FontWeight=FontWeights.SemiBold});
        panel.Children.Add(new TextBlock{Text="Chỉ dọn thumbnail/cache/tệp tạm của Captovix. Ảnh gốc, bản chỉnh sửa, bản sao lưu và KEY được giữ lại. Tệp đang dùng hoặc vừa thay đổi sẽ được bỏ qua.",TextWrapping=TextWrapping.Wrap,Margin=new(0,10,0,12)});
        var rows=new StackPanel();panel.Children.Add(rows);var status=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new(0,10,0,10)};panel.Children.Add(status);
        var actions=new StackPanel{Orientation=Orientation.Horizontal};var refresh=new Button{Content="Kiểm tra dung lượng"};var clearRam=new Button{Content="Giải phóng cache RAM"};var clean=new Button{Content="Dọn mục đã chọn",IsEnabled=false};actions.Children.Add(refresh);actions.Children.Add(clearRam);actions.Children.Add(clean);panel.Children.Add(actions);
        var checks=new List<(CheckBox Check,StorageCategory Category)>();DateTime scanTime=DateTime.UtcNow;bool working=false;
        void Busy(bool busy){working=busy;refresh.IsEnabled=clearRam.IsEnabled=!busy;clean.IsEnabled=!busy&&checks.Any(x=>x.Category.CanClean);}
        async Task Scan(){if(working)return;Busy(true);status.Text="Đang thống kê…";try{
            var paths=_store.Items.Select(x=>x.FilePath).ToArray();scanTime=DateTime.UtcNow;
            var categories=await Task.Run(()=>new[]{StorageMaintenance.Inspect("Thư mục ứng dụng (giữ lại)",AppContext.BaseDirectory),StorageMaintenance.Originals(paths),StorageMaintenance.Inspect("Bản chỉnh sửa & sao lưu (giữ lại)",Path.Combine(App.DataRoot,"Edits")),StorageMaintenance.Inspect("Thumbnail · tạo lại khi cần",Path.Combine(App.DataRoot,"Thumbnails"),true),StorageMaintenance.Inspect("Cache · tạo lại khi cần",Path.Combine(App.DataRoot,"Cache"),true),StorageMaintenance.Inspect("Tệp tạm · cũ hơn 24 giờ",Path.Combine(App.DataRoot,"Temp"),true),StorageMaintenance.Inspect("OCR tạm · cũ hơn 24 giờ",Path.Combine(App.DataRoot,"OcrJobs"),true)});
            rows.Children.Clear();checks.Clear();foreach(var c in categories){var check=new CheckBox{Content=$"{c.Name}: {StorageMaintenance.Size(c.Bytes)} · {c.Files.Count} tệp"+(c.Unreadable>0?$" · chưa đọc được {c.Unreadable} mục":""),IsEnabled=c.CanClean,IsChecked=false};rows.Children.Add(check);checks.Add((check,c));}
            status.Text=$"Cache thumbnail RAM: {StorageMaintenance.Size(_store.ThumbnailCacheStats.Bytes)} · cache lớp ảnh: {StorageMaintenance.Size(RasterPixels.CacheStats.Bytes)}. Chọn mục cần dọn; số liệu ổ đĩa là kích thước tệp, chưa tính cấp phát filesystem.";
        }catch(Exception ex){App.Log(ex);status.Text="Chưa thống kê được: "+ex.Message;}finally{Busy(false);}}
        refresh.Click+=async(_,_)=>await Scan();
        clearRam.Click+=(_,_)=>{_store.ClearThumbnailCache();EditorRenderer.ReleaseImageCaches();foreach(var item in _recentItems)_store.RequestThumbnail(item);status.Text="Đã bỏ cache RAM; dữ liệu sẽ được nạp lại khi cần.";};
        clean.Click+=async(_,_)=>{if(working)return;var selected=checks.Where(x=>x.Check.IsChecked==true&&x.Category.CanClean).Select(x=>x.Category).ToArray();if(selected.Length==0){status.Text="Chọn ít nhất một mục có thể dọn.";return;}
            if(AppDialog.Show(_settingsDialog??this,"Dọn các mục đã chọn? Thumbnail được tạo lại từ ảnh và bản chỉnh sửa khi cần.","Dọn dung lượng",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;
            Busy(true);string message;try{var result=await Task.Run(()=>StorageMaintenance.Clean(App.DataRoot,selected,scanTime.AddHours(-24)));_store.ClearThumbnailCache();message=$"Đã dọn {StorageMaintenance.Size(result.Bytes)} · {result.Removed} tệp; bỏ qua {result.Skipped} tệp mới/đang dùng/đã thay đổi.";}catch(Exception ex){App.Log(ex);message="Chưa dọn xong: "+ex.Message;}finally{Busy(false);}await Scan();foreach(var item in _recentItems)_store.RequestThumbnail(item);status.Text=message;
        };
        bool loaded=false;panel.Loaded+=async(_,_)=>{if(loaded)return;loaded=true;await Scan();};return panel;
    }
}
