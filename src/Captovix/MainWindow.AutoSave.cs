using System.Windows.Media;
namespace Captovix;
public partial class MainWindow
{
    private bool _autoSaveRunning;
    private Task? _autoSaveWork;
    private EditorSession? _autoSaveSession;
    private long _autoSaveRevision;
    private async Task AutoSaveAsync()
    {
        if(_autoSaveRunning||_session is not {} session||_activeItem is not {} item||Surface.Original is not {} image||!session.Dirty||session.InTransaction||_inlineId!=null)return;
        using var measurement=PerformanceTimings.Measure("document.autosave");
        _autoSaveRunning=true;long revision=session.Revision;
        var snapshot=new EditorSession(session.Document.Snapshot());var savedItem=new MediaItem{Id=item.Id,FilePath=item.FilePath};
        _autoSaveSession=session;_autoSaveRevision=revision;
        try{
            _autoSaveWork=StaImageWorker.Run(()=>{try{DocumentStore.Save(savedItem,snapshot,image);}finally{EditorRenderer.ReleaseImageCaches(image);}});
            await _autoSaveWork;
            if(ReferenceEquals(_session,session)&&session.MarkSaved(revision)){_store.RequestThumbnail(item);SavedLabel.Text="✓ Đã lưu";SavedLabel.Foreground=Brushes.SeaGreen;}
        }catch(Exception ex){App.Log(ex);if(ReferenceEquals(_session,session)){SavedLabel.Text="Chưa lưu · Ctrl+S thử lại";SetStatus("Không lưu được chú thích: "+ex.Message);}}
        finally{_autoSaveSession=null;_autoSaveWork=null;_autoSaveRevision=0;_autoSaveRunning=false;if(_session?.Dirty==true){_saveTimer.Stop();_saveTimer.Start();}}
    }
}
