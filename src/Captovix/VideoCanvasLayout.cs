namespace Captovix;

internal enum VideoAspect { Source, Landscape, Portrait, Square }
internal enum VideoFit { Fit, Fill }

internal sealed record VideoCanvasLayout(VideoAspect Aspect=VideoAspect.Source,VideoFit Fit=VideoFit.Fit,string Background="#000000")
{
    internal static readonly VideoCanvasLayout Default=new();
    internal bool IsValid()=>Enum.IsDefined(Aspect)&&Enum.IsDefined(Fit)&&IsColor(Background);
    internal static bool IsColor(string? value)=>value is {Length:7}&&value[0]=='#'&&value.Skip(1).All(Uri.IsHexDigit);

    internal (int Width,int Height) OutputSize(int sourceWidth,int sourceHeight)=>Aspect switch{
        VideoAspect.Landscape=>(1920,1080),VideoAspect.Portrait=>(1080,1920),VideoAspect.Square=>(1080,1080),
        _=>(Math.Max(2,sourceWidth&~1),Math.Max(2,sourceHeight&~1))
    };
}

internal sealed record VideoClipLayout(int Rotation=0,double Left=0,double Top=0,double Right=0,double Bottom=0)
{
    internal static readonly VideoClipLayout Default=new();
    internal bool IsValid()=>(Rotation is 0 or 90 or 180 or 270)&&Margin(Left)&&Margin(Top)&&Margin(Right)&&Margin(Bottom)
        &&Left+Right<=.95&&Top+Bottom<=.95;

    internal VideoPixelRect Crop(int width,int height)
    {
        int x=Math.Min(width-1,(int)Math.Floor(width*Left)),y=Math.Min(height-1,(int)Math.Floor(height*Top));
        return new(x,y,Math.Max(1,width-x-(int)Math.Floor(width*Right)),Math.Max(1,height-y-(int)Math.Floor(height*Bottom)));
    }

    private static bool Margin(double value)=>value is >=0 and <=.95;
}

internal readonly record struct VideoPixelRect(int X,int Y,int Width,int Height);

internal sealed record VideoCanvasGeometry(VideoPixelRect Crop,int Rotation,VideoPixelRect Image)
{
    internal static VideoCanvasGeometry Create(VideoCanvasLayout canvas,VideoClipLayout layout,int sourceWidth,int sourceHeight,int width,int height)
    {
        var crop=layout.Crop(sourceWidth,sourceHeight);
        bool sideways=layout.Rotation is 90 or 270;
        int rotatedWidth=sideways?crop.Height:crop.Width,rotatedHeight=sideways?crop.Width:crop.Height;
        double x=(double)width/rotatedWidth,y=(double)height/rotatedHeight;
        if(canvas.Fit==VideoFit.Fill){
            int visibleWidth=Math.Clamp((int)Math.Round(width/Math.Max(x,y)),1,rotatedWidth);
            int visibleHeight=Math.Clamp((int)Math.Round(height/Math.Max(x,y)),1,rotatedHeight);
            int cropWidth=sideways?visibleHeight:visibleWidth,cropHeight=sideways?visibleWidth:visibleHeight;
            crop=new(crop.X+(crop.Width-cropWidth)/2,crop.Y+(crop.Height-cropHeight)/2,cropWidth,cropHeight);
            return new(crop,layout.Rotation,new(0,0,width,height));
        }
        double scale=Math.Min(x,y);
        int w=Even(rotatedWidth*scale),h=Even(rotatedHeight*scale);
        return new(crop,layout.Rotation,new((width-w)/2,(height-h)/2,w,h));
    }

    private static int Even(double value)=>Math.Max(2,(int)Math.Round(value/2)*2);

    internal string Filter(int width,int height,string background)
    {
        var rotate=Rotation switch{90=>"transpose=clock,",180=>"hflip,vflip,",270=>"transpose=cclock,",_=>""};
        string pad=Image.Width==width&&Image.Height==height?"":$"pad={width}:{height}:{Image.X}:{Image.Y}:color=0x{background[1..]},";
        return $"crop={Crop.Width}:{Crop.Height}:{Crop.X}:{Crop.Y}:exact=1,{rotate}scale={Image.Width}:{Image.Height}:flags=bicubic,setsar=1,format=yuv444p,{pad}format=yuv420p";
    }
}
