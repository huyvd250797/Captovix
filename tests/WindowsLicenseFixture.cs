// TEST ASSEMBLIES ONLY. Not referenced or compiled by the production app.
// Existing editor UI checks need licensed state without a real customer license.
// Generate a fresh test authority and verified in-memory certificate per process.
// No private key/device cache is installed; _device stays null, so clock checkpoint
// and refresh cannot write production license files or contact any server.
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Captovix.Licensing;
internal static class WindowsLicenseFixture
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        var assembly=typeof(Captovix.MainWindow).Assembly;
        var runtime=assembly.GetType("Captovix.LicenseRuntime",true)!;
        var configuration=assembly.GetType("Captovix.LicenseConfiguration",true)!;
        using var authority=RSA.Create(3072);using var device=RSA.Create(2048);var now=DateTimeOffset.UtcNow;
        var id=Protocol.Hash("isolated-editor-fixture");var keyHash=Protocol.Hash(device.ExportSubjectPublicKeyInfoPem());
        var certificate=Protocol.Sign(authority,new(1,Protocol.Product,"test-only","fixture","fixture",id,keyHash,"perpetual","active",now,now.AddDays(1),null));
        var claims=Protocol.Verify(certificate,authority.ExportSubjectPublicKeyInfoPem(),"test-only",id,keyHash);
        void Set(string name,object? value)=>runtime.GetField(name,BindingFlags.Static|BindingFlags.NonPublic)!.SetValue(null,value);
        Set("_config",Activator.CreateInstance(configuration,"https://license.example.invalid","test-only",authority.ExportSubjectPublicKeyInfoPem()));
        Set("_claims",claims);Set("_deviceId",id);Set("_highWater",now);Set("_anchor",now);Set("_tick",System.Diagnostics.Stopwatch.GetTimestamp());Set("_identityReady",false);Set("_device",null);Set("_ready",true);
    }
}
