using System.IO;
using System.Windows;

namespace Captovix;
public partial class MainWindow
{
    private UpdateSettings _updateSettings = new();
    private UpdateSession? _updates;
    private CancellationTokenSource? _updateCancellation;
    private bool _updateApplying, _updateUiClosed;
    private readonly System.Windows.Threading.DispatcherTimer _updateCheckTimer = new() { Interval = TimeSpan.FromMinutes(30) };

    private void InitializeUpdates()
    {
        CurrentVersionLabel.Text = $"Captovix V{VersionInfo.Current}";
        _updateCheckTimer.Tick += async (_, _) => await AutoCheckUpdatesAsync();
        _updateCheckTimer.Start();
        _updateSettings = UpdateSettingsStore.Load(App.DataRoot, Path.Combine(AppContext.BaseDirectory, "update-source.json"), out var warning);
        SourceKindPicker.SelectedIndex = _updateSettings.SourceKind == "Https" ? 1 : 0;
        UpdateSourceBox.Text = _updateSettings.SourceUrl;
        AutoUpdateCheck.IsChecked = _updateSettings.AutoCheck;
        RestoreEditorAfterUpdate();
        if (warning != null) { UpdateStatus.Text = warning; RenderUpdateUi(); return; }
        ConfigureUpdateSession();
        _ = AutoCheckUpdatesAsync();
    }

    private void ConfigureUpdateSession()
    {
        _updates = null;
        if (!_updateSettings.IsConfigured)
        {
            UpdateStatus.Text = "Chưa cấu hình nguồn phát hành. Mở Cấu hình nguồn cập nhật để thiết lập.";
            UpdateInstallLabel.Text = "Nguồn cập nhật chưa được thiết lập";
            RenderUpdateUi(); return;
        }
        try
        {
            var backend = new VelopackUpdateBackend(_updateSettings);
            UpdateInstallLabel.Text = backend.InstallLabel;
            _updates = new(backend); _updates.Changed += RenderUpdateUi; _updates.Failed += App.Log;
            RenderUpdateUi();
        }
        catch (Exception ex) { App.Log(ex); UpdateStatus.Text = "Chưa khởi tạo được cập nhật. Hãy kiểm tra và lưu lại cấu hình nguồn."; RenderUpdateUi(); }
    }

    private async Task AutoCheckUpdatesAsync()
    {
        try
        {
            // Allow the Editor to finish opening; check at most once / 24 h / source.
            await Task.Delay(1800);
            if (_updateUiClosed || _updates == null || _updates.Stage != UpdateStage.Idle || !_updateSettings.ShouldAutoCheck(DateTimeOffset.UtcNow)) return;
            await CheckUpdatesAsync();
        }
        catch (Exception ex) { App.Log(ex); }
    }

    private async Task CheckUpdatesAsync()
    {
        if (_updates == null || _updates.IsWorking || _updates.Stage == UpdateStage.Ready) return;
        _updateSettings.LastCheckUtc = DateTimeOffset.UtcNow; _updateSettings.LastCheckedSource = _updateSettings.SourceKey;
        try { UpdateSettingsStore.Save(App.DataRoot, _updateSettings); }
        catch (Exception ex) { App.Log(ex); UpdateStatus.Text = "Không lưu được cấu hình cập nhật. Hãy kiểm tra quyền thư mục dữ liệu."; return; }
        await _updates.CheckAsync();
    }

    private void RenderUpdateUi()
    {
        if (_updateUiClosed) return;
        var stage = _updates?.Stage ?? UpdateStage.Idle;
        var working = _updates?.IsWorking == true;
        if (_updates != null) UpdateStatus.Text = _updates.Message;
        UpdateConfiguration.IsEnabled = !working && stage != UpdateStage.Ready;
        CheckUpdatesButton.IsEnabled = _updates != null && !working && stage != UpdateStage.Ready;
        DownloadUpdateButton.Visibility = stage == UpdateStage.Available && _updates?.CanApply == true ? Visibility.Visible : Visibility.Collapsed;
        OpenReleaseButton.Visibility = stage == UpdateStage.Available && _updates?.CanApply == false ? Visibility.Visible : Visibility.Collapsed;
        CancelUpdateButton.Visibility = stage == UpdateStage.Downloading ? Visibility.Visible : Visibility.Collapsed;
        ApplyUpdateButton.Visibility = stage == UpdateStage.Ready ? Visibility.Visible : Visibility.Collapsed;
        DeferUpdateButton.Visibility = stage is UpdateStage.Ready or UpdateStage.Available ? Visibility.Visible : Visibility.Collapsed;
        ApplyUpdateButton.IsEnabled = !_busy && !_updateApplying;
        UpdateProgress.Visibility = stage is UpdateStage.Checking or UpdateStage.Downloading or UpdateStage.Applying ? Visibility.Visible : Visibility.Collapsed;
        UpdateProgress.IsIndeterminate = stage != UpdateStage.Downloading;
        UpdateProgress.Value = _updates?.Progress ?? 0;
        UpdatePercent.Text = stage == UpdateStage.Downloading ? $"{_updates?.Progress ?? 0}%" : "";
        var release = _updates?.Release;
        UpdateReleasePanel.Visibility = release == null ? Visibility.Collapsed : Visibility.Visible;
        if (release != null)
        {
            UpdateVersionLabel.Text = $"V{release.Version} · Gói đầy đủ {release.Bytes / 1048576d:F1} MB";
            UpdateNotes.Text = release.Notes.Length > 24000 ? release.Notes[..24000] + "\n…" : release.Notes;
        }
        UpdateFooterButton.Content = stage == UpdateStage.Ready ? "↑ Khởi động lại để cập nhật" :
            stage == UpdateStage.Available ? $"↑ Có bản V{release?.Version}" : stage == UpdateStage.Downloading ? "↓ Đang tải cập nhật" : "↻ Cập nhật";
        UpdateCaptureHint.Visibility = _busy && stage == UpdateStage.Ready ? Visibility.Visible : Visibility.Collapsed;
        LastUpdateCheckLabel.Text = _updateSettings.LastCheckUtc is {} time ? $"Lần kiểm tra: {time.ToLocalTime():dd/MM/yyyy HH:mm}" : "Chưa kiểm tra lần nào";
    }

