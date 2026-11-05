using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
namespace Captovix;
public partial class MainWindow
{
    private async void OpenMockupMenu(Button anchor)
    {
        if(_mockupOpen||!RequireLicense()||_session is not {} session||Surface.Original is not {} original||_busy)return;
        try{if(!FlushEdits())return;_mockupOpen=true;if(_studioPopup!=null)_studioPopup.IsOpen=false;var document=session.Document.Snapshot();int legacy=MockupRasterState.RemoveLegacyScans(document);long revision=session.Revision;
            async Task<BitmapSource> Preview(CancellationToken token){BitmapSource? result=null;await StaImageWorker.Run(()=>{try{token.ThrowIfCancellationRequested();if(document.Annotations.Count==0&&document.Recoveries.Count==0){var v=document.Viewport;var image=new System.Windows.Media.Imaging.CroppedBitmap(RasterPixels.Source(original,document),new(v.X,v.Y,v.Width,v.Height));image.Freeze();result=image;}else{if((long)document.Viewport.Width*document.Viewport.Height>20_000_000)throw new InvalidOperationException("Ảnh đã chỉnh vượt20MP; hãy Crop một vùng nhỏ hơn trước khi quét.");result=EditorRenderer.MockupSnapshot(original,document,document.Viewport);}}finally{EditorRenderer.ReleaseImageCaches(original);}});return result!;}
            var dialog=new MockupCaptureWindow(document.Viewport,Preview){Owner=this};if(legacy>0)dialog.SourceNotice=$"Phát hiện {legacy} phần tử quét cũ. Dùng lại ảnh nguồn để tránh vùng trắng che chữ; chỉ thay lớp quét cũ khi Áp dụng, có thể Undo.";if(dialog.ShowDialog()!=true)return;
            if(!ReferenceEquals(_session,session)||session.Revision!=revision)return;_mockupOpen=false;
            if(dialog.ExtraAction=="Manual"){Mockup_Click(this,new());return;}if(dialog.ExtraAction=="Recovery"){RecoveryEditor_Click(this,new());return;}
            if(dialog.ScanBounds is {} region)await OpenElementScanAsync(region,dialog.OcrLanguage);
        }catch(Exception e){App.Log(e);SetStatus("Không mở được Mockup: "+e.Message);}finally{_mockupOpen=false;}
    }
    private async Task OpenElementScanAsync(PixelRect region,int language=0)
    {
        if(_mockupOpen||!RequireLicense()||_session is not {} session||Surface.Original is not {} original||_busy)return;
        try{
            if(!FlushEdits())return;var document=session.Document.Snapshot();MockupRasterState.RemoveLegacyScans(document);region=region.Intersect(document.Viewport);
            if(region.Width<8||region.Height<8||(long)region.Width*region.Height>OcrContract.MaxPixels){SetStatus("Quét phần tử: chọn vùng8×8px đến20MP. Với ảnh dài/lớn hơn20MP, hãy dùng Quét vùng.");return;}
            _mockupOpen=true;long revision=session.Revision;
            async Task<BitmapSource> Load(CancellationToken token){BitmapSource? snapshot=null;await StaImageWorker.Run(()=>{try{token.ThrowIfCancellationRequested();snapshot=EditorRenderer.MockupSnapshot(original,document,region);token.ThrowIfCancellationRequested();}finally{EditorRenderer.ReleaseImageCaches(original);}});return snapshot!;}
            var active=_activeItem;var hybrid=active==null?null:await Task.Run(()=>HybridCapture.Load(App.DataRoot,active.Id,active.FilePath,document.OriginalWidth,document.OriginalHeight));
            var dialog=new ElementScanReviewWindow(region,document,Load,hybrid,language){Owner=this};if(dialog.ShowDialog()!=true||dialog.Result is not {} result)return;
            if(!ReferenceEquals(_session,session)||!ReferenceEquals(Surface.Original,original)||session.Revision!=revision){SetStatus("Ảnh đã thay đổi trong lúc rà soát; chưa áp dụng kết quả. Hãy quét lại ảnh hiện tại.");return;}
            var prior=session.Document.Annotations.Select(x=>x.Id).ToHashSet();session.Change(d=>{MockupRasterState.RemoveLegacyScans(d);var additions=ElementScan.Prepare(d,result,region);var preserving=MockupRasterState.Attach(additions,region,dialog.SourcePng!);d.Annotations.AddRange(preserving);d.Validate(d.OriginalWidth,d.OriginalHeight,d.SourceHash);});var added=session.Document.Annotations.Where(x=>!prior.Contains(x.Id)).Select(x=>x.Id).ToArray();SelectTool(EditorTool.Select);Surface.SelectIds(session.Document.Annotations.Where(a=>added.Contains(a.Id)&&a.Mockup?.RasterBacking!=true).Take(1).Select(a=>a.Id));Surface.Refresh();SyncProperties();if(Surface.Selected is {} selected)OpenMockupInspector(selected);SetStatus($"Đã áp dụng {added.Length} phần tử đã chọn · Ctrl+Z hoàn tác toàn bộ lần áp dụng · nhấp đúp phần tử để sửa Mockup.");
        }catch(Exception ex){App.Log(ex);SetStatus("Chưa áp dụng quét phần tử: "+ex.Message);}finally{_mockupOpen=false;}
    }
}
