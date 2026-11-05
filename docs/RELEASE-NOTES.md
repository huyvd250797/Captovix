# V1.46.0 — Mockup Fidelity & Editing Reliability

- Thuộc tính Mockup có mẫu màu, bảng màu nhanh, font mẫu, B/I/U dạng nút và bản xem trước. Áp dụng cho cả rà soát trước quét và inspector sau áp dụng.
- Ctrl+B/Ctrl+I/Ctrl+U nhận chữ Mockup. Mở/đóng inline không đổi nội dung sẽ hủy giao dịch; giữ số thực font đầy đủ trong inspector để không tự ghi vì làm tròn.
- Nút có label luôn tách thành lớp thân nút và lớp nhãn phía trên, kể cả UI Automation chỉ trả tên nút. Có nút chọn từng lớp; thay thân không cập nhật thuộc tính chữ.
- Đổi nền control phẳng giữ glyph raster của caption chưa sửa qua alpha mask, có cache; không thay font khi chỉ đổi màu thân. Font OCR dùng thêm đối chiếu nét ảnh và khung; font UI đã xác nhận vẫn được ưu tiên.
- Biểu tượng chỉnh màu ở góc phải menu. Màu nhấn và icon có màu đơn/gradient hai điểm/hướng; đồng bộ hoặc chọn màu icon riêng. Logo header/cửa sổ/tray theo cấu hình; EXE/installer/shortcut giữ logo tĩnh gốc.
- Bỏ mục Cập nhật & khôi phục dữ liệu khỏi menu, giữ backup/kiểm gói/cứu hộ nội bộ. Nút Quay đỏ tươi/chữ trắng.
- Không có SQL/migration mới; schema tài liệu tối đa 23 và appearance schema 1 giữ tương thích. Chưa nghiệm thu Windows native; không tuyên bố nhận diện chính xác mọi font hoặc phục hồi mọi nền phức tạp.
