# Validation — Captovix V1.5.0

Source đầu vào: Captovix-v1.4.0-Source(1).zip do Boss cung cấp. C# .NET 10 WPF x64, SDK 10.0.100, runtime Windows 10.0.13, Velopack SDK/CLI 1.2.161 và ScreenRecorderLib 7.0.1 giữ nguyên.

## Kiểm thử logic

Geometry 12; EditorCore 38; Annotation 76; AdvancedAnnotation 49; ImageAppearance 61; Viewport 48; Update core 41; InstallerContract 25; CaptureWorkflow 53: tổng 403 ca logic/tĩnh. UpdateChecks chạy thêm 9 ca SDK trên nupkg thực tế sau đóng gói, tổng 412.

Workflow kiểm tra opt-in mặc định, roundtrip cấu hình/phím/preset/vùng, bảo vệ file hỏng/schema mới, tên/giới hạn preset, trùng phím/phím dành riêng, phím Windows không hợp lệ, topology/monitor identity/DPI/âm tọa độ, xung đột đăng ký và rollback, gate đồng thời, quoting/giới hạn autorun và backup workflow. Image/viewport giữ kiểm tra 2000 ma trận và 4000 crop ngẫu nhiên.

## Build và đóng gói

Cross-build app và EditorWindowsChecks, publish win-x64 self-contained, đóng gói Velopack và biên dịch bộ cài NSIS. Harness WPF thêm kiểm tra cấu hình workflow trong cùng Editor, mặc định và preset riêng; chỉ biên dịch trên Linux, chưa thực thi WPF.

Kiểm tra gói: phiên bản/AppId/hash/size/feed, URL mặc định, DLL payload giống nupkg/Portable, ZIP CRC và whitelist release assets, Setup canonical giống standalone. Source có README, CHANGELOG, ROADMAP và checklist Windows. Không tự đăng GitHub Release.

## Giới hạn kiểm chứng

Chưa chạy cài đặt/update restart, tray và Explorer restart, hotkey thật/foreground/focus, clipboard/thông báo, đăng nhập Windows/autorun, overlay chụp thật, screen/audio/DPI end-to-end. Các ca logic dùng adapter giả cho đăng ký hotkey và không chứng minh Win32 đã hoạt động trên máy người dùng. Cần chạy docs/WINDOWS-TEST-CHECKLIST.md trước phát hành rộng.

V1.5 giữ schema ảnh 4 và dữ liệu ảnh/chú thích cũ. Cấu hình workflow có schema 1 riêng; không downgrading bằng ghi đè cấu hình mới. Chưa ký chứng thư Authenticode. Chưa thêm xử lý video nâng cao/chụp cuộn/OCR/cloud.
