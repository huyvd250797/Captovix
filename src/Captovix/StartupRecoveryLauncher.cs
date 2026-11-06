using System.Diagnostics;
using System.IO;
using System.Windows;
using Velopack.Windows;

namespace Captovix;

internal static class StartupRecoveryLauncher
{
    private static string ShortcutPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Captovix Recovery Center.lnk");
    private static string RecoveryExe(string root) => Path.Combine(root, "current", "Captovix.Recovery.exe");

    internal static void EnsureShortcut(string root, string icon)
    {
        var exe = RecoveryExe(root);
        if (File.Exists(ShortcutPath) || !File.Exists(exe)) return;
        using var link = new ShellLink { Target = exe, IconPath = icon, WorkingDirectory = root };
        link.Save(ShortcutPath);
    }

    internal static void RemoveShortcut(string? root)
    {
        if (root == null || !File.Exists(ShortcutPath)) return;
        try
        {
            using var link = new ShellLink(ShortcutPath);
            if (string.Equals(link.Target, RecoveryExe(root), StringComparison.OrdinalIgnoreCase)) File.Delete(ShortcutPath);
        }
        catch (Exception error) { App.Log(error); }
    }

    public static bool Open(bool startupFailure = false)
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Captovix.Recovery.exe");
            if (!File.Exists(path)) return false;
            var start = new ProcessStartInfo(path) { UseShellExecute = false, WorkingDirectory = Path.GetTempPath() };
            if (startupFailure) start.ArgumentList.Add("--startup-failure");
            using var process = Process.Start(start);
            return process != null;
        }
        catch (Exception error) { App.Log(error); return false; }
    }
}

public partial class MainWindow
{
    private void RecoveryCenter_Click(object sender, RoutedEventArgs e)
    {
        if (!StartupRecoveryLauncher.Open()) SetStatus("Chưa mở được Trung tâm khôi phục. Dùng Full Setup để bổ sung công cụ.");
    }
}
