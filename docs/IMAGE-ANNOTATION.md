# Captovix V1.1 — Chỉnh sửa ảnh

## Làm mờ, Pixel hóa và Che kín

1. Mở/chụp một ảnh.
2. Chọn Làm mờ (B), Pixel (P) hoặc Che kín (K), kéo vùng cần xử lý rồi thả.
3. Với làm mờ/pixel, đổi mức xử lý 4–64 px ở bảng phải; giá trị lớn cho hiệu ứng mạnh hơn. Mức là bán kính blur hoặc cạnh ô pixel trong ảnh gốc, độc lập mức zoom.
4. Che kín dùng màu đặc từ bảng màu/preset; hiệu ứng này luôn ở trên các vùng xử lý/chú thích khác. Làm mờ và pixel hóa vẫn có thể để lộ hình dạng/nội dung, dùng Che kín khi cần ẩn hoàn toàn.
5. Chọn (V), click vùng xử lý để di chuyển/đổi kích thước. Trong lúc kéo, vùng tạm phủ màu kín; thả chuột mới tính hiệu ứng, không làm việc nặng theo từng lần chuột di chuyển.
6. Copy ảnh hoặc Xuất file để chia sẻ PNG/JPEG đã xử lý. Không chia sẻ ảnh gốc, file sidecar Edits hoặc toàn thư viện nếu mục đích là giấu nội dung gốc.

Các hiệu ứng là đối tượng có thể sửa lại; không xóa dữ liệu trong ảnh gốc. Ảnh xuất chỉ có pixel đã tổng hợp, không kèm lớp đối tượng. Không phải công cụ che nội dung trong video.

## Chọn, di chuyển và đổi kích thước

- Click bằng công cụ Chọn; Shift/Ctrl+click để thêm hoặc bỏ một đối tượng.
- Kéo vùng trống để chọn các đối tượng giao vùng đó; Ctrl+A chọn tất cả (khi không đang nhập chữ).
- Kéo đối tượng đang chọn để di chuyển cả nhóm. Phím mũi tên đi 1 px; Shift+mũi tên đi 10 px.
- Khung/ellipse chọn theo nét, nên phần trống ở giữa không cản chọn đối tượng bên dưới. Vùng che luôn được ưu tiên hit-test theo thứ tự hiển thị.
- Tay nắm ở 8 hướng dùng cho hình/vùng và nhóm; chữ/số dùng các góc để đổi cỡ theo tỷ lệ; mũi tên có 2 đầu.
- Shift khi kéo để khóa trục di chuyển, giữ tỷ lệ khung hoặc hướng mũi tên theo bước 45 độ. Khi kéo chữ/số riêng, tỷ lệ được giữ sẵn.
- Bắt dính mép/tâm ảnh hoặc crop trong phạm vi 6 px màn hình; giữ Ctrl trong lúc kéo để bỏ bắt dính.
- Click chọn không tự di chuyển đối tượng. Escape hoặc mất capture chuột hủy thao tác đang kéo; Ctrl+Z/Ctrl+Y hoàn tác/làm lại.
- Một lần kéo/căn chỉnh/đổi preset cho nhóm là một bước undo.

## Căn chỉnh

Chọn đối tượng, mở Căn chỉnh đối tượng bên phải rồi chọn Trái/Giữa ngang/Phải/Trên/Giữa dọc/Dưới. Một đối tượng căn theo viewport ảnh/crop hiện tại; nhiều đối tượng căn theo hình chữ nhật bao cả nhóm. Không có chia đều khoảng cách trong bản này.

## Preset

Chọn mẫu → Áp dụng. Có 6 mẫu: Hướng dẫn, Cảnh báo, Ghi chú, Nổi bật, Tối giản, Chữ lớn. Preset áp dụng màu, độ dày, cỡ chữ và mức xử lý cho các đối tượng đã chọn; nếu chưa chọn thì làm kiểu mặc định cho đối tượng tiếp theo.

Tùy chỉnh kiểu → Lưu thành preset riêng → nhập tên → Lưu mẫu hiện tại. Tối đa 20 mẫu, tên không trùng mẫu có sẵn hoặc mẫu riêng. Muốn biến thể, dùng tên mới. Chọn mẫu riêng → Xóa mẫu để bỏ preset, không xóa các chú thích đã dùng mẫu đó. Preset lưu cục bộ và không cần tài khoản.

## Lưu và tương thích

Chú thích/crop/schema 1 từ V1.0 được đọc nguyên vẹn, nâng sang schema 2 khi lưu. Schema 2 chứa loại hiệu ứng và mức xử lý mới; V1.0 không hỗ trợ schema này. Backup trước nâng cấp giữ chú thích cũ, không phải chức năng rollback tự động. Media gốc giữ nguyên. Các preset riêng được đưa vào backup của những lần cập nhật từ V1.1 trở đi.

Mỗi ảnh vẫn có undo/redo tối đa 100 bước trong phiên đang mở; lịch sử undo không lưu qua lần đóng app. Bản chú thích cuối cùng tự lưu; Ctrl+S lưu ngay. Các file Edits/annotation-presets.json nằm ở %LOCALAPPDATA%\HuyVo\Captovix.
