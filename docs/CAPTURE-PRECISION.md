# Captovix V1.24.0 — Capture Precision

- Chụp vùng: kéo → chỉnh cạnh/góc nếu cần → Enter hoặc nút máy ảnh là chụp, bỏ bước chọn hành động dư. Nét đứt chuyển động chậm báo vùng chưa chụp. Mũi tên cuộn dọc/ngang nằm ngay vùng chọn, nền trong suốt; video chỉ có xác nhận vùng. Esc/chuột phải hủy.
- Giữ nền capture pixel và kiểm topology/DPI sau hẹn giờ. Chọn phần tử cha/con bằng con lăn, ưu tiên cạnh/góc modal/cửa sổ; vùng cố định/gần đây kế thừa nền trước. Độ tương thích UIA phụ thuộc ứng dụng; không cam kết nhận diện mọi UI.
- Quản lý thư viện: cây năm/tháng/ngày, ảnh dạng ô tự xuống dòng theo chiều rộng; tải100ảnh/lần, nút Xem thêm. Tùy chọn Ẩn ngày tháng không có hình bỏ cả tháng trống. Ctrl/Shift chọn nhiều, mở/xóa/xuất giữ nguyên. Bỏ đường vào màn hình quản lý bộ sưu tập cũ; dữ liệu tag/collection giữ, backup/export còn trong menu.
- Chọn công cụ áp dụng mẫu đã lưu gần nhất trong họ công cụ; chọn mẫu cụ thể vẫn ưu tiên mẫu vừa bấm. Đã lưu mở sẵn; sao trên preview chỉ hiện hover/focus. Sắp xếp công cụ bằng kéo-thả trên thanh công cụ hoặc Hiển thị → Sắp xếp không gian → Sắp xếp công cụ cá nhân; thứ tự giữ trên máy.
- Callout vẽ kéo giữ nguyên thân khi thả, tắt tự co chữ cho đối tượng mới để nhập không đổi thân. Bật Tự co trong thuộc tính nếu muốn. Một click giữ kích thước mặc định. Tay nắm đuôi màu cam, tay nắm thân xanh/trắng dạng tròn. Đuôi vẫn tự do.
- Footer/nút update gọn hơn. Giới thiệu ghi: Ý tưởng, định hướng sản phẩm và thiết kế trải nghiệm: Võ Đức Huy (HuyVo). ©2026HuyVo. All rights reserved.

## Cài/update

Giải nén Update-Packages, đóng app và chạy HuyVo.Captovix-win-Setup.exe, hoặc đăng đủ5asset lên GitHub Release stable/latest tagv1.24.0 để app kiểm tra cập nhật. Không đăng chỉ file ZIP. Source ZIP dùng phát triển, không dùng cài. Không tự publish trong lần bàn giao này. Setup chưa ký số.

## Giới hạn và nghiệm thu

Build/publish Windows x64 tự chứa.NET10.0.13; giữ stack/logo/update source/schema và dữ liệu. Đã chạy7bộ logic/static liên quan,620assertions, và cross-build RefinementWindowsChecks; chưa chạy WPF/Windows desktop. Kiểm nupkg và integrity gói riêng sau đóng gói.

Cần Windows nghiệm thu:1Enter/chuột/nút camera, hủy/countdown/quay, arrows cuộn, nhiều DPI100–200%và tọa độ âm; ảnh/clipboard/PNG đúng pixel; thư viện co giãn/lazy/empty month/multi-select/delete; chọn mẫu gần nhất sau restart/chọn mẫu thủ công; callout thân/đuôi/IME/Undo; caption/taskbar/footer/install-update. Không có screenshot UI chạy thật hoặc tuyên bố đã kiểm thị giác. Margins cũ vẫn theo giới hạn1.23.1, chữ không kéo méo glyph hai chiều.

## Tiếp theo V1.25 — Scrolling Capture Quality

Cải thiện mối nối dọc/ngang, vùng header/footer cố định, phát hiện khung lặp/lỗi, bộ trang mẫu để kiểm không mất/lặp dòng/cột. Ưu tiên bản vá phản hồi Windows1.24 trước.
