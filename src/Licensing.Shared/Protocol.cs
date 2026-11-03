using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace Captovix.Licensing;

public record Envelope(string Payload,string Signature);
public record Claims(int Schema,string Product,string Issuer,string LicenseId,string ActivationId,string DeviceId,string PublicKeyHash,string Kind,string Status,DateTimeOffset IssuedAt,DateTimeOffset LeaseUntil,DateTimeOffset? TrialUntil);
public record Proof(string PublicKey,DateTimeOffset Timestamp,string Nonce,string Payload,string Signature);
public record DeviceInput(string DeviceId,string DeviceName,string Key="",string RecoveryCode="",string ActivationId="");
public record Reply(Envelope Certificate,string? RecoveryCode=null);
public static class Protocol
{
    public const string Product="HuyVo.Captovix";
    public static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);
    public static string Hash(string value)=>Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    public static string RandomSecret()=>Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(24));
    public static string NormalizeKey(string value)=>value.Trim().Replace("-","").ToUpperInvariant();
    public static string KeyHash(string value)=>Hash(NormalizeKey(value));
    public static byte[] ProofBytes(string operation,DateTimeOffset timestamp,string nonce,string payload)=>Encoding.UTF8.GetBytes($"captovix-license-v1\n{operation}\n{timestamp.ToUnixTimeSeconds()}\n{nonce}\n{Hash(payload)}");
    public static Proof SignRequest(RSA rsa,string operation,DeviceInput input,DateTimeOffset now)
    {
        var payload=JsonSerializer.Serialize(input,Json);var nonce=RandomSecret();
        return new(rsa.ExportSubjectPublicKeyInfoPem(),now,nonce,payload,Convert.ToBase64String(rsa.SignData(ProofBytes(operation,now,nonce,payload),HashAlgorithmName.SHA256,RSASignaturePadding.Pss)));
    }
    public static DeviceInput VerifyRequest(Proof p,string operation,DateTimeOffset now)
    {
        if(p.Payload.Length>4096||p.PublicKey.Length>1200||p.Nonce.Length!=48||!p.Nonce.All(Uri.IsHexDigit)||Math.Abs((now-p.Timestamp).TotalMinutes)>5)throw new InvalidDataException("REQUEST_INVALID_OR_CLOCK");
        using var rsa=RSA.Create();rsa.ImportFromPem(p.PublicKey);if(rsa.KeySize<2048||rsa.KeySize>4096||!rsa.VerifyData(ProofBytes(operation,p.Timestamp,p.Nonce,p.Payload),Convert.FromBase64String(p.Signature),HashAlgorithmName.SHA256,RSASignaturePadding.Pss))throw new InvalidDataException("PROOF_INVALID");
        var input=JsonSerializer.Deserialize<DeviceInput>(p.Payload,Json)??throw new InvalidDataException("REQUEST_INVALID");
        if(input.DeviceId.Length!=64||!input.DeviceId.All(Uri.IsHexDigit)||input.DeviceName.Length is <1 or >80||input.Key.Length>128||input.RecoveryCode.Length>128||input.ActivationId.Length is not (0 or 32))throw new InvalidDataException("REQUEST_INVALID");
        return input;
    }
    public static Envelope Sign(RSA signer,Claims claims)
    {
        var payload=Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(claims,Json));
        return new(payload,Convert.ToBase64String(signer.SignData(Encoding.UTF8.GetBytes(payload),HashAlgorithmName.SHA256,RSASignaturePadding.Pss)));
    }
    public static Claims Verify(Envelope value,string publicKey,string issuer,string deviceId,string publicKeyHash)
    {
        if(value.Payload.Length>10000||value.Signature.Length>1500)throw new InvalidDataException("CERT_INVALID");
        using var rsa=RSA.Create();rsa.ImportFromPem(publicKey);
        if(rsa.KeySize<3072||!rsa.VerifyData(Encoding.UTF8.GetBytes(value.Payload),Convert.FromBase64String(value.Signature),HashAlgorithmName.SHA256,RSASignaturePadding.Pss))throw new InvalidDataException("CERT_SIGNATURE");
        var c=JsonSerializer.Deserialize<Claims>(Convert.FromBase64String(value.Payload),Json)??throw new InvalidDataException("CERT_INVALID");
        if(c.IssuedAt<DateTimeOffset.UnixEpoch||c.IssuedAt>DateTimeOffset.MaxValue.AddDays(-31)||c.Schema!=1||c.Product!=Product||c.Issuer!=issuer||c.DeviceId!=deviceId||c.PublicKeyHash!=publicKeyHash||c.Kind is not ("trial" or "perpetual")||c.Status is not ("active" or "revoked" or "released")||c.LeaseUntil<=c.IssuedAt||c.LeaseUntil>c.IssuedAt.AddDays(31)||c.Kind=="trial"&&(c.TrialUntil==null||c.LeaseUntil>c.TrialUntil&&c.Status=="active"))throw new InvalidDataException("CERT_BINDING");
        return c;
    }
    public static bool FullAccess(Claims? c,DateTimeOffset now)=>c is {Status:"active"}&&now-c.IssuedAt>=TimeSpan.FromMinutes(-5)&&now<c.LeaseUntil&&(c.Kind!="trial"||now<c.TrialUntil);
}
