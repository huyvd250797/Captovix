namespace Captovix;
// Compatibility entry point. Both modes now use the exact same overlay/session.
public static class ElementPicker
{
    public static Task<PixelRect?> PickAsync(IReadOnlyList<DisplayInfo> displays,CancellationToken cancellation)=>RegionPicker.PickAsync(displays,cancellation,true,true);
}
