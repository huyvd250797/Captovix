# V1.37.2 — Capture Bounds Fix

## Lỗi và nguyên nhân trong source

Log2026-11-04T09:31:05+07 xác nhận ArgumentOutOfRangeException từ FrozenFrame.Crop → FrozenDesktop.Save → CaptureAsync. Log cũ chưa có rectangle/framebounds, nên chưa chứng minh thao tác cụ thể của lần lỗi.

Đã tìm thấy các đường có thể tạo rectangle vượt frame: gợi ý cửa sổ thô từ GetWindowRect (viền cửa sổ phóng to có thể nằm ngoài desktop), move/resize/nudge chưa giới hạn, và chốt vùng không kiểm lại. Draw chỉ vẽ phần giao với từng màn hình, nên vùng nhìn thấy và tọa độ trả về có thể khác nhau.

## Sửa

- Session lưu Bounds của đúng FrozenFrame; nếu không frozen thì dùng virtual desktop vật lý. Mọi gán Current đều cắt theo Bounds.
- Gợi ý cửa sổ đầu tiên và hierarchy được giới hạn trước khi khóa. Rawwindowbounds vẫn giữ riêng để kiểm cửa sổ đã di chuyển.
- Move chuột/phím giới hạn vị trí và giữ kích thước; resize cắt theo bounds. Shift dùng anchor/capacity để giữ tỉ lệ khi gặp mép, có sai số làm tròn tối đa pixel.
- Vùng trả về khi Enter/nút chụp là cùng Current đã vẽ. Vùng clipped nhỏ hơn8×8px không được xác nhận.
- Trước lưu, CaptureAsync chặn cuối theo đúng frame và cập nhật region dùng chung cho file/dimension/metadata/repeat. Ngoài toàn bộ hoặc quá nhỏ sẽ yêu cầu chọn lại.
- Không re-capture màn hình khi xác nhận; tiếp tục cắt pixel từ FrozenDesktop đã giữ trước overlay, tránh đưa toolbar vào ảnh.
- FrozenFrame.Crop vẫn nghiêm ngặt. Nếu còn lỗi khác, exception gồm ActualValue và bounds để chẩn đoán.

## Kiểm tra đã chạy

-73 CaptureBoundsChecks đạt, có20.000 trường hợp randomclip/move; các crop hợp lệ giữ đúng pixel nguồn/kích thước.
-16 FrozenCaptureChecks,49 SmartCaptureChecks,20 QuickCaptureChecks đạt.
- Release app cross-build0warnings/0errors. Fixture CaptureBoundsWindowsChecks được cross-build đối chiếu DLL app Release (SDKLinux không xử lý GetTargetFrameworks của WPF projectreference trong lần build này). Chưa chạy fixture trên Windows.
- Kiểm package/feed/source và update gói thật được ghi ở docs/VALIDATION.md.

## Nghiệm thu Windows còn chờ

1. Cửa sổ phóng to, cửa sổ nằm một phần ngoài desktop, viền góc màn hình.
2. Chọn phần tử/cửa sổ rồi Enter nhanh trước khi UIA hoàn tất; đảm bảo không trả rawwindowbounds ngoài frame.
3. Move/resize8node tới từng mép bằng chuột/phím/Ctrl/Shift; khung/nhãn/kích thước ảnh xuất trùng nhau.
4. Màn hình gốcâm, bố trí trái/trên, DPI100/125/150/200%, nhiều màn hình khácDPI; kéo qua màn hình.
5. Chụp lại vùng cũ, fullscreen, chụp chính app; toolbar/kínhlúp không xuất hiện trong PNG.
6. Chuyển sang chụp cuộn và quay; không khẳng định đã nghiệm thu các luồng này chỉ từ kiểm logic.
7. App1.37.1 nhận1.37.2 sau khi publish đủassets, cài đè giữ thư viện/key/cấu hình.

Không cần migration mới hoặc deploybackend. Không thay bộ cài trực tuyến native1.1.0. Chưa tự publish GitHub.
