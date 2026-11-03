namespace Captovix;
public static class ScrollQuality
{
    public static bool Settled(ScrollFrame a,ScrollFrame b){a.Validate();b.Validate();if(a.Width!=b.Width||a.Height!=b.Height)return false;if(a.Pixels.AsSpan().SequenceEqual(b.Pixels))return true;
        // Tile-local checks prevent a moving small loading panel from being diluted by the rest of a large page.
        for(int ty=0;ty<8;ty++)for(int tx=0;tx<8;tx++){double error=0;int changed=0;for(int y=0;y<8;y++)for(int x=0;x<8;x++){int px=Math.Min(a.Width-1,(int)((tx+(x+.5)/8)*a.Width/8)),py=Math.Min(a.Height-1,(int)((ty+(y+.5)/8)*a.Height/8));int at=(py*a.Width+px)*4,d=0;for(int c=0;c<3;c++)d+=Math.Abs(a.Pixels[at+c]-b.Pixels[at+c]);error+=d/3d;if(d>36)changed++;}if(error/64>1||changed>1)return false;}return true;
    }
    public static (int Header,int Footer) FixedBands(ScrollFrame a,ScrollFrame b){a.Validate();b.Validate();if(a.Width!=b.Width||a.Height!=b.Height)return (0,0);int limit=Math.Min(160,Math.Min(a.Height/3,Math.Max(0,(a.Height-32)/2)));
        bool Same(int y){for(int x=0;x<a.Width;x+=Math.Max(1,a.Width/160)){int i=(y*a.Width+x)*4;for(int c=0;c<3;c++)if(Math.Abs(a.Pixels[i+c]-b.Pixels[i+c])>1)return false;}return true;}
        bool Detail(int start,int count){int detailed=0;for(int y=start;y<start+count;y++){int lo=255,hi=0;for(int x=0;x<a.Width;x+=Math.Max(1,a.Width/80)){int i=(y*a.Width+x)*4;int value=(a.Pixels[i]+a.Pixels[i+1]+a.Pixels[i+2])/3;lo=Math.Min(lo,value);hi=Math.Max(hi,value);}if(hi-lo>16)detailed++;}return detailed>=3;}
        int top=0,bottom=0;while(top<limit&&Same(top))top++;while(bottom<limit&&Same(a.Height-1-bottom))bottom++;if(top<6||!Detail(0,top))top=0;if(bottom<6||!Detail(a.Height-bottom,bottom))bottom=0;return (top,bottom);
    }
}
