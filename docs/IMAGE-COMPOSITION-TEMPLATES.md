# V1.13 — Ghép ảnh & mẫu bố cục

## Tạo và ghép ảnh

1. Vào Bố cục → Tạo bản ghép mới. Chọn chiều rộng/cao, đệm ngoài, khoảng cách, màu nền, tiêu đề, ghi chú và caption.
2. Bấm một mẫu xem trước: hai cột, một cột, ba cột, ba ảnh dọc, một lớn/hai nhỏ, lưới hoặc tự do. Bấm Tạo canvas và chọn nhiều PNG/JPEG/BMP. Có thể hủy chọn ảnh để giữ canvas trống.
3. Bố cục → Thêm ảnh để thêm nhiều ảnh vào tài liệu hiện tại. Kéo file từ Explorer vào vùng Editor cũng thêm lớp, không thay ảnh đang mở. Kéo thumbnail thư viện sang canvas sẽ lấy bản render đã chỉnh của ảnh đó và thêm vào tài liệu đích đang mở trước khi kéo; nguồn vẫn giữ trong thư viện.
4. Bố cục → Sắp xếp ảnh áp dụng mẫu vào những lớp ảnh trên canvas hiện tại. Đệm/khoảng cách/tiêu đề/caption được áp dụng trong một Undo; ảnh gốc và ID lớp được giữ. Giảm đệm hoặc tăng canvas nếu ô quá nhỏ. Hiện và mở khóa các nhóm trước khi áp dụng; khôi phục crop toàn ảnh trước khi xếp lại.
5. Các mẫu thích nghi theo số ảnh, tối đa 16 ảnh khi xếp; thêm tối đa 16 file mỗi lần. Mẫu một lớn/hai nhỏ dành cho đúng 3 ảnh; số khác dùng lưới.

## Chỉnh từng ảnh

- Bấm ảnh hoặc chọn trong Layers. Bảng **Lớp ảnh** cho chọn Vừa ảnh (Fit) hoặc Lấp đầy ô (Fill); cả hai giữ tỷ lệ, không kéo méo bitmap.
- Fit giữ toàn bộ vùng nguồn với khoảng trống nếu khác tỷ lệ. Fill cắt phần vượt ô; bật **Kéo ảnh bên trong ô** để kéo vùng nhìn thấy. Tắt chế độ này để kéo vị trí cả ô. Hai thanh ngang/dọc cũng chỉnh vị trí bên trong; một lần kéo trực tiếp = một Undo. Resize/rotate/group/giữ tỷ lệ có trong hình học và menu chuột phải như các lớp khác.
- **Thay ảnh trong ô** giữ hình học/style của ô và đặt lại vùng crop nguồn. **Cắt riêng ảnh này** nhập X/Y/rộng/cao bằng pixel nguồn; giữ toàn bộ PNG gốc nhúng và có nút khôi phục. Chưa có tay nắm crop tương tác riêng cho từng image layer.
- Bo góc, độ dày viền (0 = không viền), màu viền và bật bóng thay đổi lớp đã chọn. Bóng lớp ảnh dùng cấu hình mặc định; điều chỉnh hướng/khoảng cách nâng cao chưa có riêng trong panel ảnh.
- Ctrl+click hai ảnh/nhóm → Bố cục → Đổi chỗ hai ảnh đã chọn. Đổi nội dung ảnh và kiểu nguồn, giữ vị trí các ô và caption.
- Caption tự nhóm cùng ảnh. Double-click/F2 sửa chữ. Bỏ nhóm bằng Ctrl+Shift+G để di chuyển/chỉnh riêng, rồi Ctrl+G để nhóm lại. Khi xếp lại, các caption do bố cục tạo giữ chữ theo thứ tự; tiêu đề/ghi chú nhập trong form.
- Ctrl+V ảnh từ clipboard cũng tạo image layer; Ctrl+C/V các đối tượng Captovix vẫn giữ đối tượng có thể sửa.

## Mẫu và dự án

- **Lưu mẫu bố cục** → `.cvtemplate`. Mẫu giữ kích thước canvas, appearance, ô ảnh, style và các đối tượng vector; bỏ dữ liệu PNG lớp ảnh và nền pixel đã sửa, thay chữ công việc bằng Tiêu đề/Chú thích/Ghi chú. Không tự gửi hay chia sẻ file.
- **Dùng mẫu bố cục** mở một canvas mới với các ô màu nhạt. Thêm ảnh sẽ điền lần lượt các ô; hoặc chọn ô trong Layers và dùng Thay ảnh. Mẫu tối đa 4 MB/1000 đối tượng; không chứa ảnh công việc. Cần mở khóa ô được khóa trong mẫu trước khi thay.
- **Lưu dự án kèm ảnh** → `.cvproject`: original.png và document.json. document.json nhúng các PNG lớp ảnh, crop và thuộc tính; mở dự án không phụ thuộc đường dẫn ảnh ngoài máy. Kiểm tra hash/kích thước/schema trước khi nhập; không giải nén đường dẫn tùy ý.
- Dự án giữ ảnh gốc và lớp sửa được. PNG/JPEG xuất là ảnh ghép phẳng để chia sẻ, không giữ đối tượng. Đổi kích thước ảnh trong bố cục chủ động resample lúc render; zoom chỉ thay cách xem, không đổi kích thước xuất.

