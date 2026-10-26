using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
namespace Captovix;
public partial class MainWindow
{
    private Window DialogOwner=>Application.Current.Windows.Cast<Window>().LastOrDefault(w=>w.IsActive)??this;
    private bool DeleteMediaBatch(IEnumerable<MediaItem> input)
    {
        var items=input.Distinct().Where(_store.Items.Contains).ToArray();if(items.Length==0||_busy||_updateApplying)return false;
        var owner=DialogOwner;var w=new Window{Owner=owner,Title="Xóa nội dung",Width=450,SizeToContent=SizeToContent.Height,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner};var panel=new StackPanel{Margin=new(22)};w.Content=panel;
        panel.Children.Add(AppIcons.Content("Delete","Chuyển vào thùng rác"));panel.Children.Add(new TextBlock{Text=$"Xóa {items.Length} nội dung đã chọn?",FontSize=20,FontWeight=FontWeights.SemiBold,Margin=new(0,16,0,8)});panel.Children.Add(new TextBlock{Text=string.Join("\n",items.Take(3).Select(i=>i.Title))+(items.Length>3?$"\n… và {items.Length-3} nội dung khác":""),TextWrapping=TextWrapping.Wrap,MaxHeight=130});panel.Children.Add(new TextBlock{Text="Ảnh/video và bản chỉnh sẽ chuyển vào thùng rác Captovix. Có thể khôi phục cả lần xóa này bằng nút Khôi phục.",TextWrapping=TextWrapping.Wrap,Margin=new(0,14,0,18)});
        var row=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};panel.Children.Add(row);var cancel=new Button{Content="Hủy",IsCancel=true};row.Children.Add(cancel);cancel.Click+=(_,_)=>w.DialogResult=false;var yes=new Button{Content=$"Xóa {items.Length} nội dung",Style=(Style)FindResource("Primary")};yes.SetResourceReference(Control.BackgroundProperty,"Danger");yes.Foreground=System.Windows.Media.Brushes.White;yes.Click+=(_,_)=>w.DialogResult=true;row.Children.Add(yes);w.Loaded+=(_,_)=>cancel.Focus();if(w.ShowDialog()!=true)return false;
        if(!FlushEdits())return false;var positions=items.ToDictionary(i=>i,i=>_store.Items.IndexOf(i));TrashReceipt? receipt=null;bool active=items.Contains(_activeItem);_libraryRefreshHold++;bool oldChanging=_changingSelection;_changingSelection=true;
        try{
            var paths=new List<string>();foreach(var i in items){if(!_store.Items.Except(items).Any(other=>other.FilePath.Equals(i.FilePath,StringComparison.OrdinalIgnoreCase)))paths.Add(i.FilePath);string edit=DocumentStore.EditPath(i.Id);paths.Add(edit);paths.Add(DocumentStore.ThumbnailPath(i.Id));if(Directory.Exists(Path.GetDirectoryName(edit)))paths.AddRange(Directory.EnumerateFiles(Path.GetDirectoryName(edit)!,Path.GetFileName(edit)+".*.bak"));}
            receipt=MediaTrash.Move(App.DataRoot,JsonSerializer.Serialize(items),paths);foreach(var i in items)_store.Items.Remove(i);_store.Save();if(active){VideoPreview.Stop();VideoPreview.Source=null;UnloadEditor();}_lastDeleted=receipt;RestoreMediaButton.IsEnabled=true;SetStatus($"Đã chuyển {items.Length} nội dung vào thùng rác.");return true;
        }catch(Exception ex){if(receipt!=null)try{MediaTrash.Restore(receipt);foreach(var i in items.OrderBy(i=>positions[i]))if(!_store.Items.Contains(i))_store.Items.Insert(Math.Clamp(positions[i],0,_store.Items.Count),i);_store.Save();}catch(Exception rollback){App.Log(rollback);}App.Log(ex);MessageBox.Show(owner,ex.Message,"Chưa xóa được nội dung");return false;}
        finally{_libraryRefreshHold--;_changingSelection=oldChanging;RebuildRecent();RefreshLibrary();if(active)MediaList.SelectedItem=_recentItems.FirstOrDefault();}
    }
    private void RestoreMedia_Click(object sender,RoutedEventArgs e)
    {
        if(_lastDeleted==null||_busy||_updateApplying)return;var receipt=_lastDeleted;MediaItem[] items=[];bool restored=false;_libraryRefreshHold++;
        try{using var json=JsonDocument.Parse(receipt.Metadata);items=json.RootElement.ValueKind==JsonValueKind.Array?JsonSerializer.Deserialize<MediaItem[]>(receipt.Metadata)??[]:[JsonSerializer.Deserialize<MediaItem>(receipt.Metadata)!];if(items.Any(i=>i==null||_store.Items.Any(old=>old.Id==i.Id)))throw new IOException("Nội dung đã có trong thư viện.");MediaTrash.Restore(receipt);restored=true;foreach(var i in items){_store.TryThumbnail(i);_store.Items.Add(i);}_store.Save();_lastDeleted=null;RestoreMediaButton.IsEnabled=false;SetStatus($"Đã khôi phục {items.Length} nội dung và bản chỉnh.");}
        catch(Exception ex){if(restored)try{foreach(var i in items)_store.Items.Remove(i);MediaTrash.UndoRestore(receipt);}catch(Exception rollback){App.Log(rollback);}App.Log(ex);MessageBox.Show(this,ex.Message,"Chưa khôi phục được nội dung");}
        finally{_libraryRefreshHold--;RebuildRecent();RefreshLibrary();}
    }
}
