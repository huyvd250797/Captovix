namespace Captovix;
public static class RecentSelection
{
    public static List<T> Select<T>(IEnumerable<T> source,Func<T,DateTimeOffset> date,T? pin=null,int limit=50) where T:class
    {
        if(limit<1||limit>50)throw new ArgumentOutOfRangeException(nameof(limit));var all=source.Distinct().ToArray();var recent=all.OrderByDescending(date).Take(limit).ToList();if(pin!=null&&all.Contains(pin)&&!recent.Contains(pin)){if(recent.Count==limit)recent.RemoveAt(limit-1);recent.Add(pin);}return recent;
    }
}
