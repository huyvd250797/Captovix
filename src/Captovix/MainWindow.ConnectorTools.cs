using System.Windows;
using System.Windows.Controls;
namespace Captovix;
public partial class MainWindow
{
    private void InitializeCanvasMenu()
    {
        var menu=new ContextMenu();menu.Items.Add(new MenuItem{Header="Thao tác ảnh"});Surface.ContextMenu=menu;
        Surface.ContextMenuOpening+=(_,e)=>{if(!_editorReady||_session==null){e.Handled=true;return;}BuildCanvasMenu();};
    }
    private void BuildCanvasMenu()
    {
        if(!_editorReady||_session==null||Surface.ContextMenu is not {} menu)return;
        EndInlineText(true);menu.Items.Clear();
            bool active=ObjectControl.CanEdit(_session.Document,Surface.SelectedIds)&&Surface.Tool!=EditorTool.Crop;
            var units=AnnotationEditing.Units(_session.Document,Surface.SelectedIds);
            MenuItem Add(ItemsControl owner,string title,Action action,bool enabled=true){var item=new MenuItem{Header=title,IsEnabled=enabled};item.Click+=(_,_)=>action();owner.Items.Add(item);return item;}
            Add(menu,"Hoàn tác",()=>Undo_Click(this,new()),_session.CanUndo);Add(menu,"Làm lại",()=>Redo_Click(this,new()),_session.CanRedo);menu.Items.Add(new Separator());
            Add(menu,"Copy đối tượng · Ctrl+C",CopyObjects,Surface.SelectedIds.Count>0);Add(menu,"Dán đối tượng · Ctrl+V",PasteObjects,HasObjectClipboard());
            Add(menu,"Nhân bản · Ctrl+D",()=>ObjectCommand("Duplicate"),active);
            Add(menu,"Nhóm · Ctrl+G",()=>ObjectCommand("Group"),active&&units.Length>1);
            Add(menu,"Bỏ nhóm · Ctrl+Shift+G",()=>ObjectCommand("Ungroup"),active&&units.Any(u=>u.Any(a=>a.GroupId!=null)));
            var order=new MenuItem{Header="Sắp xếp lớp",IsEnabled=active};menu.Items.Add(order);
            foreach(var (title,command) in new[]{("Lên trước cùng","Front"),("Lên một lớp","Forward"),("Xuống một lớp","Backward"),("Xuống sau cùng","Back")})Add(order,title,()=>ObjectCommand(command));
            var align=new MenuItem{Header="Căn chỉnh",IsEnabled=active};menu.Items.Add(align);
            foreach(var (title,command) in new[]{("Trái","Left"),("Giữa ngang","CenterX"),("Phải","Right"),("Trên","Top"),("Giữa dọc","CenterY"),("Dưới","Bottom")})Add(align,title,()=>AlignObjects(command));
            Add(align,"Chia đều ngang",()=>ObjectCommand("Horizontal"),units.Length>=3);Add(align,"Chia đều dọc",()=>ObjectCommand("Vertical"),units.Length>=3);
            var members=ObjectControl.Members(_session.Document,Surface.SelectedIds);bool unlock=members.Any(a=>a.Locked);
            Add(menu,unlock?"Mở khóa đối tượng":"Khóa đối tượng",()=>{_session.Change(d=>ObjectControl.Lock(d,Surface.SelectedIds,!unlock));Surface.Refresh();SyncProperties();},members.Length>0);
            Add(menu,"Ẩn đối tượng · hiện lại ở Layers",()=>{_session.Change(d=>ObjectControl.Show(d,Surface.SelectedIds,false));Surface.Refresh();SyncProperties();},active);
            Add(menu,"Tháo liên kết đường nối",()=>Surface.DetachConnectors(),active&&members.Any(a=>a.StartAnchor!=null||a.EndAnchor!=null));
            Add(menu,"Xóa đối tượng · Delete",()=>Surface.DeleteSelected(),active);menu.Items.Add(new Separator());
            Add(menu,"Copy ảnh đã chỉnh · Ctrl+Shift+C",()=>Copy_Click(this,new()));Add(menu,"Xuất ảnh…",()=>Export_Click(this,new()));Add(menu,"OCR & chỉnh chữ…",()=>Assist_Click(this,new()));AppMenus.Decorate(menu);
    }
    private void AlignObjects(string tag)
    {
        if(_session==null||Surface.Tool==EditorTool.Crop||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds))return;
        EndInlineText(true);Surface.CancelInteraction();var ids=Surface.SelectedIds.ToHashSet();_session.Commit();
        _session.Change(d=>{ConnectorStudio.DetachForTransform(d,ids);AnnotationEditing.Align(d,ids,Enum.Parse<AnnotationAlignment>(tag),EditorRenderer.Box);});Surface.Refresh();SyncProperties();Surface.Focus();
    }
    private void SyncConnectorTools()
    {
        var selected=Surface.Selected;bool line=selected!=null&&ShapeStudio.IsLine(selected.Kind)||Surface.Tool is EditorTool.Arrow or EditorTool.Line;
        ConnectorPanel.Visibility=line?Visibility.Visible:Visibility.Collapsed;if(!line)return;
        ConnectorRoutePicker.SelectedIndex=(int)(selected!=null&&ShapeStudio.IsLine(selected.Kind)?selected.Route:Surface.ArrowRoute);
        ConnectorAttachToggle.IsChecked=Surface.ConnectToShapes;
        int links=selected==null?0:(selected.StartAnchor!=null?1:0)+(selected.EndAnchor!=null?1:0);
        ConnectorSummary.Text=Surface.SelectedIds.Count>0?$"{Surface.SelectedIds.Count} đối tượng được chọn · {links}/2 đầu đang liên kết":"Kéo từ shape này sang shape khác để tạo đường nối. V để chỉnh hai đầu và điểm điều khiển.";
        DetachConnectorButton.IsEnabled=links>0;
    }
    private void ConnectorRoute_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(!_editorReady||_syncProperties||ConnectorRoutePicker.SelectedIndex<0)return;
        EndInlineText(true);Surface.SetConnectorRoute((ConnectorRoute)ConnectorRoutePicker.SelectedIndex);SyncProperties();
    }
    private void ConnectorAttach_Changed(object sender,RoutedEventArgs e){if(_editorReady&&!_syncProperties)Surface.ConnectToShapes=ConnectorAttachToggle.IsChecked==true;}
    private void DetachConnector_Click(object sender,RoutedEventArgs e)=>Surface.DetachConnectors();
}
