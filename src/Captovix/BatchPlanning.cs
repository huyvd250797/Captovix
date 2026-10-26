using System.IO;
using System.Text.RegularExpressions;
namespace Captovix;
public sealed record BatchOptions(string Format="PNG",string NamePattern="{date}-{n}-{name}",int LongEdge=0,string Watermark="",int WatermarkSize=28)
{
    public void Validate(){if(Format is not ("PNG" or "JPEG" or "PDF")||NamePattern.Length>160||string.IsNullOrWhiteSpace(NamePattern)||LongEdge<0||LongEdge>7680||Watermark.Length>80||WatermarkSize<8||WatermarkSize>96)throw new InvalidDataException("Kiểm tra định dạng, tên, cạnh dài0–7680 và watermark tối đa80 ký tự.");if(Regex.Replace(NamePattern,@"\{(date|name|n)\}","").IndexOfAny(['{','}'])>=0)throw new InvalidDataException("Tên chỉ hỗ trợ {date}, {name}, {n}.");}
}
public static class BatchNaming
{
    public static string Safe(string text){var name=new string(text.Select(c=>c<32||"<>:\"/\\|?*".Contains(c)?'_':c).ToArray()).Trim().TrimEnd('.', ' ');if(name.Length>120)name=name[..120].TrimEnd('.', ' ');if(name.Length==0)name="Captovix";if(Regex.IsMatch(name.Split('.')[0],@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])$",RegexOptions.IgnoreCase))name="_"+name;return name;}
    public static string FileName(BatchOptions options,string title,DateTimeOffset date,int number){options.Validate();return Safe(options.NamePattern.Replace("{date}",date.ToLocalTime().ToString("yyyyMMdd-HHmmss")).Replace("{n}",number.ToString("D3")).Replace("{name}",title))+(options.Format=="JPEG"?".jpg":options.Format=="PDF"?".pdf":".png");}
    public static string Unique(string folder,string name,ISet<string> reserved){string stem=Path.GetFileNameWithoutExtension(name),ext=Path.GetExtension(name),candidate=Path.Combine(folder,name);int n=2;while(reserved.Contains(candidate)||File.Exists(candidate)||Directory.Exists(candidate)){candidate=Path.Combine(folder,$"{stem} ({n++}){ext}");if(n>10000)throw new IOException("Quá nhiều tên file trùng.");}reserved.Add(candidate);return candidate;}
    public static (int Width,int Height) Size(int width,int height,int edge){if(width<=0||height<=0||edge<0)throw new InvalidDataException("Kích thước không hợp lệ.");double ratio=edge==0?1:Math.Min(1,edge/(double)Math.Max(width,height));return(Math.Max(1,(int)Math.Round(width*ratio)),Math.Max(1,(int)Math.Round(height*ratio)));}
}
