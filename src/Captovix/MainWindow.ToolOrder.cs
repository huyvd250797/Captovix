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
    private Button[]? _toolSwapOriginal;private string? _toolSwapSource,_toolSwapTarget;private Point _toolSwapPoint;
    private static string ToolId(Button b)=>b.Tag?.ToString()??b.Name;
    private static T? Ancestor<T>(DependencyObject? node) where T:DependencyObject{while(node!=null){if(node is T t)return t;node=node is Visual||node is System.Windows.Media.Media3D.Visual3D?VisualTreeHelper.GetParent(node):LogicalTreeHelper.GetParent(node);}return null;}
    private void InitializeToolOrder()
    {
        _defaultTools=ToolBox.Children.OfType<Button>().ToArray();try{string path=Path.Combine(App.DataRoot,"tool-order-v133.json");if(File.Exists(path)){var ids=JsonSerializer.Deserialize<string[]>(File.ReadAllText(path))??[];if(ids.Length>64||ids.Distinct().Count()!=ids.Length)throw new InvalidDataException("Bố cục công cụ lỗi.");ReorderTools(ids);}}catch(Exception ex){App.Log(ex);SetStatus("Không đọc được bố cục công cụ, đang dùng thứ tự mặc định.");}
        ToolBox.PreviewMouseLeftButtonDown+=(_,e)=>{_toolDragButton=Ancestor<Button>(e.OriginalSource as DependencyObject);_toolDragPoint=e.GetPosition(ToolBox);};
        ToolBox.PreviewMouseMove+=(_,e)=>{
            if(_toolDragButton==null||e.LeftButton!=MouseButtonState.Pressed||(e.GetPosition(ToolBox)-_toolDragPoint).Length<8)return;
            var button=_toolDragButton;_toolDragButton=null;if(button==RecentStudioButton||button==StudioMoreButton)return;
            var original=ToolBox.Children.OfType<Button>().ToArray();_toolSwapOriginal=original;_toolSwapSource=ToolId(button);_toolSwapTarget=null;_toolSwapPoint=new(-1000,-1000);
            try{var effect=DragDrop.DoDragDrop(button,new DataObject("Captovix.Tool",ToolId(button)),DragDropEffects.Move);if(effect!=DragDropEffects.Move)ReorderTools(original.Select(ToolId));else SaveToolOrder();}
            finally{_toolSwapOriginal=null;_toolSwapSource=null;_toolSwapTarget=null;}e.Handled=true;
        };
        foreach(var target in _defaultTools){target.AllowDrop=true;target.DragOver+=(_,e)=>{
            if(_toolSwapOriginal==null||target==RecentStudioButton||target==StudioMoreButton||!e.Data.GetDataPresent("Captovix.Tool")){e.Effects=DragDropEffects.None;e.Handled=true;return;}
            var point=e.GetPosition(ToolBox);if(_toolSwapTarget!=ToolId(target)&&(point-_toolSwapPoint).Length>=6){var list=_toolSwapOriginal.ToList();int from=list.FindIndex(b=>ToolId(b)==_toolSwapSource),to=list.IndexOf(target);if(from>=0&&to>=0){(list[from],list[to])=(list[to],list[from]);ReorderTools(list.Select(ToolId));_toolSwapTarget=ToolId(target);_toolSwapPoint=point;ToolHint.Text="Thả để đổi vị trí hai công cụ · Esc hủy";}}
            e.Effects=DragDropEffects.Move;e.Handled=true;
        };target.Drop+=(_,e)=>{if(_toolSwapOriginal==null||target==RecentStudioButton||target==StudioMoreButton)return;e.Effects=DragDropEffects.Move;e.Handled=true;};}

    }
    private void ReorderTools(IEnumerable<string> ids){var all=_defaultTools.ToDictionary(ToolId);var list=ids.Where(all.ContainsKey).Select(id=>all[id]).Distinct().ToList();list.AddRange(_defaultTools.Except(list));if(list.Remove(RecentStudioButton))list.Insert(Math.Max(0,list.IndexOf(StudioMoreButton)),RecentStudioButton);ToolBox.Children.Clear();foreach(var b in list)ToolBox.Children.Add(b);}
    private void SaveToolOrder(){try{MediaStore.AtomicWrite(Path.Combine(App.DataRoot,"tool-order-v133.json"),JsonSerializer.Serialize(ToolBox.Children.OfType<Button>().Select(ToolId)));}catch(Exception ex){SetStatus("Chưa lưu được vị trí công cụ: "+ex.Message);}}
    private void ToolOrder_Click(object sender,RoutedEventArgs e)
    {if(!RequireLicense())return;
        if(_busy)return;var w=new Window{Owner=this,Title="Sắp xếp công cụ cá nhân",Width=420,Height=560,WindowStartupLocation=WindowStartupLocation.CenterOwner};var panel=new DockPanel{Margin=new(16)};w.Content=panel;var row=new WrapPanel();DockPanel.SetDock(row,Dock.Bottom);panel.Children.Add(row);var list=new ListBox();panel.Children.Add(list);
        void Refresh(string? selected=null){list.ItemsSource=ToolBox.Children.OfType<Button>().Where(b=>b.Visibility==Visibility.Visible).Select(b=>new ToolRow(ToolId(b),System.Windows.Automation.AutomationProperties.GetName(b))).ToArray();list.DisplayMemberPath="Label";list.SelectedItem=list.Items.Cast<ToolRow>().FirstOrDefault(t=>t.Id==selected);}
        void Action(string text,Action action){var b=new Button{Content=text};b.Click+=(_,_)=>action();row.Children.Add(b);}
        void Move(int delta){if(list.SelectedItem is not ToolRow item)return;var ids=ToolBox.Children.OfType<Button>().Select(ToolId).ToList();int i=ids.IndexOf(item.Id),j=Math.Clamp(i+delta,0,ids.Count-1);ids.RemoveAt(i);ids.Insert(j,item.Id);ReorderTools(ids);SaveToolOrder();Refresh(item.Id);}
        Action("Lên",()=>Move(-1));Action("Xuống",()=>Move(1));Action("Mặc định",()=>{ReorderTools(_defaultTools.Select(ToolId));SaveToolOrder();Refresh();});Action("Đóng",w.Close);Refresh();w.ShowDialog();
    }
    private sealed record ToolRow(string Id,string Label);
}
