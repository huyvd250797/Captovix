using System.Diagnostics;

namespace Captovix;

internal static class RecoveryProgram
{
    internal static readonly string DataRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HuyVo", "Captovix");

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var failure = args.Contains("--startup-failure", StringComparer.Ordinal);
            if (args.Length >= 2 && args[0] == "--run")
            {
                var content = Path.GetFullPath(args[1]);
                using var instance = new Mutex(true, @"Local\HuyVo.Captovix.RecoveryCenter", out var first);
                if (!first)
                {
                    MessageBox.Show("Trung tâm khôi phục đang mở.", "Captovix");
                    return;
                }
                Application.Run(new RecoveryWindow(content, failure));
                return;
            }

            // Velopack replaces current in its entirety. The running tool and
            // its working directory must both be outside the install folder.
            var parent = Path.Combine(Path.GetTempPath(), "Captovix-Recovery");
            Directory.CreateDirectory(parent);
            foreach (var folder in Directory.EnumerateDirectories(parent))
            {
                if (!Guid.TryParseExact(Path.GetFileName(folder), "N", out _) ||
                    Directory.GetLastWriteTimeUtc(folder) > DateTime.UtcNow.AddDays(-1) ||
                    File.GetAttributes(folder).HasFlag(FileAttributes.ReparsePoint)) continue;
                try
                {
                    File.Delete(Path.Combine(folder, "Captovix.Recovery.exe"));
                    Directory.Delete(folder);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            var staging = Path.Combine(parent, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            var copy = Path.Combine(staging, "Captovix.Recovery.exe");
            File.Copy(Environment.ProcessPath!, copy);
            var start = new ProcessStartInfo(copy) { WorkingDirectory = staging, UseShellExecute = false };
            start.ArgumentList.Add("--run");
            start.ArgumentList.Add(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory));
            if (failure) start.ArgumentList.Add("--startup-failure");
            using var child = Process.Start(start) ?? throw new IOException("Không mở được Trung tâm khôi phục.");
        }
        catch (Exception error)
        {
            MessageBox.Show(error.GetBaseException().Message, "Trung tâm khôi phục Captovix",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
