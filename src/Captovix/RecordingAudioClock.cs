using System.Diagnostics;

namespace Captovix;

internal readonly record struct AudioSlice(int Offset,int Count,long OutputFrame);

internal sealed class RecordingAudioClock
{
    private readonly object _gate=new();
    private Span[] _spans=[];
    private bool _finished;
    private readonly record struct Span(long Start,long End,long Output);

    internal static long Now=>Stopwatch.GetElapsedTime(0,Stopwatch.GetTimestamp()).Ticks;
    internal void Resume(long time)
    {
        lock(_gate){
            var spans=_spans;
            if(_finished||spans.Length>0&&spans[^1].End==long.MaxValue)return;
            long output=spans.Length==0?0:spans[^1].Output+spans[^1].End-spans[^1].Start;
            Volatile.Write(ref _spans,[..spans,new Span(time,long.MaxValue,output)]);
        }
    }

    internal void Pause(long time,bool finish=false)
    {
        lock(_gate){
            _finished|=finish;
            var spans=_spans;
            if(spans.Length==0||spans[^1].End!=long.MaxValue)return;
            var closed=(Span[])spans.Clone();
            closed[^1]=closed[^1] with{End=Math.Max(time,closed[^1].Start)};
            Volatile.Write(ref _spans,closed);
        }
    }

    internal long? ActiveElapsed(long time)
    {
        var spans=Volatile.Read(ref _spans);
        if(spans.Length==0||spans[^1].End!=long.MaxValue||time<spans[^1].Start)return null;
        var span=spans[^1];
        return span.Output+time-span.Start;
    }

    internal IEnumerable<AudioSlice> Place(long timestamp,int frameCount,int rate)
    {
        const long ticks=TimeSpan.TicksPerSecond;
        long end=timestamp+(long)Math.Ceiling(frameCount*(double)ticks/rate);
        foreach(var span in Volatile.Read(ref _spans)){
            long from=Math.Max(timestamp,span.Start),to=Math.Min(end,span.End);
            if(from>=to)continue;
            int first=(int)Math.Clamp(Math.Ceiling((from-timestamp)*(double)rate/ticks),0,frameCount);
            int last=(int)Math.Clamp(Math.Ceiling((to-timestamp)*(double)rate/ticks),0,frameCount);
            if(last<=first)continue;
            var output=(long)Math.Round((timestamp-span.Start+span.Output)*(double)rate/ticks)+first;
            yield return new(first,last-first,Math.Max(0,output));
        }
    }
}
