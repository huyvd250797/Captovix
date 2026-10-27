# V1.23 — Text & Callout Refinement

## Luồng sử dụng

1. Chụp hoặc mở ảnh: Editor chọn Vừa khung; Ctrl+con lăn thay zoom, con lăn cuộn dọc, Shift+con lăn cuộn ngang. Chọn 100% để xem từng pixel. Zoom không resize file.
2. Chọn Chữ/Callout, tạo đối tượng rồi nhập ngay trên chữ. Ô nhập trong suốt, body/fill/viền giữ nguyên màu thật. Enter xuống dòng, Ctrl+Enter hoàn tất, Esc hủy phiên sửa. Bấm ngoài hoàn tất và trở về Chọn. Chuột đôi hoặc nút Sửa chữ trực tiếp mở lại.
3. Thanh phải: Màu & kích thước; Hình dạng & kiểu nét; Chữ & bố trí; Hiệu ứng. Các phần chỉ hiện cho công cụ tương ứng; tham số nâng cao nằm trong expander. Mẫu đã lưu lên đầu nhưng gấp mặc định.
4. Căn ngang/dọc, đệm mặc định30px, rộng chữ, giãn dòng và tự co khung dùng lại các thuộc tính editable. Ctrl+B/I/U tác động toàn đối tượng, không định dạng riêng một đoạn. Tắt tự co để giữ thân theo kích thước đã đặt.
5. Callout có đuôi: kéo tay nắm đuôi đến mục tiêu. Khi sửa chữ, tự co cố giữ đích đuôi theo tọa độ thế giới; nếu đuôi bên dưới thì thân nở hướng lên. Các mẫu không đuôi giữ neo thân.
6. Vẽ/kéo đối tượng vượt ảnh: chỉ cạnh có nét thật vượt ra ngoài được nới theo pixel làm tròn lên; bao gồm bóng và đuôi. Giao diện không vẽ ô caro ở gutter thao tác. Phần canvas thực có nền trong suốt mới hiển thị checkerboard; checkerboard không xuất ra file.
7. Tệp có nhóm Mở ảnh & dự án, Xuất & chia sẻ, Thư viện & sao lưu, Thông tin & tệp gốc. Chụp & quay chia chụp, cuộn/phục hồi, quay, thiết lập. Hiển thị chia khung nhìn, không gian, giao diện. Công cụ chia OCR/sửa chữ và thiết lập hệ thống.

## Biên canvas và tương thích

- Tự nới khi tạo/kéo đối tượng và khi hoàn tất sửa chữ/thuộc tính có thay biên. Không thêm buffer cố định. Ngưỡng32768px/cạnh,100MP tổng tiếp tục áp dụng; vượt ngưỡng báo lỗi và giữ đối tượng để chỉnh lại.
- Không tự cắt canvas khi di chuyển lại vào trong/xóa đối tượng; dùng Undo hoặc Crop để thu phần đã nới. Nới bốn cạnh thủ công và canvas của project cũ được giữ. Không thay kích thước bitmap nguồn, không nâng chất lượng chi tiết mà nguồn không có.
- Nhập X/Y/kích thước, nudge, căn/chia đều không tự nới trong bản này; dùng mở rộng thủ công hoặc kéo để nới. Không cho hiệu ứng pixel-source vẽ ra ngoài nguồn.
- Gutter tương tác48DIP chỉ dành Editor, không là phần xuất. Khi ở zoom cao, thu zoom/pan rồi kéo tiếp nếu cần đi xa. Sau commit, chế độ Fit tự fit cả canvas mới.
- Native TextBox dùng cho caret/selection/IME, nền và border trong suốt; glyph editor ẩn trong lúc nhập, exporter vẫn vẽ nội dung thật. Layout của font/IME có thể chênh nhỏ giữa caret và renderer, cần Windows acceptance. Không có rich text theo đoạn.
- Giữ .NET10.0.13/WPF/ScreenRecorderLib7.0.1/Velopack1.2.161/Tesseract5.2.0; schema12/13/14; approved logo, nguồn update gắn sẵn và installer hỗ trợ đường dẫn cũ. Không reset dữ liệu.

## Cửa sổ/taskbar

WindowPlacement đọc work area của monitor thực bằng GetMonitorInfo, xử lý WM_GETMINMAXINFO và cập nhật theo DPI/display/settings. Đặt vị trí/kích thước native theo work area, dùng cả tọa độ âm. Bỏ qua capture overlay trong suốt. Caption dùng theme hiện tại; hover hạ/phóng dùng nền Hover, đóng dùng đỏ. Chưa tuyên bố nghiệm thu trên mọi cách bố trí taskbar/auto-hide.

Tài liệu Microsoft: https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-getminmaxinfo và https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-monitorinfo .

## Nghiệm thu trên Windows thật

- Cài/update từ1.22, About hiển thị1.23, logo mới taskbar/shortcut/About; dữ liệu/màu/phím tắt còn nguyên.
- Taskbar dưới/trái/trên, auto-hide bật/tắt, monitor phụ tọa độ âm,125/150/200%DPI; footer, modal, caption hover/max/restore không bị che.
- Ảnh mới vừa khung; kiểm native pixel/PNG/clipboard không đổi do zoom. Không có checkerboard quanh capture khi chưa nới.
- Vẽ/kéo callout lên trên/trái/phải/dưới, có bóng/xoay; chỉ cạnh vượt tăng; Undo/Redo một bước. Crop toàn canvas và project mở lại khớp.
- Gõ Việt bằng Telex/VNI, IME có composition; Enter/Esc khi composition, click ngoài, đổi selection, xuất/chuyển ảnh khi đang gõ; không mất chữ/không hai lớp/không nền đen. Kiểm TextBox caret so glyph với font thực tế.
-10mẫu callout, chữ dài/ngắn, đệm0/30/100, căn ba ngang/ba dọc, auto-fit bật/tắt, đuôi tự do/đuôi xoay và thân nhỏ. Mẫu đã lưu không cắt glyph hoặc mất tail.
- Dark/light/system, sidebar hẹp/rộng, menu grouped, icon Shape/Folder, metadata compact và tooltip; không lỗi khởi tạo/reentrant SelectionChanged.

Linux chỉ kiểm logic/static/build. scripts/Run-Checks.py sẽ thực thi các fixture Windows trên Windows; fixture không thay thao tác UX/capture/install thật.
