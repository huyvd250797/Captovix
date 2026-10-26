using System.Collections.Concurrent;
namespace Captovix;
public sealed record RenderSample(int Width,int Height,int Annotations,double Milliseconds,int Thread);
public static class RenderMetrics
{
    private static readonly ConcurrentQueue<RenderSample> Recent=new();
    public static void Add(RenderSample sample){Recent.Enqueue(sample);while(Recent.Count>64)Recent.TryDequeue(out _);}
    public static RenderSample[] Snapshot()=>Recent.ToArray();
}
