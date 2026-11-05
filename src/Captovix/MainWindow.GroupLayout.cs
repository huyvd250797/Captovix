using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace Captovix;
public partial class MainWindow
{
 private sealed record GroupTreeSelection(string[] Ids,string? Primary,string? Scope);
 private string _groupTreeKey="";
 private void GroupTree_Expanded(object sender,RoutedEventArgs e){if(_layersReady)SyncProperties();}
 private void SyncGroupTree(EditorDocument? doc)
 {
  if(Ancestor<Expander>(ObjectTree) is {IsExpanded:false})return;
  GroupScopeLabel.Text=Surface.GroupScope is {} scope?$"Trong nhóm {scope[..4]} · Alt+click chọn riêng":"Toàn cây · nhấp đúp vào nhóm để đi vào";
  string key=doc==null?"":string.Join("|",doc.Annotations.Select(a=>$"{a.Id}:{a.Name}:{a.GroupId}:{string.Join('/',a.GroupPath??[])}:{a.Locked}:{a.Visible}"));
  if(key!=_groupTreeKey||doc==null){_groupTreeKey=key;var open=new HashSet<string>();void Collect(ItemsControl p){foreach(var node in p.Items.OfType<TreeViewItem>()){if(node.IsExpanded&&node.Tag is string id)open.Add(id);Collect(node);}}Collect(ObjectTree);ObjectTree.Items.Clear();
   if(doc!=null){var roots=new Dictionary<string,TreeViewItem>();foreach(var a in doc.Annotations.AsEnumerable().Reverse()){ItemsControl parent=ObjectTree;var chain=GroupLayout.Chain(a);for(int i=0;i<chain.Length;i++){var g=chain[i];if(!roots.TryGetValue(g,out var node)){var members=doc.Annotations.Where(x=>GroupLayout.Chain(x).Contains(g)).ToArray();node=new TreeViewItem{Tag=g,IsExpanded=open.Contains(g)||_groupTreeKey.Length<1};node.Header=TreeHeader($"Nhóm · {members.Length}",members.Select(x=>x.Id).ToArray(),a.Id,i>0?chain[i-1]:null,null);roots[g]=node;parent.Items.Add(node);}parent=node;}
    var label=string.IsNullOrWhiteSpace(a.Name)?a.Mockup!=null?$"{a.Mockup.Type} · {a.Text}":ShapeStudio.Label(a.Kind):a.Name;
    var leaf=new TreeViewItem{Tag="item:"+a.Id,Header=TreeHeader(label,[a.Id],a.Id,a.GroupId,a)};parent.Items.Add(leaf);
   }}
  }
  void Mark(ItemsControl p){foreach(var n in p.Items.OfType<TreeViewItem>()){if(n.Header is FrameworkElement h&&h.Tag is GroupTreeSelection choice){h.Opacity=choice.Ids.Any(Surface.SelectedIds.Contains)?1:.75;if(h is DockPanel panel&&panel.Children.OfType<TextBlock>().FirstOrDefault() is {} t)t.FontWeight=choice.Ids.All(Surface.SelectedIds.Contains)?FontWeights.SemiBold:FontWeights.Normal;}Mark(n);}}Mark(ObjectTree);
 }
 private FrameworkElement TreeHeader(string label,string[] ids,string? primary,string? scope,Annotation? a)
 {
  var row=new DockPanel{LastChildFill=true,Tag=new GroupTreeSelection(ids,primary,scope),ToolTip=label,Margin=new(0,2,0,2)};
  if(a!=null){foreach(var command in new[]{"Lock","Visible"}){var b=new Button{Content=AppIcons.Content(command=="Lock"?(a.Locked?"Lock":"Unlock"):(a.Visible?"Eye":"EyeOff"),"",11),Tag=command,CommandParameter=a.Id,Padding=new(2),Margin=new(1),Width=23,Height=24,ToolTip=command=="Lock"?"Khóa / mở khóa":"Ẩn / hiện"};DockPanel.SetDock(b,Dock.Right);b.Click+=(_,e)=>{Surface.SelectInGroup([a.Id],a.Id,a.GroupId);LayerToggle_Click(b,e);};row.Children.Add(b);}}
  var text=new TextBlock{Text=label.Length>70?label[..70]+"…":label,MaxWidth=160,TextTrimming=TextTrimming.CharacterEllipsis,VerticalAlignment=VerticalAlignment.Center};text.SetResourceReference(TextBlock.ForegroundProperty,"Text");row.Children.Add(text);return row;
 }
 private void ObjectTree_Changed(object sender,RoutedPropertyChangedEventArgs<object> e)
 {
  if(_syncLayers||_session==null||e.NewValue is not TreeViewItem {Header:FrameworkElement {Tag:GroupTreeSelection choice}})return;
  EndInlineText(true);SelectTool(EditorTool.Select);IEnumerable<string> ids=choice.Ids;if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control)&&Surface.GroupScope==choice.Scope)ids=Surface.SelectedIds.Union(ids).ToArray();Surface.SelectInGroup(ids,choice.Primary,choice.Scope);SyncProperties();Surface.Focus();
 }
 private void SelectGroupParent_Click(object sender,RoutedEventArgs e){EndInlineText(true);Surface.SelectParentGroup();SyncProperties();}
 private void EnterGroup_Click(object sender,RoutedEventArgs e){EndInlineText(true);Surface.EnterSelectedGroup();SyncProperties();}
 private void RootGroup_Click(object sender,RoutedEventArgs e){EndInlineText(true);Surface.SelectInGroup(Surface.SelectedIds.ToArray(),Surface.SelectedId,null);SyncProperties();}
 private void GroupAlign_Click(object sender,RoutedEventArgs e){if(sender is Button {Tag:string command})AlignObjects(command);}
 private void GroupShared_Click(object sender,RoutedEventArgs e)
 {
  if(!RequireLicense()||_session==null||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds)||!FlushEdits())return;
  var session=_session;var ids=Surface.SelectedIds.ToArray();var w=new Window{Owner=this,Title="Thuộc tính chung",Width=420,SizeToContent=SizeToContent.Height,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner};var panel=new StackPanel{Margin=new(18)};w.Content=panel;
  panel.Children.Add(new TextBlock{Text=$"Áp dụng cho {ids.Length} phần tử · để trống để giữ nguyên",TextWrapping=TextWrapping.Wrap,Margin=new(0,0,0,12)});
  TextBox Field(string title){panel.Children.Add(new TextBlock{Text=title});var field=new TextBox{Margin=new(0,4,0,10)};panel.Children.Add(field);return field;}
  var ink=Field("Màu chữ · #RRGGBB");var fill=Field("Màu nền / tô · #RRGGBB");var font=Field("Cỡ chữ · 8–240 px");var error=new TextBlock{TextWrapping=TextWrapping.Wrap};error.SetResourceReference(TextBlock.ForegroundProperty,"Danger");panel.Children.Add(error);var actions=new DockPanel{LastChildFill=false,Margin=new(0,12,0,0)};panel.Children.Add(actions);var close=new Button{Content="Đóng",IsCancel=true};close.Click+=(_,_)=>w.Close();actions.Children.Add(close);var apply=new Button{Content="Áp dụng",IsDefault=true};apply.SetResourceReference(StyleProperty,"Primary");DockPanel.SetDock(apply,Dock.Right);actions.Children.Add(apply);apply.Click+=(_,_)=>{try{if(!ReferenceEquals(_session,session))throw new InvalidOperationException("Ảnh đang mở đã thay đổi.");double? size=string.IsNullOrWhiteSpace(font.Text)?null:double.Parse(font.Text,System.Globalization.CultureInfo.InvariantCulture);EndInlineText(true);session.Change(d=>GroupLayout.ApplyShared(d,ids,ink.Text.Trim(),fill.Text.Trim(),size));Surface.Refresh();SyncProperties();w.Close();}catch(Exception ex){error.Text=ex.Message;}};w.ShowDialog();
 }
 private void MediaRight_Click(object sender,MouseButtonEventArgs e)
 {
  if(Ancestor<ListBoxItem>(e.OriginalSource as DependencyObject)?.DataContext is not MediaItem item)return;
  if(!MediaList.SelectedItems.Contains(item))MediaList.SelectedItem=item;
  var menu=new ContextMenu();var copy=new MenuItem{Header="Copy ảnh đã chọn"};copy.Click+=(_,_)=>CopyLibrarySelection();menu.Items.Add(copy);var location=new MenuItem{Header="Mở vị trí tệp",Icon=AppIcons.Content("FolderLocation","",16)};location.Click+=(_,_)=>{try{if(!System.IO.File.Exists(item.FilePath))throw new System.IO.FileNotFoundException("Tệp không còn ở vị trí đã lưu.");var info=new System.Diagnostics.ProcessStartInfo("explorer.exe"){UseShellExecute=true};info.ArgumentList.Add("/select,");info.ArgumentList.Add(System.IO.Path.GetFullPath(item.FilePath));System.Diagnostics.Process.Start(info);}catch(Exception ex){AppDialog.Show(this,ex.Message,"Mở vị trí tệp");}};menu.Items.Add(location);AppMenus.Decorate(menu);menu.PlacementTarget=MediaList;menu.IsOpen=true;e.Handled=true;
 }
}
