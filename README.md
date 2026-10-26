# Captovix V1.20.0 — Image Release & Performance

Windows x64 · C#/.NET10 WPF · SDK10.0.100 · runtime tự chứa10.0.13 · ScreenRecorderLib7.0.1 · Velopack1.2.161 · Tesseract5.2.0 x64 + model Việt/Anh kèm app.

Captovix = Capture + Tools + Visual + eXperience. Bộ công cụ chụp, ghi màn hình và biên tập hình ảnh trực quan.

## Chức năng mới

- Caption nhỏ28DIP, nút32×26/glyph11; icon thao tác16/nét mềm1.1, giữ logo. Mẫu gallery64 và hình46×28; bounds thật không clip.
- Thư viện tổng dạng cây năm →12tháng → đúng ngày trong tháng → ảnh/video, kể cả ngày trống; mở nhánh mới nạp, Xem thêm100mục/ngày, Ctrl/Shift nhiều ngày. Gần đây tối đa50mục giữ nguyên.
- Footer thư viện một hàng4nút và … cho chức năng còn lại; không mất tính năng.
- Bố cục → Vẽ ngoài ảnh / Ctrl+Alt+E: nới canvas160px mỗi cạnh, khung/callout/chữ/mũi tên nằm ngoài ảnh vẫn edit và export; nguồn không resize, kích thước xuất tăng có chủ đích, Undo được. Chưa tự mở rộng vô hạn/crop toàn canvas.
- Cache thumbnail16MiB/128entry,2worker nền; decode PNG lớp256MiB; renderer sampling riêng thread và redaction cache đồng bộ. Báo cáo render/cache/RAM/history trong Trợ giúp.
- Sidecar last-good và fallback có validate/hash/schema; giữ file lỗi, không giả khôi phục nét/chữ chưa lưu. Project thường schema12; project vẽ ngoài ảnh schema13 cần1.20+. Backup/import/update giữ dữ liệu nguồn, không reset thư viện.

Chi tiết và giới hạn: **docs/IMAGE-RELEASE-PERFORMANCE.md**. Giữ batchPNG/JPEG/PDF/OCR/capture/quay và các chức năng trước. Chỉ cache có budget, không là trần RAM toàn app; tile-render/HDR/Windows profiling chưa hoàn tất. `docs/PERFORMANCE-BENCHMARK.json` chỉ là pixel-copy4K/8K tổng hợp trên Linux.

## Cài / cập nhật

Giải nén Captovix-v1.20.0-Update-Packages.zip, đóng Captovix cả tray, chạy HuyVo.Captovix-win-Setup.exe. Máy mới chọn thư mục riêng, ví dụ D:\\Captovix\\Captovix; managed install cũ giữ installroot. App có .NET runtime; installer có Visual C++ prerequisite. Setup chưa có chữ ký nhà phát hành.

Hoặc publisher đăng riêng5asset trong Update-Packages lên public stable Release tag v1.20.0, Set as latest: full.nupkg, HuyVo.Captovix-win-Setup.exe, releases.win.json, assets.win.json, RELEASES. Khi đó Trợ giúp → Kiểm tra cập nhật dùng URL ổn định gắn sẵn. Source ZIP chỉ dành cho phát triển, không dùng cài app. Bản bàn giao chưa tự đăng GitHub.

## Build từ source

Cài .NET10 SDK và NSIS3 trên Windows. Mở PowerShell tại thư mục source:

```powershell
dotnet restore src/Captovix/Captovix.csproj
dotnet run --project src/Captovix/Captovix.csproj -c Release -p:Platform=x64
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Kết quả ở artifacts. scripts/Run-Checks.py chạy các bộ logic và thực thi WPF trên Windows; trên Linux chỉ cross-build WPF. Đóng app trước cài/update. Không thêm dấu -- trước --installto trong installer vì gây lỗi EXE_ARGS.

## Dữ liệu

%LOCALAPPDATA%\\HuyVo\\Captovix: Media, Edits, library.json, appearance-settings.json, recent-capture-regions.json, ScrollSessions, OcrJobs, media-location.json, Brand, branding.log, capture.log và errors.log. Thư mục media có thể chọn riêng cho file mới; file cũ giữ đường dẫn. Metadata/Edits không chuyển theo media. Giữ dữ liệu khi update; sao lưu toàn thư mục trước nâng cấp/rollback. Tài liệu Editor vẫn schema12. Phiên cuộn mới schema2 đọc phiên1; app1.14 không mở phiên2. Khi loại khung, phiên gốc vẫn giữ và revision là thư mục mới. Không tự xóa frame nguồn; dọn phiên đã hết nhu cầu bằng Explorer.

## Kiểm tra và tiếp tục

27 bộ logic/static đạt1501 assertions;16 bộ WPF chỉ cross-build. Main build/publish không lỗi/cảnh báo; gói updater/feed/branding được kiểm byte/hash. Chưa chạy Windows desktop: assist/render/preview, menu/animation, capture/UIA, folder và icon sau update thực tế. Xem docs/VALIDATION.md. Kiểm tra logic không thay thế nghiệm thu Windows.

Đề xuất tiếp theo **V1.21 — Canvas & Layout Precision**: nhập khoảng nới riêng từng cạnh, crop toàn canvas gồm chú thích và tùy chọn tự nới khi chạm mép. Ưu tiên patch kết quả nghiệm thu Windows của1.20 trước khi mở thêm phạm vi. Gửi SourceZIP mới nhất này để tiếp tục; chưa tự xây1.21.
