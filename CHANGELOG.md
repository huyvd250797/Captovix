# Captovix changelog

## 1.0.3 — Default Update Source (2026-10-20)

- Gắn sẵn nguồn GitHub Releases công khai: https://github.com/huyvd250797/captovix-releases.
- Cài mới rồi chọn Cập nhật → Kiểm tra cập nhật, không cần nhập URL.
- Giữ cấu hình nguồn riêng đã lưu, lựa chọn tự kiểm tra và lịch sử ảnh/video/chú thích.
- Duy trì sửa lỗi EXE_ARGS và bộ cài có tiến trình/log từ V1.0.2.
- Tăng phiên bản gói lên 1.0.3 để máy V1.0.2 nhận qua updater sau khi chủ app Publish release mới.

Máy V1.0.2 đã lưu đúng nguồn có thể cập nhật trong app. Cũng có thể chạy Setup V1.0.3 trực tiếp. Nguồn rỗng đã lưu vẫn được giữ theo lựa chọn cũ; trường hợp này nhập URL một lần. Portable chỉ kiểm tra/mở trang tải thủ công. Chưa thực thi cài đặt và cập nhật trên Windows thật trong môi trường đóng gói.

## 1.0.2 — Installer Recovery (2026-10-20)

- Sửa lỗi bộ cài V1.0.1 bị đứng ở CoreSetup.exe: bỏ tham số truyền ứng dụng sau `--` gây panic `EXE_ARGS` trong Velopack 1.2.161 (issue #1039).
- Hiển thị cửa sổ tiến trình/lỗi của bộ cài lõi thay vì chạy silent; bộ cài ngoài hiển thị 3 giai đoạn.
- Log chi tiết cố định tại `%LOCALAPPDATA%\HuyVo\Captovix\Logs\setup-core.log`, giữ lần trước thành `.previous`.
- Nút hoàn tất kiểm tra app đã mở để tránh chạy thêm bản thứ hai.
- Giữ cơ chế chỉ gỡ NSIS V1.0 sau khi cài bản mới thành công, dữ liệu ngoài thư mục cài được giữ.
- Thêm kiểm tra hồi quy hợp đồng tham số/lệnh cài; giữ các chức năng updater và Unified Editor của V1.0.1.

Có thể cài trực tiếp V1.0.0 → V1.0.2, không cần cài V1.0.1 trước. Nguồn phát hành mặc định vẫn trống. Cần kiểm tra trên Windows thực tế trước khi triển khai rộng.


# Captovix changelog

## 1.0.1 — Update Foundation (2026-10-19)

- Kiểm tra phiên bản mới ngay trong Editor; tự kiểm tra mỗi 24 giờ hoặc bấm thủ công.
- Nguồn GitHub Releases công khai hoặc thư mục HTTPS, có thể cấu hình trong ứng dụng.
- Hiển thị phiên bản mới và nội dung thay đổi; tải nền, tiến trình, hủy, thử lại và hoãn.
- Bản cài đặt: khởi động lại để áp dụng cập nhật bằng Velopack.
- Lưu chú thích, cấu hình và thư viện; sao lưu metadata/Edits trước khi áp dụng. Chặn cập nhật khi đang chụp/quay hoặc lưu lỗi.
- Kiểm tra AppId, phiên bản stable tăng dần, kích thước và SHA-256 gói.
- Bộ cài đầu tiên chuyển từ NSIS V1.0 sang Velopack; dữ liệu nằm riêng và được giữ lại.
- Portable chỉ kiểm tra và mở nguồn tải thủ công. App tiếp tục mở ở chế độ Maximized.

Nguồn phát hành chưa được thiết lập sẵn. Cấu hình trước khi kiểm tra cập nhật trực tuyến.


# Changelog

## 1.0.0 — Captovix Unified Editor

- Brand, namespace, assembly, app icon, launcher and installer renamed Captovix.
- Editor starts maximized; capture restores the previous window state.
- Editable arrows, rectangles, ellipses, multiline text, translucent highlights and numbered steps.
- Selection, moving, endpoint/corner handles, color, stroke, font/text/number properties and deletion.
- Non-destructive crop with restore-original-frame and undo/redo (100 operations per current image opening).
- Sidecar JSON autosave/reopen with original-content hash checks and invalid-document backup.
- Shared render path for editor and full-resolution PNG/JPEG/clipboard export; thumbnails reflect saved edits.
- V0.1 metadata/settings migration without modifying or duplicating original media.
- Capture countdown cancellation on close; original-file overwrite prevention.
- 38 editor/history/migration checks, 12 geometry checks, Windows-specific renderer/document/UI checks in source.

Builds and core tests do not establish Windows desktop runtime verification. See docs/VALIDATION.md.

## 0.1.0 — Capture Foundation

Initial single-window capture/recording foundation, local media library and Windows packaging.
