using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace Captovix;
public sealed class UpdateSettings
{
    public string SourceKind { get; set; } = "GitHub";
    public string SourceUrl { get; set; } = "";
    public bool AutoCheck { get; set; } = true;
    public DateTimeOffset? LastCheckUtc { get; set; }
    public string LastCheckedSource { get; set; } = "";
    [JsonIgnore] public string SourceKey => SourceKind + "|" + NormalizeUrl();
    [JsonIgnore] public bool IsConfigured => !string.IsNullOrWhiteSpace(SourceUrl);

    public string NormalizeUrl()
    {
        if (!Uri.TryCreate(SourceUrl.Trim(), UriKind.Absolute, out var url) ||
            url.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(url.UserInfo) ||
            !string.IsNullOrEmpty(url.Query) || !string.IsNullOrEmpty(url.Fragment))
            throw new ArgumentException("Hãy nhập địa chỉ HTTPS, không chứa tài khoản, token, query hoặc fragment.");
        if (SourceKind == "GitHub")
        {
            var parts = url.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (!url.IsDefaultPort || !url.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
                parts.Length != 2 || parts.Any(p => !p.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.')) ||
                parts.Any(p => p is "." or ".." || p.EndsWith(".git", StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Nguồn GitHub cần dạng https://github.com/OWNER/REPO (repository công khai).");
        }
        else if (SourceKind != "Https") throw new ArgumentException("Loại nguồn cập nhật không hợp lệ.");
        if (SourceKind == "Https" && url.AbsolutePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Nhập URL thư mục chứa releases.win.json, không nhập URL file JSON.");
        return url.GetLeftPart(UriPartial.Path).TrimEnd('/');
    }

    public bool ShouldAutoCheck(DateTimeOffset now) => AutoCheck && IsConfigured &&
        (LastCheckedSource != SourceKey || LastCheckUtc == null ||
         now - LastCheckUtc.Value >= TimeSpan.FromHours(24) || LastCheckUtc.Value > now);
}

public static class UpdateSettingsStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    public static UpdateSettings Load(string root, string defaultsPath, out string? warning)
    {
        warning = null;
        var path = Path.Combine(root, "update-settings.json");
        // A malformed override must not silently switch to another publisher.
        var selected = File.Exists(path) ? path : defaultsPath;
        if (!File.Exists(selected)) return new();
        try
        {
            var s = JsonSerializer.Deserialize<UpdateSettings>(File.ReadAllText(selected)) ?? throw new InvalidDataException("Empty configuration.");
            if (s.IsConfigured) s.SourceUrl = s.NormalizeUrl();
            return s;
        }
        catch (Exception)
        {
            warning = "Chưa đọc được cấu hình cập nhật. Hãy lưu lại nguồn trong mục Cập nhật ứng dụng.";
            return new() { AutoCheck = false };
        }
    }

    public static void Save(string root, UpdateSettings settings)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "update-settings.json");
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(settings, Json));
        File.Move(temp, path, true);
    }
}
