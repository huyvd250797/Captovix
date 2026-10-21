using System.IO;
using Captovix;
internal static class Program
{
    [STAThread]private static int Main()
    {
        var folder=Path.Combine(Path.GetTempPath(),"Captovix-Fidelity-"+Guid.NewGuid().ToString("N"));
        try{var results=FidelityWindowsProbe.CheckRendering(folder);foreach(var result in results)Console.WriteLine($"{(result.Passed?"PASS":"FAIL")}: {result.Name}: {result.Detail}");Console.WriteLine($"{results.Count} Windows fidelity renderer/PNG checks; desktop capture/DPI/clipboard still require interactive Windows probe.");return results.All(r=>r.Passed)?0:1;}
        catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
        finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}
    }
}
