# V1.14 — Scrolling Capture Foundation & Personal Appearance

## Chụp cuộn dọc

1. Mở **Chụp & quay → Chụp cuộn dọc…**. Editor vẫn là cửa sổ làm việc chính; thanh điều khiển và hộp xem trước chỉ là cửa sổ tạm của phiên capture.
2. Kéo chọn vùng nội dung muốn ghép. Tránh thanh tiêu đề/menu/header/footer cố định, scrollbar, video và nội dung đang chuyển động. Vùng phải rộng ≥8px, cao ≥32px, tối đa4MP. V1.14 hướng tới màn hình SDR8.
3. Lấy khung đầu bằng nút hoặc **Ctrl+Shift+F10**. Con trỏ không được ghi vào khung cuộn. Thanh điều khiển luôn ẩn350ms trước khi lấy khung; không chỉ dựa vào API exclude capture. App không tự click/cuộn nội dung.
4. Đưa chuột vào trang, cuộn **xuống** khoảng nửa chiều cao vùng. Giữ nguyên kích thước cửa sổ, vị trí, zoom trang và DPI. Chờ nội dung đứng yên, lấy khung tiếp. Nên giữ overlap30–60%; bộ tìm tự động yêu cầu ít nhất max(16px, chiều cao/6) phần chồng lấn. Overlap ít hơn có thể sửa bằng tay.
5. **Ctrl+Shift+F11** hoặc **Ghép & xem trước**. Khi hotkey đang bị app khác sử dụng, dùng các nút trên thanh. Captovix vẫn phải đang chạy.
6. Xem ảnh và các đường tím đánh dấu mối nối. Chuyển **100%** để kiểm tra chữ/hàng pixel. Chọn từng cặp khung, nhập **Overlap** = số hàng đầu của khung sau cần bỏ. Bấm **Cập nhật điểm nối & xem lại**.
7. Điểm không chắc chắn phải được người dùng sửa/kiểm tra, đánh dấu **Tôi đã kiểm tra điểm nối này**, rồi cập nhật. Chưa xác nhận thì nút đưa vào Editor bị khóa. Điểm tự khớp vẫn cần kiểm tra bằng mắt: kết quả thuật toán là gợi ý, không chứng nhận nội dung đầy đủ.
8. **Đưa ảnh ghép vào Editor** tạo PNG mới, giữ số pixel, sửa chú thích/xuất như ảnh thường. Đường tím không nằm trong PNG. Một khung duy nhất cũng có thể lưu. V1.14 luôn mở Editor sau phiên cuộn để kiểm tra, không tự copy clipboard hay tự thêm viền capture; có thể thêm viền ở Trình bày toàn ảnh sau đó.

## An toàn, khôi phục và giới hạn

