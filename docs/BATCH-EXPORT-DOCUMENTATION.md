# Captovix V1.19 — Batch Export & Documentation

## Xuất nhiều ảnh và PDF

Chọn ảnh trong thanh gần đây hoặc Thư viện tổng bằng Ctrl+click từng mục, Shift+click một khoảng. Bấm icon Xuất nhiều ở chân thư viện hoặc Tệp → Xuất hàng loạt / PDF. Video không đưa vào hàng đợi ảnh; chức năng xuất video riêng vẫn giữ.

Có preset PNG gốc, PNG chia sẻ1920, JPEG1920 và PDF A4; đổi format, cạnh dài, tên và watermark theo nhu cầu. Các thiết lập xuất gần nhất lưu trong `batch-export.json`. Cạnh dài0 giữ kích thước bản chỉnh; 1280/1920/3840 chỉ thu nhỏ, không tự phóng ảnh nhỏ. Tên hỗ trợ `{date}`, `{name}`, `{n}`, xem tên dự kiến ngay trong hàng đợi. Tên không hợp lệ Windows được làm sạch; trùng tên trên đĩa/trong cùng lượt được thêm số. Không tự ghi đè file cũ hoặc ảnh gốc.

Chọn thư mục xuất, có thể chọn một ảnh trong hàng đợi rồi Xem trước. Watermark là chữ Segoe UI ở góc dưới phải, xám bán trong suốt, cỡ8–96px; để trống để tắt. Ảnh gốc/project không bị chỉnh. Preview xem ảnh đã render cả crop/annotation/mask và các thiết lập xuất. Một bản chỉnh hỏng/khác hash nguồn phải báo lỗi, không tự xuất bản gốc thay thế.

Nhấn Xuất tất cả. Render ảnh trên một luồng STA riêng cho mỗi công việc, tuần tự; ghi file bằng file tạm, commit không ghi đè. Hủy giữ file PNG/JPEG đã xong, bỏ file tạm đang làm. Thử lại mục lỗi/đã hủy bỏ qua PNG/JPEG đã hoàn tất; chỉnh options hoặc bấm Xuất tất cả sẽ lập lượt mới. CSV ghi tên ảnh, đường dẫn thực tế và trạng thái từng mục. Đóng cửa sổ khi đang chạy sẽ yêu cầu hủy rồi chờ công việc hiện tại trả về an toàn.

PDF gồm các trang ảnh A4, tự xoay hướng trang theo ảnh, lề24pt. Pixel được nhúng RGB nén lossless Flate; alpha ghép nền trắng. Chỉ thu tỷ lệ trình bày trên trang, dữ liệu ảnh nhúng giữ kích thước sau preset. PDF này chưa có lớp chữ OCR tìm kiếm, không tạo văn bản biên tập trong Word. Cần đủ trang thành công mới ghi PDF cuối; lỗi/hủy không xuất một PDF thiếu trang. Thử lại PDF dựng lại toàn bộ nhóm trang. PNG giữ alpha và pixel bản render khi không resize/watermark; JPEG95% có mất mát, nền trắng. **WebP chưa triển khai**, cần đánh giá codec sau.

Giới hạn:500 ảnh/lượt ở engine,40MP/ảnh đã render, PDF100trang và200MB dữ liệu ảnh nén. Thư viện tổng chia100mục/trang, thao tác chọn áp dụng cho trang đang xem. Không có cam kết không tốn RAM với ảnh100MP; xuất riêng vẫn dùng pipeline cũ, tối ưu ảnh lớn thuộc V1.20.

## Thư viện gần đây và toàn bộ lịch sử

Thanh Editor chỉ chứa tối đa50 nội dung. Mục đang mở từ lịch sử có thể giữ một slot trong50; các mục còn lại chọn theo thời gian mới nhất. Favorite được ưu tiên trong tập này, không kéo toàn bộ favorite cũ vào thanh. Metadata và file cũ vẫn giữ đầy đủ trong thư viện, không tự xóa hay chuyển file chỉ vì vượt50.

Tệp → Thư viện tổng · lịch sử, hoặc icon lịch sử ở chân thanh ảnh. Chọn nhóm theo ngày/tháng/năm, tìm theo tên/tag/collection/ngày, Trước/Sau để xem các trang100mục. Thumbnail cũ nạp khi hiện mục; không giải mã toàn bộ lịch sử ngay lúc mở app. Mở trong Editor vẫn dùng Editor chính; không tạo một Editor mới. Ctrl/Shift chọn nhiều để xuất hoặc xóa trong lịch sử. Chọn nhiều trong thanh gần đây giữ nội dung Editor đang xem, tránh tải lại ảnh chỉ vì thêm mục vào nhóm.

Nút ☆ và × trên tile mặc định ẩn; hiện khi hover hoặc focus bàn phím để vẫn dùng được bằng bàn phím. Delete hoặc nút xóa chân thư viện tác động cả nhóm đã chọn; × trên một tile trong nhóm xóa nhóm, ngoài nhóm chỉ xóa tile đó. Hộp thoại riêng theo theme hiển thị số lượng và một số tên; mặc định focus Hủy. Ảnh/video, sidecar và thumbnail chuyển vào Trash theo một receipt. Khôi phục phục hồi cả lần xóa; không ghi đè file đã xuất hiện lại. Cần kiểm thử thực tế rollback/hết đĩa trên Windows.

