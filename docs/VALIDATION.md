# V1.14.0 — Kết quả kiểm tra (23/10/2026)

- Môi trường: Linux x64, SDK10.0.100, cross-target Windows x64, runtime đi kèm10.0.13. Không có desktop Windows thật trong lần bàn giao này.
- Đã chạy20 bộ kiểm tra logic/static: **1.258 kiểm tra đạt**, gồm78 kiểm tra mới về scrolling/appearance. Bao gồm match shift1–106px, khung trùng, nội dung lặp, reverse, uncertainty, ghép BGRA từng byte, overlap thủ công, cancellation và giới hạn; mode/theme persistence/corrupt backup/isolation/contrast; menu và wiring system/theme/capture safety. Giữ các bộ geometry/fidelity/editor/history/migration/shapes/layers/text/blur/composition/update/installer trước đây.
- **11 bộ WPF chỉ cross-build thành công**, không thực thi trên Linux. Bộ mới ScrollAppearanceWindowsChecks kiểm tra resource/template sáng/tối, logo pack URI và session PNG/hash/compose/export roundtrip khi chạy trên Windows. Không được tính các assertions này là đã chạy.
- Main app build không lỗi; publish self-contained và kiểm tra package/feed/hash/ZIP được ghi riêng ở PACKAGE-VALIDATION.txt sau đóng gói. Kiểm tra UpdateChecks với nupkg thật bổ sung9 kiểm tra tích hợp so với52 baseline; không cộng baseline52 hai lần.
- Nền tảng capture/native/keyboard và câu lệnh installer --installto được so byte với source V1.13; chỉ cập nhật version installer và logo asset. Không thêm dấu -- passthrough (lỗi EXE_ARGS).

## Chưa được xác minh thực tế

Không khẳng định UI/theme/hotkey/mouse/capture, độ chính xác ghép trang Windows, nhiều DPI/màn hình, dung lượng/bộ nhớ, clipboard, installer/update đã đạt trên máy Windows thật. Cần thực hiện docs/WINDOWS-TEST-V1.14.md, lưu kết quả theo từng app và từng scale100–200%. Trang offline docs/scroll-capture-test.html dùng làm mẫu có hàng đánh số. HDR/màu rộng đánh giá riêng.

Nguồn cuộn tĩnh/SDR8, một chiều xuống; không tự cuộn, không hiểu nội dung DOM, không tự loại thanh cố định. Matching là lấy mẫu và có thể sai ở nội dung lặp/động; luôn xem điểm nối100%. Giới hạn4MP/khung,24khung,64MPnguồn,40MPoutput,32768px cao. Theme lưu LocalAppData theo tài khoản/máy, không đồng bộ. Native dialogs/installer có thể theo Windows thay vì theme ứng dụng. Không đổi schema12/tài liệu cũ và không mở rộng video.

Build/kiểm tra lại: scripts/Run-Checks.py dùng DOTNET_EXE nếu dotnet không có PATH; trên Windows dùng scripts/Build-Windows.ps1 để thực thi WPF và kiểm thử installer path. Gói bàn giao chưa tự đăng GitHub; Setup chưa ký bằng chứng thư nhà phát hành.
