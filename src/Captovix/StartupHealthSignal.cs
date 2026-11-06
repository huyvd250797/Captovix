using System.Threading;

namespace Captovix;

internal static class StartupHealthSignal
{
    public const string AppMutex = @"Local\HuyVo.Captovix";
    public const string RepairGate = @"Local\HuyVo.Captovix.RepairGate";
    public const string Prefix = @"Local\HuyVo.Captovix.Ready.";
    public const string Argument = "--recovery-probe=";
    public static string? Probe { get; set; }

    public static void Signal()
    {
        if (!Guid.TryParseExact(Probe, "N", out var id)) return;
        if (EventWaitHandle.TryOpenExisting(Prefix + id.ToString("N"), out var ready))
        {
            using (ready) ready.Set();
        }
    }
}
