# Captovix — Roadmap chuyên sâu về ảnh

Ngày lập: 21/10/2026. Nền hiện tại: V1.5.1 Capture Workflow — Editor Switch Fix. Bản vá sửa lỗi unload/layout khi chuyển ảnh; đã build và bổ sung hồi quy WPF, còn chờ chạy UI Windows thực tế. Các mốc V1.6–V1.20 dưới đây là kế hoạch đề xuất, chưa triển khai.

## 1. Định hướng mới

Captovix là ứng dụng Windows x64 chụp và biên tập hình ảnh phục vụ tài liệu kỹ thuật, hướng dẫn, ghi nhận lỗi và trao đổi công việc. Một Editor duy nhất chứa thư viện, canvas, nhóm công cụ và thuộc tính theo đối tượng; overlay chỉ phục vụ capture. Mở thủ công mặc định phóng to. Ưu tiên toàn bộ nhánh ảnh; giữ khả năng quay hiện có và hoãn phát triển editor video cho đến sau mốc Image Release.

Thứ tự mới thay roadmap cũ: V1.6 không còn là Library & Export; chuyển thành Image Fidelity. Library chuyển V1.18, Batch Export chuyển V1.19. Các số phiên bản là đề xuất; minor có thể phát hành patch sửa lỗi trước khi sang mốc mới. Không tự hứa ngày hoàn thành hay mức tương đương toàn bộ Snagit.

Tham khảo nhóm tính năng/cách giải quyết công việc của Snagit; Captovix tự thiết kế giao diện, icon, shape, preset, tài nguyên và mã nguồn. Không dùng tên/định dạng dự án của Snagit cho định dạng riêng.

## 2. Tiêu chuẩn chất lượng ảnh

- Capture pixel vật lý của nguồn; màn hình 3840×2160 chụp toàn màn hình SDR có kích thước 3840×2160. Chụp vùng giữ đúng số pixel vùng, kể cả màn hình tọa độ âm và scale Windows khác nhau.
- Màn hình 1920×1080 không tự có thêm chi tiết thật khi resize lên 3840×2160. Upscale và sharpen là xử lý ảnh riêng, có xem trước và undo; không đặt nhãn chụp 4K cho ảnh phóng lớn từ nguồn nhỏ.
- PNG lossless làm bản gốc; không dùng thumbnail làm dữ liệu xuất, không tự resize hoặc chuyển JPEG trong pipeline chụp/lưu/copy.
- Tách độ zoom của canvas, độ scale Windows và độ phân giải file. Có Actual Pixels để kiểm chứng; hình xem vừa khung có thể bị nội suy và không đại diện cho file bị mất chi tiết.
- Chú thích lưu dưới dạng đối tượng vector có thể sửa; render lại ở kích thước đầu ra, không phóng bitmap preview. Antialias có chủ đích ở cạnh vector không được đánh đồng với lỗi mờ toàn ảnh.
- Test nền SDR trước. HDR/wide gamut cần test và chính sách tone mapping riêng; không tuyên bố PNG 8-bit khớp tuyệt đối mọi màn hình HDR.
- DPI in ấn không tạo thêm chi tiết pixel. Với tài liệu dài, có thể xuất nhiều trang/ảnh từng phần thay vì ép ảnh rất dài vào một trang Word.

## 3. Bộ thuộc tính đối tượng mục tiêu

Shape đầu tiên: rectangle, rounded rectangle, ellipse/circle, triangle, diamond, trapezoid, pentagon, hexagon, star, polygon tùy điểm. Nhóm line/arrow có công cụ riêng nhưng dùng chung thuộc tính nét.

Mỗi đối tượng có ID ổn định; hình học/tọa độ theo ảnh gốc, z-order, trạng thái khóa/hiện, màu viền và fill riêng, độ dày, opacity riêng cho nét/nền, solid/dash/dot/dash-dot, góc xoay và các thuộc tính theo loại. Bán kính bo góc và đầu/cuối nét xuất hiện khi loại hình hỗ trợ. Bảng thuộc tính hiển thị mixed value khi nhiều đối tượng khác nhau được chọn.

Khi chọn đối tượng, thay thuộc tính tác động đối tượng đang chọn. Khi không chọn, thuộc tính áp dụng cho lần vẽ tiếp theo. Có thao tác rõ ràng để lưu style thành preset hoặc đặt làm mặc định; không âm thầm đổi mọi đối tượng cũ.

