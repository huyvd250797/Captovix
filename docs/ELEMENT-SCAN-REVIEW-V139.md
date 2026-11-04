# V1.39.0 — Element Scan & Review (M2)

Xây trên ZIP V1.38.0 Boss gửi. Giữ Capture Bounds1.37.2, Image Layer Thread Safety1.37.3 và M1. M2 dùng OCR nội bộ và phân tích hình ảnh, không gọi AI/cloud để gửi ảnh.

## Luồng sử dụng

1. Mở/chụp ảnh giao diện. Chọn **Thêm → Mockup**. Công cụ xuất hiện trong một ô công cụ động đã dùng gần nhất; không có nút Mockup cố định riêng trên hàng công cụ. Các công cụ phát triển về sau khai báo trong StudioToolCatalog và đi qua Thêm/ô động này.
2. Mockup có **Sửa một vùng chữ/nút** (M1), **Quét vùng phần tử** (kéo chọn; Alt+kéo khi chồng đối tượng đã có) và **Quét toàn ảnh đang hiển thị** (vùng ảnh/crop hiện tại, không gồm lề trang trí). Ảnh/vùng tối đa20MP, tối thiểu8×8px; ảnh dài/lớn hơn thì quét từng vùng.
3. Màn rà soát dựng snapshot đã áp dụng các che dữ liệu đang hiển thị. Phân tích vùng màu/viền phẳng trước, nhận chữ Việt+Anh sau; có chọn Việt/Anh riêng, Dừng và Quét lại. Quét lại xác nhận bỏ bản rà soát chưa áp dụng. OCR bị dừng/lỗi vẫn giữ gợi ý hình ảnh đã có để sửa/thêm khung thủ công. Đóng/Hủy không thay tài liệu.
4. Các khung gợi ý gồm **Chữ, Tiêu đề, Nút, Ô nhập, Khối giao diện**. Không chọn sẵn khung nào. Xanh dương: gợi ý rõ; cam: cần kiểm tra; xanh lá: người dùng đã chọn áp dụng. Mức phần trăm là điểm gợi ý tổng hợp/OCR, không phải cam kết xác suất chính xác.
5. Bấm khung hoặc danh sách để chọn. Kéo thân để dời, kéo4góc để sửa; có thể nhập X/Y/W/H. Đổi loại, sửa chữ/màu nền che/nền control/màu chữ. X/Y là pixel nguồn tuyệt đối, kể cả vùng quét có offset/crop. Có Kéo thêm khung và Loại bỏ khung. Chi tiết viền/bo góc/font có thể sửa tiếp bằng nhấp đúp sau khi áp dụng, như M1.
6. Label gần ô nhập được gợi ý ghép; dropdown cho đổi/bỏ liên kết. Chữ nằm trong nút/ô nhập được ghép vào nội dung control thay vì tạo một đối tượng chữ trùng trên nó. Khung hình học và dòng OCR trùng bị loại; khi chỉnh tay tạo khung gần trùng, Apply yêu cầu bỏ chọn/sửa trước.
7. Chọn từng phần tử đã kiểm tra, hoặc bấm **Chọn kết quả rõ** rồi tiếp tục rà soát. Đây là thao tác chủ động chọn bản nháp, chưa thêm đối tượng vào ảnh. Tối đa200gợi ý/500dòng OCR; có cảnh báo khi giới hạn. Tối đa120phần tử mỗi lần áp dụng.
8. Bấm **Xem sau áp dụng** để xem kết quả dựng từ các phần tử đang chọn. Mọi sửa bản nháp đưa về ảnh gốc vùng quét; bấm xem lại khi cần. Scroll dọc; Ctrl+Scroll zoom; Shift+Scroll ngang; có Vừa khung và +/-.
9. **Áp dụng phần tử đã chọn** tạo đúng phần được chọn, trong một bước Undo/Redo. Ảnh gốc được giữ nguyên. Nhấp đúp từng đối tượng để sửa Mockup. Lưu sidecar/dự án và mở lại giữ nội dung/loại/liên kết; PNG xuất là ảnh phẳng. Paste/nhân bản ánh xạ liên kết label/control sang bộ ID mới và không che nhầm vùng nguồn trên ảnh khác.

