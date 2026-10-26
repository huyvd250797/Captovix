# Captovix V1.15.1 — Capture Reliability

## Chụp cuộn

Chụp & quay → Chụp cuộn dọc / ngang → chọn vùng → chọn hướng, header/footer và thời gian chờ.

- Chờ mặc định 700 ms sau mỗi nhịp; chỉnh 350–5000 ms. Trang tải chậm nên thử 1500–2500 ms.
- Chọn 1–3 nấc cuộn. Mặc định 1 để giữ phần giao nhau; giảm số nấc nếu có nguy cơ cuộn quá xa. Với vùng rất nhỏ, dùng thủ công.
- Bấm Tự cuộn, sau đó đưa cửa sổ nguồn ra trước. Các nút trên controller có thể lấy focus; bấm Tiếp tục rồi đưa nguồn ra trước trước nhịp kế tiếp.
- Kiểm tra trước/sau mỗi nhịp: cửa sổ còn tồn tại, chưa thu nhỏ, chưa di chuyển/đổi kích thước, còn ở trước; lấy mẫu 4 góc và tâm vùng để nhận biết cửa sổ khác che.
- Có thay đổi thì tạm dừng, không ghi khung đó. Mất focus/bị che: khôi phục nguồn rồi Tiếp tục. Di chuyển/resize: kết thúc phiên, chọn lại vùng. Không tự đổi sang chụp cửa sổ khác.
- Sau 3 khung trùng, dừng tự cuộn và báo có thể đã cuối trang hoặc app không nhận cuộn. Không khẳng định chắc chắn tới cuối.
- Thủ công: Ctrl+Shift+F10 lấy khung, Ctrl+Shift+F11 ghép; Esc hủy. Vẫn có thể khôi phục PNG đã ghi.
- Loại khung tạo revision; chỉ chuyển revision đang làm việc khi đã ghi xong manifest. Sửa overlap ghép ngang/header-footer dùng đúng kích thước vùng xử lý.
- File candidate chưa commit được dọn sau lỗi/hủy; frame đã commit được giữ nguyên.

`capture.log` và `capture.log.previous` tại `%LOCALAPPDATA%\HuyVo\Captovix` ghi thời điểm, vùng pixel, hướng cuộn, thời gian chờ, số khung và lý do dừng. Không ghi nội dung pixel vào log. Lỗi exception vẫn ở `errors.log`.

## Chế độ tối

Nền than xám #1B1E25, panel #23262E, chữ #DCE1EA; hover #353D4B, viền #3B424F. Menu/submenu/context menu có template riêng, không dùng nền hover trắng của theme Windows. Nút công cụ, nền vùng làm việc và ô caro trong suốt đổi theo theme. Màu ảnh và đối tượng đã vẽ giữ nguyên. Màu nhấn cá nhân và Light/Dark/System vẫn được lưu trên máy.

## Logo khi chạy

Giữ đúng hình logo tím đã chốt. Gán icon nhỏ/lớn cho cửa sổ đang chạy; tray đọc cùng file ICO. EXE, cửa sổ và shortcut cài mới cùng dùng artwork đó. Tên file icon mới giúp tránh đường dẫn icon cũ còn trong bộ nhớ Windows.

Sau update, thoát cả tray rồi mở lại. Nếu **shortcut đã ghim từ bản cũ** vẫn hiện icon cũ, bỏ ghim nó, mở Captovix từ Start/Desktop rồi ghim lại. App không tự sửa/xóa các mục taskbar người dùng đã ghim và không xóa icon cache toàn hệ thống. Khi mở nhầm bản cũ, kiểm tra Trợ giúp → Giới thiệu Captovix phải là 1.15.1.

## Giới hạn và nghiệm thu

Chưa có Windows desktop trong môi trường build này. Chưa xác nhận tương thích cuộn tự động với bất kỳ app cụ thể nào; không công bố Chrome/Edge/Excel đã đạt. Gửi WM_MOUSEWHEEL/WM_MOUSEHWHEEL tùy app hỗ trợ, có đường thủ công. Lấy mẫu che khuất không phát hiện được mọi lớp phủ/transparency; nội dung tự thay đổi, zoom trang, sticky header động và tải vô hạn vẫn phải kiểm tra mối nối. Giữ nguồn đứng yên và không bị che.

Checklist cần chạy trên Windows trước triển khai rộng:

| Hạng mục | Thao tác / kết quả cần đạt | Trạng thái |
| --- | --- | --- |
| Menu tối | Hover menu cấp 1/2/3, context menu, dòng bị vô hiệu; chữ đọc rõ | Chờ Windows |
| Theme | Light/Dark/System, đổi màu nhấn khi đang chọn tool; mở lại giữ lựa chọn | Chờ Windows |
| Logo | EXE, Alt+Tab, taskbar, tray, Desktop/Start đều logo tím; thử ghim mới | Chờ Windows |
| Chụp dọc | Mẫu scroll-capture-test.html; không mất/lặp dòng, thử 700/1500 ms | Chờ Windows |
| Chụp ngang | Mẫu horizontal-capture-test.html; không mất/lặp cột, sửa overlap | Chờ Windows |
| Focus / che | Chuyển cửa sổ, che góc/tâm; tạm dừng không thêm frame sai | Chờ Windows |
| Di chuyển / DPI | Đổi kích thước, chuyển màn, scale 100–200%; không ghép sai vùng | Chờ Windows |
| Hủy / phục hồi | Hủy trong lúc đợi/lấy ảnh; mở lại manifest khôi phục frame đã commit | Chờ Windows |
| Revision | Loại khung; phiên gốc giữ nguyên, revision lưu/mở lại được | Chờ Windows |
| Cài / update | Cài mới vào D:\Captovix\Captovix; update từ 1.15.0 giữ dữ liệu/theme | Chờ Windows |

V1.16 theo roadmap là OCR & Text Extraction; cần chọn engine phù hợp, lấy chữ theo vùng/toàn ảnh, copy Unicode và kiểm tra tiếng Việt/Anh. Chưa triển khai OCR trong bản này.
