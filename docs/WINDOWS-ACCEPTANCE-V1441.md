# V1.44.1 — Windows acceptance

Host xây dựng Linux chỉ cross-build WPF. Chưa ghi native PASS. Kiểm Windows10/11x64, light/dark,100/125/150/200%DPI, app nhỏ900px và màn hình1920px trở lên.

1. Cài mới/cài đè1.44.0, thoát khay trước cài, mở/reboot; giữ thư viện/KEY/thiết lập. Update gói đúng/sai checksum, thiếu dung lượng. Menu phải/tooltip không lỗi parent. Giữ startup native fallback.
2. Bấm Mockup mở cấu hình ngay, không menu4dòng. Quét vùng: kéo ngược/crop/biên/nhỏ8px, đổi vùng rồi bấm Quét. Quét toàn ảnh: không kéo vẫn Quét. Kiểm tọa độ khung khớp nội dung; đổi OCR language; đóng trước/đang load không crash. Ảnh>20MP phải dùng vùng.
3. Rà soát140–200khung → Chọn tất cả → bỏ khung trùng → preview/apply không lỗi trần120 cũ. Không giả xác nhận font. Áp dụng mở sidebar và sửa riêng/nhóm; Undo toàn lần apply, redo/lưu/mở.
4. Dùng ảnh Boss cung cấp có chuỗi01350006 - Nguyen Phi Long, và Times14px xanh nghiêng. Trước double-click chụp pixel/font/cỡ/khung, sau double-click chưa gõ không xuống dòng/tăng size/dời vị trí. IME Việt/SelectAll/paste/CtrlEnter/Esc, nhập ngắn, nhập dài mới wrap. Sidebar cũng wrap; undo/redo/save/reopen/export giữ format và dòng. Test font serif/mono, đậm/nghiêng/gạch chân.
5. Thư viện1000ảnh: cuộn1nấc dọc/ngang không nhảy cuối; trackpad/Shift+wheel, đổi layout/lọc/đổi kích thước/selection không thay vị trí ngoài chủ ý. X/star hai góc dễ click, margin nhỏ, không chọn nhầm.
6. Arrow Không đầu/Hai đầu độc lập đúng, tắt loại kia; vẽ/đổi đối tượng/undo/lưu. Shadow9hướng không icon vuông thừa; bật/tắt/khoảng cách/blur/opacity, selected state/theme đúng.
7. Chọn5ảnh(Ctrl/Shift), Ctrl+C/menuCopy/drag thành nhóm sang Explorer, ứng dụng chat/trình duyệt/Office có nhận FileDrop. Đối chiếu đủ5PNG, chú thích/crop/alpha/kích thước xuất đúng, nguồn không bị di chuyển. Thử1ảnh, mixedvideo, file mất/sidecar lỗi, bấm thả trước chuẩn bị xong, clipboard bận. Một số đích không hỗ trợ dán nhiều file cần dùng drag/drop/upload.
8. RAM khi xuất50ảnh4K không giữ50bitmap cùng lúc; UI vẫn phản hồi, chuẩn bị có status. Temp/LibraryExports trước24h giữ để paste, sau24h có thể dọn, nguồn/KEY/project không bị tác động.
9. Toolbar ở giữa toànapp không bị inspector đè; window900/1366/1920px, nhiều hàng không che workspace. Header chỉ Captovix·V1.44.1, About/changelog vẫn đủ tên bản.

WorkflowPolishWindowsChecks bổ sung các kiểm soát TextBox một dòng, font/wrap, cuộn dọc/ngang và cửa sổ cấu hình. Cần chạy thật trên Windows, kèm nghiệm thu M6 theo WINDOWS-ACCEPTANCE-V144.md. Chưa publish stable trong lần bàn giao.
