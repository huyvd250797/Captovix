# Validation — Captovix V1.6.1

Nền: Captovix-v1.6.0-Source.zip. Giữ .NET SDK 10.0.100, Windows runtime 10.0.13, Velopack SDK/CLI 1.2.161, ScreenRecorderLib 7.0.1; schema ảnh 4, URL cập nhật và hotfix chuyển ảnh V1.5.1.

## Đã chạy trên Linux

ImageFidelity 122, CaptureWorkflow 53, Geometry 30, EditorCore 38, Annotation 76, AdvancedAnnotation 49, ImageAppearance 61, Viewport 48, Update core 41, InstallerContract 25: **543 kiểm tra logic/tĩnh**. Geometry thêm 18 kiểm tra vùng ảnh lẻ/chẵn/1 pixel, tọa độ âm và qua màn hình, giữ diện tích và mép ảnh. Đây là kiểm tra mô hình, không chạy GDI trên Linux.

Cross-build ứng dụng WPF, FidelityWindowsChecks và EditorWindowsChecks thành công. Publish self-contained Windows x64; compile NSIS wrapper. Tính năng mới: ảnh desktop qua GDI BitBlt không qua engine căn chẵn, pixel alpha opaque SDR8; thông báo lỗi runtime có điều kiện.

Thêm **9 kiểm tra SDK trên nupkg thực tế** (hash/package/version/channel), tổng **552**. Kiểm tra CRC ZIP, version/AppId/feed SHA1/SHA256/size và DLL giữa payload/nupkg; URL nguồn phát hành giữ nguyên. Chưa đăng GitHub Release.

## Chưa chạy trên Windows

Không có Windows host/VM. Chưa chạy native GDI, cursor/hotspot, Clipboard, installer/update/restart, WPF renderer/PNG, monitor âm hay DPI thực tế. Không đánh dấu nghiệm thu các mức 100/125/150/175/200%.

Probe Windows dùng cùng CaptureEngine/backend desktop mới. Ngoài mẫu 256×192 và full-display/window, thêm các mẫu 255×192, 256×191, 255×191, 1×1 so mọi byte BGRA, cùng hồi quy kích thước 1467×386 khi màn hình đủ lớn. Phải chạy chức năng Kiểm chứng SDR để có report thật; gói này không chứa report đạt Windows.

Cần kiểm tra thêm vùng sát mép/qua màn hình ở DPI khác nhau, bật/tắt cursor, không rò handle khi chụp liên tục, đổi ảnh/editor, crop/rotate/copy/export, cập nhật giữ dữ liệu và đường dẫn D:\Captovix\Captovix. Protected content/secure desktop có thể không chụp được. HDR/wide gamut/ICC/16-bit chưa đánh giá. GDI lấy hình desktop đang hiển thị; chụp cửa sổ độc lập vẫn dùng backend cũ.
