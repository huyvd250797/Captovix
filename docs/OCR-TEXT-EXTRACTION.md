> TừV1.17: OCR có zoom/pan/lens và có chức năng Chỉnh sửa hỗ trợ riêng. Xem ASSISTED-IMAGE-EDITING.md; phần dưới ghi lại nền1.16.

# Captovix V1.16.0 — OCR & Text Extraction

## Nhận chữ

1. Chọn ảnh trong Editor, hoàn tất crop hoặc nhập chữ đang sửa.
2. Bấm **OCR** ở thanh công cụ ảnh hoặc **Công cụ → Nhận chữ từ ảnh · OCR…**.
3. Mặc định chọn toàn ảnh. Kéo trên preview để chọn vùng; bấm Chọn toàn ảnh để đặt lại.
4. Chọn Việt + English, Việt hoặc English. Chọn bố cục Tự động, Một khối văn bản hoặc Chữ rải rác trên giao diện.
5. Bấm Nhận chữ. Kết quả có thể sửa ngay rồi Copy văn bản hoặc Lưu TXT UTF-8.
6. Hủy nhận dạng/Esc dừng tác vụ; đóng cửa sổ lúc đang chạy sẽ hủy trước khi đóng.

OCR không biến chữ trong screenshot thành đối tượng chữ editable. Sửa kết quả ở đây chỉ sửa văn bản trích xuất; chỉnh chữ trên ảnh là hướng V1.17. Không tự thêm văn bản OCR vào project/sidecar; lưu TXT nếu cần giữ kết quả.

## Công nghệ, dữ liệu và giới hạn

- Tesseract .NET/native 5.2.0 x64; Leptonica1.82.0; model LSTM eng/vie từ tessdata_fast, commit và SHA256 trong `src/Captovix/tessdata/models.json`.
- Model và DLL đi kèm app, không cần API key hay tải ngôn ngữ sau cài. VC++ Runtime đã nằm trong Setup. Thiếu/hỏng model hoặc native DLL sẽ báo lỗi, không lặng lẽ chuyển sang dịch vụ khác.
- Nhận dạng chạy trong tiến trình riêng, có giới hạn 120 giây; hủy/timeout kết thúc tiến trình rồi dọn file tạm.
- Mỗi vùng OCR tối đa 20 triệu pixel, không tự downscale. Ảnh dài hơn: chọn từng vùng. Preview có thể thu nhỏ nhưng vùng OCR lấy từ bitmap đầu ra đầy đủ pixel.
- Chỉ gửi snapshot **đã render** của ảnh hiện tại cho worker: crop, lớp ảnh và phần che đang thấy được giữ lại. Không đọc nội dung gốc nằm sau phần che để nhận chữ.
- Alpha được ghép trên trắng trong bản tạm cho OCR; không sửa ảnh gốc, file xuất, đối tượng hoặc Undo.
- Input/output tạm trong `%LOCALAPPDATA%\HuyVo\Captovix\OcrJobs\<job-id>`, dọn sau thành công/hủy/lỗi thông thường. Nếu app bị tắt cưỡng bức/mất điện có thể còn thư mục tạm; có thể xóa `OcrJobs` khi app đã thoát.
- Engine confidence là ước lượng, không phải tỷ lệ đúng được kiểm chứng. Kiểm tra lại dấu, tên, mã và số. Chữ viết tay, bảng nhiều cột, font cách điệu, ảnh mờ và thứ tự đọc phức tạp chưa được cam kết.

## Crop mới

Bấm Crop sẽ hiện sẵn khung bao quanh vùng ảnh hiện tại với 8 tay nắm và lưới chia ba. Kéo cạnh/góc để chỉnh, kéo bên trong để di chuyển khung đã thu nhỏ; Shift giữ tỷ lệ khi phù hợp. Có thể nhập X/Y/Rộng/Cao rồi **Enter** hoặc **Áp dụng crop**. Enter trong ô nhập cũng áp dụng. Esc/Hủy bỏ bản chọn; Chọn lại đưa khung về toàn vùng hiện tại. Crop chỉ commit khi áp dụng và có Undo; không làm mất ảnh gốc.

## Header theo theme

Tiêu đề native của Editor và dialog cập nhật khi đổi Light/Dark/System. Windows11 dùng DWM caption/text/border colours; Windows10 thử dark titlebar qua API DWM, khả năng hiển thị phụ thuộc phiên bản Windows và theme hệ thống. Giữ nút đóng/thu nhỏ/phóng to native. Chế độ tương phản cao/theme đặc biệt cần kiểm riêng.

## Độ rõ ảnh

Ảnh mở mặc định ở **100% pixel vật lý**, có cuộn khi ảnh lớn hơn vùng xem. Crop xong giữ 100% nếu đang dùng chế độ này. Nút **Rõ nét · 100%** về tỷ lệ 1 pixel ảnh = 1 pixel màn hình, kể cả Windows scale125–200%. **Vừa khung** vẫn có để xem toàn ảnh.

Ảnh báo cáo trước ở zoom65,7%; thu nhỏ có thể làm chữ trông mềm hơn. Không có bằng chứng file gốc đã bị mất chi tiết, nên không tự sharpen/upscale hoặc đổi pixel ảnh để che vấn đề preview. Pipeline chụp màn hình PNG native, copy/xuất không resize ngầm được giữ. 100% không tạo thêm chi tiết vượt nguồn. HDR/wide gamut và backend chụp cửa sổ vẫn cần đánh giá riêng.

## Kiểm tra

`tests/OcrChecks`: giới hạn kích thước/ngôn ngữ/bố cục, Unicode và ánh xạ chọn vùng preview. `tests/OcrWindowsChecks`: chạy native engine và crop/render trên Windows; môi trường bàn giao chỉ cross-build bộ này. Bộ mẫu tổng hợp ở `tests/OcrFixtures`; kết quả Linux Tesseract5.3.4 trong `docs/OCR-BENCHMARK.json` không thay cho nghiệm thu native Windows5.2.0.

Trên Windows cần kiểm: Việt/Anh/ghép ngôn ngữ; vùng đảo chiều/biên ảnh; bảng nhiều cột; không tìm thấy chữ; hủy và đóng dialog khi chạy; model/DLL thiếu; OCR ảnh có mask; Crop kéo8tay nắm/Enter trong ô/sauUndo; theme titlebar các cửa sổ; DPI100–200%; PNG/copy/export ở nhiều mức zoom; cài/update từ1.15.1 giữ dữ liệu.

## Nguồn kỹ thuật

- https://github.com/charlesw/tesseract/tree/5.2.0
- https://github.com/tesseract-ocr/tessdata_fast
- https://tesseract-ocr.github.io/tessdoc/Data-Files
- https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute

V1.17 dự kiến: Assisted Image Editing — xem trước sửa chữ OCR trên ảnh, xử lý nền có kiểm soát và gợi ý tách vùng giao diện; có Undo và xác nhận, không tự sửa ảnh chỉ vì có kết quả OCR.
