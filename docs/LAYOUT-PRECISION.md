# V1.21.0 — Layout Precision

## Thao tác

- Mở ảnh, chọn Khung/Callout/Chữ/Mũi tên/Vẽ tự do. Vùng trống 96 DIP quanh đầu ra chỉ là không gian thao tác, không tự thêm vào file xuất. Bắt đầu vẽ ở đây để mở rộng canvas ngay; không cần vào thư viện hay bật một chế độ riêng. Có thể tiếp tục kéo ra ngoài ảnh. Nền mặc định trong suốt.
- Nút **Canvas…** nằm cùng các công cụ vẽ. Menu **Bố cục → Canvas · lề và nền…** hoặc **Ctrl+Alt+E** mở cùng hộp thoại. Nhập phần cần thêm ở Trái/Trên/Phải/Dưới (pixel đầu ra), bấm **Thêm phần mở rộng**. Đây là phép cộng vào khung hiện tại; số nhập trở về 0 sau khi thực hiện. Không resize bitmap gốc. Mỗi lần thêm có Undo.
- Trong hộp thoại, chọn màu nền bằng bảng màu; chọn Trong suốt để bỏ màu nền. Nền và checkbox **Tự mở rộng khi vẽ sát mép** áp dụng ngay. Khi bật, kéo tạo đối tượng trong vòng 24 pixel đầu ra sát cạnh cũng thêm chỗ trống. Khi tắt, vẽ ra ngoài đầu ra vẫn thêm vùng cần dùng; không tự thêm chỉ vì tiến gần mép. Tùy chọn được lưu theo ảnh/project.
- **Crop**: khung ban đầu bao toàn bộ đầu ra, gồm phần mở rộng và chú thích. Kéo các tay nắm, nhập X/Y/Rộng/Cao bằng pixel đầu ra, Enter hoặc Áp dụng; Esc hủy. X/Y bắt đầu tại góc trên trái canvas đang thấy. Crop không xóa đối tượng hoặc raster gốc. Mở rộng lại sau crop có thể làm lộ phần nội dung trước đó bị cắt; đây là crop không phá hủy dữ liệu. Phần bị cắt được ẩn, Ctrl+Z khôi phục cả hình học đầu ra; tiếp tục chỉnh đối tượng còn thấy bình thường. Copy ảnh đã chỉnh, PNG/JPEG/PDF và project dùng cùng hình học.
- **Chữ**: click để tạo, nhập chữ. Click ra ngoài hoặc Ctrl+Enter lưu và chuyển về Chọn; không sinh thêm chữ trên click kết thúc. Muốn tạo tiếp phải chọn lại công cụ Chữ. Double-click chữ/callout để sửa. Esc hủy phần chữ đang nhập.
- **Callout**: một click tạo hộp mặc định 220×120 đơn vị nguồn; kéo để tự đặt kích thước. Đuôi bắt đầu tại vị trí click, sau khi hoàn tất có đuôi ngắn cạnh hộp; không nối đến tọa độ (0,0) khi vừa nhấn chuột. Vẫn kéo tay nắm đuôi để chỉ vào ảnh.
- **Thư viện tổng**: cây Năm → 12 tháng → ngày → ảnh/video. Checkbox **Ẩn ngày không có hình** mặc định bật, ghi nhớ trên máy. Bỏ chọn để xem tất cả ngày theo đúng lịch/tháng/năm nhuận. Năm và tháng vẫn được giữ để điều hướng. Nhánh có icon, số lượng nhỏ và thụt cấp; Ctrl/Shift chọn nhiều như trước.
- Menu Chỉnh sửa dư thừa được thay bằng menu Vùng pixel. Undo/Redo vẫn có Ctrl+Z/Ctrl+Y và nút nhanh. Nút canvas đã chuyển khỏi danh sách thao tác media.

## Dữ liệu và giới hạn

