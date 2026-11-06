using System.Collections.Concurrent;
using System.Threading;
namespace Captovix;
/// <summary>Bounded STA work for offscreen WPF rendering, with a separate export queue.</summary>
public static class StaImageWorker
{
    private sealed class WorkItem(Action action)
    {
        public Action? Action=action;
        public readonly TaskCompletionSource Completion=new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private sealed class Worker
    {
        internal readonly BlockingCollection<WorkItem> Queue=new(8);
        internal Worker(string name){var thread=new Thread(()=>{foreach(var item in Queue.GetConsumingEnumerable()){try{item.Action!();item.Completion.TrySetResult();}catch(Exception ex){item.Completion.TrySetException(ex);}finally{item.Action=null;}}System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();}){IsBackground=true,Name=name};thread.SetApartmentState(ApartmentState.STA);thread.Start();}
    }
    private static readonly Lazy<Worker> Instance=new(()=>new Worker("Captovix image save"));
    private static readonly Lazy<Worker> Export=new(()=>new Worker("Captovix image export"));

    public static Task Run(Action action)=>Enqueue(Instance.Value,action);
    internal static Task RunExport(Action action)=>Enqueue(Export.Value,action);
    private static Task Enqueue(Worker worker,Action action)
    {
        var item=new WorkItem(action);
        if(!worker.Queue.TryAdd(item))throw new InvalidOperationException("Hàng đợi xử lý ảnh đang bận; hãy thử lại.");
        return item.Completion.Task;
    }

    public static void Stop()
    {
        if(Instance.IsValueCreated)Instance.Value.Queue.CompleteAdding();
        if(Export.IsValueCreated)Export.Value.Queue.CompleteAdding();
    }
}
