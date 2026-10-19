# Kiến trúc V0.1

## Một Editor

MainWindow là cửa sổ quản lý duy nhất. Capture configuration là Expander trong Editor. RegionPicker chỉ là overlay chọn vùng theo từng màn hình; RecordingBar là điều khiển tạm thời trong phiên video. Hai loại cửa sổ này không lưu thư viện hoặc cấu hình độc lập.

## Pixel và DPI

Manifest khai báo PerMonitorV2. EnumDisplayMonitors/GetMonitorInfo và GetCursorPos cung cấp pixel vật lý; tránh nhân thêm scale cho crop. Overlay WPF dùng PointFromScreen chỉ khi vẽ vùng chọn trong tọa độ DIP. Crop trên nguồn màn hình là tọa độ cục bộ; vị trí ghép là tương đối với góc trái trên vùng chọn. Tọa độ desktop có thể âm. Không gộp các độ scale thành một hệ DIP chung.

Desktop Duplication được dùng cho nguồn màn hình; Windows Graphics Capture được dùng cho HWND cửa sổ, thông qua ScreenRecorderLib. Engine mã hóa H.264/MP4 bằng Media Foundation; microphone và WASAPI loopback là hai nguồn được trộn.

## State capture

Idle → Selecting/Delay → Starting → Recording → Stopping → Completed/Failed → Idle. Trong toàn bộ phiên, khóa nút và cấu hình để ngăn hai recorder đồng thời. Screenshot đi thẳng từ Starting đến Completed. Hotkey stop/pause chỉ tác động phiên video đang có. Engine hoàn tất file rồi mới đổi tên `pending` thành tên cuối và ghi lịch sử. Stop có watchdog 45 giây; không gán thành công nếu watchdog hết hạn.

## Persist

Media lưu riêng theo file; JSON metadata/settings ghi vào temp rồi atomic replace trong cùng thư mục. Startup khôi phục file cuối chưa có metadata; không tự khôi phục file pending. Một mutex theo phiên người dùng ngăn hai cửa sổ app ghi đè thư viện. MediaImport chuyển ảnh JPEG/BMP thành PNG gốc nội bộ để schema nhất quán.

## Đóng gói

Publish win-x64 self-contained, không trimming/single-file để giữ WPF và mixed native engine tương thích. App runtime .NET đi kèm. VC++ x64 là prerequisite Microsoft riêng. NSIS cài app per-user, chỉ trình cài VC++ yêu cầu UAC khi máy thiếu. Gỡ app giữ dữ liệu.

## Chưa nằm trong V0.1

Editor chú thích dạng vector, undo/redo, video trimming, SQLite, tray, auto-update, thiết bị âm thanh tùy chọn, HDR, OCR, scrolling capture. Không thêm nút giả cho tính năng chưa có.
