# V1.9 — Hướng dẫn và nghiệm thu Windows

Các bộ test ở đây chưa được thực thi trên Windows tại nơi build. Không đánh dấu đạt trước khi chạy thực tế và giữ log.

## Kịch bản bắt buộc

| Nhóm | Thao tác | Kết quả cần đạt |
|---|---|---|
| Khởi động | Cài từ V1.8.1, mở app, chuyển ảnh nhiều lần | Editor phóng to, không lỗi Style_Changed/SyncProperties hoặc UnloadEditor |
| Phím dấu | Đặt Theo cấu hình = Ctrl + `; lưu; focus Notepad/browser rồi nhấn | Chỉ một phiên chụp; có thể hủy Esc và dùng tiếp |
| Chạy nền | Bật đóng xuống khay, đóng Editor rồi nhấn Ctrl + ` | Chụp theo cấu hình, mở Editor hoặc thông báo theo tùy chọn |
| Xung đột | Đổi sang phím đã có app khác đăng ký | Báo xung đột; phím trước vẫn hoạt động |
| Bàn phím | Thử Ctrl + `, Alt + dấu, F1, PrintScreen; đổi layout | OEM_3 không bị biến thành F sai; kiểm tra vị trí thực tế theo layout |
| Modal / grid | Mở modal của web/app native, rê vào vùng trống rồi vào grid/chữ | Khung thay đổi theo accessibility hierarchy có sẵn; không hứa nhận mọi div |
| Cấp cha con | Dừng chuột trên grid, cuộn lên đến modal, xuống đến cell/chữ | Khung đúng cấp; click được nuốt, app đích không thực hiện click |
| Fallback | App vẽ canvas không có cây UIA hoặc provider không trả lời | Khung native hoặc kéo vùng thủ công vẫn dùng được, Esc hủy được |
| Hủy / cleanup | Chuột phải, Esc, hủy khi đang dò; thử chụp lại ngay | Overlay/hook/tiến trình dò cũ kết thúc; không chiếm chuột sau khi hủy |
| Nhiều màn hình | Modal qua màn hình có tọa độ âm và DPI khác; scale 100/125/150/175/200% | Highlight/capture cùng pixel vật lý; ảnh không có overlay |
| Pixel | Kéo thủ công vùng 1467×386; PNG/copy/zoom khác nhau | Đúng pixel, zoom không đổi file xuất; kiểm chứng SDR theo bộ cũ |
| Arrow | Vẽ thẳng/gấp khúc/cong; đổi đầu/hai đầu/nét đứt/màu | Hit và xuất khớp đường đang thấy |
| Tay nắm | V chọn, kéo hai đầu và tay nắm thứ ba | Chỉnh control được; gấp khúc thay trục ngang; Ctrl+Z/Y trọn thao tác |
| Bám shape | Kéo đầu từ rectangle/ellipse/triangle; move/resize/rotate shape | Đầu nối theo biên; Ctrl tạm tắt snap canvas như cũ |
| Kết nối lại | Kéo một đầu sang shape khác, hoặc thả ra chỗ trống | Đầu còn lại giữ liên kết, đầu kéo đổi liên kết hoặc tháo |
| Nhân bản / xóa | Nhân bản cả mạng, nhân bản riêng đường, xóa target, Undo | ID liên kết đúng bản sao; xóa target giữ điểm cuối; Undo khôi phục |
| Menu chuột phải | Click phải trên shape rồi sắp xếp lớp; thử nhiều lựa chọn | Chọn đúng shape; giữ nhiều lựa chọn khi click trong nhóm; xử lý có Undo |
| Lớp bảo vệ | Đưa arrow lên trước cạnh vùng blur/cover | Vùng che vẫn phủ trên chú thích; không làm lộ thông tin |
| Lưu / nâng schema | Mở sidecar schema 6, sửa và đóng/mở lại | Tạo pre-v1.9.bak trước khi ghi schema 7; originals không đổi |
| Setup / update | Cài D:\Captovix\Captovix và update từ bản trước | Giữ root, cấu hình và dữ liệu; phiên bản hiển thị V1.9.0 |

## Thiết kế và phạm vi

Connector dùng ID của target cùng vị trí chuẩn hóa trên biên. Mỗi lần commit giải quyết lại endpoint trước khi ghi snapshot Undo. Không tạo liên kết connector-to-connector, không tìm đường tránh chướng ngại, không thêm/xóa đỉnh tùy ý. Mũi tên cong dùng quadratic Bezier với một control; gấp khúc dùng trục dọc giữa và hai góc. Có thể đổi các endpoint sau vẽ. Kéo cả đường sẽ tháo liên kết ra ngoài selection; group có cả target giữ liên kết trong nhóm. Control đã chỉnh thủ công nằm tại tọa độ ảnh, control mặc định tự tính từ endpoints.

Element probe chỉ đọc UIA và bounds, không Invoke/SetFocus/đọc DOM hay gọi API riêng website. UIA có thể trả text/cell hoặc không cung cấp container; con lăn cho chọn cấp có sẵn. Vùng capture là bounding rectangle đang hiển thị, không chụp nội dung ngoài viewport hay phần bị cửa sổ khác che. Mỗi request UIA timeout 900 ms trong tiến trình riêng; giới hạn duyệt cây 300 nodes/24 levels. Khi đổi mục tiêu nhanh, bỏ kết quả của vị trí cũ. Tọa độ từ UIA là pixel vật lý, được clip vào cửa sổ/desktop. Chọn vùng thủ công luôn khả dụng qua giữ-kéo.

Tài liệu Microsoft đã tham khảo:
- https://learn.microsoft.com/en-us/dotnet/api/system.windows.automation.automationelement.frompoint
- https://learn.microsoft.com/en-us/dotnet/framework/ui-automation/ui-automation-threading-issues
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
- https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