## Các chỉnh sửa giao diện

- Callout mới có padding=30. Mẫu callout chỉ vẽ hình mẫu, không mang chữ đang chỉnh; font/đệm/cỡ preview riêng để tránh tràn. Thay font/màu ở đối tượng không làm mẫu bị kéo dài. Mẫu ☆ và preset riêng được lọc theo công cụ; preset typography cũ chưa có định danh loại được giữ trong nhóm chữ để tương thích.
- Auto-fit mở rộng chiều ngang và tăng chiều cao đến giới hạn canvas. Chữ vượt vẫn được giữ trong đối tượng nhưng phần ngoài vùng nội dung không vẽ ra ảnh: tăng ô, giảm font/padding hoặc rút nội dung để xem đầy đủ. Callout cũ không tự đổi geometry khi vừa mở; quy tắc mới áp dụng khi vẽ/chỉnh/resize.
- Chọn màu chữ bằng palette hoặc **Chọn màu khác…** (hộp chọn màu Windows); HEX vẫn là tùy chọn bổ sung. Ctrl+B, Ctrl+I, Ctrl+U chỉnh toàn bộ chữ của đối tượng đã chọn; khi nhập inline giữ vị trí con trỏ. Các ô nhập số/font/phím tắt không nhận nhầm lệnh định dạng.
- Bỏ UI tạo Pixelate; các vùng Pixelate đã lưu vẫn render đúng và sửa mức cũ được. Đây không phải bỏ Vùng pixel chọn/cắt/copy.
- Menu Chụp & quay → Cấu hình: Vùng chụp, âm thanh/video, kết quả sau chụp, viền và preset. Menu Công cụ → Phím tắt & chạy nền: tổ hợp phím và tùy chọn khay/khởi động Windows. Hai trang độc lập. Trợ giúp → Kiểm tra cập nhật chỉ chứa trạng thái/phiên bản/tải/cài.
- Thanh phải ưu tiên thuộc tính công cụ, chữ/nội dung/mẫu; Hình học, Layers và Trình bày toàn ảnh ở dưới. Bỏ mô tả dài, dòng “ảnh gốc giữ riêng”, footer và gạch phân cách thừa.
- Icon WPF vector tự dựng trong MainWindow.Polish.cs; không dùng emoji cho công cụ/layers. Chụp có icon camera, quay có record.

## Dữ liệu và giới hạn

Schema12 đọc schema1–11, giữ ID/style/ảnh gốc. Lần lưu đầu của sidecar cũ tạo `.pre-v1.13.bak`; app cũ không được dùng để ghi dự án schema12. Preset mới có ToolKind; preset và mẫu sao cũ tiếp tục đọc được. `.cvproject` chỉ mở bằng V1.13 trở lên.

Canvas pixel tối đa 40 MP, mỗi PNG nguồn tối đa khoảng 72 MB, tổng dữ liệu PNG mã hóa tài liệu 128 triệu ký tự. PNG decode/encode và snapshot Undo có thể tốn RAM/CPU; lịch sử 8 triệu ký tự có thể giữ ít bước với ảnh lớn. Thử trên Windows máy đích trước khi dùng công việc lớn. HDR/màu rộng chưa thuộc nghiệm thu này.

Chưa có OCR, AI, cut-out hàng/cột, tách nền, tự nhận đối tượng từ screenshot, stamp chuyên biệt, quản lý kho template cloud hoặc video editor mới. Crop từng image layer đang là form pixel nguồn; không có UI crop-handle riêng. Các tính năng này không được mô tả như đã hoàn tất.

## Cập nhật và build

Nguồn đã gắn trong `src/Captovix/update-source.json`. V1.13 dùng nguồn trong file được đóng gói; nguồn cũ trong update-settings.json chỉ giữ thời điểm/tùy chọn kiểm tra, không ghi đè nguồn của nhà phát hành. Không có textbox/URL trên UI; log kỹ thuật vẫn giữ lỗi đầy đủ.

Cập nhật qua app hoặc chạy HuyVo.Captovix-win-Setup.exe trong Update-Packages. Installer giữ luồng chọn đường dẫn, nhận diện managed root và dữ liệu, mặc định/policy cài đặt hiện có. Không thêm dấu `--` trước `--installto` để tránh panic EXE_ARGS.

Đăng Release công khai stable, Set as latest, tải từng asset trong Update-Packages (nupkg, setup canonical, releases.win.json, assets.win.json, RELEASES). Không chỉ tải ZIP/source. Chưa tự đăng Release trong lần bàn giao này.

Build: cài .NET SDK10 và NSIS3 trên Windows, chạy Build.cmd. scripts/Build-Windows.ps1 chạy logic và Windows checks trước publish; xem VALIDATION.md và WINDOWS-TEST-CHECKLIST.md. Môi trường bàn giao chỉ chạy logic/Linux và cross-build WPF; không thay thế kiểm thử Windows thật.
