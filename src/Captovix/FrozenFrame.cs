using System.IO;
namespace Captovix;

// Physical-pixel storage. No DIP, display scale, viewport zoom or resampling.
public sealed class FrozenFrame
{
    public const long MaxPixels=64_000_000;
    private readonly byte[] _pixels;
    public PixelRect Bounds {get;}
    internal byte[] Pixels=>_pixels;
    public FrozenFrame(PixelRect bounds,byte[] pixels)
    {
        ValidateBounds(bounds);
        if(pixels.LongLength!=(long)bounds.Width*bounds.Height*4)throw new ArgumentException("Sai dữ liệu ảnh nền.");
        Bounds=bounds;_pixels=pixels; // Ownership transferred by capture; never shared with a live writer.
    }
    public static void ValidateBounds(PixelRect b)
    {
        if(b.Width<1||b.Height<1||b.Width>32768||b.Height>32768||(long)b.Width*b.Height>MaxPixels||(long)b.X+b.Width>int.MaxValue||(long)b.Y+b.Height>int.MaxValue)throw new InvalidDataException("Bố trí màn hình vượt giới hạn giữ ảnh 64 triệu pixel. Hãy giảm phạm vi màn hình; app không tự thu nhỏ ảnh.");
    }
    public byte[] Crop(PixelRect region)
    {
        if(region.Width<1||region.Height<1||region.X<Bounds.X||region.Y<Bounds.Y||(long)region.X+region.Width>(long)Bounds.X+Bounds.Width||(long)region.Y+region.Height>(long)Bounds.Y+Bounds.Height)throw new ArgumentOutOfRangeException(nameof(region),region,$"Vùng chọn nằm ngoài ảnh màn hình đã giữ. Vùng chọn: {region}; ảnh đã giữ: {Bounds}.");
        var result=new byte[checked(region.Width*region.Height*4)];int x=region.X-Bounds.X,y=region.Y-Bounds.Y;
        for(int row=0;row<region.Height;row++)Buffer.BlockCopy(_pixels,checked(((y+row)*Bounds.Width+x)*4),result,row*region.Width*4,region.Width*4);
        return result;
    }
    public bool TryColor(int x,int y,out byte b,out byte g,out byte r)
    {
        b=g=r=0;
        if(x<Bounds.X||y<Bounds.Y||(long)x>=(long)Bounds.X+Bounds.Width||(long)y>=(long)Bounds.Y+Bounds.Height)return false;
        int i=checked(((y-Bounds.Y)*Bounds.Width+x-Bounds.X)*4);b=_pixels[i];g=_pixels[i+1];r=_pixels[i+2];return true;
    }
}
