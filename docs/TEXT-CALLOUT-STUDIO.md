# V1.10 — Hướng dẫn và nghiệm thu Text & Callout Studio

## Cách dùng

1. Vẽ khung với R. Bấm vùng trống/kéo tiếp để tạo khung mới.
2. Chuyển N, click vùng trống tạo bước 1, 2, 3. Bấm vào viền khung trước đó và kéo: hệ thống chọn khung, bảng phải hiện thuộc tính khung. Không phải bấm Chọn. Sau đó click vùng trống vẫn tạo bước tiếp theo.
3. Bấm khung đã tô, bỏ Tô nền hoặc bấm Không tô nền. Nền biến mất ngay; viền và màu đã chọn được giữ. Bật lại Tô nền để phục hồi màu. Undo/Redo được.
4. Chọn một hoặc nhiều đối tượng → Ctrl+C → Ctrl+V. Bản dán vẫn là đối tượng, sửa màu/font/kích thước được. Dán tiếp lệch 16 pixel mỗi lần (có giới hạn theo viewport), không tự resize. Có thể chuyển ảnh rồi dán. Chỉ copy một connector thì đầu ra ngoài selection được tháo; copy cả mạng giữ liên kết giữa các bản sao.
5. Ctrl+Shift+C copy ảnh đã chỉnh để dán vào ứng dụng khác. Ctrl+C không có selection cũng copy ảnh. Khi focus trong ô chữ/nhập số, phím copy/paste theo ô nhập. Chưa paste bitmap ngoài thành layer.
6. T tạo Text, Q tạo Callout. Double-click/F2 sửa chữ ngay trên ảnh; Enter xuống dòng, Ctrl+Enter hoàn tất, Esc hủy. Đang sửa chữ dùng Ctrl+C/V văn bản bình thường.
7. Text & Callout Studio: chọn font, đậm/nghiêng/gạch chân, HEX màu chữ; đặt rộng chữ >0 để wrap/căn dòng. 0 dùng rộng tự nhiên. Padding theo pixel ảnh, giãn dòng 0 tự động hoặc 0.8–3 lần cỡ chữ. Bấm Áp dụng kiểu chữ/hộp hoặc Enter trong ô thuộc tính. Cỡ chữ dùng điều khiển Cỡ chữ hiện có.
8. Callout lấy rộng wrap theo hộp; bật giãn hộp để tăng chiều cao theo chữ. Chọn một trong 5 kiểu; chỉnh căn dọc, nền và viền. Chọn callout rồi kéo tay nắm đuôi. Hai kiểu không đuôi không hiển thị đuôi khi xuất.
9. Step chọn tròn/vuông/thoi; các số 1–9999. Đánh lại số theo thứ tự trong tài liệu (không tự sắp không gian). Có selection thì chỉ các Step thuộc selection/nhóm hợp lệ; không chọn thì mọi Step đang hiện, không khóa. Khóa/mixed group không hợp lệ được từ chối trước khi thay đổi.
10. Lưu preset để giữ typography cùng màu/nét. Font không có trên máy khác dùng Segoe UI để xem/xuất và báo trong inspector; chưa nhúng font.

## Chọn và chồng lớp

Công cụ vẽ hiện tại được giữ riêng với loại đối tượng đang chỉnh. Bấm đối tượng hiện trên cùng để chỉnh; Ctrl/Shift để thêm selection, V+Alt để luân phiên đối tượng chồng theo cơ chế cũ. Khung trong suốt bắt viền, vùng trống bên trong dùng vẽ mới. Khung có nền bắt bên trong. Giữ Alt trong công cụ vẽ để ép tạo mới chồng lên đối tượng có sẵn; hữu ích khi nối connector từ giữa shape hoặc đặt bước lên nền. Crop có chế độ riêng. Space kéo khung nhìn giữ như trước. Đối tượng khóa/ẩn không trở thành mục tiêu kéo trên canvas; dùng Layers để mở khóa/hiện.

