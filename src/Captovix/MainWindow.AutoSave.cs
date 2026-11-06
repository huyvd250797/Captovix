using System.Windows.Media;
namespace Captovix;
public partial class MainWindow
{
    private bool _autoSaveRunning;
    private Task? _autoSaveWork;
    private EditorSession? _autoSaveSession;
    private long _autoSaveRevision;
    private async Task<bool> AutoSaveAsync()
    {
        if(_autoSaveRunning||_session is not {} session||_activeItem is not {} item||Surface.Original is not {} image||!session.Dirty||session.InTransaction||_inlineId!=null)return false;
        using var measurement=PerformanceTimings.Measure("document.autosave");
        _autoSaveRunning=true;long revision=session.Revision;
        var snapshot=new EditorSession(session.Document.Snapshot());var savedItem=new MediaItem{Id=item.Id,FilePath=item.FilePath};
        _autoSaveSession=session;_autoSaveRevision=revision;
        try{
            _autoSaveWork=StaImageWorker.Run(()=>{try{DocumentStore.Save(savedItem,snapshot,image);}finally{EditorRenderer.ReleaseImageCaches(image,clearSharedRaster:false);}});
            await _autoSaveWork;
            if(session.MarkSaved(revision)&&ReferenceEquals(_session,session)){SavedLabel.Text="✓ Đã lưu";SavedLabel.Foreground=Brushes.SeaGreen;}
            _store.RequestThumbnail(item);
            return true;
        }catch(Exception ex){App.Log(ex);if(ReferenceEquals(_session,session)){SavedLabel.Text="Chưa lưu · Ctrl+S thử lại";SetStatus("Không lưu được chú thích: "+ex.Message);}return false;}
        finally{_autoSaveSession=null;_autoSaveWork=null;_autoSaveRevision=0;_autoSaveRunning=false;if(_session?.Dirty==true){_saveTimer.Stop();_saveTimer.Start();}}
    }

    private async Task<bool> SaveEditorAsync(Func<bool>? isCurrent = null)
    {
        var session = _session;
        try
        {
            while (isCurrent?.Invoke() != false && ReferenceEquals(session, _session))
            {
                if (session?.InTransaction == true && _inlineId == null && Surface.IsInteracting)
                    Surface.CancelInteraction();
                if (!PrepareEditorSave()) return false;
                if (session?.Dirty != true) return true;
                if (_autoSaveWork is {} pending)
                {
                    // Do not cancel writes. A later selection can abandon its wait,
                    // but only after the captured revision is safely persisted.
                    var pendingSession = _autoSaveSession;
                    var revision = _autoSaveRevision;
                    await pending;
                    if (ReferenceEquals(session, pendingSession)) session.MarkSaved(revision);
                    if (session.Dirty && ReferenceEquals(_autoSaveWork, pending))
                        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);
                    continue;
                }
                if (!await AutoSaveAsync()) return false;
            }
            return false;
        }
        catch (Exception error)
        {
            App.Log(error);
            if (ReferenceEquals(session, _session))
            {
                SavedLabel.Text = "Chưa lưu · Ctrl+S thử lại";
                SetStatus("Không lưu được chú thích: " + error.Message);
            }
            return false;
        }
    }
}
