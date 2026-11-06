using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
namespace Captovix;
public partial class MainWindow
{
    private bool _mockupOpen,_mockupScanRegion;
    private void InitializeMockup()
    {
        Surface.MockupRequested+=async region=>{if(_mockupScanRegion)await OpenElementScanAsync(region);else await OpenMockupAsync(region,null);};
        Surface.MockupEditRequested+=async()=>await EditMockupAsync();
    }
    private void Mockup_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||_session==null||_busy)return;
        _mockupScanRegion=false;SelectTool(EditorTool.Mockup);Surface.Focus();SetStatus("Mockup · Kéo trọn vùng chữ/nút cũ trên nền màu phẳng. Nhấp đúp đối tượng Mockup để sửa.");
    }
    private async void EditMockup_Click(object sender,RoutedEventArgs e)=>await EditMockupAsync();
    private async Task EditMockupAsync()
    {
        if(Surface.Selected is not {Mockup:not null} a)return;
        if(a.Locked||!a.Visible){SetStatus("Mở khóa và hiện đối tượng Mockup trước khi sửa.");return;}
        if(a.Mockup.Type==MockupKind.Button&&MockupControlPart(a,true) is {} caption){
            Surface.SelectInGroup([caption.Id],caption.Id,a.GroupId);
            a=caption;
        }
        OpenMockupInspector(a);
        if(a.Text.Length>0||a.Mockup!.Type is MockupKind.Label or MockupKind.Title)BeginInlineText();
        await Task.CompletedTask;
    }
    private async Task OpenMockupAsync(PixelRect region,Annotation? editing)
    {
        if(_mockupOpen||!RequireLicense()||_session is not {} session||Surface.Original is not {} original||_busy)return;
        try{
            if(!FlushEdits())return;
            if(region.Width<8||region.Height<8||(long)region.Width*region.Height>OcrContract.MaxPixels){SetStatus("Mockup: chọn vùng từ 8 × 8 px, tối đa 20 triệu pixel, nằm trong ảnh.");return;}
            _mockupOpen=true;long revision=session.Revision;var document=session.Document.Snapshot();string? editId=editing?.Id;
            if(editId!=null)document.Annotations.RemoveAll(a=>a.Id==editId);
            var sampled=CaptureSelection.Clip(new(region.X-4,region.Y-4,region.Width+8,region.Height+8),document.Viewport);
            var draft=editing?.Copy()??MockupStudio.Create(region,new(0,0,document.OriginalWidth,document.OriginalHeight),new("#FFFFFF","#FFFFFF","#202020",1));
            async Task<BitmapSource> Load(CancellationToken token){BitmapSource? snapshot=null;await StaImageWorker.Run(()=>{try{token.ThrowIfCancellationRequested();snapshot=EditorRenderer.MockupSnapshot(original,document,sampled);token.ThrowIfCancellationRequested();}finally{EditorRenderer.ReleaseImageCaches(original);}});return snapshot!;}
            var dialog=new MockupEditorWindow(draft,sampled,region,Load,editing!=null){Owner=this};
            if(dialog.ShowDialog()!=true||dialog.Result is not {} result)return;
            if(!ReferenceEquals(_session,session)||!ReferenceEquals(Surface.Original,original)||session.Revision!=revision){SetStatus("Ảnh đã thay đổi trong lúc mở Mockup; chưa áp dụng. Hãy chọn lại đối tượng/vùng trên ảnh hiện tại.");return;}
            session.Change(d=>MockupStudio.Apply(d,result,editId));Surface.SelectIds([result.Id]);Surface.Refresh();SyncProperties();SetStatus("Đã áp dụng Mockup · Ctrl+Z hoàn tác · nhấp đúp hoặc Sửa Mockup để sửa tiếp. Ảnh gốc giữ nguyên.");
        }catch(Exception ex){App.Log(ex);SetStatus("Chưa áp dụng Mockup: "+ex.Message);}
        finally{_mockupOpen=false;}
    }
    private void SyncMockup()
    {
        bool selected=Surface.Selected?.Mockup!=null;
        MockupProperties.Visibility=selected?Visibility.Visible:Visibility.Collapsed;
        if(!selected){_mockupInspector?.Clear();_inspectorSession=null;}
        if(selected){SyncMockupInspector(Surface.Selected!);ShapeStudioPanel.Visibility=Visibility.Collapsed;VisualStudioPanel.Visibility=Visibility.Collapsed;PropertyTitle.Text="Mockup · "+ElementScan.Label(Surface.Selected!.Mockup!.Type);var recovery=_session?.Document.Recoveries.FirstOrDefault(p=>p.Id==Surface.Selected.Mockup.RecoveryId);MockupDescription.Text=recovery==null?"Nhấp đúp sửa chữ ngay trên ảnh · thuộc tính bên phải. Di chuyển/xóa sẽ tách nền cũ; Ctrl+Z hoàn tác.":recovery.NeedsReview?"Nền đã tách · cần kiểm tra màu/vùng giữ trong Nền & mặt nạ.":"Nền đã tách · di chuyển/xóa không làm hiện lại phần tử cũ.";}
    }
}
