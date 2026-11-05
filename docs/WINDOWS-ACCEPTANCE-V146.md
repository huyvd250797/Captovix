# V1.46.0 — Windows acceptance: PENDING

Chạy trên Windows 10/11 x64, dùng bản sao dữ liệu, thử cả theme và DPI 100/125/150/200%. Host Linux chỉ kiểm logic/cross-build, không được điền PASS thay kết quả thực tế.

1. Cài mới và cài đè V1.45.0 ở đường dẫn đang dùng; mở app, chụp vùng, quay, OCR, xuất PNG, license và thư viện. So hash ảnh gốc trước/sau. Không đổi KEY/backend.
2. Chạy toàn bộ scripts/Run-Checks.py. Chạy MockupReliabilityWindowsChecks để kiểm pixel nguyên vẹn khi tách nút, glyph nhãn sau đổi màu thân, inspector không tự làm tròn/commit khi mở, typography pill, gradient paint và icon alpha. Các bộ Windows trước đây vẫn phải đạt.
3. Quét form thực tế có nút, input, label đậm/nghiêng, icon và bảng. Cấu hình/rà soát 100% rõ như ảnh nguồn; OCR dùng ảnh đầy đủ độ phân giải. Apply chưa sửa giữ nguyên ảnh dù OCR/font dự đoán sai.
4. Nút có label tạo hai lớp: thân dưới, nhãn trên. Thử UI-only không có text runs và OCR nhiều dòng. Chọn từng lớp qua inspector, chọn cả nhóm, đi vào/ra nhóm, đổi vị trí và z-order.
5. Đổi nền/viền/bo góc nút không đổi text/font nhãn; đổi text nhãn không đổi hình thức thân. Sau đổi nền phẳng, không thấy mảng nền cũ phía sau chữ. Kiểm chữ trắng, chữ tối, chống răng cưa, nút góc tròn và nền ảnh phức tạp.
6. Double-click chữ → click ngoài không sửa; nhập rồi trả về đúng nội dung ban đầu; Esc; IME tiếng Việt; Ctrl+Enter. Font/cỡ/vị trí/khung/ảnh nguồn không đổi và không tạo Undo mới khi không thay nội dung. Văn bản một dòng không tự rớt dòng. Nội dung vượt khung được xuống dòng.
7. Ctrl+B/I/U khi chọn chữ, đang nhập trực tiếp, đang ở nội dung inspector. Chỉ toàn đối tượng được chọn đổi định dạng; không đổi nhóm khác. Click B/I/U và phím tắt cho kết quả tương đương. Ctrl+Z/Y hoàn tác đúng.
8. Times New Roman 14 px xanh/nghiêng, Arial/Segoe UI/Tahoma, font không cài và UI Automation có font đã xác nhận. Đối chiếu ảnh trước/sau. Font bitmap chỉ là gần giống; giữ ảnh khi chưa sửa. Font vắng trên máy không làm mất tên gốc khi mở rồi đóng.
9. Inspector và màn rà soát: thấy màu hiện tại, palette, chọn màu tùy chỉnh, mã không hợp lệ không áp dụng; sample rõ ở dark/light và panel hẹp. Kiểm bàn phím/Tab, tooltip và không cuộn ngang cả form.
10. Icon chỉnh màu góc phải mở đúng trang. Màu nhấn đơn/gradient, góc gradient, icon theo nhấn hoặc màu/gradient riêng. Header, title icon và tray đổi theo; reload giữ lựa chọn; reset khôi phục mặc định. EXE/installer/shortcut giữ artwork tĩnh. Nút Quay đỏ tươi, chữ trắng.
11. Menu không còn Cập nhật & khôi phục dữ liệu. Backup cũ không bị xóa; update kiểm hash/audit và backup trước bàn giao updater vẫn hoạt động. Recovery.cmd còn dùng khi app không mở được.
12. Lưu/mở lại, Undo/Redo, copy/paste/duplicate, di chuyển/resize/rotate/delete/hide lớp và nhóm; kiểm caption cache không giữ ảnh đã đóng. Thử nhiều vùng quét 4K/20MP và thư viện 300 ảnh cả ngang/dọc; đo RAM/peak/p95 trên cùng máy với V1.45. Không giảm ảnh để lấy số đo đẹp.

Lưu evidence gồm OS/DPI/version/từng kết quả/log. Chỉ đặt manualAcceptance=PASS trong artifacts/windows-acceptance.json khi toàn bộ kiểm thực tế đạt. First-paint smoke không thay thế nghiệm thu này.
