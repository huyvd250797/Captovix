# Captovix V1.33.0 — Scrolling Capture Quality

- Chụp cuộn: đối chiếu dày theo các hàng của vùng overlap và nhiều dải cột; khung lệch ngang hoặc mối nối không chắc chắn cần kiểm tra/xác nhận. Không kéo giãn, nội suy hay tự làm mờ pixel để che mối nối.
- Lấy khung thủ công và tự động đều đợi ổn định, kiểm tra từng vùng nhỏ để tránh bỏ qua loading/animation; không thêm khung chưa ổn định sau thời gian chờ. Giới hạn kiểm tra ổn định không đảm bảo nhận ra trang vẫn đang chờ dữ liệu mạng khi hình đang đứng yên; có thể lấy lại khung.
- Tự phát hiện header/footer cố định khi hai khung đủ chi tiết và ghép phần thân chắc chắn; giữ vùng đầu/cuối một lần. Có tùy chọn tắt và nhập số pixel thủ công, hỗ trợ cả cuộn dọc/ngang.
- Xem trước mối nối, bật dấu chỉ dẫn, sửa overlap và xác nhận các điểm chưa chắc; bỏ khung bất kỳ trong phiên. Nguồn gốc và phiên trước được giữ khi tạo revision mới để khôi phục.
- Chụp ảnh được chính Captovix: giữ giao diện app/popup hiện có trong nền đã đóng băng. Chọn nguồn cửa sổ có thể bao gồm Captovix; selector/kính lúp/thanh điều khiển vẫn không lọt vào file chụp.
- Công cụ đưa lên hàng Chụp/Quay theo thứ tự: Khung, Callout, Mũi tên, Chữ, Crop, Làm mờ, Chọn vùng, Bước, Nhận chữ, ô công cụ động, Thêm. Chọn vùng là vùng pixel trên ảnh; Chọn đối tượng có trong Thêm và phím V.
- Thêm chứa các công cụ phụ (Chọn đối tượng, Phóng đại, Spotlight, Highlight, Vẽ tự do, Canvas…) và các Studio/Stamp hiện có; dùng lại bằng ô công cụ động cạnh Thêm.
- Bỏ nút Mở file/Copy ảnh/Xuất file khỏi hàng công cụ. Các lệnh vẫn có trong menu Tệp và phím tắt tương ứng. Thuộc tính bên phải kéo lên ngang hàng công cụ; kích thước/zoom thu gọn một dòng, tăng chiều cao canvas.
- Kéo công cụ A lên B để xem trước hoán đổi; thả lưu vị trí, Esc hủy về bố cục trước. Thêm và ô công cụ động giữ ở cuối. Bố cục V1.33 dùng file riêng để lần đầu hiện đúng thứ tự mới; giữ file bố cục cũ.
- Giữ ảnh thư viện khoảng 160ms rồi kéo: cung cấp PNG đã có chú thích qua FileDrop/Bitmap cho Word, Explorer, trình duyệt upload Drive và ứng dụng hỗ trợ kéo file. Không tự tải lên Drive. Kéo nội bộ vào canvas vẫn giữ cơ chế ghép ảnh.
- Popup bản quyền có logo/tiêu đề/mô tả ngắn, ô KEY và nút Kích hoạt; chức năng nâng cao ở Xem thêm. Link không có KEY hiển thị lời mời hoặc lịch sử dùng thử.
- Máy đã trial không được kích hoạt lại 30 ngày: chặn trong UI và Request dưới khóa đồng thời, lưu chứng nhận trial đã ký ở registry DPAPI; hiển thị ngày bắt đầu (hạn gốc trừ 30 ngày) và ngày hết hạn. Backend hiện có giữ nguyên hạn gốc khi nhận lại trial, không reset 30 ngày. Không thay cặp khóa hoặc dữ liệu server.
- Trạng thái vĩnh viễn còn quyền: đèn xanh lá có quầng sáng + Đã kích hoạt. Trial còn quyền: đèn vàng; trạng thái hết hạn/chờ xác minh hiển thị riêng.

Không cần chạy migration mới. Không cần deploy lại backend/admin bản quyền. Không thay mô hình đóng gói/dung lượng đã tạm giữ.
