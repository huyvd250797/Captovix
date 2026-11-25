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

