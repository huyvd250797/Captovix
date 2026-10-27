# Captovix V1.22.0 — Object Editing Precision

Windows x64 · C#/.NET10 WPF · SDK10.0.100 · runtime tự chứa10.0.13 · ScreenRecorderLib7.0.1 · Velopack1.2.161 · Tesseract5.2.0 x64 + model Việt/Anh kèm app.

Captovix = Capture + Tools + Visual + eXperience. Bộ công cụ chụp, ghi màn hình và biên tập hình ảnh trực quan.

## Chức năng mới

- Hover gợi ý đối tượng; Alt+click khi ở Chọn để chuyển lớp; menu chuột phải có danh sách đối tượng/nhóm tại con trỏ, kể cả khóa để mở khóa. Không chọn nhầm lớp ẩn.
- Căn theo lựa chọn, vùng ảnh đang thấy hoặc toàn canvas; chia đều khoảng trống theo hướng ảnh hiện tại. Mỗi nhóm là một đơn vị; reference được nhớ trên máy.
- Snap mép/tâm theo canvas, vùng ảnh và đối tượng khác; ngưỡng theo giao diện, Ctrl tạm bỏ snap, Shift khóa hướng màn hình; đường gióng không đi vào file xuất.
- X/Y/rộng/cao theo pixel đầu ra; góc được ghi rõ theo hệ nguồn; nhóm/đường dùng góc xoay thêm, không áp lặp khi sửa trường kế tiếp. Sửa số có Undo/Redo, không đổi kích thước ảnh/canvas.
- Checkbox giữ tỷ lệ dùng cho cả nhập và kéo. Tay nắm cạnh/góc giữ đối diện, bù điểm click để tránh nhảy; giảm tay nắm của đối tượng nhỏ; kích thước gizmo theo giao diện. Nhóm có chữ/callout dừng ở giới hạn phù hợp.
- Nudge1px đầu ra, Shift+nudge10px; nhóm/khóa/thứ tự lớp tiếp tục có trong chuột phải. Nhân bản/copy/paste ngoài ảnh giữ miền canvas mở rộng. Phần nguồn không tràn vào viền khi annotation được đặt ngoài ảnh.
- Giữ Canvas/Crop1.21, cache, last-good, OCR/capture/quay, logo và nguồn cập nhật. Không thêm schema dự án; đọc12/13/14; preference thêm vào settings máy.

Hướng dẫn, giới hạn và checklist Windows: **docs/OBJECT-EDITING-PRECISION.md**. Canvas vẫn có giới hạn32.768px/chiều và100MP. Không có canvas vô hạn, nhóm lồng nhau hoặc editor video mới. Nghiệm thu Windows còn chờ.

## Cài / cập nhật

Giải nén Captovix-v1.22.0-Update-Packages.zip, đóng Captovix cả tray, chạy HuyVo.Captovix-win-Setup.exe. Máy mới chọn thư mục riêng, ví dụ D:\\Captovix\\Captovix; managed install cũ giữ installroot. App có .NET runtime; installer có Visual C++ prerequisite. Setup chưa có chữ ký nhà phát hành.

Hoặc publisher đăng riêng5asset trong Update-Packages lên public stable Release tag v1.22.0, Set as latest: full.nupkg, HuyVo.Captovix-win-Setup.exe, releases.win.json, assets.win.json, RELEASES. Khi đó Trợ giúp → Kiểm tra cập nhật dùng URL ổn định gắn sẵn. Source ZIP chỉ dành cho phát triển, không dùng cài app. Bản bàn giao chưa tự đăng GitHub.

## Build từ source

Cài .NET10 SDK và NSIS3 trên Windows. Mở PowerShell tại thư mục source:

```powershell
dotnet restore src/Captovix/Captovix.csproj
dotnet run --project src/Captovix/Captovix.csproj -c Release -p:Platform=x64
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Kết quả ở artifacts. scripts/Run-Checks.py chạy các bộ logic và thực thi WPF trên Windows; trên Linux chỉ cross-build WPF. Đóng app trước cài/update. Không thêm dấu -- trước --installto trong installer vì gây lỗi EXE_ARGS.

## Dữ liệu

%LOCALAPPDATA%\\HuyVo\\Captovix: Media, Edits, library.json, appearance-settings.json, recent-capture-regions.json, ScrollSessions, OcrJobs, media-location.json, Brand, branding.log, capture.log và errors.log. Thư mục media có thể chọn riêng cho file mới; file cũ giữ đường dẫn. Metadata/Edits không chuyển theo media. Giữ dữ liệu khi update; sao lưu toàn thư mục trước nâng cấp/rollback. Tài liệu Editor đọc schema12/13/14; schema phụ thuộc chức năng đã dùng. Phiên cuộn mới schema2 đọc phiên1; app1.14 không mở phiên2. Khi loại khung, phiên gốc vẫn giữ và revision là thư mục mới. Không tự xóa frame nguồn; dọn phiên đã hết nhu cầu bằng Explorer.

## Kiểm tra và tiếp tục

28 bộ logic/static đạt1615 assertions;17 bộ WPF chỉ cross-build. Main build/publish không lỗi/cảnh báo; gói updater/feed/branding được kiểm byte/hash. Chưa chạy Windows desktop: assist/render/preview, menu/animation, capture/UIA, folder và icon sau update thực tế. Xem docs/VALIDATION.md. Kiểm tra logic không thay thế nghiệm thu Windows.

Đề xuất tiếp theo **V1.23 — Text & Callout Refinement**: hoàn thiện nhập chữ/IME, căn lề/giãn dòng/đệm, hộp co theo chữ, đuôi callout tự do và mẫu xem trước nhất quán. Ưu tiên patch phản hồi Windows của1.22 trước. Gửi SourceZIP mới nhất này để tiếp tục; chưa tự xây1.23.
