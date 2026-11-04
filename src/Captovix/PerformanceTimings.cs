using System.Collections.Concurrent;
using System.Diagnostics;
namespace Captovix;
public sealed record PerformanceTiming(string Operation,double Milliseconds);
public static class PerformanceTimings
{
    private static readonly ConcurrentQueue<PerformanceTiming> Samples=new();
    public static void Record(string name,double ms){Samples.Enqueue(new(name,ms));while(Samples.Count>128)Samples.TryDequeue(out _);}
    public static PerformanceTiming[] Snapshot()=>Samples.ToArray();
    public static IDisposable Measure(string name)=>new Timing(name);
    private sealed class Timing(string name):IDisposable
    {
        readonly long start=Stopwatch.GetTimestamp();
        public void Dispose()=>Record(name,Stopwatch.GetElapsedTime(start).TotalMilliseconds);
    }
}