Ví dụ nghiệm thu: vẽ rectangle nền vàng nhạt, viền đỏ nét đứt 3 px → chuyển công cụ Chọn → kéo vị trí, đổi kích thước, đổi fill sang trong suốt → lưu và mở lại → đối tượng vẫn sửa được, undo/redo chính xác, PNG xuất đúng nét và màu.

## 4. Roadmap tổng thể

Tất cả mốc sau ở trạng thái DỰ KIẾN. Các nền tảng di chuyển/nhóm/chú thích từ V1.1–V1.5 được kế thừa và cải thiện, không viết lại hoặc xóa chức năng chỉ vì có tên trong mốc mới.

| Phiên bản | Mục tiêu | Chức năng chính | Kỹ thuật và dữ liệu | Điều kiện hoàn thành | Phụ thuộc/thứ tự |
|---|---|---|---|---|---|
| V1.6 — Image Fidelity | Chụp/lưu/xem/xuất rõ đúng nguồn | Pixel-native capture; Actual Pixels; thông tin kích thước; PNG gốc; copy/xuất không resize ngầm | Rà pipeline capture→file→bitmap→viewport→renderer→clipboard; test DPI; fixture pixel SDR | Đúng W×H; PNG không đổi pixel ở vùng không chỉnh trong môi trường SDR xác định; zoom không đổi file | Làm trước mọi mở rộng ảnh |
| V1.7 — Shapes & Stroke Studio | Nhiều shape với style đầy đủ | 10 nhóm shape; stroke/fill riêng; trong suốt; độ dày; solid/dash/dot/dash-dot; preset shape | Mở rộng schema annotation, geometry/path, hit testing và renderer; migration giữ file cũ | Chỉnh shape sau vẽ; lưu/mở/undo/export đúng màu, nền, nét và hình | Sau 1.6; kiểm thử transform hiện có |
| V1.8 — Object Control & Layers | Điều khiển đối tượng chính xác | Kéo/resize/rotate; X/Y/W/H/góc số; multi-select; duplicate/group; snap/căn/chia đều; layer lock/visibility/order | Selection overlay riêng; transform/undo theo gesture; layer panel tích hợp; stable ID | 1 gesture=1 undo; chọn xuyên lớp; khóa không bị kéo nhầm; mở lại giữ nhóm/lớp | Sau schema shape 1.7; nâng các tính năng có sẵn |
| V1.9 — Arrows, Lines & Connectors | Vẽ hướng dẫn và sơ đồ rõ | Mũi tên thẳng/gấp khúc/cong; một/hai đầu; dạng đầu; nét đứt; connector bám shape | Endpoint/control point model, liên kết theo ID; cập nhật khi di chuyển/nhân bản/undo | Kéo shape thì connector theo; xóa shape xử lý liên kết nhất quán; export không lệch | Dựa object model 1.8 |
| V1.10 — Text & Callout Studio | Chú thích chữ dễ đọc | Font/style/căn dòng, padding; nhiều callout; chỉnh đuôi; step marker nhiều kiểu; tự đánh số | Text layout/metrics; fallback font; text và shape không flatten khi lưu | Gõ tiếng Việt/IME, đổi zoom/rotate/export không cắt chữ; callout sửa lại được | Sau 1.8–1.9; kế thừa inline text cũ |
| V1.11 — Focus & Privacy | Nhấn đúng nội dung và che thông tin | Magnifier; spotlight; highlight theo vùng; mask blur/pixel/opaque; preset che | Object-based effect/mask; xác định thứ tự compositing; xử lý tile | Hiệu ứng đúng vị trí sau crop/rotate/resize; bản xuất che kín không chứa bản gốc ẩn | Sau object/render model ổn định; nâng mask cũ |
| V1.12 — Selection & Pixel Editing | Sửa vùng pixel có kiểm soát | Rectangle/ellipse/lasso selection; copy/cut/paste thành image object; fill/eraser; cut-out hàng/cột | Raster patch/image layer model, selection mask; giữ ảnh gốc; undo pixel có giới hạn bộ nhớ | Cắt/dán vùng không phá nguồn; undo hoàn chỉnh; phân biệt chọn pixel và chọn annotation | Cần layer control 1.8; rủi ro dữ liệu lớn |
| V1.13 — Image Composition & Templates | Ghép nhiều ảnh thành tài liệu | Kéo thêm ảnh vào canvas; crop/resize từng ảnh; layout 2/3/4 ô; header/caption; stamp/icon tự thiết kế | Image object tham chiếu asset; layout/preset; đóng gói asset dự án | Mở lại đủ ảnh ghép; layer vẫn sửa được; xuất đúng kích thước | Sau 1.10/1.12 |
| V1.14 — Scrolling Capture Foundation | Chụp nội dung dài | Người dùng chọn vùng và cuộn một chiều; ghép có overlap; xem trước và sửa điểm nối | Đăng ký/ghép khung, phát hiện trùng, cancellation, giới hạn pixel/bộ nhớ | Trang mẫu không lặp/mất đoạn; báo điểm nối không chắc; lỗi giữ các khung nguồn | Dựa fidelity 1.6; thử nghiệm có phạm vi |
| V1.15 — Advanced Capture & Stitching | Mở rộng capture sau bản cuộn cơ bản | Cuộn tự động với ứng dụng hỗ trợ; ghép ngang; fixed-region số; window/client/menu capture; con trỏ thành object ở capture hỗ trợ | Từng adapter/capability; resize/animation detection; cursor metadata và overlay riêng | Công bố danh sách hỗ trợ; không ghép sai âm thầm; không lưu HWND lâu dài | Sau 1.14; chia patch/nhánh nếu quá rộng |
| V1.16 — OCR & Text Extraction | Lấy chữ từ ảnh | OCR vùng chọn; Việt/Anh; copy plain text; giữ dòng; kết quả có thể sửa | Đánh giá OCR engine offline/license/model, chạy nền; ngôn ngữ tải tùy chọn | Bộ mẫu Việt/Anh có báo cáo lỗi/tỷ lệ; hủy được; nguồn ảnh không bị sửa | Sau selection; không hứa đúng 100% |
| V1.17 — Assisted Image Editing | Hỗ trợ sửa nội dung có sẵn trong ảnh | Sửa text OCR với preview; gợi ý vùng UI để tách/di chuyển; simplify thủ công rồi bán tự động; gợi ý vùng nhạy cảm | Detection/inpainting tách module; confidence và xác nhận; local-first; data contract có version | Mọi thao tác có xem trước/undo; vùng lỗi chỉnh thủ công được; không gửi cloud ngầm | Phụ thuộc OCR 1.16; nghiên cứu khó nhất, phạm vi có thể chia |
| V1.18 — Image Library & Projects | Quản lý kho ảnh và dự án sửa lại | Tag/folder/collection; lọc/tìm; favorite; bản gốc/bản chỉnh; project bundle; backup/import | Metadata index và migration; bundle ảnh gốc+JSON+asset; thumbnail cache độc lập | Mở lại còn object; chuyển project sang máy khác không mất asset; metadata phục hồi được | Sau mô hình project/composition; nâng thư viện hiện có |
| V1.19 — Batch Export & Documentation | Xuất ảnh/tài liệu hàng loạt | Rename preview; preset PNG/JPEG/WebP sau kiểm chứng; kích thước/chất lượng; PDF nhiều trang; watermark; xung đột tên | Job queue có cancel/retry; render nền; format capability; không chạm ảnh gốc | Xuất đúng style/mask/chữ, hiển thị kết quả từng file; không ghi đè âm thầm; hủy giữ file hoàn tất | Sau composition/library; PDF ảnh không mặc định có text OCR |
| V1.20 — Image Release & Performance | Hoàn thiện bộ công cụ ảnh ổn định | Tối ưu ảnh 4K/8K nguồn và ảnh cuộn; cache/tile/history budget; recovery; trợ năng; Windows matrix | Profiling theo máy chuẩn; soak/regression/migration/install-update tests; tài liệu hỗ trợ | Báo cáo RAM/thời gian/lỗi theo kích thước thật; không mất dự án khi lỗi; nghiệm thu máy Windows | Chỉ sau các mốc ảnh được nghiệm thu; rồi mới lập nhánh video |

