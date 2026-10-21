# Captovix V1.4 — Image Transform & Styling

## Bắt đầu

Mở ảnh trong Editor, mở mục **Ảnh & khung · V1.4** ở đầu bảng thuộc tính bên phải. Không cần mở cửa sổ cấu hình riêng. Kích thước đầu ra hiển thị ở đây và dưới ảnh; metadata bên phải vẫn mô tả file gốc.

## Xoay và lật

Xoay trái/phải mỗi lần 90°. Lật ngang/dọc theo hướng ảnh đang hiển thị. Lệnh xoay sau khi lật vẫn xoay kết quả hiện tại, không đổi ngược thứ tự thao tác. Chữ, callout, đuôi, nét vẽ và vùng che đi theo ảnh. Nếu đã nhập cỡ ảnh, xoay 90° đổi chỗ rộng/cao. Canvas riêng giữ kích thước hiện có.

Các lệnh này không ghi lại file ảnh gốc. Có thể tiếp tục vẽ, chọn/kéo/resize, sửa chữ trực tiếp, undo/redo. Ô nhập chữ và khung chọn đi theo hướng xoay/lật hiện tại. Con lăn zoom/Space pan làm việc trên toàn đầu ra.

## Kích thước ảnh

Nhập rộng/cao bằng pixel rồi bấm Áp dụng cỡ. Giữ tỷ lệ mặc định bật, thay một chiều tự tính chiều kia theo tỷ lệ hiện có; bỏ chọn để kéo giãn riêng hai chiều. Phóng lớn không tạo thêm chi tiết ảnh. Chú thích được render theo tỷ lệ của ảnh, không đổi tọa độ lưu gốc.

Cỡ gốc xóa resize, lấy kích thước crop hiện tại sau xoay. Các ô này là kích thước riêng phần ảnh, chưa gồm khung. Khi resize sau crop, vùng crop được kéo về cỡ đã nhập. Crop số và thông tin vị trí chú thích vẫn tính bằng pixel ảnh gốc.

## Canvas và vị trí

Bật Dùng canvas riêng, nhập rộng/cao và chọn một trong 9 vị trí neo, bấm Áp dụng canvas. Canvas lớn hơn ảnh tạo khoảng trống; canvas nhỏ hơn ảnh cắt phần tràn, không xóa ảnh/chú thích ở phần bị ẩn. Tắt canvas riêng rồi Áp dụng để tự vừa phần ảnh.

Không tạo chú thích mới trên viền/đệm hoặc phần canvas không chứa ảnh. Chú thích ngoài vùng canvas vẫn được giữ trong tài liệu; khôi phục canvas để thấy lại. Crop tiếp tục làm việc trong tọa độ ảnh gốc.

## Viền, nền và bóng

- Khoảng đệm 0–512 px ở mỗi bên, viền 0–128 px; nhập màu #RRGGBB.
- Nền trong suốt hoặc nền màu; ô caro trong Editor chỉ là nền xem trước, không xuất vào ảnh.
- Bóng đổ khung: độ mềm 0–80 px, lệch X/Y -80…80 px, độ đậm 0–100%. Bóng là bóng chữ nhật mềm của khung, không tách theo đường alpha của từng vật thể trong ảnh gốc.
- Mẫu Sáng/Tối thêm đệm, viền và bóng. Bỏ kiểu chỉ bỏ các hiệu ứng khung, giữ xoay/resize/canvas.

Bấm Áp dụng kiểu khung để ghi các thông số. Xoay/lật và mẫu áp dụng ngay. Nếu màu/số/kích thước không hợp lệ, app báo và không đổi cấu hình đã lưu. Kiểu đang gõ trong các ô chưa Áp dụng là bản nháp của bảng điều khiển.

Đầu ra = canvas + đệm/viền hai bên + phần không gian cần cho bóng. Các thay đổi có thể làm đầu ra lớn hơn kích thước ảnh đã nhập. Giới hạn 32.768 px mỗi chiều và 100 triệu pixel, tính cả trang trí. Đặt lại trình bày ảnh xóa xoay/lật/resize/canvas/kiểu, giữ crop và toàn bộ chú thích.

## Lưu và xuất

Mỗi lần Áp dụng/xoay/lật/mẫu là một giao dịch undo. Cấu hình nằm trong sidecar schema 4; ảnh gốc giữ nguyên. Mở lại sẽ dựng cùng phép biến đổi. Thumbnail, clipboard và xuất dùng cùng pipeline hiển thị.

PNG và clipboard giữ alpha; JPEG không có alpha, phần trong suốt được ghép trắng. Ảnh xuất là ảnh đã ghép; file JSON lưu riêng để sửa lại. V1.4 đọc schema 1/2/3, chỉ nâng file khi có thay đổi và lưu; app cũ không đọc schema 4. In-app updater backup Edits/metadata/preset. Khi cài thủ công, sao lưu Edits nếu cần quay lại.

Chưa chạy thao tác chuột/IME/render Windows trong môi trường đóng gói; checklist Windows có các ca xoay + crop + inline text + canvas.
