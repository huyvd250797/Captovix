using System.IO;
namespace Captovix;
public sealed record ScrollFrame(int Width,int Height,byte[] Pixels)
{
    public void Validate(){if(Width<8||Height<32||(long)Width*Height>ScrollStitching.MaxFramePixels||Pixels.Length!=(long)Width*Height*4)throw new InvalidDataException("Khung cuộn phải cao ít nhất 32 px, tối đa 4 triệu pixel và có dữ liệu BGRA32 đầy đủ.");}
}
public enum ScrollMatchKind { Matched, Duplicate, Uncertain }
public sealed record ScrollMatch(ScrollMatchKind Kind,int Overlap,double Error,string Message,int CrossAxisOffset=0);
public static class ScrollStitching
{
    public const int MaxFrames=24,MaxHeight=32768;
    public const long MaxFramePixels=4_000_000,MaxOutputPixels=40_000_000,MaxSourcePixels=64_000_000;
    public static int OutputHeight(int width,int frameHeight,IReadOnlyList<int> overlaps)
    {
        if(width<8||frameHeight<32||(long)width*frameHeight>MaxFramePixels||overlaps.Count>=MaxFrames)throw new InvalidDataException("Vượt giới hạn khung nguồn (24 khung, 4 MP/khung).");
        if((long)width*frameHeight*(overlaps.Count+1)>MaxSourcePixels)throw new InvalidDataException("Phiên chụp vượt 64 triệu pixel nguồn.");
        long h=frameHeight;foreach(int overlap in overlaps){if(overlap<1||overlap>=frameHeight)throw new InvalidDataException("Overlap phải từ 1 đến chiều cao khung trừ 1.");h+=frameHeight-overlap;}
        if(h>MaxHeight||h*width>MaxOutputPixels)throw new InvalidDataException("Ảnh ghép vượt 40 triệu pixel hoặc chiều cao 32768 px. Hãy chia thành nhiều phiên.");return (int)h;
    }
    public static ScrollMatch Match(ScrollFrame previous,ScrollFrame next,CancellationToken cancellation=default)
    {
        previous.Validate();next.Validate();if(previous.Width!=next.Width||previous.Height!=next.Height)throw new InvalidDataException("Kích thước khung đã thay đổi. Dừng phiên và chọn vùng mới.");
        int w=previous.Width,h=previous.Height;
        double Score(int shift,bool dense=false){int rows=dense?h-shift:Math.Min(24,h-shift),cols=Math.Min(dense?128:40,w);double error=0;int count=0;for(int iy=0;iy<rows;iy++){int y=(int)((iy+.5)*(h-shift)/rows);for(int ix=0;ix<cols;ix++){int x=(int)((ix+.5)*w/cols);int a=((y+shift)*w+x)*4,b=(y*w+x)*4;for(int c=0;c<3;c++){error+=Math.Abs(previous.Pixels[a+c]-next.Pixels[b+c]);count++;}}}return error/count;}
        cancellation.ThrowIfCancellationRequested();if(previous.Pixels.AsSpan().SequenceEqual(next.Pixels))return new(ScrollMatchKind.Duplicate,h,0,"Khung không đổi hoặc đã tới cuối trang; không thêm khung trùng.");
        int minOverlap=Math.Max(16,h/6);var candidates=new List<(int Shift,double Error)>();
        for(int shift=1;shift<=h-minOverlap;shift++){if((shift&31)==0)cancellation.ThrowIfCancellationRequested();candidates.Add((shift,Score(shift)));}
        var shortlisted=candidates.OrderBy(x=>x.Error).Take(16).SelectMany(x=>Enumerable.Range(Math.Max(1,x.Shift-2),Math.Min(h-minOverlap,x.Shift+2)-Math.Max(1,x.Shift-2)+1)).Distinct().ToArray();
        var ordered=shortlisted.Select(shift=>{cancellation.ThrowIfCancellationRequested();return(Shift:shift,Error:Score(shift,true));}).OrderBy(x=>x.Error).ToArray();var best=ordered[0];double runner=ordered.Where(x=>Math.Abs(x.Shift-best.Shift)>3).Select(x=>x.Error).DefaultIfEmpty(255).Min();
        // Validate the proposed join in independent horizontal bands; a single easy patch cannot approve the whole table.
        double worst=0;for(int band=0;band<4;band++){double error=0;int n=0;for(int y=0;y<h-best.Shift;y+=Math.Max(1,(h-best.Shift)/96))for(int x=Math.Max(1,w*band/4);x<Math.Min(w-1,w*(band+1)/4);x+=Math.Max(1,w/160)){int a=((y+best.Shift)*w+x)*4,b=(y*w+x)*4;for(int c=0;c<3;c++){error+=Math.Abs(previous.Pixels[a+c]-next.Pixels[b+c]);n++;}}if(n>0)worst=Math.Max(worst,error/n);}
        int cross=0;double crossError=best.Error;for(int dx=-4;dx<=4;dx++){if(dx==0)continue;double error=0;int n=0;for(int y=0;y<h-best.Shift;y+=Math.Max(1,(h-best.Shift)/64))for(int x=5;x<w-5;x+=Math.Max(1,w/96)){int a=((y+best.Shift)*w+x)*4,b=(y*w+x-dx)*4;for(int c=0;c<3;c++){error+=Math.Abs(previous.Pixels[a+c]-next.Pixels[b+c]);n++;}}if(n>0&&error/n<crossError){crossError=error/n;cross=dx;}}
        bool shifted=cross!=0&&crossError<1.5&&best.Error-crossError>2;
        bool good=!shifted&&best.Error<2.5&&worst<4&&runner-best.Error>1.5;
        return new(good?ScrollMatchKind.Matched:ScrollMatchKind.Uncertain,h-best.Shift,best.Error,shifted?$"Khung lệch ngang {cross} px so với khung trước. Giữ lại để kiểm tra; nên bỏ khung và chụp lại để tránh lệch cột.":good?"Đã đối chiếu toàn chiều vùng ghép và các dải cột; hãy kiểm tra ảnh trước khi lưu.":"Mối nối chưa chắc chắn: nội dung lặp/động, thiếu vùng chồng hoặc lệch cột. Cần kiểm tra và xác nhận thủ công.",shifted?cross:0);

    }
    // Copy original rows, no interpolation, rescaling or recompression of source pixels.
    public static byte[] Compose(int width,int height,IReadOnlyList<int> overlaps,Func<int,ScrollFrame> load,CancellationToken cancellation=default)
    {
        int output=OutputHeight(width,height,overlaps),stride=checked(width*4),offset=0;var pixels=new byte[checked(output*stride)];
        for(int i=0;i<=overlaps.Count;i++){cancellation.ThrowIfCancellationRequested();var frame=load(i);frame.Validate();if(frame.Width!=width||frame.Height!=height)throw new InvalidDataException("Khung nguồn không đồng nhất.");int start=i==0?0:overlaps[i-1];int bytes=checked((height-start)*stride);Buffer.BlockCopy(frame.Pixels,start*stride,pixels,offset,bytes);offset+=bytes;}
        cancellation.ThrowIfCancellationRequested();return pixels;
    }
}
