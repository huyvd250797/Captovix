using System.Reflection;

namespace Captovix;
public static class VersionInfo
{
    public const string AppId = "HuyVo.Captovix";
    public const string Channel = "win";
    public static string Current => typeof(VersionInfo).Assembly.GetName().Version?.ToString(3) ?? "1.8.1";
}