## 5. Chi tiết ba phiên bản ưu tiên

### V1.6 — Image Fidelity

Tách các lỗi capture bị scale, preview bị nội suy và xuất ảnh sai kích thước. Hiển thị kích thước file gốc, kích thước đầu ra và mức zoom ở các vị trí rõ ràng. Ghi nhận monitor, DPI, phương pháp capture, không đưa thông tin kỹ thuật tràn vào luồng sử dụng thông thường.

Các ca test: màn hình SDR 100/125/150/175/200%, nguồn 1080p/1440p/4K, màn hình phụ tọa độ âm, di chuyển Editor qua monitor, crop, rotate/flip và xuất từ mức zoom bất kỳ. Dùng pattern 1-pixel, chữ nhỏ và thang màu để tìm resize/offset/tone-mapping ngoài ý muốn. Phải chạy trên Windows thật, không dùng build Linux thay thế bằng chứng runtime.

Giới hạn: không phục hồi chi tiết chưa có trong nguồn; không khôi phục nội dung bị cửa sổ khác che trong mọi capture backend; nguồn DRM/bảo vệ có thể không chụp được. Ghi rõ backend hỗ trợ từng chế độ khi nghiệm thu.

### V1.7 — Shapes & Stroke Studio

Gallery shape gọn trong toolbar: rectangle, rounded rectangle, ellipse/circle, triangle, diamond, trapezoid, pentagon, hexagon, star, polygon; line/arrow mở rộng ở V1.9. Shift hỗ trợ hình cân đối; polygon có Enter hoàn tất/Esc hủy và chỉnh điểm theo khả năng loại hình.

