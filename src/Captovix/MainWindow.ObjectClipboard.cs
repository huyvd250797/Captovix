using System.IO;
using System.Text;
using System.Windows;
namespace Captovix;
public partial class MainWindow
{
    private string? _lastObjectClipboard,_lastPasteItem;
    private int _pasteCount;
    private static bool HasObjectClipboard(){try{return Clipboard.ContainsData(AnnotationClipboard.Format);}catch{return false;}}
    private void CopyObjects()
    {
        if(_session==null||Surface.SelectedIds.Count==0)return;
        try{
            EndInlineText(true);Surface.CancelInteraction();_session.Commit();var json=AnnotationClipboard.Copy(_session.Document,Surface.SelectedIds);var data=new DataObject();
            data.SetData(AnnotationClipboard.Format,new MemoryStream(Encoding.UTF8.GetBytes(json)),false);Clipboard.SetDataObject(data,true);
            _lastObjectClipboard=null;_pasteCount=0;SetStatus($"Đã copy {Surface.SelectedIds.Count} đối tượng · Ctrl+V để dán · Ctrl+Shift+C copy ảnh đã chỉnh.");
        }catch(Exception e){SetStatus("Chưa copy được đối tượng: "+e.Message);}
    }
    private void PasteObjects()
    {
        if(_session==null||Surface.Tool==EditorTool.Crop||_busy)return;
        try{
            var raw=Clipboard.GetDataObject()?.GetData(AnnotationClipboard.Format,false);string? json=raw switch{MemoryStream stream when stream.Length<=AnnotationClipboard.MaxChars*4L=>Encoding.UTF8.GetString(stream.ToArray()),byte[] bytes when bytes.Length<=AnnotationClipboard.MaxChars*4L=>Encoding.UTF8.GetString(bytes),string value when value.Length<=AnnotationClipboard.MaxChars=>value,_=>null};
            if(json==null){PastePixelImage();return;}
            EndInlineText(true);Surface.CancelInteraction();_session.Commit();if(_lastObjectClipboard!=json||_lastPasteItem!=_activeItem?.Id)_pasteCount=0;
            HashSet<string> added=[];int count=Math.Min(_pasteCount+1,1000);_session.Change(d=>{added=AnnotationClipboard.Paste(d,json,count*16,EditorRenderer.Box);RasterPixels.ValidateDecoded(d);});
            _lastObjectClipboard=json;_lastPasteItem=_activeItem?.Id;_pasteCount=count;if(Surface.IsPixelTool)SelectTool(EditorTool.Select);Surface.SelectIds(added);Surface.Focus();SyncProperties();SetStatus($"Đã dán {added.Count} đối tượng, vẫn chỉnh sửa được · Ctrl+Z hoàn tác.");
        }catch(Exception e){SetStatus("Chưa dán được đối tượng: "+e.Message);}
    }
}
