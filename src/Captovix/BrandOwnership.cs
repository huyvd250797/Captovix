using System.IO;
namespace Captovix;
public static class BrandOwnership
{
    public static bool IsOwned(string root,string target)
    {
        if(string.IsNullOrWhiteSpace(target)||!Path.IsPathFullyQualified(target))return false;
        string full=Path.GetFullPath(target),basePath=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
        return full.StartsWith(basePath,StringComparison.OrdinalIgnoreCase)&&(Path.GetFileName(full).Equals("Captovix.exe",StringComparison.OrdinalIgnoreCase)||full.Equals(Path.Combine(root,"Update.exe"),StringComparison.OrdinalIgnoreCase));
    }
}
