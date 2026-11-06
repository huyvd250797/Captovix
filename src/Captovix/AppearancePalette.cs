namespace Captovix;

// Shared source values. Recovery compiles these into its own executable.
internal sealed record AppearancePalette(string Surface, string AppBackground, string CaptionSurface,
    string Text, string Muted, string Line, string Soft, string Hover, string PopupLine)
{
    public static AppearancePalette For(bool dark) => dark
        ? new("#27282D", "#202126", "#28292F", "#F0F0F3", "#B9BBC5", "#45464F", "#303139", "#393B44", "#6E707B")
        : new("#FFFFFF", "#F1F2F5", "#E8E9F0", "#202129", "#575A65", "#D7D8DF", "#EEF0F5", "#E3E5EC", "#92949E");

    public static bool SystemLight()
    {
        try
        {
            return Microsoft.Win32.Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme", 1) is not int value || value != 0;
        }
        catch { return true; }
    }
}
