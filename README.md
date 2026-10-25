# Captovix V1.15.0 — Advanced Capture & Stitching

Windows x64 · C#/.NET10 WPF · SDK10.0.100 · runtime tự chứa10.0.13 · ScreenRecorderLib7.0.1 · Velopack1.2.161.

## Chức năng mới

- Chụp cuộn dọc/ngang, bật/tắt tự cuộn, tạm dừng/tiếp tục; dừng khi3 lần không thấy nội dung mới hoặc điểm nối chưa chắc.
- Header/footer nhập pixel cho chụp dọc, giữ một lần hoặc bỏ. Xem hai khung, sửa overlap, loại khung lỗi tạo revision; giữ phiên gốc để khôi phục.
- Vùng cố định nhập X/Y/rộng/cao;8 vùng gần đây; chụp client đang hiển thị; vùng menu hẹn giờ0–15giây.
- Giữ bộ công cụ ảnh, logo tím, màu cá nhân, Light/Dark/System và một Editor maximized.

Hướng dẫn, giới hạn, phím tắt và checklist: docs/ADVANCED-CAPTURE.md. Tự cuộn gửi nhịp cuộn tới ứng dụng đích; ứng dụng không nhận sẽ dùng thủ công. Chưa có danh sách ứng dụng được nghiệm thu Windows; kiểm tra thuật toán không chứng minh trang thực tế không mất/lặp dòng/cột.

## Cài / cập nhật

Giải nén Captovix-v1.15.0-Update-Packages.zip, đóng Captovix cả tray, chạy HuyVo.Captovix-win-Setup.exe. Máy mới chọn thư mục riêng, ví dụ D:\\Captovix\\Captovix; managed install cũ giữ installroot. App có .NET runtime; installer có Visual C++ prerequisite. Setup chưa có chữ ký nhà phát hành.

Hoặc publisher đăng riêng5asset trong Update-Packages lên public stable Release tag v1.15.0, Set as latest: full.nupkg, HuyVo.Captovix-win-Setup.exe, releases.win.json, assets.win.json, RELEASES. Khi đó Trợ giúp → Kiểm tra cập nhật dùng URL ổn định gắn sẵn. Source ZIP chỉ dành cho phát triển, không dùng cài app. Bản bàn giao chưa tự đăng GitHub.

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

21 bộ logic/static đạt1278 kiểm tra;11 bộ WPF cross-build. Windows runtime/capture/install/update chưa nghiệm thu. Chi tiết docs/VALIDATION.md. Sau nghiệm thu đề xuất V1.15.1 củng cố tương thích cuộn/điểm nối; V1.16 OCR & Text Extraction vẫn theo roadmap. Lần sau gửi Source ZIP mới nhất này.

Repo source có thể git init/add/commit rồi push lên repo riêng; với repo có sẵn, clone/fetch/pull và tích hợp trước, không force push. Desktop WPF phân phối bằng Windows Releases, không deploy app lên Vercel/Netlify. URL phát hành cấu hình trong src/Captovix/update-source.json, không thêm token. Không cần database hoặc biến môi trường để dùng app offline.
