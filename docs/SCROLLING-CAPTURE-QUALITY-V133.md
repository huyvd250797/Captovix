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

## Cài đặt và dùng nhanh

1. Thoát Captovix cả khay hệ thống. Giải nén Update-Packages, chạy HuyVo.Captovix-win-Setup.exe và giữ đường dẫn đang cài. Không uninstall/xóa dữ liệu.
2. Chụp ảnh hoặc nhấn phím tắt khi Captovix đang hiện để chụp chính giao diện app; không cần ứng dụng thứ hai. Chế độ quay giữ quy trình hiện tại.
3. Chụp cuộn: menu Chụp & quay → Chụp cuộn dọc/ngang. Chọn vùng, bật tự nhận vùng cố định hoặc nhập header/footer, lấy khung đầu, cuộn theo hướng đã chọn. Phím Ctrl+Shift+F10 lấy khung, Ctrl+Shift+F11 kết thúc/ghép.
4. Xem trước: chọn điểm nối, sửa overlap theo hàng/cột, tick đã kiểm tra rồi Cập nhật. Nối không chắc chặn lưu đến khi xác nhận. Chọn khung nguồn để loại bỏ nếu lỗi; phiên/revision trước vẫn còn trong ScrollSessions.
5. Kéo ảnh ra ngoài: giữ và kéo chính ảnh ở danh sách, tránh nút sao/xóa hoặc scrollbar. PNG mang nội dung chỉnh sửa hiện tại. Nền tảng đích phải nhận kéo file; nếu không hỗ trợ, dùng menu Tệp → Xuất ảnh rồi chọn file ở nền tảng đó.

## Kiểm tra trên Windows thật — còn chờ nghiệm thu

- Cuộn dọc/ngang trên bảng, trang web có sticky header/footer, trang tải chậm; không sọc/mất/lặp dòng; các cột thẳng. Trang ít chi tiết hoặc lặp hàng phải cảnh báo, không tự tuyên bố chắc chắn.
- Kiểm tra bỏ khung đầu/giữa/cuối, sửa overlap, hủy giữa phiên, mở lại session.json. PNG đầu ra giữ pixel nguồn, không có dấu chỉ dẫn mối nối.
- Chrome/Edge, Win32/WPF: chụp chính Editor, Settings và popup bản quyền Captovix; selector/kính lúp không vào ảnh. Scale100/125/150/175/200%, màn hình phụ tọa độ âm.
- Toolbar ở Windows1440/1920 và cửa sổ hẹp: đúng thứ tự, tools phụ trong Thêm, ô gần nhất hoạt động cho từng công cụ. Thuộc tính lên sát hàng trên, không bị mất khi đổi thư viện dọc/ngang.
- Drag A→B hiện preview hoán đổi; thả giữ, Esc trả lại; mở lại app giữ vị trí. Thêm/dynamic luôn cuối hàng.
- Giữ/kéo PNG đã chú thích từ thư viện sang Word, Explorer và vùng upload Drive trên trình duyệt; không mất ảnh gốc, kéo trở lại canvas vẫn ghép ảnh được. Kiểm tra ứng dụng đích không nhận FileDrop có fallback xuất file qua menu.
- Popup KEY light/dark/system; trial chưa dùng có xác nhận30 ngày. Máy đang trial và đã hết trial đều không có nút bắt đầu mới, hiển thị hạn gốc; request lặp không đổi hạn. Kích hoạt key vĩnh viễn hiển thị đèn xanh có quầng; trial còn quyền là vàng.
- Cài đè và updater từ V1.32 giữ media, ghi chú, key và thời hạn trial. Backend/admin không cần triển khai lại.

Đã có các kiểm tra logic/biên dịch tự động, nhưng môi trường Linux không chạy native Windows/Word/Drive drag-drop/IME/GDI/UIA. Không coi cross-build là nghiệm thu máy thật.
