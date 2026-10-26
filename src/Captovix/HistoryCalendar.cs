namespace Captovix;
public sealed class HistoryCalendar<T>
{
    private readonly Dictionary<DateOnly,T[]> _days;
    public int[] Years {get;}
    public HistoryCalendar(IEnumerable<T> source,Func<T,DateTimeOffset> created,int currentYear)
    {
        if(currentYear<1||currentYear>9999)throw new ArgumentOutOfRangeException(nameof(currentYear));_days=source.GroupBy(x=>DateOnly.FromDateTime(created(x).LocalDateTime)).ToDictionary(g=>g.Key,g=>g.ToArray());Years=_days.Keys.Select(d=>d.Year).Append(currentYear).Distinct().OrderDescending().ToArray();
    }
    public IReadOnlyList<T> Items(int year,int month,int day)=>_days.GetValueOrDefault(new(year,month,day))??[];
    public int Count(int year,int month=0)=>_days.Where(p=>p.Key.Year==year&&(month==0||p.Key.Month==month)).Sum(p=>p.Value.Length);
    public static int[] Months()=>Enumerable.Range(1,12).ToArray();
    public static int[] Days(int year,int month)=>Enumerable.Range(1,DateTime.DaysInMonth(year,month)).ToArray();
}