Thuộc tính: stroke color + alpha, fill color + alpha, no fill/no outline, stroke width theo px, solid/dash/dot/dash-dot, radius khi hỗ trợ. Có eyedropper và bảng màu gần đây; preset gợi ý như Khung lỗi, Vùng cần chú ý, Khung nét đứt. Màu fill không được thay màu stroke ngầm.

Sau khi vẽ, có thể chuyển Chọn để kéo/resize ngay. V1.8 bổ sung quản lý nâng cao, không có nghĩa V1.7 lưu shape thành ảnh phẳng. Schema mới phải đọc/migrate sidecar trước, có backup và không cho app cũ ghi đè dữ liệu mới không hiểu.

### V1.8 — Object Control & Layers

Click chọn 1 đối tượng, Ctrl+click thêm/bớt, kéo khung chọn nhiều. Có cách chọn lớp nằm dưới; bảng layer cho chọn khi các object chồng khít. Rename/lock/hide/reorder có undo. Phân biệt khóa chỉnh sửa và ẩn khỏi ảnh xuất bằng biểu tượng/tooltip rõ.

Kéo bằng chuột, phím mũi tên nudge, modifier bước lớn, Shift giữ tỷ lệ, tay nắm rotate và ô nhập góc. X/Y/W/H theo pixel của canvas/ảnh gốc được ghi nhãn nhất quán; không dùng DIP khiến màn hình scale khác nhau cho giá trị khác nhau. Snap có toggle, chỉ gợi ý không giật đối tượng ngoài ý muốn.

Giữ đầy đủ duplicate/group/ungroup/bring front/send back/align/distribute đã có, cải thiện hit testing và thao tác. Một thao tác kéo/resize/rotate phải undo một lần, không tạo hàng trăm bản lịch sử. Selection handles và guide không xuất vào PNG.

## 6. Giải thích hai loại đối tượng

1. Shape/callout/text do Boss vẽ: Captovix biết ID và hình học nên chọn, di chuyển, đổi style sau vẽ được. Đây là ưu tiên V1.7–1.8.
2. Nút/chữ đã có sẵn trong screenshot: chỉ là pixel. Để di chuyển hoặc sửa như object cần selection/cut-paste hoặc OCR/detection, và phải xử lý nền phía sau. V1.12/V1.16/V1.17 giải quyết dần; không tự biến mọi screenshot thành thiết kế editable hoàn hảo.

PNG/JPEG xuất là bản ghép; mở riêng lại không phục hồi shape. Metadata dự án/sidecar hoặc project bundle giữ object. Copy ảnh ghép sang Word cũng không mang theo mô hình chỉnh sửa Captovix. Project có thể chứa ảnh gốc chưa che; bản chia sẻ an toàn cho trường hợp che thông tin là ảnh ghép đã render và kiểm tra, không phải bundle gốc.

## 7. Kiến trúc tiếp tục phát triển

Giữ nền native Windows C#/.NET WPF đang dùng; không chuyển Web App và không thêm database/cloud cho các thao tác ảnh offline. Trước mỗi mốc phải đọc source mới nhất và kiểm tra phần đã có.

