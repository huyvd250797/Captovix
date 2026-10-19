# Checklist kiểm chứng Windows — V0.1.0

Trạng thái ban đầu: **CHƯA CHẠY TRÊN WINDOWS THẬT**. Người kiểm thử điền Đạt/Không đạt, máy, GPU, OS, DPI, thiết bị âm thanh và đường dẫn bằng chứng. Không suy ra hiệu năng video từ kết quả build.

## Chuẩn bị

- Dùng Windows x64 với desktop đang đăng nhập; cài qua Setup.exe hoặc giải nén Portable.
- Chụp thông tin máy bằng nút Báo cáo máy & màn hình trong Editor.
- Với lần đầu, thử SDR, một màn hình, 30 FPS; mở Notepad và phát một file âm thanh mẫu.
- Nếu thiếu mic/quyền, mở Windows Settings > Privacy & security > Microphone > Let desktop apps access your microphone. Không cần chạy app bằng quyền admin cho tác vụ thông thường.
- Mở thư mục nội dung để kiểm tra kích thước PNG, phát MP4 trong trình phát độc lập. Đánh dấu âm thanh thực tế, không chỉ dựa vào nhãn Mic bật.

| Mã | Trường hợp | Kết quả mong đợi | Thực tế |
|---|---|---|---|
| INS-01 | Máy chưa có .NET Desktop Runtime; cài Setup | App mở bằng runtime đi kèm, có shortcut và mục gỡ | Chưa chạy |
| INS-02 | Máy thiếu VC Runtime | Bộ cài chạy runtime Microsoft có UAC, hoặc dừng rõ ràng khi hủy/lỗi | Chưa chạy |
| INS-03 | Giải nén Portable | App mở khi đủ VC Runtime; không cần SDK | Chưa chạy |
| INS-04 | Gỡ cài đặt | Xóa app/shortcut; giữ media và cấu hình | Chưa chạy |
| CAP-01 | Vùng 800×600 trên màn chính | PNG 800×600; cạnh vùng khớp nội dung | Chưa chạy |
| CAP-02 | Kéo ngược từ phải dưới sang trái trên | Ảnh đúng vùng, không âm kích thước | Chưa chạy |
| CAP-03 | Esc/chuột phải khi chọn vùng | Hủy, trở về cùng Editor, không thêm file lịch sử | Chưa chạy |
| CAP-04 | Toàn màn hình đã chọn | PNG đúng độ phân giải vật lý màn đó | Chưa chạy |
| CAP-05 | Chọn cửa sổ Notepad | WGC chụp đúng cửa sổ, không lấy cửa sổ Editor | Chưa chạy |
| CAP-06 | Cửa sổ đóng/thu nhỏ trước capture | Thông báo lỗi rõ; không thêm bản thành công | Chưa chạy |
| DPI-01 | Màn hình 100%, 125%, 150%, 200% | Overlay đúng toàn màn; vùng PNG khớp pixel vật lý | Chưa chạy |
| DPI-02 | Hai màn hình khác scale | Chụp riêng từng màn không lệch tọa độ | Chưa chạy |
| DPI-03 | Màn phụ ở bên trái, tọa độ X âm | Chụp đúng vùng trên màn phụ | Chưa chạy |
| DPI-04 | Màn phụ ở trên, Y âm | Chọn vùng chính xác | Chưa chạy |
| DPI-05 | Kéo vùng qua hai màn hình | Các phần ghép đúng vị trí, không nhân scale hai lần | Chưa chạy |
| DPI-06 | Di chuyển Editor qua hai DPI, chọn 100% pixel | Ảnh 100% hiển thị theo pixel màn hiện tại | Chưa chạy |
| REC-01 | Quay 60 giây không âm thanh | MP4 có hình, đúng nguồn, phát được | Chưa chạy |
| REC-02 | Bật mic, tắt tiếng máy | Nghe được giọng nói; không trộn loopback điện tử | Chưa chạy |
| REC-03 | Tắt mic, bật tiếng máy | Nghe được file âm thanh đang phát | Chưa chạy |
| REC-04 | Bật mic + tiếng máy, dùng tai nghe | Hai nguồn nghe được, không lệch tăng dần sau 10 phút | Chưa chạy |
| REC-05 | Không có âm thanh phát trong 30 giây rồi phát lại | Không treo, âm thanh tiếp tục khi có tín hiệu | Chưa chạy |
| REC-06 | Pause 10 giây rồi Resume | Khoảng dừng bị loại; hình/âm không lệch sau resume | Chưa chạy |
| REC-07 | Ctrl+Shift+3 khi Editor ẩn | Dừng, chờ hoàn tất, mở video trong cùng Editor | Chưa chạy |
| REC-08 | Thanh điều khiển nằm trong vùng quay | Thanh không hiện trong video trên Windows hỗ trợ exclusion | Chưa chạy |
| REC-09 | Kích thước vùng lẻ | Xuất kích thước chẵn, không stretch; bỏ tối đa 1 px cạnh phải/dưới | Chưa chạy |
| REC-10 | Tắt GPU encoding rồi quay | Có thể quay software nếu máy đủ khả năng; lỗi thì báo | Chưa chạy |
| REC-11 | Quay 30 phút, ghi CPU/RAM/GPU, FPS thực tế | Kết thúc được, âm/hình đồng bộ; ghi số đo thực tế | Chưa chạy |
| ERR-01 | Đổi/rút thiết bị âm thanh giữa phiên | Kiểm tra hành vi, không tự kết luận hỗ trợ; lưu log nếu lỗi | Chưa chạy |
| ERR-02 | Rút màn hình đang capture | Lỗi được ghi nhận, app trở lại Editor | Chưa chạy |
| ERR-03 | Ổ đĩa đầy hoặc không ghi được | Không báo thành công, có thông tin lỗi | Chưa chạy |
| ERR-04 | Kill app giữa phiên (trên dữ liệu thử) | File pending không được liệt kê như bản hoàn chỉnh khi mở lại | Chưa chạy |
| LIB-01 | Chụp 3 ảnh, quay 2 video, mở lại app | Cùng một Editor; lịch sử/cấu hình còn đúng | Chưa chạy |
| LIB-02 | Tìm tên, đổi tên, xuất JPEG/PNG/MP4 | Kết quả/file xuất đúng; bản gốc giữ nguyên | Chưa chạy |
| LIB-03 | Xóa media có xác nhận | File và metadata được xóa thống nhất | Chưa chạy |
| LIB-04 | File bị xóa bên ngoài | App báo thiếu file và không crash | Chưa chạy |

## Ghi nhận kết quả một ca

Mã ca:
Ngày / Người kiểm thử:
OS / build Windows:
GPU / driver:
Màn hình / độ phân giải / scale:
Nguồn âm thanh:
Đạt / Không đạt:
Mô tả thực tế, độ lệch âm hình nếu có:
File mẫu / báo cáo diagnostics / recorder.log:

Chỉ nâng V1.0 sau khi các ca capture/DPI/âm thanh cốt lõi đạt trên máy mục tiêu hoặc có giới hạn đã thống nhất.
