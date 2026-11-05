using System.IO;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
namespace Captovix;
public partial class MainWindow
{
    bool _libraryTransferBusy;
    private async Task<DataObject?> LibraryTransferAsync(MediaItem[] items){
        if(_libraryTransferBusy||items.Length==0||!RequireLicense()||!FlushEdits())return null;
        if(items.Any(i=>i.IsVideo)){SetStatus("Chọn các ảnh để copy/kéo thả cùng nhau; video dùng Xuất video.");return null;}
        _libraryTransferBusy=true;MediaList.IsEnabled=false;string folder=Path.Combine(App.DataRoot,"Temp","LibraryExports",Guid.NewGuid().ToString("N"));var activeId=_activeItem?.Id;var active=_session?.Document.Snapshot();
        try{await Task.Run(()=>{var stale=StorageMaintenance.Inspect("Tệp chuyển ảnh tạm",Path.GetDirectoryName(folder)!,true);StorageMaintenance.Clean(App.DataRoot,new[]{stale},DateTime.UtcNow.AddDays(-1));});Directory.CreateDirectory(folder);var paths=new StringCollection();System.Windows.Media.Imaging.BitmapSource? single=null;int n=0;
            foreach(var item in items){SetStatus($"Đang chuẩn bị ảnh {++n}/{items.Length}…");string path=Path.Combine(folder,$"Captovix-{n:000}.png");await StaImageWorker.Run(()=>{var original=MediaStore.LoadImage(item.FilePath);try{EditorDocument document;if(item.Id==activeId&&active!=null)document=active;else{var loaded=DocumentStore.Load(item,original,out var warning);if(warning!=null)throw new IOException(warning);document=loaded.Document;}var result=EditorRenderer.Render(original,document);ImageTransfer.Save(result,path);if(items.Length==1)single=result;}finally{EditorRenderer.ReleaseImageCaches(original);}});paths.Add(path);}
            var data=new DataObject();data.SetFileDropList(paths);data.SetData("Preferred DropEffect",new MemoryStream(BitConverter.GetBytes((int)DragDropEffects.Copy)));if(single!=null)data.SetImage(single);return data;
        }catch(Exception e){try{Directory.Delete(folder,true);}catch{}App.Log(e);SetStatus("Chưa chuẩn bị được ảnh: "+e.Message);return null;}finally{_libraryTransferBusy=false;MediaList.IsEnabled=true;}
    }
    private async void CopyLibrarySelection(){var items=MediaList.SelectedItems.OfType<MediaItem>().ToArray();var data=await LibraryTransferAsync(items);if(data==null)return;try{Clipboard.SetDataObject(data,true);SetStatus($"Đã copy {items.Length} ảnh đã chỉnh · dán sang ứng dụng nhận tệp ảnh. Tệp tạm có thể dọn sau24giờ.");}catch(Exception e){SetStatus("Không copy được: "+e.Message);}}
}
