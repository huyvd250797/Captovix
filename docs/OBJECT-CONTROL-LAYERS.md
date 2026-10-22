# V1.8 — Object Control & Layers

## Thao tác

1. Mở/chụp ảnh. Layers ở inspector bên phải, thứ tự trên là phía trước. Chọn dòng để lấy đối tượng bị che; Ctrl/Shift chọn nhiều. Đối tượng cùng nhóm được chọn/khóa/ẩn như một đơn vị.
2. Tên layer: chọn một dòng, nhập tên (tối đa 80 ký tự) rồi Enter/Đổi tên. Tên để trống dùng nhãn tự động theo công cụ. Đổi tên là metadata, có thể làm khi khóa/ẩn.
3. Khóa: nút khóa dòng; vẫn có trong file xuất, không chỉnh trên canvas, nudge, delete, style, transform, group hay order. Nút mở khóa tại layer cho phép sửa lại. Layer ẩn cũng không sửa trên canvas; bật hiện trước khi chỉnh.
4. Ẩn: nút hiện/ẩn dòng; loại khỏi preview và cả PNG/Copy, kể cả vùng che. Thông tin này được nhắc ngay trong panel. Chỉ xuất khi đúng trạng thái muốn chia sẻ; Cover/blur/pixelate vẫn ở lớp bảo vệ trên các chú thích thường.
5. Vị trí/kích thước/góc: X/Y/W/H theo pixel ảnh gốc. Một hình dùng khung local trước xoay và góc tuyệt đối; nhóm dùng khung bao world hiện tại và góc xoay thêm (reset về 0 sau áp dụng). Nhóm có thành viên xoay giữ tỷ lệ để tránh shear. Chữ/số đổi kích thước qua font, giữ tỷ lệ, font 8–240 px; cỡ hiện ra sau áp dụng là giá trị thực tế.
6. Nhập số rồi Áp dụng/Enter. Esc đồng bộ lại trường. Các giá trị chưa Áp dụng sẽ bị bỏ khi đổi lựa chọn. Sai định dạng/NaN/vượt giới hạn không được commit; đọc lỗi ngay dưới form.
7. Chọn (V), kéo để di chuyển. Tay nắm góc/biên đổi kích thước, đường/mũi tên có tay nắm đầu mút; vòng tròn nối khung để xoay, Shift theo nấc 15°. Khung sát mép có thể bị viewport cắt tay nắm; dùng ô góc để thao tác trong trường hợp đó. Group nhiều thành viên xoay resize đồng tỷ lệ.
8. Bật Bắt dính mép/tâm: snap theo ảnh/crop hoặc peer đang hiện, kể cả peer khóa. Guide hồng chỉ là lớp tương tác; Ctrl trong lúc kéo tạm bỏ snap, Shift khóa trục. Snap toggle lưu settings.json, không ghi vào ảnh.
9. Ctrl+D nhân bản, Ctrl+G nhóm, Ctrl+Shift+G bỏ nhóm; công cụ thứ tự/căn/chia đều kế thừa. Lớp bảo vệ chỉ thay thứ tự trong tier của nó. Ctrl+Z/Y hoàn tác/làm lại một gesture trọn vẹn. Chuyển ảnh/đóng mở giữ tên, khóa, ẩn, góc, nhóm và style.

## Hình học và giữ ảnh

Rotation lưu góc quanh tâm local bounds. Chữ lấy bounds từ layout font, callout tính cả đuôi. Vẽ, hit test inverse, bounds world, tay nắm và inline text sử dụng cùng tâm. Resize hình đã xoay hiệu chỉnh tâm mới để giữ điểm neo; group rotation quay tâm từng thành viên quanh pivot chung và cộng góc. Không áp dụng nonuniform scale gây shear lên group có thành viên xoay.

Vùng che/blur/pixelate xoay được dựng bằng geometry clip ở đích: filter ROI world của ảnh gốc, clip theo hình đã xoay. Không xoay bitmap chứa nội dung cũ vào một vị trí khác. Cover giữ màu opaque; mask draft che kín. Lớp ẩn bị loại, lớp khóa vẫn render. Pixels nền không chỉnh dùng nhánh exact BGRA V1.6, ảnh gốc không ghi đè; zoom/handles/guides không tham gia export. Arbitrary-angle annotation dùng antialias vector; không tuyên bố ảnh gốc được nâng chi tiết.

## Dữ liệu và hạ cấp

Schema 6 thêm Name (<=80, không control chars), Locked=false, Visible=true, Rotation=0. Đọc 1–5 giữ nguyên style trước đây. Copy/gesture/history/duplicate giữ metadata. Khi ghi đè sidecar schema <6 lần đầu, giữ `.pre-v1.8.bak`; nếu không backup được thì không ghi. Backup `.pre-v1.7.bak` đã có được giữ. Schema >6 không bị ghi đè. Nếu hạ cấp V1.7 hoặc cũ hơn, thoát app và khôi phục đúng backup tương ứng; không dùng trực tiếp file schema 6 trên app cũ.

## Nghiệm thu Windows — chưa thực thi tại nơi build

- Layers: chọn xuyên lớp, Ctrl/Shift, rename, lock/hide/unlock/show, nhóm và thứ tự tier; Undo/Redo/mở lại.
- Frame: X/Y/W/H/góc số, giữ tỷ lệ, lỗi NaN/quá giới hạn; một bước Undo; text/font và callout nhỏ.
- Canvas: drag/resize/rotate ở 100/125/150/175/200% DPI, nhiều zoom; nguồn đã crop/rotate/flip/resize; chuyển monitor âm. Đầu mút, điểm neo và Shift 15° không lệch.
- Snap crop/peer, Ctrl bypass, axis lock, toggle persistence; không có guide/handles trong file.
- Render: ẩn không xuất, khóa vẫn xuất, góc xoay giữ pixel nền; Cover/blur/pixelate lấy vùng đích đúng. Xem ObjectWindowsChecks (đã cross-build, chưa chạy).
- Chuyển 30 lượt giữa ảnh, save/reopen, backup schema5, preset/shape V1.7, capture GDI 1467×386, clipboard/PNG/JPEG.
- Installer/update giữ D:\Captovix\Captovix, URL phát hành, data/preset/library/Edits. Chạy Build-Windows.ps1 trên Windows để chạy các bộ Windows; capture/DPI thật vẫn cần probe/checklist.

Không bổ sung editor video/connector tự bẻ góc; V1.9 xử lý mũi tên và connector. HDR/wide gamut/ICC/16-bit chưa đánh giá.

Tham khảo WPF: https://learn.microsoft.com/dotnet/desktop/wpf/graphics-multimedia/transforms-overview ; https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.drawingcontext.pushclip .

Đường/mũi tên ngang/dọc có W hoặc H bằng 0, dùng tâm thật của hai đầu mút; khung số cho đổi hướng/chiều này và từ chối đoạn dài dưới 2 px. Giữ tỷ lệ tắt ở đoạn có một chiều 0. Group vẫn dùng khung bao tối thiểu 1 px cho tay nắm.
