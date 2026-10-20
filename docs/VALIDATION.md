# Validation — Captovix V1.0.3

Ngày đóng gói: 20/10/2026. Source đầu vào V1.0.2 đã bàn giao. Phạm vi sửa: version/nhãn, update-source.json, lựa chọn file đóng gói và tài liệu phát hành. Không thay engine capture, schema thư viện hoặc cơ chế apply.

## Đã kiểm tra

- Cross-build/publish C#/WPF Windows x64 thành công. .NET self-contained 10.0.13; ScreenRecorderLib 7.0.1; Velopack SDK/CLI 1.2.161.
- 12 geometry, 38 editor/history/migration, 40 update state/config/backup và 10 installer static contract checks đạt (100 checks).
- Các checks settings xác nhận default được đọc, cấu hình riêng được ưu tiên, nguồn rỗng đã lưu giữ tắt nguồn và cấu hình lỗi không tự đổi publisher.
- Gói mặc định chứa SourceKind=GitHub, SourceUrl=https://github.com/huyvd250797/captovix-releases, AutoCheck=true.
- Sau khi tạo gói, chạy riêng 9 SDK integration checks với nupkg thật (UpdateChecks nhận đường dẫn gói), gồm download/SHA-256 và từ chối AppId/hash/version không hợp lệ; xác minh ZIP/hash/size và default trong nupkg trước bàn giao.

## Giới hạn

Chưa chạy WPF/NSIS/CoreSetup.exe/Update.exe trên Windows thật trong môi trường đóng gói. Kiểm tra geometry/settings và installer contract không thay thế kiểm thử cài–gỡ, restart, capture, DPI/âm thanh bằng Windows desktop. Xem WINDOWS-TEST-CHECKLIST.md. Chưa tự đăng Release lên GitHub; chủ app phải Publish gói V1.0.3 trước khi máy V1.0.2 thấy cập nhật.

Nguồn riêng đã lưu không bị ghi đè. Không có Authenticode HuyVo, tự rollback hoặc private feed. Portable chỉ kiểm tra và mở trang tải thủ công.
