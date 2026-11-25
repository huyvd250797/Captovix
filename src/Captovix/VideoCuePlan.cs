namespace Captovix;

internal sealed record VideoCue(int SourceIndex,TimeSpan At,string Kind,double X=.5,double Y=.5,string Text="")
{
    internal bool IsValid(int count)=>SourceIndex>=0&&SourceIndex<count&&At>=TimeSpan.Zero
        &&Kind is "left" or "right" or "middle" or "ring" or "key" or "marker"
        &&X is >=0 and <=1&&Y is >=0 and <=1&&Text is {Length:<=48}
        &&!Text.Any(char.IsControl);
    internal bool HasPosition=>Kind is "left" or "right" or "middle" or "ring";
    internal double Seconds=>Kind=="key"?1.25:Kind=="ring"?1.1:.55;
    internal string Color=>Kind switch{"right"=>"#56BBFF","middle"=>"#C7A1FF","ring"=>"#FFF2A1",_=>"#FFD34F"};
}

internal sealed record VideoCueSpan(VideoCue Cue,VideoRange Clip,TimeSpan ClipAt,TimeSpan Start,TimeSpan End);

internal static class VideoCuePlan
{
    internal static VideoCueSpan[] Create(IEnumerable<VideoRange> clips,IEnumerable<VideoCue> cues)
    {
        var bySource=cues.ToLookup(cue=>cue.SourceIndex);
        var spans=new List<VideoCueSpan>();
        var offset=TimeSpan.Zero;
        foreach(var clip in clips){
            foreach(var cue in bySource[clip.SourceIndex]){
                if(cue.At<clip.Start||cue.At>=clip.End)continue;
                var start=offset+clip.LocalAt(cue.At);
                var end=start+TimeSpan.FromSeconds(cue.Seconds);
                if(end>offset+clip.Duration)end=offset+clip.Duration;
                if(start<end)spans.Add(new(cue,clip,offset,start,end));
            }
            offset+=clip.Duration;
        }
        return spans.OrderBy(span=>span.Start).ToArray();
    }

    internal static (double X,double Y)? Point(VideoCueSpan span,TimeSpan time,VideoCanvasGeometry geometry,int sourceWidth,int sourceHeight,int width,int height)
    {
        var crop=geometry.Crop;
        double x=(span.Cue.X*sourceWidth-crop.X)/crop.Width;
        double y=(span.Cue.Y*sourceHeight-crop.Y)/crop.Height;
        if(x<0||x>1||y<0||y>1)return null;
        (x,y)=geometry.Rotation switch{90=>(1-y,x),180=>(1-x,1-y),270=>(y,1-x),_=>(x,y)};
        var image=geometry.Image;
        x=(image.X+x*image.Width)/width;
        y=(image.Y+y*image.Height)/height;
        var key=span.Clip.Timing.At(span.Clip.Fraction(time-span.ClipAt));
        var origin=VideoMotion.Origin(key.Zoom,key.X,key.Y);
        x=(x-origin.X)*key.Zoom;
        y=(y-origin.Y)*key.Zoom;
        return x is >=0 and <=1&&y is >=0 and <=1?(x*width,y*height):null;
    }
}
