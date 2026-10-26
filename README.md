# Captovix V1.15.1 — Capture Reliability

Windows x64 · C#/.NET10 WPF · SDK10.0.100 · runtime tự chứa10.0.13 · ScreenRecorderLib7.0.1 · Velopack1.2.161.

Captovix = Capture + Tools + Visual + eXperience. Bộ công cụ chụp, ghi màn hình và biên tập hình ảnh trực quan.

## Chức năng mới

- Chụp cuộn có thời gian chờ 350–5000 ms và 1–3 nấc mỗi nhịp; mặc định 700 ms, 1 nấc.
- Tạm dừng nếu nguồn mất focus, bị che tại điểm kiểm tra, di chuyển, resize hoặc đóng; lý do được hiển thị rõ và ghi capture.log.
- Dọn frame chưa commit khi hủy/lỗi; loại khung chỉ đổi revision sau khi commit thành công; sửa kiểm tra overlap ngang/header-footer.
- Dark mode nền than xám, chữ dịu, menu/submenu/context menu hover tối; công cụ và nền quanh ảnh theo theme.
- Logo tím đã chốt được gán cho cửa sổ/tray/EXE/shortcut; file ICO có đường dẫn mới. Shortcut ghim cũ có thể cần bỏ ghim/ghim lại.
- Trợ giúp → Giới thiệu Captovix có tên phiên bản và diễn giải thương hiệu.

Hướng dẫn và checklist Windows: docs/CAPTURE-RELIABILITY.md. Giữ toàn bộ công cụ ảnh/chụp/quay của 1.15.0. Chưa có ứng dụng nguồn nào được nghiệm thu trên Windows thật; bộ kiểm tra tổng hợp không chứng minh trang thực tế luôn không mất/lặp dòng/cột.

## Cài / cập nhật

Giải nén Captovix-v1.15.1-Update-Packages.zip, đóng Captovix cả tray, chạy HuyVo.Captovix-win-Setup.exe. Máy mới chọn thư mục riêng, ví dụ D:\\Captovix\\Captovix; managed install cũ giữ installroot. App có .NET runtime; installer có Visual C++ prerequisite. Setup chưa có chữ ký nhà phát hành.

Hoặc publisher đăng riêng5asset trong Update-Packages lên public stable Release tag v1.15.1, Set as latest: full.nupkg, HuyVo.Captovix-win-Setup.exe, releases.win.json, assets.win.json, RELEASES. Khi đó Trợ giúp → Kiểm tra cập nhật dùng URL ổn định gắn sẵn. Source ZIP chỉ dành cho phát triển, không dùng cài app. Bản bàn giao chưa tự đăng GitHub.

## Build từ source

Cài .NET10 SDK và NSIS3 trên Windows. Mở PowerShell tại thư mục source:

```powershell
dotnet restore src/Captovix/Captovix.csproj
dotnet run --project src/Captovix/Captovix.csproj -c Release -p:Platform=x64
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Kết quả ở artifacts. scripts/Run-Checks.py chạy các bộ logic và thực thi WPF trên Windows; trên Linux chỉ cross-build WPF. Đóng app trước cài/update. Không thêm dấu -- trước --installto trong installer vì gây lỗi EXE_ARGS.

## Dữ liệu

%LOCALAPPDATA%\\HuyVo\\Captovix: Media, Edits, library.json, appearance-settings.json, recent-capture-regions.json, ScrollSessions và errors.log. Giữ dữ liệu khi update; sao lưu toàn thư mục trước nâng cấp/rollback. Tài liệu Editor vẫn schema12. Phiên cuộn mới schema2 đọc phiên1; app1.14 không mở phiên2. Khi loại khung, phiên gốc vẫn giữ và revision là thư mục mới. Không tự xóa frame nguồn; dọn phiên đã hết nhu cầu bằng Explorer.

## Kiểm tra và tiếp tục

22 bộ logic/static chạy trên Linux; 11 bộ WPF chỉ cross-build. Chi tiết kết quả trong docs/VALIDATION.md. Windows runtime/capture/install/update chưa nghiệm thu. Tiếp theo V1.16 — OCR & Text Extraction: chọn vùng/toàn ảnh để nhận chữ, copy Unicode, kiểm tra tiếng Việt và Anh; engine/ngôn ngữ sẽ được đánh giá khi xây dựng. Lần sau gửi Source ZIP mới nhất này.

Repo source có thể git init/add/commit rồi push lên repo riêng; với repo có sẵn, clone/fetch/pull và tích hợp trước, không force push. Desktop WPF phân phối bằng Windows Releases, không deploy app lên Vercel/Netlify. URL phát hành cấu hình trong src/Captovix/update-source.json, không thêm token. Không cần database hoặc biến môi trường để dùng app offline.