- Tối đa24 khung,4MP/khung,64 triệu pixel nguồn/phiên,40MP đầu ra và cao32768px. Thường cần vài trăm MB RAM ở trường hợp lớn nhất; các khung nguồn lưu PNG trên ổ đĩa. Không giữ đồng thời tất cả ảnh giải mã. Không có cam kết máy yếu không giật.
- Esc/hủy dừng thu khung. Khi đang ghi, app chờ hoàn tất tác vụ ghi hiện tại rồi đóng. Xem trước có thể đóng để hủy ghép; khung nguồn vẫn giữ. Dữ liệu không tự xóa theo thời gian.
- Thư mục: `%LOCALAPPDATA%\HuyVo\Captovix\ScrollSessions\<phiên>`. Gồm `session.json`, `frame-000.png`… và SHA256. `candidate-*.png`/`uncommitted-*.png` có thể xuất hiện sau lỗi/gián đoạn trước khi commit: mở PNG trực tiếp qua Tệp → Mở ảnh; không tự chèn vào phiên chưa hoàn tất.
- **Chụp & quay → Khôi phục phiên chụp cuộn…** rồi chọn `session.json`. Khôi phục chỉ xem lại/ghép các khung đã commit, không tiếp tục capture vào cửa sổ cũ. File thiếu, kích thước khác hoặc hash đổi sẽ bị từ chối; nguồn không bị xóa. Khi không còn cần, mở thư mục nguồn và xóa thủ công phiên của chính mình.
- Khung trùng bị bỏ; điểm ít chi tiết/lặp/cuộn ngược/lệch ngang/nội dung động thường bị gắn chưa chắc. Thuật toán lấy mẫu, không hiểu DOM hay UI Automation của nội dung cuộn; không thể phát hiện mọi biến động nhỏ. Không âm thầm hứa hỗ trợ mọi website/app.
- Thay bố trí màn hình/DPI bị chặn trước khung tiếp; không tự phát hiện mọi thay đổi vị trí/zoom của cửa sổ mục tiêu. Cần người dùng giữ nguyên vùng. Khi thay đổi, kết thúc và chọn vùng mới.
- Giới hạn theo từng phiên, không phải quota tổng của tất cả phiên. Kiểm tra dung lượng ổ chứa LocalAppData định kỳ; đường dẫn cài app ởD không đồng nghĩa thư viện ảnh tự chuyển sangD.
- Chưa có auto-scroll, cuộn ngang, tự loại header/footer cố định, chỉnh phối cảnh, OCR hoặc HDR/wide gamut. Video giữ tính năng cũ, chưa mở rộng xử lý.

## Giao diện cá nhân

**Hiển thị → Giao diện · Sáng / Tối / Theo hệ thống…** hoặc cây Thiết lập → Giao diện.

- Chọn Light/Sáng, Dark/Tối hoặc System/Theo hệ thống. Mặc định System. System theo tùy chọn **màu ứng dụng Windows**, không lấy riêng màu thanh taskbar; theo dõi thay đổi khi app đang chạy. Light/Dark không bị đổi theo Windows.
- Màu nhấn mặc định tím `#7C3AED`; có6 màu nhanh và hộp chọn màu Windows. Lưu ngay, không có nút Áp dụng. Chữ trên nút tự chọn trắng/đen theo độ tương phản.
- Màu nhấn và chế độ lưu tại `%LOCALAPPDATA%\HuyVo\Captovix\appearance-settings.json` cho **tài khoản Windows trên máy này**. Không đồng bộ cloud, không nằm trong template/project, không gắn với thư mục cài. Máy/tài khoản khác cấu hình độc lập. Update không ghi đè file này; backup trước update có chứa file.
- Khôi phục mặc định trả về tím+System. File cấu hình lỗi không làm app dừng khởi động; chỉ khi người dùng lưu thiết lập mới, file cũ lỗi mới được sao lưu riêng rồi thay. Không thay đổi màu nền/viền/chữ của ảnh đang sửa hoặc file xuất.
- Theme áp dụng cho Editor, inspector, thiết lập và hộp chụp cuộn. Nền ô trong suốt/thumbnail/style preview phản ánh ảnh thực tế vẫn giữ màu của nội dung. Hộp chọn màu/file, MessageBox và installer là cửa sổ native, có thể theo cách hiển thị của Windows thay vì theme app.

## Logo đã chốt

Đưa biểu tượng hai góc đối diện có chấm tròn, bố cục gọn và tím Captovix vào header, fileexe/taskbar, tray và installer. Source gồm bản traceSVG, PNG thường/sáng và ICO16–256px. `scripts/Generate-Brand.py` tạo lại tài nguyên từ geometrySVG (Pillow+CairoSVG); build không cần cài các thư viện này vì đã có asset. Đây là triển khai vector của concept được Boss chọn, không phải chứng nhận đăng ký nhãn hiệu.

## Trạng thái xác minh

Kiểm tra logic và cross-build WPF được ghi ở `VALIDATION.md`. Chưa có phiên Windows thật trong môi trường bàn giao này; đọc `WINDOWS-TEST-V1.14.md` trước khi phát hành rộng. Các API tham khảo chính chủ:

- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.win32.systemevents.userpreferencechanged
