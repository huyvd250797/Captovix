using System.Windows;
using System.Windows.Controls;
namespace Captovix;
public partial class MainWindow
{
    private AlignmentReference _alignmentReference;
    private void SavePrecisionPreferences(){if(!_loaded)return;try{_store.SaveSettings(Settings());}catch(Exception ex){App.Log(ex);SetStatus("Chưa lưu được tùy chọn đối tượng: "+ex.Message);}}
    private void ObjectRatio_Changed(object sender,RoutedEventArgs e)
    {
        if(!_layersReady||_syncLayers||_syncProperties||Surface==null)return;Surface.KeepAspectRatio=ObjectKeepRatio.IsChecked==true;SavePrecisionPreferences();
    }
    private void AddAlignmentReference(MenuItem parent)
    {
        var reference=new MenuItem{Header="Tham chiếu căn"};parent.Items.Add(reference);
        foreach(var (name,value) in new[]{("Lựa chọn · một đối tượng căn theo canvas",AlignmentReference.Selection),("Vùng ảnh đang thấy",AlignmentReference.Image),("Toàn canvas",AlignmentReference.Canvas)}){
            var item=new MenuItem{Header=name,IsCheckable=true,IsChecked=_alignmentReference==value,StaysOpenOnClick=true};item.Click+=(_,_)=>{_alignmentReference=value;foreach(var choice in reference.Items.OfType<MenuItem>())choice.IsChecked=choice==item;SavePrecisionPreferences();SetStatus("Đã chọn tham chiếu căn: "+name);};reference.Items.Add(item);
        }
        parent.Items.Add(new Separator());
    }
    private void AddPrecisionMenu(ContextMenu menu)
    {
        if(_session==null||Surface.Tool==EditorTool.Crop)return;
        var pick=new MenuItem{Header="Chọn đối tượng tại đây"};menu.Items.Add(pick);
        var stack=Surface.ContextSourcePoint is {} p?Surface.HitStack(p,true):[];var units=stack.GroupBy(a=>a.GroupId??a.Id).Select(g=>g.First()).ToArray();pick.IsEnabled=units.Length>0;
        foreach(var a in units.Take(50)){
            string id=a.Id;int members=ObjectControl.Members(_session.Document,[id]).Length;string label=(string.IsNullOrWhiteSpace(a.Name)?ShapeStudio.Label(a.Kind):a.Name)+(members>1?$" · Nhóm {members}":"")+(a.Locked?" · Khóa":"");
            var item=new MenuItem{Icon=AppIcons.Content(a.Kind==AnnotationKind.Callout?"Callout":a.Kind==AnnotationKind.Text?"Text":ShapeStudio.IsLine(a.Kind)?"Arrow":"Shape",""),Header=label,IsCheckable=true,IsChecked=Surface.SelectedIds.Contains(id)};item.Click+=(_,_)=>{SelectTool(EditorTool.Select);Surface.SelectIds([id],id);Surface.Focus();};pick.Items.Add(item);
        }
        if(units.Length>50){var all=new MenuItem{Header="Xem toàn bộ trong Layers…"};all.Click+=(_,_)=>{if(Ancestor<Expander>(LayerList) is {} panel)panel.IsExpanded=true;LayerList.BringIntoView();};pick.Items.Add(all);}
        menu.Items.Add(new Separator());
    }
}
