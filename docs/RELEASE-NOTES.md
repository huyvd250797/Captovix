# Captovix V1.36.0 — Text & Callout Polish

Nâng thẳng từ source V1.33.0 theo yêu cầu Boss, bỏ qua phát hành V1.34.0 và V1.35.0. Giữ chức năng đã có; không tuyên bố hoàn thành các phạm vi bị bỏ qua.

- Callout bắt đầu từ đuôi: điểm nhấn đầu tiên được giữ cố định, kéo để tạo thân ở bên trái/phải. Thân mặc định căn giữa theo chiều dọc quanh điểm đuôi, đệm30px, tối thiểu172×96px; bấm đơn tạo thân220×120px. Đuôi có node riêng; resize thân không kéo theo đuôi (kể cả khi xoay).
- Auto-fit mặc định giữ kích thước đã vẽ và chỉ nở thêm theo nội dung đang nhập, xuống dòng theo chiều rộng. Không tự co hộp khi xóa chữ; vẫn sửa rộng/cao độc lập bằng node/các trường thuộc tính. Mẫu đã lưu giữ lựa chọn bật/tắt auto-fit của người dùng.
- Nhập trực tiếp trên ảnh: có chữ Việt/IME, căn dọc của con trỏ theo hộp; click ngoài/Ctrl+Enter hoàn tất, Esc hủy, Enter xuống dòng. Composition được hoàn tất trước khi đóng phiên nhập; không lưu chữ preedit dở.
- Di chuyển Callout dài tái sử dụng dàn chữ và bản vẽ đã đóng băng; không dựng lại outline toàn bộ glyph hay ghép hình đuôi mỗi lần đổi vị trí. Cập nhật khi gõ gộp theo nhịp render, không chờ bộ hẹn giờ400ms của thuộc tính. Cache yếu theo đối tượng và riêng từng thread, không giữ lịch sử chữ không giới hạn.
- Thuộc tính/preset áp dụng đúng loại Chữ/Callout/Bước; chọn mẫu Callout cập nhật bố cục và tự nở nếu cần, không đổi vị trí đuôi. Không đổi schema17 hay phá mẫu cũ.
- Khôi phục logo và tên Captovix gọn ở góc trái hàng menu; bỏ menu chẩn đoán, báo cáo hiệu năng và hướng dẫn kiểm thử, cùng mục Máy & màn hình trong Thiết lập. Giữ công cụ chụp/chỉnh ảnh, khôi phục, cập nhật và bản quyền.
- Tọa độ chụp dùng lõi vàng2pixel vật lý, viền đen0.25pixel mỗi bên thay lõi cam1pixel/viền0.5pixel trước đây.
- Giữ snapshot ảnh ở pixel vật lý và xuất PNG/copy không giảm theo zoom. Xem100% để đánh giá pixel thật; Vừa khung là bản xem đã thu nhỏ. Không tăng nét giả hay upscale nguồn.
- Updater luôn chọn gói Full của stable mới nhất, không bắt cài bản trung gian. V1.31→V1.33/V1.36 được kiểm thử chọn đích bằng SDK thật; gói V1.36 được kiểm tra tải/hash với locator V1.31. Backup dữ liệu trước áp dụng và xác minh SHA-256 giữ nguyên.

## Cài đặt và bàn giao

- `Captovix-v1.36.0-Update-Packages.zip`: giải nén, thoát Captovix, chạy `HuyVo.Captovix-win-Setup.exe`, giữ đường dẫn cài hiện tại. Không uninstall.
- `Captovix V1.36.0 — Text & Callout Polish.zip`: source, tests và hướng dẫn build. Windows x64/.NET10 SDK/NSIS3; chạy `python scripts/Run-Checks.py`, sau đó `powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1`.
- Cập nhật trong app chỉ thấy bản mới sau khi publisher đưa đủ5assets trong ZIP lên stable release v1.36.0. Không upload duy nhất ZIP hoặc source thay assets. Không tự push/publish trong lần bàn giao này.
- Không migration DB; không redeploy Supabase/admin. Public config bản quyền, dữ liệu và endpoint update giữ nguyên từ source Boss gửi. Không gọi production để tạo KEY/trial khi kiểm thử.
- Build/logic/package đã kiểm theo `docs/VALIDATION.md`. Chưa thực thi WPF/Setup/IME native tại Linux; checklist Windows ở `docs/TEXT-CALLOUT-POLISH-V136.md`.
- Phiên bản tiếp theo theo roadmap đã có: **V1.37.0 — Image Tools & OCR Usability**: kéo vùng nhận chữ/copy, zoom/pan và khả năng xem trước/hoàn tác các công cụ pixel. Chưa xây phiên bản này.

