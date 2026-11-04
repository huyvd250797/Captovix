using System.Windows;
using System.Windows.Media.Imaging;
namespace Captovix;
public partial class MainWindow
{
    private void RecoveryEditor_Click(object sender,RoutedEventArgs e)
    {
        if(!RequireLicense()||_session is not {} session||Surface.Original is not {} original||_busy||!FlushEdits())return;
        try{
            var patches=session.Document.Recoveries.Select(p=>p.Copy()).ToList();string? id=Surface.Selected?.Mockup?.RecoveryId;bool existing=true;
            if(Surface.Selected is {Mockup:not null} a&&id==null){patches=[Surface.MakeRecoveryDraft(a)];existing=false;}else if(id!=null)patches=patches.OrderBy(p=>p.Id==id?0:1).ToList();
            if(patches.Count==0){SetStatus("Chưa có nền đã tách. Chọn một phần tử Mockup để chuẩn bị nền.");return;}
            string? target=Surface.Selected?.Id;long revision=session.Revision;var source=RasterPixels.Source(original,session.Document);
            (BitmapSource,PixelRect) Load(RecoveryPatch patch){var r=patch.Bounds;var sampled=new PixelRect(r.X-4,r.Y-4,r.Width+8,r.Height+8).Intersect(new(0,0,session.Document.OriginalWidth,session.Document.OriginalHeight));var crop=new CroppedBitmap(source,new(sampled.X,sampled.Y,sampled.Width,sampled.Height));return(ImageTransfer.Detach(crop),sampled);}
            var dialog=new RecoveryEditorWindow(patches,Load,existing,session.Document.RecoveryHistory){Owner=this};if(dialog.ShowDialog()!=true||dialog.Result is not {} result)return;if(!ReferenceEquals(session,_session)||session.Revision!=revision){SetStatus("Ảnh đã thay đổi; chưa áp dụng nền.");return;}
            session.Change(d=>{if(dialog.Remove){d.Recoveries.RemoveAll(p=>p.Id==result.Id);foreach(var item in d.Annotations.Where(x=>x.Mockup?.RecoveryId==result.Id)){item.Mockup!.RecoveryId=null;item.Mockup.EraseOriginal=false;}}
                else{int at=d.Recoveries.FindIndex(p=>p.Id==result.Id);if(at<0)d.Recoveries.Add(result.Copy());else d.Recoveries[at]=result.Copy();foreach(var item in d.Annotations.Where(x=>x.Mockup!=null&&(x.Mockup.RecoveryId==result.Id||!existing&&x.Id==target))){item.Mockup!.RecoveryId=result.Id;item.Mockup.EraseOriginal=false;}}d.Validate(d.OriginalWidth,d.OriginalHeight,d.SourceHash);});Surface.Refresh();SyncProperties();SetStatus("Đã cập nhật nền và vùng giữ · Ctrl+Z hoàn tác.");
        }catch(Exception ex){App.Log(ex);SetStatus("Chưa phục hồi nền: "+ex.Message);}
    }
}
