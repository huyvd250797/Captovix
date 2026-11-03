using Captovix.Licensing;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=2){Console.WriteLine("Interop harness: emit PATH / verify PATH. Invoked by serverless integration tests; no test keys are installed in Captovix.");return;}
if(args[0]=="emit"){
 using var device=RSA.Create(2048);var id=Protocol.Hash("interop-"+Guid.NewGuid());var proof=Protocol.SignRequest(device,"trial",new(id,"Máy kiểm thử · C#"),DateTimeOffset.UtcNow);
 File.WriteAllText(args[1],JsonSerializer.Serialize(new{proof,deviceId=id,publicKeyHash=Protocol.Hash(device.ExportSubjectPublicKeyInfoPem())},Protocol.Json));
 Console.WriteLine("PASS .NET device proof emitted");
}else if(args[0]=="verify"){
 using var input=JsonDocument.Parse(File.ReadAllText(args[1]));var root=input.RootElement;var envelope=root.GetProperty("reply").GetProperty("certificate").Deserialize<Envelope>(Protocol.Json)!;
 var claims=Protocol.Verify(envelope,root.GetProperty("publicKey").GetString()!,"test",root.GetProperty("deviceId").GetString()!,root.GetProperty("publicKeyHash").GetString()!);
 if(!Protocol.FullAccess(claims,DateTimeOffset.UtcNow))throw new Exception("Expected active interoperable lease");Console.WriteLine("PASS TypeScript certificate verified by production .NET protocol");
}else throw new Exception("Unknown test mode");
