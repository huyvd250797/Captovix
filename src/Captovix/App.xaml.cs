using System.IO;
using System.Windows;
using System.Threading;

namespace Captovix;
public partial class App : Application
{
    private Mutex? _singleInstance;
    private readonly ExceptionDialogGate _errorDialogs=new();
    internal Exception? StartupFailure { get; private set; }
    private bool _startupHealthy;

    internal void ConfirmStartup()
    {
        if (StartupFailure != null) return;
        _startupHealthy = true;
        ReleaseRecoveryState.Healthy(DataRoot, VersionInfo.Current);
        try { StartupHealthSignal.Signal(); }
        catch (Exception error) { Log(error); }
    }

    public static bool StartedAutomatically {get;set;}
    protected override void OnStartup(StartupEventArgs e)
    {
        bool first;
        using (var gate = new Mutex(false, StartupHealthSignal.RepairGate))
        {
            bool mayStart;
            try { mayStart = gate.WaitOne(0); }
            catch (AbandonedMutexException) { mayStart = true; }
            if (!mayStart) { Shutdown(1); return; }
            try { _singleInstance = new Mutex(true, StartupHealthSignal.AppMutex, out first); }
            finally { gate.ReleaseMutex(); }
        }
        if (!first) { CaptureTray.RestoreExisting(); Shutdown(); return; }
        DispatcherUnhandledException += (_, a) => {
            // Mark handled before entering MessageBox's nested dispatcher.
            a.Handled=true;
            if (!_startupHealthy)
            {
                if (StartupFailure == null)
                {
                    StartupFailure = a.Exception;
                    Dispatcher.BeginInvoke(() => Shutdown(1));
                }
                return;
            }
            _errorDialogs.Report(()=>{Log(a.Exception);AppDialog.Show("Có lỗi: " + a.Exception.Message + "\nBáo cáo lỗi được lưu trong thư mục dữ liệu của app.", "Captovix");});
        };
        AppScrolling.Initialize();Dispatcher.BeginInvoke(BrandShortcuts.Repair,System.Windows.Threading.DispatcherPriority.ApplicationIdle);WindowTheme.Initialize();AppearanceManager.Initialize();
        base.OnStartup(e);
    }
    public static string DataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HuyVo", "Captovix");
    public static void Log(Exception e) { try { Directory.CreateDirectory(DataRoot); File.AppendAllText(Path.Combine(DataRoot, "errors.log"), $"{DateTimeOffset.Now:O} {e}\n"); } catch { } }
    protected override void OnExit(ExitEventArgs e) { StaImageWorker.Stop();AppearanceManager.Stop(); _singleInstance?.Dispose(); base.OnExit(e); }
}
