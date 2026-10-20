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
        VelopackApp.Build().SetAutoApplyOnStartup(false).Run();
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