## Cách xác định và giới hạn

- Phân tích BGRA đã render, lấy mẫu tối đa khoảng1,2triệu điểm, lượng tử hóa màu rồi tìm thành phần liên thông. Bỏ nền toàn ảnh, vùng quá nhỏ/không đủ dạng chữ nhật; suy luận control theo kích thước, màu và viền.
- Tesseract lấy khung dòng + độ tin cậy, ghép chữ vào control có chứa dòng; nhận tiêu đề tương đối theo chiều cao chữ. Liên kết label/ô nhập dựa trên khoảng cách và căn hàng/cột, giảm tin cậy khi nhiều ô gần nhau. Loại trùng bằng diện tích giao/diện tích hợp.
- Đây là gợi ý để người dùng duyệt. Ảnh không chứa DOM/cây control thật nên không bảo đảm nhận đúng tất cả. Form phẳng rõ nét phù hợp nhất; icon, bảng dày, chữ nhỏ, viền rất mảnh, gradient/bóng/ảnh nền và control chồng nhau có thể cần sửa thủ công.
- M2 chưa phục hồi nền phức tạp, nhận font chính xác hay tạo ứng dụng/HTML tương tác. Chữ/ô/nút được dựng bằng đối tượng Mockup, không phải control đang hoạt động.
- Khối giao diện sẽ che toàn bộ vùng bằng màu phẳng khi áp dụng; chỉ chọn sau khi kiểm tra cả phần tử con và preview, tránh bỏ mất nội dung bên trong. Ô nhập/khối luôn được đánh dấu cần kiểm tra.

## Dữ liệu và đóng gói

M2 dùng schema19 cho Input/Block, đối tượng từ scan và liên kết LabelForId; M1 thuần vẫn dùng18. Khi nâng sidecar cũ lên19, giữ bytes cũ ở `.pre-v1.39.bak`. Các phiên bản cũ không đọc sidecar19; sao lưu trước rollback. Metadata mới không ghi đè ảnh nguồn. Cache snapshot dùng pixel tách/freeze, OCR có tiến trình riêng/hủy/timeout; snapshot/preview dựng trên STA worker, kiểm session/revision trước khi áp dụng.

V1.39.0 dùng .NET10.0.13 self-contained và VC++ prerequisite như bộ cài đầy đủ V1.38, giữ nguyên model OCR và cấu hình license. Thay Captovix.exe/Captovix.dll và Licensing.Shared.dll bằng bản mới; không nâng dependency/runtime.

## Bản quyền

Xem [ACTIVATION-METADATA-V139.md](ACTIVATION-METADATA-V139.md). Cần chạy một migration SQL bổ sung để ngày kích hoạt vĩnh viễn lấy đúng ngày tạo activation trên server; không cần deploy lại Edge Function. Không reset dữ liệu/KEY/trial. Thông tin cũ thiếu trường ngày sẽ báo rõ, không lấy ngày refresh làm ngày kích hoạt.

## Kiểm tra

Đã chạy ElementScanChecks, MockupChecks, CompositionChecks, TextClipboardChecks, PixelStudioChecks, CaptureBoundsChecks, ErrorDialogChecks, QuickCaptureChecks, LicensingChecks và handler tests. Cross-build app/fixtureWindows thành công. Xem VALIDATION.md cho số kiểm tra và giới hạn.

Chưa nghiệm thu trực tiếp OCR/WPF/drag/popup/IME/DPI/Setup trên Windows. FixtureWindows chứa render che chữ cũ/nguồn bất biến, một bước Undo/Redo, privacy snapshot, sidecar backup/load, portable project và popup trial/perpetual thật. Không coi cross-build là đã thực thi các kiểm tra Windows này.
