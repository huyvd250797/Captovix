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
    Check(new UpdateSettings{SourceUrl=UpdateSettings.PublisherRepository}.ResolvedFeed()==UpdateSettings.StableFeed,"GitHub source resolves directly to stable feed without release-list paging");
    var migrate=Path.Combine(root,"migrate");Directory.CreateDirectory(migrate);UpdateSettingsStore.Save(migrate,new(){SourceUrl=UpdateSettings.PublisherRepository,AutoCheck=false});var migrated=UpdateSettingsStore.Load(migrate,"absent",out _);Check(migrated.SourceKind=="Https"&&migrated.SourceUrl==UpdateSettings.StableFeed&&!migrated.AutoCheck,"Migrate publisher source while preserving opt-out");
    UpdateSettingsStore.Save(migrate,new(){SourceUrl="https://github.com/Custom/Publisher"});Check(UpdateSettingsStore.Load(migrate,"absent",out _).SourceUrl=="https://github.com/Custom/Publisher","Keep custom publisher unchanged");
    Check(UpdateError.Explain(new HttpRequestException("missing",null,System.Net.HttpStatusCode.NotFound)).Contains("404"),"404 reports missing feed instead of latest version");
    Check(UpdateError.Explain(new TimeoutException()).Contains("25"),"Timeout reports retryable source error");
    Check(new UpdateSettings{SourceKind="Https",SourceUrl=UpdateSettings.StableFeed}.ReleasePage()==UpdateSettings.PublisherRepository+"/releases/latest","Portable release button opens a page instead of the feed directory redirect");
    Check(new UpdateSettings{SourceKind="Https",SourceUrl="https://example.com/feed"}.ReleasePage()=="https://example.com/feed","Custom feed keeps its manual release target");
    var now = DateTimeOffset.UtcNow;
    Check(settings.ShouldAutoCheck(now), "First daily check");
    Check(settings.ShouldCheckInSession(now,null),"New app session checks even with a recent persisted timestamp");Check(!settings.ShouldCheckInSession(now,now.AddMinutes(-29)),"Session polls no more than every30minutes");Check(settings.ShouldCheckInSession(now,now.AddMinutes(-30)),"Session can discover a release after30minutes");settings.AutoCheck=false;Check(!settings.ShouldCheckInSession(now,null),"Session preserves explicit auto-check opt-out");settings.AutoCheck=true;
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
    File.WriteAllText(Path.Combine(root,"annotation-presets.json"), "[{\"Name\":\"Mẫu\"}]");
    var backup = UpdateBackup.Create(root, "1.0.3", "1.1.0");
    using (var zip = ZipFile.OpenRead(backup))
    {
        Check(zip.GetEntry("annotation-presets.json")!=null,"Custom annotation presets are included in update backups");
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
        var locator = new TestVelopackLocator(VersionInfo.AppId, "1.31.0", packagesDir);
        var source = new SimpleFileSource(new DirectoryInfo(sourceDir));
        var sdk = new VelopackUpdateBackend(new() { SourceKind = "Https", SourceUrl = "https://example.com/feed" }, locator, "1.31.0", source);
        Check(sdk.CanApply, "SDK recognizes test installed app");
        var release = await sdk.CheckAsync(); Check(release?.Version == actualVersion.ToString(), "SDK discovers real packaged version");
        await sdk.DownloadAsync(release!, _ => { }, default);
        Check(File.Exists(Path.Combine(packagesDir, Path.GetFileName(package))), "SDK download and SHA-256 verification pass");
        File.Delete(Path.Combine(packagesDir, Path.GetFileName(package)));
        var patchPackages=Path.Combine(root,"patch-from1370");Directory.CreateDirectory(patchPackages);
        var patchLocator=new TestVelopackLocator(VersionInfo.AppId,"1.37.0",patchPackages);
        var patchSdk=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl="https://example.com/feed"},patchLocator,"1.37.0",source);
        var patchRelease=await patchSdk.CheckAsync();
        Check(patchRelease?.Version==actualVersion.ToString() && actualVersion>SemanticVersion.Parse("1.37.0"),"Installed1.37.0 discovers actual patch package newer than1.37.0");
        await patchSdk.DownloadAsync(patchRelease!,_=>{},default);
        Check(File.Exists(Path.Combine(patchPackages,Path.GetFileName(package))),"Installed1.37.0 downloads and verifies patch Full package");
        var from1371=Path.Combine(root,"patch-from1371");Directory.CreateDirectory(from1371);
        var installed1371=new TestVelopackLocator(VersionInfo.AppId,"1.37.1",from1371);
        var backend1371=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl="https://example.com/feed"},installed1371,"1.37.1",source);
        var update1371=await backend1371.CheckAsync();Check(update1371?.Version==actualVersion.ToString()&&actualVersion>SemanticVersion.Parse("1.37.1"),"Installed1.37.1 discovers actual newer patch");
        await backend1371.DownloadAsync(update1371!,_=>{},default);Check(File.Exists(Path.Combine(from1371,Path.GetFileName(package))),"Installed1.37.1 downloads and verifies actual full patch");
        var from1372=Path.Combine(root,"patch-from1372");Directory.CreateDirectory(from1372);
        var installed1372=new TestVelopackLocator(VersionInfo.AppId,"1.37.2",from1372);
        var backend1372=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl="https://example.com/feed"},installed1372,"1.37.2",source);
        var update1372=await backend1372.CheckAsync();Check(update1372?.Version==actualVersion.ToString()&&actualVersion>SemanticVersion.Parse("1.37.2"),"Installed1.37.2 discovers actual newer patch");
        await backend1372.DownloadAsync(update1372!,_=>{},default);Check(File.Exists(Path.Combine(from1372,Path.GetFileName(package))),"Installed1.37.2 downloads and verifies actual full patch");
        var from1373=Path.Combine(root,"patch-from1373");Directory.CreateDirectory(from1373);
        var installed1373=new TestVelopackLocator(VersionInfo.AppId,"1.37.3",from1373);
        var backend1373=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl="https://example.com/feed"},installed1373,"1.37.3",source);
        var update1373=await backend1373.CheckAsync();Check(update1373?.Version==actualVersion.ToString()&&actualVersion>SemanticVersion.Parse("1.37.3"),"Installed1.37.3 discovers actual newer M1 release");
        await backend1373.DownloadAsync(update1373!,_=>{},default);Check(File.Exists(Path.Combine(from1373,Path.GetFileName(package))),"Installed1.37.3 downloads and verifies actual full M1 package");
        var from1380=Path.Combine(root,"patch-from1380");Directory.CreateDirectory(from1380);
        var installed1380=new TestVelopackLocator(VersionInfo.AppId,"1.38.0",from1380);
        var backend1380=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl="https://example.com/feed"},installed1380,"1.38.0",source);
        var update1380=await backend1380.CheckAsync();Check(update1380?.Version==actualVersion.ToString()&&actualVersion>SemanticVersion.Parse("1.38.0"),"Installed1.38.0 discovers actual newer M2 release");
        await backend1380.DownloadAsync(update1380!,_=>{},default);Check(File.Exists(Path.Combine(from1380,Path.GetFileName(package))),"Installed1.38.0 downloads and verifies actual full M2 package");
        var from1390=Path.Combine(root,"patch-from1390");Directory.CreateDirectory(from1390);
        var installed1390=new TestVelopackLocator(VersionInfo.AppId,"1.39.0",from1390);
        var backend1390=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl="https://example.com/feed"},installed1390,"1.39.0",source);
        var update1390=await backend1390.CheckAsync();Check(update1390?.Version==actualVersion.ToString()&&actualVersion>SemanticVersion.Parse("1.39.0"),"Installed1.39.0 discovers actual newer M3 release");
        await backend1390.DownloadAsync(update1390!,_=>{},default);Check(File.Exists(Path.Combine(from1390,Path.GetFileName(package))),"Installed1.39.0 downloads and verifies actual full M3 package");
        var from1400=Path.Combine(root,"patch-from1400");Directory.CreateDirectory(from1400);
        var installed1400=new TestVelopackLocator(VersionInfo.AppId,"1.40.0",from1400);
        var backend1400=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl="https://example.com/feed"},installed1400,"1.40.0",source);
        var update1400=await backend1400.CheckAsync();Check(update1400?.Version==actualVersion.ToString()&&actualVersion>SemanticVersion.Parse("1.40.0"),"Installed1.40.0 discovers actual M4 release");
        await backend1400.DownloadAsync(update1400!,_=>{},default);Check(File.Exists(Path.Combine(from1400,Path.GetFileName(package))),"Installed1.40.0 downloads and verifies full M4 package");
        var from1410=Path.Combine(root,"patch-from1410");Directory.CreateDirectory(from1410);
        var installed1410=new TestVelopackLocator(VersionInfo.AppId,"1.41.0",from1410);
        var backend1410=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl="https://example.com/feed"},installed1410,"1.41.0",source);
        var update1410=await backend1410.CheckAsync();Check(update1410?.Version==actualVersion.ToString()&&actualVersion>SemanticVersion.Parse("1.41.0"),"Installed1.41.0 discovers actual startup recovery patch");
        await backend1410.DownloadAsync(update1410!,_=>{},default);Check(File.Exists(Path.Combine(from1410,Path.GetFileName(package))),"Installed1.41.0 downloads and verifies full recovery package");
        foreach (var previousVersion in new[] { "1.46.1", "1.46.2", "1.46.3", "1.46.4", "1.47.0", "1.48.0", "1.49.0" }.Select(SemanticVersion.Parse).Where(v => actualVersion > v))
        {
            var previousPackages = Directory.CreateDirectory(Path.Combine(root, "patch-from-" + previousVersion)).FullName;
            var previousLocator = new TestVelopackLocator(VersionInfo.AppId, previousVersion.ToString(), previousPackages);
            var previousBackend = new VelopackUpdateBackend(new() { SourceKind = "Https", SourceUrl = "https://example.com/feed" }, previousLocator, previousVersion.ToString(), source);
            var upgrade = await previousBackend.CheckAsync();
            Check(upgrade?.Version == actualVersion.ToString(), $"Installed {previousVersion} discovers actual newer release");
            await previousBackend.DownloadAsync(upgrade!, _ => { }, default);
            Check(File.Exists(Path.Combine(previousPackages, Path.GetFileName(package))), $"Installed {previousVersion} downloads and verifies actual full package");
        }
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
        asset.Version = SemanticVersion.Parse("1.31.0"); WriteFeed(asset);
        Check(await sdk.CheckAsync() == null, "SDK does not offer the same version");
        asset.Version = SemanticVersion.Parse("99.0.0-beta.1"); WriteFeed(asset);
        rejected = false; try { rejected = await sdk.CheckAsync() == null; } catch { rejected = true; }
        Check(rejected, "Prerelease never reaches the stable apply flow");
        asset.Version = actualVersion; asset.SHA256 = ""; WriteFeed(asset);
        rejected = false; try { await sdk.CheckAsync(); } catch { rejected = true; }
        Check(rejected, "Feed without SHA-256 is rejected");
    }
    var jump=Path.Combine(root,"direct-jump");Directory.CreateDirectory(jump);
    foreach(var target in new[]{"1.33.0","1.36.0","1.37.0","1.37.1","1.37.2","1.37.3","1.38.0","1.39.0","1.40.0","1.41.0","1.41.1","1.42.0","1.43.0","1.44.0","1.44.1","1.45.0","1.46.0","1.46.1"}){
        File.WriteAllText(Path.Combine(jump,"releases.win.json"),JsonSerializer.Serialize(new{Assets=new[]{new{PackageId=VersionInfo.AppId,Version=target,Type="Full",FileName=$"HuyVo.Captovix-{target}-full.nupkg",SHA256=new string('A',64),SHA1=new string('B',40),Size=100}}}));
        var installed=new TestVelopackLocator(VersionInfo.AppId,"1.31.0",Path.Combine(root,"jump-"+target));
        var direct=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl=UpdateSettings.StableFeed},installed,"1.31.0",new SimpleFileSource(new DirectoryInfo(jump)));
        Check((await direct.CheckAsync())?.Version==target,"Installed1.31 directly targets"+target+" without intermediate packages");
    }
    // The newest version is deliberately beyond the first ten feed entries.
    var many=Path.Combine(root,"many");Directory.CreateDirectory(many);
    var versions=Enumerable.Range(0,15).Select(n=>new {PackageId=VersionInfo.AppId,Version=n==14?"1.11.1":"1.0."+n,Type="Full",FileName="Captovix-"+n+"-full.nupkg",SHA256=new string('A',64),SHA1=new string('B',40),Size=100}).ToArray();
    File.WriteAllText(Path.Combine(many,"releases.win.json"),JsonSerializer.Serialize(new{Assets=versions}));
    var manyLocator=new TestVelopackLocator(VersionInfo.AppId,"1.9.0",Path.Combine(root,"many-packages"));
    var backend=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl=UpdateSettings.StableFeed},manyLocator,"1.9.0",new SimpleFileSource(new DirectoryInfo(many)));
    Check((await backend.CheckAsync())?.Version=="1.11.1"&&backend.SourceVersion=="1.11.1","Production SDK discovers stable version at feed position15");
    Check(backend.FeedDescription==UpdateSettings.StableFeed,"Diagnostics show exact resolved stable endpoint");
    var same=new VelopackUpdateBackend(new(){SourceKind="Https",SourceUrl=UpdateSettings.StableFeed},manyLocator,"1.11.1",new SimpleFileSource(new DirectoryInfo(many)));var diagnosticSession=new UpdateSession(same);await diagnosticSession.CheckAsync();Check(diagnosticSession.Message.Contains("1.11.1")&&diagnosticSession.SourceVersion=="1.11.1","Up-to-date message displays actual source version");
    File.WriteAllText(Path.Combine(many,"releases.win.json"),"{\"Assets\":[]}");await diagnosticSession.CheckAsync();Check(diagnosticSession.SourceVersion==null&&diagnosticSession.Message.Contains("Nguồn không có"),"Empty feed is an error rather than a false latest-version result");
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
