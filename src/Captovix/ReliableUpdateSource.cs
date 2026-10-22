using Velopack;
using Velopack.Logging;
using Velopack.Sources;
namespace Captovix;
// GitHub's latest stable redirect avoids the SDK's first-page/10-release limit.
public sealed class ReliableUpdateSource(IUpdateSource inner):IUpdateSource
{
    public VelopackAssetFeed? Snapshot {get;set;}
    public Task<VelopackAssetFeed> GetReleaseFeed(IVelopackLogger logger,string? appId,string channel,Guid? stagingId=null,VelopackAsset? latestLocalRelease=null)
        =>Snapshot!=null?Task.FromResult(Snapshot):inner.GetReleaseFeed(logger,appId,channel,stagingId,latestLocalRelease);
    public Task DownloadReleaseEntry(IVelopackLogger logger,VelopackAsset releaseEntry,string localFile,Action<int> progress,CancellationToken cancelToken)
        =>inner.DownloadReleaseEntry(logger,releaseEntry,localFile,progress,cancelToken);
}
public interface IUpdateDiagnostics
{
    string? SourceVersion {get;}
    string FeedDescription {get;}
}
