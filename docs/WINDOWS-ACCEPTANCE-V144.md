# Windows acceptance V1.44.0 — bắt buộc trước phát hành rộng

Chưa chạy native trong môi trường Linux. Chạy trên Windows10/11x64, màn hình100/125/150/200%, màn hình phụ có gốc âm.

1. Cài mới Full Setup, chạy lại sau reboot; cài đè1.43, dùng KEY/dữ liệu cũ; update valid/corrupt/thiếu dung lượng. Không có lỗi startup v1.40 hoặc parent ContextMenu/ToolTip. Chuột phải thư viện Mở vị trí tệp và Editor đủ4lệnh lớp + Undo.
2. Chụp cửa sổ/vùng/toàn màn hình, ảnh/OCR khi không có UIA, DPI/crop/multi-monitor; pixel capture/export đúng kích thước, ảnh gốc không bị sửa.
3. Chạy `DOTNET_EXE=... python scripts/Run-Checks.py` tương ứng env Windows hoặc dotnet build/run từng project. M6WindowsChecks kiểm renderer/sidebar/doubleclick/save22; các native suite cũ không được bỏ qua.
4. Form thật: font Times New Roman14px xanh nghiêng; đủ cả đậm/gạch chân; UI thiếu style phải đánh dấu/chọn giữ ảnh. Sửa chữ inline và sidebar → Ctrl+Z/Y → lưu/đóng/mở → export; định dạng/geometry không tự đổi.
5. Sidebar: mọi field, OCR vi/en/Dừng/lỗi/đổi lựa chọn, eyedropper/chọn màu, compare bật/tắt, thu hẹp cửa sổ và đổi object nhanh; không popup khi sửa object hiện có, không mất ký tự tiếng Việt/IME. Tạo mới vẫn rà soát như trước.
6. Quét form2cột/panel/nút có chữ/tab/menu/icon/bảng; khung con không trùng, label ghép đúng, di chuyển nhóm/ungroup/duplicate/undo. Nền phức tạp/không chắc giữ ảnh, preview khớp apply. Chuẩn bị bộ>=30form theo MODEL-EVALUATION-V144.
7. AI: chưa cài dịch vụ vẫn scanảnh; list/model/timeout/dừng/close/badJSON/sai tọa độ/ảnh có thông tin nhạy cảm. Chỉ gửi đúng vùng khi bấm; model không tự nạp ởstartup; xác minh dịch vụ giải phóng model sau phiên.
8. Mở ảnh4K/8K, sửa50lần, mở/đóng/chuyển100ảnh, OCR lặp, đóngeditor; ghi peak/idleRAM và p50/p95 UI trước/sau trên cùng máy. Đối chiếu pixel export; đo cả tiến trình model nếu dùngAI. Không dùng benchmark lịch sử JSON để khẳng định working set Windows.
9. Save schema22 tạo `.pre-v1.44.bak`; source/KEY/thiết lập còn nguyên, rollback bảo toàn backup; bản cũ từ chối schema mới rõ ràng. Dọn cache/temp rồi mở lại thumbnails tự tạo, không mất nguồn/dựán.

Ghi rõ máy/OS/DPI, phiên bản cũ, kết quả từng mục và đính kèm log/crash nếu có. Không phát hành rộng khi mục1/2/4/5/9 chưa đạt.
