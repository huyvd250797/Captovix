using System.Windows.Media.Imaging;

namespace Captovix;

public partial class MainWindow
{
    private bool _flatteningImage;

    private async Task FlattenSelectedImageAsync()
    {
        if (_flatteningImage || _busy || !RequireLicense() || _session is not {} session ||
            Surface.Original is not {} original || SelectedImage() is not {} selected) return;
        if (Surface.SelectedIds.Count != 1 || !ObjectControl.CanEdit(session.Document, [selected.Id]) ||
            selected.ImagePlaceholder || selected.GroupId != null)
        {
            SetStatus("Chọn một lớp ảnh riêng đang hiển thị và đã mở khóa để hợp nhất.");
            return;
        }
        if (!FlushEdits()) return;
        Surface.CancelInteraction();
        session.Commit();
        var revision = session.Revision;
        var snapshot = session.Document.Snapshot();
        _flatteningImage = true;
        try
        {
            string? merged = null;
            await StaImageWorker.Run(() => merged = EditorRenderer.FlattenImageLayer(original, snapshot, selected.Id));
            if (!ReferenceEquals(_session, session) || session.Revision != revision || session.InTransaction)
            {
                SetStatus("Ảnh đã thay đổi trong lúc hợp nhất. Chọn lại lớp ảnh rồi thử lại.");
                return;
            }
            session.Change(doc =>
            {
                doc.PixelBasePng = merged;
                doc.Annotations.RemoveAll(a => a.Id == selected.Id);
                ConnectorStudio.Resolve(doc);
                doc.Validate(doc.OriginalWidth, doc.OriginalHeight, doc.SourceHash);
            });
            Surface.SelectIds([]);
            Surface.Refresh();
            SyncProperties();
            SetStatus("Đã hợp nhất ảnh vào nền · Ctrl+Z để khôi phục lớp ảnh.");
        }
        catch (Exception error)
        {
            App.Log(error);
            SetStatus("Chưa hợp nhất ảnh. " + error.Message);
        }
        finally { _flatteningImage = false; }
    }
}
