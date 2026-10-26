# Captovix V1.21.0 — Layout Precision

Windows x64 · C#/.NET10 WPF · SDK10.0.100 · runtime tự chứa10.0.13 · ScreenRecorderLib7.0.1 · Velopack1.2.161 · Tesseract5.2.0 x64 + model Việt/Anh kèm app.

Captovix = Capture + Tools + Visual + eXperience. Bộ công cụ chụp, ghi màn hình và biên tập hình ảnh trực quan.

## Chức năng mới

- Vẽ trực tiếp trong vùng thao tác quanh ảnh để tự thêm canvas, không cần lệnh trong thư viện. Vùng thao tác không tự xuất thành phần trống.
- Nút Canvas… cùng hàng công cụ vẽ hoặc Ctrl+Alt+E: thêm phần mở rộng riêng từng cạnh, chọn màu nền/trong suốt; bật/tắt tự mở rộng khi vẽ sát mép. Không resize ảnh nguồn; Undo được.
- Crop toàn đầu ra gồm chú thích; tay nắm, số pixel X/Y/Rộng/Cao, Enter/Áp dụng; Esc hủy. Giữ đối tượng sửa lại được, không raster hóa hoặc xóa pixel nguồn.
- Kết thúc nhập chữ bằng click ngoài sẽ lưu và trở về Chọn, không tạo chữ thừa. Callout có vị trí đuôi ban đầu tại chỗ; một click tạo hộp mặc định, kéo để tạo kích thước riêng.
- Thư viện dạng cây có icon/cấp/số lượng, checkbox ẩn ngày trống ghi nhớ trên máy; vẫn có12tháng và đúng số ngày. Chức năng Canvas chuyển sang công cụ vẽ. Menu bỏ nhóm Chỉnh sửa dư thừa, giữ Ctrl+Z/Ctrl+Y và nút nhanh; giữ công cụ vùng pixel.
- Giữ cache/thumbnail nền, last-good, capture/OCR, batch và logo đã chốt của1.20. Schema14 cho mở rộng từng cạnh/crop đầu ra; dữ liệu cũ không dùng chức năng mới giữ12/13.

Hướng dẫn cụ thể, giới hạn và checklist Windows: **docs/LAYOUT-PRECISION.md**. Canvas vẫn có giới hạn32.768px/chiều và100MP; không là canvas vô hạn. Tự mở rộng khi tạo chú thích, chưa tự nới khi kéo/resize đối tượng cũ. Video nâng cao và HDR/màu rộng chưa thuộc bản này.

## Cài / cập nhật

Giải nén Captovix-v1.21.0-Update-Packages.zip, đóng Captovix cả tray, chạy HuyVo.Captovix-win-Setup.exe. Máy mới chọn thư mục riêng, ví dụ D:\\Captovix\\Captovix; managed install cũ giữ installroot. App có .NET runtime; installer có Visual C++ prerequisite. Setup chưa có chữ ký nhà phát hành.

Hoặc publisher đăng riêng5asset trong Update-Packages lên public stable Release tag v1.21.0, Set as latest: full.nupkg, HuyVo.Captovix-win-Setup.exe, releases.win.json, assets.win.json, RELEASES. Khi đó Trợ giúp → Kiểm tra cập nhật dùng URL ổn định gắn sẵn. Source ZIP chỉ dành cho phát triển, không dùng cài app. Bản bàn giao chưa tự đăng GitHub.

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

27 bộ logic/static đạt1523 assertions;16 bộ WPF chỉ cross-build. Main build/publish không lỗi/cảnh báo; gói updater/feed/branding được kiểm byte/hash. Chưa chạy Windows desktop: assist/render/preview, menu/animation, capture/UIA, folder và icon sau update thực tế. Xem docs/VALIDATION.md. Kiểm tra logic không thay thế nghiệm thu Windows.

Đề xuất tiếp theo **V1.22 — Object Editing Precision**: căn theo ảnh/đối tượng, đường gióng/bắt dính, nhập tọa độ/kích thước, cải thiện chọn lớp chồng và chỉnh nhóm. Ưu tiên patch phản hồi Windows của1.21 trước. Gửi SourceZIP mới nhất này để tiếp tục; chưa tự xây1.22.
