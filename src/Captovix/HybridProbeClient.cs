using System.Diagnostics;
using System.IO;
using System.Text.Json;
namespace Captovix;
public sealed class HybridProbeClient(string? executable=null,IReadOnlyList<string>? arguments=null)
{
    public async Task<HybridNode[]> QueryAsync(HybridQuery query,CancellationToken cancellation){
        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(cancellation);timeout.CancelAfter(1400);Process? process=null;
        try{
            var start=new ProcessStartInfo(executable??Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true};foreach(var arg in arguments??["--hybrid-ui-probe"])start.ArgumentList.Add(arg);
            process=Process.Start(start)??throw new IOException("Không mở được bộ đọc UI.");
            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(query).AsMemory(),timeout.Token);process.StandardInput.Close();
            var line=await process.StandardOutput.ReadLineAsync(timeout.Token);if(line==null||line.Length>HybridCapture.MaxJsonBytes)return [];
            var nodes=JsonSerializer.Deserialize<HybridNode[]>(line)??[];return nodes.Length<=HybridCapture.MaxNodes&&nodes.All(n=>n!=null&&n.Id!=null&&n.Id.Length<=100&&n.Name!=null&&n.Name.Length<=200&&n.Font!=null&&n.Font.Length<=100&&n.Type!=null&&n.Type.Length<=50&&HybridCapture.Valid(n.Bounds))?nodes:[];
        }catch(Exception e)when(e is OperationCanceledException or IOException or InvalidOperationException or JsonException or System.ComponentModel.Win32Exception){return [];}
        finally{if(process!=null){try{if(!process.HasExited)process.Kill(true);}catch{}process.Dispose();}}
    }
}
