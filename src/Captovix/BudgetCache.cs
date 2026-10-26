namespace Captovix;
public sealed record CacheSnapshot(int Count,long Bytes,long Hits,long Misses);
public sealed class BudgetCache<TKey,TValue> where TKey:notnull
{
    private readonly object _gate=new();private readonly Dictionary<TKey,LinkedListNode<(TKey Key,TValue Value,long Bytes)>> _map;private readonly LinkedList<(TKey Key,TValue Value,long Bytes)> _order=[];private readonly long _budget;private readonly int _limit;private long _bytes,_hits,_misses;
    public BudgetCache(long budget,int limit=128,IEqualityComparer<TKey>? comparer=null){if(budget<1||limit<1)throw new ArgumentOutOfRangeException();_budget=budget;_limit=limit;_map=new(comparer);}
    public CacheSnapshot Stats {get{lock(_gate)return new(_map.Count,_bytes,_hits,_misses);}}
    public bool TryGet(TKey key,out TValue value){lock(_gate){if(_map.TryGetValue(key,out var node)){_order.Remove(node);_order.AddLast(node);_hits++;value=node.Value.Value;return true;}_misses++;value=default!;return false;}}
    public List<TValue> Add(TKey key,TValue value,long bytes)
    {
        if(bytes<0)throw new ArgumentOutOfRangeException(nameof(bytes));lock(_gate){var evicted=new List<TValue>();if(_map.Remove(key,out var old)){_bytes-=old.Value.Bytes;_order.Remove(old);}if(bytes>_budget)return evicted;var node=_order.AddLast((key,value,bytes));_map.Add(key,node);_bytes+=bytes;while(_bytes>_budget||_map.Count>_limit){var first=_order.First!;_order.RemoveFirst();_map.Remove(first.Value.Key);_bytes-=first.Value.Bytes;evicted.Add(first.Value.Value);}return evicted;}
    }
    public void Clear(){lock(_gate){_map.Clear();_order.Clear();_bytes=0;}}
}
