# Validation — Captovix V1.7.0

Nền Captovix-v1.6.1-Source.zip (SHA256 f814c4365944d78a2dd150ea3fd9c0c04c79832ef5d32b8d29c3a54be65fa23e). Giữ .NET SDK 10.0.100/runtime Windows 10.0.13, ScreenRecorderLib 7.0.1, Velopack SDK/CLI 1.2.161 và NSIS 3.08. Giữ bản vá V1.6.1 GDI và V1.5.1 unload; schema annotation lên 5.

## Đã chạy trên Linux

ShapeStudio 115, ImageFidelity 122, CaptureWorkflow 53, Geometry 30, EditorCore 38, Annotation 76, AdvancedAnnotation 49, ImageAppearance 61, Viewport 48, Update core 41, InstallerContract 25: **658 kiểm tra logic/tĩnh**. ShapeStudio gồm migration 1–4, JSON/copy/duplicate/resize đầy đủ style, undo/redo, style không hợp lệ, normalized polygon/star, khóa góc tại biên, preset cũ/mới và kiểm tra file không bị ghi đè khi lỗi.

Cross-build app WPF và 3 bộ Windows checks: ShapeWindowsChecks, EditorWindowsChecks, FidelityWindowsChecks. Publish self-contained Windows x64; compile NSIS wrapper. Kiểm tra gói thật qua SDK sau đóng gói và kiểm tra ZIP/feed/identity/hash/size/DLL/default URL trước bàn giao; xem PACKAGE-VALIDATION.txt trong source cho kết quả cuối.

## Chưa chạy trên Windows

Không có Windows host/VM/native UI trong môi trường build. Chưa chạy ShapeWindowsChecks (renderer/fill/dash/arrow/hit/surface/zoom/roundtrip), EditorWindowsChecks (thêm inspector, defaults và migration backup), FidelityWindowsChecks hoặc probe SDR. Các bộ này chỉ cross-build, không cộng vào số checks đã chạy.

Chưa xác minh thao tác chuột thật, layout thực tế, DPI 100/125/150/175/200%, native GDI/cursor, clipboard, cài/update/restart/path D, sidecar migration backup bằng API Windows. Không có report Windows đạt trong ZIP. V1.7: triển khai/build xong, CHỜ NGHIỆM THU WINDOWS. Không đăng GitHub Release.

## Giới hạn

SDR8; HDR/ICC/wide gamut/16-bit chưa đánh giá. Polygon tự do/eyedropper/layer panel/rotate object là nội dung tương lai. File schema 5 không dùng ngược bằng app cũ; backup Edits trước khi rollback. Trường thuộc tính cần Áp dụng trước khi đổi lựa chọn. Cả viền/nền cùng tắt/0% tạo shape vô hình có thể phục hồi bằng Undo/chọn tất cả. Không thay đổi originals hay video engine.
