# V1.6 — Matrix nghiệm thu Windows SDR

Trạng thái lúc bàn giao: **CHƯA CHẠY TRÊN WINDOWS THẬT**. Các cột bên dưới phải được điền bằng kết quả máy thật, không lấy unit tests DPI làm bằng chứng.

Chuẩn bị: Windows x64 interactive desktop; cập nhật app V1.6.1, VC++ runtime hoạt động, HDR tắt (xác nhận thủ công), scale Windows ổn định, nguồn SDR 8-bit. Không có ứng dụng che cửa sổ mẫu topmost. Dùng ảnh gốc có chữ nhỏ và pattern 1 px, ghi lại máy/GPU/driver/độ phân giải/cấu hình màu thực tế trong biên bản.

| Windows scale | Chụp vùng pixel mẫu | Full-screen W×H | Native-window pixel mẫu | PNG/clipboard | Report Windows |
|---|---|---|---|---|---|
| 100% | Chưa chạy | Chưa chạy | Chưa chạy | Chưa chạy | Chưa có |
| 125% | Chưa chạy | Chưa chạy | Chưa chạy | Chưa chạy | Chưa có |
| 150% | Chưa chạy | Chưa chạy | Chưa chạy | Chưa chạy | Chưa có |
| 175% | Chưa chạy | Chưa chạy | Chưa chạy | Chưa chạy | Chưa có |
| 200% | Chưa chạy | Chưa chạy | Chưa chạy | Chưa chạy | Chưa có |

Với mỗi scale, chạy nút Kiểm chứng ảnh SDR trên máy này. Không chọn đạt nếu report có lỗi hoặc bị hủy. Probe không tự đổi Windows scale; thay trong Settings rồi chạy lại khi desktop đã ổn định. Cửa sổ mẫu được di chuyển và DPI đọc thật; một lượt có nhiều monitor sẽ ghi riêng từng monitor. Bằng chứng đủ scale trên ít nhất một monitor, và các cấu hình màn hình mục tiêu được chạy riêng; không suy diễn một màn hình chứng minh mọi GPU/monitor.

Các ca bổ sung chưa chạy:

- [ ] Màn hình phụ bên trái và/hoặc phía trên, tọa độ âm: region đi qua hai monitor giữ offset/kích thước, không nhân DPI hai lần. Probe test từng monitor; vùng xuyên hai monitor phải chụp thêm từ Editor.
- [ ] Kéo chính Editor từ monitor 100% sang 150/200% rồi quay lại: Actual Pixels luôn 100%; zoom tự chọn giữ tỷ lệ vật lý; Fit tự tính lại. Output W×H và hash pixel PNG không đổi.
- [ ] Crop với X/Y lẻ; xoay 90/180/270°, lật; source có DPI file khác 96. So toàn bộ pixel theo phép hoán vị, không chỉ xem ảnh có vẻ rõ.
- [ ] Ảnh có chữ, callout, blur/pixel/cover: vùng không chỉnh giữ pixel; vùng che đúng sau crop/rotate; không xuất selection handles.
- [ ] Vary zoom 25/75/100/125/200%, export PNG cùng tài liệu: số pixel và decoded BGRA hash bằng nhau. PNG file bytes có thể khác metadata/encoder, so decoded pixels.
- [ ] Explicit resize đổi W×H đúng yêu cầu; reset resize khôi phục kích thước; không dùng pixel equality để đánh fail phép resize có chủ đích.
- [ ] Đặt canvas dư lẻ, padding, viền, bóng; kiểm tra source opaque không bị resampling khi layout không resize.
- [ ] Clipboard PNG roundtrip và dán vào app đích (Paint/Word/Teams...); phân biệt app đích tự resize/nén với Captovix. PNG giữ alpha; JPEG mất mát và ghép trắng là dự kiến.
- [ ] Chuyển ảnh qua lại (bao gồm V1.5.1 regression), restart và update giữ nội dung/edit/preset/hotkey/source URL.
- [ ] App cài tại đường dẫn đã chọn; updater lên bản thử cao hơn trong môi trường kiểm thử, không publish bản thử vào stable.

HDR/wide gamut/16-bit/ICC: chưa nghiệm thu, đánh giá riêng; không điền đạt SDR để bao phủ HDR. Report chỉ ghi HDR do người dùng xác nhận tắt, không phải đo tự động.

Tổng hợp report (PowerShell, thư mục app có file script):

```powershell
powershell -ExecutionPolicy Bypass -File .\Summarize-Fidelity.ps1
```

Hoặc chạy từ source: scripts\Summarize-Fidelity.ps1. Exit 2 báo chưa đủ scale hoặc thiếu monitor âm, không đồng nghĩa app crash. Dù đủ report, các ô manual vẫn cần nghiệm thu. Gửi report.json/renderer-results.json cùng mô tả cấu hình khi cần phân tích; không cần gửi toàn bộ thư viện ảnh.


## Hồi quy V1.6.1 — chưa thực thi

- [ ] Chụp vùng 1467×386 giữ đúng kích thước, không mất cột cuối.
- [ ] Chụp 1466×387, 1467×387, 1×1; kiểm tra cạnh phải và dưới.
- [ ] Chụp sát mép monitor, qua monitor trái/trên có tọa độ âm, khe không có màn hình màu đen.
- [ ] Mỗi mức scale 100/125/150/175/200%: probe pixel khớp, không resize.
- [ ] Bật/tắt con trỏ; hotspot đúng, cursor cắt tại cạnh vùng đúng.
- [ ] Chụp liên tiếp và hủy; không lỗi handle/ảnh dở dang vào thư viện.
- [ ] Kiểm tra quay video và chụp cửa sổ không hồi quy; GDI chỉ áp dụng ảnh desktop.
