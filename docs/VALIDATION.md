# V1.15.1 — Kiểm tra ngày 26/10/2026

- App WPF build và publish Windows x64 tự chứa runtime10.0.13 trên Linux: thành công, 0 lỗi/0 cảnh báo.
- 22 bộ logic/static, 1330 assertions đạt. Gồm 52 kiểm tra mới: giới hạn thời gian chờ/nấc cuộn và mọi tổ hợp trạng thái nguồn (đóng/di chuyển/focus/che).
- 11 bộ WPF cross-build thành công. Bổ sung kiểm tra template submenu/hover/disabled và tương phản tài nguyên ở Light/Dark vào ScrollAppearanceWindowsChecks; chưa thực thi bộ WPF trên Windows.
- Đo 8 cặp màu chữ/nền thường, hover, selected và workspace của Light/Dark: đều >=4,5:1; Dark hover 8,33:1. Đây là kiểm tra màu khai báo, không thay thế kiểm tra render và cảm nhận trên màn hình thật.
- Xác minh trực tiếp PE của Captovix.exe: 9 kích thước icon nhúng khớp ICO artwork tím đã chốt; ICO runtime khớp byte. Không đổi thiết kế logo.
- Gói full.nupkg thật được chạy qua UpdateChecks; Verify-Packages kiểm tra feed/hash/size, sourceZIP, updateZIP, payload, runtime, stable URL và installer wrapper. Xem PACKAGE-VALIDATION.txt cho kết quả đóng gói.

Chưa chạy Windows desktop: hover/menu/keyboard, icon taskbar/tray/shortcut ghim cũ, cuộn và focus trên từng app, chụp/clipboard/HDR/DPI, installer/update từ1.15.0. Không tuyên bố app nguồn nào đã nghiệm thu. Checklist cụ thể trong CAPTURE-RELIABILITY.md; không thay kết quả thực tế bằng số assertions.
