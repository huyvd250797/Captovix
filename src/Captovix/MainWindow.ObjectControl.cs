using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace Captovix;
public partial class MainWindow
{
    private bool _layersReady,_syncLayers,_editingObjectRatio;
    private LayerRow[] _layerRows=[];
    private double _objectAspect=1;
    private sealed record LayerRow(string Id,string Label,string Detail,bool Locked,bool Visible)
    {
        public string LockLabel=>Locked?"🔒":"🔓";
        public string VisibleLabel=>Visible?"◉":"○";
    }
    private void InitializeObjectControl(){_layersReady=true;}
    private static string ObjectNumber(double n)=>n.ToString("0.######",CultureInfo.InvariantCulture);
    private void SyncObjectControl()
    {
        if(!_layersReady)return;_syncLayers=true;
        try{
            var session=_session!=null&&ReferenceEquals(_session,Surface.Session)?_session:null;
            var doc=session?.Document;var objects=doc==null?[]:ObjectControl.Members(doc,Surface.SelectedIds);
            var rows=doc?.Annotations.Select((a,i)=>new{Object=a,Index=i}).OrderBy(x=>AnnotationEditing.Tier(x.Object)).ThenBy(x=>x.Index).Reverse().Select(x=>new LayerRow(x.Object.Id,string.IsNullOrWhiteSpace(x.Object.Name)?$"{ShapeStudio.Label(x.Object.Kind)} {x.Index+1}":x.Object.Name,$"{ShapeStudio.Label(x.Object.Kind)}{(x.Object.GroupId!=null?" · Nhóm "+x.Object.GroupId[..4]:"")}{(x.Object.IsRedaction?" · Bảo vệ":x.Object.Kind==AnnotationKind.Spotlight?" · Nền focus":"")}",x.Object.Locked,x.Object.Visible)).ToArray()??[];
            if(!_layerRows.SequenceEqual(rows)){_layerRows=rows;LayerList.ItemsSource=_layerRows;}
            LayerList.SelectedItems.Clear();foreach(var row in _layerRows)if(Surface.SelectedIds.Contains(row.Id))LayerList.SelectedItems.Add(row);
            LayerCount.Text=doc==null?"Chưa mở ảnh.":$"{doc.Annotations.Count} đối tượng · {doc.Annotations.Count(a=>a.Visible)} đang hiện";
            bool editable=doc!=null&&ObjectControl.CanEdit(doc,Surface.SelectedIds)&&!Surface.IsInteracting;
            EditableObjectProperties.IsEnabled=objects.Length==0||editable;ObjectFrameFields.IsEnabled=editable;
            ObjectFrameFields.Visibility=objects.Length==0?Visibility.Collapsed:Visibility.Visible;
            RenameLayerButton.IsEnabled=Surface.Selected!=null&&!Surface.IsInteracting;LayerName.IsEnabled=RenameLayerButton.IsEnabled;
            LayerName.Text=Surface.Selected?.Name??"";
            LayerMessage.Text=objects.Any(a=>a.Locked)?"Lựa chọn có đối tượng khóa. Mở khóa tại dòng layer để chỉnh.":objects.Any(a=>!a.Visible)?"Lựa chọn có đối tượng ẩn. Bật hiện tại dòng layer để chỉnh; layer ẩn không có trong ảnh xuất.":"";
            ObjectFrameError.Text="";
            if(objects.Length==0){ObjectFrameHint.Text="Chọn đối tượng trên ảnh hoặc trong Layers.";ObjectX.Text=ObjectY.Text=ObjectWidth.Text=ObjectHeight.Text=ObjectAngle.Text="";return;}
            var frame=objects.Length==1?EditorRenderer.LocalBox(objects[0]):AnnotationEditing.Union(objects.Select(EditorRenderer.Box));
            _objectAspect=frame.Width>1e-9&&frame.Height>1e-9?frame.Width/frame.Height:0;ObjectX.Text=ObjectNumber(frame.X);ObjectY.Text=ObjectNumber(frame.Y);ObjectWidth.Text=ObjectNumber(frame.Width);ObjectHeight.Text=ObjectNumber(frame.Height);ObjectAngle.Text=objects.Length==1?ObjectNumber(objects[0].Rotation):"0";
            bool force=objects.Length>1&&objects.Any(a=>Math.Abs(a.Rotation)>1e-8)||objects.Length==1&&objects[0].Kind is AnnotationKind.Text or AnnotationKind.Step;
            if(force)ObjectKeepRatio.IsChecked=true;if(_objectAspect==0)ObjectKeepRatio.IsChecked=false;ObjectKeepRatio.IsEnabled=!force&&_objectAspect>0;
            ObjectFrameHint.Text=objects.Length==1&&ConnectorStudio.IsPath(objects[0])?"Đường cong/gấp khúc: khung bao theo pixel gốc; góc là xoay thêm và được áp vào tọa độ điểm. Đổi khung sẽ tháo các liên kết ngoài lựa chọn.":objects.Length==1?"Khung trước xoay · pixel ảnh gốc. Góc là góc tuyệt đối; chữ/số giữ tỷ lệ theo cỡ chữ.":"Khung bao nhóm trong ảnh gốc. Góc là xoay thêm quanh tâm nhóm; nhóm có đối tượng xoay giữ tỷ lệ.";
            ObjectFrameError.Text="";
        }finally{_syncLayers=false;}
    }
    private void LayerSelection_Changed(object sender,SelectionChangedEventArgs e)
    {
        if(!_layersReady||_syncLayers||_session==null)return;
        var ids=LayerList.SelectedItems.Cast<LayerRow>().Select(r=>r.Id).ToArray();var primary=(LayerList.SelectedItem as LayerRow)?.Id;
        SelectTool(EditorTool.Select);Surface.SelectIds(ids,primary);SyncProperties();
    }
    private void LayerToggle_Click(object sender,RoutedEventArgs e)
    {
        if(_session==null||sender is not Button {CommandParameter:string id,Tag:string command}||Surface.IsInteracting)return;
        EndInlineText(true);var a=_session.Document.Annotations.FirstOrDefault(a=>a.Id==id);if(a==null)return;bool next=command=="Lock"?!a.Locked:!a.Visible;
        _session.Commit();_session.Change(d=>{if(command=="Lock")ObjectControl.Lock(d,[id],next);else ObjectControl.Show(d,[id],next);});Surface.Refresh();SyncProperties();e.Handled=true;
    }
    private void RenameLayer_Click(object sender,RoutedEventArgs e)
    {
        if(_session==null||Surface.Selected is not {} a||Surface.IsInteracting)return;var name=LayerName.Text;var id=a.Id;
        try{_session.Commit();_session.Change(d=>ObjectControl.Rename(d.Annotations.First(a=>a.Id==id),name));Surface.Refresh();SyncProperties();}catch(Exception ex){LayerMessage.Text=ex.Message;}
    }
    private void LayerName_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Enter){RenameLayer_Click(sender,e);e.Handled=true;}else if(e.Key==Key.Escape){SyncProperties();e.Handled=true;}}
    private void ObjectSnap_Changed(object sender,RoutedEventArgs e)
    {
        if(!_layersReady||Surface==null)return;Surface.SnapEnabled=ObjectSnapToggle.IsChecked==true;
        if(_loaded)try{_store.SaveSettings(Settings());}catch(Exception ex){App.Log(ex);SetStatus("Chưa lưu được cấu hình snap: "+ex.Message);}
    }
    private void ObjectWidth_Changed(object sender,TextChangedEventArgs e)=>UpdateObjectRatio(true);
    private void ObjectHeight_Changed(object sender,TextChangedEventArgs e)=>UpdateObjectRatio(false);
    private void UpdateObjectRatio(bool width)
    {
        if(!_layersReady||_syncLayers||_syncProperties||_editingObjectRatio||ObjectKeepRatio.IsChecked!=true||_objectAspect<=0)return;
        var input=width?ObjectWidth:ObjectHeight;if(!double.TryParse(input.Text.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out var n)||!double.IsFinite(n)||n<=0)return;
        _editingObjectRatio=true;try{if(width)ObjectHeight.Text=ObjectNumber(n/_objectAspect);else ObjectWidth.Text=ObjectNumber(n*_objectAspect);}finally{_editingObjectRatio=false;}
    }
    private bool ApplyObjectFrame()
    {
        if(_session==null||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds)||Surface.IsInteracting)return false;
        try{
            bool line=Surface.SelectedIds.Count==1&&Surface.Selected is {} selected&&ShapeStudio.IsLine(selected.Kind);
            double x=ShapeValue(ObjectX,"X",-1000000,1000000),y=ShapeValue(ObjectY,"Y",-1000000,1000000),w=ShapeValue(ObjectWidth,"Rộng",line?0:1,100000),h=ShapeValue(ObjectHeight,"Cao",line?0:1,100000),angle=ShapeValue(ObjectAngle,"Góc",-360,360);
            var ids=Surface.SelectedIds.ToArray();EndInlineText(true);_session.Commit();_session.Change(d=>{ObjectControl.ApplyFrame(d,ids,x,y,w,h,angle,EditorRenderer.LocalBox);foreach(var target in d.Annotations.Where(a=>ids.Contains(a.Id)))EditorRenderer.FitText(target,d.Viewport);d.Validate(d.OriginalWidth,d.OriginalHeight,d.SourceHash);});Surface.Refresh();SyncProperties();return true;
        }catch(Exception ex){Surface.Refresh();SyncProperties();ObjectFrameError.Text=ex.Message;return false;}
    }
    private void ApplyObjectFrame_Click(object sender,RoutedEventArgs e)=>ApplyObjectFrame();
    private void ObjectField_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.Enter){ApplyObjectFrame();e.Handled=true;}else if(e.Key==Key.Escape){SyncProperties();e.Handled=true;}}
}
