# Captovix V1.11.0 — Trạng thái kiểm chứng

## Đã chạy trên Linux

- 895 logic/static checks: FocusPrivacyChecks 65; TextClipboardChecks 49; ConnectorChecks 81; ObjectControlChecks 42; ShapeStudioChecks 115; ImageFidelityChecks 122; CaptureWorkflowChecks 53; GeometryChecks 30; EditorCoreChecks 38; AnnotationChecks 76; AdvancedAnnotationChecks 49; ImageAppearanceChecks 61; ViewportChecks 48; UpdateChecks 41; InstallerContractChecks 25.
- FocusPrivacyChecks: 6 loại vùng × 2 hình, serialization/clone; schema 1–8/legacy highlight alpha; từ chối NaN/Infinity/range/enum; tâm mẫu kính, move/resize/duplicate/clipboard; Undo/Redo; focus preset; lớp bảo vệ, review mask ẩn/vượt crop; cache key theo vùng che/crop; giới hạn mẫu ngoài crop.
- Thêm 9 kiểm tra SDK gói full thực tế so với UpdateChecks không có gói = 904 logic/static/package checks. Gồm đọc version thật, download/hash, từ chối hash sai/same/older/wrong AppId/prerelease.
- Cross-build app và 7 bộ WPF: FocusPrivacyWindowsChecks, TextStudioWindowsChecks, ConnectorWindowsChecks, ObjectWindowsChecks, ShapeWindowsChecks, FidelityWindowsChecks, EditorWindowsChecks. 0 cảnh báo/lỗi; không thực thi WPF tại đây.
- Publish self-contained win-x64, Velopack full nupkg và NSIS wrapper. Verifier kiểm tra CRC ZIP, feed SHA1/SHA256/size, AppId/version, DLL/docs/config/runtime theo byte. Capture/fidelity/image transfer/hotkey/UIA/updater/install-path nền so sánh với source V1.10.0 đã gửi; giữ startup/unload guard.

## Bộ WPF bổ sung — CHỈ BIÊN DỊCH, CHƯA CHẠY

FocusPrivacyWindowsChecks dùng hai ảnh khác nhau ở vùng bí mật đã được che, kiểm tra bản render không đổi khi có kính, blur, che đặc, hai hình vùng, xoay 0/33°, zoom 1.25/3/8. Kiểm tra che đặc vẫn opaque dù opacity chung 0; save/reopen; spotlight union/ẩn/tối ngoài; ellipse hit test; crop/zoom/xoay toàn ảnh; PNG flattened; pointer chung với native handler; inspector Apply và MainWindow startup.

Các bài này chưa có kết quả chạy tại nơi build. Cần chạy trên Windows để xác nhận thực tế compositing, antialias, màu và pixel bảo vệ. Không được tuyên bố privacy đã được chứng nhận chỉ dựa vào source/test build.

## Chờ nghiệm thu Windows

Theo docs/FOCUS-PRIVACY.md: thao tác chọn/Alt vẽ chồng, preset, đổi loại che, Undo/copy/paste; PNG/JPEG/clipboard; lens trên mask với thứ tự lớp khác nhau; mask sát biên/ellipse/xoay; mixed DPI 100–200%, monitor âm, hiệu năng 4K/8K và nhiều mask; install D:\Captovix\Captovix và update restart. Giữ checklist text/IME, hotkey/UIA/GDI/capture kế thừa. HDR/màu rộng riêng.

Không push/deploy/publish GitHub Release ở lần bàn giao này. Không OCR/tự tìm dữ liệu nhạy cảm, không chứng minh blur/pixel không phục hồi, không nhúng image layers hoặc phát triển Video Editor trong V1.11.
