# Nghiệm thu Windows V1.52.0

Thử trên Windows x64 với ít nhất một micro và hai thiết bị phát. Ghi phiên bản Windows, tên thiết bị, FPS và kết quả từng ca. Tiếp tục kiểm hồi quy V1.51.0 theo WINDOWS-ACCEPTANCE-V1510.md.

| Ca | Thực hiện | Kết quả mong đợi |
| --- | --- | --- |
| Chọn nguồn và đo mức âm | Chọn micro USB và tai nghe USB. Nói vào micro, phát tiếng ra đúng tai nghe rồi đổi sang loa | Hai thanh lên mức độc lập. Thiết bị được chọn đúng khi mở lại hộp quay. Tắt nguồn thì thanh về 0. |
| Bốn chế độ thu | Thử im lặng, chỉ micro, chỉ tiếng máy và cả hai | MP4 mở được. Chế độ im lặng không có WAV. Mỗi nguồn bật có WAV riêng và nút nghe trong thư viện. |
| Hai nguồn độc lập | Phát âm mẫu ra tai nghe và nói vào micro. Dừng, nghe MP4 rồi nghe riêng từng WAV | MP4 phát bản trộn. Tiếng hệ thống và lời nói nghe riêng được. Dùng tai nghe để tránh lọt tiếng loa vào micro. |
| Pause/resume | Tạo tín hiệu hình/âm trước và sau hai lần tạm dừng. Tạo thêm tín hiệu trong lúc tạm dừng | Bản quay và hai WAV bỏ phần tạm dừng. Các mốc sau tiếp tục khớp nhau. |
| Đồng bộ 30 phút | Tạo mốc hình/âm ở đầu, cuối và sau tạm dừng. Đối chiếu MP4 với từng WAV. Dùng thiết bị tạo mốc chung hoặc ghi rõ trễ của thiết bị thử | Báo số đo thực tế theo ms. Mục tiêu nghiệm thu đề xuất là lệch tuyệt đối tối đa 100 ms và chênh lệch giữa đầu/cuối tối đa 50 ms. |
| Ngắt thiết bị | Rút lần lượt micro USB hoặc thiết bị phát giữa hai phiên. Thử mất trước khi bắt đầu và sau khi quay | Báo đúng tên nguồn. Giữ phần đã ghi và tiếp tục nguồn còn lại khi backend cho phép. Nếu bộ quay dừng, kiểm tệp còn lại và lỗi hiển thị. |
| Lớp phủ và modal | Quay một màn hình phụ, vùng qua hai màn hình và một modal. Thử Esc ở giây 2 | Lớp tối nằm tại nguồn quay, số trắng đếm 3–2–1 và lớp phủ rời trước khung đầu tiên. Chọn vùng hỗ trợ khóa gợi ý modal, chỉnh cạnh và Enter như chụp ảnh. |
| Nút và thư viện | Kiểm icon Pause, Tiếp tục, Dừng đỏ có ô vuông. Xóa, khôi phục, sao lưu và nhập thử một bản quay có hai nguồn | Trạng thái nút đúng. WAV đi cùng mục khi xóa/khôi phục và khi sao lưu/nhập. |
| Nâng cấp | Cài đè từ V1.51.0 và mở video cũ, cấu hình cũ và bản sao lưu cũ | Video cũ phát như trước. Chỉ hiện nút nghe riêng nếu có tệp WAV đi kèm. |

Ghi AudioSources, AudioTrackPlayback, AudioSync, AudioDeviceLoss và RecordingCountdown vào đối tượng checks trong artifacts/windows-acceptance.json sau khi thử trên Windows. Kiểm thử tính toán timestamp trên Linux chỉ xác minh cách ánh xạ, bao gồm pause/resume và khoảng lặng.
