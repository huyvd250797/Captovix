using System.IO;
using System.Diagnostics;
using System.Windows;
namespace Captovix;
public static class RecoveryWorker
{
    public static int Run(string[] args){try{if(args.Length!=3||!int.TryParse(args[2],out int parent))throw new InvalidDataException("Lệnh khôi phục không hợp lệ.");try{using var process=Process.GetProcessById(parent);if(!process.WaitForExit(60000))throw new IOException("App chưa đóng; chưa khôi phục dữ liệu.");}catch(ArgumentException){}
        using var mutex=new Mutex(false,@"Local\HuyVo.Captovix");bool locked=false;try{try{locked=mutex.WaitOne(0);}catch(AbandonedMutexException){locked=true;}if(!locked)throw new IOException("Một phiên Captovix vẫn đang chạy.");string safety=UpdateBackup.Restore(App.DataRoot,args[1],VersionInfo.Current);ReleaseRecoveryState.Write(App.DataRoot,new(VersionInfo.Current,VersionInfo.Current,safety,"Restored",DateTimeOffset.UtcNow));}finally{if(locked)mutex.ReleaseMutex();}
        Process.Start(new ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=true});return 0;
    }catch(Exception ex){App.Log(ex);MessageBox.Show("Chưa khôi phục được dữ liệu: "+ex.Message+"\nDữ liệu hiện có được giữ hoặc hoàn tác từ bản an toàn. Xem errors.log.","Captovix · Khôi phục",MessageBoxButton.OK,MessageBoxImage.Warning);return 1;}}
}
