using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
namespace Captovix;
public partial class MainWindow
{
    private void OpenMockupMenu(Button anchor)
    {
        if(!RequireLicense()||_session==null||_busy)return;
        var body=new StackPanel{Width=330,Margin=new(12)};body.Children.Add(new TextBlock{Text="Mockup",FontSize=18,FontWeight=FontWeights.SemiBold,Margin=new(3,3,3,10)});
        void Add(string label,Action action){var button=new Button{Content=AppIcons.Content("Mockup",label),HorizontalContentAlignment=HorizontalAlignment.Left,Padding=new(10,9,10,9)};button.Click+=(_,_)=>{if(_studioPopup!=null)_studioPopup.IsOpen=false;action();};body.Children.Add(button);}
        Add("Sửa một vùng chữ / nút",()=>Mockup_Click(this,new()));
        Add("Quét vùng phần tử…",()=>{_mockupScanRegion=true;SelectTool(EditorTool.Mockup);Surface.Focus();SetStatus("Mockup · Kéo vùng cần quét; Alt+kéo nếu chọn chồng đối tượng. Kết quả phải được rà soát trước khi áp dụng.");});
        Add("Quét toàn ảnh đang hiển thị…",async()=>{if(_session!=null)await OpenElementScanAsync(_session.Document.Viewport);});
        Add("Nền & mặt nạ đã tách…",()=>RecoveryEditor_Click(this,new()));
        body.Children.Add(new TextBlock{Text="Chữ · nút · ô nhập · khối giao diện. Nền màu phẳng; không tự áp dụng kết quả quét.",TextWrapping=TextWrapping.Wrap,Margin=new(3,10,3,3)});OpenStudioPopup(anchor,body);
    }
    private async Task OpenElementScanAsync(PixelRect region)
    {
        if(_mockupOpen||!RequireLicense()||_session is not {} session||Surface.Original is not {} original||_busy)return;
        try{
            if(!FlushEdits())return;var document=session.Document.Snapshot();region=region.Intersect(document.Viewport);
            if(region.Width<8||region.Height<8||(long)region.Width*region.Height>OcrContract.MaxPixels){SetStatus("Quét phần tử: chọn vùng8×8px đến20MP. Với ảnh dài/lớn hơn20MP, hãy dùng Quét vùng.");return;}
            _mockupOpen=true;long revision=session.Revision;
            async Task<BitmapSource> Load(CancellationToken token){BitmapSource? snapshot=null;await StaImageWorker.Run(()=>{try{token.ThrowIfCancellationRequested();snapshot=EditorRenderer.MockupSnapshot(original,document,region);token.ThrowIfCancellationRequested();}finally{EditorRenderer.ReleaseImageCaches(original);}});return snapshot!;}
            var active=_activeItem;var hybrid=active==null?null:await Task.Run(()=>HybridCapture.Load(App.DataRoot,active.Id,active.FilePath,document.OriginalWidth,document.OriginalHeight));
            var dialog=new ElementScanReviewWindow(region,document,Load,hybrid){Owner=this};if(dialog.ShowDialog()!=true||dialog.Result is not {} result)return;
            if(!ReferenceEquals(_session,session)||!ReferenceEquals(Surface.Original,original)||session.Revision!=revision){SetStatus("Ảnh đã thay đổi trong lúc rà soát; chưa áp dụng kết quả. Hãy quét lại ảnh hiện tại.");return;}
            var prior=session.Document.Annotations.Select(x=>x.Id).ToHashSet();session.Change(d=>ElementScan.Apply(d,result,region));var added=session.Document.Annotations.Where(x=>!prior.Contains(x.Id)).Select(x=>x.Id).ToArray();Surface.SelectIds(added);Surface.Refresh();SyncProperties();SetStatus($"Đã áp dụng {added.Length} phần tử đã chọn · Ctrl+Z hoàn tác toàn bộ lần áp dụng · nhấp đúp phần tử để sửa Mockup.");
        }catch(Exception ex){App.Log(ex);SetStatus("Chưa áp dụng quét phần tử: "+ex.Message);}finally{_mockupOpen=false;}
    }
}
