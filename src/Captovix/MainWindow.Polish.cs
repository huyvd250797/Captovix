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
        if(Keyboard.Modifiers!=ModifierKeys.Control||e.Key is not (Key.B or Key.I or Key.U)||_session==null||_busy)return false;
        // Formatting applies to the whole object, including while its inline editor is active.
        bool inline=InlineText.IsKeyboardFocusWithin;int caret=InlineText.SelectionStart,length=InlineText.SelectionLength;bool textFocus=inline||AnnotationText.IsKeyboardFocusWithin;
        if(Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase&&!textFocus)return false;
        if(Keyboard.FocusedElement is ComboBox or ComboBoxItem)return false;
        var a=Surface.Selected;if(a==null||!TextStudio.IsText(a.Kind)||!ObjectControl.CanEdit(_session.Document,Surface.SelectedIds))return false;
        if(textFocus)EndInlineText(true);
        bool value=e.Key==Key.B?!a.Bold:e.Key==Key.I?!a.Italic:!a.Underline;
        ChangeSelected(item=>{if(!TextStudio.IsText(item.Kind))return;if(e.Key==Key.B)item.Bold=value;else if(e.Key==Key.I)item.Italic=value;else item.Underline=value;EditorRenderer.FitText(item,_session.Document.Viewport);});Surface.Refresh();SyncProperties();if(inline){BeginInlineText();Dispatcher.BeginInvoke(()=>InlineText.Select(Math.Min(caret,InlineText.Text.Length),Math.Min(length,InlineText.Text.Length-Math.Min(caret,InlineText.Text.Length))));}else if(!textFocus)Surface.Focus();e.Handled=true;return true;
    }
    private void InitializeIcons()
    {
        foreach(var button in Descendants(this).OfType<Button>()){
            if(button.Content is not string text)continue;string? key=button.Tag?.ToString();string label=text.Trim();
            if(key!=null&&Enum.TryParse<EditorTool>(key,out _))label=key switch{"Select"=>"Chọn","Arrow"=>"Mũi tên","Shape"=>"Khung","Text"=>"Chữ","Magnifier"=>"Phóng đại","Spotlight"=>"Spotlight","Highlight"=>"Highlight","Step"=>"Bước","Blur"=>"Làm mờ","Callout"=>"Callout","Freehand"=>"Vẽ tự do","Crop"=>"Crop","PixelRectangle"=>"Chọn vùng","PixelEllipse"=>"Ellipse","PixelLasso"=>"Lasso",_=>label};
            else{key=button.Name switch{"CaptureButton"=>"Camera","RecordButton"=>"Record","UndoButton"=>"Undo","RedoButton"=>"Redo","CopyButton"=>"Copy","ExportButton"=>"Export","RestoreMediaButton"=>"Undo","UpdateFooterButton"=>"Refresh",_=>null};
                if(key==null){(key,label)=text switch{"⌫"=>("Delete",""),"▱"=>("Folder",""),"⇄"=>("Layout",""),"Mở file"=>("Folder","Mở file"),"↻"=>("Refresh",""),_=>((string?)null,label)};}
                else label=button.Name switch{"CaptureButton"=>"Chụp ảnh","RecordButton"=>"Quay màn hình","UndoButton" or "RedoButton" or "RestoreMediaButton"=>"","UpdateFooterButton"=>"Cập nhật",_=>label};
            }
            if(key==null)continue;button.Content=AppIcons.Content(key,label);button.SetValue(System.Windows.Automation.AutomationProperties.NameProperty,string.IsNullOrEmpty(label)?button.ToolTip?.ToString()??key:label);
        }
        CaptureButton.Background=new SolidColorBrush(Color.FromRgb(0,139,146));CaptureButton.Foreground=Brushes.White;
        RecordButton.Background=new SolidColorBrush(Color.FromRgb(255,241,242));RecordButton.Foreground=new SolidColorBrush(Color.FromRgb(190,18,60));RecordButton.BorderBrush=new SolidColorBrush(Color.FromRgb(254,205,211));
    }
}
public static class AppIcons
{
    private static readonly Dictionary<string,string> Paths=new(){
        ["Lock"]="M6,10 L18,10 L18,21 L6,21 Z M8,10 L8,7 A4,4 0 0 1 16,7 L16,10",["Unlock"]="M6,10 L18,10 L18,21 L6,21 Z M8,10 L8,7 A4,4 0 0 1 16,7",["Eye"]="M2,12 Q12,0 22,12 Q12,24 2,12 Z M12,8 A4,4 0 1 1 11.99,8",["EyeOff"]="M2,12 Q12,0 22,12 Q12,24 2,12 Z M3,3 L21,21",["PixelEllipse"]="M12,3 A9,9 0 1 1 11.99,3",["PixelLasso"]="M12,3 C24,3 24,16 11,17 C1,18 -2,3 12,3 M11,17 C5,24 22,23 19,17",
        ["Select"]="M4,3 L18,12 L12,13 L9,20 Z",["Arrow"]="M4,20 L20,4 M10,4 L20,4 L20,14",["Shape"]="M4,5 L20,5 L20,19 L4,19 Z",["Text"]="M5,5 L19,5 M12,5 L12,20 M8,20 L16,20",["Magnifier"]="M18,18 L22,22 M10,4 A6,6 0 1 1 9.99,4 M7,10 L13,10 M10,7 L10,13",["Spotlight"]="M12,3 A9,9 0 1 1 11.99,3 M12,8 A4,4 0 1 1 11.99,8",["Highlight"]="M5,15 L14,4 L20,9 L11,19 Z M3,21 L13,21",["Step"]="M12,3 A9,9 0 1 1 11.99,3 M10,8 L12,6 L12,18 M9,18 L15,18",["Blur"]="M4,5 L20,5 M6,9 L18,9 M4,13 L20,13 M6,17 L18,17 M9,21 L15,21",["Callout"]="M5,4 L19,4 Q22,4 22,7 L22,14 Q22,17 19,17 L10,17 L5,22 L5,17 Q2,17 2,14 L2,7 Q2,4 5,4 Z",["Freehand"]="M3,17 C8,1 5,23 12,9 C18,0 12,25 21,12",["Crop"]="M6,2 L6,18 L22,18 M2,6 L18,6 L18,22",["PixelRectangle"]="M9,3 L3,3 L3,9 M15,3 L21,3 L21,9 M3,15 L3,21 L9,21 M15,21 L21,21 L21,15",["Camera"]="M3,7 L7,7 L9,4 L15,4 L17,7 L21,7 L21,21 L3,21 Z M12,10 A4,4 0 1 1 11.99,10",["Record"]="M12,4 A8,8 0 1 1 11.99,4 M12,8 A4,4 0 1 1 11.99,8",["Undo"]="M4,10 L10,4 M4,10 L10,16 M4,10 L15,10 Q22,10 20,20",["Redo"]="M20,10 L14,4 M20,10 L14,16 M20,10 L9,10 Q2,10 4,20",["Delete"]="M4,6 L20,6 M9,3 L15,3 M6,6 L7,21 L17,21 L18,6 M10,10 L10,17 M14,10 L14,17",["Folder"]="M3,7 L3,21 L21,21 L21,7 L11,7 L8,4 L3,4 Z",["Layout"]="M3,4 L21,4 L21,20 L3,20 Z M8,4 L8,20 M8,15 L21,15",["Copy"]="M8,8 L21,8 L21,21 L8,21 Z M4,16 L3,16 L3,3 L16,3 L16,4",["Export"]="M12,16 L12,3 M7,8 L12,3 L17,8 M3,14 L3,21 L21,21 L21,14",["Refresh"]="M20,9 A8,8 0 1 0 20,16 M20,3 L20,9 L14,9",["ImageLayer"]="M3,3 L21,3 L21,21 L3,21 Z M3,17 L9,11 L14,16 L17,13 L21,17 M16,6 A2,2 0 1 1 15.99,6"};
    public static FrameworkElement Content(string key,string label)
    {
        var row=new StackPanel{Orientation=Orientation.Horizontal,VerticalAlignment=VerticalAlignment.Center};var path=new System.Windows.Shapes.Path{Data=Geometry.Parse(Paths.GetValueOrDefault(key,Paths["Shape"])),StrokeThickness=1.7,StrokeLineJoin=PenLineJoin.Round,StrokeStartLineCap=PenLineCap.Round,StrokeEndLineCap=PenLineCap.Round,Width=17,Height=17,Stretch=Stretch.Uniform,Margin=new(0,string.IsNullOrEmpty(label)?0:1,string.IsNullOrEmpty(label)?0:6,0)};
        path.SetBinding(System.Windows.Shapes.Shape.StrokeProperty,new System.Windows.Data.Binding("Foreground"){RelativeSource=new(System.Windows.Data.RelativeSourceMode.FindAncestor,typeof(Control),1)});row.Children.Add(path);if(label.Length>0)row.Children.Add(new TextBlock{Text=label,VerticalAlignment=VerticalAlignment.Center});return row;
    }
}
