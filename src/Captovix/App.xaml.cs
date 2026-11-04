using System.IO;
using System.Windows;
using System.Threading;

namespace Captovix;
public partial class App : Application
{
    private Mutex? _singleInstance;
    private readonly ExceptionDialogGate _errorDialogs=new();
    public static bool StartedAutomatically {get;set;}
    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstance = new Mutex(true, "Local\\HuyVo.Captovix", out bool first);
        if (!first) { CaptureTray.RestoreExisting(); Shutdown(); return; }
        DispatcherUnhandledException += (_, a) => {
            // Mark handled before entering MessageBox's nested dispatcher.
            a.Handled=true;
            _errorDialogs.Report(()=>{Log(a.Exception);AppDialog.Show("Có lỗi: " + a.Exception.Message + "\nBáo cáo lỗi được lưu trong thư mục dữ liệu của app.", "Captovix");});
        };
        AppScrolling.Initialize();Dispatcher.BeginInvoke(BrandShortcuts.Repair,System.Windows.Threading.DispatcherPriority.ApplicationIdle);WindowTheme.Initialize();AppearanceManager.Initialize();
        base.OnStartup(e);
    }
    public static string DataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HuyVo", "Captovix");
    public static void Log(Exception e) { try { Directory.CreateDirectory(DataRoot); File.AppendAllText(Path.Combine(DataRoot, "errors.log"), $"{DateTimeOffset.Now:O} {e}\n"); } catch { } }
    protected override void OnExit(ExitEventArgs e) { StaImageWorker.Stop();AppearanceManager.Stop(); _singleInstance?.Dispose(); base.OnExit(e); }
}
