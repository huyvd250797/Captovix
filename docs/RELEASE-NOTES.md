# Captovix V1.2.1 — Installer Path Fix

- Sửa lỗi bộ cài từ chối thư mục mới chưa tồn tại, bao gồm D:\Captovix\Captovix.
- Thay lệnh NSIS GetFullPathName trong bước xác thực bằng Win32 GetFullPathNameW, chuẩn hóa trước khi tạo thư mục và không yêu cầu thư mục tồn tại.
- Giữ kiểm tra ổ cục bộ cố định, quyền ghi, thư mục trống và vùng dữ liệu riêng. Tách thông báo để thấy đúng nguyên nhân nếu một kiểm tra khác không đạt.
- Giữ các tính năng Image Editor UX của V1.2.0, nguồn cập nhật và AppId/schema dữ liệu.

Để chuyển bản đã cài từ C sang D: đóng app, gỡ Captovix qua Windows Settings, chạy Setup này và chọn D:\Captovix\Captovix. Thư viện ảnh/video giữ tại vị trí hiện có. Nếu V1.2.0 bị chặn trước khi cài, chỉ cần đóng bộ cài cũ và chạy bản này.

Chưa thực thi trên Windows thật trong môi trường đóng gói. Kèm harness NSIS kiểm tra chuẩn hóa thư mục mới và checklist Windows.
