using System.Diagnostics;
using System.IO;
using System.Text.Json;
namespace Captovix;
public sealed class ElementProbeClient(string? executable=null,IReadOnlyList<string>? arguments=null) : IDisposable
{
    private Process? _process;
    public async Task<ElementCandidate[]> QueryAsync(ElementQuery query,CancellationToken cancellation)
    {
        try{
            if(_process==null||_process.HasExited){Dispose();var start=new ProcessStartInfo(executable??Environment.ProcessPath!){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true};foreach(var arg in arguments??new[]{"--element-probe"})start.ArgumentList.Add(arg);_process=Process.Start(start)??throw new IOException("Không mở được bộ nhận diện phần tử.");}
            var process=_process; // Stable reference across awaits; capture may dispose the session.
            using var timeout=CancellationTokenSource.CreateLinkedTokenSource(cancellation);timeout.CancelAfter(1200);
            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(query).AsMemory(),timeout.Token);await process.StandardInput.FlushAsync(timeout.Token);
            var line=await process.StandardOutput.ReadLineAsync().WaitAsync(timeout.Token);return line==null?[]:JsonSerializer.Deserialize<ElementCandidate[]>(line)??[];
        }catch(Exception e)when(e is OperationCanceledException or IOException or InvalidOperationException or JsonException or ObjectDisposedException or System.ComponentModel.Win32Exception){Dispose();return [];}
    }
    public void Dispose(){var process=_process;_process=null;if(process==null)return;try{if(!process.HasExited)process.Kill(true);}catch{}process.Dispose();}
}
