using System.Buffers.Binary;
using System.IO;

namespace Captovix;

internal static class RecordingFrameRate
{
    private readonly record struct Box(uint Type,long Data,long End);
    private const uint Moov=0x6d6f6f76,Trak=0x7472616b,Mdia=0x6d646961,Hdlr=0x68646c72,Vide=0x76696465,
        Mdhd=0x6d646864,Minf=0x6d696e66,Stbl=0x7374626c,Stts=0x73747473;

    internal static double Read(string path)
    {
        using var file=File.OpenRead(path);
        Span<byte> row=stackalloc byte[8];
        foreach(var movie in Children(file,0,file.Length).Where(b=>b.Type==Moov))
        foreach(var track in Children(file,movie.Data,movie.End).Where(b=>b.Type==Trak))
        foreach(var media in Children(file,track.Data,track.End).Where(b=>b.Type==Mdia)){
            var boxes=Children(file,media.Data,media.End).ToArray();
            var handler=boxes.FirstOrDefault(b=>b.Type==Hdlr);
            if(handler.Type==0||UInt32(file,handler,8)!=Vide)continue;
            var header=boxes.FirstOrDefault(b=>b.Type==Mdhd);
            if(header.Type==0)break;
            uint version=UInt32(file,header,0)>>24;
            if(version>1)break;
            uint scale=UInt32(file,header,version==0?12:20);
            foreach(var minf in boxes.Where(b=>b.Type==Minf))
            foreach(var table in Children(file,minf.Data,minf.End).Where(b=>b.Type==Stbl))
            foreach(var time in Children(file,table.Data,table.End).Where(b=>b.Type==Stts)){
                uint entries=UInt32(file,time,4);
                if(entries>(time.End-time.Data-8)/8)throw Invalid();
                double frames=0,ticks=0;
                file.Position=time.Data+8;
                for(uint i=0;i<entries;i++){
                    file.ReadExactly(row);
                    uint count=BinaryPrimitives.ReadUInt32BigEndian(row),delta=BinaryPrimitives.ReadUInt32BigEndian(row[4..]);
                    frames+=count;ticks+=(double)count*delta;
                }
                if(scale>0&&ticks>0&&frames>0)return frames*scale/ticks;
            }
        }
        throw Invalid();
    }

    private static uint UInt32(Stream file,Box box,int offset)
    {
        if(box.End-box.Data<offset+4)throw Invalid();
        Span<byte> value=stackalloc byte[4];
        file.Position=box.Data+offset;
        file.ReadExactly(value);
        return BinaryPrimitives.ReadUInt32BigEndian(value);
    }

    private static IEnumerable<Box> Children(Stream file,long start,long end)
    {
        for(long position=start;position<end;){
            var box=ReadBox(file,position,end);
            yield return box;
            position=box.End;
        }
    }

    private static Box ReadBox(Stream file,long position,long end)
    {
        Span<byte> bytes=stackalloc byte[8];
        if(end-position<8)throw Invalid();
        file.Position=position;
        file.ReadExactly(bytes);
        ulong size=BinaryPrimitives.ReadUInt32BigEndian(bytes);
        uint type=BinaryPrimitives.ReadUInt32BigEndian(bytes[4..]);
        int length=8;
        if(size==1){
            if(end-position<16)throw Invalid();
            file.ReadExactly(bytes);
            size=BinaryPrimitives.ReadUInt64BigEndian(bytes);
            length=16;
        }
        if(size==0)size=(ulong)(end-position);
        if(size<(uint)length||size>(ulong)(end-position))throw Invalid();
        return new(type,position+length,position+(long)size);
    }

    private static InvalidDataException Invalid()=>new("Không đọc được FPS thực tế trong MP4. Tệp tạm được giữ để kiểm tra.");
}
