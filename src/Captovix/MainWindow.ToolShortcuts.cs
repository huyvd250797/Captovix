using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
namespace Captovix;
public partial class MainWindow
{
    readonly Dictionary<string,(CheckBox Enabled,TextBox Key)> _toolKeyEditors=[];
    List<EditorShortcut> _editorShortcuts=[];
    void RenderToolShortcuts()
    {
        _editorShortcuts=ToolShortcutCatalog.Merge(_workflow.EditorHotkeys,_workflow.Hotkeys);ToolHotkeyRows.Children.Clear();_toolKeyEditors.Clear();
        foreach(var t in ToolShortcutCatalog.All){var binding=_editorShortcuts.First(s=>s.Command==t.Command);var row=new Grid{Margin=new(0,3,0,3)};row.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});row.ColumnDefinitions.Add(new(){Width=new GridLength(215)});var enabled=new CheckBox{Content=t.Label,IsChecked=binding.Enabled,VerticalAlignment=VerticalAlignment.Center};var key=new TextBox{IsReadOnly=true,Text=binding.Key==0?"Chưa đặt":binding.Label,Tag=binding,ToolTip="Bấm ô rồi nhấn phím. Bỏ chọn để tắt."};Grid.SetColumn(key,1);row.Children.Add(enabled);row.Children.Add(key);ToolHotkeyRows.Children.Add(row);_toolKeyEditors[t.Command]=(enabled,key);key.GotKeyboardFocus+=(_,_)=>PauseKeysForEditing();key.LostKeyboardFocus+=(_,_)=>RestoreKeysAfterEditing();
            void Capture(KeyEventArgs e){e.Handled=true;var k=e.Key==Key.System?e.SystemKey:e.Key==Key.ImeProcessed?e.ImeProcessedKey:e.Key;uint vk=(uint)KeyInterop.VirtualKeyFromKey(k);if(!CaptureKeys.IsSupported(vk))return;var mods=Keyboard.Modifiers;uint m=((mods&ModifierKeys.Control)!=0?2u:0)|((mods&ModifierKeys.Alt)!=0?1u:0)|((mods&ModifierKeys.Shift)!=0?4u:0)|((mods&ModifierKeys.Windows)!=0?8u:0);var draft=new EditorShortcut(t.Command,true,m,vk);key.Tag=draft;key.Text=draft.Label;enabled.IsChecked=true;}
            key.PreviewKeyDown+=(_,e)=>Capture(e);key.PreviewKeyUp+=(_,e)=>{if(e.Key==Key.Snapshot)Capture(e);};
        }
        foreach(var button in ToolBox.Children.OfType<Button>()){if(button.Tag is string name){string command=name.StartsWith("Pixel")?"Pixel:"+name:"Tool:"+name;var tool=ToolShortcutCatalog.All.FirstOrDefault(t=>t.Command==command);if(tool!=null)button.ToolTip=tool.Label+" · "+ToolShortcutLabel(command);}}AssistButton.ToolTip="Nhận chữ · "+ToolShortcutLabel("Assist");RenderRecentStudio();
    }
    List<EditorShortcut> ReadToolShortcuts()=>_toolKeyEditors.Select(p=>((EditorShortcut)p.Value.Key.Tag) with{Enabled=p.Value.Enabled.IsChecked==true}).ToList();
    private string ToolShortcutLabel(string command)=>_editorShortcuts.FirstOrDefault(s=>s.Command==command&&s.Enabled)?.Label??"";
    bool TryToolShortcut(KeyEventArgs e,bool released=false)
    {
        if(!LicenseRuntime.Full||_session==null||_busy||Surface.IsMouseCaptured||_settingsDialog!=null||Keyboard.FocusedElement is TextBoxBase or PasswordBox or ComboBox or ComboBoxItem)return false;
        var key=e.Key==Key.System?e.SystemKey:e.Key;if(key==Key.Snapshot&&!released)return false;var mods=Keyboard.Modifiers;uint m=((mods&ModifierKeys.Control)!=0?2u:0)|((mods&ModifierKeys.Alt)!=0?1u:0)|((mods&ModifierKeys.Shift)!=0?4u:0)|((mods&ModifierKeys.Windows)!=0?8u:0);uint vk=(uint)KeyInterop.VirtualKeyFromKey(key);var match=_editorShortcuts.FirstOrDefault(s=>s.Enabled&&s.Modifiers==m&&s.Key==vk);if(match==null)return false;if(e.IsRepeat){e.Handled=true;return true;}
        if(match.Command.StartsWith("Studio:"))RunStudioCommand(match.Command[7..]);else if(match.Command.StartsWith("Tool:")){SelectTool(Enum.Parse<EditorTool>(match.Command[5..]));Surface.Focus();}else if(match.Command.StartsWith("Pixel:"))PixelTool_Click(new Button{Tag=match.Command[6..]},new());else if(match.Command=="Assist")Assist_Click(this,new());e.Handled=true;return true;
    }
}
