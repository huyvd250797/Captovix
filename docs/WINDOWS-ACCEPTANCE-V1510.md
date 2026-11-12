# Nghiệm thu Windows V1.51.0

Dùng Windows 10 hoặc 11 x64. Ghi phiên bản Windows, encoder, số màn hình, tọa độ vật lý, DPI, kích thước video và kết quả mỗi ca. Dùng thư viện và KEY thử nghiệm. Kết quả biên dịch hoặc kiểm thử hình học không thay cho quay màn hình thật.

| Ca | Thực hiện | Kết quả mong đợi |
| --- | --- | --- |
| Nguồn quay | Bấm Quay và thử Vùng chọn, Cửa sổ, Toàn màn hình | Hộp chọn và thanh quay nhận đúng nguồn. Cửa sổ được chọn đúng tiêu đề. Chế độ toàn màn hình dùng đúng màn hình đã chọn. |
| Hủy trước khi quay | Bật hẹn giờ 3 giây. Hủy bằng nút Hủy quay và Esc ở hai lượt | Trở về Editor hoặc khay theo trạng thái trước phiên. Không thêm mục video. |
| Dừng và lưu | Quay vài giây, tạm dừng, tiếp tục, rồi dừng bằng thanh quay. Lặp bằng phím tắt | Thanh có tên nguồn, thời lượng và đúng trạng thái. Đang lưu tồn tại đến khi kiểm MP4 xong. Video mở được từ thư viện và mở lại sau khi khởi động app. |
| Ngắt nguồn | Đóng cửa sổ nguồn khi đang quay và khi đang tạm dừng | App báo cửa sổ nguồn đóng và dừng. Chỉ phần MP4 đã hoàn tất, được Windows mở và có hình/thời lượng mới vào thư viện. |
| MP4 lỗi | Thử bản quay quá ngắn hoặc gây lỗi ghi trong môi trường thử | App báo lỗi. Mục chưa xác minh không xuất hiện trong thư viện. Tệp pending còn lại dùng để chẩn đoán. |
| DPI và mép ảnh | Chạy màn hình 100% và 150%, một màn hình đặt bên trái. Dùng lưới màu và viền mỏng. Quay từng màn hình và vùng kéo qua đường ghép ở cả hai chiều, gồm kích thước chẵn/lẻ | Kiểm pixel ở bốn mép và đường ghép. Với vùng lẻ, đối chiếu cả phần căn khung tối đa một pixel mỗi chiều. Ghi riêng mọi sai lệch do backend native. |
| Menu nhiều cấp | Mở menu Chụp & quay → Chụp ảnh. Dùng phím tắt chụp trong khi menu đang mở, rồi Esc hoặc lưu. Lặp với menu Tệp nhiều cấp. | Không có lỗi PresentationSource. Ảnh giữ được menu đã mở tại thời điểm bấm phím. Mở lại menu và chụp tiếp được. |
| Menu chụp | Đối chiếu menu ảnh trên thanh đầu và menu khay | Có đúng Chụp mặc định, Toàn màn hình, Chụp lại vùng. Chụp cuộn vẫn dùng từ lớp chọn vùng. |
| Gradient | Chọn hai màu nền, bật gradient và thay hướng. Thử Hủy rồi Lưu. Khởi động lại | Hủy trả về màu đã lưu. Lưu giữ gradient nền độc lập với gradient hình logo. Kiểm logo ở thanh tiêu đề, khay và shortcut đã ghim. |
| Cài đặt | Cài mới, cài đè và nâng qua kênh thử từ V1.50.0 | Mở được Editor, thư viện, bản chỉnh sửa, ảnh/video nguồn và KEY còn dùng được. |

Kiểm hồi quy V1.50.0 theo WINDOWS-ACCEPTANCE-V1500.md trước khi đưa lên kênh chính thức.

Khi lập bằng chứng phát hành, giữ các mục kiểm của V1.50.0 và bổ sung RecordingSources, RecordingControls, RecordingIntegrity, RecordingMixedDpi, CaptureMenus vào đối tượng checks trong artifacts/windows-acceptance.json. Đánh dấu PASS cho từng mục sau khi đã kiểm trên Windows.
