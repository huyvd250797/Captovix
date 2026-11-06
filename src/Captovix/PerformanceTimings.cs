using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Captovix;

public sealed record PerformanceTiming(string Operation,double Milliseconds);

public static class PerformanceTimings
{
    private const int Limit=512;
    private static readonly object Gate=new();
    private static readonly Dictionary<string,Queue<double>> Samples=new(StringComparer.Ordinal);
    private static readonly IDisposable Disabled=new NoTiming();
    public static bool IsEnabled { get; }=Environment.GetEnvironmentVariable("CAPTOVIX_MEASURE_PERFORMANCE")=="1";

    public static void Record(string name,double ms)
    {
        if(!IsEnabled)return;
        lock(Gate){
            if(!Samples.TryGetValue(name,out var items))Samples[name]=items=new();
            if(items.Count==Limit)items.Dequeue();
            items.Enqueue(ms);
        }
    }

    public static PerformanceTiming[] Snapshot()
    {
        lock(Gate)return Samples.SelectMany(entry=>entry.Value.Select(ms=>new PerformanceTiming(entry.Key,ms))).ToArray();
    }

    public static IDisposable Measure(string name)=>IsEnabled?new Timing(name):Disabled;

    internal static void WriteReport()
    {
        if(!IsEnabled)return;
        try{
            Directory.CreateDirectory(App.DataRoot);
            string path=Path.Combine(App.DataRoot,"performance-latest.json");
            var json=JsonSerializer.Serialize(new{Version=VersionInfo.Current,At=DateTimeOffset.UtcNow,Samples=Snapshot()});
            MediaStore.AtomicWrite(path,json);
        }catch(Exception ex){App.Log(ex);}
    }

    private sealed class NoTiming:IDisposable { public void Dispose(){} }
    private sealed class Timing(string name):IDisposable
    {
        readonly long start=Stopwatch.GetTimestamp();
        public void Dispose()=>Record(name,Stopwatch.GetElapsedTime(start).TotalMilliseconds);
    }
}
