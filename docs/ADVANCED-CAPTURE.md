> V1.15.1 bổ sung độ ổn định, thời gian chờ/nấc cuộn và giao diện: xem CAPTURE-RELIABILITY.md. Cấu hình cũ 1,4 giây đã được thay bằng thời gian chờ lựa chọn.

# Captovix V1.15.0 — Advanced Capture & Stitching

## Chụp cuộn

Chụp & quay → Chụp cuộn → chọn vùng. Chọn dọc hoặc ngang trong hộp cấu hình trước phiên.
Dọc: nhập số pixel header/footer cố định; chọn giữ header đầu và footer cuối một lần, hoặc bỏ cả hai. Nội dung giữa được đối chiếu và ghép nguyên hàng pixel. Ngang: đặt header/footer0, chọn vùng chỉ chứa các hàng dữ liệu muốn nối.

Nút Tự cuộn bật/tắt tự động, mỗi nhịp1,4 giây. Đưa cửa sổ nội dung ra trước. Captovix gửi nhịp cuộn tới cửa sổ dưới tâm vùng chọn, không di chuyển chuột. Cửa sổ đích phải còn hiện, không bị che. Pause/Resume tạm ngưng/lấy tiếp. Ctrl+Shift+F10 lấy khung thủ công; Ctrl+Shift+F11 kết thúc để xem trước. Esc/hủy giữ các khung đã ghi.

Tự dừng khi có3 lần không xuất hiện nội dung mới; thông báo phân biệt đây có thể là cuối trang hoặc app không nhận cuộn. Nếu matching không chắc, dừng tự cuộn để kiểm tra. Nếu ứng dụng không nhận WM_MOUSEWHEEL, dùng cuộn thủ công. Không hỗ trợ thao tác với cửa sổ quyền cao hơn app hoặc mọi loại canvas/provider. Không tự nhận diện DOM, tốc độ cố định, chưa tự chờ tải mạng vô hạn.

Xem trước có ảnh hai khung của điểm nối đang chọn, đường tím đánh dấu nối và nhập overlap. Ngang: overlap là số cột; dọc: số hàng của phần nội dung sau khi bỏ header/footer. Loại khung sau của điểm nối đang chọn sẽ tạo thư mục revision mới, giữ nguyên phiên cũ; các điểm nối phải xác nhận lại vì bỏ khung có thể gây thiếu nội dung. Lấy lại phiên mới nếu không còn đủ overlap.

Nguồn PNG+SHA256 giữ tại LocalAppData/HuyVo/Captovix/ScrollSessions. Menu khôi phục mở session.json để xem/ghép lại, không tự tiếp tục điều khiển cửa sổ cũ. Schema phiên2 đọc cả phiên1; bản1.14 không đọc phiên2. Tài liệu Editor vẫn schema12. Header/footer giữ một lần không tự nhận diện: người dùng nhập đúng vùng cố định. Giới hạn24khung,4MP/khung,64MPnguồn,40MPảnh ghép,32768px mỗi chiều. Ghép dọc hoặc ngang riêng từng phiên, chưa có ghép lưới hai chiều.

## Chụp chính xác

- Vùng cố định / gần đây: nhập X/Y và rộng/cao theo pixel màn hình; tọa độ âm được hỗ trợ. Giữ8 vùng gần đây riêng máy trong recent-capture-regions.json. Vùng phải nằm hoàn toàn trong các màn hình đang kết nối.
- Nội dung cửa sổ: chọn cửa sổ; lấy client bounds qua Windows, chụp các pixel màn hình của vùng này. Cửa sổ phải đang hiển thị không bị che; không phải chụp nền của cửa sổ bị ẩn. App không cung cấp client rectangle sẽ báo lỗi.
- Menu có hẹn giờ: nhập vùng và0–15 giây, bấm Chụp, mở menu/dropdown trong thời gian chờ. Không tự nhận diện menu. Esc hủy trong thời gian chờ.

App mở một Editor maximized; thiết lập theme/logo và các công cụ ảnh trước đó được giữ. Bàn giao hai ZIP: Update-Packages có Setup và Source. Giải nén Update-Packages, đóng app/tray, chạy HuyVo.Captovix-win-Setup.exe; hoặc publisher đăng5asset lên stable latest Release để app cập nhật.

## Kiểm tra và nghiệm thu

Build và kiểm tra logic trên Linux; WPF chỉ cross-build. Chưa kiểm thử tự cuộn/client/menu, nhiều màn hình hoặc cài/update trên Windows thật. Chưa có danh sách ứng dụng đã nghiệm thu. Không công bố điều kiện không mất/lặp dòng/cột đã đạt chỉ từ kiểm tra tổng hợp.

Trên Windows: mở docs/scroll-capture-test.html, lấy6–10khung với header/footer0 rồi có header/footer; so hàng đánh số không thiếu/lặp. Thử bảng rộng có số cột liên tục để ghép ngang, so từng cột tại nối. Thử pause/resume, cuối trang, focus app khác, app không nhận cuộn, hủy lúc ghi, khôi phục, loại khung giữa và lưu revision. Thử fixed region tọa độ âm/scale100–200%, client không bị che, menu5giây, vùng ngoài màn hình và Esc. Ghi Windows/app/version/DPI cùng ảnh nguồn và ảnh ghép. Điều kiện hoàn thành sản phẩm phụ thuộc nghiệm thu này.

V1.15.1 dự kiến: tinh chỉnh theo kết quả Windows, cấu hình tốc độ/chờ ổn định, lựa chọn vùng cuộn lồng nhau và trực quan hóa điểm nối tốt hơn.
