# Captovix V1.20 — Image Release & Performance

## Giao diện gọn

- Cụm caption của cửa sổ Captovix: thanh tiêu đề28DIP, nút32×26DIP, glyph11DIP/nét0.9DIP. Nút maximize đổi sang restore khi phóng to; tên trợ năng và hover X đỏ giữ. Áp dụng cho các cửa sổ app qua WindowChromeStudio; hộp thoại file/màu của Windows do hệ điều hành quản lý.
- Icon thao tác mặc định16DIP/nét1.1DIP, đầu/join tròn, hình mềm. Logo tím đã chốt vẫn giữ, không thay artwork ứng dụng.
- Mẫu gallery nhỏ64DIP, phần hình tối đa46×28DIP; giữ bounds thật để không cắt Step/callout. Mẫu yêu thích theo công cụ, tên ở tooltip, không đổi style đã lưu trên ảnh.
- Chân thanh thư viện chỉ một hàng: Xóa, Tìm, Lịch sử, … . Trong … có Khôi phục, Mở thư mục, Dọc/Ngang, Xuất nhiều, Quản lý, Thư mục lưu media và Vẽ ngoài ảnh. Menu ngang phía trên vẫn truy cập được các chức năng. Ctrl/Shift chọn nhiều, hover/focus ☆/× giữ từ1.19.

## Thư viện tổng dạng cây

Mở Tệp → Thư viện tổng hoặc icon lịch sử. Cây là **năm → đủ12tháng → đúng số ngày của tháng → danh sách ảnh/video**; không còn combo đổi grouping/bộ lọc thay cho cây. Tháng4 có30ngày, tháng1 có31ngày, tháng2 có28/29 theo năm nhuận. Ngày rỗng vẫn mở được và ghi chưa có nội dung. Năm hiện tại luôn có để thư viện rỗng không thành màn hình trắng; năm cũ xuất hiện khi có media.

Các nhánh nạp khi mở; ngày có nhiều nội dung nạp100mục đầu, Xem thêm thêm100mục. Danh sách trong ngày có viewport giới hạn/virtualization; thumbnail nạp nền. Ctrl/Shift chọn nhiều trong danh sách ngày, giữ Ctrl để gom thêm từ danh sách ngày khác đang mở. Số mục đã chọn hiện dưới; xóa/xuất áp dụng nhóm đó. Mở/Enter từ danh sách hoặc double-click ảnh mở Editor chính; phím mũi tên dùng để đi cây. Đổi/đóng nhánh không xóa file. Không tạo các năm giả chứa ảnh demo. Thư viện gần đây vẫn tối đa50mục, toàn bộ lịch sử vẫn trong metadata/file gốc.

## Vẽ bên ngoài ảnh

1. Mở ảnh. Bố cục → **Vẽ ngoài ảnh · nới canvas**, hoặc … ở chân thư viện → cùng chức năng, hoặc Ctrl+Alt+E.
2. Canvas thêm160px ở mỗi cạnh và căn ảnh nguồn vào giữa. Kích thước xuất hiển thị tăng rõ ràng; nguồn không resize/ghi đè. Có thể bấm thêm lần nữa nếu cần nhiều chỗ. Giới hạn32.768px/chiều và100MP đầu ra vẫn áp dụng.
3. Chọn Khung/Callout/Chữ/Mũi tên/Vẽ tự do/Bước và vẽ vào vùng nền ngoài ảnh. Kéo đối tượng hoặc đuôi callout vào đúng vị trí; đầu đuôi có thể chỉ vào ảnh gốc. Di chuyển, resize, sửa chữ/thuộc tính, Copy ảnh đã chỉnh/export/project giữ phần ngoài ảnh.
4. Ctrl+Z hoàn tác lệnh nới canvas như một thay đổi; thao tác vẽ sau đó là Undo riêng. Ảnh mở rộng mặc định giữ background hiện tại, thường trong suốt; chọn nền trắng ở Trình bày toàn ảnh nếu muốn. Zoom không đổi file, nới canvas là thay đổi kích thước đầu ra có chủ đích.

Đây là canvas hữu hạn, chưa tự nới khi kéo chạm mép. Crop/vùng pixel/làm mờ/kính phóng đại vẫn làm việc với phần ảnh nguồn; chưa có crop toàn canvas. Vẽ ngoài ảnh không có nghĩa xử lý pixel tồn tại ngoài bitmap nguồn.

Project cũ giữ clipping cũ: cờ DrawOutsideImage chỉ bật khi dùng lệnh trên. Ảnh bình thường tiếp tục schema12. Project bật cờ dùng schema13 khi lưu, cần Captovix1.20+; app cũ không được ghi đè sidecar13. Giữ SourceZIP1.20 và backup trước khi cần đổi phiên bản. Nếu cài lại1.19, project13 không thể biên tập đúng ở bản đó; cài lại1.20 để dùng tiếp. Header/footer/padding/rotation/crop trước đây vẫn trong recipe, không sửa byte nguồn.

## Hiệu năng và phục hồi

