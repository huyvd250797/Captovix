# V1.13.0 — Kết quả kiểm tra

23/10/2026. Môi trường thực thi Linux x64, .NET SDK10.0.100. Ứng dụng Windows C#/.NET10 WPF, runtime tự chứa10.0.13, ScreenRecorderLib7.0.1 và Velopack1.2.161; không thay dependency app.

- 19 bộ kiểm tra logic/cấu trúc chạy thành công: 1.180 kiểm tra, trong đó CompositionChecks151. Bao gồm bố cục1–16 ảnh,1000 trường hợp Fit/Fill ngẫu nhiên, crop/ratio, group caption, Undo/Redo, template không nhúng ảnh công việc, clone/style, migrate schema1–11, từ chối future schema và project hash/path lạ.
- 10 bộ kiểm tra WPF cross-build thành công; **chưa thực thi** vì môi trường Linux. Bộ CompositionWindowsChecks chứa render Fit/Fill/bo góc, kéo bên trong ô/Undo, zoom không đổi export, callout hẹp/chữ dài/10 kiểu, khởi tạo cửa sổ và tách module.
- Publish Windows x64 self-contained đã thành công; kiểm tra nupkg/feed/installer và ZIP được ghi riêng trong PACKAGE-VALIDATION.txt sau đóng gói.
- Không coi cross-build hoặc kiểm tra XML là bằng chứng giao diện/IME/chuột/bộ nhớ/clipboard đã hoạt động trên Windows. Đặc biệt cần thử native color picker, drag Explorer/thư viện, Ctrl+B/I/U trong inline, template/project trên máy khác và installer/update ở D:\Captovix\Captovix.

Giới hạn: pixel operations/canvas40MP,128 triệu ký tự PNG mã hóa mỗi doc, history8 triệu ký tự; ảnh lớn có thể ít Undo và tốn RAM/CPU. HDR/màu rộng riêng. Cắt lớp ảnh bằng form pixel nguồn; template giữ layout/vector và placeholder, không phải kho asset cloud. Bản phát hành chưa được đăng GitHub tự động. Xem WINDOWS-TEST-CHECKLIST.md.