Thanh cuộn có Value/Minimum/Maximum/ViewportSize binding rõ. Quan trọng hơn: drag để ghép ảnh chỉ bắt đầu từ item thật, loại trừ Thumb/ScrollBar/Button và cử chỉ Ctrl/Shift; không còn bắt thao tác kéo thanh cuộn làm drag ảnh.

## Công cụ, icon và preview

Giữ kéo một nút công cụ trên thanh chỉnh ảnh rồi thả lên nút khác để đổi vị trí; hoặc Hiển thị → Sắp xếp công cụ cá nhân để Lên/Xuống/Mặc định. Áp dụng ngay, lưu `tool-order.json` riêng theo tài khoản Windows/máy. Công cụ mới trong bản sau tự bổ sung, giữ các handler và thuộc tính công cụ cũ.

Bảng thuộc tính dùng thứ tự chung: mẫu đã lưu → nhóm màu nhanh/viền/nền → hình dạng/kiểu nét → thông số hình/chữ/độ dày → đổ bóng. Callout gallery chuyển vào phần hình dạng trước bóng như Khung; palette/cỡ chữ chung không còn nằm phía sau bóng. Các trường không áp dụng bị ẩn. Vị trí trong bảng thuộc tính được chuẩn hóa; tùy biến cá nhân ở bản này là vị trí các nút trên thanh công cụ.

Icon thao tác dùng đường bo mềm ở các góc, nét1.4px, đầu nét/join tròn. Chỉ đổi hình icon, không đổi geometry của annotation hay logo đã chốt. Preview mẫu dựa trên bounds thực của drawing, không clip vào ô bắt đầu tại0; Step/callout/chữ căn giữa và vừa block, không thay đổi kiểu vẽ đã lưu. Hiệu ứng có hình minh họa mẫu, không nhúng pixel screenshot vào favorite.

## Đường ngang trong chụp cuộn

Ảnh Boss gửi có các đường tím đều nhau: đó là canvas hướng dẫn mối nối của cửa sổ preview, tách riêng khỏi bitmap ghép. V1.19 mặc định ẩn; bật “Hiện điểm nối đang chọn” mới hiện đúng một đường đang kiểm tra. Không vẽ các đường này vào ảnh đưa vào Editor hoặc ảnh xuất.

Không coi ảnh này là bằng chứng thuật toán overlap sai. Engine ghép hàng pixel không đổi; nội dung lặp/mất dòng, app có sticky header, animation hoặc lệch cuộn vẫn cần kiểm tra overlap và xác nhận như trước. Các trường hợp khác cần khung nguồn/session để tái hiện. Chưa công bố hỗ trợ mọi trang/app.

## Nghiệm thu Windows

1. Cài/update1.18→1.19: đường dẫn D:\Captovix\Captovix, logo runtime/Setup/pin, dữ liệu/theme/phím tắt/project giữ nguyên.
2. Ctrl/Shift/Ctrl+A và Delete ở thư viện không tác động annotation; xóa nhiều ở gần đây/lịch sử, Hủy, khôi phục nhóm, file xung đột và rollback lỗi ghi metadata.
3. Kéo thumb vertical/horizontal, page track, mouse wheel; drag item vào bản ghép vẫn hoạt động; không drag khi Ctrl/Shift hoặc nhấn tile-action.
4. 0/49/50/51/1000mục, favorite cũ, mở lịch sử quá50, tìm theo ngày, đổi grouping/page và chọn nhiều; ảnh cũ luôn mở được, không reset thư viện.
5. Drag reorder/Lên/Xuống/Mặc định, mở lại giữ thứ tự; Ctrl/Shift drawing, selection và Tool_Click còn đúng.
6. Sáng/tối/system, scale100/125/150/175/200%, màn phụ tọa độ âm: icon mềm/nét rõ, hover/focus actions, preview Step/callout không cắt, thứ tự thuộc tính giống nhau, layout không che nút.
7. Batch có crop/rotate/connector/callout/redaction/alpha, PNG/JPEG/PDF, preset/preview/watermark, giữ số pixel native; đối chiếu nguồn và file xuất; hủy giữa render/ghi, thiếu file, sidecar hỏng/hash sai, hết đĩa, trùng tên và retry.
8. PDF kiểm trình đọc phổ biến: đủ trang/thứ tự/RGB/alpha nền trắng, hướng A4 và nội dung không cắt; đây là PDF ảnh không có OCR text layer.
9. Scroll preview mặc định không đường tím; bật chỉ một đường theo selection; thay overlap/loại khung/100%/Fit, ảnh ghép PNG không có guide; kiểm lặp/mất dòng trên các trang mẫu riêng.

Build/cross-test Linux không thay nghiệm thu UI/capture/install Windows. Xem VALIDATION.md cho bằng chứng đã chạy.
