using System.IO;
namespace Captovix;
public static class AdvancedScroll
{
    public static ScrollFrame Transpose(ScrollFrame f){var p=new byte[f.Pixels.Length];for(int y=0;y<f.Height;y++)for(int x=0;x<f.Width;x++)Buffer.BlockCopy(f.Pixels,(y*f.Width+x)*4,p,(x*f.Height+y)*4,4);return new(f.Height,f.Width,p);}
    public static ScrollFrame Trim(ScrollFrame f,int top,int bottom){if(top<0||bottom<0||f.Height-top-bottom<32)throw new InvalidDataException("Header/footer phải chừa ít nhất32px nội dung.");var p=new byte[f.Width*(f.Height-top-bottom)*4];Buffer.BlockCopy(f.Pixels,top*f.Width*4,p,0,p.Length);return new(f.Width,f.Height-top-bottom,p);}
}
