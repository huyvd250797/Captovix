using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace Captovix;
public partial class MainWindow
{
    private readonly ObservableCollection<MediaItem> _recentItems=[];
    private int _libraryRefreshHold;
    private MediaItem? _historyPin;
    private void RebuildRecent()
    {
        if(_store==null)return;var selected=Selected;var wanted=RecentSelection.Select(_store.Items,i=>i.Created,_historyPin??selected);
        bool changing=_changingSelection;_changingSelection=true;try{foreach(var old in _recentItems.Except(wanted).ToArray())_recentItems.Remove(old);for(int i=0;i<wanted.Count;i++){if(!_recentItems.Contains(wanted[i]))_recentItems.Insert(i,wanted[i]);else if(_recentItems.IndexOf(wanted[i])!=i)_recentItems.Move(_recentItems.IndexOf(wanted[i]),i);}if(CountLabel!=null)CountLabel.Text=$"{_recentItems.Count}/{_store.Items.Count}";}finally{_changingSelection=changing;}
    }
    private void OpenHistoryItem(MediaItem item){if(_busy||!FlushEdits())return;_historyPin=item;_libraryFilter=0;_collectionFilter=_tagFilter="";SearchBox.Clear();RebuildRecent();RefreshLibrary();MediaList.SelectedItems.Clear();MediaList.SelectedItem=item;MediaList.ScrollIntoView(item);}
    private void Archive_Click(object sender,RoutedEventArgs e)
    {
        if(_busy||!FlushEdits())return;var archive=new ArchiveWindow(_store,items=>{if(DeleteMediaBatch(items))RebuildRecent();},items=>OpenBatchExport(items)){Owner=this};archive.OpenItem=OpenHistoryItem;archive.ShowDialog();foreach(var item in _recentItems)_store.RequestThumbnail(item);
    }
}
