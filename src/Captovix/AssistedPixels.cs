namespace Captovix;
public sealed record BackgroundSample(string Color,int Spread);
public static class AssistedPixels
{
    public static BackgroundSample Sample(byte[] pixels,int width,int height,PixelRect region)
    {
        if(width<1||height<1||pixels.LongLength!=(long)width*height*4||region.X<0||region.Y<0||region.Width<2||region.Height<2||region.Right>width||region.Bottom>height)throw new ArgumentException("Vùng lấy mẫu nền không hợp lệ.");
        var samples=new List<(int B,int G,int R)>();
        void Add(int x,int y){int i=(y*width+x)*4,a=pixels[i+3];samples.Add(((pixels[i]*a+255*(255-a))/255,(pixels[i+1]*a+255*(255-a))/255,(pixels[i+2]*a+255*(255-a))/255));}
        int step=Math.Max(1,(region.Width+region.Height)/256);
        for(int x=region.X;x<region.Right;x+=step){Add(x,region.Y);Add(x,region.Bottom-1);}
        for(int y=region.Y;y<region.Bottom;y+=step){Add(region.X,y);Add(region.Right-1,y);}
        int Median(Func<(int B,int G,int R),int> get)=>samples.Select(get).Order().ElementAt(samples.Count/2);
        int b=Median(v=>v.B),g=Median(v=>v.G),r=Median(v=>v.R);int spread=samples.Select(v=>Math.Max(Math.Abs(v.B-b),Math.Max(Math.Abs(v.G-g),Math.Abs(v.R-r)))).Order().ElementAt(samples.Count*3/4);
        return new($"#{r:X2}{g:X2}{b:X2}",spread);
    }
}
public sealed class UpdatePromptPolicy
{
    private readonly HashSet<string> _seen=[];
    public void Release(string version)=>_seen.Remove(version);
    public bool Offer(string? version,bool available,bool blocked)=>available&&!blocked&&!string.IsNullOrWhiteSpace(version)&&_seen.Add(version);
}
