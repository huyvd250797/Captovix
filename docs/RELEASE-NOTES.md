# Captovix V1.5.1 — Editor Switch Fix

- Sửa NullReferenceException khi chuyển giữa các ảnh: UnloadEditor đã xóa canvas nhưng callback VisualChanged còn đọc Layout qua phiên chỉnh sửa cũ.
- Dọn session trước khi canvas phát sự kiện unload; callback chỉ đọc layout khi session/canvas khớp và có ảnh, xóa thông tin kích thước/zoom khi rỗng.
- Bảng thuộc tính ảnh cũng xử lý trạng thái thiếu layout, không dùng dữ liệu ảnh cũ trong lúc chuyển.
- Bổ sung 11 ca hồi quy WPF: surface trống với session cũ, unload lặp, chuyển ảnh khác kích thước, lưu chỉnh sửa, tách sự kiện session cũ, 30 vòng đổi ảnh, bỏ chọn và chọn ảnh mới.
- Giữ ảnh gốc, sidecar schema 4, thư viện, cấu hình workflow và nguồn cập nhật GitHub. Không reset dữ liệu để sửa lỗi.

Phạm vi kiểm chứng: build/publish Windows x64 và test logic/SDK có thể chạy tại Linux. Bộ hồi quy WPF đã bổ sung và biên dịch; chưa thực thi trên Windows thật. Cần nghiệm thu chuyển ảnh theo WINDOWS-TEST-CHECKLIST.md.

Phiên bản kế tiếp: V1.6 — Image Fidelity. Rà capture→file→preview→clipboard→export để giữ đúng pixel nguồn, Actual Pixels, PNG gốc và nhiều màn hình/DPI. V1.7 tiếp tục Shapes & Stroke Studio; V1.8 nâng Object Control & Layers.
