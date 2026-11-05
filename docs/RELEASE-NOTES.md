# V1.44.0 — M6 Advanced UI Reconstruction

- Ghép UI/OCR, loại khung trùng; liên kết label–input, nhóm theo hàng/cột và panel, giữ tọa độ và khoảng cách nguồn. Nhận cấu trúc Table/Tab/Menu/Icon từ UI và gợi ý model; đường lưới bảng có thêm bộ phân tích ảnh giới hạn. Không tự chuyển hàng loạt; icon/đường lưới/nền phức tạp có thể giữ pixel nguồn.
- Bổ sung nghiêng, gạch chân, màu chữ từ UIA khi có đủ thông tin và OCR khớp. Mở rộng ước lượng font serif/mono/nghiêng. Rà soát có xác nhận định dạng và giữ dạng ảnh; font suy ra từ ảnh không được coi là chắc chắn.
- Double-click Mockup sửa chữ ngay trên ảnh; chọn đối tượng xem đầy đủ thuộc tính ở sidebar phải: nội dung, font/cỡ/B/I/U, căn ngang/dọc, padding/giãn dòng, bốn màu, viền/bo góc/loại, OCR có Dừng, lấy màu và so sánh nhỏ trước/sau. Công cụ nền/mặt nạ vẫn có lối truy cập riêng. Tạo Mockup mới giữ luồng rà soát hiện có.
- Sửa nội dung không tự fit lại cỡ chữ của Mockup. Giữ định dạng khi Undo/Redo/lưu dự án; font gốc phải được cài trên máy để render đúng.
- AI tùy chọn dùng Ollama vision API: chọn model đã cài, endpoint local/HTTPS, lưu cấu hình, lấy danh sách, timeout/hủy, giới hạn phản hồi. Chỉ chạy khi bấm Phân tích AI; gửi bản xem vùng quét, không gửi tự động khi chụp. keep_alive=0 yêu cầu dịch vụ giải phóng model sau phiên. Không kèm trọng số model hay tải model lúc khởi động.
- Lịch sử Undo chia sẻ chuỗi PNG/mặt nạ bất biến và nén phần JSON, thay vì lặp Base64 cho mỗi bước. Cache raster giảm96→48MiB, giữ thumbnail16MiB; giải phóng bản xem khi đổi đối tượng/đóng Editor. Không giảm độ phân giải/chất lượng ảnh chụp và xuất.
- Editor tối đa schema22 cho semantic role mới. Lần lưu nâng cấp tạo .pre-v1.44.bak; không cần SQL migration, đổi KEY hoặc deploy backend.
- Giữ nền sửa startup/popup/menu chuột phải V1.43.0. Cross-build và bộ kiểm logic không thay thế kiểm cài/mở/chụp trên Windows thật. AI chưa có model được nghiệm thu chất lượng trên bộ form thực tế.
