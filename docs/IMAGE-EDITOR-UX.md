# Captovix V1.2 — Image Editor UX

## Zoom và di chuyển khung nhìn

Di chuột vào vùng ảnh rồi cuộn để zoom quanh con trỏ. Với nội dung nhỏ hơn khung nhìn, ảnh vẫn được căn giữa và không thể cuộn ra ngoài giới hạn. Các nút +/- zoom quanh tâm; 100% tương ứng một pixel ảnh cho một pixel vật lý trên màn hình; Vừa khung tính lại tỷ lệ khi đổi kích thước cửa sổ. Không đổi zoom trong lúc đang kéo một đối tượng/tay nắm.

Giữ Space khi con trỏ ở vùng ảnh hoặc canvas có focus, giữ chuột trái rồi kéo. Khung nhìn di chuyển, đối tượng và crop không thay đổi tọa độ. Nhả chuột hoặc Space để kết thúc. Escape trong lúc pan trả về vị trí trước khi kéo; mất focus cũng nhả chế độ pan. Space trong ô nhập chữ vẫn là dấu cách. Con lăn ở bảng thuộc tính vẫn cuộn bảng như bình thường.

## Crop có thể tinh chỉnh

1. Chọn Crop (C), kéo vùng ảnh cần giữ, thả chuột để xem trước. Ảnh chưa bị crop ở bước này.
2. Kéo bên trong vùng để di chuyển; kéo 8 tay nắm ở cạnh/góc để sửa. Shift vẽ hình vuông; Shift kéo góc giữ tỷ lệ với sai số làm tròn tối đa khoảng một pixel.
3. Hoặc nhập X/Y/rộng/cao ở bảng phải rồi chọn Xem vùng. X/Y là tọa độ trong ảnh gốc, kể cả khi ảnh đã crop. Vùng mới phải nằm trong khung ảnh/crop hiện tại và ít nhất 2 × 2 px. Giá trị sai bị từ chối, vùng trước được giữ.
4. Phím mũi tên di chuyển vùng chờ 1 px; Shift+mũi tên 10 px. Ctrl+A chọn toàn khung hiện tại. Chọn lại để vẽ một vùng khác.
5. Bấm Áp dụng crop hoặc Enter khi focus canvas. Khi nhập số: Enter xem vùng, Ctrl+Enter áp dụng. Crop xong hiển thị vừa khung.
6. Escape hoặc Hủy bỏ vùng đang chờ. Đổi công cụ/đổi ảnh cũng bỏ phần crop chưa xác nhận. Undo/Redo trong lúc crop trước hết hủy chế độ crop; crop đã áp dụng là một bước undo bình thường.

Copy ảnh áp dụng crop đang chờ rồi copy ảnh đã tổng hợp. Xuất file chỉ áp dụng crop sau khi xác nhận hộp thoại lưu; nếu hủy hộp thoại thì vùng chờ vẫn giữ. Ctrl+S trong chế độ crop áp dụng vùng nhập rồi lưu; lưu tự động không tự xác nhận vùng chờ. Đóng/chụp nội dung khác chỉ giữ tài liệu đã áp dụng, không lưu vùng crop chờ qua lần mở app.

Khôi phục khung ảnh gốc bỏ crop đã áp dụng; các đối tượng ngoài crop vẫn giữ trong tài liệu. Pixel ảnh gốc không bị viết lại. Schema chỉnh sửa vẫn là 2, tương thích V1.1.

## Chọn, kéo và đổi kích thước

Giữ công cụ Chọn V1.1: click/Shift/Ctrl+click, kéo vùng trống chọn nhiều, Ctrl+A chọn tất cả. Alt+click lần lượt đi qua các đối tượng dưới con trỏ để chọn đối tượng bị che. Kéo tay nắm đổi kích thước, Shift khóa hướng/tỷ lệ; Ctrl trong lúc kéo bỏ bắt dính. Một click chọn không tạo dịch chuyển hay undo. Escape/mất capture hủy giao dịch đang kéo. Zoom/pan chỉ đổi khung nhìn, không vào lịch sử undo.

Các tính năng làm mờ, pixel, che kín và preset vẫn dùng theo IMAGE-ANNOTATION.md. Windows desktop QA cho zoom/pan/crop và DPI nằm trong WINDOWS-TEST-CHECKLIST.md.
