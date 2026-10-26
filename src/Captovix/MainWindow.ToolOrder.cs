using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace Captovix;
public partial class MainWindow
{
    private Button[] _defaultTools=[];
    private Point _toolDragPoint;
    private Button? _toolDragButton;
    private static string ToolId(Button b)=>b.Tag?.ToString()??b.Name;
    private static T? Ancestor<T>(DependencyObject? node) where T:DependencyObject{while(node!=null){if(node is T t)return t;node=node is Visual||node is System.Windows.Media.Media3D.Visual3D?VisualTreeHelper.GetParent(node):LogicalTreeHelper.GetParent(node);}return null;}
    private void InitializeToolOrder()
    {
        _defaultTools=ToolBox.Children.OfType<Button>().ToArray();try{string path=Path.Combine(App.DataRoot,"tool-order.json");if(File.Exists(path)){var ids=JsonSerializer.Deserialize<string[]>(File.ReadAllText(path))??[];if(ids.Length>64||ids.Distinct().Count()!=ids.Length)throw new InvalidDataException("Bố cục công cụ lỗi.");ReorderTools(ids);}}catch(Exception ex){App.Log(ex);SetStatus("Không đọc được bố cục công cụ, đang dùng thứ tự mặc định.");}
        ToolBox.PreviewMouseLeftButtonDown+=(_,e)=>{_toolDragButton=Ancestor<Button>(e.OriginalSource as DependencyObject);_toolDragPoint=e.GetPosition(ToolBox);};
        ToolBox.PreviewMouseMove+=(_,e)=>{if(_toolDragButton==null||e.LeftButton!=MouseButtonState.Pressed||(e.GetPosition(ToolBox)-_toolDragPoint).Length<8)return;var b=_toolDragButton;_toolDragButton=null;DragDrop.DoDragDrop(b,new DataObject("Captovix.Tool",ToolId(b)),DragDropEffects.Move);e.Handled=true;};
        foreach(var b in _defaultTools){b.AllowDrop=true;b.DragOver+=(_,e)=>{e.Effects=e.Data.GetDataPresent("Captovix.Tool")?DragDropEffects.Move:DragDropEffects.None;e.Handled=true;};b.Drop+=(_,e)=>{if(e.Data.GetData("Captovix.Tool") is not string id)return;var current=ToolBox.Children.OfType<Button>().ToList();var source=current.FirstOrDefault(t=>ToolId(t)==id);if(source==null||source==b)return;current.Remove(source);current.Insert(current.IndexOf(b),source);ReorderTools(current.Select(ToolId));SaveToolOrder();e.Handled=true;};}
    }
    private void ReorderTools(IEnumerable<string> ids){var all=_defaultTools.ToDictionary(ToolId);var list=ids.Where(all.ContainsKey).Select(id=>all[id]).Distinct().ToList();list.AddRange(_defaultTools.Except(list));ToolBox.Children.Clear();foreach(var b in list)ToolBox.Children.Add(b);}
    private void SaveToolOrder(){try{MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"tool-order.json"),JsonSerializer.Serialize(ToolBox.Children.OfType<Button>().Select(ToolId)));}catch(Exception ex){SetStatus("Chưa lưu được vị trí công cụ: "+ex.Message);}}
    private void ToolOrder_Click(object sender,RoutedEventArgs e)
    {
        if(_busy)return;var w=new Window{Owner=this,Title="Sắp xếp công cụ cá nhân",Width=420,Height=560,WindowStartupLocation=WindowStartupLocation.CenterOwner};var panel=new DockPanel{Margin=new(16)};w.Content=panel;var row=new WrapPanel();DockPanel.SetDock(row,Dock.Bottom);panel.Children.Add(row);var list=new ListBox();panel.Children.Add(list);
        void Refresh(string? selected=null){list.ItemsSource=ToolBox.Children.OfType<Button>().Where(b=>b.Visibility==Visibility.Visible).Select(b=>new ToolRow(ToolId(b),System.Windows.Automation.AutomationProperties.GetName(b))).ToArray();list.DisplayMemberPath="Label";list.SelectedItem=list.Items.Cast<ToolRow>().FirstOrDefault(t=>t.Id==selected);}
        void Action(string text,Action action){var b=new Button{Content=text};b.Click+=(_,_)=>action();row.Children.Add(b);}
        void Move(int delta){if(list.SelectedItem is not ToolRow item)return;var ids=ToolBox.Children.OfType<Button>().Select(ToolId).ToList();int i=ids.IndexOf(item.Id),j=Math.Clamp(i+delta,0,ids.Count-1);ids.RemoveAt(i);ids.Insert(j,item.Id);ReorderTools(ids);SaveToolOrder();Refresh(item.Id);}
        Action("Lên",()=>Move(-1));Action("Xuống",()=>Move(1));Action("Mặc định",()=>{ReorderTools(_defaultTools.Select(ToolId));SaveToolOrder();Refresh();});Action("Đóng",w.Close);Refresh();w.ShowDialog();
    }
    private sealed record ToolRow(string Id,string Label);
}