- Capture service: pixel/source/cursor/topology; không trộn với Editor zoom.
- Document model có version: original + objects/layers/assets + image appearance, stable ID và migration.
- Render pipeline: cùng quy tắc hình học cho preview/export/clipboard; preview cache/thumbnail không phải nguồn xuất.
- Command/undo: ghi theo thao tác, bounded memory; lưu atomic/autosave và recovery.
- Shape/path/hit-test dùng cùng geometry; renderer phụ thuộc Windows nhưng geometry/model có test thuần.
- Tile/cache cho nguồn lớn: đặt giới hạn trước, báo rõ trước khi cấp phát; không quảng cáo ảnh kích thước vô hạn.
- OCR/assisted editing là module tùy chọn; chưa chốt engine/model trước khi thử Việt/Anh và kiểm tra giấy phép/khả năng phân phối.
- Mở tài liệu cũ có migration và backup. Phiên bản không hiểu schema mới không được âm thầm ghi đè.

## 8. Rủi ro và cổng nghiệm thu

- Chất lượng: số pixel đúng chưa đủ; kiểm tra offset, màu, font, nét, crop và clipboard ở Windows thật.
- Shape và transform: xoay/flip/crop không làm lệch hit-test/mask; mỗi loại shape có test riêng.
- Chụp cuộn: fixed header, sticky footer, animation, virtualized list, tải lazy và nội dung đổi giữa các khung có thể làm ghép sai. Ưu tiên capture một chiều có kiểm soát, giữ source frames và báo không chắc; không cam kết mọi ứng dụng.
- OCR: độ chính xác tùy chữ, ngôn ngữ, tương phản; sửa text không bảo đảm giống font gốc. Gợi ý tự động cần người kiểm tra.
- Che thông tin: blur/pixelation không tương đương che đặc; bản xuất che đặc phải kiểm tra pixel và không nhúng lại nguồn. Không thay đổi nguyên tắc giữ source trong dự án.
- Dữ liệu: migration/undo/autosave và update phải giữ các ảnh/chú thích đã bàn giao; không reset thư viện để nâng cấp.
- Nghiệm thu: build, unit/property tests, renderer Windows, thao tác người dùng, installer/updater và recovery. Phân biệt rõ kiểm thử tự động đã chạy với checklist còn chờ.

## 9. Cách bàn giao mỗi bản xây dựng

Chỉ hai file theo yêu cầu Boss:

- Captovix-vX.X.X-Update-Packages.zip: gói phát hành updater, kèm installer canonical và hướng dẫn asset phù hợp.
- Captovix-vX.X.X-Source.zip: source dựng lại được, README/CHANGELOG/ROADMAP, test và checklist Windows.

Không gửi riêng Setup/Portable trừ khi Boss yêu cầu. Sau mỗi bản phải mô tả phiên bản kế tiếp: mục tiêu, chức năng cụ thể, cách dùng/lợi ích và phần chưa nằm trong phạm vi. Không tự code cả roadmap khi mới duyệt kế hoạch.

Đề xuất bản tiếp theo: V1.6 Image Fidelity. Sau đó V1.7 Shapes & Stroke Studio và V1.8 Object Control & Layers để đáp ứng trực tiếp yêu cầu ảnh rõ, nhiều shape, sửa màu/nền/nét và di chuyển đối tượng.

## 10. Nguồn tham khảo chính thức

Các nguồn dùng để tham khảo nhu cầu/chức năng và giới hạn chất lượng, không phải đặc tả implementation của Captovix:

- TechSmith, Snagit Features: https://www.techsmith.com/snagit/features/ — shapes, arrows, callouts, templates, scrolling, OCR, smart move, simplify và export/library. Một số tính năng khác theo nền tảng; không mặc định mọi tính năng đều có trên Windows.
- TechSmith, Images Captured in Snagit Are Blurry or Fuzzy: https://support.techsmith.com/hc/en-us/articles/203732158-Images-Captured-in-Snagit-Are-Blurry-or-Fuzzy — scale/zoom/lossy compression là các nguyên nhân ảnh trông mờ.
- TechSmith, Does Snagit Support Scrolling Capture?: https://support.techsmith.com/hc/en-us/articles/41686829112205-Does-Snagit-Support-Scrolling-Capture
- TechSmith, What is Smart Move?: https://support.techsmith.com/hc/en-us/articles/39980449934093-What-is-Smart-Move
- TechSmith, Snagit Grab Text & Edit Text Best Practices: https://support.techsmith.com/hc/en-us/articles/115002031592-Snagit-Grab-Text-Edit-Text-Best-Practices — OCR có hạn chế theo font, cỡ chữ và tương phản; không dùng như cam kết chính xác cho Captovix.
