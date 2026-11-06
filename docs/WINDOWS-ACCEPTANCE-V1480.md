# Nghiệm thu Windows V1.48.0

Dùng cùng máy Windows, ổ lưu trữ, DPI, tần số màn hình và một bản sao thư viện cho V1.47.0 và V1.48.0. Giữ cùng ảnh 1080p, 4K, ảnh nhiều chú thích và ảnh có lớp làm mờ. Dùng cùng thiết lập xuất, cùng thư mục đích, cùng chuỗi thao tác. Thử mỗi phiên bản ở lượt mở đầu và lượt đã có cache. Đo ít nhất 20 lần cho từng thao tác để so trung vị và p95.

| Kịch bản | Đo từ lúc thao tác đến khi | Kiểm kết quả |
| --- | --- | --- |
| Chuyển nhanh A → B → C, gồm ảnh A đã sửa | Ảnh C và thông tin ảnh C xuất hiện | Không hiển thị lại B. Mở lại A và kiểm chỉnh sửa đã lưu. |
| Ctrl + lăn, kéo đối tượng, nhập chữ có IME | Khung ảnh hoặc đối tượng phản hồi | Zoom bám điểm neo, thả chuột đúng vị trí, Ctrl+Enter lưu chữ cuối, Esc hủy đúng. |
| Cuộn khi thumbnail đang tải | Thư viện bắt đầu di chuyển và hiện mục mới | Không thay nhầm thumbnail sau đổi thư mục hoặc dọn cache. |
| Xuất PNG/JPEG ảnh 4K trong lúc cuộn, zoom và kéo | Giao diện phản hồi và tệp đích hoàn tất | Ảnh xuất phản ánh nội dung lúc bắt đầu. So kích thước và pixel với ảnh tham chiếu. |
| Ctrl+S trong khi xuất, sau đó mở ảnh khác | Chỉnh sửa được lưu và ảnh mới hiện | Không ghi đè bản chỉnh sửa mới. Thử thêm lỗi đĩa hoặc thư mục chỉ đọc. |

Dùng cùng một phương pháp đo ngoài app cho cả hai phiên bản, chẳng hạn quay màn hình cùng tần số rồi đếm số khung hình giữa thao tác và phản hồi. Không dùng thời gian ghi tệp để thay cho thời gian phản hồi giao diện.

Có thể chạy thử WPF tổng hợp từ hai source tương ứng bằng `dotnet run --project tests/PerformanceWindowsChecks -c Release -p:Platform=x64`. So mục **30 WPF zoom frames,20k-char callout** vì hai bản dùng cùng fixture. V1.48.0 thêm bài kiểm tra hàng đợi xuất và lưu chạy tách riêng. Báo cáo trong app V1.48.0 cung cấp mẫu thời gian nội bộ để đối chiếu chi tiết.

Cài mới, cài đè và cập nhật thử trước khi phát hành. Chỉ ghi manualAcceptance=PASS khi đã kiểm trên Windows và lưu số đo A/B.
