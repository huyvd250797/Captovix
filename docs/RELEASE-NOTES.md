# Captovix V1.4.0 — Image Transform & Styling

- Xoay trái/phải 90°, lật ngang/dọc theo ảnh đang hiển thị.
- Resize ảnh theo pixel, giữ tỷ lệ hoặc nhập riêng rộng/cao; khôi phục cỡ gốc theo crop/xoay hiện tại.
- Canvas riêng với 9 vị trí đặt ảnh. Canvas nhỏ hơn ảnh cắt phần vượt khung, không xóa dữ liệu gốc.
- Thêm khoảng đệm, viền, màu nền hoặc nền trong suốt; bóng đổ mềm của khung với độ mềm/lệch/đậm.
- Mẫu khung Sáng/Tối/Bỏ kiểu. Xem kích thước đầu ra gồm canvas, đệm, viền và bóng.
- Ảnh/chú thích gốc giữ nguyên tọa độ. Preview, thao tác chuột, inline text, thumbnail, copy và xuất ảnh dùng cùng phép biến đổi; hỗ trợ undo/redo và lưu mở lại.
- PNG/clipboard giữ trong suốt; JPEG ghép nền trắng ở phần trong suốt. Giới hạn đầu ra 32.768 px mỗi chiều và 100 triệu pixel.
- Schema 4 mở chú thích schema 1/2/3; không mở ngược file đã lưu bằng app V1.3 trở xuống. Giữ cơ chế backup trước cập nhật.

Mở ảnh → bảng bên phải → Ảnh & khung · V1.4. Các trường số/màu áp dụng khi bấm nút Áp dụng của từng nhóm; xoay/lật và preset áp dụng ngay. Bộ cài giữ chức năng chọn thư mục (kể cả D:\Captovix\Captovix), nguồn GitHub mặc định và Editor mở phóng to. Video giữ nguyên tính năng hiện tại.

Đã chạy kiểm tra logic/tĩnh/SDK và biên dịch Windows x64; chưa chạy UI/render/bộ cài trên Windows thật trong môi trường đóng gói.
