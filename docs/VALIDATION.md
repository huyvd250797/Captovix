# Captovix V1.12.0 — Kết quả kiểm chứng (23/10/2026)

## Đã thực thi trên Linux

1029 logic/static checks, tất cả đạt: AdvancedAnnotationChecks 49; AnnotationChecks 76; CaptureWorkflowChecks 53; ConnectorChecks 81; EditorCoreChecks 38; FocusPrivacyChecks 64; GeometryChecks 30; ImageAppearanceChecks 61; ImageFidelityChecks 122; InstallerContractChecks 25; ObjectControlChecks 42; PixelStudioChecks 57; ShapeStudioChecks 115; ShellLayoutChecks 18; StudioReliabilityChecks 44; TextClipboardChecks 54; UpdateChecks 52; ViewportChecks 48.

Sau đóng gói chạy thêm9 kiểm tra SDK với full nupkg thực tế (download/hash, phiên bản mới, app khác, same/older/prerelease/thiếu hash và hash sai). Tổng logic/static/package sau gói thực tế: 1038. Xem PACKAGE-VALIDATION.txt và script Verify-Packages.py để kiểm tra artifact.

Pixel checks kiểm tra vùng chữ nhật/ellipse/lasso, scanline khớp geometric mask, giữ nguyên byte bên ngoài và input, fill/erase/alpha/extract/move/clamp, PNG header/budget, schema11/Undo/Redo/clipboard lớp ảnh,10 kiểu callout, head None và migration schema1–10. Hash favorite cũ được đối chiếu với bản V1.11.1 thực đã biên dịch từ ZIP đầu vào, không chỉ tính lại bằng mã mới. Các kiểm tra shell kiểm tra hệ thống nằm trong staging cho modal, menu cây, nhóm công cụ, nút gọn, thư viện và unique names.

## Chỉ cross-build — CHƯA thực thi Windows

App WPF win-x64 và9 harness: ConnectorWindowsChecks, EditorWindowsChecks, FidelityWindowsChecks, FocusPrivacyWindowsChecks, ObjectWindowsChecks, PixelStudioWindowsChecks, ShapeWindowsChecks, StudioReliabilityWindowsChecks, TextStudioWindowsChecks. Không có runtime Windows trong môi trường build này. Không đánh dấu UI/input/render/install/clipboard/restart cập nhật đã đạt chỉ vì biên dịch thành công.

PixelStudioWindowsChecks bao phủ: PNG nền sửa không đổi pixel ngoài vùng, alpha lớp dán, native-shared pointer chọn/di chuyển vùng, zoom không đổi export;10 geometry callout; MainWindow startup/maximized; đổi thư viện trái/dưới; mở modal và đổi các node để phát hiện lỗi logical parent. Các kiểm tra WPF kế thừa cho text/IME/shape/object/fidelity/privacy tiếp tục được biên dịch.

## Giới hạn và nghiệm thu bắt buộc

Theo WINDOWS-TEST-CHECKLIST.md và SELECTION-PIXEL-EDITOR.md: Windows thật ở DPI100–200%/monitor âm, modal/phím tắt/capture, layout khi1366/1920, callout cũ xoay và đuôi mới, PNG/Bitmap clipboard ngoài app, raster layer/Blur/lens/Cover cũ, Undo/redo/save/reopen, RAM và thời gian ảnh4K/8K≤40MP, cài mới/update từ1.11.1 tại D:\Captovix\Captovix. HDR/màu rộng đánh giá riêng.

Giữ nguyên Native/GDI/capture engine/ImageFidelity/ImageTransfer/hotkey/UIA/program và install-path nền so với ZIP V1.11.1; installer chỉ đổi version. Payload self-contained .NET10.0.13, ScreenRecorderLib7.0.1, Velopack1.2.161, NSIS3.09. Package CRC/feed SHA1/SHA256/size/canonical installer/source tree cần chạy Verify-Packages.py sau đóng gói.

Copy vùng lấy phần hiển thị; Cut/Fill/Erase chỉ sửa nền, đối tượng giữ riêng. Không cut-out hàng/cột, flood-fill/magic-wand/feather, chỉnh pixel riêng từng lớp hay đuôi callout nhiều nhánh. Pixel budget40MP, PNG và lịch sử có giới hạn; xem hướng dẫn để không nhầm với khả năng đã triển khai.

Chưa push/publish GitHub Release; không phát triển video editor. V1.13 Composition & Templates là kế hoạch.
