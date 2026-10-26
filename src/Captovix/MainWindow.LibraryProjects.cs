using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
namespace Captovix;
public partial class MainWindow
{
    private string _collectionFilter="",_tagFilter="";
    private int _libraryFilter;
    private void InitializeLibraryProjects(){var v=CollectionViewSource.GetDefaultView(_recentItems);v.SortDescriptions.Add(new(nameof(MediaItem.Favorite),ListSortDirection.Descending));v.SortDescriptions.Add(new(nameof(MediaItem.Created),ListSortDirection.Descending));}
    private bool LibraryMatch(MediaItem i)
    {
        var q=SearchBox.Text??"";bool edited=File.Exists(DocumentStore.EditPath(i.Id));
        return (_libraryFilter!=1||i.Favorite)&&(_libraryFilter!=2||!i.IsVideo)&&(_libraryFilter!=3||i.IsVideo)&&(_libraryFilter!=4||edited)&&(_libraryFilter!=5||!edited)&&
            (string.IsNullOrEmpty(_collectionFilter)||i.Collection.Equals(_collectionFilter,StringComparison.CurrentCultureIgnoreCase))&&
            (string.IsNullOrEmpty(_tagFilter)||(i.Tags??[]).Contains(_tagFilter,StringComparer.CurrentCultureIgnoreCase))&&
            $"{i.Title} {i.Collection} {string.Join(' ',i.Tags??[])} {(i.IsVideo?"video":"ảnh")} {i.Created:dd/MM/yyyy}".Contains(q,StringComparison.CurrentCultureIgnoreCase);
    }
    private void ViewOriginal_Click(object sender,RoutedEventArgs e){if(_busy||Selected is not {IsVideo:false} i)return;try{var image=new System.Windows.Controls.Image{Source=MediaStore.LoadImage(i.FilePath),Stretch=System.Windows.Media.Stretch.Uniform};var w=new Window{Owner=this,Title="Ảnh gốc · "+i.Title,Width=900,Height=650,Content=new ScrollViewer{Content=image,HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,VerticalScrollBarVisibility=ScrollBarVisibility.Auto},WindowStartupLocation=WindowStartupLocation.CenterOwner};w.ShowDialog();}catch(Exception ex){SetStatus(ex.Message);}}
    private void RefreshLibrary()=>CollectionViewSource.GetDefaultView(_recentItems).Refresh();
    private void FavoriteMedia_Click(object sender,RoutedEventArgs e){if(_busy||_updateApplying)return;if((sender as Button)?.Tag is MediaItem i){bool old=i.Favorite;try{i.Favorite=!old;_store.Save();i.RefreshLabels();RefreshLibrary();}catch(Exception ex){i.Favorite=old;SetStatus(ex.Message);}e.Handled=true;}}
    private void LibraryManager_Click(object sender,RoutedEventArgs e)
    {
        if(_busy||_updateApplying)return;
        var panel=new StackPanel{Margin=new(18)};var dialog=new Window{Owner=this,Title="Captovix · Thư viện & dự án",Width=480,SizeToContent=SizeToContent.Height,WindowStartupLocation=WindowStartupLocation.CenterOwner,MaxHeight=SystemParameters.WorkArea.Height-32};dialog.Content=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        panel.Children.Add(new TextBlock{Text="Lọc thư viện",FontSize=20,FontWeight=FontWeights.SemiBold});
        var filter=new ComboBox{ItemsSource=new[]{"Tất cả","Yêu thích","Ảnh","Video","Có bản chỉnh","Chưa có bản chỉnh"},SelectedIndex=_libraryFilter};panel.Children.Add(filter);
        var collections=new ComboBox{IsEditable=true,ItemsSource=_store.Items.Select(i=>i.Collection).Where(c=>!string.IsNullOrEmpty(c)).Distinct().Order().ToArray(),Text=_collectionFilter};panel.Children.Add(new TextBlock{Text="Bộ sưu tập · để trống = tất cả"});panel.Children.Add(collections);
        var tagFilter=new TextBox{Text=_tagFilter};panel.Children.Add(new TextBlock{Text="Tag cần lọc · để trống = tất cả"});panel.Children.Add(tagFilter);
        var apply=new Button{Content="Lọc"};panel.Children.Add(apply);apply.Click+=(_,_)=>{_libraryFilter=filter.SelectedIndex;_collectionFilter=collections.Text.Trim();_tagFilter=tagFilter.Text.Trim();RefreshLibrary();dialog.Close();};
        var reset=new Button{Content="Bỏ toàn bộ bộ lọc"};panel.Children.Add(reset);reset.Click+=(_,_)=>{_libraryFilter=0;_collectionFilter=_tagFilter="";SearchBox.Clear();RefreshLibrary();dialog.Close();};
        if(Selected is {} i){panel.Children.Add(new Separator());panel.Children.Add(new TextBlock{Text="Nội dung đang chọn",FontWeight=FontWeights.SemiBold});var title=new TextBox{Text=i.Title,MaxLength=200};panel.Children.Add(title);panel.Children.Add(new TextBlock{Text="Tag · cách nhau bằng dấu phẩy"});var tags=new TextBox{Text=string.Join(", ",i.Tags??[]),MaxLength=2000};panel.Children.Add(tags);panel.Children.Add(new TextBlock{Text="Bộ sưu tập · nhóm logic, không di chuyển file"});var collection=new TextBox{Text=i.Collection,MaxLength=80};panel.Children.Add(collection);var star=new CheckBox{Content="Yêu thích",IsChecked=i.Favorite};panel.Children.Add(star);var save=new Button{Content="Lưu thông tin"};panel.Children.Add(save);save.Click+=(_,_)=>{var old=(i.Title,i.Tags??[],i.Collection,i.Favorite);try{var parsed=LibraryArchive.Tags(tags.Text);i.Title=title.Text.Trim();i.Tags=parsed;i.Collection=collection.Text.Trim();i.Favorite=star.IsChecked==true;_store.Save();i.RefreshLabels();RefreshLibrary();dialog.Close();SetStatus("Đã lưu thông tin thư viện.");}catch(Exception ex){(i.Title,i.Tags,i.Collection,i.Favorite)=old;MessageBox.Show(dialog,ex.Message,"Chưa lưu được");}};}
        panel.Children.Add(new Separator());var backup=new Button{Content="Sao lưu thư viện kèm ảnh…"};panel.Children.Add(backup);backup.Click+=(_,_)=>{dialog.Close();ExportLibrary_Click(this,new());};var import=new Button{Content="Nhập bản sao thư viện…"};panel.Children.Add(import);import.Click+=(_,_)=>{dialog.Close();ImportLibrary_Click(this,new());};dialog.ShowDialog();
    }
    private async void ExportLibrary_Click(object sender,RoutedEventArgs e)
    {
        if(_busy||_updateApplying||!FlushEdits())return;var picker=new SaveFileDialog{Filter="Thư viện Captovix|*.cvlibrary",FileName="Captovix-library.cvlibrary"};if(picker.ShowDialog(this)!=true)return;SetBusy(true);WorkspaceGrid.IsEnabled=false;_captureCancellation=new();SetStatus("Đang sao lưu thư viện…");
        try{var assets=_store.Items.Select(i=>{var clone=JsonSerializer.Deserialize<MediaItem>(JsonSerializer.Serialize(i))!;clone.FilePath="";return new LibraryAsset(Guid.NewGuid().ToString("N"),Path.GetExtension(i.FilePath).ToLowerInvariant(),JsonSerializer.Serialize(clone),i.FilePath,File.Exists(DocumentStore.EditPath(i.Id))?DocumentStore.EditPath(i.Id):null);}).ToArray();await Task.Run(()=>LibraryArchive.Save(picker.FileName,assets,_captureCancellation.Token));SetStatus("Đã sao lưu toàn bộ thư viện kèm ảnh gốc, bản chỉnh, tag và bộ sưu tập. Bản sao chứa cả dữ liệu gốc trước khi che.");}
        catch(Exception ex){App.Log(ex);MessageBox.Show(this,ex.Message,"Chưa sao lưu được");}finally{_captureCancellation?.Dispose();_captureCancellation=null;WorkspaceGrid.IsEnabled=true;SetBusy(false);if(_exitRequested)_=Dispatcher.BeginInvoke(RequestExit);}
    }
    private async void ImportLibrary_Click(object sender,RoutedEventArgs e)
    {
        if(_busy||_updateApplying||!FlushEdits())return;var picker=new OpenFileDialog{Filter="Thư viện Captovix|*.cvlibrary"};if(picker.ShowDialog(this)!=true)return;SetBusy(true);WorkspaceGrid.IsEnabled=false;_captureCancellation=new();SetStatus("Đang kiểm tra và nhập thư viện…");string stage=Path.Combine(App.DataRoot,"Import-"+Guid.NewGuid().ToString("N"));var written=new List<string>();var added=new List<MediaItem>();
        try{var assets=await Task.Run(()=>LibraryArchive.Read(picker.FileName,stage,_captureCancellation.Token));var items=new List<(LibraryAsset Asset,MediaItem Item)>();
            foreach(var a in assets){_captureCancellation.Token.ThrowIfCancellationRequested();var i=JsonSerializer.Deserialize<MediaItem>(a.Metadata)??throw new InvalidDataException("Metadata trống.");if(i.Title==null||i.Title.Length>200||i.Collection==null||i.Collection.Length>80||i.Tags==null||i.Tags.Length>32||i.Tags.Any(t=>t==null||t.Length>60)||i.IsVideo!=a.Extension.Equals(".mp4",StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Metadata không hợp lệ.");i.Id=Guid.NewGuid().ToString("N");i.FilePath=Path.Combine(_store.Root,"import-"+i.Id+a.Extension);if(!i.IsVideo){var bitmap=MediaStore.LoadImage(a.Source);i.Width=bitmap.PixelWidth;i.Height=bitmap.PixelHeight;if(a.Edit!=null){var doc=EditorDocument.Deserialize(File.ReadAllText(a.Edit));using var input=File.OpenRead(a.Source);doc.Validate(i.Width,i.Height,Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(input)));RasterPixels.ValidateDecoded(doc);}}else if(a.Edit!=null)throw new InvalidDataException("Video không có sidecar ảnh.");items.Add((a,i));}
            Directory.CreateDirectory(_store.Root);foreach(var (a,i) in items){_captureCancellation.Token.ThrowIfCancellationRequested();written.Add(i.FilePath);await Task.Run(()=>File.Copy(a.Source,i.FilePath,false));if(a.Edit!=null){var edit=DocumentStore.EditPath(i.Id);Directory.CreateDirectory(Path.GetDirectoryName(edit)!);File.Copy(a.Edit,edit,false);written.Add(edit);} if(a.Edit!=null&&!i.IsVideo){var bitmap=MediaStore.LoadImage(i.FilePath);var session=new EditorSession(EditorDocument.Deserialize(File.ReadAllText(a.Edit)));written.Add(DocumentStore.ThumbnailPath(i.Id));DocumentStore.Save(i,session,bitmap);}else _store.TryThumbnail(i);_store.Items.Add(i);added.Add(i);await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);}_store.Save();written.Clear();added.Clear();SetStatus($"Đã nhập {items.Count} nội dung với mã mới; giữ nguyên thư viện hiện có.");RefreshLibrary();
        }catch(Exception ex){foreach(var i in added)_store.Items.Remove(i);foreach(var p in written)try{File.Delete(p);}catch{}App.Log(ex);MessageBox.Show(this,ex.Message,"Chưa nhập được thư viện");}
        finally{if(Directory.Exists(stage))try{Directory.Delete(stage,true);}catch{}_captureCancellation?.Dispose();_captureCancellation=null;WorkspaceGrid.IsEnabled=true;SetBusy(false);if(_exitRequested)_=Dispatcher.BeginInvoke(RequestExit);}
    }
}
