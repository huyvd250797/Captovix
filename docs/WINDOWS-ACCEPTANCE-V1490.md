# Nghiệm thu Windows V1.49.0

## Đo bộ nhớ A/B

Dùng cùng máy Windows, thiết lập DPI/màn hình, dữ liệu thử, phiên bản hệ điều hành và trạng thái HDR. Chạy V1.48.0 rồi V1.49.0 với cùng số ảnh trong thư viện và cùng bộ ảnh mẫu. Lặp ba lượt cho mỗi bản. Đóng cả biểu tượng khay trước mỗi lượt. Chờ Windows ổn định sau khi cài và tránh chạy ứng dụng nặng đồng thời.

Trong PowerShell tương tác, chạy công cụ từ source trước khi mở app. Dùng cùng công cụ cho cả hai phiên bản.

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Measure-Memory.ps1 -Version 1.48.0 -Output .\memory-1480.csv
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Measure-Memory.ps1 -Version 1.49.0 -Output .\memory-1490.csv
~~~

| Giai đoạn | Thao tác |
| --- | --- |
| Khởi động | Mở Captovix. Khi Editor xuất hiện, nhấn I trong terminal và chờ 30 giây. |
| Tải nặng | Nhấn C. Chụp màn hình 4K hoặc vùng trải qua hai màn hình với cùng bố trí ở cả hai bản. Mở một PNG 7680 × 4320 cố định có chữ nhỏ. Dùng cùng vùng OCR và Mockup khoảng 3840 × 2160, đóng hai cửa sổ. Thêm chú thích, xuất PNG và JPEG, chờ hoàn tất. |
| Sau tác vụ | Chuyển về cùng ảnh nhỏ trong thư viện để so khi lượng dữ liệu đang mở tương đương. Nhấn R, chờ 30 giây. |
| Lặp lại | Chạy đủ 10 vòng, ghi đỉnh Working Set và mức Private Bytes khi nghỉ từng vòng. Nhấn Q để lưu tổng hợp. |

Working Set là RAM đang cư trú của các tiến trình Captovix, gồm worker OCR trong lúc còn chạy. Giá trị cộng có thể tính lặp trang chia sẻ, vì vậy dùng cùng phương pháp cho hai phiên bản. Private Bytes dùng để quan sát phần bộ nhớ riêng giữ lại khi tác vụ đã đóng. CSV có mẫu liên tục ở khoảng 100 ms mặc định. Dùng trung vị của đỉnh giai đoạn work qua ba lượt cho mỗi bản. Phần trăm giảm tính theo (đỉnh V1.48.0 − đỉnh V1.49.0) / đỉnh V1.48.0 × 100. Mục tiêu ít nhất 20%.

Các vòng 1–2 có thể làm nóng cache. Ở vòng 3–10, theo dõi Private Bytes sau 30 giây. Đánh dấu chưa đạt nếu mức này tăng liên tiếp trong ít nhất bốn vòng hoặc trung vị ba vòng cuối cao hơn trung vị ba vòng đầu sau khi làm nóng quá 10% và 16 MiB. Ghi nhận lỗi và giữ CSV từng lượt.

## Độ nét và vòng đời ảnh

- Chụp vùng lẻ, màn hình 4K, vùng trên hai màn hình có DPI khác nhau và ảnh chứa chữ nhỏ. Dùng **Kiểm chứng pixel** để so ảnh mẫu theo từng pixel. Kiểm độ nét ở 100%.
- Xuất PNG nguyên ảnh, crop, xoay, lật, có alpha, có chú thích và vùng che. Kiểm kích thước pixel và so với V1.48.0 trên cùng dự án. JPEG giữ thiết lập chất lượng 95.
- Lặp OCR, Mockup và xuất rồi đóng hoặc hủy trong lúc xử lý. Kiểm ảnh gốc, tệp đích, thư viện và khả năng mở lại cửa sổ.
- Undo/Redo và hợp nhất ảnh thông thường vẫn hoạt động. Mục quá lớn vượt ngân sách lịch sử có thể bị loại. Tài liệu hiện hành và ảnh gốc vẫn phải mở đúng.

## Logo và cài đặt

- Trên Windows 10/11, kiểm taskbar với Captovix đang chạy ở cả trạng thái ghim và chưa ghim. Đổi riêng màu hình và nền, thử gradient và khôi phục mặc định. Kiểm cửa sổ, khay, taskbar, khi chuyển DPI và sau khi mở lại app. Ghi riêng trường hợp Explorer vẫn hiển thị icon đã lưu đệm.
- Cài mới và cài đè V1.48.0. Kiểm Captovix mở được, thư viện/KEY còn nguyên, Recovery Center chạy riêng, rồi kiểm nâng cấp với feed staging. Chỉ dùng kênh stable sau khi các mục bắt buộc đạt.
