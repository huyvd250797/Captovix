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
            RenderLicenseIndicator();
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
        RenderLicenseIndicator();LicenseFooterButton.ToolTip="Bản quyền, chuyển máy và khôi phục";
    }
    private string? _licenseIndicatorState;
    private void RenderLicenseIndicator(){string state=LicenseRuntime.Status;if(state==_licenseIndicatorState)return;_licenseIndicatorState=state;bool active=LicenseRuntime.Full,permanent=active&&LicenseRuntime.Certificate?.Kind=="perpetual";var color=permanent?System.Windows.Media.Colors.LimeGreen:active?System.Windows.Media.Colors.Goldenrod:System.Windows.Media.Colors.Gray;var row=new StackPanel{Orientation=Orientation.Horizontal};row.Children.Add(new System.Windows.Shapes.Ellipse{Width=8,Height=8,Fill=new System.Windows.Media.SolidColorBrush(color),Margin=new(4,0,8,0),VerticalAlignment=VerticalAlignment.Center,Effect=new System.Windows.Media.Effects.DropShadowEffect{Color=color,BlurRadius=10,ShadowDepth=0,Opacity=active?0.85:0.2}});row.Children.Add(new TextBlock{Text=permanent?"Đã kích hoạt":state,VerticalAlignment=VerticalAlignment.Center});LicenseFooterButton.Content=row;LicenseFooterButton.ToolTip=state;}
    private void License_Click(object sender,RoutedEventArgs e)
    {
        if(_licenseDialog!=null){_licenseDialog.Activate();return;}
        var dialog=new Window{Owner=this,Title="Captovix · Kích hoạt bản quyền",Width=480,SizeToContent=SizeToContent.Height,MaxHeight=Math.Max(320,SystemParameters.WorkArea.Height-64),ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterOwner,ShowInTaskbar=false};_licenseDialog=dialog;
        dialog.SetResourceReference(StyleProperty,typeof(Window));
        dialog.SetResourceReference(Control.BackgroundProperty,"AppBackground");dialog.SetResourceReference(Control.ForegroundProperty,"Text");
        var panel=new StackPanel{Margin=new(24)};dialog.Content=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        var heading=new StackPanel{Orientation=Orientation.Horizontal,Margin=new(0,0,0,16)};heading.Children.Add(new Image{Source=TryFindResource("BrandMark") as System.Windows.Media.ImageSource,Width=38,Height=38,Margin=new(0,0,12,0)});var titles=new StackPanel();titles.Children.Add(new TextBlock{Text="Bản quyền Captovix",FontSize=21,FontWeight=FontWeights.SemiBold});var subtitle=new TextBlock{Text="Kích hoạt để sử dụng đầy đủ công cụ.",FontSize=12,Margin=new(0,4,0,0)};subtitle.SetResourceReference(TextBlock.ForegroundProperty,"Muted");titles.Children.Add(subtitle);heading.Children.Add(titles);panel.Children.Add(heading);panel.Children.Add(new TextBlock{Text="KEY BẢN QUYỀN",FontSize=10,FontWeight=FontWeights.SemiBold,Margin=new(0,0,0,6)});
        var key=new TextBox{MinHeight=40,MaxLength=128,VerticalContentAlignment=VerticalAlignment.Center,ToolTip="Dán key bản quyền"};System.Windows.Automation.AutomationProperties.SetName(key,"Nhập KEY");panel.Children.Add(key);
        var activate=new Button{Content="Kích hoạt",MinHeight=40,Margin=new(0,10,0,4),IsDefault=true};activate.SetResourceReference(Control.BackgroundProperty,"Accent");activate.SetResourceReference(Control.ForegroundProperty,"OnAccent");panel.Children.Add(activate);
        var noKey=new Button{Content="Tôi không có KEY",BorderThickness=new(0),Background=System.Windows.Media.Brushes.Transparent,Margin=new(0,4,0,8)};noKey.SetResourceReference(Control.ForegroundProperty,"AccentText");panel.Children.Add(noKey);
        var trialPanel=new StackPanel{Margin=new(12)};var trialCard=new Border{Child=trialPanel,CornerRadius=new(8),Margin=new(0,0,0,12),Visibility=Visibility.Collapsed};trialCard.SetResourceReference(Border.BackgroundProperty,"Soft");panel.Children.Add(trialCard);
        trialPanel.Children.Add(new TextBlock{Text="Bạn chưa có KEY?",FontWeight=FontWeights.SemiBold});var trialDescription=new TextBlock{Text="Bạn có muốn dùng thử đầy đủ công cụ trong 30 ngày không?",TextWrapping=TextWrapping.Wrap,Margin=new(0,6,0,10)};trialPanel.Children.Add(trialDescription);
        var trialActions=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};trialPanel.Children.Add(trialActions);var later=new Button{Content="Để sau"};var trial=new Button{Content="Dùng thử 30 ngày"};trial.SetResourceReference(Control.BackgroundProperty,"Accent");trial.SetResourceReference(Control.ForegroundProperty,"OnAccent");trialActions.Children.Add(later);trialActions.Children.Add(trial);later.Click+=(_,_)=>trialCard.Visibility=Visibility.Collapsed;noKey.Click+=(_,_)=>trialCard.Visibility=trialCard.Visibility==Visibility.Visible?Visibility.Collapsed:Visibility.Visible;
        var result=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new(0,4,0,8)};result.SetResourceReference(TextBlock.ForegroundProperty,"AccentText");panel.Children.Add(result);
        var more=new Expander{Header="Xem thêm",IsExpanded=false,Margin=new(0,6,0,0)};panel.Children.Add(more);var advanced=new StackPanel{Margin=new(0,12,0,0)};more.Content=advanced;
        var status=new TextBlock{TextWrapping=TextWrapping.Wrap,FontWeight=FontWeights.SemiBold};advanced.Children.Add(status);
        var detail=new TextBlock{TextWrapping=TextWrapping.Wrap,Margin=new(0,5,0,12)};detail.SetResourceReference(TextBlock.ForegroundProperty,"Muted");advanced.Children.Add(detail);
        advanced.Children.Add(new TextBlock{Text="Mã khôi phục",Margin=new(0,4,0,5)});var recovery=new PasswordBox{MinHeight=36,MaxLength=128,Padding=new(8)};recovery.SetResourceReference(Control.BackgroundProperty,"Surface");recovery.SetResourceReference(Control.ForegroundProperty,"Text");recovery.SetResourceReference(Control.BorderBrushProperty,"Line");advanced.Children.Add(recovery);
        var actions=new WrapPanel{Margin=new(0,8,0,0)};advanced.Children.Add(actions);var buttons=new List<Button>{activate,noKey,trial,later};
        var secret=new TextBox{IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Visibility=Visibility.Collapsed,MinHeight=50};advanced.Children.Add(secret);
        void Refresh(){bool used=LicenseRuntime.TrialUntil!=null||LicenseRuntime.Certificate?.Kind=="perpetual";trial.Visibility=used?Visibility.Collapsed:Visibility.Visible;trialDescription.Text=used?LicenseRuntime.TrialHistoryMessage:"Bạn có muốn dùng thử đầy đủ công cụ trong 30 ngày không?";status.Text=LicenseRuntime.Status;var c=LicenseRuntime.Certificate;detail.Text=c==null?"Chưa kích hoạt":(c.Kind=="trial"&&c.TrialUntil is {} end?$"Dùng thử đến {end.ToLocalTime():dd/MM/yyyy}":"Bản quyền vĩnh viễn")+$" · Xác minh trước {c.LeaseUntil.ToLocalTime():dd/MM/yyyy}";}
        async Task Run(string operation){
            if(operation=="trial"&&(LicenseRuntime.TrialUntil!=null||LicenseRuntime.Certificate?.Kind=="perpetual")){result.Text=LicenseRuntime.TrialHistoryMessage;return;}
            if(operation is "activate" or "recover"&&string.IsNullOrWhiteSpace(key.Text)){result.Text="Hãy nhập KEY trước khi tiếp tục.";key.Focus();return;}
            if(operation is "release" or "recover"){
                var prompt=operation=="release"?"Trả bản quyền máy này để dùng trên máy khác? Công cụ chỉnh sửa sẽ khóa sau khi trả thành công.":"Khôi phục sẽ trả tất cả máy cũ và đổi mã khôi phục. Tiếp tục?";
                if(MessageBox.Show(dialog,prompt,"Xác nhận bản quyền",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;
            }
            foreach(var b in buttons)b.IsEnabled=false;key.IsEnabled=recovery.IsEnabled=false;result.Text="Đang xác minh…";
            try{var code=await LicenseRuntime.Request(operation,key.Text.Trim(),recovery.Password);result.Text=LicenseRuntime.Status;trialCard.Visibility=Visibility.Collapsed;if(code!=null){secret.Text=code;secret.Visibility=Visibility.Visible;more.IsExpanded=true;result.Text+=" · Lưu mã khôi phục mới trong Xem thêm.";}recovery.Clear();ApplyLicenseState();}
            catch(Exception ex){result.Text=ex.Message;}finally{foreach(var b in buttons)b.IsEnabled=true;key.IsEnabled=recovery.IsEnabled=true;Refresh();}
        }
        void Add(string text,string operation){var b=new Button{Content=text,Margin=new(0,3,6,3),Padding=new(10,7,10,7)};buttons.Add(b);actions.Children.Add(b);b.Click+=async(_,_)=>await Run(operation);}
        Add("Xác minh lại","refresh");Add("Trả bản quyền máy này","release");Add("Khôi phục trên máy này","recover");
        activate.Click+=async(_,_)=>await Run("activate");trial.Click+=async(_,_)=>await Run("trial");
        dialog.PreviewKeyDown+=(_,args)=>{if(args.Key==Key.Escape){dialog.Close();args.Handled=true;}};
        Refresh();dialog.Closed+=(_,_)=>{_licenseDialog=null;ApplyLicenseState();};dialog.Loaded+=(_,_)=>key.Focus();dialog.ShowDialog();
    }
}
