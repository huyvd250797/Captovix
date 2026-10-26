using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
namespace Captovix;
public sealed record PdfImagePage(int Width,int Height,byte[] DeflatedRgb);
public static class PdfImages
{
    public static PdfImagePage Encode(int width,int height,byte[] rgb){if(width<=0||height<=0||(long)width*height>40_000_000||rgb.Length!=(long)width*height*3)throw new InvalidDataException("Trang PDF vượt40MP hoặc thiếu pixel RGB.");using var output=new MemoryStream();using(var z=new ZLibStream(output,CompressionLevel.Fastest,true))z.Write(rgb);return new(width,height,output.ToArray());}
    public static void Write(string path,IReadOnlyList<PdfImagePage> pages,CancellationToken cancel=default)
    {
        if(pages.Count==0||pages.Count>100||pages.Sum(p=>(long)p.DeflatedRgb.Length)>200_000_000||pages.Any(p=>p.Width<=0||p.Height<=0||(long)p.Width*p.Height>40_000_000))throw new InvalidDataException("PDF từ1–100 trang, tối đa200MB dữ liệu ảnh nén.");
        string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{using(var file=File.Create(temp)){var offsets=new List<long>{0};void Text(string s){file.Write(Encoding.ASCII.GetBytes(s));}void Object(int id,string value){offsets.Add(file.Position);Text($"{id} 0 obj\n{value}\nendobj\n");}void Stream(int id,string dictionary,byte[] bytes){offsets.Add(file.Position);Text($"{id} 0 obj\n<< {dictionary} /Length {bytes.Length} >>\nstream\n");file.Write(bytes);Text("\nendstream\nendobj\n");}
            Text("%PDF-1.4\n");file.Write([0x25,0xE2,0xE3,0xCF,0xD3,10]);Object(1,"<< /Type /Catalog /Pages 2 0 R >>");Object(2,$"<< /Type /Pages /Count {pages.Count} /Kids [{string.Join(' ',Enumerable.Range(0,pages.Count).Select(i=>$"{3+i*3} 0 R"))}] >>");
            for(int i=0;i<pages.Count;i++){cancel.ThrowIfCancellationRequested();var p=pages[i];int id=3+i*3;double w=p.Width>p.Height?842:595,h=p.Width>p.Height?595:842,scale=Math.Min((w-48)/p.Width,(h-48)/p.Height),iw=p.Width*scale,ih=p.Height*scale;string F(double n)=>n.ToString("0.###",CultureInfo.InvariantCulture);Object(id,$"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {w} {h}] /Resources << /XObject << /Im0 {id+1} 0 R >> >> /Contents {id+2} 0 R >>");Stream(id+1,$"/Type /XObject /Subtype /Image /Width {p.Width} /Height {p.Height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /FlateDecode",p.DeflatedRgb);Stream(id+2,"",Encoding.ASCII.GetBytes($"q {F(iw)} 0 0 {F(ih)} {F((w-iw)/2)} {F((h-ih)/2)} cm /Im0 Do Q\n"));}
            long start=file.Position;Text($"xref\n0 {offsets.Count}\n0000000000 65535 f \n");foreach(var offset in offsets.Skip(1))Text(offset.ToString("D10",CultureInfo.InvariantCulture)+" 00000 n \n");Text($"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{start}\n%%EOF\n");}cancel.ThrowIfCancellationRequested();File.Move(temp,path,false);}finally{File.Delete(temp);}
    }
}
