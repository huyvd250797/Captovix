# Nghiệm thu Windows V1.50.0

Chạy trên máy Windows thật hoặc máy ảo Windows có thiết bị hiển thị phù hợp. Dùng tài khoản và thư viện thử để thử cài đè, cập nhật hoặc tạo lỗi. Ghi phiên bản Windows, CPU/GPU/RAM, số màn hình, DPI, cấu hình HDR và mã băm SHA-256 của bộ cài.

| Mục | Kịch bản và điều kiện đạt |
| --- | --- |
| FreshInstall | Cài mới Windows x64 bằng Full Setup. Mở đúng V1.50.0, mở Recovery Center độc lập, thoát cả khay và mở lại. |
| OverwriteInstall | Cài V1.49.0, tạo thư viện gồm ảnh, video, bản chỉnh sửa và KEY hợp lệ. Cài đè đúng thư mục. Mở lại các mục, bản chỉnh sửa và tính năng được cấp phép. |
| InAppUpdate | Nâng từ V1.49.0 qua kênh thử dùng đủ năm tài sản release. Kiểm tải, xác minh checksum, tạo backup và khởi động lại. Thử hủy và gói sai hash. |
| DataIntegrity | Kiểm hash thư viện, ảnh/video nguồn, bản chỉnh sửa và tệp cấp phép trên cùng tài khoản. Mở thử bản chỉnh sửa và xác nhận KEY vẫn sử dụng được. |
| StartupRecovery | Trong máy thử, tạo lỗi khởi động trên bản cài có thể khôi phục. Xác nhận app thoát hoặc hết thời gian khi Editor chưa mở, và chỉ báo Đã khôi phục khi Editor hiển thị thật. Thử Mở bản hiện tại và Cập nhật & mở lại. |
| MixedDpiCapture | Dùng hai màn hình 100%/150% hoặc 125%/200%, một màn hình tọa độ âm. Chụp toàn màn hình, kéo vùng qua hai màn hình, rê modal, hủy và thử lại. Đối chiếu kích thước pixel, khung vùng và vị trí ảnh. |
| LongSession | Chạy ít nhất hai giờ và 10 vòng với PNG 8K, chụp 4K, OCR, Mockup, Undo/Redo, xuất PNG/JPEG, video, tìm kiếm thư viện và chuyển màn hình. Ghi đỉnh RAM và RAM khi nghỉ theo tài liệu V1.49.0. Kiểm ứng dụng vẫn thao tác và thoát được. |
| Latency | Dùng cùng máy và ảnh ở hai bản. Đo từ phím chụp đến overlay, từ kéo đến khung hình đổi, từ giữ chuột trên modal đến khung nhận diện, rồi đo mở ảnh, zoom, kéo đối tượng và xuất. Báo trung vị và p95, tối thiểu 20 lượt mỗi thao tác. Điều tra nếu p95 chậm thêm đồng thời trên 20% và 16 ms. |
| Appearance | Đổi màu nhấn, nền logo và hình logo. Xem trước trên thanh tiêu đề, Alt+Tab và khay. Hủy, đóng bằng X, lưu rồi sửa tiếp và hủy. Mở lại kiểm màu đã lưu. Lặp với app ghim/chưa ghim và sau khi chuyển DPI. Ghi riêng nếu Explorer giữ hình ghim cũ. |

## Đo độ trễ

Đo bên ngoài bằng cùng cách ở hai phiên bản để so được mốc. Khi cần phân tích V1.50.0, thoát cả khay rồi mở app từ PowerShell có biến môi trường sau.

~~~powershell
$env:CAPTOVIX_MEASURE_PERFORMANCE = '1'
Start-Process 'C:\duong-dan-cai-dat\current\Captovix.exe'
~~~

Thoát app bình thường để ghi `%LOCALAPPDATA%\HuyVo\Captovix\performance-latest.json`. File giữ tối đa 512 mẫu gần nhất cho mỗi phép đo. `capture.drag.frame` là từ khi khung kéo đổi đến lượt render kế tiếp. `capture.element.hover-frame` tính từ khi con trỏ ổn định đến lượt render nhận diện hợp lệ. `capture.photo.saved` tính từ lúc bắt đầu ghi ảnh tới khi thêm vào thư viện, sau thời gian đếm ngược. Đây là số liệu nội bộ ứng dụng. Dùng video quay thao tác hoặc công cụ đo đầu vào và khung hình để đối chiếu hai phiên bản.

## Kiểm dữ liệu qua cài đè, cập nhật và khôi phục

Ở cùng tài khoản Windows, tạo thư viện thử có bản chỉnh sửa đã lưu và KEY đã kích hoạt. Đóng cả khay trước mỗi lệnh. Chụp baseline, thực hiện đúng một kịch bản nâng cấp, rồi so sánh trước khi tiếp tục sửa dữ liệu.

~~~powershell
.\scripts\Test-UpgradeData.ps1 -Manifest .\upgrade-before.json
.\scripts\Test-UpgradeData.ps1 -Manifest .\upgrade-before.json -Compare
~~~

Chạy phép so trong khoảng KEY vẫn hợp lệ và tránh thao tác làm mới chứng thư trong lúc so. Nếu chứng thư được làm mới, ghi lại sự kiện và kiểm lại trong phiên có thể tái lập. Đối chiếu thêm bằng cách mở ảnh, mở bản chỉnh sửa và xác nhận chức năng có bản quyền hoạt động. Script chỉ ghi hash, ID và nhóm dữ liệu.

## Bằng chứng phát hành

Lưu log cài đặt, kết quả so dữ liệu, số đo và báo cáo từng kịch bản. Tệp `artifacts/windows-acceptance.json` cần có `version` là `1.50.0`, `platform` là `Windows`, `windowVisible` là `true`, `manualAcceptance` là `PASS` và đối tượng `checks` có đủ chín mục trong bảng, mỗi mục là `PASS`. Windows Sandbox smoke chỉ kiểm cài mới và cửa sổ, để các mục khác ở trạng thái chờ.
