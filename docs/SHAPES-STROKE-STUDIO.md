# V1.7 — Shapes & Stroke Studio

## Thao tác

1. Mở/chụp ảnh, chọn Hình dạng (S). Chọn loại ở inspector; đây là 9 hình kín cố định, không phải polygon tự do.
2. Nhập màu viền/nền dạng #RRGGBB hoặc bấm màu nhanh. Bật Có viền/Tô nền; nhập đậm viền/nền 0–100%, độ dày 1–40 px và kiểu nét. Bấm Áp dụng hoặc Enter. Esc lấy lại thuộc tính đã lưu của lựa chọn.
3. Kéo để vẽ, giữ Shift cho hình cân đối. Chọn Đường (L) hoặc Mũi tên (A) để vẽ đoạn. Shift khóa hướng theo 45° và giữ khóa khi chạm biên.
4. Mũi tên có đầu tam giác đặc/mở/thoi/tròn, một/hai đầu. Cỡ 0 tự động, tối đa 160 px; đoạn ngắn tự giới hạn đầu để không lấn toàn thân.
5. Nhấn V, bấm vùng tô hoặc mép hình rỗng, kéo để di chuyển; kéo tay nắm để resize. Đường/mũi tên có tay nắm hai đầu. Shift khi kéo góc giữ tỷ lệ. Ctrl/Shift+click chọn nhiều, Alt+click chọn lớp dưới theo nền sẵn có.
6. Chọn đối tượng rồi đổi shape ở picker: chỉ hình kín được đổi, giữ bounds và style. Đổi màu/kiểu nét bằng Áp dụng; mỗi lần áp dụng là một Undo.
7. Preset Áp dụng sửa lựa chọn; Chỉ dùng preset cho nét mới bỏ lựa chọn, không sửa đối tượng. Sau chỉnh và Áp dụng, nhập tên để lưu preset riêng. Tối đa 20 mẫu; không ghi đè tên có sẵn.
8. Chuyển ảnh hoặc Ctrl+S lưu sidecar; Copy/Xuất dùng cùng renderer, không đưa selection handles vào file. Ưu tiên PNG giữ chất lượng.

Lưu ý: các trường có nút Áp dụng không commit từng phím. Áp dụng trước khi đổi lựa chọn hoặc lưu preset. Viền/nền cùng tắt hoặc đậm 0% có thể làm hình không thấy; Ctrl+Z hoặc Chọn tất cả để tìm lại. Đối tượng che thông tin (Cover) giữ màu opaque và luôn nằm trên chú thích, không bị preset shape đổi thành hình khác.

## Dữ liệu và giới hạn

Annotation thêm FillColor/FillEnabled/StrokeEnabled/StrokeOpacity/FillOpacity/Dash/CornerRadius/StarPoints/StarInset/Tip/DoubleArrow/ArrowHeadSize. Enum Kind chỉ nối thêm, giữ số cũ. Copy/serialize/history/duplicate giữ đủ trường. Schema 5 đọc 1–4; mặc định giữ viền 100% liền và nền không tô. Lần ghi đè sidecar cũ đầu tiên tạo file `.pre-v1.7.bak`. Nếu backup lỗi thì Save lỗi, nội dung đang chỉnh vẫn được giữ trong Editor. Muốn hạ cấp cần bản backup phù hợp, không dùng file schema 5 trên app cũ.

Hình học normalized tạo StreamGeometry, RectangleGeometry và EllipseGeometry. Preview, hit testing và full-resolution export dùng cùng hình học/kiểu nét. Vùng chưa sửa đi qua nhánh exact pixel V1.6; zoom không tham gia file xuất. Chụp desktop giữ GDI V1.6.1; window/video giữ backend hiện có. Không cam kết HDR/ICC hoặc ảnh 16-bit.

Không triển khai layer panel, xoay từng đối tượng, editable polygon, eyedropper hay connector tự bẻ góc trong V1.7. V1.8 nâng điều khiển đối tượng/layer. Shape UI vẫn nằm trong một Editor.

## Nghiệm thu Windows — chưa thực thi tại nơi build

- Vẽ cả 9 hình, đổi nét/opacity/no fill/no stroke, bo góc/sao cực trị.
- Chọn vùng tô/mép rỗng, chọn xuyên, kéo/resize, đường ngang/dọc/chéo, Shift sát biên; Undo một lần/Redo.
- Đổi shape/preset/HEX; lỗi trường không làm đổi đối tượng; preset chỉ cho nét mới không đổi ảnh cũ.
- Chuyển ảnh nhiều lần, đóng mở, kiểm tra file backup và style; export/copy/PNG mở lại.
- DPI 100/125/150/175/200%, chuyển monitor, ảnh lớn, thumbnail; V1.6.1 vùng chụp 1467×386.
- Cài/update giữ D:\Captovix\Captovix, URL, preset/library/Edits. Chạy ShapeWindowsChecks, EditorWindowsChecks, FidelityWindowsChecks trên Windows để có bằng chứng.

Tham khảo triển khai WPF: https://learn.microsoft.com/dotnet/desktop/wpf/graphics-multimedia/geometry-overview và https://learn.microsoft.com/dotnet/desktop/wpf/graphics-multimedia/how-to-define-a-pen .

Preset riêng cũ trùng tên với một trong ba mẫu mới vẫn được giữ; dropdown ưu tiên mẫu của người dùng. Không tự đổi màu hay ghi đè preset đó. Sidecar bị lỗi JSON được giữ byte trước khi lưu bản phục hồi; schema tương lai bị từ chối ghi đè.