- Thumbnail: tối đa128entry/16MiB trong cache app,2worker decode nền, decode theo khung240×160 và không upscale; file signature phát hiện thumbnail mới. UI nhận bitmap đã Freeze qua Dispatcher. Cache bị đẩy ra sẽ thả reference thumbnail của item nếu còn trỏ đúng ảnh cũ. Khi đóng lịch sử, danh sách gần đây yêu cầu lại thumbnail khi cần.
- Lớp PNG/PixelBase: LRU256MiB/128entry, tính cả chuỗi encoded và pixel decoded. Entry lớn hơn budget không lưu cache. Có thể decode lại khi hết cache. Đây là giới hạn reference cache, **không phải trần RAM toàn app**: document/undo/source/render buffers, WPF/GPU và cache mask còn dùng bộ nhớ riêng.
- Cache sampling của renderer riêng theo thread, cache redaction khóa khi dùng chung và bitmap chuyển đổi được Freeze; tránh render nền thay thế state của Editor. Mỗi lần render hoàn tất ghi ring64sample (kích thước, số annotation, ms, thread), không chứa ảnh.
- Trợ giúp → Báo cáo hiệu năng: số media/recent, cache hit/miss/byte, heap/process, undo/redo và render gần nhất; Copy để gửi chẩn đoán. Số đo là snapshot trên máy đang chạy, không tự kết luận đạt hiệu năng hay tương thích capture.
- Undo giữ budget lịch sử hiện có8triệu ký tự/mỗi nhánh, tối đa100snapshot; vẫn giữ ít nhất một snapshot lớn để còn Undo gần nhất, vì thế không là hard memory cap. Không đổi hành vi này thành mất Undo âm thầm.
- Mỗi lần lưu sidecar giữ bản chính tốt trước đó thành `.lastgood.bak`; nếu bản chính hỏng/mất, chỉ phục hồi khi backup qua kiểm schema/kích thước/hash nguồn và PNG nhúng. Bản chính lỗi được giữ riêng, UI báo đã phục hồi và yêu cầu lưu lại bằng Ctrl+S (hoặc lần autosave/flush tiếp theo). Nếu cả hai lỗi không tự nhận đã khôi phục. Sidecar tối đa140MB; vượt giới hạn sẽ báo lỗi, giữ phần đang chỉnh để xuất/tách dự án. Bản lưu tốt cũng được đưa vào backup trước update và Trash của nội dung.
- Last-good là **bản đã lưu trước đó**, không phải recovery mọi ký tự chưa autosave hoặc nét đang kéo trước crash. Nguồn PNG/MP4 không đổi. Không xóa/sửa project cũ để lấy thêm RAM.

## Bằng chứng kiểm tra và giới hạn

`PERFORMANCE-BENCHMARK.json` đo copy pixel thuần .NET trên Linux với dữ liệu tổng hợp3840×2160 và7680×4320,3lượt mỗi cỡ; so toàn bộ byte đầu ra bằng nguồn. Không phải WPF/GPU/capture/8K UI benchmark trên Windows. Không dùng ms này làm cam kết tốc độ thực tế.

Windows-only suite có kiểm rectangle/callout ngoài bitmap, export pixel, native source không đổi, legacy clipping, Undo, caption nhỏ/trợ năng và render redaction đồng thời. Trong môi trường bàn giao chỉ cross-build, **chưa chạy Windows**. Chưa có tile renderer hoặc nghiệm thu peak RAM/end-to-end8K, HDR/màu rộng hay mọi app chụp cuộn. V1.20 có implementation và kiểm logic/build; việc nghiệm thu bản ảnh vẫn cần checklist máy Windows.

## Checklist Windows cần thực hiện

1. Cài mới D:\Captovix\Captovix, update1.19→1.20; icon app/updater/setup/pin, library/history/schema12 và theme/phím tắt không mất.
2. Caption ở Editor/OCR/Settings/Archive/scroll/Batch/confirm: nút nhỏ nhưng click/hover/focus rõ, min/max/restore/X, Alt+F4/drag/resize, light/dark/system, DPI100–200%, màn phụ tọa độ âm.
3. Cây2026→12tháng→30/31/28/29ngày, ngày trống, năm cũ, mở nhiều nhánh,100+ảnh/ngày, Ctrl/Shift, Enter/double-click, xóa/khôi phục/export nhiều ngày.
4. Footer một dòng ở sidebar hẹp/filmstrip; … có đủ chức năng/keyboard/focus; mẫu nhỏ/giữa, style màu/step/callout không bị đổi.
5. Ảnh nhỏ, nới canvas1/2lần, negative X/Y: tạo/chọn/kéo/resize/rotate/nudge/inline text/tail vào ảnh, Ctrl+B/I, lưu/mở lại/project/library bundle/PNG/Copy/PDF. Kiểm đúng pixel và legacy crop không lộ lại nội dung ngoài crop.
6. Khóa nguồn, lỗi ghi/hết đĩa, sidecar chính hỏng/mất, backup hỏng/hash nguồn khác/schema14: không overwrite dữ liệu chưa hỗ trợ; warning chính xác, recovery và Undo.
7. Đo trên máy chuẩn4K/8K/ảnh cuộn: chuyển ảnh, zoom/pan, annotate/blur/lens, import/export/thumbnail, thời gian/RAM trước-sau. Gửi báo cáo hiệu năng và bước tái hiện; chưa gắn nhãn “đã nghiệm thu” trước khi có kết quả.
