using System.IO;
using System.Windows;
using System.Threading;

namespace CaptureFoundation;
public partial class App : Application
{
    private Mutex? _singleInstance;
    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstance = new Mutex(true, "Local\\HuyVo.CaptureFoundation", out bool first);
        if (!first) { MessageBox.Show("Capture Foundation đang mở. Hãy chọn cửa sổ app trên taskbar."); Shutdown(); return; }
        DispatcherUnhandledException += (_, a) => {
            Log(a.Exception); MessageBox.Show("Có lỗi: " + a.Exception.Message + "\nBáo cáo lỗi được lưu trong thư mục dữ liệu của app.", "Capture Foundation"); a.Handled = true;
        };
        base.OnStartup(e);
    }
    public static string DataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HuyVo", "CaptureFoundation");
    public static void Log(Exception e) { try { Directory.CreateDirectory(DataRoot); File.AppendAllText(Path.Combine(DataRoot, "errors.log"), $"{DateTimeOffset.Now:O} {e}\n"); } catch { } }
    protected override void OnExit(ExitEventArgs e) { _singleInstance?.Dispose(); base.OnExit(e); }
}
