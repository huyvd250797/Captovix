using System.Buffers.Binary;
using System.IO;

namespace Captovix;

internal static class RecordingContainer
{
    internal static void Verify(string path)
    {
        using var stream=File.OpenRead(path);
        bool fileType=false,index=false,media=false;
        Span<byte> header=stackalloc byte[8];
        Span<byte> extended=stackalloc byte[8];
        while(stream.Position<stream.Length){
            long start=stream.Position;
            stream.ReadExactly(header);
            ulong size=BinaryPrimitives.ReadUInt32BigEndian(header);
            var type=header[4..];
            int headerSize=8;
            if(size==1){
                stream.ReadExactly(extended);
                size=BinaryPrimitives.ReadUInt64BigEndian(extended);
                headerSize=16;
            }
            if(size==0)size=(ulong)(stream.Length-start);
            if(size<(ulong)headerSize||size>(ulong)(stream.Length-start))
                throw new InvalidDataException("MP4 chưa hoàn tất hoặc có khối dữ liệu bị cắt. Tệp tạm được giữ để kiểm tra.");
            if(size>(ulong)headerSize){
                fileType|=type.SequenceEqual("ftyp"u8);
                index|=type.SequenceEqual("moov"u8);
                media|=type.SequenceEqual("mdat"u8);
            }
            stream.Position=start+(long)size;
        }
        if(!fileType||!index||!media)
            throw new InvalidDataException("MP4 thiếu dữ liệu hoặc chỉ mục phát. Tệp tạm được giữ để kiểm tra.");
    }
}
