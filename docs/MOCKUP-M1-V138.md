# Captovix V1.38.0 — Text & Button Editing (M1)

Xây từ source V1.37.3 Boss gửi, giữ sửa Capture Bounds và Image Layer Thread Safety. M1 chỉnh một form đơn giản trên nền màu phẳng. Không quét/tách tự động toàn giao diện, không phục hồi nền gradient/ảnh hoặc nhận chính xác font gốc.

## Cách dùng

1. Mở/chụp một form. Bấm **Mockup** trên thanh công cụ (hoặc **Thêm → Mockup chữ / nút**). Kéo bao trọn chữ/title/nút cũ, có chừa hết nét chữ và bóng trong vùng chọn; tối thiểu 8×8px, tối đa20MP. Bấm đơn không tạo đối tượng.
2. Hộp chỉnh sửa tự nhận chữ tiếng Việt + Anh bằng OCR có sẵn. Xem lại dấu/chữ; có thể nhập thủ công, chọn ngôn ngữ, nhận lại hoặc dừng OCR. Nếu đang gõ khi OCR hoàn tất, kết quả OCR được giữ riêng để chọn dùng, không ghi đè phần đang gõ.
3. Chọn Label, Title hoặc Nút; sửa nội dung, font/cỡ/đậm/căn chữ/đệm; màu chữ, màu nền che; với nút có nền, viền và bo góc. Màu được gợi ý từ vùng ảnh; có thể nhập HEX, bấm Chọn hoặc lấy màu trực tiếp trên ảnh xem trước.
4. So sánh Gốc / Xem trước. Với nền màu phẳng, chọn màu che khớp nền và bao hết nội dung cũ để chữ cũ không lộ. M1 không tự đảm bảo trên nền phức tạp; chọn lại vùng/màu khi cần.
5. **Áp dụng** tạo một đối tượng chỉnh sửa được trong một bước Undo. **Hủy/Esc** không thay tài liệu. Nhấp đúp đối tượng hoặc chọn **Sửa Mockup** ở bảng thuộc tính/menu chuột phải để sửa. Công cụ vẫn được giữ sau khi áp dụng; kéo vùng trống để tạo tiếp; Alt+kéo để chọn vùng mới chồng lên đối tượng đã có.
6. Kéo/resize/xoay bằng thao tác đối tượng hiện có; vùng che chữ cũ ghim ở tọa độ nguồn khi đối tượng mới di chuyển. Delete/Undo bỏ cả đối tượng và vùng che của nó, làm hiện lại nội dung gốc ở vùng đó. Nhân bản/copy-paste/mẫu bố cục chỉ mang đối tượng mới, không mang thao tác che nguồn sang vị trí/ảnh khác.
7. Ctrl+S lưu sidecar thư viện; lưu dự án `.captovix` giữ cả PNG gốc và đối tượng. Khi chuyển ảnh rồi mở lại, đối tượng và vùng che được nạp lại. Xuất/Copy PNG có kết quả đã chỉnh; PNG phẳng không giữ các đối tượng để sửa tiếp.

## Dữ liệu và hiệu năng

- Ảnh gốc và PixelBase không bị ghi đè bởi M1. Metadata Mockup tùy chọn gắn vào Rectangle; gồm loại Label/Title/Button, tọa độ nguồn cần che và màu nền che. Các thuộc tính chữ/nút tái sử dụng model hiện tại.
- Chỉ tài liệu có Mockup mới dùng schema18. App1.38 đọc tài liệu cũ; app cũ không mở được sidecar schema18. Lần lưu đầu nâng một sidecar cũ sang18 giữ nguyên bytes bản cũ ở `.pre-v1.38.bak`. Không cần migration DB hoặc deploy backend; cần backup trước rollback app.
- Vùng snapshot dựng trên STA worker; pixels tách/freeze để trả UI. OCR qua worker hiện có, có hủy và kiểm vòng đời. Preview debounce100ms, render tối đa900×480px. Bản xem trước có thể phóng nhưng ảnh xuất vẫn dùng tọa độ/pixel nguồn.
- Áp dụng kiểm session, revision và ảnh nguồn; kết quả cũ không áp vào ảnh khác. Draft không vào Undo trước khi Áp dụng. Hết phiên hộp chỉnh sửa bỏ các tham chiếu preview.
- Giữ runtime .NET10.0.13 self-contained, VC++ prerequisite wrapper, OCRvie/eng, license/backend/update-source. Không thêm dịch vụ AI hay upload ảnh ra ngoài.

## Kiểm tra và giới hạn nghiệm thu

- Đã chạy kiểm tra logic Mockup: lựa chọn/che nguồn, tiếng Việt, schema, lưu/đọc JSON, sửa không nhân đôi, snapshot độc lập, Undo/Redo khớp bytes, paste/duplicate/template, màu gợi ý không đổi pixels, validate và resize tối thiểu.
- Chạy lại các suite EditorCore/Composition/TextClipboard/PixelStudio/ErrorDialog/CaptureBounds/StudioReliability/Precision. Số lượng và kết quả chính xác trong docs/VALIDATION.md.
- App Release và `tests/MockupWindowsChecks` được cross-build. Fixture Windows bao gồm che hết glyph cũ trên nền phẳng, pixel ngoài vùng không đổi, Undo/Redo render, nguồn nguyên vẹn, snapshot liên luồng, sidecar backup/load, portable project và kéo chọn/bấm đơn. CHƯA thực thi fixture này vì host không có WindowsDesktop.
- Cần Windows để nghiệm thu: một form thực có label/title/nút; OCR vie/eng và nhập IME; nền sáng/tối; đổi nền/viền/bo góc; drag/move/resize; Cancel; chuyển thư viện/mở lại; Undo/Redo; Copy/xuất; DPI100/125/150%; Setup cài đè và updater khởi động lại. Không coi cross-build là đã nghiệm thu chuột/IME/OCR/render native.

## Phát hành

Hai ZIP: `Captovix-v1.38.0-Update-Packages.zip` và `Captovix V1.38.0 — Text & Button Editing.zip`. Thoát cả app/khay; giải nén gói update rồi chạy `HuyVo.Captovix-win-Setup.exe` vào thư mục hiện tại. Không uninstall/xóa dữ liệu. Để app cũ và `Captovix-Setup.exe` trực tuyến thấy1.38.0, đưa đủ5releaseassets trong updateZIP lên stable release v1.38.0 và đặt Latest. Bản bàn giao chưa tự publish GitHub.

Tiếp theo phần Mockup: **M2 — Element Scan & Review**, chưa gán số phiên bản; quét gợi ý phần tử và cho kiểm tra/sửa kết quả. Chỉ xây khi Boss chọn.
