# Captovix V1.11.0 — Focus & Privacy

- **Kính phóng đại (M):** kéo vùng đặt kính tại chỗ, chỉnh 1.25–8×, chữ nhật hoặc ellipse, màu/dày viền và bật/tắt viền. Di chuyển kính để lấy vùng khác; hình ảnh bên trong giữ hướng của ảnh. Kính lấy ảnh gốc đã áp các vùng che đang bật, không lấy chữ/shape chú thích, không tạo thêm chi tiết thật.
- **Spotlight (O):** giữ sáng vùng chọn, làm tối phần ngoài; chữ nhật/ellipse, màu và độ tối 0–100%. Nhiều vùng sáng được hợp lại; màu/độ tối lấy Spotlight trên cùng. Lớp nền spotlight luôn dưới các chú thích; dùng menu chuột phải sắp xếp các spotlight để chọn style chung.
- **Highlight (H):** chữ nhật/ellipse, màu và độ đậm 0–100%. Dữ liệu highlight cũ giữ nền chữ nhật và alpha 78 như trước.
- **Blur / Pixel / Che kín:** chọn hình vùng chữ nhật/ellipse; xoay, resize, đổi màu che và mức Blur/Pixel 4–64. Chuyển ba kiểu che tại cùng vùng bằng Áp dụng. Che kín luôn đặc 100%, độc lập opacity chung. Lớp che đặc luôn phủ trên chú thích, kính và filter.
- Bộ lọc làm việc theo tile, có halo cho blur và cache giới hạn. Khi có che đặc, mẫu filter được tô che trước khi tính blur/pixel để tránh kéo màu từ phần đã che ra ngoài. Mẫu ngoài crop được thay trắng; kính không lấy ảnh ngoài crop. Cache cập nhật khi vị trí/màu/trạng thái che hoặc crop thay đổi.
- Các hiệu ứng là đối tượng sửa lại được: kéo/resize/xoay, nhóm, khóa/ẩn, Undo/Redo, Ctrl+C/V, Ctrl+D, lưu/mở lại. Bấm trực tiếp để chỉnh từ công cụ vẽ khác; Alt+kéo để tạo mới chồng lên vùng đã có.
- 5 preset mới: Kính rõ 2x, Spotlight nền tối, Che thông tin, Làm mờ mạnh, Highlight vàng. Có thể lưu preset riêng gồm hình vùng, mức zoom/opacity, màu và mức xử lý. Chọn preset → Chỉ dùng preset cho nét mới để chuyển công cụ tương ứng; Áp dụng trên đối tượng giữ loại công cụ hiện tại.
- **Kiểm tra trước khi chia sẻ:** thống kê vùng che đặc/filter/đang ẩn và vùng vượt ảnh/crop; nút kiểm tra ở Actual Pixels ngay trong Editor. Copy/xuất nhắc trạng thái che. PNG/JPEG xuất là ảnh đã ghép; thư viện/dự án vẫn giữ ảnh gốc. Blur/pixel và highlight không thay thế che đặc khi cần giấu dữ liệu.

Schema 9 đọc 1–8; sidecar cũ giữ .pre-v1.11.bak trước lần lưu đầu; giữ backup cũ và ảnh gốc. Workflow vẫn cơ chế .pre-v1.9.bak. Root cài, URL update và stack giữ nguyên.

Đã build/cross-build trên Linux; chưa chạy Windows thật. Không coi logic/build là bằng chứng pixel privacy đã nghiệm thu. Thử PNG/JPEG/clipboard, lens chồng mask, nhiều monitor/DPI và installer/updater trên máy Windows trước triển khai rộng. Không có OCR hoặc tự phát hiện thông tin nhạy cảm.

V1.12 — Selection & Pixel Editing dự kiến bổ sung chọn vùng pixel bằng chữ nhật/ellipse/lasso, copy/cut/paste vùng thành đối tượng ảnh, fill/eraser và cắt bỏ hàng/cột thừa. Tách rõ chọn pixel với chọn chú thích; giữ ảnh gốc, Undo có giới hạn bộ nhớ và lưu/mở đủ dữ liệu vùng đã dán. Video tiếp tục để sau.
