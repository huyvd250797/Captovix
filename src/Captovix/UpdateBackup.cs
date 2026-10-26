using System.IO;
using System.IO.Compression;

namespace Captovix;
public static class UpdateBackup
{
    // Only lightweight state and annotation JSON. Originals and thumbnails stay in place.
    public static string Create(string root, string currentVersion, string nextVersion)
    {
        var folder = Path.Combine(root, "Backups"); Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, "before-update-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N")[..6] + ".zip");
        var temp = path + ".tmp";
        try
        {
            using (var zip = ZipFile.Open(temp, ZipArchiveMode.Create))
            {
                foreach (var name in new[] { "library.json", "settings.json", "update-settings.json", "editor-resume.json", "annotation-presets.json", "capture-workflow.json", "style-favorites.json", "tool-stars.json", "tool-order.json", "batch-export.json", "editor-layout.json", "media-location.json", "recent-capture-regions.json" })
                {
                    var source = Path.Combine(root, name);
                    if (File.Exists(source)) zip.CreateEntryFromFile(source, name);
                }
                var appearance=Path.Combine(root,"appearance-settings.json");if(File.Exists(appearance))zip.CreateEntryFromFile(appearance,"appearance-settings.json");
                var edits = Path.Combine(root, "Edits");
                if (Directory.Exists(edits))
                    foreach (var source in Directory.EnumerateFiles(edits, "*", SearchOption.TopDirectoryOnly).Where(p=>p.EndsWith(".json",StringComparison.OrdinalIgnoreCase)||p.EndsWith(".lastgood.bak",StringComparison.OrdinalIgnoreCase)))
                    {
                        if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0) throw new IOException("Không sao lưu liên kết ngoài thư mục dữ liệu.");
                        zip.CreateEntryFromFile(source, "Edits/" + Path.GetFileName(source));
                    }
                using var writer = new StreamWriter(zip.CreateEntry("backup-info.txt").Open());
                writer.WriteLine($"Captovix {currentVersion} -> {nextVersion}\nUTC: {DateTimeOffset.UtcNow:O}\nMetadata and edit documents only. Media files are not included.");
            }
            File.Move(temp, path);
            return path;
        }
        catch { File.Delete(temp); throw; }
    }
}
