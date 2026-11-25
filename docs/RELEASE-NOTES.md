# Captovix V1.69.0 — Multi-Pointer Callout & Dynamic Version

- Tiêu đề MainWindow lấy VersionInfo.Current từ assembly, không gán cứng.
- Callout ảnh: Số mũi chỉ từ 1 (mặc định) đến 8, mỗi mũi là hình nhô liền thân, có điểm điều khiển cam riêng.
- Bảo toàn mũi chỉ khi Undo/Redo, sao chép, kéo thân, xoay, chỉnh kích thước, lưu/mở lại và xuất ảnh. Kiểu không mũi chỉ không hiển thị các mũi.
- Giữ bản vá Retention V1.68.2, chất lượng ghi 1080p, license, kênh update và dữ liệu. Không migration backend.
- Callout video giữ một mũi như cũ; không sửa video. Mũi mới có thể chồng/nhập nét khi cùng hướng. Tài liệu mới mở trên bản cũ chỉ còn mũi đầu; sao lưu trước rollback.
- Build Update Candidate có kiểm thử tập trung, sửa NSIS include/working directory. Full regression vẫn giữ riêng. Chưa biên dịch hoặc chạy WPF trong môi trường này.

Xem docs/V1.69.0-VALIDATION.md.

# Captovix V1.68.2 — Retention Startup Stability Fix

- P0: sửa lỗi khởi động khi lịch tự dọn thư viện đã quá hạn. Tính thời gian chờ từ một mốc `DateTime.Now`, kiểm tra trước khi gán `DispatcherTimer.Interval`; lịch quá hạn hoặc trùng thời điểm hiện tại chờ 15 giây.
- Giữ nguyên ngày hết hạn của lịch bị lỡ, cơ chế dọn bù, lịch 23:59, trì hoãn khi app bận, quy tắc bảo vệ tệp liên kết/bản sao lưu và Thùng rác Windows.
- Cô lập lỗi khởi tạo/lập lịch/tick Retention; ghi log lỗi và thông báo trạng thái, không để lỗi lập lịch làm thất bại khởi động. Không tự ghi đè cấu hình bị lỗi.
- Đồng bộ phiên bản app, Recovery Center, installer và hướng dẫn phát hành; bổ sung kiểm tra RetentionStartup vào cổng publish stable.
- Giữ nguyên chức năng quay/chụp/xuất video V1.68.1, dữ liệu, license và kênh update. Không migration.

## Xác minh và giới hạn

Đã thêm 10 kiểm tra C# hồi quy, trong đó có thời điểm lỗi 25/11/2026 08:19:56, lịch trùng 23:59, +/- 1 tick, dọn bù một lần và thay đổi giờ. Chưa chạy .NET test, WPF, publish hoặc kiểm gói Velopack trong môi trường Linux thiếu SDK và kết nối mạng. Xem `docs/V1.68.2-VALIDATION.md` để nghiệm thu trên Windows.

Build trên Windows: chạy `Build.cmd` hoặc `scripts/Build-Windows.ps1` với .NET 10 SDK, Python, NSIS và mạng. Chỉ cài đè bằng Setup đã build và kiểm thành công; không gỡ app hay xóa dữ liệu.

## Lịch sử V1.68.1

# Captovix V1.68.1 — 1080p Recording Sharpness Fix

- Khôi phục bộ ghi H.264 High, bitrate mode **Quality**, Quality70 như luồng V1.67. V1.68 đã đổi sang CBR với bitrate preset: cùng1080p nhưng bitrate bị giới hạn có thể mất chi tiết chữ. Đây là thay đổi cấu hình xác nhận được qua source; chưa có hai MP4 gốc để đo nguyên nhân trên máy Boss. Dung lượng ghi lại biến thiên theo nội dung, ước lượng bitrate trong preset chỉ mang tính gần đúng.
- Bỏ bước tự gắn nhãn sRGB/range cho video sau khi quay của V1.68. Bỏ ép nhãn màu trong lệnh chuẩn hóa xuất. Màu của preview không được dùng để sửa nội dung/metadata MP4. Không tự retag các video cũ; video đã mất chi tiết không thể phục hồi bằng thay nhãn màu.
- Khung tạm dừng dùng FFmpeg giải mã PNG trực tiếp, giữ1080p gốc, giới hạn tối đa4K thay cho1280×720 rồi phóng lớn. Thumbnail thư viện vẫn dùng kích thước nhỏ riêng. Khi không có FFmpeg, fallback bộ đọc Windows. Xem video đang phát vẫn dùng Windows MediaElement nên khác biệt màu do decoder/ICC/HDR cần kiểm thực tế.
- Nếu xuất toàn bộ nguồn mà không thay đổi nội dung (kể cả Split nhưng giữ liên tục, nguyên thứ tự và không hiệu ứng/điều chỉnh âm thanh), sao chép đúng byte MP4: không tái nén video hay tiếng.
- Timeline cần render vẫn cắt chính xác, nhưng tăng chất lượng từ CRF17 lên14 và bỏ scale/chroma resample cho canvas trùng kích thước nguồn. Crop/rotate/zoom/canvas khác vẫn xử lý đúng. Không dùng sharpen hay tăng saturation để che lỗi.
- Giữ con trỏ video riêng, HUD/icon, nút xuất và mọi dữ liệu/license. Không cần migration mới. Chọn preset1080p rồi quay lại để đánh giá sửa bộ ghi; file đã quay mờ không được tự biến thành bản sắc nét.

## Xác minh

20 kiểm tra media/export qua trên Linux: giữ byte nguồn chính xác, Split liên tục, loại trừ trim/reorder/motion khỏi copy, không bỏ crop, fixture nét UI1920×1080, PNG preview1920×1080, âm thanh, màu và nhiều điểm cắt. Fixture render có MAE luma dưới1.5 mức trên255; không phải đo bản quay thực tế Windows. CaptureWorkflowChecks160 qua. Publish Windows x64 và kiểm gói update trước bàn giao.

Nghiệm thu Windows còn chờ: cùng màn hình/app mẫu/preset1080p, quay V1.67/V1.68/V1.68.1, so frame gốc và file xuất ở100% pixel, kiểm bitrate/detail/âm thanh, paused preview vs playback, HDR/ICC nếu có. Không phóng ảnh720p rồi dùng nó để đánh giá file1080p. Video cũ do CBR mất nét cần quay lại; không áp bộ tăng nét tự động.

Build: chạy Build.cmd; bộ build tải FFmpeg/ffprobe theo checksum. Gói update Windows có engine/runtime: giải nén, thoát app cả khay hệ thống, chạy HuyVo.Captovix-win-Setup.exe. Gói chưa được ký số.

