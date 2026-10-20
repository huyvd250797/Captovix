# Captovix V1.0.2 — Sửa lỗi EXE_ARGS khi cài đặt

## Nguyên nhân đã xác nhận

Log máy người dùng báo `Mismatch between definition and access of EXE_ARGS` trong `clap_builder`. Bộ cài V1.0.1 gọi:

```text
CoreSetup.exe --silent --installto "..." -- --captovix-install-only
```

Phần truyền tham số cho app sau `--` kích hoạt lỗi đã được báo cáo trong Velopack issue #1039: đối số được định nghĩa là String nhưng đọc như OsString; lỗi ở parser trước khi chạy cài đặt/hook, có thể để tiến trình đứng. Đường `C:\Users\runneradmin\.cargo\...` trong log là đường source khi Velopack được build, không phải đường cần tạo/sửa trên máy người dùng.

Tham khảo nguồn chính: https://github.com/velopack/velopack/issues/1039 và https://github.com/velopack/velopack/pull/1088 . Bản SDK/CLI stable đang pin 1.2.161 được giữ nguyên; hotfix chủ động tránh đường truyền đối số gây lỗi, không phụ thuộc bản prerelease.

## Cách bản V1.0.2 xử lý

Lệnh cài chỉ còn các tùy chọn được hỗ trợ, không có separator/app args:

```text
CoreSetup.exe --installto "..." --log "...\setup-core.log" --verbose
```

Bộ cài lõi hiển thị tiến trình và thông báo trực tiếp. Cửa sổ bootstrap bên ngoài có trạng thái ba giai đoạn, chờ bộ cài lõi kết thúc rồi mới dọn bản NSIS V1.0. App có thể tự mở khi lõi hoàn tất; nút Hoàn tất sẽ kiểm tra mutex trước khi mở lại để tránh bản thứ hai.

Log: `%LOCALAPPDATA%\HuyVo\Captovix\Logs\setup-core.log`; log lần trước là `setup-core.log.previous`. Log Velopack mặc định của những lần cài V1.0.1 vẫn ở `%LOCALAPPDATA%\velopack`.

## Thao tác trên máy đã gặp lỗi

1. Nếu bộ cài cũ còn đứng, mở Task Manager → Details, kết thúc `CoreSetup.exe` thuộc lần cài lỗi và đóng cửa sổ cài Captovix V1.0.1. Nếu có nhiều tiến trình, xem đường dẫn/PID của tiến trình đúng lần cài trước khi kết thúc.
2. Đóng app Captovix/Capture Foundation đang mở, kết thúc quay và lưu trước khi đóng.
3. Chạy **Captovix-v1.0.2-Setup.exe**. Không cần cài V1.0.1; không cần tự gỡ V1.0.0 trước.
4. Theo dõi cửa sổ cài đặt Captovix xuất hiện ở giai đoạn 2. Chấp nhận yêu cầu prerequisite của Microsoft nếu máy còn thiếu.
5. Khi hoàn tất, mở/kiểm tra app hiển thị V1.0.2, thư viện và chú thích cũ còn nguyên. Nếu có lỗi khác, gửi log mới ở đường trên.

Không chỉnh sửa/xóa thư mục dữ liệu `%LOCALAPPDATA%\HuyVo\Captovix` hoặc dữ liệu CaptureFoundation đang được thư viện cũ tham chiếu.

## Kiểm tra và giới hạn

`tests/InstallerContractChecks` kiểm tra hồi quy dạng lệnh: không app passthrough, không silent, quoted install/log paths, gỡ cũ sau điều kiện thành công, không xóa đệ quy, tránh mở trùng. Đây là kiểm tra tĩnh cấu hình/lệnh, không phải chạy NSIS/CoreSetup trên Windows.

Build, regression và gói cập nhật được kiểm tra trong môi trường Linux. Chưa thực thi NSIS/Velopack cài–gỡ/WPF trên Windows thật trong phiên bàn giao. Nguyên nhân EXE_ARGS được xác nhận bởi log và lệnh cũ; việc chạy trọn bộ cài trên máy Boss cần được xác nhận bằng bản mới.
