using System;
using Velopack;

namespace Captovix;
public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Must precede WPF, the single-instance mutex and all user-data access.
        // Pending updates only apply after the user explicitly chooses Restart.
        VelopackApp.Build().SetAppUserModelId("velopack."+VersionInfo.AppId).SetAutoApplyOnStartup(false).OnBeforeUninstallFastCallback(_=>{StartupRegistration.RemoveOwned();StartupRecoveryLauncher.RemoveShortcut(Velopack.Locators.VelopackLocator.Current.RootAppDir);}).Run();
        if(args.Length>0&&args[0]=="--restore-backup"){Environment.ExitCode=RecoveryWorker.Run(args);return;}
        if(args.Length>0&&args[0]=="--ocr-worker"){Environment.ExitCode=OcrWorker.Run(args);return;}
        if(args.Contains("--hybrid-ui-probe",StringComparer.Ordinal)){Environment.ExitCode=HybridProbeHost.Run();return;}
        if(args.Contains("--element-probe",StringComparer.Ordinal)){Environment.ExitCode=ElementProbeHost.Run();return;}
        if(args.Contains("--fidelity-check",StringComparer.Ordinal)){Environment.ExitCode=FidelityWindowsProbe.RunInteractive();return;}
        App.StartedAutomatically=args.Contains("--captovix-autostart",StringComparer.Ordinal);
        StartupHealthSignal.Probe=args.FirstOrDefault(a=>a.StartsWith(StartupHealthSignal.Argument,StringComparison.Ordinal))?[StartupHealthSignal.Argument.Length..];
        try {
            var app = new App();
            app.InitializeComponent();
            Environment.ExitCode=app.Run();
            if (app.StartupFailure is {} failure) ReportStartupFailure(failure);
        } catch(Exception error) {
            ReportStartupFailure(error);
        }
    }

    private static void ReportStartupFailure(Exception error)
    {
        App.Log(new InvalidOperationException("Khởi động Captovix V"+VersionInfo.Current+" thất bại.",error));
        Environment.ExitCode=1;
        // The running Recovery Center already observes this probe process.
        if (StartupHealthSignal.Probe != null || StartupRecoveryLauncher.Open(startupFailure:true)) return;

        string log=System.IO.Path.Combine(App.DataRoot,"errors.log");
        try { System.Windows.MessageBox.Show("Không thể khởi động Captovix V"+VersionInfo.Current+".\n\n"+error.GetBaseException().Message+"\n\nChi tiết lỗi: "+log+"\nChạy Recovery.cmd hoặc dùng Full Setup.", "Captovix · Lỗi khởi động",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error); } catch { /* Keep the original startup error. */ }
    }
}
