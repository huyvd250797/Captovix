using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
namespace Captovix;
internal static class NativeColorPicker
{
    [StructLayout(LayoutKind.Sequential)] private struct ChooseColorData { public uint Size;public nint Owner;public nint Instance;public uint Rgb;public nint CustomColors;public uint Flags;public nint UserData;public nint Hook;public nint Template; }
    [DllImport("comdlg32.dll",CharSet=CharSet.Unicode,EntryPoint="ChooseColorW")] [return:MarshalAs(UnmanagedType.Bool)] private static extern bool ChooseColor(ref ChooseColorData data);
    private static readonly int[] CustomColors=Enumerable.Repeat(0xFFFFFF,16).ToArray();
    public static string? Pick(Window owner)
    {
        var pin=GCHandle.Alloc(CustomColors,GCHandleType.Pinned);try{var data=new ChooseColorData{Size=(uint)Marshal.SizeOf<ChooseColorData>(),Owner=new WindowInteropHelper(owner).Handle,CustomColors=pin.AddrOfPinnedObject(),Flags=3,Rgb=0x928B00};return ChooseColor(ref data)?$"#{data.Rgb&255:X2}{(data.Rgb>>8)&255:X2}{(data.Rgb>>16)&255:X2}":null;}finally{pin.Free();}
    }
}
