# V1.41.0 — Groups & Layout Editing (M4)

Nâng từ source V1.40.0 Boss gửi; giữ chụp/quay, OCR, Mockup M1/M2/M3, runtime, bản quyền và updater.

## Nhóm và cây đối tượng

- Mở **Thêm → Nhóm & bố cục**. Công cụ vào ô động và có phím tắt tùy chỉnh như các công cụ khác. Cây cũng nằm trong bảng thuộc tính **Nhóm & đối tượng**.
- Ctrl/Shift chọn nhiều trên canvas rồi **Nhóm / Ctrl+G**. Nhóm thêm một nhóm đang có với panel sẽ tạo nhóm cha và giữ nhóm con.
- Click thường chọn cả nhóm ở cấp hiện tại. **Vào nhóm**, nhấp đúp vào nhóm hoặc chọn một dòng con trong cây để chỉnh bên trong. **Alt+click** chọn riêng một phần tử, đồng thời luân phiên các đối tượng chồng lấp.
- **Chọn cha** lần lượt chọn nhóm gần rồi nhóm ngoài; **Toàn cây** quay lại cấp ngoài. Ctrl+click các dòng ở cùng cấp để chọn nhiều; chọn nhiều bằng Shift trên canvas vẫn giữ cơ chế cũ.
- Kéo cả panel giữ khoảng cách tương đối của label/control. Chọn dòng label để kéo/sửa riêng mà không cần phá nhóm. Tên riêng, khóa/ẩn của từng phần tử vẫn dùng được trong cây; khóa/ẩn một con chặn thao tác cả nhóm ở cấp ngoài, nhưng không chặn chỉnh một con khác đã chọn riêng.
- **Bỏ nhóm / Ctrl+Shift+G** tại cấp nhóm đang chọn chỉ tháo cấp đó, giữ các nhóm con. Muốn tháo nhóm chứa một label đã chọn riêng, bấm Chọn cha trước.

## Bố cục và thuộc tính

- Nút Trái/Giữa/Trên/Đều ngang/Đều dọc ở cây; đầy đủ sáu cách căn, tham chiếu ảnh/canvas, chia khoảng và sắp lớp qua menu chuột phải.
- Nhóm được coi là một khối khi căn/chia khoảng; vào nhóm để thao tác các control riêng. Chia khoảng cần ít nhất ba đơn vị và đủ chỗ, không tự ép chồng lấp khi thiếu khoảng trống.
- **Thuộc tính chung…**: màu chữ, nền/tô và cỡ chữ. Để trống để giữ thuộc tính đó; chỉ áp dụng lên loại phần tử tương thích. Kiểm tra đầu vào trước khi đổi; một lần áp dụng là một bước Undo. Mặt nạ nền nguồn M3 giữ độc lập.
- Sắp lớp vẫn tuân theo các lớp bảo vệ: che/làm mờ nằm trên chú thích; không đưa dữ liệu cần che ra phía trước vùng bảo vệ.
- Khi đối tượng bị chồng, Alt+click hoặc menu **Chọn đối tượng tại đây** và cây để chọn đúng phần tử. Cây phản ánh quan hệ logic; thứ tự ảnh xuất theo lớp vẽ đã lưu.

## Popup và thư viện

- Các cửa sổ Captovix dùng chung tiêu đề/viền theo theme. Thông báo lỗi/xác nhận được chuyển sang form chung; Hủy/Đóng bên trái, xác nhận bên phải.
- Popup nổi và menu có viền, bóng đổ; màu nền/chữ theo sáng/tối/hệ thống. Các overlay chụp/quay không bị bọc viền dialog.
- Khi modal đang chặn cửa sổ cha, click ngoài làm viền/quầng sáng nháy nhẹ, không ẩn/hiện cửa sổ. Hook tháo khi đóng dialog. Cần kiểm native Windows để xác nhận mọi đường nhập và DPI.
- Menu **Thêm** dùng hai cột chia đều, không còn khoảng trống do tile có margin làm rớt cột; cuộn dọc khi danh sách dài.
- Chuột phải đúng ảnh/video trong thư viện trái hoặc dưới → **Mở vị trí tệp**: Explorer mở thư mục và chọn file gốc. Không tự xuất ảnh có chú thích hay đổi file gốc; nếu file bị mất có thông báo. Thao tác giữ/kéo ảnh ra ngoài vẫn giữ nguyên.
- Hộp chọn tệp/màu do Windows cung cấp vẫn theo giao diện hệ điều hành.

## Dữ liệu và cập nhật

- GroupId vẫn là nhóm trực tiếp; GroupPath lưu các tổ tiên, tối đa16cấp. Có kiểm tra mã nhóm/vòng lặp/nhóm nhiều cha; schema21 khi dùng nhóm lồng nhau. Nhóm phẳng và ảnh cũ vẫn đọc như trước.
- Snapshot, clipboard, nhân bản, template và portable project giữ quan hệ; clipboard tạo bộ mã nhóm độc lập, nhân bản một con trong nhóm giữ cha hiện có.
- Chọn cấp cha/con là trạng thái phiên, không ghi vào ảnh. Lưu/mở lại giữ bố cục/hệ nhóm; không cam kết lưu toàn bộ Undo stack qua đóng/chuyển ảnh.
- Lần đầu ghi schema21 tự tạo backup `.pre-v1.41.bak` của sidecar cũ. Sao lưu thư viện trước rollback; app1.40 không đọc nhóm schema21.
- Không cần chạy migration mới. Không deploy lại backend/Edge Function. Không đổi KEY/trial/API/protocol hoặc dependency.
- Full Setup giữ runtime.NET10.0.13 self-contained, Visual C++ prerequisite và OCR của gói1.40. Thoát app cả khay, cài đè đường dẫn hiện tại, không uninstall.
- Auto-update chỉ nhận bản mới khi publish đủ năm asset trong Update-Packages lên stable Release v1.41.0 và đặt Latest. Source ZIP/Update ZIP không thay thế năm asset. Lần bàn giao này chưa publish GitHub.

## Kiểm tra Windows còn chờ

Chạy `tests/GroupLayoutWindowsChecks` và `tests/MoveRecoveryWindowsChecks` trên Windows x64. Kiểm panel-nút-label, lồng2–3cấp, kéo/resize cả nhóm, sửa label, Ctrl+G/ungroup/copy/duplicate, crop/rotate, khóa/ẩn, chồng lấp và xuất PNG/save-reopen/project/Undo. Thử popup xác nhận/xóa/KEY/OCR/Mockup/nhóm ở sáng/tối, scale100/125/150/200%, nhiều màn hình; click cha/ngoài dialog xem viền nháy nhưng nội dung không nháy; More hai cột/scroll/Esc/click ngoài; chuột phải thư viện hai hướng; Explorer chọn đúng file. Cài đè/updater/restart cần nghiệm thu native riêng. Host Linux chỉ cross-build Windows, không chạy được WPF.
