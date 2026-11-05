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
        VelopackApp.Build().SetAppUserModelId("velopack."+VersionInfo.AppId).SetAutoApplyOnStartup(false).OnBeforeUninstallFastCallback(_=>StartupRegistration.RemoveOwned()).Run();
        if(args.Length>0&&args[0]=="--ocr-worker"){Environment.ExitCode=OcrWorker.Run(args);return;}
        if(args.Contains("--hybrid-ui-probe",StringComparer.Ordinal)){Environment.ExitCode=HybridProbeHost.Run();return;}
        if(args.Contains("--element-probe",StringComparer.Ordinal)){Environment.ExitCode=ElementProbeHost.Run();return;}
        if(args.Contains("--fidelity-check",StringComparer.Ordinal)){Environment.ExitCode=FidelityWindowsProbe.RunInteractive();return;}
        App.StartedAutomatically=args.Contains("--captovix-autostart",StringComparer.Ordinal);
        try {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        } catch(Exception error) {
            // Resource loading can fail before OnStartup registers dispatcher handling.
            App.Log(new InvalidOperationException("Khởi động Captovix V"+VersionInfo.Current+" thất bại.",error));
            string log=System.IO.Path.Combine(App.DataRoot,"errors.log");
            try { System.Windows.MessageBox.Show("Không thể khởi động Captovix V"+VersionInfo.Current+".\n\n"+error.GetBaseException().Message+"\n\nChi tiết lỗi: "+log,"Captovix · Lỗi khởi động",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error); } catch { /* Logging must not cause another startup failure. */ }
            Environment.ExitCode=1;
        }
    }
}
