namespace Captovix;

// CPU pixel operations shared by preview/export. Input is straight BGRA32; output is always opaque.
public static class RedactionPixels
{
    public static byte[] Apply(byte[] pixels,int width,int height,int strength,bool pixelate)
    {
        if(width<1||height<1||pixels.Length!=checked(width*height*4)||strength<4||strength>64)throw new ArgumentException("Invalid pixel region.");
        var opaque=new byte[pixels.Length];
        for(var i=0;i<pixels.Length;i+=4){int alpha=pixels[i+3];for(var c=0;c<3;c++)opaque[i+c]=(byte)((pixels[i+c]*alpha+255*(255-alpha)+127)/255);opaque[i+3]=255;}
        if(pixelate){
            for(var y=0;y<height;y+=strength)for(var x=0;x<width;x+=strength){
                var right=Math.Min(width,x+strength);var bottom=Math.Min(height,y+strength);var count=(right-x)*(bottom-y);long b=0,g=0,r=0;
                for(var yy=y;yy<bottom;yy++)for(var xx=x;xx<right;xx++){var i=(yy*width+xx)*4;b+=opaque[i];g+=opaque[i+1];r+=opaque[i+2];}
                for(var yy=y;yy<bottom;yy++)for(var xx=x;xx<right;xx++){var i=(yy*width+xx)*4;opaque[i]=(byte)(b/count);opaque[i+1]=(byte)(g/count);opaque[i+2]=(byte)(r/count);}
            }
            return opaque;
        }
        // Separable box blur with truncated windows. Work is O(pixels), independent of radius.
        var horizontal=new byte[opaque.Length];var result=new byte[opaque.Length];var radius=strength;
        for(var y=0;y<height;y++)for(var c=0;c<3;c++){
            int sum=0;for(var x=0;x<=Math.Min(radius,width-1);x++)sum+=opaque[(y*width+x)*4+c];
            for(var x=0;x<width;x++){
                var count=Math.Min(width-1,x+radius)-Math.Max(0,x-radius)+1;horizontal[(y*width+x)*4+c]=(byte)(sum/count);
                if(x-radius>=0)sum-=opaque[(y*width+x-radius)*4+c];
                if(x+radius+1<width)sum+=opaque[(y*width+x+radius+1)*4+c];
            }
        }
        for(var x=0;x<width;x++)for(var c=0;c<3;c++){
            int sum=0;for(var y=0;y<=Math.Min(radius,height-1);y++)sum+=horizontal[(y*width+x)*4+c];
            for(var y=0;y<height;y++){
                var count=Math.Min(height-1,y+radius)-Math.Max(0,y-radius)+1;result[(y*width+x)*4+c]=(byte)(sum/count);
                if(y-radius>=0)sum-=horizontal[((y-radius)*width+x)*4+c];
                if(y+radius+1<height)sum+=horizontal[((y+radius+1)*width+x)*4+c];
            }
        }
        for(var i=3;i<result.Length;i+=4)result[i]=255;return result;
    }
}
