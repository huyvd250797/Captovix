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
        VelopackApp.Build().SetAutoApplyOnStartup(false).OnBeforeUninstallFastCallback(_=>StartupRegistration.RemoveOwned()).Run();
        if(args.Contains("--fidelity-check",StringComparer.Ordinal)){Environment.ExitCode=FidelityWindowsProbe.RunInteractive();return;}
        App.StartedAutomatically=args.Contains("--captovix-autostart",StringComparer.Ordinal);
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
