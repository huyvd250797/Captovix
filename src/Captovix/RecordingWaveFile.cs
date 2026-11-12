using System.IO;
using System.Runtime.InteropServices;

namespace Captovix;

// PCM WAV with timestamp-positioned writes. Skipping silent spans avoids
// processing minutes of zero samples inside a WASAPI packet callback.
internal sealed class RecordingWaveFile : IDisposable
{
    private const int HeaderSize=44;
    private readonly FileStream _stream;
    private readonly int _rate,_channels,_block;
    private byte[] _buffer=[];
    private long _frames;
    private bool _closed;

    internal RecordingWaveFile(string path,int rate,int channels)
    {
        _rate=rate;
        _channels=channels;
        _block=channels*sizeof(short);
        _stream=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read);
        _stream.Position=HeaderSize;
    }

    internal void Write(long target,ReadOnlySpan<float> samples)
    {
        int count=samples.Length/_channels;
        int skip=(int)Math.Clamp(_frames-target,0,count);
        if(skip==count)return;
        target+=skip;
        samples=samples[(skip*_channels)..];
        int bytes=samples.Length*sizeof(short);
        if(_buffer.Length<bytes)_buffer=new byte[bytes];
        var output=MemoryMarshal.Cast<byte,short>(_buffer.AsSpan(0,bytes));
        for(int i=0;i<output.Length;i++){
            float value=float.IsFinite(samples[i])?Math.Clamp(samples[i],-1,1):0;
            output[i]=(short)Math.Round(value*(value<0?32768:32767));
        }
        _stream.Position=HeaderSize+DataLength(target);
        _frames=target+count-skip;
        DataLength(_frames);
        _stream.Write(_buffer,0,bytes);
    }

    private uint DataLength(long frames)
    {
        long bytes=checked(frames*_block);
        if(bytes>uint.MaxValue-36)throw new IOException("Tệp WAV vượt giới hạn dung lượng.");
        return (uint)bytes;
    }

    internal void Complete(long frames)
    {
        if(_closed)return;
        _closed=true;
        try{
            uint bytes=DataLength(frames);
            _stream.SetLength(HeaderSize+(long)bytes);
            _stream.Position=0;
            using var header=new BinaryWriter(_stream,System.Text.Encoding.ASCII,true);
            header.Write("RIFF"u8);
            header.Write(36+bytes);
            header.Write("WAVEfmt "u8);
            header.Write(16);
            header.Write((ushort)1);
            header.Write((ushort)_channels);
            header.Write(_rate);
            header.Write(_rate*_block);
            header.Write((ushort)_block);
            header.Write((ushort)16);
            header.Write("data"u8);
            header.Write(bytes);
        }finally{_stream.Dispose();}
    }

    public void Dispose()=>Complete(_frames);
}