- Các cạnh mở rộng là pixel đầu ra, có thể lệch nhau; tọa độ annotation vẫn nằm trong hệ nguồn, kể cả số âm. Crop đầu ra được lưu bằng hình học và không raster hóa chú thích. Schema mới 14; dữ liệu cũ không dùng tính năng mới giữ schema12/13. App cũ được chặn ghi đè schema14. Giữ source mới nhất để phát triển tiếp; không hạ phiên bản trên project mới rồi lưu đè.
- Giới hạn 32.768 pixel mỗi chiều và 100 triệu pixel vẫn áp dụng, bao gồm khung trước crop. Không có canvas vô hạn. Nếu vượt giới hạn, hiển thị thông báo; điểm bắt đầu ngoài khung không tạo đối tượng vô hình khi không thể mở rộng.
- Tự mở rộng chỉ trong thao tác tạo chú thích; không tự mở rộng khi kéo/resize đối tượng cũ hay sửa chữ. Blur, spotlight, magnifier và chỉnh vùng pixel vẫn có miền nguồn riêng. Không thay engine capture/OCR/quay.
- Xoay ảnh bằng lệnh Rotate xóa crop đầu ra để tránh giữ hình chữ nhật ở hướng cũ; Undo khôi phục được. Đổi kích thước/kiểu canvas có thể cần hủy/đặt lại crop nếu hình học không còn hợp lệ; đầu vào lỗi không thay tài liệu.
- Bộ nhớ/cache, thumbnail nền, last-good và nguồn update ổn định của1.20 được giữ. Crop không làm ảnh chưa chỉnh tự mất chi tiết; phóng to ảnh chụp không tạo thêm chi tiết vốn không có trong nguồn.
- Chưa tự publish. Giải nén Update-Packages để lấy Setup, hoặc đăng riêng5asset lên stable GitHub Release v1.21.0 (Latest) để app nhận update.

## Kiểm chứng

Logic thuần kiểm: bốn cạnh độc lập; crop liên tiếp; mở rộng sau crop; xoay/lật; JSON/schema; Undo/Redo; byte pixel không chỉnh; từ chối kích thước/vùng sai. Các test WPF được bổ sung render ngoài ảnh/crop canvas/callout một click và compiled trên Linux. **Chưa chạy nghiệm thu Windows desktop.**

Checklist Windows bắt buộc trước triển khai rộng:

1. Cài mới ở D:\Captovix\Captovix và cập nhật từ1.20; kiểm tra logo, version, mở app.
2. Mở ảnh nhỏ/lớn; vẽ từng công cụ ở bốn khoảng trống; bật/tắt tự mở rộng; kéo chậm/nhanh; hủy Esc; Undo/Redo; đạt giới hạn an toàn.
3. Chữ: nhập tiếng Việt/IME; click ngoài; đổi công cụ; sửa lại; không tạo chữ thừa; Ctrl+Enter/Esc đúng.
4. Callout: click và kéo theo mọi hướng; không có đuôi dài lúc nhấn chuột; sửa chữ/di chuyển đuôi; nền trong suốt.
5. Crop sau xoay/lật/resize/mở rộng; xem trước, Enter/Esc, trường số; đối chiếu PNG và clipboard; save/reopen/project, Undo/Redo.
6. Thư viện: ngày trống checkbox; tháng28/29/30/31ngày; mở/đóng nhiều cấp; Ctrl/Shift chọn/xóa/khôi phục; light/dark/system.
7. Scale100/125/150/175/200%, màn hình phụ tọa độ âm; move app giữa màn hình; zoom Ctrl+wheel, Space pan, con trỏ/tay nắm/text overlay phải trùng nhau.

Phiên bản tiếp theo đề xuất: **V1.22 — Object Editing Precision**, căn theo ảnh/đối tượng, đường gióng, nhập tọa độ/kích thước, chọn lớp chồng nhau và chỉnh nhóm. Ưu tiên sửa phản hồi Windows1.21 trước.
