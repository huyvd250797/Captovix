# Captovix V1.2.1 — Cập nhật và phát hành

## Người dùng cài mới

1. Chạy Captovix-v1.2.1-Setup.exe.
2. Mở Captovix → Cập nhật → Kiểm tra cập nhật.
3. Nguồn đã gắn sẵn: https://github.com/huyvd250797/captovix-releases.
4. Khi có bản cao hơn: Tải cập nhật → Khởi động lại để cập nhật. Kết thúc chụp/quay trước khi áp dụng.

Mặc định tự kiểm tra mỗi 24 giờ, không tự tải hoặc ép cài. Có thể tắt tự kiểm tra và dùng nút thủ công.

## Máy đang V1.0.2–V1.1.0

Nếu đã lưu URL trên, không cần nhập lại: sau khi V1.2.1 được Publish, bấm Kiểm tra cập nhật → Tải → Khởi động lại. Có thể chạy Setup V1.2.1 để nâng trực tiếp. Dữ liệu giữ tại %LOCALAPPDATA%\HuyVo\Captovix; không cần gỡ app trước. Bộ cài hỗ trợ chuyển trực tiếp từ V1.0.0; nếu lần cài V1.0.1 còn treo, xem INSTALLER-RECOVERY.md.

Nguồn riêng đã lưu ưu tiên hơn mặc định mới. Nguồn rỗng đã lưu vẫn tắt nguồn; cần nhập URL một lần trong trường hợp này. Cấu hình lỗi được báo để người dùng sửa, không âm thầm đổi sang nguồn khác.

## Chủ app phát hành mỗi phiên bản

1. Xây dựng gói có Version cao hơn, ví dụ 1.2.1; giữ AppId HuyVo.Captovix, channel win, win-x64 và URL trên.
2. Giải nén Captovix-v1.2.1-Update-Packages.zip.
3. Mở https://github.com/huyvd250797/captovix-releases/releases/new.
4. Tạo tag v1.2.1, target main, title Captovix V1.2.1 — Image Editor UX.
5. Gắn 5 file vào phần Assets của Release:
   - HuyVo.Captovix-1.2.1-full.nupkg
   - HuyVo.Captovix-win-Setup.exe
   - releases.win.json
   - assets.win.json
   - RELEASES
6. Chờ upload xong, chọn phát hành ổn định (không Pre-release), bấm Publish release.
7. Trên máy V1.0.2–V1.1.0 đã cấu hình nguồn, kiểm tra/tải/khởi động lại và xác nhận hiển thị V1.2.1, thư viện cũ còn nguyên.

Không cần upload các file này lên tab Code; updater đọc Release Assets. Không upload mỗi ZIP ngoài, không dùng Source.zip, không đổi tên hay sửa JSON/hash. Không sửa tag V1.0.2 để thay gói mới: mỗi phiên bản là một Release riêng. Giữ các Release trước. Máy đã chạy V1.2.1 sẽ báo mới nhất cho đến khi Publish gói có version cao hơn. Đổi tên file/tag không thay được version bên trong gói.

## Build trên Windows

Cần .NET 10 SDK và NSIS 3. Chạy Build.cmd; update-source.json trong source đã có URL chính thức, build bình thường giữ nguyên mặc định này. Chỉ dùng tham số UpdateSource/UpdateKind nếu chủ động xây dựng bản cho nguồn khác:

```powershell
.\scripts\Build-Windows.ps1
# Nguồn khác nếu cần:
.\scripts\Build-Windows.ps1 -UpdateSource 'https://github.com/OWNER/REPO' -UpdateKind GitHub
```

Kết quả ở artifacts/: Setup, Portable, Update-Packages.zip và releases/. CoreSetup.exe là file trung gian, dùng bản Setup đầy đủ khi chuyển từ bản NSIS cũ. Có thể cung cấp PreviousReleases chứa nupkg đã phát hành để tạo delta; thiếu delta thì updater dùng gói full.

Script Publish-GitHub.ps1 tạo draft khi dùng VPK_TOKEN trong shell phát hành; không đưa token vào app/source. Bản bàn giao không tự đăng Release lên GitHub.

## Dữ liệu và giới hạn

Backup trước áp dụng gồm metadata và Edits JSON, không gồm media gốc. App giữ ảnh/video trên máy, không tải media lên GitHub. Portable chỉ kiểm tra/mở trang tải thủ công; dùng Setup để cập nhật trong app. Nguồn công khai không cần đăng nhập. Chưa hỗ trợ private repository, tự rollback dữ liệu, chữ ký nhà phát hành độc lập; Setup chưa có Authenticode của HuyVo. HTTPS/SHA-256 kiểm tra tính toàn vẹn gói, không thay thế ký mã.

Gói được cross-build và kiểm tra SDK với feed local; chưa chạy WPF/Setup/Update.exe trên Windows thật. Xem VALIDATION.md và WINDOWS-TEST-CHECKLIST.md.

V1.1 dùng schema chú thích 2. Backup trước nâng cấp giữ tài liệu cũ; tránh mở ngược chú thích đã lưu bởi V1.1 bằng app V1.0. Xem IMAGE-ANNOTATION.md.

V1.2: nâng qua updater giữ thư mục cài hiện tại. Muốn chuyển ổ, xem INSTALL-LOCATION.md và dùng Setup.
