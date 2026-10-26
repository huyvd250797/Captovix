using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
namespace Captovix;
internal sealed class ArchiveWindow:Window
{
    private readonly MediaStore _store;
    private readonly TreeView _tree=new(){BorderThickness=new(0)};
    private readonly List<ListBox> _lists=[];
    private readonly HashSet<string> _expanded=[];
    private readonly HashSet<string> _selected=[];
    private readonly TextBlock _count=new(){VerticalAlignment=VerticalAlignment.Center,Margin=new(8)};
    private HistoryCalendar<MediaItem> _calendar=null!;
    private bool _sync;
    private readonly CheckBox _hideEmpty=new(){Content="Ẩn ngày không có hình",Margin=new(4,0,0,12)};
    private string ViewPreference=>System.IO.Path.Combine(App.DataRoot,"archive-view.json");
    public Action<MediaItem>? OpenItem {get;set;}
    public ArchiveWindow(MediaStore store,Action<MediaItem[]> delete,Action<MediaItem[]> export)
    {
        _store=store;SetResourceReference(StyleProperty,typeof(Window));Title="Captovix · Thư viện theo năm / tháng / ngày";Width=Math.Min(1080,SystemParameters.WorkArea.Width-32);Height=Math.Min(800,SystemParameters.WorkArea.Height-32);WindowStartupLocation=WindowStartupLocation.CenterOwner;
        var root=new DockPanel{Margin=new(14)};Content=root;var header=new TextBlock{Text="Mở năm → tháng → ngày để xem ảnh",FontSize=18,FontWeight=FontWeights.SemiBold,Margin=new(4,0,4,12)};DockPanel.SetDock(header,Dock.Top);root.Children.Add(header);
        header.Text="Thư viện ảnh";header.FontSize=24;
        var hint=new TextBlock{Text="Lịch sử theo năm, tháng và ngày · Ctrl / Shift để chọn nhiều",Margin=new(4,0,4,12)};hint.SetResourceReference(TextBlock.ForegroundProperty,"Muted");DockPanel.SetDock(hint,Dock.Top);root.Children.Add(hint);
        try{_hideEmpty.IsChecked=System.IO.File.Exists(ViewPreference)?System.Text.Json.JsonSerializer.Deserialize<bool>(System.IO.File.ReadAllText(ViewPreference)):true;}catch{_hideEmpty.IsChecked=true;}
        DockPanel.SetDock(_hideEmpty,Dock.Top);root.Children.Add(_hideEmpty);_hideEmpty.Click+=(_,_)=>{try{MediaStore.AtomicWrite(ViewPreference,System.Text.Json.JsonSerializer.Serialize(_hideEmpty.IsChecked==true));}catch(Exception ex){App.Log(ex);}Refresh();};
        var actions=new StackPanel{Orientation=Orientation.Horizontal};DockPanel.SetDock(actions,Dock.Bottom);root.Children.Add(actions);
        MediaItem[] Selection()=>_store.Items.Where(i=>_selected.Contains(i.Id)).ToArray();void Button(string icon,string text,Action a){var b=new Button{Content=AppIcons.Content(icon,text)};b.Click+=(_,_)=>a();actions.Children.Add(b);}
        Button("Folder","Mở",()=>Open(Selection().FirstOrDefault()));Button("Export","Xuất",()=>export(Selection()));Button("Delete","Xóa",()=>{delete(Selection());Refresh();});actions.Children.Add(_count);root.Children.Add(_tree);
        _tree.PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Delete){delete(Selection());Refresh();e.Handled=true;}if(e.Key==Key.Enter&&_lists.Any(l=>l.IsKeyboardFocusWithin)&&Selection().Length==1){Open(Selection()[0]);e.Handled=true;}};Refresh();
    }
    private void Open(MediaItem? item){if(item==null)return;Close();OpenItem?.Invoke(item);}
    private TreeViewItem Branch(string text,string key,Action<TreeViewItem> load)
    {
        var parts=text.Split('·',2);var row=new StackPanel{Orientation=Orientation.Horizontal,Margin=new(1,1,4,1)};
        row.Children.Add(AppIcons.Content(key.Count(c=>c=='/')==2?"Camera":"Folder",parts[0].Trim(),14));
        var badge=new TextBlock{Text=parts.Length>1?parts[1].Trim():"",FontSize=11,Margin=new(12,2,0,0)};badge.SetResourceReference(TextBlock.ForegroundProperty,"Muted");row.Children.Add(badge);
        var node=new TreeViewItem{Header=row,Padding=new(5,7,5,7),HorizontalContentAlignment=HorizontalAlignment.Stretch};node.Items.Add(new TextBlock{Text="…"});bool loaded=false;
        node.Expanded+=(_,e)=>{if(e.OriginalSource!=node)return;_expanded.Add(key);if(!loaded){loaded=true;node.Items.Clear();load(node);}e.Handled=true;};node.Collapsed+=(_,e)=>{if(e.OriginalSource==node)_expanded.Remove(key);};if(_expanded.Contains(key))node.IsExpanded=true;return node;
    }
    private void Refresh()
    {
        _selected.RemoveWhere(id=>!_store.Items.Any(i=>i.Id==id));_calendar=new(_store.Items.OrderByDescending(i=>i.Created),i=>i.Created,DateTime.Now.Year);_lists.Clear();_tree.Items.Clear();
        foreach(int year in _calendar.Years)_tree.Items.Add(Branch($"{year} · {_calendar.Count(year)} nội dung",year.ToString(),y=>{
            foreach(int month in HistoryCalendar<MediaItem>.Months())y.Items.Add(Branch($"Tháng {month:00} · {_calendar.Count(year,month)}",$"{year}/{month}",m=>{
                foreach(int day in HistoryCalendar<MediaItem>.Days(year,month)){var items=_calendar.Items(year,month,day);if(_hideEmpty.IsChecked==true&&items.Count==0)continue;m.Items.Add(Branch($"Ngày {day:00} · {items.Count}",$"{year}/{month}/{day}",d=>PopulateDay(d,items)));}
            }));
        }));UpdateCount();
    }
    private void PopulateDay(TreeViewItem day,IReadOnlyList<MediaItem> items)
    {
        if(items.Count==0){day.Items.Add(new TextBlock{Text="Chưa có ảnh / video",Margin=new(10,4,10,6)});return;}
        var visible=new ObservableCollection<MediaItem>(items.Take(100));var list=new ListBox{ItemsSource=visible,SelectionMode=SelectionMode.Extended,BorderThickness=new(0),MinWidth=260,MaxHeight=420,HorizontalContentAlignment=HorizontalAlignment.Stretch};ScrollViewer.SetCanContentScroll(list,true);VirtualizingPanel.SetIsVirtualizing(list,true);VirtualizingPanel.SetVirtualizationMode(list,VirtualizationMode.Recycling);_lists.Add(list);day.Items.Add(list);
        var template=new DataTemplate();var row=new FrameworkElementFactory(typeof(DockPanel));row.SetValue(FrameworkElement.MarginProperty,new Thickness(4,5,4,5));var image=new FrameworkElementFactory(typeof(Image));image.SetValue(FrameworkElement.WidthProperty,84d);image.SetValue(FrameworkElement.HeightProperty,52d);image.SetValue(DockPanel.DockProperty,Dock.Left);image.SetValue(FrameworkElement.MarginProperty,new Thickness(0,0,12,0));image.SetBinding(Image.SourceProperty,new Binding(nameof(MediaItem.Thumbnail)));image.AddHandler(FrameworkElement.LoadedEvent,new RoutedEventHandler((sender,_)=>{if(sender is Image{DataContext:MediaItem item})_store.RequestThumbnail(item);}));row.AppendChild(image);var text=new FrameworkElementFactory(typeof(StackPanel));foreach(string property in new[]{nameof(MediaItem.Title),nameof(MediaItem.Detail)}){var line=new FrameworkElementFactory(typeof(TextBlock));line.SetBinding(TextBlock.TextProperty,new Binding(property));line.SetValue(TextBlock.TextTrimmingProperty,TextTrimming.CharacterEllipsis);text.AppendChild(line);}row.AppendChild(text);template.VisualTree=row;list.ItemTemplate=template;
        list.SelectionChanged+=(_,_)=>{if(_sync)return;_sync=true;try{if(!Keyboard.Modifiers.HasFlag(ModifierKeys.Control)&&!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)&&list.SelectedItems.Count>0)foreach(var other in _lists.Where(l=>l!=list))other.SelectedItems.Clear();_selected.Clear();foreach(var selected in _lists.SelectMany(l=>l.SelectedItems.Cast<MediaItem>()))_selected.Add(selected.Id);UpdateCount();}finally{_sync=false;}};
        _sync=true;foreach(var i in visible.Where(i=>_selected.Contains(i.Id)))list.SelectedItems.Add(i);_sync=false;
        list.MouseDoubleClick+=(_,e)=>{if(ItemsControl.ContainerFromElement(list,e.OriginalSource as DependencyObject) is ListBoxItem{DataContext:MediaItem i})Open(i);};
        if(items.Count>100){var more=new Button{Content=$"Xem thêm · {items.Count-100} còn lại"};day.Items.Add(more);more.Click+=(_,_)=>{int count=visible.Count;foreach(var item in items.Skip(count).Take(100))visible.Add(item);more.Content=$"Xem thêm · {items.Count-visible.Count} còn lại";more.Visibility=visible.Count==items.Count?Visibility.Collapsed:Visibility.Visible;};}
    }
    private void UpdateCount()=>_count.Text=$"{_store.Items.Count} nội dung · {_selected.Count} đã chọn";
}
