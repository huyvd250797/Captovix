# Nghiệm thu Windows V1.54.0

Chạy lại ca V1.52.0 trong WINDOWS-ACCEPTANCE-V1520.md và các ca dưới đây. Ghi thiết bị, bản Windows, cấu hình đã chọn, kết quả thực tế và MP4 mẫu.

| Mục | Ca kiểm |
| --- | --- |
| RecordingPresets | Quay vùng 4K, vùng nhỏ lẻ pixel, màn dọc, cửa sổ và vùng qua hai màn hình khác DPI. Thử từng preset và tùy chỉnh 24 FPS. Đối chiếu kích thước, tỷ lệ và FPS trên MP4. So dung lượng ước tính với MP4 và WAV. |
| RecordingPresets | Chọn ổ gần đầy và thử encoder không hỗ trợ cấu hình. Kiểm cảnh báo rõ ràng, chọn lại encoder phần mềm hoặc hạ cấu hình rồi thử lại. File chưa xác minh không được thêm vào thư viện. |
| RecordingPresenter | Ctrl+Shift+2 mở quay, 3 dừng, 4 pause/resume, 5 thêm mốc. Click trước pause, trong pause, sau resume. Mở danh sách Mốc, kiểm vị trí phát và tọa độ/thời điểm trong tệp cues. Click lúc pause không xuất hiện. Thử xung đột phím marker, nút Đánh dấu vẫn hoạt động. |
| RecordingPresenter | Bật/tắt làm nổi click, kiểm vị trí hiển thị trong bản quay toàn màn hình, vùng và cửa sổ. Thử sao lưu/nhập, xóa/khôi phục video có click và WAV. |
| RecordingModalClick | Rê vào modal, đợi viền xuất hiện, click một lần rồi đối chiếu vùng 3–2–1 với MP4. Rê rồi kéo quá ngưỡng để chọn thủ công. Thử Esc trong đếm ngược và xác nhận ảnh chụp giữ hành vi cũ. |
| LiveAudioMeters | Chỉ mic, chỉ loopback, hai nguồn, tắt cả hai. Nói và phát nhạc độc lập. Tắt, không tín hiệu, tạm dừng và mất thiết bị phải phân biệt được. Đóng combo và xác nhận tên thiết bị trùng mục đã chọn. |
| VideoPlaybackToggle | Phát → tạm dừng → tiếp tục đúng vị trí. Tua khi dừng, hết video, đổi video và mở nguồn WAV riêng. Biểu tượng phản ánh đúng trạng thái. |
| InlineStepEditing | Tạo Bước rồi nhấp đúp. Enter lưu số 1–9999. Esc hoàn tác. Thử giá trị không hợp lệ, zoom, xoay, Undo/Redo, mở lại ảnh và kiểm kiểu trình bày. |

Lưu kết quả tương ứng vào `artifacts/windows-acceptance.json` trước khi phát hành stable.
