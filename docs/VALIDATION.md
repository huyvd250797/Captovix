# Validation record — Capture Foundation 0.1.0

Ngày đóng gói: 19/10/2026.

## Đã chạy trong môi trường build Linux

- .NET SDK 10.0.100; target net10.0-windows, x64, EnableWindowsTargeting.
- C# và WPF/XAML build Release: **0 lỗi, 0 cảnh báo** ở lần build sạch trước publish cuối.
- Publish Windows x64 self-contained thành công; đi kèm Microsoft.NETCore.App và Microsoft.WindowsDesktop.App **10.0.13**.
- ScreenRecorderLib **7.0.1** được đưa vào payload x64.
- **12/12** kiểm tra hình học đạt: clipping, reverse drag, vùng ngoài desktop, màn hình gốc âm X/Y, composition qua hai màn hình, pixel khi khác scale, căn chẵn H.264.
- XML của project, XAML, DPI manifest hợp lệ; kiểm tra liên kết 23 event handler của UI.
- Bộ cài Windows được biên dịch bằng NSIS 3.09 trong môi trường Linux. Có publish payload, Microsoft VC++ x64 redistributable và uninstaller. Đây là kiểm chứng tạo bộ cài, không phải kết quả đã cài/chạy trên Windows.
- VC++ redistributable được tải trực tiếp từ URL Microsoft; có Authenticode certificate của Microsoft. Chưa xác nhận chuỗi tin cậy/chạy installer trong Windows ở môi trường này. Script rebuild/cài portable kiểm tra Authenticode trên Windows trước khi thực thi bản tải về.

## Chưa kiểm chứng runtime — cần Windows thật

Không có Windows desktop thật trong phiên xây dựng. Chưa chứng nhận:

- App khởi động, tải mixed native recording engine trên máy đích.
- Chụp đúng pixel, overlay qua nhiều màn hình/DPI khác nhau.
- Hình/âm đúng và đồng bộ trong MP4; pause/resume, thời lượng, FPS thực tế.
- GPU encoding và nhiều GPU, nội dung HDR, thay đổi độ phân giải/thiết bị giữa phiên.
- Hành vi cài đặt, nâng quyền VC Runtime, tạo shortcut, gỡ app, giao diện ở độ scale lớn.

Thực hiện `WINDOWS-TEST-CHECKLIST.md` và lưu diagnostics/log/file mẫu trước khi kết luận các ca trên Đạt. Không coi build sạch là bằng chứng đạt runtime.

## Phạm vi

Đây là V0.1 kiểm chứng engine và khung Editor, chưa có công cụ chú thích/cắt video. Không gán các tính năng trong mockup tương lai thành tính năng đã triển khai.
