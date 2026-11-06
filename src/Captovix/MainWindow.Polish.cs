using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace Captovix;
public partial class MainWindow
{
    private void TextColorPaint_Click(object sender,RoutedEventArgs e){if(!RequireLicense())return;OpenPaint(TextColorPaint,color=>{if(color==null)return;EndInlineText(true);var kind=CurrentTextKind();Surface.TextDefaultsFor(kind).TextColor=color;ChangeSelected(a=>{if(TextStudio.IsText(a.Kind))a.TextColor=color;});Surface.Refresh();SyncProperties();},false);}
    private bool TryTextShortcut(KeyEventArgs e)
    {
        if(_imeComposing&&_inlineId!=null)return false;
        if(Keyboard.Modifiers!=ModifierKeys.Control||e.Key is not (Key.B or Key.I or Key.U)||_session==null||_busy)return false;
        // Formatting applies to the whole object, including while its inline editor is active.
        bool inline=InlineText.IsKeyboardFocusWithin;int caret=InlineText.SelectionStart,length=InlineText.SelectionLength;bool textFocus=inline||AnnotationText.IsKeyboardFocusWithin||MockupInspectorHost.IsKeyboardFocusWithin;
        if(Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase&&!textFocus)return false;
        if(Keyboard.FocusedElement is ComboBox or ComboBoxItem)return false;
        if(MockupInspectorHost.IsKeyboardFocusWithin&&_mockupInspector?.Flush()==false){e.Handled=true;return true;}
        var a=Surface.Selected;if(a==null||!(TextStudio.IsText(a.Kind)||a.Mockup is {Type:MockupKind.Label or MockupKind.Title}||a.Mockup!=null&&a.Text.Length>0)||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds))return false;
        if(textFocus)EndInlineText(true);a=Surface.Selected;if(a==null)return false;
        bool value=e.Key==Key.B?!a.Bold:e.Key==Key.I?!a.Italic:!a.Underline;
        ChangeSelected(item=>{if(!(TextStudio.IsText(item.Kind)||item.Mockup is {Type:MockupKind.Label or MockupKind.Title}||item.Mockup!=null&&item.Text.Length>0))return;if(e.Key==Key.B)item.Bold=value;else if(e.Key==Key.I)item.Italic=value;else item.Underline=value;if(item.Mockup==null)EditorRenderer.FitText(item,Surface.EditingBounds,true);});Surface.Refresh();SyncProperties();if(inline){BeginInlineText();Dispatcher.BeginInvoke(()=>InlineText.Select(Math.Min(caret,InlineText.Text.Length),Math.Min(length,InlineText.Text.Length-Math.Min(caret,InlineText.Text.Length))));}else if(!textFocus)Surface.Focus();e.Handled=true;return true;
    }
    private void InitializeIcons()
    {
        foreach(var button in Descendants(this).OfType<Button>()){
            if(button.Content is not string text)continue;string? key=button.Tag?.ToString();string label=text.Trim();
            if(key!=null&&Enum.TryParse<EditorTool>(key,out _))label=key switch{"Select"=>"Chọn","Arrow"=>"Mũi tên","Shape"=>"Khung","Text"=>"Chữ","Magnifier"=>"Phóng đại","Spotlight"=>"Spotlight","Highlight"=>"Highlight","Step"=>"Bước","Blur"=>"Làm mờ","Callout"=>"Callout","Freehand"=>"Vẽ tự do","Crop"=>"Crop","PixelRectangle"=>"Chọn vùng","PixelEllipse"=>"Ellipse","PixelLasso"=>"Lasso",_=>label};
            else{key=button.Name switch{"SettingsShortcut"=>"Settings","StudioMoreButton"=>"More","RecentStudioButton"=>"Stamp","ExtendCanvasButton"=>"Expand","LibraryMoreButton"=>"More","ArchiveButton"=>"Folder","BatchButton"=>"Export","LibraryManagerButton"=>"Layers","AssistButton"=>"Ocr","LibrarySearchButton"=>"Search","MediaLocationButton"=>"SaveFolder","OcrButton"=>"Ocr","CaptureButton"=>"Camera","RecordButton"=>"Record","UndoButton"=>"Undo","RedoButton"=>"Redo","CopyButton"=>"Copy","ExportButton"=>"Export","RestoreMediaButton"=>"Undo","UpdateFooterButton"=>"Refresh",_=>null};
                if(key==null){(key,label)=text switch{"⌫"=>("Delete",""),"▱"=>("Folder","Mở thư mục media"),"⇄"=>("Layout","Thư viện dọc / ngang"),"Mở file"=>("Folder","Mở file"),"↻"=>("Refresh",""),_=>((string?)null,label)};}
                else label=button.Name switch{"LibrarySearchButton" or "ArchiveButton" or "LibraryMoreButton"=>"","MediaLocationButton"=>"Thư mục lưu media","LibraryManagerButton"=>"Quản lý thư viện","BatchButton"=>"Xuất nhiều / PDF","RestoreMediaButton"=>"Khôi phục lần xóa","AssistButton"=>"Nhận chữ","CaptureButton"=>"Chụp","RecordButton"=>"Quay","UndoButton" or "RedoButton"=>"","UpdateFooterButton"=>"Cập nhật",_=>label};
            }
            if(key==null)continue;button.Content=AppIcons.Content(key,label);button.SetValue(System.Windows.Automation.AutomationProperties.NameProperty,string.IsNullOrEmpty(label)?button.ToolTip?.ToString()??key:label);
        }
        foreach (var tool in ToolBox.Children.OfType<Button>())
            tool.SetResourceReference(StyleProperty, "ToolbarButton");
        CaptureButton.SetResourceReference(Control.BackgroundProperty,"Accent");CaptureButton.SetResourceReference(Control.ForegroundProperty,"OnAccent");
        RecordButton.SetResourceReference(Control.BackgroundProperty,"RecordBackground");RecordButton.SetResourceReference(Control.ForegroundProperty,"RecordText");RecordButton.SetResourceReference(Control.BorderBrushProperty,"RecordBackground");_libraryLabels.Clear();ApplyLibraryLayout();
    }
}
