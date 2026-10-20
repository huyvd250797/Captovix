using Captovix;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

var passed = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); passed++; Console.WriteLine("PASS: " + name); }
void Reject(Action action, string name) { try { action(); } catch { Check(true, name); return; } Check(false, name); }
var root = Path.Combine(Path.GetTempPath(), "captovix-update-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
try
{
    var settings = new UpdateSettings { SourceUrl = " https://github.com/HuyVo/Captovix/ " };
    Check(settings.NormalizeUrl() == "https://github.com/HuyVo/Captovix", "Normalize GitHub repository URL");
    foreach (var url in new[] { "http://github.com/a/b", "https://token@github.com/a/b", "https://github.com/a/b?token=secret", "https://github.com/a/b#x", "https://github.com/a/b/releases", "https://github.com:444/a/b", "https://example.com/a/b", "file:///C:/feed" })
        Reject(() => new UpdateSettings { SourceUrl = url }.NormalizeUrl(), "Reject invalid source " + url);
    Check(new UpdateSettings { SourceKind = "Https", SourceUrl = "https://downloads.example.com/captovix/" }.NormalizeUrl() == "https://downloads.example.com/captovix", "Static HTTPS folder");
    Reject(() => new UpdateSettings { SourceKind = "Https", SourceUrl = "https://example.com/releases.win.json" }.NormalizeUrl(), "Reject JSON URL instead of directory");
    var now = DateTimeOffset.UtcNow;
    Check(settings.ShouldAutoCheck(now), "First daily check");
    settings.LastCheckedSource = settings.SourceKey; settings.LastCheckUtc = now;
    Check(!settings.ShouldAutoCheck(now.AddHours(23)) && settings.ShouldAutoCheck(now.AddHours(24)), "24-hour throttle");
    settings.SourceUrl = "https://github.com/HuyVo/Other";
    Check(settings.ShouldAutoCheck(now), "Changed source clears daily throttle");
    settings.AutoCheck = false; Check(!settings.ShouldAutoCheck(now.AddDays(2)), "Auto check opt-out");
    settings.AutoCheck = true; settings.LastCheckedSource = settings.SourceKey; settings.LastCheckUtc = now.AddDays(2);
    Check(settings.ShouldAutoCheck(now), "Recover from clock moving backward");
    var defaults = Path.Combine(root, "defaults.json"); File.WriteAllText(defaults, "{\"SourceUrl\":\"https://github.com/a/b\"}");
    Check(UpdateSettingsStore.Load(root, defaults, out _).SourceUrl == "https://github.com/a/b", "Packaged defaults");
    UpdateSettingsStore.Save(root, settings);
    Check(UpdateSettingsStore.Load(root, defaults, out _).SourceUrl == settings.SourceUrl, "Saved source overrides packaged default");
    UpdateSettingsStore.Save(root, new() { SourceUrl = "" });
    Check(!UpdateSettingsStore.Load(root, defaults, out _).IsConfigured, "Blank saved source disables packaged defaults");
    File.WriteAllText(Path.Combine(root, "update-settings.json"), "broken");
    var invalid = UpdateSettingsStore.Load(root, defaults, out var warning);
    Check(!invalid.IsConfigured && !invalid.AutoCheck && warning != null, "Corrupt override never switches publisher silently");

    var fake = new FakeBackend(); var session = new UpdateSession(fake);
    var pendingCheck = new TaskCompletionSource<UpdateRelease?>(); fake.Check = () => pendingCheck.Task;
    var first = session.CheckAsync(); await session.CheckAsync();
    Check(fake.CheckCount == 1 && session.Stage == UpdateStage.Checking, "Concurrent checks coalesced");
    pendingCheck.SetResult(FakeBackend.Release); await first;
    Check(session.Stage == UpdateStage.Available && session.Release?.Version == "1.0.2", "Available release and notes");
    var pendingDownload = new TaskCompletionSource();
    fake.Download = (_, token) => pendingDownload.Task.WaitAsync(token);
    using var cancel = new CancellationTokenSource(); var download = session.DownloadAsync(cancel.Token); await session.DownloadAsync(default);
    Check(fake.DownloadCount == 1 && session.Stage == UpdateStage.Downloading, "Concurrent downloads prevented");
    cancel.Cancel(); await download;
    Check(session.Stage == UpdateStage.Available, "Cancel restores retry action");
    fake.Download = (_, _) => throw new InvalidDataException("checksum mismatch"); await session.DownloadAsync(default);
    Check(session.Stage == UpdateStage.Available && !session.Apply(false, () => true) && fake.ApplyCount == 0, "Corrupt download cannot apply");
    fake.Download = (_, _) => Task.CompletedTask; await session.DownloadAsync(default);
    Check(session.Stage == UpdateStage.Ready, "Successful download ready for explicit restart");
    var prepareCalls = 0;
    Check(!session.Apply(true, () => { prepareCalls++; return true; }) && prepareCalls == 0, "Capture blocks apply before any save/exit");
    Check(!session.Apply(false, () => false) && fake.ApplyCount == 0 && session.Stage == UpdateStage.Ready, "Save failure prevents exit");
    Check(!session.Apply(false, () => throw new IOException("backup failed")) && fake.ApplyCount == 0, "Backup failure prevents exit");
    fake.FailApply = true;
    Check(!session.Apply(false, () => true) && session.Stage == UpdateStage.Ready, "Apply launch error remains retryable");
    fake.FailApply = false;
    Check(session.Apply(false, () => true) && fake.ApplyCount == 2 && session.Stage == UpdateStage.Applying, "Apply follows successful preparation only");
    Check(!session.Apply(false, () => true) && fake.ApplyCount == 2, "Repeated restart cannot apply twice");
    session = new(new FakeBackend()); await session.CheckAsync(); await session.DownloadAsync(default); session.Defer();
    Check(session.Stage == UpdateStage.Idle && session.Release == null && !session.Apply(false, () => true), "Defer never applies cached update");
    fake = new() { CanApply = false }; session = new(fake); await session.CheckAsync(); await session.DownloadAsync(default);
    Check(session.Stage == UpdateStage.Available && fake.DownloadCount == 0 && !session.Apply(false, () => true), "Portable notify-only mode");
    fake = new() { Check = () => Task.FromResult<UpdateRelease?>(null) }; session = new(fake); await session.CheckAsync();
    Check(session.Stage == UpdateStage.Idle && session.Release == null, "No update available");
    fake.Check = () => throw new IOException("offline"); await session.CheckAsync();
    Check(session.Stage == UpdateStage.Idle && session.Message.Contains("Chưa kiểm tra"), "Offline check leaves current app usable");

    Directory.CreateDirectory(Path.Combine(root, "Edits")); Directory.CreateDirectory(Path.Combine(root, "Media"));
    File.WriteAllText(Path.Combine(root, "library.json"), "[{\"Id\":\"a\"}]"); File.WriteAllText(Path.Combine(root, "Edits", "a.json"), "{\"annotations\":[1]}");
    File.WriteAllBytes(Path.Combine(root, "Media", "a.png"), [1, 2, 3]);
    var originalHash = SHA256.HashData(File.ReadAllBytes(Path.Combine(root, "Media", "a.png")));
    var backup = UpdateBackup.Create(root, "1.0.1", "1.0.2");
    using (var zip = ZipFile.OpenRead(backup))
    {
        Check(zip.GetEntry("library.json") != null && zip.GetEntry("Edits/a.json") != null && zip.GetEntry("backup-info.txt") != null, "Metadata and annotation backup");
        Check(zip.Entries.All(e => !e.FullName.StartsWith("Media/")), "Backup excludes original media");
        using var reader = new StreamReader(zip.GetEntry("Edits/a.json")!.Open());
        Check(reader.ReadToEnd() == "{\"annotations\":[1]}", "Annotation backup remains exact");
    }
    Check(originalHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, "Media", "a.png")))) && File.Exists(Path.Combine(root, "Edits", "a.json")), "Update backup leaves originals and documents untouched");

    // Optional real SDK integration against a generated package. No Windows binary is executed.
    if (args.Length == 1)
    {
        var package = Path.GetFullPath(args[0]); var sourceDir = Path.Combine(root, "feed"); Directory.CreateDirectory(sourceDir);
        var sourceCopy = Path.Combine(sourceDir, Path.GetFileName(package)); File.Copy(package, sourceCopy);
        var asset = await VelopackAsset.FromNupkgGenerateChecksumAsync(sourceCopy);
        var actualVersion = asset.Version;
        void WriteFeed(VelopackAsset a) => File.WriteAllText(Path.Combine(sourceDir, "releases.win.json"), JsonSerializer.Serialize(new { Assets = new[] { new {
            a.PackageId, Version = a.Version.ToString(), Type = a.Type.ToString(), a.FileName, a.SHA1, a.SHA256, a.Size, a.NotesMarkdown, a.NotesHTML
        } } }));
        WriteFeed(asset);
        var packagesDir = Path.Combine(root, "packages"); Directory.CreateDirectory(packagesDir);
        var locator = new TestVelopackLocator(VersionInfo.AppId, "1.0.0", packagesDir);
        var source = new SimpleFileSource(new DirectoryInfo(sourceDir));
        var sdk = new VelopackUpdateBackend(new() { SourceKind = "Https", SourceUrl = "https://example.com/feed" }, locator, "1.0.0", source);
        Check(sdk.CanApply, "SDK recognizes test installed app");
        var release = await sdk.CheckAsync(); Check(release?.Version == actualVersion.ToString(), "SDK discovers real packaged version");
        await sdk.DownloadAsync(release!, _ => { }, default);
        Check(File.Exists(Path.Combine(packagesDir, Path.GetFileName(package))), "SDK download and SHA-256 verification pass");
        File.Delete(Path.Combine(packagesDir, Path.GetFileName(package)));
        // Change expected SHA-256 while keeping SHA-1: ensure explicit SHA-256 check rejects it.
        asset.SHA256 = new string('0', 64); WriteFeed(asset);
        release = await sdk.CheckAsync(); var rejected = false;
        try { await sdk.DownloadAsync(release!, _ => { }, default); } catch { rejected = true; }
        Check(rejected, "Real package with wrong SHA-256 is rejected");
        asset.PackageId = "Other.App"; WriteFeed(asset);
        rejected = false; try { var wrong = await sdk.CheckAsync(); rejected = wrong == null; } catch { rejected = true; }
        Check(rejected, "Another app package is never offered");
        asset.PackageId = VersionInfo.AppId; asset.Version = SemanticVersion.Parse("0.9.0"); WriteFeed(asset);
        Check(await sdk.CheckAsync() == null, "SDK does not offer older release");
        asset.Version = SemanticVersion.Parse("1.0.0"); WriteFeed(asset);
        Check(await sdk.CheckAsync() == null, "SDK does not offer the same version");
        asset.Version = SemanticVersion.Parse("99.0.0-beta.1"); WriteFeed(asset);
        rejected = false; try { rejected = await sdk.CheckAsync() == null; } catch { rejected = true; }
        Check(rejected, "Prerelease never reaches the stable apply flow");
        asset.Version = actualVersion; asset.SHA256 = ""; WriteFeed(asset);
        rejected = false; try { await sdk.CheckAsync(); } catch { rejected = true; }
        Check(rejected, "Feed without SHA-256 is rejected");
    }
    Console.WriteLine($"{passed} update checks passed.");
}
finally { Directory.Delete(root, true); }

sealed class FakeBackend : IUpdateBackend
{
    public static readonly UpdateRelease Release = new("1.0.2", "Release notes", 100, new object());
    public bool CanApply { get; set; } = true;
    public Func<Task<UpdateRelease?>> Check { get; set; } = () => Task.FromResult<UpdateRelease?>(Release);
    public Func<Action<int>, CancellationToken, Task> Download { get; set; } = (_, _) => Task.CompletedTask;
    public int CheckCount, DownloadCount, ApplyCount;
    public bool FailApply;
    public Task<UpdateRelease?> CheckAsync() { CheckCount++; return Check(); }
    public Task DownloadAsync(UpdateRelease release, Action<int> progress, CancellationToken token) { DownloadCount++; return Download(progress, token); }
    public void Apply(UpdateRelease release) { ApplyCount++; if (FailApply) throw new IOException("Update.exe failed"); }
}
