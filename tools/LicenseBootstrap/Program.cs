using System.Security.Cryptography;
using System.Text.Json;
if(args.Length==2&&args[0]=="--validate"){
 using var config=JsonDocument.Parse(File.ReadAllText(args[1]));var c=config.RootElement;if(c.ValueKind!=JsonValueKind.Object||c.EnumerateObject().Count()!=3||c.EnumerateObject().Any(p=>p.Name is not ("apiUrl" or "issuer" or "publicKeyPem")))throw new ArgumentException("Only apiUrl/issuer/publicKeyPem are allowed in public config.");var api=c.GetProperty("apiUrl").GetString()??"";var issuer=c.GetProperty("issuer").GetString()??"";var pub=c.GetProperty("publicKeyPem").GetString()??"";
 if(!Uri.TryCreate(api,UriKind.Absolute,out var endpoint)||endpoint.Scheme!="https"||endpoint.UserInfo!=""||endpoint.Query!=""||endpoint.Fragment!=""||issuer.Length is <1 or >100||!pub.TrimStart().StartsWith("-----BEGIN PUBLIC KEY-----")||pub.Contains("PRIVATE KEY"))throw new ArgumentException("Invalid PUBLIC license configuration. Use HTTPS API URL, issuer and PUBLIC KEY PEM only.");
 using var check=RSA.Create();check.ImportFromPem(pub);if(check.KeySize<3072)throw new ArgumentException("Public signing key must be >=3072 bits.");Console.WriteLine("PUBLIC_LICENSE_CONFIG_VALID");return;
}
if(args.Length!=2||!Uri.TryCreate(args[0],UriKind.Absolute,out var url)||url.Scheme!="https"||url.UserInfo!=""||url.Query!=""||url.Fragment!="")throw new ArgumentException("Usage: dotnet run --project tools/LicenseBootstrap -- https://license.example.com ABSOLUTE_PRIVATE_DIRECTORY");
var folder=Path.GetFullPath(args[1]);if(!Path.IsPathRooted(args[1])||Directory.Exists(folder)&&Directory.EnumerateFileSystemEntries(folder).Any())throw new ArgumentException("Use a new empty absolute private directory outside the source/release tree. Never overwrite signing keys.");
DirectoryInfo? sourceRoot=new(AppContext.BaseDirectory);while(sourceRoot!=null&&!File.Exists(Path.Combine(sourceRoot.FullName,"src","Captovix","Captovix.csproj")))sourceRoot=sourceRoot.Parent;
var comparison=OperatingSystem.IsWindows()?StringComparison.OrdinalIgnoreCase:StringComparison.Ordinal;
foreach(var blocked in new[]{Directory.GetCurrentDirectory(),sourceRoot?.FullName}.Where(p=>p!=null))if(folder.Equals(blocked,comparison)||folder.StartsWith(blocked+Path.DirectorySeparatorChar,comparison))throw new ArgumentException("Choose private storage OUTSIDE the source directory.");
Directory.CreateDirectory(folder);using var rsa=RSA.Create(3072);
File.WriteAllText(Path.Combine(folder,"captovix-signing.pem"),rsa.ExportPkcs8PrivateKeyPem());
File.WriteAllText(Path.Combine(folder,"captovix-public.pem"),rsa.ExportSubjectPublicKeyInfoPem());
File.WriteAllText(Path.Combine(folder,"edge-secrets.env"),"LICENSE_SIGNING_KEY_B64="+Convert.ToBase64String(rsa.ExportPkcs8PrivateKey())+"\nLICENSE_ISSUER=captovix-production\nPERPETUAL_LEASE_DAYS=30\nRECOVERY_COOLDOWN_HOURS=72\nADMIN_ORIGINS=\n");
File.WriteAllText(Path.Combine(folder,"license-config.json"),JsonSerializer.Serialize(new{apiUrl=url.AbsoluteUri.TrimEnd('/'),issuer="captovix-production",publicKeyPem=rsa.ExportSubjectPublicKeyInfoPem()},new JsonSerializerOptions{WriteIndented=true}));
if(!OperatingSystem.IsWindows()){File.SetUnixFileMode(folder,UnixFileMode.UserRead|UnixFileMode.UserWrite|UnixFileMode.UserExecute);foreach(var f in Directory.EnumerateFiles(folder))File.SetUnixFileMode(f,UnixFileMode.UserRead|UnixFileMode.UserWrite);}
Console.WriteLine("Created signing key, public key, edge-secrets.env and PUBLIC license-config.json in the private directory. No secret printed. Copy only license-config.json to src/Captovix and rebuild the Windows release. Keep/back up the private key; do not regenerate for each version.");
