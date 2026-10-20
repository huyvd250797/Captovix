namespace Captovix;

public readonly record struct PixelRect(int X, int Y, int Width, int Height)
{
    public int Right => X + Width;
    public int Bottom => Y + Height;
    public bool IsEmpty => Width <= 0 || Height <= 0;
    public PixelRect Intersect(PixelRect b) => new(Math.Max(X,b.X),Math.Max(Y,b.Y),Math.Max(0,Math.Min(Right,b.Right)-Math.Max(X,b.X)),Math.Max(0,Math.Min(Bottom,b.Bottom)-Math.Max(Y,b.Y)));
    public static PixelRect FromPoints(int x1,int y1,int x2,int y2) => new(Math.Min(x1,x2),Math.Min(y1,y2),Math.Abs(x2-x1),Math.Abs(y2-y1));
    public PixelRect Even() => this with {Width = Width - Width % 2, Height = Height - Height % 2};
    public override string ToString() => $"{Width} × {Height} px · ({X}, {Y})";
}
public record SourcePlacement(string Device, PixelRect Crop, int X, int Y);
public static class CaptureGeometry
{
    // Input/output positions are always physical pixels, including negative monitor origins.
    public static List<SourcePlacement> Split(PixelRect region, IEnumerable<(string Device, PixelRect Bounds)> screens)
    {
        var result = new List<SourcePlacement>();
        foreach (var (device,bounds) in screens) {
            var part = region.Intersect(bounds);
            if (!part.IsEmpty) result.Add(new(device,new(part.X-bounds.X,part.Y-bounds.Y,part.Width,part.Height),part.X-region.X,part.Y-region.Y));
        }
        return result;
    }
}
