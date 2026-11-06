using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;

namespace Captovix;

internal static class ShellBrandIcon
{
    private static BitmapSource? _image;
    private static byte[]? _resource;
    internal static string? ShortcutPath { get; private set; }

    // A 32-bit RT_ICON DIB with a bottom-up XOR bitmap and a padded AND mask.
    // The live icon is built in memory, independently of Explorer's file cache.
    internal static byte[] Resource(BitmapSource image)
    {
        if(ReferenceEquals(image,_image)&&_resource is { } cached)return cached;
        int width=image.PixelWidth,height=image.PixelHeight,stride=width*4;
        var pixels=ImageTransfer.Bgra(image);
        int maskStride=((width+31)/32)*4;
        var mask=new byte[maskStride*height];
        using var stream=new MemoryStream();
        using var writer=new BinaryWriter(stream);
        writer.Write(40);
        writer.Write(width);
        writer.Write(height*2);
        writer.Write((ushort)1);
        writer.Write((ushort)32);
        for(int i=0;i<6;i++)writer.Write(0);
        for(int y=height-1;y>=0;y--){
            int offset=y*stride;
            writer.Write(pixels,offset,stride);
            int maskRow=(height-1-y)*maskStride;
            for(int x=0;x<width;x++){
                if(pixels[offset+x*4+3]==0)mask[maskRow+x/8]|=(byte)(0x80>>(x%8));
            }
        }
        writer.Write(mask);
        _image=image;
        return _resource=stream.ToArray();
    }

    internal static nint Create(BitmapSource image,int width,int height)
    {
        var resource=Resource(image);
        return CreateIconFromResourceEx(resource,(uint)resource.Length,true,0x00030000,width,height,0);
    }

    internal static void SaveForShortcuts(BitmapSource image)
    {
        try{
            var resource=Resource(image);
            var folder=Path.Combine(App.DataRoot,"Brand");
            Directory.CreateDirectory(folder);
            string hash=Convert.ToHexString(SHA256.HashData(resource))[..24];
            string path=Path.Combine(folder,$"captovix-theme-{hash}.ico");
            if(!File.Exists(path)){
                var temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
                try{
                    using(var stream=File.Create(temp))
                    using(var writer=new BinaryWriter(stream)){
                        writer.Write((ushort)0);
                        writer.Write((ushort)1);
                        writer.Write((ushort)1);
                        writer.Write((byte)image.PixelWidth);
                        writer.Write((byte)image.PixelHeight);
                        writer.Write((ushort)0);
                        writer.Write((ushort)1);
                        writer.Write((ushort)32);
                        writer.Write(resource.Length);
                        writer.Write(22);
                        writer.Write(resource);
                    }
                    File.Move(temp,path,true);
                }finally{if(File.Exists(temp))File.Delete(temp);}
            }
            ShortcutPath=path;
            if(BrandShortcuts.RefreshPinned(path)){
                // Retain the current image and the last few cache keys for Explorer.
                var stale=Directory.EnumerateFiles(folder,"captovix-theme-*.ico")
                    .Where(file=>file!=path).OrderByDescending(File.GetLastWriteTimeUtc).Skip(3);
                foreach(var file in stale)File.Delete(file);
            }
        }catch(Exception ex){App.Log(ex);}
    }

    [DllImport("user32.dll",SetLastError=true)]
    private static extern nint CreateIconFromResourceEx(byte[] bits,uint size,[MarshalAs(UnmanagedType.Bool)]bool icon,uint version,int width,int height,uint flags);
}
