namespace Captovix;

public enum UpdateStage { Idle, Checking, Available, Downloading, Ready, Applying }
public sealed record UpdateRelease(string Version, string Notes, long Bytes, object Payload);
public interface IUpdateBackend
{
    bool CanApply { get; }
    Task<UpdateRelease?> CheckAsync();
    Task DownloadAsync(UpdateRelease release, Action<int> progress, CancellationToken token);
    void Apply(UpdateRelease release);
}

// All state transitions run on the calling (UI) context. The backend never touches WPF.
public sealed class UpdateSession(IUpdateBackend backend)
{
    public UpdateStage Stage { get; private set; }
    public UpdateRelease? Release { get; private set; }
    public string Message { get; private set; } = "Sẵn sàng kiểm tra cập nhật.";
    public int Progress { get; private set; }
    public bool IsWorking => Stage is UpdateStage.Checking or UpdateStage.Downloading or UpdateStage.Applying;
    public bool CanApply => backend.CanApply;
    public event Action? Changed;
    public event Action<Exception>? Failed;
    private void Notify() => Changed?.Invoke();

    public async Task CheckAsync()
    {
        if (IsWorking || Stage == UpdateStage.Ready) return;
        Release = null; Stage = UpdateStage.Checking; Message = "Đang kiểm tra phiên bản mới…"; Notify();
        try
        {
            Release = await backend.CheckAsync();
            Stage = Release == null ? UpdateStage.Idle : UpdateStage.Available;
            Message = Release == null ? "Bạn đang dùng phiên bản mới nhất trên nguồn đã chọn." : $"Có phiên bản mới: V{Release.Version}.";
        }
        catch (Exception ex) { Stage = UpdateStage.Idle; Message = "Chưa kiểm tra được. Kiểm tra kết nối và nguồn cập nhật rồi thử lại."; Failed?.Invoke(ex); }
        Notify();
    }

    public async Task DownloadAsync(CancellationToken token)
    {
        if (Stage != UpdateStage.Available || Release == null || !CanApply) return;
        Stage = UpdateStage.Downloading; Progress = 0; Message = "Đang tải và kiểm tra gói cập nhật…"; Notify();
        // Progress<T> marshals backend callbacks onto the original synchronization context.
        var progress = new Progress<int>(value => { if (Stage == UpdateStage.Downloading) { Progress = Math.Clamp(value, 0, 100); Notify(); } });
        try
        {
            await backend.DownloadAsync(Release, value => ((IProgress<int>)progress).Report(value), token);
            token.ThrowIfCancellationRequested();
            Stage = UpdateStage.Ready; Progress = 100; Message = "Đã tải và kiểm tra xong. Khởi động lại để cập nhật.";
        }
        catch (OperationCanceledException) { Stage = UpdateStage.Available; Message = "Đã hủy tải. Bạn có thể tải lại sau."; }
        catch (Exception ex) { Stage = UpdateStage.Available; Message = "Chưa tải/kiểm tra được gói cập nhật. Bản hiện tại vẫn dùng được; hãy thử lại."; Failed?.Invoke(ex); }
        Notify();
    }

    public void Defer()
    {
        if (IsWorking) return;
        Release = null; Stage = UpdateStage.Idle;
        Message = "Đã hoãn cập nhật. Bấm Kiểm tra cập nhật khi muốn tiếp tục; gói cache sẽ được kiểm tra lại.";
        Notify();
    }

    public bool Apply(bool captureBusy, Func<bool> prepare)
    {
        if (Stage != UpdateStage.Ready || Release == null || !CanApply) return false;
        if (captureBusy) { Message = "Hãy kết thúc chụp/quay và chờ file được lưu trước khi cập nhật."; Notify(); return false; }
        try
        {
            // Flush edits, metadata, settings and backup BEFORE asking Update.exe to exit us.
            if (!prepare()) { Message = "Chưa lưu được nội dung. Đã hoãn cập nhật để giữ phiên làm việc."; Notify(); return false; }
            Stage = UpdateStage.Applying; Message = "Đang khởi động lại để cập nhật…"; Notify();
            backend.Apply(Release);
            return true;
        }
        catch (Exception ex) { Stage = UpdateStage.Ready; Message = "Chưa áp dụng được cập nhật. Nội dung đã lưu; hãy thử lại."; Failed?.Invoke(ex); Notify(); return false; }
    }
}
