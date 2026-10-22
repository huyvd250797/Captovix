# Captovix V1.8.1 — Startup Recovery

Sửa lỗi khởi động TargetInvocationException / NullReferenceException tại MainWindow.SyncProperties do Style_Changed chạy khi XAML chưa dựng xong. Thêm cổng sẵn sàng cho callback Editor và chặn đồng bộ lồng nhau. Giữ nguyên Image Editor V1.8, dữ liệu schema 6, đường dẫn cài và nguồn cập nhật mặc định.

App không mở được: thoát tiến trình Captovix, chạy Captovix-v1.8.1-Setup.exe và giữ đường dẫn cài hiện tại, ví dụ D:\Captovix\Captovix. Không cần gỡ ứng dụng hoặc xóa thư mục dữ liệu. Log: %LOCALAPPDATA%\HuyVo\Captovix\errors.log.

Kiểm tra logic/build trên Linux; cross-build WPF. Chưa chạy ứng dụng, cài đặt hoặc cập nhật trên Windows thật. V1.9 kế tiếp dự kiến: mũi tên cong/gấp khúc, connector bám shape và điều khiển điểm.
