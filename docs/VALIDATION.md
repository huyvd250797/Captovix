# Captovix V1.10.0 — Trạng thái kiểm chứng

## Đã chạy trên Linux

- 830 logic/static checks: TextClipboardChecks 49; ConnectorChecks 81; ObjectControlChecks 42; ShapeStudioChecks 115; ImageFidelityChecks 122; CaptureWorkflowChecks 53; GeometryChecks 30; EditorCoreChecks 38; AnnotationChecks 76; AdvancedAnnotationChecks 49; ImageAppearanceChecks 61; ViewportChecks 48; UpdateChecks 41; InstallerContractChecks 25.
- TextClipboardChecks kiểm tra schema 8, migration callout cũ schema 1–7, font/giãn dòng/màu không hợp lệ, preset typography, bỏ nền/Undo, copy/paste nhóm và remap connector, tháo đầu ra ngoài selection, dán sang ảnh khác giữ cỡ, khóa, renumber và rollback khi dữ liệu sai.
- Cross-build app và 6 bộ WPF: TextStudioWindowsChecks, ConnectorWindowsChecks, ObjectWindowsChecks, ShapeWindowsChecks, FidelityWindowsChecks, EditorWindowsChecks: 0 warnings/errors. Không thực thi WPF tại đây.
- Publish self-contained win-x64, gói Velopack, NSIS wrapper. SDK gói thực tế có thêm 9 kiểm tra so với UpdateChecks không có gói: tổng 839 logic/static/package checks. Kiểm tra CRC ZIP, package ID/version, hash feed, DLL, docs và config theo byte.
- So sánh pipeline capture/fidelity/image transfer, element/hotkey/updater và install-path helper với source V1.9.0 đã nhận. Giữ startup readiness/unload guard. Không đăng release/push GitHub.

## Bộ WPF mới chỉ được biên dịch

TextStudioWindowsChecks gọi cùng đường xử lý PointerDown/Move/Up mà native mouse handler sử dụng: vẽ khung, tạo bước, chọn/di chuyển khung trong Step/Arrow, Alt vẽ chồng, Undo/Redo, sync inspector và bỏ nền ngay. Có render text Việt nhiều dòng/italic/underline/căn giữa, roundtrip PNG pixel, zoom không đổi xuất, 5 callout và 3 step marker, fallback font, WPF DataObject và khởi tạo MainWindow thật. Các test này CHƯA ĐƯỢC CHẠY tại đây; không coi là bằng chứng thao tác Windows đã đạt.

## Chờ nghiệm thu Windows thật

Chạy Build.cmd và docs/TEXT-CALLOUT-STUDIO.md. Thử clipboard qua hai lần mở app và hai ảnh, Unicode/IME, Ctrl+C/V trong ô nhập, drag/capture/lost focus, chọn đối tượng qua mọi công cụ, UI layout 100–200%, nhiều màn hình, xuất chữ/callout có xoay, installer tại D:\Captovix\Captovix và update restart. Các regression hotkey/UIA/capture/GDI vẫn cần máy Windows. HDR/màu rộng đánh giá riêng.

Chưa có rich text theo từng từ, paste bitmap ngoài thành layer, OCR, scrolling capture hay Video Editor trong V1.10.
