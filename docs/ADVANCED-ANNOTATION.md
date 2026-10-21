# Captovix V1.3 — Advanced Annotation

## Sửa chữ tại ảnh

Chọn T rồi click ảnh (click chữ có sẵn để sửa); double-click chữ/callout có sẵn hoặc chọn và nhấn F2. Khung nhập xuất hiện ngay trên ảnh; với zoom nhỏ, app tăng lên ít nhất 50% để nhập dễ hơn. Enter thêm dòng; Ctrl+Enter hoàn tất, Escape bỏ phần nhập đang chờ. Click ra ngoài, đổi công cụ, Ctrl+S, xuất/copy hoặc đóng app sẽ hoàn tất nội dung đang nhập. Nội dung chưa hoàn tất là bản nháp; không tự lưu từng ký tự. Một lần hoàn tất là một bước undo; sau khi kết thúc nhập, công cụ trở về Chọn để di chuyển/đổi kích thước.

Bảng thuộc tính vẫn cho sửa chữ. Khi chọn cả nhóm, double-click/F2 sửa thành viên đang chọn chính mà không tách nhóm. Ctrl+Z khi đang gõ dùng undo của ô nhập; hoàn tất rồi Ctrl+Z sẽ undo trong tài liệu.

## Callout — Q

Kéo một hộp ít nhất 36 × 36 px. Thả chuột để nhập chữ; hộp có nền trắng, viền/màu chữ theo màu chọn. Double-click sửa nội dung. Chọn callout, kéo tay nắm ở đầu đuôi để đổi điểm chỉ; kéo các tay nắm khung để thay kích thước cả hình và đuôi. Nếu đuôi nằm trong hộp thì không vẽ phần nhô ra. Khi di chuyển/nhân bản/resize nhóm, đuôi đi theo hình.

Chữ tự xuống dòng theo chiều rộng hộp. Phần vượt chiều cao được cắt ở mép trong để không tràn lên hình khác; tăng chiều cao hoặc giảm cỡ chữ để hiện đủ. Đuôi được cắt tại vùng ảnh/crop như các chú thích khác.

## Vẽ tự do — F

Giữ chuột trái, vẽ, thả chuột để kết thúc nét; Escape khi vẽ hủy toàn nét. Chọn công cụ V để chọn theo đường nét, di chuyển, thay kích thước; thay màu/độ dày tại thuộc tính. Giữ nguyên ảnh gốc.

Tối đa 4.096 điểm mỗi nét và 100.000 điểm mỗi tài liệu để giới hạn dữ liệu/bộ nhớ. Các điểm gần nhau được lấy mẫu theo zoom. Với nét quá dài, thả chuột rồi vẽ tiếp một nét mới. Ảnh xuất render ở kích thước nguồn, không phụ thuộc zoom.

## Nhân bản và nhóm

- Shift/Ctrl+click hoặc kéo vùng chọn nhiều; click thành viên nhóm sẽ chọn cả nhóm.
- Ctrl+D hoặc Nhân bản tạo đối tượng mới, lệch 16 px trong giới hạn ảnh/crop; có thể trùng vị trí nếu không còn chỗ. Đối tượng và nhóm bản sao có ID mới.
- Ctrl+G nhóm ít nhất 2 đối tượng/nhóm; nhóm phẳng, không có nhóm lồng. Các nhóm đang chọn được gộp.
- Ctrl+Shift+G bỏ nhóm đang chọn; giữ các đối tượng và vị trí.
- Di chuyển/resize/xóa/đổi kiểu áp dụng toàn lựa chọn. Căn chỉnh coi mỗi nhóm là một phần tử; không làm các thành viên chồng vào nhau.

## Thứ tự và chia đều

Trong bảng Nhóm & thứ tự: Lên/Xuống một lớp, Lên trước/Xuống sau cùng. Phím tắt Ctrl+] / Ctrl+[; thêm Shift để tới trước/sau cùng. Các đối tượng trong lựa chọn giữ thứ tự tương đối. Chú thích thường, filter và che kín có lớp bảo vệ riêng; lệnh không đưa chữ hoặc nét vẽ đè lên nội dung cần che. Che kín luôn phủ trên filter.

Chọn ít nhất 3 phần tử độc lập (mỗi nhóm tính là 1), bấm Chia đều ngang/dọc. Khoảng cách tính giữa các mép, có xét kích thước từng phần tử; hai phần tử ở đầu/cuối giữ nguyên, nội bộ nhóm giữ nguyên. Nếu không đủ khoảng trống, app báo và không thay đổi vị trí.

## Lưu và tương thích

Mọi đối tượng, điểm, đuôi callout, GroupId và thứ tự nằm trong JSON sidecar; ảnh gốc không đổi. Schema 3 nhận schema 1/2; chỉ nâng file khi có thay đổi và lưu. Không mở ngược file schema 3 bằng app V1.2 trở xuống. In-app update giữ bước backup metadata/Edits; khi cài thủ công, sao lưu Edits nếu cần quay lại.

Undo/redo tối đa 100 bước, loại bớt bước cũ khi đạt giới hạn bộ nhớ lịch sử. PNG/JPEG/clipboard là ảnh đã ghép, không chứa metadata chỉnh sửa. Video xử lý sau.
