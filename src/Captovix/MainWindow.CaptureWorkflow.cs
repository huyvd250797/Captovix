using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Captovix;
public partial class MainWindow
{
    private CaptureWorkflowSettings _workflow=new();
    private bool _workflowReadOnly,_exitRequested,_workflowSuspended;
    private CaptureTray? _tray;
    private ContextMenu? _trayMenu;
    private CaptureHotkeyRegistry? _photoKeys;
    private readonly CaptureGate _captureGate=new();
    private readonly Dictionary<PhotoAction,(CheckBox Enabled,TextBox Key)> _keyEditors=[];
    private string? _notificationItem;
    private bool _escapeRegistered,_editingCaptureKeys;
    private static string ActionName(PhotoAction a)=>a switch{PhotoAction.Current=>"Theo cấu hình",PhotoAction.Region=>"Chọn vùng",PhotoAction.Window=>"Chọn cửa sổ",PhotoAction.Display=>"Toàn màn hình",PhotoAction.Element=>"Phần tử / modal",_=>"Chụp lại vùng"};
    private void InitializeCaptureWorkflow()
    {
        _workflow=CaptureWorkflowStore.Load(App.DataRoot,out var warning);_workflowReadOnly=warning!=null;
        var hwnd=new WindowInteropHelper(this).Handle;_photoKeys=new((id,mods,key)=>Native.RegisterHotKey(hwnd,id,mods,key),id=>Native.UnregisterHotKey(hwnd,id));
        ResumeWorkflow();RenderWorkflow();
        if(warning!=null)WorkflowStatus.Text=warning;
        if(App.StartedAutomatically&&_workflow.StartInTray&&_tray?.Ready==true)Dispatcher.BeginInvoke(()=>{Hide();_tray.Notify("Đang chạy nền. Nhấp biểu tượng để mở Editor.");},DispatcherPriority.ApplicationIdle);
    }
    private void ResumeWorkflow()
    {
        _workflowSuspended=false;var hwnd=new WindowInteropHelper(this).Handle;
        _tray?.Dispose();_tray=new(hwnd);var errors=_photoKeys!.Start(_workflow.Hotkeys);
        foreach(var id in new[]{2,3,4})if(!_hotkeys.Contains(id)&&Native.RegisterHotKey(hwnd,id,0x4006,(uint)(0x30+id)))_hotkeys.Add(id);
        if(errors.Count>0)WorkflowStatus.Text="Phím bị ứng dụng khác sử dụng: "+string.Join(", ",errors)+". Đổi phím rồi Lưu cấu hình.";
        if(_hotkeys.Count<3)WorkflowStatus.Text+=" Phím quay Ctrl+Shift+2/3/4 đang có xung đột.";
        if(!_tray.Ready)WorkflowStatus.Text+=" Windows chưa tạo được biểu tượng khay; Editor sẽ luôn được mở sau khi chụp.";
    }
    private void SuspendWorkflow()
    {
        _workflowSuspended=true;_trayMenu?.SetCurrentValue(ContextMenu.IsOpenProperty,false);_photoKeys?.Stop();ReleaseCaptureEscape();
        var hwnd=new WindowInteropHelper(this).Handle;foreach(var id in _hotkeys)Native.UnregisterHotKey(hwnd,id);_hotkeys.Clear();_tray?.Dispose();_tray=null;
    }
    private void RenderWorkflow()
    {
        AfterPhotoPicker.SelectedIndex=_workflow.Background?1:0;AutoCopyPhoto.IsChecked=_workflow.AutoCopy;CloseToTrayToggle.IsChecked=_workflow.CloseToTray;StartWindowsToggle.IsChecked=_workflow.StartWithWindows;StartTrayToggle.IsChecked=_workflow.StartInTray;
        CapturePresetPicker.ItemsSource=_workflow.Presets;CapturePresetPicker.SelectedIndex=_workflow.Presets.Count>0?0:-1;
        HotkeyRows.Children.Clear();_keyEditors.Clear();
        foreach(var h in _workflow.Hotkeys){
            var row=new StackPanel{Orientation=Orientation.Horizontal};var enabled=new CheckBox{Content=ActionName(h.Action),IsChecked=h.Enabled,Width=150};
            var key=new TextBox{Text=h.Label,Tag=h,Width=230,IsReadOnly=true,ToolTip="Nhấn tổ hợp phím mới; bỏ chọn để tắt. F12 dành riêng cho Windows."};
            key.GotKeyboardFocus+=(_,_)=>PauseKeysForEditing();key.LostKeyboardFocus+=(_,_)=>RestoreKeysAfterEditing();
            key.PreviewKeyDown+=(_,e)=>{e.Handled=true;var k=e.Key==Key.System?e.SystemKey:e.Key==Key.ImeProcessed?e.ImeProcessedKey:e.Key==Key.DeadCharProcessed?e.DeadCharProcessedKey:e.Key;if(k==Key.Tab){e.Handled=false;return;}if(k==Key.Escape)return;var vk=(uint)KeyInterop.VirtualKeyFromKey(k);if(k is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)return;
                var mods=Keyboard.Modifiers;if((mods&ModifierKeys.Windows)!=0){WorkflowStatus.Text="Không dùng phím Windows cho hotkey Captovix.";return;}uint m=((mods&ModifierKeys.Control)!=0?2u:0)|((mods&ModifierKeys.Alt)!=0?1u:0)|((mods&ModifierKeys.Shift)!=0?4u:0);if(!CaptureKeys.IsValid(m,vk)){WorkflowStatus.Text="Tổ hợp chưa hợp lệ. Dùng Ctrl/Alt + chữ, số hoặc dấu; ví dụ Ctrl + `. F12 dành riêng cho Windows.";return;}var draft=new CaptureHotkey(h.Action,true,m,vk);key.Tag=draft;key.Text=draft.Label;enabled.IsChecked=true;};
            row.Children.Add(enabled);row.Children.Add(key);HotkeyRows.Children.Add(row);_keyEditors[h.Action]=(enabled,key);
        }
        SyncCaptureBorder();
        CaptureButton.ToolTip=_workflow.Hotkeys.First(h=>h.Action==PhotoAction.Current).Label;
    }
    private void PauseKeysForEditing()
    {
        _editingCaptureKeys=true;_photoKeys?.Stop();var hwnd=new WindowInteropHelper(this).Handle;foreach(var id in _hotkeys)Native.UnregisterHotKey(hwnd,id);_hotkeys.Clear();
    }
    private void RestoreKeysAfterEditing()
    {
        if(!_editingCaptureKeys)return;_editingCaptureKeys=false;if(_photoKeys==null||_workflowSuspended)return;var errors=_photoKeys.Start(_workflow.Hotkeys);var hwnd=new WindowInteropHelper(this).Handle;foreach(var id in new[]{2,3,4})if(!_hotkeys.Contains(id)&&Native.RegisterHotKey(hwnd,id,0x4006,(uint)(0x30+id)))_hotkeys.Add(id);
        if(errors.Count>0)WorkflowStatus.Text="Phím tắt chưa đăng ký được: "+string.Join(", ",errors);
    }
    private void SaveWorkflow(CaptureWorkflowSettings candidate,bool applySystem=false)
    {
        if(_workflowReadOnly)throw new InvalidOperationException("Cấu hình cũ đang được bảo vệ. Dùng Khôi phục mặc định trước khi lưu.");
        CaptureWorkflowStore.Validate(candidate);
        var old=_workflow;var oldRun=applySystem?StartupRegistration.Read():null;var changed=false;
        try{
            if(applySystem){_photoKeys!.Replace(candidate.Hotkeys);changed=true;StartupRegistration.Write(candidate.StartWithWindows?CaptureWorkflowStore.StartupCommand(Environment.ProcessPath!):null);}
            CaptureWorkflowStore.Save(App.DataRoot,candidate);_workflow=candidate;
        }catch(Exception failure){
            if(changed){var recovery=new List<string>();try{StartupRegistration.Write(oldRun);}catch(Exception e){App.Log(e);recovery.Add("Chưa phục hồi được cấu hình khởi động Windows.");}try{_photoKeys!.Replace(old.Hotkeys);}catch(Exception e){App.Log(e);recovery.Add(e.Message);}if(recovery.Count>0)throw new IOException(failure.Message+" "+string.Join(" ",recovery),failure);}throw;
        }
    }
    private void SaveWorkflow_Click(object sender,RoutedEventArgs e)
    {
        if(_busy)return;try{
            var candidate=_workflow.Clone();candidate.Background=AfterPhotoPicker.SelectedIndex==1;candidate.AutoCopy=AutoCopyPhoto.IsChecked==true;candidate.CloseToTray=CloseToTrayToggle.IsChecked==true;candidate.StartWithWindows=StartWindowsToggle.IsChecked==true;candidate.StartInTray=StartTrayToggle.IsChecked==true;
            candidate.Hotkeys=_keyEditors.Select(k=>((CaptureHotkey)k.Value.Key.Tag) with{Enabled=k.Value.Enabled.IsChecked==true}).ToList();SaveWorkflow(candidate,true);RenderWorkflow();WorkflowStatus.Text="Đã lưu và đăng ký phím tắt với Windows. Phím chụp: "+_workflow.Hotkeys.First(h=>h.Action==PhotoAction.Current).Label+". Captovix cần đang chạy hoặc nằm ở khay hệ thống.";
        }catch(Exception ex){App.Log(ex);WorkflowStatus.Text=ex.Message;}
    }
    private void ResetWorkflow_Click(object sender,RoutedEventArgs e)
    {
        if(_busy)return;try{var path=Path.Combine(App.DataRoot,CaptureWorkflowStore.FileName);if(File.Exists(path))File.Copy(path,path+".backup-"+Guid.NewGuid().ToString("N"));var wasReadOnly=_workflowReadOnly;_workflowReadOnly=false;try{SaveWorkflow(new(),true);}catch{_workflowReadOnly=wasReadOnly;throw;}RenderWorkflow();WorkflowStatus.Text="Đã sao lưu cấu hình trước đó và khôi phục mặc định.";}catch(Exception ex){App.Log(ex);WorkflowStatus.Text=ex.Message;}
    }
    private void ApplyPreset(CapturePreset preset)
    {
        if(_busy)return;RefreshSources();var display=_displays.FirstOrDefault(d=>d.Device==preset.Display);
        if(preset.Mode==2&&preset.Display!=""&&display==null)throw new InvalidOperationException("Màn hình trong preset không còn kết nối. Chọn màn hình mới rồi lưu preset khác.");
        SourceMode.SelectedIndex=preset.Mode;DelayPicker.SelectedIndex=preset.Delay==5?2:preset.Delay==3?1:0;CursorToggle.IsChecked=preset.Cursor;if(display!=null)DisplayPicker.SelectedItem=display;AfterPhotoPicker.SelectedIndex=preset.Background?1:0;AutoCopyPhoto.IsChecked=preset.AutoCopy;
        WorkflowStatus.Text="Đã áp dụng preset: "+preset.Name+". Cửa sổ sẽ được chọn lại khi chụp.";
    }
    private void ApplyCapturePreset_Click(object sender,RoutedEventArgs e){try{if(CapturePresetPicker.SelectedItem is CapturePreset p)ApplyPreset(p);}catch(Exception ex){WorkflowStatus.Text=ex.Message;}}
    private void SaveCapturePreset_Click(object sender,RoutedEventArgs e)
    {
        try{var s=Settings();var candidate=_workflow.Clone();candidate.Presets.Add(new(Guid.NewGuid().ToString("N"),CapturePresetName.Text.Trim(),s.SourceMode,s.Display,s.Delay,s.Cursor,AfterPhotoPicker.SelectedIndex==1,AutoCopyPhoto.IsChecked==true));SaveWorkflow(candidate);CapturePresetPicker.ItemsSource=_workflow.Presets;CapturePresetPicker.SelectedIndex=_workflow.Presets.Count-1;CapturePresetName.Clear();WorkflowStatus.Text="Đã lưu preset mới.";}catch(Exception ex){WorkflowStatus.Text=ex.Message;}
    }
    private void DeleteCapturePreset_Click(object sender,RoutedEventArgs e){try{if(CapturePresetPicker.SelectedItem is not CapturePreset p)return;var candidate=_workflow.Clone();candidate.Presets.RemoveAll(x=>x.Id==p.Id);SaveWorkflow(candidate);CapturePresetPicker.ItemsSource=_workflow.Presets;CapturePresetPicker.SelectedIndex=0;WorkflowStatus.Text="Đã xóa preset.";}catch(Exception ex){WorkflowStatus.Text=ex.Message;}}
    private void ShowEditor(string? id=null)
    {
        if(_busy){_tray?.Notify("Đang chụp/quay. Hãy hoàn tất hoặc hủy phiên trước khi mở Editor.");return;}
        Show();if(WindowState==WindowState.Minimized)WindowState=WindowState.Maximized;Activate();
        if(id!=null&&_store.Items.FirstOrDefault(i=>i.Id==id) is {} item){SearchBox.Clear();MediaList.SelectedItem=item;MediaList.ScrollIntoView(item);}
    }
    private void RequestExit(){_exitRequested=true;if(_busy){CancelCapture();return;}Close();if(IsVisible)_exitRequested=false;}
    private void CancelCapture(){if(_fidelityProcess!=null){try{_fidelityProcess.CloseMainWindow();}catch(Exception ex){App.Log(ex);}return;}if(_recordingActive)StopRecording();else _captureCancellation?.Cancel();}
    private void RegisterCaptureEscape(){_escapeRegistered=Native.RegisterHotKey(new WindowInteropHelper(this).Handle,990,0x4000,0x1B);}
    private void ReleaseCaptureEscape(){if(_escapeRegistered)Native.UnregisterHotKey(new WindowInteropHelper(this).Handle,990);_escapeRegistered=false;}
    private void CancelCapture_Click(object sender,RoutedEventArgs e)=>CancelCapture();
    private void ElementCapture_Click(object sender,RoutedEventArgs e)=>_=CaptureAsync(false,PhotoAction.Element);
    private void RepeatCapture_Click(object sender,RoutedEventArgs e)=>_=CaptureAsync(false,PhotoAction.Repeat);
    private bool HandleWorkflowMessage(int msg,nint w,nint l)
    {
        if(msg==(int)CaptureTray.ShowEditorMessage){Dispatcher.BeginInvoke(()=>ShowEditor());return true;}
        if(msg==(int)CaptureTray.TaskbarCreated&&!_workflowSuspended){if(_tray?.Add()!=true)Dispatcher.BeginInvoke(()=>ShowEditor());return false;}
        if(msg==CaptureTray.Callback){var code=(int)((long)l&0xffff);if(code==0x7B||code==0x205)Dispatcher.BeginInvoke(OpenTrayMenu);else if(code is 0x400 or 0x401 or 0x203)Dispatcher.BeginInvoke(()=>ShowEditor());else if(code==0x405)Dispatcher.BeginInvoke(()=>ShowEditor(_notificationItem));return true;}
        if(msg==0x312){if(_editingCaptureKeys)return true;var id=(int)w;if(id==990){CancelCapture();return true;}if(Enum.IsDefined(typeof(PhotoAction),id)){if(!_keyEditors.Values.Any(v=>v.Key.IsKeyboardFocusWithin))_=CaptureAsync(false,(PhotoAction)id);return true;}}
        return false;
    }
    private void OpenTrayMenu()
    {
        if(_workflowSuspended)return;_trayMenu?.SetCurrentValue(ContextMenu.IsOpenProperty,false);var menu=new ContextMenu();_trayMenu=menu;
        void Add(string title,Action action,bool enabled=true){var item=new MenuItem{Header=title,IsEnabled=enabled};item.Click+=(_,_)=>{menu.IsOpen=false;Dispatcher.BeginInvoke(action);};menu.Items.Add(item);}
        Add("Mở Editor",()=>ShowEditor(),!_busy);
        foreach(var action in Enum.GetValues<PhotoAction>())Add(ActionName(action),()=>_=CaptureAsync(false,action),!_busy&&!_updateApplying);
        var presets=new MenuItem{Header="Preset chụp",IsEnabled=!_busy&&!_updateApplying};foreach(var preset in _workflow.Presets){var item=new MenuItem{Header=preset.Name};item.Click+=(_,_)=>{menu.IsOpen=false;Dispatcher.BeginInvoke(()=>{try{ApplyPreset(preset);_=CaptureAsync(false);}catch(Exception e){_tray?.Notify(e.Message);}});};presets.Items.Add(item);}menu.Items.Add(presets);
        if(_busy)Add(_fidelityProcess!=null?"Hủy kiểm chứng SDR":_recordingActive?"Dừng và lưu video":"Hủy phiên chụp",CancelCapture);
        menu.Items.Add(new Separator());Add("Thoát Captovix",RequestExit);_tray?.Foreground();menu.IsOpen=true;
    }
    private IReadOnlyList<WorkflowDisplay> WorkflowDisplays()=>_displays.Select(d=>new WorkflowDisplay(d.Device,d.Bounds,d.DpiX,d.DpiY,d.Identity)).ToArray();
}
