using System.Diagnostics;
using System.Runtime.InteropServices;
using Velopack.Locators;

namespace Captovix;

internal sealed class RecoveryWindow : Form
{
    private readonly string _content;
    private readonly Label _status = new() { Dock = DockStyle.Fill, AutoEllipsis = true };
    private readonly TextBox _notes = new()
    {
        Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
        BackColor = SystemColors.Window
    };
    private readonly Button _retry = new() { Text = "Thử lại", AutoSize = true };
    private readonly Button _start = new() { Text = "Mở bản hiện tại", AutoSize = true, Visible = false };
    private readonly Button _update = new() { Text = "Cập nhật & mở lại", AutoSize = true, Visible = false };
    private VelopackUpdateBackend? _backend;
    private WindowsVelopackLocator? _locator;
    private UpdateRelease? _release;
    private bool _busy;

    internal RecoveryWindow(string content, bool startupFailure)
    {
        _content = content;
        Text = "Trung tâm khôi phục Captovix";
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        FormBorderStyle = FormBorderStyle.None;
        DoubleBuffered = true;
        Padding = new Padding(1);
        var settings = AppearanceSettings.Load(RecoveryProgram.DataRoot, out _);
        var palette = AppearancePalette.For(AppearanceSettings.UseDark(settings.Mode, AppearancePalette.SystemLight()));
        Color Ink(string color) => ColorTranslator.FromHtml(color);
        BackColor = Ink(palette.PopupLine);
        ForeColor = Ink(palette.Text);
        var chrome = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Ink(palette.AppBackground) };
        chrome.RowStyles.Add(new(SizeType.Absolute, 36));
        chrome.RowStyles.Add(new(SizeType.Percent, 100));
        Controls.Add(chrome);
        MinimumSize = new(660, 380);
        Size = new(740, 445);
        Font = new Font("Segoe UI", 10);
        Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        MaximizeBox = false;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new(24), RowCount = 5, ColumnCount = 1 };
        layout.RowStyles.Add(new(SizeType.Absolute, 40));
        layout.RowStyles.Add(new(SizeType.Absolute, 40));
        layout.RowStyles.Add(new(SizeType.Absolute, 54));
        layout.RowStyles.Add(new(SizeType.Percent, 100));
        layout.RowStyles.Add(new(SizeType.Absolute, 46));
        var titleBar = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Ink(palette.CaptionSurface) };
        var titleIcon = new PictureBox { Image = Icon?.ToBitmap(), SizeMode = PictureBoxSizeMode.Zoom, Width = 20, Dock = DockStyle.Left, Margin = Padding.Empty };
        var title = new Label { Text = Text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8,0,0,0) };
        var dismiss = new Button { Text = "×", Dock = DockStyle.Right, Width = 44, AccessibleName = "Đóng" };
        titleBar.Padding = new Padding(10,0,0,0);
        titleBar.Controls.Add(title);
        titleBar.Controls.Add(titleIcon);
        titleBar.Controls.Add(dismiss);
        foreach (var item in new Control[] { titleBar, title, titleIcon })
            item.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 2, 0); } };
        dismiss.Click += (_, _) => Close();
        chrome.Controls.Add(titleBar,0,0);
        chrome.Controls.Add(layout,0,1);
        layout.Margin = Padding.Empty;
        _notes.BackColor = Ink(palette.Surface);
        _notes.ForeColor = ForeColor;
        _notes.TabStop = false;
        layout.Controls.Add(new Label { Dock = DockStyle.Fill, Text = startupFailure ? "Captovix chưa thể khởi động" : "Khôi phục khởi động", Font = new Font(Font.FontFamily, 18, FontStyle.Bold) });
        layout.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Thử mở lại ứng dụng hoặc kiểm tra bản vá từ kênh cập nhật chính." });
        layout.Controls.Add(_status);
        layout.Controls.Add(_notes);

        var footer = new Panel { Dock = DockStyle.Fill };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
        var report = new Button { Text = "Mở báo cáo lỗi", AutoSize = true };
        var close = new Button { Text = "Đóng", AutoSize = true, Dock = DockStyle.Left };
        actions.Controls.AddRange([_update, _retry, _start, report]);
        footer.Controls.Add(actions);
        footer.Controls.Add(close);
        layout.Controls.Add(footer);
        foreach (var button in new[] { dismiss, close, report, _start, _retry, _update })
        {
            bool primary = button == _retry || button == _update;
            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderColor = Ink(palette.Line);
            button.FlatAppearance.BorderSize = button == dismiss || primary ? 0 : 1;
            button.FlatAppearance.MouseOverBackColor = Ink(primary ? settings.Accent : palette.Hover);
            button.BackColor = Ink(primary ? settings.Accent : button == dismiss ? palette.CaptionSurface : palette.Soft);
            button.ForeColor = primary ? (AppearanceSettings.WhiteText(settings.Accent) ? Color.White : Color.Black) : ForeColor;
            if (button != dismiss) button.Padding = new Padding(8,4,8,4);
        }
        CancelButton = close;
        close.Click += (_, _) => Close();
        report.Click += (_, _) => OpenReport();
        _retry.Click += async (_, _) => await RunAsync(CheckAndRetryAsync);
        _start.Click += async (_, _) => await RunAsync(LaunchAndReportAsync);
        _update.Click += async (_, _) => await RunAsync(InstallAndRestartAsync);
        FormClosing += (_, e) => { if (_busy) e.Cancel = true; };
        AcceptButton = _retry;
        var version = FileVersionInfo.GetVersionInfo(Environment.ProcessPath!).ProductVersion;
        _status.Text = "Trung tâm khôi phục · V" + version;
        _notes.Text = startupFailure ? "Chọn Thử lại để kiểm tra bản mới. Chi tiết lần khởi động lỗi nằm trong báo cáo." :
            "Chọn Thử lại để kiểm tra bản mới và thử mở Captovix. Bản cài đặt giữ nguyên thư viện và ảnh gốc.";
    }

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint wParam, nint lParam);

    private async Task RunAsync(Func<Task> action)
    {
        if (_busy) return;
        _busy = true;
        _retry.Enabled = _start.Enabled = _update.Enabled = false;
        try { await action(); }
        catch (Exception error)
        {
            Log(error);
            _status.Text = "Chưa hoàn tất. " + error.GetBaseException().Message;
        }
        finally
        {
            _busy = false;
            _retry.Enabled = _start.Enabled = _update.Enabled = true;
        }
    }

    private async Task CheckAndRetryAsync()
    {
        _release = null;
        _update.Visible = _start.Visible = false;
        AcceptButton = _retry;
        _notes.Clear();
        _status.Text = "Đang kiểm tra phiên bản mới…";
        try
        {
            _locator = InstalledLocator();
            var defaults = Path.Combine(_content, "update-source.json");
            // The publisher channel remains available even if a damaged app
            // installation lost its defaults file.
            var settings = UpdateSettingsStore.Load(RecoveryProgram.DataRoot, defaults, out var warning);
            if (warning != null) throw new InvalidDataException(warning);
            if (!settings.IsConfigured)
                settings = new() { SourceKind = "Https", SourceUrl = UpdateSettings.StableFeed };
            _backend = new VelopackUpdateBackend(settings, _locator, _locator.CurrentlyInstalledVersion!.ToString());
            _release = await _backend.CheckAsync();
            if (_release != null)
            {
                _status.Text = $"Có bản mới V{_release.Version} · đang cài V{_locator.CurrentlyInstalledVersion}.";
                var notes = _release.Notes;
                _notes.Text = notes.Length <= 4000 ? notes : notes[..4000] + "…";
                _update.Visible = _start.Visible = true;
                AcceptButton = _update;
                return;
            }
            _notes.Text = $"V{_locator.CurrentlyInstalledVersion} đã là bản mới nhất trên nguồn cập nhật.";
        }
        catch (Exception error)
        {
            Log(error);
            _notes.Text = "Chưa kiểm tra được cập nhật. " + error.GetBaseException().Message;
        }
        await LaunchAndReportAsync();
    }

    private WindowsVelopackLocator InstalledLocator()
    {
        var executable = Path.Combine(_content, "Captovix.exe");
        var root = Path.GetDirectoryName(_content);
        if (root == null || !Path.GetFileName(_content).Equals("current", StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(Path.Combine(root, "Update.exe")) || !File.Exists(Path.Combine(_content, "sq.version")))
            throw new InvalidOperationException("Cần bản cài Captovix để cập nhật. Có thể thử mở app hoặc dùng Full Setup.");

        var locator = new WindowsVelopackLocator(new InstalledProcess(executable), null);
        if (locator.AppId != VersionInfo.AppId || locator.Channel != VersionInfo.Channel ||
            locator.CurrentlyInstalledVersion == null || locator.IsPortable ||
            !string.Equals(root, locator.RootAppDir, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Thư mục không thuộc bản cài Captovix chính.");
        return locator;
    }

    private async Task InstallAndRestartAsync()
    {
        if (_release is not {} release || _backend is not {} backend || _locator is not {} locator) return;
        EnsureAppClosed();
        _status.Text = "Đang tải và kiểm tra gói cập nhật…";
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(15));
        var downloading = true;
        var progress = new Progress<int>(percent => { if (downloading) _status.Text = $"Đang tải bản V{release.Version} · {percent}%"; });
        try { await backend.DownloadAsync(release, ((IProgress<int>)progress).Report, deadline.Token); }
        finally { downloading = false; }

        using var gate = new Mutex(false, StartupHealthSignal.RepairGate);
        if (!Acquire(gate)) throw new InvalidOperationException("Đang có phiên sửa lỗi khác.");
        try
        {
            EnsureAppClosed();
            var installed = InstalledLocator();
            var from = locator.CurrentlyInstalledVersion!.ToString();
            if (!Equals(installed.CurrentlyInstalledVersion, locator.CurrentlyInstalledVersion))
                throw new InvalidOperationException("Phiên bản đã thay đổi trong khi tải. Bấm Thử lại để kiểm tra lại.");

            _status.Text = "Đang sao lưu thiết lập và dữ liệu chỉnh sửa…";
            var backup = await Task.Run(() => UpdateBackup.Create(RecoveryProgram.DataRoot, from, release.Version));
            var package = await Task.Run(() => backend.VerifyReadyPackage(release));
            var journal = new RecoveryState(from, release.Version, backup, "Applying", DateTimeOffset.UtcNow);
            ReleaseRecoveryState.Write(RecoveryProgram.DataRoot, journal);
            var log = Path.Combine(RecoveryProgram.DataRoot, "Recovery", "repair-updater.log");
            try
            {
                _status.Text = $"Đang cài V{release.Version} vào thư mục hiện tại…";
                var start = new ProcessStartInfo(locator.UpdateExePath!) { UseShellExecute = false, WorkingDirectory = locator.RootAppDir! };
                foreach (var arg in new[] { "--rootDir", locator.RootAppDir!, "--packageDir", locator.PackagesDir!, "--log", log, "apply", "--norestart", "--package", package })
                    start.ArgumentList.Add(arg);
                using var updater = Process.Start(start) ?? throw new IOException("Không mở được trình cài cập nhật.");
                await updater.WaitForExitAsync();
                if (updater.ExitCode != 0 || InstalledLocator().CurrentlyInstalledVersion!.ToString() != release.Version)
                    throw new IOException("Chưa xác nhận được bản mới đã cài. Xem báo cáo rồi dùng Thử lại hoặc Full Setup.");
            }
            catch
            {
                ReleaseRecoveryState.Write(RecoveryProgram.DataRoot, journal with { Phase = "ApplyFailed", At = DateTimeOffset.UtcNow });
                throw;
            }
        }
        finally { gate.ReleaseMutex(); }

        _release = null;
        _update.Visible = false;
        AcceptButton = _retry;
        _notes.Text = $"Đã cài V{release.Version}. Đang xác nhận màn hình chính mở được.";
        await LaunchAndReportAsync();
    }

    private async Task LaunchAndReportAsync()
    {
        EnsureAppClosed();
        _status.Text = "Đang thử mở Captovix…";
        var id = Guid.NewGuid().ToString("N");
        using var ready = new EventWaitHandle(false, EventResetMode.ManualReset, StartupHealthSignal.Prefix + id);
        var executable = Path.Combine(_content, "Captovix.exe");
        var start = new ProcessStartInfo(executable) { WorkingDirectory = _content, UseShellExecute = false };
        start.ArgumentList.Add(StartupHealthSignal.Argument + id);
        using var app = Process.Start(start) ?? throw new IOException("Không thể mở Captovix.exe.");
        var limit = Stopwatch.StartNew();
        while (limit.Elapsed < TimeSpan.FromSeconds(40))
        {
            if (app.HasExited)
            {
                _status.Text = $"Captovix đã thoát trước khi mở được màn hình chính (mã {app.ExitCode}).";
                Log(new InvalidOperationException(_status.Text));
                return;
            }
            if (ready.WaitOne(0))
            {
                app.Refresh();
                if(app.HasExited||app.MainWindowHandle==0||!IsWindowVisible(app.MainWindowHandle)||IsIconic(app.MainWindowHandle))
                {
                    await Task.Delay(200);
                    continue;
                }
                _status.Text = "Đã khôi phục. Màn hình chính của Captovix đã mở.";
                _notes.Text = "Boss có thể đóng Trung tâm khôi phục và tiếp tục sử dụng Captovix.";
                _start.Visible = _update.Visible = false;
                return;
            }
            await Task.Delay(200);
        }
        _status.Text = "Chưa xác nhận Captovix mở được màn hình chính. Nếu ứng dụng đang treo, hãy đóng nó rồi thử lại.";
        Log(new TimeoutException("Không nhận tín hiệu màn hình chính sau 40 giây."));
    }

    [DllImport("user32.dll")]
    [return:MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(nint window);
    [DllImport("user32.dll")]
    [return:MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(nint window);

    private static bool Acquire(Mutex gate)
    {
        try { return gate.WaitOne(0); }
        catch (AbandonedMutexException) { return true; }
    }

    private static void EnsureAppClosed()
    {
        if (Mutex.TryOpenExisting(StartupHealthSignal.AppMutex, out var instance))
        {
            instance.Dispose();
            throw new InvalidOperationException("Captovix đang chạy. Hãy thoát ứng dụng cả ở khay hệ thống trước.");
        }
    }

    private static void Log(Exception error)
    {
        try
        {
            Directory.CreateDirectory(RecoveryProgram.DataRoot);
            File.AppendAllText(Path.Combine(RecoveryProgram.DataRoot, "errors.log"), $"{DateTimeOffset.Now:O} Recovery Center: {error}\n");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static void OpenReport()
    {
        try
        {
            var errors = Path.Combine(RecoveryProgram.DataRoot, "errors.log");
            var updater = Path.Combine(RecoveryProgram.DataRoot, "Recovery", "repair-updater.log");
            var path = File.Exists(updater) && File.GetLastWriteTimeUtc(updater) > File.GetLastWriteTimeUtc(errors) ? updater : errors;
            if (!File.Exists(path))
            {
                MessageBox.Show("Chưa có báo cáo lỗi.", "Captovix");
                return;
            }
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true })?.Dispose();
        }
        catch (Exception error) { MessageBox.Show(error.Message, "Captovix"); }
    }

    private sealed class InstalledProcess(string executable) : IProcessImpl
    {
        public string GetCurrentProcessPath() => executable;
        public uint GetCurrentProcessId() => (uint)Environment.ProcessId;
        public void StartProcess(string exePath, IEnumerable<string> args, string workDir, bool showWindow)
            => throw new NotSupportedException("Recovery launches the updater explicitly and observes its exit status.");
        public void Exit(int exitCode) => throw new NotSupportedException();
    }
}
