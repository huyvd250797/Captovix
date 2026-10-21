using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Captovix;

// SDR desktop photographs only. No StretchBlt, encoder alignment or DIP sizing.
// Window capture and video retain their existing native recorder backends.
internal static class DesktopPhotoCapture
{
    public static void Save(PixelRect region,IReadOnlyList<DisplayInfo> displays,bool cursor,string path)
    {
        ImageFidelity.ValidateCapture(region,region.Width,region.Height,false);
        var parts=CaptureGeometry.Split(region,displays.Select(d=>(d.Device,d.Bounds)));
        if(parts.Count==0)throw new InvalidOperationException("Vùng chọn không nằm trên màn hình đang kết nối.");
        nint screen=0,dc=0,bitmap=0,previous=0;
        try{
            screen=GetDC(0);if(screen==0)throw Error("GetDC");
            dc=CreateCompatibleDC(screen);if(dc==0)throw Error("CreateCompatibleDC");
            var info=new BitmapInfo{Size=(uint)Marshal.SizeOf<BitmapInfo>(),Width=region.Width,Height=-region.Height,Planes=1,Bits=32};
            bitmap=CreateDIBSection(screen,ref info,0,out var bits,0,0);if(bitmap==0||bits==0)throw Error("CreateDIBSection");
            previous=SelectObject(dc,bitmap);if(previous==0||previous==(nint)(-1))throw Error("SelectObject");
            var pixels=new byte[checked(region.Width*region.Height*4)];
            Marshal.Copy(pixels,0,bits,pixels.Length); // Gaps between monitors are black.
            foreach(var part in parts){
                if(!BitBlt(dc,part.X,part.Y,part.Crop.Width,part.Crop.Height,screen,
                    region.X+part.X,region.Y+part.Y,0x40CC0020))throw Error("BitBlt");
            }
            if(cursor)DrawCursor(dc,region);
            if(!GdiFlush())throw Error("GdiFlush");
            Marshal.Copy(bits,pixels,0,pixels.Length);
            // BI_RGB has no defined alpha; the visible desktop is opaque SDR.
            for(int i=3;i<pixels.Length;i+=4)pixels[i]=255;
            ImageTransfer.Save(ImageTransfer.Bitmap(region.Width,region.Height,pixels),path);
        }finally{
            if(dc!=0&&previous!=0&&previous!=(nint)(-1))SelectObject(dc,previous);
            if(bitmap!=0)DeleteObject(bitmap);
            if(dc!=0)DeleteDC(dc);
            if(screen!=0)ReleaseDC(0,screen);
        }
    }
    private static void DrawCursor(nint dc,PixelRect region)
    {
        var cursor=new CursorInfo{Size=(uint)Marshal.SizeOf<CursorInfo>()};
        if(!GetCursorInfo(ref cursor))throw Error("GetCursorInfo");
        if((cursor.Flags&1)==0)return;
        var icon=CopyIcon(cursor.Handle);if(icon==0)throw Error("CopyIcon");
        var info=new IconInfo();
        try{
            if(!GetIconInfo(icon,out info))throw Error("GetIconInfo");
            if(!DrawIconEx(dc,cursor.X-region.X-(int)info.HotspotX,cursor.Y-region.Y-(int)info.HotspotY,icon,0,0,0,0,3))throw Error("DrawIconEx");
        }finally{
            if(info.Mask!=0)DeleteObject(info.Mask);if(info.Color!=0)DeleteObject(info.Color);DestroyIcon(icon);
        }
    }
    private static Win32Exception Error(string api)=>new(Marshal.GetLastWin32Error(),"Không thể chụp pixel màn hình ("+api+"). Hãy kiểm tra desktop đang mở khóa rồi thử lại.");
    [StructLayout(LayoutKind.Sequential)] private struct BitmapInfo{public uint Size;public int Width,Height;public ushort Planes,Bits;public uint Compression,ImageSize;public int XResolution,YResolution;public uint ColorsUsed,ColorsImportant;}
    [StructLayout(LayoutKind.Sequential)] private struct CursorInfo{public uint Size,Flags;public nint Handle;public int X,Y;}
    [StructLayout(LayoutKind.Sequential)] private struct IconInfo{[MarshalAs(UnmanagedType.Bool)]public bool Icon;public uint HotspotX,HotspotY;public nint Mask,Color;}
    [DllImport("user32.dll",SetLastError=true)] private static extern nint GetDC(nint window);
    [DllImport("user32.dll")] private static extern int ReleaseDC(nint window,nint dc);
    [DllImport("gdi32.dll",SetLastError=true)] private static extern nint CreateCompatibleDC(nint dc);
    [DllImport("gdi32.dll",SetLastError=true)] private static extern nint CreateDIBSection(nint dc,ref BitmapInfo info,uint usage,out nint bits,nint section,uint offset);
    [DllImport("gdi32.dll",SetLastError=true)] private static extern nint SelectObject(nint dc,nint obj);
    [DllImport("gdi32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool BitBlt(nint dest,int x,int y,int width,int height,nint source,int sourceX,int sourceY,uint operation);
    [DllImport("gdi32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool GdiFlush();
    [DllImport("gdi32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool DeleteObject(nint obj);
    [DllImport("gdi32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool DeleteDC(nint dc);
    [DllImport("user32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool GetCursorInfo(ref CursorInfo info);
    [DllImport("user32.dll",SetLastError=true)] private static extern nint CopyIcon(nint icon);
    [DllImport("user32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool GetIconInfo(nint icon,out IconInfo info);
    [DllImport("user32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool DrawIconEx(nint dc,int x,int y,nint icon,int width,int height,uint step,nint brush,uint flags);
    [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyIcon(nint icon);
}
