# Captovix V1.10.0 — Text & Callout Studio

- Chọn đối tượng → Ctrl+C → Ctrl+V để dán một bản vẫn sửa được. Giữ nhóm, kiểu chữ và liên kết connector nội bộ; tạo ID mới, tháo liên kết ra ngoài lựa chọn. Dán sang ảnh khác giữ số pixel, không tự co giãn. Mỗi lần dán là một Undo. Ctrl+D tiếp tục nhân bản như cũ.
- Khi không chọn đối tượng, Ctrl+C vẫn copy ảnh đã chỉnh. Ctrl+Shift+C luôn copy ảnh đã chỉnh. Khi đang gõ trong ô chữ, Ctrl+C/V vẫn là copy/paste văn bản. Bản này chưa nhận bitmap ngoài Captovix thành image layer.
- Chọn shape đã tô → bỏ dấu Tô nền hoặc bấm Không tô nền: bỏ nền ngay, giữ màu để bật lại. Nền Text/Callout có checkbox riêng trong Text & Callout Studio. Ctrl+Z phục hồi được.
- Chọn/di chuyển/resize trực tiếp từ mọi công cụ vẽ (ngoại trừ Crop). Bấm đường viền khung trong suốt, hoặc phần nền khung đã tô, rồi kéo; inspector tự theo loại đối tượng được chọn. Kéo vùng trống để vẽ tiếp bằng công cụ đang bật; giữ Alt khi kéo để vẽ đối tượng mới chồng lên đối tượng cũ. Vẫn có Chọn (V), chọn nhiều, Layers và menu chuột phải.
- Text: font cài trong Windows, đậm/nghiêng/gạch chân, màu chữ riêng, căn trái/giữa/phải khi có chiều rộng, padding, wrap theo chiều rộng nhập và giãn dòng. Mỗi đối tượng dùng một kiểu chữ; chưa có rich text theo từng từ.
- Callout: chữ nhật có đuôi, bo góc có đuôi, ellipse có đuôi, chữ nhật không đuôi và bo góc không đuôi. Màu chữ/nền/viền độc lập; căn dọc trên/giữa/dưới, tự tăng chiều cao khi bật Giãn hộp theo chữ. Tay nắm đuôi kế thừa, kéo được khi callout đã chọn.
- Step: tròn, vuông bo góc, hình thoi; tự tăng số khi tạo; đánh lại số các bước được chọn, hoặc các bước hiện và không khóa nếu không chọn. Giới hạn 1–9999; giữ nhóm/khóa và Undo.
- Preset riêng lưu thêm kiểu chữ/callout/step; các preset cũ vẫn đọc được. Thiếu font trên máy khác thì xem/xuất dùng Segoe UI và hiện cảnh báo. Double-click chữ/callout hoặc F2 để sửa trên ảnh; Enter xuống dòng, Ctrl+Enter hoàn tất, Esc hủy. IME cần nghiệm thu trên Windows.

Giữ các bản vá khởi động, chuyển ảnh và capture pixel lẻ. Schema 8 đọc 1–7, backup sidecar .pre-v1.10.bak trước lần lưu đầu. Workflow giữ cơ chế .pre-v1.9.bak; không reset dữ liệu. Giữ URL cập nhật và root cài D:\Captovix\Captovix.

Đã chạy logic/static và cross-build WPF trên Linux; chưa thực thi Windows thật. IME, clipboard hệ điều hành, thao tác chuột, DPI/capture và installer/updater cần nghiệm thu. Chữ vượt vùng ảnh/crop có cảnh báo; không bảo đảm giữ nội dung nằm ngoài khung xuất.

V1.11 — Focus & Privacy dự kiến bổ sung kính phóng đại vùng ảnh, spotlight làm tối phần ngoài vùng chú ý, highlight theo vùng và hoàn thiện mask blur/pixel/che đặc. Các hiệu ứng cần giữ đúng vị trí khi crop/xoay/đổi kích thước, sửa lại được và có preset. Kiểm tra bản xuất che đặc trước chia sẻ; blur/pixel không thay thế che đặc khi cần giấu thông tin. Chưa phát triển Video Editor.
