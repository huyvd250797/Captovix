# Captovix V1.11.1 — Trạng thái kiểm chứng

## Đã chạy

950 logic/static checks trên Linux, tất cả đạt. Chi tiết: AdvancedAnnotationChecks 49; AnnotationChecks 76; CaptureWorkflowChecks 53; ConnectorChecks 81; EditorCoreChecks 38; FocusPrivacyChecks 65; GeometryChecks 30; ImageAppearanceChecks 61; ImageFidelityChecks 122; InstallerContractChecks 25; ObjectControlChecks 42; ShapeStudioChecks 115; StudioReliabilityChecks 44; TextClipboardChecks 49; UpdateChecks 52; ViewportChecks 48.

Thêm 9 kiểm tra SDK với gói full thực tế sau đóng gói: đọc version/package thật, tải và kiểm tra hash, từ chối SHA-256 sai, app khác, older/same/prerelease và thiếu hash. Tổng sau gói thực tế: 959 checks. Xem PACKAGE-VALIDATION.txt về kết quả đóng gói cuối.

Hồi quy mới kiểm tra đủ 9 hướng bóng, down-center/halo, clone không dùng chung bóng; schema 1–9 lên 10; favorites giữ kiểu và bỏ qua vị trí/ID, backup/corrupt/duplicate; cấu hình viền; modifier routing và scroll bounds; Trash giữ byte, phục hồi, xung đột và rollback khi lưu thất bại. Updater SDK đọc newest ở vị trí15, giữ custom publisher/opt-out, phiên bản nguồn khi up-to-date và feed rỗng không báo mới nhất.

## Chỉ biên dịch, chưa chạy Windows

App WPF win-x64 và 8 harness: StudioReliabilityWindowsChecks, FocusPrivacyWindowsChecks, TextStudioWindowsChecks, ConnectorWindowsChecks, ObjectWindowsChecks, ShapeWindowsChecks, FidelityWindowsChecks, EditorWindowsChecks. Không có runtime Windows trong môi trường build; không có bằng chứng chạy UI/cài đặt/native input tại đây.

StudioReliabilityWindowsChecks bổ sung MainWindow startup/maximized, palette transparent rồi khôi phục màu, auto-property TextChanged, gallery giữ selection và chuyển công cụ vẽ tiếp, grid bóng qua handler thật, pixel bóng xuống dưới, viền ngoài giữ pixel, zoom không đổi ảnh xuất. Harness privacy kế thừa tiếp tục bao phủ protected sampling/mask/lens, tile/crop và pipeline export. Cross-build thành công không chứng nhận kết quả render/input.

## Đóng gói và phần cần nghiệm thu

Publish .NET10.0.13 self-contained Windows x64; Velopack1.2.161 full nupkg; NSIS wrapper gồm VC++ runtime. So sánh feed SHA1/SHA256/size, CRC ZIP, AppId/version, DLL/payload/docs/config và source. Giữ nguyên installer invocation tránh EXE_ARGS, install-path normalizer, GDI/native capture, hotkey/UIA và pixel transfer nền của source V1.11.0.

Cần chạy checklist UPDATE-RELIABILITY.md và WINDOWS-TEST-CHECKLIST.md trên Windows thật: cập nhật/restart từ v1.9/v1.11; install D:\Captovix\Captovix; palette/gallery/live typing/đổi ảnh/Undo; xóa/khôi phục khi file khóa; wheel modifiers; viền chụp/auto-copy; bóng khi xoay/crop; mixed DPI100–200%/monitor âm, IME, clipboard và privacy compositing/4K/8K. HDR/màu rộng đánh giá riêng.

Không push/publish GitHub trong lần bàn giao này. Không bổ sung video editor; V1.12 Selection & Pixel Editing là kế hoạch.
