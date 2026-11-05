using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Captovix;
public partial class MainWindow
{
    private string? _inlineId;
    private bool _endingInline,_syncInline;
    private bool _inlineRefreshQueued;
    private double _inlineWrapWidth;
    private string _inlineInitialText="";
    private void UpdateInlineValue(Annotation a,string value){if(a.Mockup!=null)EditorRenderer.SetMockupText(a,value,_inlineWrapWidth);else a.Text=value;}

    private void BeginInlineText()
    {if(!RequireLicense())return;
        EndInlineText(true);
        if(_session==null||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds)||Surface.Selected is not {} a||a.Kind is not (AnnotationKind.Text or AnnotationKind.Callout)&&a.Mockup==null)return;
        _textTimer.Stop();_session.Commit();_saveTimer.Stop();
        
        _inlineInitialText=a.Text;_inlineWrapWidth=a.Mockup!=null?EditorRenderer.MockupEditingWidth(a):0;_imeComposing=false;_inlineId=a.Id;_session.Begin();Surface.InlineEditingId=a.Id;_syncInline=true;try{InlineText.Text=a.Text;_inlineInitialText=InlineText.Text;}finally{_syncInline=false;}InlineText.Visibility=Visibility.Visible;Surface.InvalidateVisual();PositionInlineText();
        Dispatcher.BeginInvoke(()=>{if(_inlineId!=a.Id)return;InlineText.Focus();InlineText.SelectAll();InlineText.BringIntoView();},DispatcherPriority.Input);
        SetStatus("Đang sửa chữ trên ảnh · Ctrl+Enter hoàn tất · Esc hủy · Enter xuống dòng.");
    }
    private void PositionInlineText()
    {
        if(_inlineId==null||_session==null)return;
        var a=_session.Document.Annotations.FirstOrDefault(x=>x.Id==_inlineId);if(a==null){EndInlineText(false);return;}
        var view=Surface.EditingBounds;bool callout=a.Kind==AnnotationKind.Callout;bool scanLine=a.Mockup is {FromScan:true,Type:MockupKind.Label or MockupKind.Title};
        var content=EditorRenderer.TextContent(a);var textLayout=EditorRenderer.TextLayout(a);var origin=EditorRenderer.TextOrigin(a,textLayout);double x=origin.X,y=origin.Y;if(callout||a.Mockup!=null&&!scanLine){x=content.X;y=content.Y;}
        var layout=Surface.Layout!;InlineCanvas.RenderTransform=new MatrixTransform(new Matrix(layout.A*Surface.Zoom,layout.B*Surface.Zoom,layout.C*Surface.Zoom,layout.D*Surface.Zoom,layout.Tx*Surface.Zoom+EditorSurface.WorkspacePad,layout.Ty*Surface.Zoom+EditorSurface.WorkspacePad));
        var inverse=EditorRenderer.LayoutMatrix(layout);inverse.Invert();InlineCanvas.Clip=new RectangleGeometry(new Rect(-EditorSurface.WorkspacePad/Surface.Zoom,-EditorSurface.WorkspacePad/Surface.Zoom,layout.Width+2*EditorSurface.WorkspacePad/Surface.Zoom,layout.Height+2*EditorSurface.WorkspacePad/Surface.Zoom)){Transform=new MatrixTransform(inverse)};
        Canvas.SetLeft(InlineText,x);Canvas.SetTop(InlineText,y);
        var frame=EditorRenderer.LocalBox(a);InlineText.RenderTransform=new RotateTransform(a.Rotation,frame.CenterX-x,frame.CenterY-y);
        InlineText.FontSize=a.FontSize;InlineText.FontFamily=EditorRenderer.TextFont(a);InlineText.FontWeight=a.Bold?FontWeights.Bold:FontWeights.Normal;InlineText.FontStyle=a.Italic?FontStyles.Italic:FontStyles.Normal;InlineText.TextAlignment=EditorRenderer.WpfAlignment(a.TextAlignment);InlineText.Foreground=EditorRenderer.Ink(TextStudio.Foreground(a));InlineText.CaretBrush=InlineText.Foreground;InlineText.Background=Brushes.Transparent;InlineText.BorderThickness=new(0);InlineText.Padding=new(0);
        TextBlock.SetLineHeight(InlineText,a.LineSpacing>0?a.FontSize*a.LineSpacing:double.NaN);TextBlock.SetLineStackingStrategy(InlineText,LineStackingStrategy.BlockLineHeight);InlineText.SetValue(TextBlock.TextDecorationsProperty,a.Underline?TextDecorations.Underline:null);
        System.Windows.Documents.TextElement.SetFontFamily(InlineText,InlineText.FontFamily);
        InlineText.TextWrapping=callout||a.TextWidth>0?TextWrapping.Wrap:TextWrapping.NoWrap;
        InlineText.Width=Math.Max(24,callout?content.Width:a.TextWidth>0?a.TextWidth:Math.Max(160,textLayout.WidthIncludingTrailingWhitespace+40));
        InlineText.VerticalContentAlignment=callout||a.Mockup!=null?a.VerticalAlignment==TextVertical.Center?VerticalAlignment.Center:a.VerticalAlignment==TextVertical.Bottom?VerticalAlignment.Bottom:VerticalAlignment.Top:VerticalAlignment.Top;
        InlineText.Height=callout?Math.Max(1,content.Height):a.TextFrame&&a.Kind==AnnotationKind.Text?Math.Max(1,content.Height):Math.Max(a.FontSize+4,textLayout.Height+4);
        if(a.TextFrame&&a.Kind==AnnotationKind.Text){InlineText.Width=Math.Max(1,content.Width);InlineText.TextWrapping=TextWrapping.Wrap;InlineText.VerticalScrollBarVisibility=ScrollBarVisibility.Auto;}else InlineText.VerticalScrollBarVisibility=ScrollBarVisibility.Hidden;
        if(a.Mockup!=null){InlineText.MaxLength=2000;InlineText.Width=Math.Max(24,_inlineWrapWidth+4);InlineText.Height=Math.Max(content.Height,textLayout.Height+4);if(scanLine)InlineText.VerticalContentAlignment=VerticalAlignment.Top;InlineText.TextWrapping=scanLine&&InlineText.Text==_inlineInitialText&&!InlineText.Text.Contains('\n')?TextWrapping.NoWrap:TextWrapping.Wrap;InlineText.IsUndoEnabled=false;}else{InlineText.MaxLength=20000;InlineText.IsUndoEnabled=true;}
        InlineText.MaxHeight=100000;
    }
    private void EndInlineText(bool commit)
    {
        if(_endingInline||_inlineId==null)return;_endingInline=true;
        try{
            if(commit&&_imeComposing&&_activeComposition is {} composition)TextCompositionManager.CompleteComposition(composition);
            var id=_inlineId;var text=InlineText.Text;_inlineId=null;_imeComposing=false;_activeComposition=null;InlineText.Visibility=Visibility.Collapsed;Surface.InlineEditingId=null;
            if(_session is {} session){if(commit&&text!=_inlineInitialText&&session.Document.Annotations.FirstOrDefault(a=>a.Id==id) is {} target){UpdateInlineValue(target,text);if(target.Kind==AnnotationKind.Text&&string.IsNullOrWhiteSpace(text))session.Document.Annotations.Remove(target);else if(target.Mockup==null)EditorRenderer.FitText(target,Surface.EditingBounds,true);Surface.FitSelectionCanvas();session.Commit();}else session.Cancel();}
            if(Surface.Tool is EditorTool.Text or EditorTool.Callout){Surface.Cursor=Cursors.Cross;SetStatus("Sẵn sàng · Giữ công cụ "+(Surface.Tool==EditorTool.Callout?"Callout":"Chữ")+" để vẽ tiếp.");}Surface.InvalidateVisual();SyncProperties();SavedLabel.Text=_session?.Dirty==true?"Đang lưu…":"✓ Đã lưu";if(_session?.Dirty==true){_saveTimer.Stop();_saveTimer.Start();}
        }finally{_endingInline=false;}
    }
    private void InlineText_Changed(object sender,TextChangedEventArgs e)
    {
        if(_syncInline||_endingInline||_imeComposing||_inlineId==null||_session?.Document.Annotations.FirstOrDefault(a=>a.Id==_inlineId) is not {} a)return;
        _session.Begin();UpdateInlineValue(a,InlineText.Text);SavedLabel.Text="Đang nhập…";
        if(_inlineRefreshQueued)return;_inlineRefreshQueued=true;
        Dispatcher.BeginInvoke(()=>{_inlineRefreshQueued=false;if(_inlineId==null||_imeComposing||_endingInline||_session?.Document.Annotations.FirstOrDefault(x=>x.Id==_inlineId) is not {} target)return;UpdateInlineValue(target,InlineText.Text);double width=target.Width,height=target.Height;if(target.Mockup==null)EditorRenderer.FitText(target,Surface.EditingBounds,true);if(target.Width!=width||target.Height!=height)Surface.FitSelectionCanvas();Surface.Refresh();},DispatcherPriority.Render);
    }
    private void InlineText_KeyDown(object sender,KeyEventArgs e)
    {
        if(_imeComposing&&e.Key is Key.Enter or Key.Escape)return;
        if(e.Key==Key.Escape){EndInlineText(false);Surface.Focus();e.Handled=true;}
        else if(e.Key==Key.Enter&&Keyboard.Modifiers.HasFlag(ModifierKeys.Control)){EndInlineText(true);Surface.Focus();e.Handled=true;}
    }
    private void InlineText_LostFocus(object sender,KeyboardFocusChangedEventArgs e){if(!InlineText.IsKeyboardFocusWithin)EndInlineText(true);}
    private void EditOnImage_Click(object sender,RoutedEventArgs e){if(!RequireLicense())return;BeginInlineText();}
    private void SyncAdvancedAnnotation()=>SyncConnectorTools();
    private void ObjectCommand(string command)
    {if(!RequireLicense())return;
        if(_session==null||Surface.Tool==EditorTool.Crop||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds))return;
        EndInlineText(true);Surface.CancelInteraction();_session.Commit();var ids=Surface.SelectedIds.ToHashSet();
        try{
            if(command=="Duplicate"){
                HashSet<string> copies=[];_session.Change(d=>copies=AnnotationEditing.Duplicate(d,ids,EditorRenderer.Box,Surface.EditingBounds));Surface.SelectIds(copies);
            }else if(command=="Group"){
                if(AnnotationEditing.Units(_session.Document,ids).Length<2)return;
                _session.Change(d=>AnnotationEditing.Group(d,ids));Surface.SelectIds(ids);
            }else if(command=="Ungroup"){
                _session.Change(d=>AnnotationEditing.Ungroup(d,ids));Surface.SelectIds(ids);
            }else if(command is "Horizontal" or "Vertical"){
                bool changed=false;_session.Change(d=>{changed=ObjectPrecision.Distribute(d,ids,command=="Horizontal",EditorRenderer.GeometryBox);});
                if(!changed)SetStatus("Chọn ít nhất 3 đối tượng/nhóm và để đủ khoảng trống giữa hai đầu để chia đều.");
            }else{
                _session.Change(d=>AnnotationEditing.Reorder(d,ids,Enum.Parse<AnnotationOrder>(command)));
                if(_session.Document.Annotations.Any(a=>ids.Contains(a.Id)&&a.IsRedaction))SetStatus("Đã sắp xếp trong lớp bảo vệ; vùng che luôn phủ trên chú thích.");
            }
            Surface.Refresh();SyncProperties();Surface.Focus();
        }catch(Exception ex){SetStatus("Chưa thực hiện được: "+ex.Message);}
    }
    private void ObjectCommand_Click(object sender,RoutedEventArgs e){if(!RequireLicense())return;if(sender is Button {Tag:string command})ObjectCommand(command);}
}
