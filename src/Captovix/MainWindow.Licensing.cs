using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
namespace Captovix;
public partial class MainWindow
{
    readonly DispatcherTimer _licenseTimer=new(){Interval=TimeSpan.FromSeconds(1)};
    bool _licenseFull,_licenseCheckpoint,_licenseRefreshing;int _licenseTicks;Window? _licenseDialog;
    void InitializeLicensing()
    {
        LicenseRuntime.Initialize();ImageTransfer.AuthorizeCopy=()=>LicenseRuntime.Full;
        AddHandler(CommandManager.PreviewExecutedEvent,new ExecutedRoutedEventHandler((_,e)=>{if(!LicenseRuntime.Full&&e.Command is RoutedCommand cmd&&(cmd==ApplicationCommands.Copy||cmd==ApplicationCommands.Cut||cmd==ApplicationCommands.Paste||cmd==ApplicationCommands.Undo||cmd==ApplicationCommands.Redo)){e.Handled=true;}}));
        EditorSession.AuthorizeMutation=()=>_licenseCheckpoint||LicenseRuntime.Full;
        _licenseFull=true;ApplyLicenseState();_licenseTimer.Tick+=async(_,_)=>{
            if(_licenseFull!=LicenseRuntime.Full)ApplyLicenseState();
            LicenseFooterButton.Content=LicenseRuntime.Status;
            if(++_licenseTicks%60==0)LicenseRuntime.CheckpointClock();
            if(_licenseTicks%3600==0)await RefreshLicenseSilently();
        };_licenseTimer.Start();Closed+=(_,_)=>{_licenseTimer.Stop();LicenseRuntime.CheckpointClock();};
        _=RefreshLicenseSilently();
        if(!LicenseRuntime.Full)Dispatcher.BeginInvoke(()=>License_Click(this,new RoutedEventArgs()),DispatcherPriority.ContextIdle);
    }
    async Task RefreshLicenseSilently()
    {
        if(_licenseRefreshing||!LicenseRuntime.Configured||LicenseRuntime.Certificate==null)return;_licenseRefreshing=true;
        try{await LicenseRuntime.Request("refresh");}catch{ /* cached signed lease remains until its expiry; no silent extension */ }
        finally{_licenseRefreshing=false;ApplyLicenseState();}
    }
    bool RequireLicense()
    {
        if(LicenseRuntime.Full)return true;
        SetStatus("Chức năng đang khóa · "+LicenseRuntime.Status+" · Bấm trạng thái bản quyền ở thanh dưới để kích hoạt.");return false;
    }
    // Saving/checkpointing existing work is always permitted; UI mutation commands remain gated.
    private bool FlushEdits(bool report=true){var previous=_licenseCheckpoint;_licenseCheckpoint=true;try{return FlushEditsCore(report);}finally{_licenseCheckpoint=previous;}}
    void ApplyLicenseState()
    {
        bool full=LicenseRuntime.Full;
        if(_licenseFull&&!full){
            if(_studioPopup!=null)_studioPopup.IsOpen=false;if(_paintPopup!=null)_paintPopup.IsOpen=false;
            _licenseCheckpoint=true;try{if(_recordingActive)StopRecording();FlushEdits(false);Surface.CancelInteraction();}finally{_licenseCheckpoint=false;}
            foreach(Window w in OwnedWindows.Cast<Window>().ToArray())if(w!=_licenseDialog)w.Close();
        }
        _licenseFull=full;Surface.IsEnabled=ImageTools.IsEnabled=EditorProperties.IsEnabled=ZoomTools.IsEnabled=full;
        CaptureButton.ToolTip=full?"Chụp ảnh màn hình":"Chụp vùng thủ công · chỉ lưu PNG gốc";RecordButton.IsEnabled=full&&!_busy;CopyButton.IsEnabled=full&&_session!=null;ExportButton.IsEnabled=full&&Selected!=null;
        UndoButton.IsEnabled=full&&_session?.CanUndo==true;RedoButton.IsEnabled=full&&_session?.CanRedo==true;
        LicenseFooterButton.Content=LicenseRuntime.Status;LicenseFooterButton.ToolTip="Bản quyền, chuyển máy và khôi phục";
    }
    private void License_Click(object sender,RoutedEventArgs e)
    {
        if(_licenseDialog!=null){_licenseDialog.Activate();return;}
        var dialog=new Window{Owner=this,Title="Captovix · Bản quyền & khôi phục",Width=650,Height=650,MinWidth=430,MinHeight=400,WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false};_licenseDialog=dialog;
        var panel=new StackPanel{Margin=new(24)};dialog.Content=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        var status=new TextBlock{FontSize=21,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap};panel.Children.Add(status);
        var detail=new TextBlock{Margin=new(0,12,0,14),TextWrapping=TextWrapping.Wrap};panel.Children.Add(detail);
        void Label(string value)=>panel.Children.Add(new TextBlock{Text=value,Margin=new(0,12,0,4),FontWeight=FontWeights.SemiBold});
        Label("Key bản quyền vĩnh viễn");var key=new TextBox{MinHeight=34,MaxLength=128};panel.Children.Add(key);
        Label("Mã khôi phục · dùng khi không còn truy cập máy cũ");var recovery=new PasswordBox{MinHeight=34,MaxLength=128,Padding=new(8)};recovery.SetResourceReference(Control.BackgroundProperty,"Surface");recovery.SetResourceReference(Control.ForegroundProperty,"Text");recovery.SetResourceReference(Control.BorderBrushProperty,"Line");panel.Children.Add(recovery);
        var note=new TextBlock{Margin=new(0,10,0,10),TextWrapping=TextWrapping.Wrap,Text="Trả bản quyền trên máy cũ trước khi kích hoạt máy mới. Nếu mất máy, dùng key + mã khôi phục: mọi lượt kích hoạt cũ sẽ bị trả và mã khôi phục được đổi. Chỉ gửi tên máy, định danh máy dạng băm, khóa công khai và yêu cầu bản quyền; không gửi ảnh."};panel.Children.Add(note);
        var actions=new WrapPanel();panel.Children.Add(actions);var buttons=new List<Button>();
        var result=new TextBlock{Margin=new(0,12,0,6),TextWrapping=TextWrapping.Wrap};var secret=new TextBox{IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Visibility=Visibility.Collapsed,MinHeight=50};
        void Refresh(){status.SetResourceReference(TextBlock.ForegroundProperty,LicenseRuntime.Full?"AccentText":"Warning");status.Text=LicenseRuntime.Status;var c=LicenseRuntime.Certificate;detail.Text=(c==null?"Chọn dùng thử 30 ngày hoặc nhập key để mở đủ công cụ.":$"Loại: {(c.Kind=="trial"?"Dùng thử":"Vĩnh viễn")}\nKích hoạt: {c.ActivationId}\nXác minh: {c.IssuedAt.ToLocalTime():dd/MM/yyyy HH:mm}\nCần kết nối lại trước: {c.LeaseUntil.ToLocalTime():dd/MM/yyyy HH:mm}"+(c.TrialUntil is {} end?$"\nHết dùng thử: {end.ToLocalTime():dd/MM/yyyy HH:mm}":""))+"\nSau khi hết quyền: chỉ kéo vùng và Enter để lưu PNG gốc; chỉnh sửa và copy/paste bị khóa.\n"+LicenseRuntime.LastError;}
        void Add(string text,string operation){var b=new Button{Content=text,Margin=new(0,3,6,3),Padding=new(10,7,10,7)};buttons.Add(b);actions.Children.Add(b);b.Click+=async(_,_)=>{
            if(operation is "release" or "recover"){
                var prompt=operation=="release"?"Trả bản quyền máy này để dùng trên máy khác? Công cụ chỉnh sửa sẽ khóa sau khi trả thành công.":"Khôi phục sẽ trả TẤT CẢ các máy cũ và đổi mã khôi phục. Tiếp tục?";
                if(MessageBox.Show(dialog,prompt,"Xác nhận bản quyền",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;
            }
            foreach(var button in buttons)button.IsEnabled=false;result.Text="Đang xác minh với máy chủ…";
            try{var code=await LicenseRuntime.Request(operation,key.Text,recovery.Password);result.Text="Đã xử lý yêu cầu. "+LicenseRuntime.Status;if(code!=null){secret.Text=code;secret.Visibility=Visibility.Visible;result.Text+="\nLưu mã khôi phục MỚI bên dưới ở nơi an toàn; mã cũ đã hết hiệu lực.";}recovery.Clear();ApplyLicenseState();}
            catch(Exception ex){result.Text=ex.Message;}finally{foreach(var button in buttons)button.IsEnabled=true;Refresh();}
        };}
        Add("Dùng thử 30 ngày","trial");Add("Kích hoạt key","activate");Add("Xác minh lại","refresh");Add("Trả bản quyền máy này","release");Add("Khôi phục trên máy này","recover");panel.Children.Add(result);panel.Children.Add(secret);
        var info=new TextBlock{Text="Dữ liệu ảnh và chú thích vẫn được giữ khi hết dùng thử hoặc chuyển máy. Bản quyền vĩnh viễn không hết hạn trả phí; chứng nhận ngoại tuyến cần được làm mới định kỳ.",TextWrapping=TextWrapping.Wrap,Margin=new(0,14,0,0)};panel.Children.Add(info);
        var support=new TextBox{Text="Mã máy: "+LicenseRuntime.DeviceId+"\nDấu khóa thiết bị: "+LicenseRuntime.DevicePublicHash,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Margin=new(0,12,0,0)};panel.Children.Add(support);
        Refresh();dialog.Closed+=(_,_)=>{_licenseDialog=null;ApplyLicenseState();};dialog.ShowDialog();
    }
}
