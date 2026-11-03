using System.Text.RegularExpressions;
namespace Captovix;
public static class ReleaseNotesPresentation
{
    public static string Text(string? notes)
    {
        if(string.IsNullOrWhiteSpace(notes))return "Cải tiến và sửa lỗi trong phiên bản mới.";
        var lines=notes.Replace("\r\n","\n").Split('\n');
        var cleaned=lines.Select(line=>{
            line=Regex.Replace(line,@"^\s{0,3}#{1,6}\s+","");
            line=Regex.Replace(line,@"^\s*[-*+]\s+","• ");
            line=Regex.Replace(line,@"\[([^\]]+)\]\([^)]*\)","$1");
            line=line.Replace("**","").Replace("__","").Replace("`","");return line.TrimEnd();
        });
        var text=string.Join("\n",cleaned).Trim();return text.Length>5000?text[..5000]+"…":text;
    }
}
