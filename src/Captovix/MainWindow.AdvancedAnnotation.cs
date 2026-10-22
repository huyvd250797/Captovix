using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Captovix;
public partial class MainWindow
{
    private string? _inlineId;
    private bool _endingInline;
    private void BeginInlineText()
    {
        EndInlineText(true);
        if(_session==null||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds)||Surface.Selected is not {} a||a.Kind is not (AnnotationKind.Text or AnnotationKind.Callout))return;
        _textTimer.Stop();_session.Commit();_saveTimer.Stop();
        if(Surface.Zoom<.5)ZoomAtCenter(.5);
        _inlineId=a.Id;InlineText.Text=a.Text;InlineText.Visibility=Visibility.Visible;PositionInlineText();
        Dispatcher.BeginInvoke(()=>{if(_inlineId!=a.Id)return;InlineText.Focus();InlineText.SelectAll();InlineText.BringIntoView();},DispatcherPriority.Input);
        SetStatus("Đang sửa chữ trên ảnh · Ctrl+Enter hoàn tất · Esc hủy · Enter xuống dòng.");
    }
    private void PositionInlineText()
    {
        if(_inlineId==null||_session==null)return;
        var a=_session.Document.Annotations.FirstOrDefault(x=>x.Id==_inlineId);if(a==null){EndInlineText(false);return;}
        var view=_session.Document.Viewport;bool callout=a.Kind==AnnotationKind.Callout;
        double x=callout?a.Left+12:a.X1,y=callout?a.Top+12:a.Y1;
        var layout=Surface.Layout!;InlineCanvas.RenderTransform=new MatrixTransform(EditorRenderer.LayoutMatrix(layout,Surface.Zoom));
        var inverse=EditorRenderer.LayoutMatrix(layout);inverse.Invert();InlineCanvas.Clip=new RectangleGeometry(EditorRenderer.RectOf(layout.Canvas)){Transform=new MatrixTransform(inverse)};
        Canvas.SetLeft(InlineText,x);Canvas.SetTop(InlineText,y);
        var frame=EditorRenderer.LocalBox(a);InlineText.RenderTransform=new RotateTransform(a.Rotation,frame.CenterX-x,frame.CenterY-y);
        InlineText.FontSize=a.FontSize;InlineText.Foreground=EditorRenderer.Ink(a.Color);
        InlineText.TextWrapping=callout?TextWrapping.Wrap:TextWrapping.NoWrap;
        InlineText.Width=Math.Max(24,callout?Math.Max(1,a.Width-24):Math.Max(160,Math.Min(view.Right-x,Math.Max(160,EditorRenderer.LocalBounds(a).Width+40))));
        InlineText.MaxHeight=Math.Max(24,callout?Math.Max(1,a.Height-24):Math.Max(24,view.Bottom-y));
    }
    private void EndInlineText(bool commit)
    {
        if(_endingInline||_inlineId==null)return;_endingInline=true;
        try{
            var id=_inlineId;var text=InlineText.Text;_inlineId=null;InlineText.Visibility=Visibility.Collapsed;
            if(commit&&_session is {} session&&session.Document.Annotations.Any(a=>a.Id==id)){
                session.Commit();session.Change(d=>d.Annotations.First(a=>a.Id==id).Text=text);
            }
            if(Surface.Tool is EditorTool.Text or EditorTool.Callout)SelectTool(EditorTool.Select);
            Surface.InvalidateVisual();SyncProperties();if(_session?.Dirty==true){_saveTimer.Stop();_saveTimer.Start();}
        }finally{_endingInline=false;}
    }
    private void InlineText_KeyDown(object sender,KeyEventArgs e)
    {
        if(e.Key==Key.Escape){EndInlineText(false);Surface.Focus();e.Handled=true;}
        else if(e.Key==Key.Enter&&Keyboard.Modifiers.HasFlag(ModifierKeys.Control)){EndInlineText(true);Surface.Focus();e.Handled=true;}
    }
    private void InlineText_LostFocus(object sender,KeyboardFocusChangedEventArgs e){if(!InlineText.IsKeyboardFocusWithin)EndInlineText(true);}
    private void EditOnImage_Click(object sender,RoutedEventArgs e)=>BeginInlineText();
    private void SyncAdvancedAnnotation()=>SyncConnectorTools();
    private void ObjectCommand(string command)
    {
        if(_session==null||Surface.Tool==EditorTool.Crop||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds))return;
        EndInlineText(true);Surface.CancelInteraction();_session.Commit();var ids=Surface.SelectedIds.ToHashSet();
        try{
            if(command=="Duplicate"){
                HashSet<string> copies=[];_session.Change(d=>copies=AnnotationEditing.Duplicate(d,ids,EditorRenderer.Box));Surface.SelectIds(copies);
            }else if(command=="Group"){
                if(AnnotationEditing.Units(_session.Document,ids).Length<2)return;
                _session.Change(d=>AnnotationEditing.Group(d,ids));Surface.SelectIds(ids);
            }else if(command=="Ungroup"){
                _session.Change(d=>AnnotationEditing.Ungroup(d,ids));Surface.SelectIds(ids);
            }else if(command is "Horizontal" or "Vertical"){
                bool changed=false;_session.Change(d=>{changed=AnnotationEditing.Distribute(d,ids,command=="Horizontal",EditorRenderer.Box);if(changed)ConnectorStudio.DetachForTransform(d,ids);});
                if(!changed)SetStatus("Chọn ít nhất 3 đối tượng/nhóm và để đủ khoảng trống giữa hai đầu để chia đều.");
            }else{
                _session.Change(d=>AnnotationEditing.Reorder(d,ids,Enum.Parse<AnnotationOrder>(command)));
                if(_session.Document.Annotations.Any(a=>ids.Contains(a.Id)&&a.IsRedaction))SetStatus("Đã sắp xếp trong lớp bảo vệ; vùng che luôn phủ trên chú thích.");
            }
            Surface.Refresh();SyncProperties();Surface.Focus();
        }catch(Exception ex){SetStatus("Chưa thực hiện được: "+ex.Message);}
    }
    private void ObjectCommand_Click(object sender,RoutedEventArgs e){if(sender is Button {Tag:string command})ObjectCommand(command);}
}
