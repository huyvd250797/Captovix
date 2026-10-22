using System.IO;
using System.Security.Cryptography;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Captovix;
public sealed class VelopackUpdateBackend : IUpdateBackend, IUpdateDiagnostics
{
    private readonly IUpdateSource _source;
    private readonly IVelopackLocator _locator;
    private readonly ReliableUpdateSource _snapshot;
    public string? SourceVersion {get;private set;}
    public string FeedDescription {get;}
    private readonly UpdateManager _manager;
    private readonly SemanticVersion _current;
    public bool CanApply => _manager.IsInstalled && !_manager.IsPortable && _manager.AppId == VersionInfo.AppId;
    public string InstallLabel => CanApply ? "Bản cài đặt · cập nhật trong ứng dụng" : "Portable / bản phát triển · tải bản mới thủ công";

    public VelopackUpdateBackend(UpdateSettings settings, IVelopackLocator? locator = null, string? currentVersion = null, IUpdateSource? testSource = null)
    {
        var url = settings.NormalizeUrl();
        FeedDescription=settings.ResolvedFeed();
        _source = testSource ?? new SimpleWebSource(FeedDescription);
        _snapshot=new(_source);
        _locator = locator ?? VelopackLocator.Current;
        _manager = new(_snapshot, new() { ExplicitChannel = VersionInfo.Channel, AllowVersionDowngrade = false }, _locator);
        _current = SemanticVersion.Parse(currentVersion ?? VersionInfo.Current);
    }

    public async Task<UpdateRelease?> CheckAsync()
    {
        SourceVersion=null;_snapshot.Snapshot=null;
        var feed=await _source.GetReleaseFeed(_locator.Log,VersionInfo.AppId,VersionInfo.Channel,null,null).WaitAsync(TimeSpan.FromSeconds(25));
        var latest=feed.Assets.Where(a=>a.PackageId==VersionInfo.AppId&&a.Type==VelopackAssetType.Full&&!a.Version.IsPrerelease).OrderByDescending(a=>a.Version).FirstOrDefault();
        if(latest==null)throw new InvalidDataException("Nguồn không có gói Captovix stable cho kênh win. Kiểm tra releases.win.json và file .nupkg.");
        ValidateMetadata(latest);SourceVersion=latest.Version.ToString();_snapshot.Snapshot=feed;
        if(latest.Version<=_current)return null;
        if(CanApply){
            var info=await _manager.CheckForUpdatesAsync().WaitAsync(TimeSpan.FromSeconds(25));
            if(info==null)throw new InvalidDataException("Feed có bản mới nhưng bộ cài chưa chọn được gói phù hợp. Kiểm tra AppId/kênh và bản cài đang chạy.");
            Validate(info.TargetFullRelease);return Describe(info.TargetFullRelease,info);
        }
        Validate(latest);return Describe(latest,latest);
    }

    private static UpdateRelease Describe(VelopackAsset asset, object payload) =>
        new(asset.Version.ToString(), string.IsNullOrWhiteSpace(asset.NotesMarkdown) ? "Bản phát hành chưa cung cấp nội dung thay đổi." : asset.NotesMarkdown, asset.Size, payload);

    private void Validate(VelopackAsset asset)
    {
        if (asset.PackageId != VersionInfo.AppId || asset.Type != VelopackAssetType.Full || asset.Version <= _current || asset.Version.IsPrerelease)
            throw new InvalidDataException("Gói không thuộc Captovix stable hoặc không phải phiên bản mới hơn.");
        ValidateMetadata(asset);
    }

    private static void ValidateMetadata(VelopackAsset asset)
    {
        if (string.IsNullOrWhiteSpace(asset.FileName) || asset.FileName.IndexOfAny(['/', '\\', ':']) >= 0 ||
            !asset.FileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase) || asset.Size <= 0 ||
            asset.SHA256?.Length != 64 || !asset.SHA256.All(Uri.IsHexDigit))
            throw new InvalidDataException("Gói cập nhật thiếu tên, kích thước hoặc SHA-256 hợp lệ.");
    }

    public async Task DownloadAsync(UpdateRelease release, Action<int> progress, CancellationToken token)
    {
        if (!CanApply || release.Payload is not UpdateInfo info) throw new InvalidOperationException("Hãy dùng bản Setup để cập nhật trong app.");
        Validate(info.TargetFullRelease);
        await _manager.DownloadUpdatesAsync(info, progress, token);
        var asset = info.TargetFullRelease;
        var path = Path.Combine(_locator.PackagesDir!, asset.FileName);
        using var stream = File.OpenRead(path);
        var digest = Convert.ToHexString(await SHA256.HashDataAsync(stream, token));
        if (stream.Length != asset.Size || !digest.Equals(asset.SHA256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("SHA-256/kích thước gói không khớp. Không áp dụng bản cập nhật.");
    }

    public void Apply(UpdateRelease release)
    {
        if (!CanApply || release.Payload is not UpdateInfo info) throw new InvalidOperationException("Không có gói sẵn sàng.");
        Validate(info.TargetFullRelease);
        _manager.ApplyUpdatesAndRestart(info.TargetFullRelease);
    }
}
