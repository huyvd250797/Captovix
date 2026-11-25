namespace Captovix;

/// <summary>Quantize cumulative boundaries, never each cut independently.</summary>
internal static class VideoExportTiming
{
    internal static long FrameAt(TimeSpan timeline,double fps)=>checked((long)Math.Round(timeline.TotalSeconds*fps,MidpointRounding.AwayFromZero));
    internal static long Frames(TimeSpan start,TimeSpan duration,double fps)=>FrameAt(start+duration,fps)-FrameAt(start,fps);
}
