namespace Captovix;
// BGRA operations preserve dimensions and alpha unless explicitly changing the mask.
public static class ImageStudioPixels
{
    private static void Validate(byte[] p,int w,int h){if(w<1||h<1||(long)w*h>40_000_000||p.LongLength!=(long)w*h*4)throw new ArgumentException("Ảnh tối đa 40 MP.");}
    private static byte Byte(double v)=>(byte)Math.Clamp((int)Math.Round(v),0,255);
    public static byte[] Adjust(byte[] p,int w,int h,double brightness,double contrast,double temperature,double saturation,double sharpen,CancellationToken token=default)
    {
        Validate(p,w,h);if(new[]{brightness,contrast,temperature,saturation,sharpen}.Any(x=>!double.IsFinite(x))||Math.Abs(brightness)>100||Math.Abs(contrast)>100||Math.Abs(temperature)>100||Math.Abs(saturation)>100||sharpen<0||sharpen>2)throw new ArgumentException("Mức chỉnh ngoài giới hạn.");
        var q=(byte[])p.Clone();double c=Math.Pow(2,contrast/50),sat=1+saturation/100;
        for(int y=0;y<h;y++){token.ThrowIfCancellationRequested();for(int x=0;x<w;x++){int i=(y*w+x)*4;double b=p[i]-temperature*.45,g=p[i+1],r=p[i+2]+temperature*.45,l=.0722*b+.7152*g+.2126*r;q[i]=Byte((l+(b-l)*sat-127.5)*c+127.5+brightness*2.55);q[i+1]=Byte((l+(g-l)*sat-127.5)*c+127.5+brightness*2.55);q[i+2]=Byte((l+(r-l)*sat-127.5)*c+127.5+brightness*2.55);}}
        if(sharpen==0)return q;var result=(byte[])q.Clone();for(int y=1;y<h-1;y++){token.ThrowIfCancellationRequested();for(int x=1;x<w-1;x++){int i=(y*w+x)*4;if(q[i+3]!=255)continue;for(int k=0;k<3;k++){double sum=0;for(int yy=-1;yy<=1;yy++)for(int xx=-1;xx<=1;xx++)sum+=q[((y+yy)*w+x+xx)*4+k];result[i+k]=Byte(q[i+k]+sharpen*(q[i+k]-sum/9));}}}return result;
    }
    public static byte[] CloneBrush(byte[] current,byte[] source,int w,int h,int x,int y,int sx,int sy,int radius,bool restore=false)
    {
        Validate(current,w,h);Validate(source,w,h);if(radius<1||radius>300)throw new ArgumentException("Cỡ cọ 1–300 px.");
        for(int yy=-radius;yy<=radius;yy++)for(int xx=-radius;xx<=radius;xx++){
            int dx=x+xx,dy=y+yy,ox=(restore?x:sx)+xx,oy=(restore?y:sy)+yy;if(xx*xx+yy*yy>radius*radius||dx<0||dx>=w||dy<0||dy>=h||ox<0||ox>=w||oy<0||oy>=h)continue;
            int i=(dy*w+dx)*4,j=(oy*w+ox)*4;Array.Copy(source,j,current,i,4);
        }return current;
    }
    public static byte[] RemoveConnected(byte[] p,int w,int h,int x,int y,int tolerance,CancellationToken token=default)
    {
        Validate(p,w,h);if(x<0||x>=w||y<0||y>=h||tolerance<0||tolerance>255)throw new ArgumentException("Vùng tách nền không hợp lệ.");
        var q=(byte[])p.Clone();var seen=new bool[w*h];var queue=new Queue<int>();int seed=(y*w+x)*4;queue.Enqueue(y*w+x);seen[y*w+x]=true;
        while(queue.Count>0){token.ThrowIfCancellationRequested();int at=queue.Dequeue(),i=at*4;if(Math.Max(Math.Abs(p[i]-p[seed]),Math.Max(Math.Abs(p[i+1]-p[seed+1]),Math.Abs(p[i+2]-p[seed+2])))>tolerance)continue;q[i+3]=0;int px=at%w,py=at/w;void Add(int next){if(!seen[next]){seen[next]=true;queue.Enqueue(next);}}if(px>0)Add(at-1);if(px<w-1)Add(at+1);if(py>0)Add(at-w);if(py<h-1)Add(at+w);}
        return q;
    }
    // Projective mapping from the unit output square to the selected source quad.
    public static byte[] Perspective(byte[] p,int w,int h,(double X,double Y)[] points,CancellationToken token=default)
    {
        Validate(p,w,h);if(points.Length!=4||points.Any(v=>!double.IsFinite(v.X)||!double.IsFinite(v.Y)||v.X<0||v.Y<0||v.X>w-1||v.Y>h-1))throw new ArgumentException("Kéo 4 góc nằm trong ảnh.");
        for(int i=0;i<4;i++){var a=points[i];var b=points[(i+1)%4];var c=points[(i+2)%4];if((b.X-a.X)*(c.Y-b.Y)-(b.Y-a.Y)*(c.X-b.X)<=.01)throw new ArgumentException("Bốn góc phải tạo tứ giác lồi theo thứ tự trái trên, phải trên, phải dưới, trái dưới.");}
        var p0=points[0];var p1=points[1];var p2=points[2];var p3=points[3];double dx1=p1.X-p2.X,dx2=p3.X-p2.X,dy1=p1.Y-p2.Y,dy2=p3.Y-p2.Y,dx3=p0.X-p1.X+p2.X-p3.X,dy3=p0.Y-p1.Y+p2.Y-p3.Y,det=dx1*dy2-dx2*dy1;
        double g=0,k=0;if(Math.Abs(dx3)+Math.Abs(dy3)>.00001){if(Math.Abs(det)<1e-9)throw new ArgumentException("Phối cảnh suy biến.");g=(dx3*dy2-dx2*dy3)/det;k=(dx1*dy3-dx3*dy1)/det;}
        var q=new byte[p.Length];for(int y=0;y<h;y++){token.ThrowIfCancellationRequested();double v=y/(double)Math.Max(1,h-1);for(int x=0;x<w;x++){double u=x/(double)Math.Max(1,w-1),d=g*u+k*v+1;if(Math.Abs(d)<1e-9)continue;double sx=((p1.X-p0.X+g*p1.X)*u+(p3.X-p0.X+k*p3.X)*v+p0.X)/d,sy=((p1.Y-p0.Y+g*p1.Y)*u+(p3.Y-p0.Y+k*p3.Y)*v+p0.Y)/d;Sample(p,q,w,h,sx,sy,(y*w+x)*4);}}return q;
    }
    public static byte[] Rotate(byte[] p,int w,int h,double degrees,CancellationToken token=default)
    {
        Validate(p,w,h);if(!double.IsFinite(degrees)||Math.Abs(degrees)>45)throw new ArgumentException("Chỉnh nghiêng từ -45 đến45 độ.");if(degrees==0)return (byte[])p.Clone();double rad=degrees*Math.PI/180,c=Math.Cos(rad),s=Math.Sin(rad),cx=(w-1)/2d,cy=(h-1)/2d;var q=new byte[p.Length];for(int y=0;y<h;y++){token.ThrowIfCancellationRequested();for(int x=0;x<w;x++){double xx=x-cx,yy=y-cy;Sample(p,q,w,h,c*xx+s*yy+cx,-s*xx+c*yy+cy,(y*w+x)*4);}}return q;
    }
    private static void Sample(byte[] p,byte[] q,int w,int h,double x,double y,int i)
    {
        if(x<-.000001||y<-.000001||x>w-1+.000001||y>h-1+.000001)return;x=Math.Clamp(x,0,w-1);y=Math.Clamp(y,0,h-1);int xx=(int)x,yy=(int)y;double fx=x-xx,fy=y-yy,alpha=0;double[] sum=new double[3];for(int row=0;row<2;row++)for(int col=0;col<2;col++){int j=(Math.Min(h-1,yy+row)*w+Math.Min(w-1,xx+col))*4;double weight=(col==0?1-fx:fx)*(row==0?1-fy:fy),a=p[j+3]*weight;alpha+=a;for(int k=0;k<3;k++)sum[k]+=p[j+k]*a;}q[i+3]=Byte(alpha);if(alpha>0)for(int k=0;k<3;k++)q[i+k]=Byte(sum[k]/alpha);
    }
}
