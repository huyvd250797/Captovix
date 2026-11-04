using System.Collections.Concurrent;
using System.Threading;
namespace Captovix;
/// <summary>One bounded STA worker for offscreen WPF rendering; never calls the UI dispatcher.</summary>
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
        internal Worker(){var thread=new Thread(()=>{foreach(var item in Queue.GetConsumingEnumerable()){try{item.Action!();item.Completion.TrySetResult();}catch(Exception ex){item.Completion.TrySetException(ex);}finally{item.Action=null;}}System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();}){IsBackground=true,Name="Captovix image save"};thread.SetApartmentState(ApartmentState.STA);thread.Start();}
    }
    private static readonly Lazy<Worker> Instance=new(()=>new Worker());
    public static Task Run(Action action){var item=new WorkItem(action);if(!Instance.Value.Queue.TryAdd(item))throw new InvalidOperationException("Hàng đợi lưu ảnh đang bận; thử lưu lại.");return item.Completion.Task;}
    public static void Stop(){if(Instance.IsValueCreated)Instance.Value.Queue.CompleteAdding();}
}
