# Nghiệm thu Windows — V1.59.0 Canvas & Layout

- Cài đè V1.58.0 trên Windows x64. Mở dự án V2 có chú thích cũ và thứ tự clip đã đổi. Xác nhận chú thích giữ khoảng hiển thị và thư viện, giấy phép, nguồn MP4 vẫn truy cập được.
- Split từ trái qua phải thành ba đoạn. Kéo Đoạn 2 xuống cuối, kiểm nhãn khi zoom và di chuột. Thử Ctrl+Z, Ctrl+Y, lưu, thoát và mở lại. Menu chuột phải có một mục Split và tách ở vạch phát.
- Dùng nguồn có lưới và mốc màu ở bốn góc. Xuất 1920×1080, 1080×1920 và 1080×1080 bằng FFmpeg có libx264. Kiểm pixel trung tâm, vùng crop, hướng xoay 90/180/270, màu nền và Fit/Fill. So từng bố cục với khung xem ở cùng mốc. Kiểm cả nguồn có thông tin xoay trong metadata.
- Vẽ Khung ở giây 1–3 và Chữ ở giây 2–5. Kiểm thời điểm 0.9, 1, 2, 3 và 5 giây ở preview và MP4. Kéo thân track và từng đầu, di chuyển/đổi kích thước trên canvas, sửa chữ, kiểm lớp ink nằm trên blur và xác nhận Undo/Redo. Kiểm chú thích xuất riêng khi mốc bắt đầu trước clip.
- Ghép nguồn VFR, nguồn ngang/dọc và một nguồn không có âm thanh. Kiểm thời lượng, tiếng trước/sau điểm nối, hủy xuất giữa chừng và bảo toàn tệp đích có sẵn. Kiểm F11 và Esc.
