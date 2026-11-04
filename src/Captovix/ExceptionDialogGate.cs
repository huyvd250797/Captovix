namespace Captovix;
// MessageBox pumps a nested dispatcher. Keep one report active until it closes.
public sealed class ExceptionDialogGate
{
    private int _active;
    public bool Report(Action report)
    {
        if(System.Threading.Interlocked.CompareExchange(ref _active,1,0)!=0)return false;
        try{report();return true;}
        finally{System.Threading.Volatile.Write(ref _active,0);}
    }
}