## Ma trận nghiệm thu trên Windows — chưa chạy tại nơi build

- Mở app/cũ và cài mới; Editor maximized, chuyển 20 ảnh không lỗi. Thử D:\Captovix\Captovix, URL update đã gắn và khởi động lại sau update.
- Vẽ rectangle → N tạo 1/2/3 → kéo viền rectangle → đổi màu/resize. Thử lại từ Line, Arrow, Shape, Text, Highlight; kéo vùng trống tạo đúng công cụ cũ. Thử Alt vẽ chồng, Shift đa chọn, Layers khóa/ẩn, chuột phải nhóm/lớp; 1 gesture = 1 Undo.
- Tô nền vàng → bỏ checkbox → nền mất → Undo/Redo → bật lại màu cũ. Thử shape/callout/text và lưu/mở lại. Không đổi vùng pixel nguồn.
- Ctrl+C/V một shape, nhóm, mạng connector, text có style; chuyển ảnh có cỡ khác rồi dán; Undo/Redo. Thoát/mở app, dán clipboard còn lại. Thử clipboard đang bị app khác giữ, clipboard văn bản/ảnh ngoài có phản hồi, không crash.
- Trong TextBox nhập Việt: “Cộng hòa — Ắ ằ ữ ệ”, dòng dài, nhiều dòng, clipboard văn bản; Telex/VNI/Windows IME theo bộ gõ máy. Enter/Ctrl+Enter/Esc và chuyển focus trong lúc composition không làm mất chữ. Chưa coi IME đạt chỉ qua build.
- 5 callout: dài/ngắn, nhiều dòng, màu nền/viền/chữ riêng, padding 0/12/40, auto-grow bật/tắt, căn dọc/căn dòng, kéo đuôi, xoay và resize. Kiểm tra glyph italic/underline, font có/không có, số step 9/10/9999 ở 3 hình.
- PNG ở zoom 25/50/100/200% có cùng số pixel và nội dung xuất; lưu/mở giữ đối tượng và style. Kiểm tra tất cả chữ trong vùng ảnh. Nếu chữ/hộp vượt ảnh/crop, inspector báo; cần di chuyển, tăng hộp hoặc giảm cỡ trước xuất. Không hứa hiển thị nội dung ngoài biên export.
- Thử scale Windows 100/125/150/175/200%, chuyển màn hình âm/tỷ lệ khác; scroll inspector đầy đủ. Capture/hotkey/UIA theo docs/ARROWS-CAPTURE-WORKFLOW.md. HDR riêng.

Chạy tự động trên Windows:

```powershell
dotnet run --project tests/TextClipboardChecks -c Release -p:Platform=x64
dotnet run --project tests/TextStudioWindowsChecks -c Release -p:Platform=x64
.\Build.cmd
```

## Dữ liệu và giới hạn

Schema 8 đọc 1–7; callout cũ được migration về nền trắng/viền như bản cũ. Lưu sidecar cũ lần đầu giữ .pre-v1.10.bak; không đụng ảnh gốc/backup cũ. Workflow giữ schema/cơ chế backup V1.9; preset thêm Typography tùy chọn, định dạng cũ vẫn nạp. Hạ bản cần khôi phục sidecar tương thích từ backup, không mở schema 8 bằng app cũ.

Mỗi đối tượng có một kiểu chữ. Chưa có rich spans, nhúng font, OCR, arbitrary bitmap paste/image composition, video editor. Vùng xuất tuân theo ảnh/crop; auto-grow không tăng pixel canvas ngoài ý muốn.

Bản tiếp theo: V1.11 — Focus & Privacy: magnifier, spotlight, highlight theo vùng và hoàn thiện mask blur/pixel/che đặc; thứ tự compositing/preset và kiểm tra bản xuất che kín. Chưa triển khai ở bản này.
