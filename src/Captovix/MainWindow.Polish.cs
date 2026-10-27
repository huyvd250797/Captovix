using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace Captovix;
public partial class MainWindow
{
    private void TextColorPaint_Click(object sender,RoutedEventArgs e)=>OpenPaint(TextColorPaint,color=>{if(color==null)return;EndInlineText(true);var kind=CurrentTextKind();Surface.TextDefaultsFor(kind).TextColor=color;ChangeSelected(a=>{if(TextStudio.IsText(a.Kind))a.TextColor=color;});Surface.Refresh();SyncProperties();},false);
    private bool TryTextShortcut(KeyEventArgs e)
    {
        if(_imeComposing&&_inlineId!=null)return false;
        if(Keyboard.Modifiers!=ModifierKeys.Control||e.Key is not (Key.B or Key.I or Key.U)||_session==null||_busy)return false;
        // Formatting applies to the whole object, including while its inline editor is active.
        bool inline=InlineText.IsKeyboardFocusWithin;int caret=InlineText.SelectionStart,length=InlineText.SelectionLength;bool textFocus=inline||AnnotationText.IsKeyboardFocusWithin;
        if(Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase&&!textFocus)return false;
        if(Keyboard.FocusedElement is ComboBox or ComboBoxItem)return false;
        var a=Surface.Selected;if(a==null||!TextStudio.IsText(a.Kind)||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds))return false;
        if(textFocus)EndInlineText(true);
        bool value=e.Key==Key.B?!a.Bold:e.Key==Key.I?!a.Italic:!a.Underline;
        ChangeSelected(item=>{if(!TextStudio.IsText(item.Kind))return;if(e.Key==Key.B)item.Bold=value;else if(e.Key==Key.I)item.Italic=value;else item.Underline=value;EditorRenderer.FitText(item,Surface.EditingBounds,true);});Surface.Refresh();SyncProperties();if(inline){BeginInlineText();Dispatcher.BeginInvoke(()=>InlineText.Select(Math.Min(caret,InlineText.Text.Length),Math.Min(length,InlineText.Text.Length-Math.Min(caret,InlineText.Text.Length))));}else if(!textFocus)Surface.Focus();e.Handled=true;return true;
    }
    private void InitializeIcons()
    {
        foreach(var button in Descendants(this).OfType<Button>()){
            if(button.Content is not string text)continue;string? key=button.Tag?.ToString();string label=text.Trim();
            if(key!=null&&Enum.TryParse<EditorTool>(key,out _))label=key switch{"Select"=>"Chọn","Arrow"=>"Mũi tên","Shape"=>"Khung","Text"=>"Chữ","Magnifier"=>"Phóng đại","Spotlight"=>"Spotlight","Highlight"=>"Highlight","Step"=>"Bước","Blur"=>"Làm mờ","Callout"=>"Callout","Freehand"=>"Vẽ tự do","Crop"=>"Crop","PixelRectangle"=>"Chọn vùng","PixelEllipse"=>"Ellipse","PixelLasso"=>"Lasso",_=>label};
            else{key=button.Name switch{"ExtendCanvasButton"=>"Expand","LibraryMoreButton"=>"More","ArchiveButton"=>"Folder","BatchButton"=>"Export","LibraryManagerButton"=>"Layers","AssistButton"=>"Ocr","LibrarySearchButton"=>"Search","MediaLocationButton"=>"SaveFolder","OcrButton"=>"Ocr","CaptureButton"=>"Camera","RecordButton"=>"Record","UndoButton"=>"Undo","RedoButton"=>"Redo","CopyButton"=>"Copy","ExportButton"=>"Export","RestoreMediaButton"=>"Undo","UpdateFooterButton"=>"Refresh",_=>null};
                if(key==null){(key,label)=text switch{"⌫"=>("Delete",""),"▱"=>("Folder","Mở thư mục media"),"⇄"=>("Layout","Thư viện dọc / ngang"),"Mở file"=>("Folder","Mở file"),"↻"=>("Refresh",""),_=>((string?)null,label)};}
                else label=button.Name switch{"LibrarySearchButton" or "ArchiveButton" or "LibraryMoreButton"=>"","MediaLocationButton"=>"Thư mục lưu media","LibraryManagerButton"=>"Quản lý thư viện","BatchButton"=>"Xuất nhiều / PDF","RestoreMediaButton"=>"Khôi phục lần xóa","AssistButton"=>"OCR · nhận chữ","CaptureButton"=>"Chụp ảnh","RecordButton"=>"Quay màn hình","UndoButton" or "RedoButton"=>"","UpdateFooterButton"=>"Cập nhật",_=>label};
            }
            if(key==null)continue;button.Content=AppIcons.Content(key,label);button.SetValue(System.Windows.Automation.AutomationProperties.NameProperty,string.IsNullOrEmpty(label)?button.ToolTip?.ToString()??key:label);
        }
        CaptureButton.SetResourceReference(Control.BackgroundProperty,"Accent");CaptureButton.SetResourceReference(Control.ForegroundProperty,"OnAccent");
        RecordButton.SetResourceReference(Control.BackgroundProperty,"RecordBackground");RecordButton.SetResourceReference(Control.ForegroundProperty,"RecordText");RecordButton.SetResourceReference(Control.BorderBrushProperty,"RecordText");_libraryLabels.Clear();ApplyLibraryLayout();
    }
}
public static class AppIcons
{
    private static readonly Dictionary<string,string> Paths=new(){
        ["Element"]="M3,8 V3 H8 M16,3 H21 V8 M3,16 V21 H8 M16,21 H21 V16 M8,8 H16 V16 H8 Z",
        ["ScrollDown"]="M12,3 V20 M6,14 L12,20 L18,14",["ScrollRight"]="M3,12 H20 M14,6 L20,12 L14,18",
        ["Library"]="M3,4 H8 V20 H3 Z M11,4 H16 V20 H11 Z M19,5 L22,19",
        ["LibraryHorizontal"]="M3,3 H21 V21 H3 Z M3,15 H21 M8,15 V21 M14,15 V21",
        ["LibraryVertical"]="M3,3 H21 V21 H3 Z M9,3 V21 M3,9 H9 M3,15 H9",
        ["Star"]="M12,2 L15,8 L22,9 L17,14 L18,21 L12,18 L6,21 L7,14 L2,9 L9,8 Z",["More"]="M5,11 A1,1 0 1 1 4.99,11 M12,11 A1,1 0 1 1 11.99,11 M19,11 A1,1 0 1 1 18.99,11",["Restore"]="M7,7 H20 V20 H7 Z M4,16 H3 V3 H16 V4",["Expand"]="M9,3 H3 V9 M15,3 H21 V9 M3,15 V21 H9 M15,21 H21 V15",["Close"]="M6,6 L18,18 M18,6 L6,18",["Minimize"]="M4,17 L20,17",["Maximize"]="M4,4 L20,4 L20,20 L4,20 Z",
        ["Ocr"]="M3,8 L3,3 L8,3 M16,3 L21,3 L21,8 M3,16 L3,21 L8,21 M16,21 L21,21 L21,16 M8,8 L16,8 M8,12 L16,12 M8,16 L13,16",
        ["Assist"]="M4,20 L17,7 L20,10 L7,23 Z M4,3 L4,7 M2,5 L6,5 M15,1 L15,5 M13,3 L17,3 M21,15 L21,19 M19,17 L23,17",
        ["Search"]="M16,16 L22,22 M10,3 A7,7 0 1 1 9.99,3",["SaveFolder"]="M3,7 L3,21 L21,21 L21,7 L11,7 L8,4 L3,4 Z M12,10 L12,18 M8,14 L12,18 L16,14",
        ["Layers"]="M2,7 L12,2 L22,7 L12,12 Z M2,12 L12,17 L22,12 M2,17 L12,22 L22,17",["Align"]="M3,3 L3,21 M7,5 L20,5 L20,10 L7,10 Z M7,14 L15,14 L15,19 L7,19 Z",
        ["Paste"]="M8,4 L5,4 L5,22 L21,22 L21,4 L18,4 M9,2 L17,2 L17,7 L9,7 Z",["Duplicate"]="M3,3 L16,3 L16,5 M3,3 L3,16 L5,16 M8,8 L21,8 L21,21 L8,21 Z M11,14 L18,14 M14.5,11 L14.5,18",
        ["Group"]="M3,3 L9,3 L9,9 L3,9 Z M15,15 L21,15 L21,21 L15,21 Z M12,4 L21,4 L21,12 M4,12 L4,21 L12,21",["Ungroup"]="M3,3 L9,3 L9,9 L3,9 Z M15,15 L21,15 L21,21 L15,21 Z M12,4 L16,4 M21,8 L21,12 M4,12 L4,16 M8,21 L12,21",
        ["Settings"]="M4,6 L20,6 M4,12 L20,12 M4,18 L20,18 M9,3 L9,9 M16,9 L16,15 M8,15 L8,21",
        ["Lock"]="M6,10 L18,10 L18,21 L6,21 Z M8,10 L8,7 A4,4 0 0 1 16,7 L16,10",["Unlock"]="M6,10 L18,10 L18,21 L6,21 Z M8,10 L8,7 A4,4 0 0 1 16,7",["Eye"]="M2,12 Q12,0 22,12 Q12,24 2,12 Z M12,8 A4,4 0 1 1 11.99,8",["EyeOff"]="M2,12 Q12,0 22,12 Q12,24 2,12 Z M3,3 L21,21",["PixelEllipse"]="M12,3 A9,9 0 1 1 11.99,3",["PixelLasso"]="M12,3 C24,3 24,16 11,17 C1,18 -2,3 12,3 M11,17 C5,24 22,23 19,17",
        ["Select"]="M4,3 L18,12 L12,13 L9,20 Z",["Arrow"]="M4,20 L20,4 M10,4 L20,4 L20,14",["Shape"]="M7,5 H17 Q20,5 20,8 V16 Q20,19 17,19 H7 Q4,19 4,16 V8 Q4,5 7,5 Z",["Text"]="M5,5 L19,5 M12,5 L12,20 M8,20 L16,20",["Magnifier"]="M18,18 L22,22 M10,4 A6,6 0 1 1 9.99,4 M7,10 L13,10 M10,7 L10,13",["Spotlight"]="M12,3 A9,9 0 1 1 11.99,3 M12,8 A4,4 0 1 1 11.99,8",["Highlight"]="M5,15 L14,4 L20,9 L11,19 Z M3,21 L13,21",["Step"]="M12,3 A9,9 0 1 1 11.99,3 M10,8 L12,6 L12,18 M9,18 L15,18",["Blur"]="M4,5 L20,5 M6,9 L18,9 M4,13 L20,13 M6,17 L18,17 M9,21 L15,21",["Callout"]="M5,4 L19,4 Q22,4 22,7 L22,14 Q22,17 19,17 L10,17 L5,22 L5,17 Q2,17 2,14 L2,7 Q2,4 5,4 Z",["Freehand"]="M3,17 C8,1 5,23 12,9 C18,0 12,25 21,12",["Crop"]="M6,2 L6,18 L22,18 M2,6 L18,6 L18,22",["PixelRectangle"]="M9,3 L3,3 L3,9 M15,3 L21,3 L21,9 M3,15 L3,21 L9,21 M15,21 L21,21 L21,15",["Camera"]="M3,7 L7,7 L9,4 L15,4 L17,7 L21,7 L21,21 L3,21 Z M12,10 A4,4 0 1 1 11.99,10",["Record"]="M12,4 A8,8 0 1 1 11.99,4 M12,8 A4,4 0 1 1 11.99,8",["Undo"]="M4,10 L10,4 M4,10 L10,16 M4,10 L15,10 Q22,10 20,20",["Redo"]="M20,10 L14,4 M20,10 L14,16 M20,10 L9,10 Q2,10 4,20",["Delete"]="M4,6 L20,6 M9,3 L15,3 M6,6 L7,21 L17,21 L18,6 M10,10 L10,17 M14,10 L14,17",["Folder"]="M3,7 L3,21 L21,21 L21,7 L11,7 L8,4 L3,4 Z",["Layout"]="M3,4 L21,4 L21,20 L3,20 Z M8,4 L8,20 M8,15 L21,15",["Copy"]="M8,8 L21,8 L21,21 L8,21 Z M4,16 L3,16 L3,3 L16,3 L16,4",["Export"]="M12,16 L12,3 M7,8 L12,3 L17,8 M3,14 L3,21 L21,21 L21,14",["Refresh"]="M20,9 A8,8 0 1 0 20,16 M20,3 L20,9 L14,9",["ImageLayer"]="M3,3 L21,3 L21,21 L3,21 Z M3,17 L9,11 L14,16 L17,13 L21,17 M16,6 A2,2 0 1 1 15.99,6"};
    public static FrameworkElement Content(string key,string label,double size=16)
    {
        var row=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center,Margin=new(2,1,2,1)};
        var box=new Grid{Width=size+4,Height=size+4,Margin=new(0,0,string.IsNullOrEmpty(label)?0:5,0),SnapsToDevicePixels=true};var path=new System.Windows.Shapes.Path{Data=SoftIcon.Create(Paths.GetValueOrDefault(key,Paths["Shape"])),StrokeThickness=size<14?1:1.15,StrokeLineJoin=PenLineJoin.Round,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round,Width=size,Height=size,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center};
        path.SetBinding(System.Windows.Shapes.Shape.StrokeProperty,new System.Windows.Data.Binding("Foreground"){RelativeSource=new(System.Windows.Data.RelativeSourceMode.FindAncestor,typeof(Control),1)});box.Children.Add(path);row.Children.Add(box);if(label.Length>0)row.Children.Add(new TextBlock{Text=label,VerticalAlignment=VerticalAlignment.Center});return row;
    }
}
