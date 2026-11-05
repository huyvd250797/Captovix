namespace Captovix;
public static class ScanSelection
{
    // Explicit user choice: convert estimates too; do not falsely certify their fonts.
    public static int SelectAll(IEnumerable<ScanElement> elements){var accepted=new List<ScanElement>();foreach(var e in elements.OrderByDescending(e=>e.UiSourceId!=null).ThenByDescending(e=>e.Confidence)){e.Include=!accepted.Any(x=>ElementScan.Overlap(x.Bounds,e.Bounds)>.90);if(e.Include){e.KeepImage=false;accepted.Add(e);}}return accepted.Count;}
}
