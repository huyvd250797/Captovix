# V1.36.0 — Text & Callout Polish

## Quy tắc thao tác

1. Chọn Callout, nhấn điểm muốn đuôi trỏ vào, giữ và kéo trái/phải; thân mở bên cạnh và căn dọc giữa điểm đuôi. Thả chuột để gõ trực tiếp. Kéo lên/xuống làm thay đổi chiều cao; thân giữ điểm đuôi ở giữa theo chiều dọc.
2. Đệm mặc định30px là lựa chọn của Captovix, có thể chỉnh/lưu mẫu; không phải thông số được TechSmith công bố. Thân giữ kích thước đã vẽ, tối thiểu hợp lý cho font/đệm. Chữ dài xuống dòng và nở chiều cao khi auto-fit bật; xóa chữ không tự co. Tắt tự giãn để giữ hộp cố định.
3. Resize cạnh riêng rộng/cao; Shift giữ tỷ lệ. Node đuôi chỉ đổi đuôi; node xanh lá xoay đối tượng. Khi đổi cỡ thân, đuôi giữ vị trí trên ảnh; kéo cả đối tượng thì thân và đuôi đi cùng nhau.
4. Click ngoài hoặc Ctrl+Enter kết thúc; Enter xuống dòng; Esc hủy phiên sửa. IME preedit không đưa vào document trước commit. Chọn mẫu lưu/đổi thuộc tính vẫn giữ nội dung và đuôi.

## Tham khảo chính thức

- TechSmith Snagit Help12.4.1: điểm bắt đầu callout là đuôi, node riêng cho resize và đuôi; hỗ trợ padding/căn chữ: https://assets.techsmith.com/Docs/pdf-snagit/Snagit_12.4.1_Help.pdf . Tham khảo mô hình thao tác, không sao chép giao diện/tài sản và không gán30px cho Snagit.
- Lưu mẫu công cụ theo loại: https://support.techsmith.com/hc/en-us/articles/45790530141069-How-to-Set-and-Save-Default-Tool-Properties-in-Snagit .

## Chất lượng ảnh

Giữ source vật lý, không scale trong DesktopPhotoCapture; PNG được decode/đối chiếu pixel sau ghi. RenderExact giữ pixel nguồn và chỉ alpha-composite chú thích; scale chỉ dùng thumbnail/xem trước khi chỉ định maxWidth. PNG/copy đầy đủ không phụ thuộc zoom. Các pipeline này đã có ở source nền, không thay bằng sharpen hoặc nén mất dữ liệu. HDR/WCG là giới hạn cần nghiệm thu riêng, không hứa màu HDR toàn hệ thống.

## Kiểm thử Windows bắt buộc trước phát hành rộng

- Telex/VNI và Microsoft IME: câu có dấu, dán nhiều dòng, đang composition click ngoài/Ctrl+Enter/Esc, đổi công cụ/chuyển ảnh/xuất; không mất/lặp chữ, undo một phiên, con trỏ ở giữa hộp trống và sau xuống dòng.
- Vẽ Callout4hướng; nhấn đơn; nội dung1dòng rồi100/1000dòng; resize ngang/dọc ở0°/25°/90°, tail giữ vị trí. Kéo node đuôi sau đó resize; Undo/Redo, lưu/mở project,10kiểu hộp và mẫu cũ/mới, đệm0/30/100.
- Kéo Callout20.000ký tự với/không bóng: đo frame UI trên máy chuẩn, không lặp đo glyph/ghép hình khi chỉ dịch chuyển. Kiểm dùng RAM sau100lần nhập/xóa/chuyển ảnh. Không coi cross-build là số đo hiệu năng Windows.
- 100/125/150/200%DPI, light/dark/system, cửa sổ hẹp; logo/tên không che menu/tool. Vàng đủ nổi trên nền sáng/tối, crosshair không có trong PNG.
- Chụp bảng chữ nhỏ, kiểm100%, clipboard/PNG ởzoom25/100/200% có cùng kích thước/hash pixel khi không thêm chú thích; nhiều màn hìnhtọa độ âm, HDR riêng.
- Cài V1.31.0, giữ ảnh/phím tắt/license rồi cung cấp stablefeed chỉ có Full1.36: check/tải/xácminh/backup/restart/About1.36. Lặp khi offline/hỏng hash/hủy; không áp dụng khi lỗi hoặc đang chụp. Setup cài đè đúng thư mục. Chưa thực thi native trong môi trường Linux.

## Dữ liệu và hoàn tác

Giữ schema17, file mẫu và cấu hình hiện hữu. Đối tượng cũ không bị tự đổi hình/tựfit khi chỉ mở; sửa nội dung/áp dụng mẫu mới mới chạy fit. Không thay backend hoặc tạo trial. Cache chỉ nằm RAM, không ghi vào project; riêng thread để không chia sẻ FormattedText mutable giữa UI và export.