    private void UpdateFooter_Click(object sender, RoutedEventArgs e)
    {
        UpdatesExpander.IsExpanded = true; UpdatesExpander.BringIntoView();
        if (!_updateSettings.IsConfigured) UpdateConfiguration.IsExpanded = true;
    }

    private void SaveUpdateSource_Click(object sender, RoutedEventArgs e)
    {
        if (_updates?.IsWorking == true || _updates?.Stage == UpdateStage.Ready) return;
        try
        {
            var candidate = new UpdateSettings {
                SourceKind = SourceKindPicker.SelectedIndex == 1 ? "Https" : "GitHub",
                SourceUrl = UpdateSourceBox.Text.Trim(), AutoCheck = AutoUpdateCheck.IsChecked == true
            };
            if (candidate.IsConfigured) candidate.SourceUrl = candidate.NormalizeUrl();
            if (candidate.IsConfigured && _updateSettings.IsConfigured && candidate.SourceKey == _updateSettings.SourceKey)
            {
                candidate.LastCheckUtc = _updateSettings.LastCheckUtc; candidate.LastCheckedSource = _updateSettings.LastCheckedSource;
            }
            UpdateSettingsStore.Save(App.DataRoot, candidate); _updateSettings = candidate;
            UpdateSourceBox.Text = candidate.SourceUrl; ConfigureUpdateSession();
            SetStatus(candidate.IsConfigured ? "Đã lưu nguồn cập nhật. Bấm Kiểm tra cập nhật để kết nối." : "Đã xóa nguồn cập nhật.");
        }
        catch (Exception ex) { App.Log(ex); MessageBox.Show(this, ex.Message, "Chưa lưu được nguồn cập nhật", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private async void CheckUpdates_Click(object sender, RoutedEventArgs e) => await CheckUpdatesAsync();
    private async void DownloadUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_updates?.Stage != UpdateStage.Available || _updateCancellation != null) return;
        _updateCancellation = new();
        try { await _updates.DownloadAsync(_updateCancellation.Token); }
        finally { _updateCancellation.Dispose(); _updateCancellation = null; }
    }
    private void CancelUpdate_Click(object sender, RoutedEventArgs e) => _updateCancellation?.Cancel();
    private void DeferUpdate_Click(object sender, RoutedEventArgs e) => _updates?.Defer();
    private void ApplyUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_updates == null) return;
        _updates.Apply(_busy, () => {
            Surface.CancelInteraction();
            if (!FlushEdits()) return false;
            _store.Save(); _store.SaveSettings(Settings()); UpdateSettingsStore.Save(App.DataRoot, _updateSettings);
            MediaStore.AtomicWrite(Path.Combine(App.DataRoot, "editor-resume.json"), System.Text.Json.JsonSerializer.Serialize(Selected?.Id));
            UpdateBackup.Create(App.DataRoot, VersionInfo.Current, _updates.Release!.Version);
            _updateApplying = true; SuspendWorkflow(); VideoPreview.Close(); _videoTimer.Stop();
            return true;
        });
        // ApplyUpdatesAndRestart normally exits the process. A failure leaves the Editor usable.
        _updateApplying = false; if(_workflowSuspended)ResumeWorkflow(); _videoTimer.Start(); RenderUpdateUi();
    }

    private void RestoreEditorAfterUpdate()
    {
        var path = Path.Combine(App.DataRoot, "editor-resume.json");
        if (!File.Exists(path)) return;
        try
        {
            var id = System.Text.Json.JsonSerializer.Deserialize<string?>(File.ReadAllText(path));
            var item = _store.Items.FirstOrDefault(i => i.Id == id);
            if (item != null) { MediaList.SelectedItem = item; MediaList.ScrollIntoView(item); }
            File.Delete(path);
        }
        catch (Exception ex) { App.Log(ex); }
    }

    private void OpenRelease_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var url = _updateSettings.NormalizeUrl();
            if (_updateSettings.SourceKind == "GitHub") url += "/releases/latest";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex) { App.Log(ex); MessageBox.Show(this, ex.Message, "Không mở được nguồn phát hành"); }
    }
}
