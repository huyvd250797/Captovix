# Windows acceptance V1.43.0 — chưa chạy trên Linux

Chạy trên Windows10/11x64 với dữ liệu1.42 và hồ sơ mới; scale100/125/150/200%, hai màn hình khác DPI, màn hình trái/trên có gốc âm.

1. `python scripts/Run-Checks.py` phải thực thi toàn bộ WPF fixtures, đặc biệt StartupWindowsChecks, HybridPopupWindowsChecks và ElementScanWindowsChecks. Cross-build không thay thế kết quả chạy.
2. Cài đè1.42 vào đường dẫn đang dùng, mở lại3lần; ảnh/nhóm/cấu hình/KEY còn nguyên. Capture/OCR/quay/clipboard/export/lưu/Undo vẫn dùng được.
3. Chuột phải thumbnail của hai ảnh khác nhau → Mở vị trí tệp chọn đúng ảnh trong Explorer. Thử file đã bị di chuyển, khoảng trắng/Unicode trong path. Mở/đóng menu10lần light/dark.
4. Hover tooltip ảnh/công cụ, menu con Sắp xếp lớp và Căn chỉnh. Không có lỗi logical/visual parent, không hộp lỗi lặp. Viền1DIP trung tính và không đổi theo accent.
5. Dán hai lớp ảnh, chuột phải một lớp chưa chọn; thử Front/Forward/Backward/Back và Undo/Redo. Chọn nhiều, nhóm/khóa/ẩn, menu trong vùng trống của selection và nền ảnh. Lệnh lớp trên ảnh gốc nền phải disable đúng.
6. Chụp form WinForms/WPF có button/input/label, OCR Việt/Anh. Kiểm trạng thái Hybrid và quét Mockup. So khung ở100%, các vị trí chữ/control, font và cây nhóm. Không tự Apply.
7. UIA không khả dụng (canvas/app tự vẽ), provider chậm, app quyền cao hơn: fallback ảnh/OCR, ảnh vẫn lưu được, hủy bằng Esc và chụp lại hoạt động.
8. Di chuyển/resize cửa sổ, mở popup che, đổi chữ/animation trong lúc đọc UI: metadata không khớp bị loại. Kiểm ảnh kết quả không lấy chữ/nút từ UI hiện tại đã khác screenshot.
9. Chụp vùng cắt control, qua hai màn hình khác DPI; crop/move/zoom/rotate ảnh sau chụp, che chữ rồi quét. Metadata không được hiện lại tên/giá trị bị che. TextUIName khác OCR không tự chèn.
10. Đóng/mở lại app quét screenshot cũ có metadata; thay file nguồn/chỉnh sidecar hỏng thì fallback. Xóa/khôi phục ảnh giữ metadata; nhập ảnh/dự án không có metadata vẫn dùng scan ảnh.
11. Khung trên20MP,900nodes/provider treo, hủy nhanh/lặp20lần: ảnh vẫn dùng chế độ cũ, không còn worker treo. Đo RAM/privatebytes và độ trễ trên máy thật, kiểm metadata tối đa512KB/ảnh/256nodes.
12. Update1.42→1.43 và nhảy bản cũ; gói saihash phải bị từ chối. Test mất mạng/retry/cài đè/rollback trước publishstable.

Ghi kết quả Windows thực tế, phiên bảnOS, DPI, ảnh mẫu và errors.log nếu có; không đánh dấu đạt bằng Linux compile.
