# V1.22.0 — Object Editing Precision · 27/10/2026

- Chọn đối tượng/nhóm tại con trỏ bằng menu, hover outline, Alt+click trong Chọn; mở khóa từ danh sách mà vẫn chặn kéo đối tượng khóa.
- Căn theo lựa chọn/ảnh/canvas; chia đều khoảng trống theo output; snap/guide theo zoom; Ctrl bypass và Shift khóa hướng màn hình.
- Thông số khung theo pixel đầu ra; ghi rõ góc nguồn/nhóm delta; số áp dụng nguyên tử, giữ Undo/Redo và output dimensions. Đường0chiều có thể đổi chiều còn lại bằng số.
- Giữ tỷ lệ nhập/kéo, tay nắm cạnh/góc, bù điểm click; kích thước giao diện gizmo; giới hạn font/callout trong nhóm. Resize không tự FitText làm đổi khung đã nhập.
- Miền duplicate/paste theo canvas ngoài ảnh. Khi chủ động đặt ngoài nguồn hoặc vào khung, mở clip annotation trong cùng Undo; nguồn/hiệu ứng pixel vẫn clip đúng, không tràn vào viền.
- Preference tỷ lệ và tham chiếu căn lưu trên máy. Giữ schema12/13/14, logo, source update, hotkey và installer; không tự publish. Docs/VALIDATION.md ghi rõ kiểm chứng và Windows còn chờ.

# V1.21.0 — Layout Precision · 26/10/2026

- Thêm bốn cạnh mở rộng pixel riêng, nền màu/trong suốt, tùy chọn tự mở rộng khi tạo chú thích sát mép; vùng thao tác quanh ảnh chỉ dành cho Editor.
- Crop theo toàn bộ đầu ra; giữ bitmap gốc và annotation editable; Undo/Redo; schema14 bảo vệ app cũ ghi đè, đọc schema12/13.
- Chữ hoàn tất khi click ngoài và trở về Chọn; callout khởi tạo đuôi tại chỗ, hỗ trợ một click có kích thước mặc định, giữ thao tác kéo.
- Làm gọn menu; giữ phím tắt/nút Undo/Redo; đưa Canvas sang công cụ vẽ. Cây thư viện thêm header icon/số lượng và tùy chọn ẩn ngày trống lưu trên máy.
- Giữ stack, approved logo, ổn định nguồn update và installer chọn đường dẫn. Không tự publish, không thay engine capture/quay.
- Logic/build/artifact verification được ghi trong docs/VALIDATION.md; nghiệm thu Windows thật còn chờ.

# V1.20.0 — Image Release & Performance

- Caption/functional icons/gallery nhỏ gọn; footer1hàng và overflow.
- Thư viện cây năm→12tháng→đúng ngày→media, nạp nhánh/ngày theo nhu cầu, giữ multi-selection và50recent.
- Nới canvas160/cạnh có Undo, annotate/edit/export ngoài bitmap; opt-in mới dùng schema13, project cũ schema12 giữ clipping. Không tự resize nguồn.
- LRU thumbnail16MiB/128,2decodeworker; PNG256MiB; sampling cache thread-local, redaction cache lock/freeze; ring render64 và báo cáo cache/RAM/history.
- Last-good sidecar có validation/fallback, backup/trash kèm recovery; limit140MB, giữ file lỗi và chặn schema14.
- Pixel-copy4K/8K đo trên Linux; chưa Windows runtime/render/profile/peak RAM/tile engine. Checklist IMAGE-RELEASE-PERFORMANCE.md.

# V1.19.0 — Batch Export & Documentation

- PNG/JPEG/PDF hàng loạt, preset/naming/preview/watermark/queue/Cancel/Retry/CSV; commit không ghi đè; PDF FlateRGB lossless/A4.
- Thanh gần đây tối đa50, lịch sử tổng nhóm ngày/tháng/năm/phân trang100mục; giữ toàn bộ metadata/file.
- Ctrl/Shift chọn nhiều, modal xóa theo theme và receipt/khôi phục nhóm; tile-action hover/focus; sửa composition-drag bắt scrollbar.
- Icon bo mềm; drag reorder và cấu hình thứ tự công cụ theo máy; chuẩn thứ tự thuộc tính; mẫu bounds thật, không clip Step/callout.
- Scroll preview ẩn guide mặc định, chỉ hiện mối nối chọn khi bật. Guide không thuộc bitmap; không đổi thuật toán overlap.
- Giữ schema12/scroll2/branding/stable update/install arguments; thêm tool-order và batch-export vào backup metadata.
- WebP, PDF OCR text layer và tối ưu ảnh100MP chưa triển khai. Windows desktop acceptance còn chờ; xem docs/BATCH-EXPORT-DOCUMENTATION.md.

# V1.18.0 — Image Library & Projects

- Thêm tag, bộ sưu tập logic, favorite, bộ lọc; backup/import .cvlibrary có media gốc và sidecar/embedded layers; hash, giới hạn, staging, mã mới và rollback.
- Sidebar/filmstrip kéo đổi kích thước, lưu layout; preview Fit sau kéo; thư viện ngang gọn; Xem ảnh gốc riêng.
- Gộp OCR/Hỗ trợ; titlebar có X rõ sáng/tối, About dùng artwork lớn; icon vector mảnh.
- Lưu mẫu mọi công cụ vẽ, bookmark Chọn/Crop/vùng pixel; mẫu ở đầu, bỏ tên dưới preview.
- Ưu tiên container ở cạnh/góc trên khi chụp element; nút ↓/→ mở scroll session từ vùng chọn.
- Viền capture mới padding0, sát ngoài ảnh; giữ ảnh cũ. Logo/runtime/setup/updater artwork đã chốt giữ nguyên.
- Chưa nghiệm thu desktop Windows; xem docs/IMAGE-LIBRARY-PROJECTS.md và VALIDATION.md.

# V1.17.0 — Assisted Image Editing

- Sửa chữ OCR với gợi ý dòng, cỡ/màu, sample nền và preview; tách vùng thành lớp; một Undo; tạo bản ảnh mới khi transform không tương thích. Nền màu phẳng, không có AI inpainting tự động.
- OCR zoom/pan/lens; popup cập nhật Later/X bay về footer, giữ badge và xác nhận restart riêng.
- Capture/element overlay đen với cutout trong suốt; nâng UIA descendant selection, container normalization, stale-window detection và timeout helper.
- Context menu rõ icon/tên/shortcut, controls bo góc/scrollbar/theme; tìm kiếm bằng icon; media folder cho ảnh/video mới, giữ file cũ.
- Sửa MUI2 ghi đè logo Setup, thêm PE audit wrapper.
- Asset v117 và refresh shortcut Captovix-owned, AppID thống nhất, branding.log; audit PE app/updater.
- Giữ schema12 và nền capture PNG/native; chờ Windows runtime/UI/install/update acceptance. Xem ASSISTED-IMAGE-EDITING.md.

# V1.16.0 — OCR & Text Extraction

- Thêm OCR toàn ảnh/vùng với model Việt/Anh đóng gói, worker riêng, Hủy/timeout, sửa/copy/TXT; kiểm hash model; không đọc phần ảnh gốc nằm sau mask.
- Crop tạo khung toàn ảnh ngay, Enter trong ô nhập cũng áp dụng; khung/tay nắm/numeric/Undo giữ pixel.
- Titlebar native đồng bộ theme; ảnh mở mặc định100% pixel, giữ mức này sau crop nếu đang dùng. Không sharpen/upscale hoặc sửa nguồn để che blur preview.
- Không đổi schema12; giữ dữ liệu cũ, nguồn update, installroot và logo.
- Đã kiểm logic/build/fixture Linux; OCR/UI/titlebar/capture/cài-update Windows còn chờ. Xem VALIDATION.md và OCR-TEXT-EXTRACTION.md.

# Captovix V1.15.1 — Capture Reliability

Đã triển khai trong source: chỉnh thời gian đợi/số nấc cuộn; kiểm tra nguồn trước/sau nhịp, tạm dừng khi thay đổi/che/mất focus; dọn candidate, commit revision trước khi chuyển phiên; sửa kích thước overlap ngang. Dark mode đồng bộ menu, hover, công cụ và nền vùng làm việc. Logo tím gán trực tiếp khi chạy và đường dẫn ICO mới. Thêm Giới thiệu thương hiệu đã chốt. Build/logic đã kiểm; Windows thực tế còn chờ. Xem docs/CAPTURE-RELIABILITY.md.

**Bản tiếp theo V1.16 — OCR & Text Extraction:** nhận chữ theo vùng hoặc toàn ảnh, copy Unicode, hỗ trợ/đánh giá tiếng Việt và Anh; lựa chọn engine và dữ liệu ngôn ngữ, thông báo lỗi khi thiếu bộ nhận dạng. OCR chưa được triển khai. Phần dưới là lịch sử các bản trước.

# Captovix V1.15.0 — Advanced Capture & Stitching

Đã triển khai trong source toàn bộ nhóm V1.15 Boss chọn: tự cuộn dọc/ngang, pause/resume/dừng khi không có nội dung mới; header/footer cố định; xem hai khung và loại khung lỗi; vùng cố định/gần đây, client hiển thị và menu hẹn giờ. Chưa nghiệm thu trên Windows thật. Hướng dẫn chi tiết: docs/ADVANCED-CAPTURE.md.

V1.16 dự kiến: củng cố khả năng cuộn theo từng app sau nghiệm thu, chỉnh tốc độ/thời gian chờ và cải thiện điểm nối. Nội dung bên dưới là lịch sử nền1.14; ưu tiên hướng dẫn1.15 bên trên.

# V1.14.0 — Scrolling Capture Foundation (23/10/2026)

Chụp vùng cuộn dọc thủ công, overlap matching có phát hiện trùng/chưa chắc, sửa và xác nhận điểm nối, xem trước100%, ghép trực tiếp hàng pixel; hạn chế24khung/4MPkhung/64MPnguồn/40MPoutput/32768h. Khung nguồn lưu disk, manifest/hash và menu khôi phục; hủy/ghi lỗi không xóa khung đã commit. Chưa auto-scroll/ghép ngang.

Tích hợp logo tím gọn đã chốt vào header/exe/tray/installer; SVG/PNG/ICO nhiều cỡ. Bổ sung thiết lập giao diện Sáng/Tối/Theo hệ thống và màu nhấn tự chọn áp dụng ngay, lưu LocalAppData từng tài khoản/máy, có backup cập nhật. Không đổi màu ảnh/chú thích. Giữ schema12 và toàn bộ công cụ ảnh trước đó. Nghiệm thu Windows thật còn chờ; xem docs/VALIDATION.md và docs/WINDOWS-TEST-V1.14.md.

Bản tiếp theo V1.15: auto-scroll theo danh sách app hỗ trợ, nghiên cứu ghép ngang/fixed-region/window/client/menu và phát hiện nội dung động; triển khai theo nhóm nhỏ sau nghiệm thu1.14.

---

# Captovix V1.13.0 — Image Composition & Templates

Ứng dụng Windows x64, C#/.NET 10 WPF, một Editor mở phóng to. Bàn giao đúng hai ZIP: Update-Packages (có setup) và Source. Chưa đăng Release/GitHub. Quay màn hình giữ chức năng hiện có; phát triển chuyên sâu về ảnh.

## Nội dung mới

- Menu **Bố cục**: tạo canvas, thêm nhiều ảnh, sắp xếp theo mẫu trực quan, đổi chỗ hai ảnh; tiêu đề, caption và ghi chú cuối là đối tượng chữ sửa được.
- Từng ảnh là lớp riêng: Fit/Fill giữ tỷ lệ, kéo nội dung ảnh trong ô, đổi vị trí bằng thanh trượt, cắt theo pixel nguồn, thay ảnh, viền/bo góc/bóng. Ảnh nguồn nhúng trong tài liệu.
- `.cvtemplate`: lưu bố cục và đối tượng vector, bỏ ảnh công việc và thay chữ bằng placeholder. `.cvproject`: một gói ZIP chứa ảnh gốc và tài liệu với đủ lớp ảnh; mở độc lập file nguồn ngoài máy.
- Callout mới có đệm 30 px, tự mở rộng chiều ngang hợp lý, giới hạn chiều cao theo canvas và clip chữ trong vùng nội dung. Mẫu preview dùng chữ/đệm/cỡ mẫu riêng; dữ liệu yêu thích giữ nguyên style thực tế.
- Màu chữ có palette và hộp chọn màu Windows. Ctrl+B/I/U cho đậm/nghiêng/gạch chân cả đối tượng, cả khi nhập inline; chưa có rich text từng từ.
- Icon vector thống nhất cho công cụ/chụp/quay/copy/xuất/thư viện/layers; chụp teal, quay đỏ dịu. Phím tắt và chụp/quay là hai module riêng. Ẩn cấu hình và địa chỉ nguồn cập nhật; trình bày thuộc tính theo công cụ trước, layers/hình học/toàn ảnh sau.
- Bỏ nút và phím P tạo Pixel hóa; giữ render dữ liệu Pixelate cũ. Vùng pixel chữ nhật/ellipse/lasso vẫn dùng để cắt/copy/tô/xóa pixel ảnh nền.

Hướng dẫn: [IMAGE-COMPOSITION-TEMPLATES.md](docs/IMAGE-COMPOSITION-TEMPLATES.md). Bản này build và kiểm tra logic trên Linux; WPF chỉ cross-build. Chưa chạy giao diện, clipboard, hộp chọn màu native, capture/DPI, cài đặt hoặc update trên Windows thật.

## Bản tiếp theo

**V1.14 — Scrolling Capture Foundation:** chọn vùng rồi cuộn một chiều; ghi các khung có overlap, ghép ảnh dài, xem điểm nối và báo vùng không chắc. Có hủy thao tác/giới hạn bộ nhớ và giữ các khung nguồn nếu ghép thất bại. Chưa triển khai; cần thử riêng theo từng ứng dụng Windows.


---

# V1.12.0 — Selection & Pixel Editing (23/10/2026)

Editor dành thêm chỗ cho ảnh: menu module/modal cấu hình, inspector chỉ công cụ, nút gọn, thư viện dọc/ngang dưới và icon ở đáy. Gộp mũi tên/đường và khung/hình kín, bỏ UI tạo Cover nhưng giữ dữ liệu cũ. Callout gallery10 mẫu gồm viên thuốc, thân/pivot ổn định cho callout mới và đuôi kéo tự do. Chọn rectangle/ellipse/lasso; copy vùng hiển thị, cắt/tô/xóa nền, dán clipboard thành lớp ảnh editable, Undo/Redo. Schema11 đọc1–10, backup .pre-v1.12.bak; giữ mẫu sao1111 qua migration hash. Chi tiết/giới hạn/nghiệm thu trong docs/SELECTION-PIXEL-EDITOR.md và docs/VALIDATION.md.

# V1.11.1 — Update Reliability

Đọc releases.win.json từ URL stable được gắn sẵn, bỏ phụ thuộc GitHubSource giới hạn 10 Releases; chẩn đoán phiên bản/URL nguồn, lỗi feed/mạng/hash rõ ràng. Thêm xóa/khôi phục nội dung, Ctrl+wheel zoom, wheel dọc và Shift+wheel ngang; viền chụp không phá hủy, thuộc tính tự áp dụng, palette trong suốt, gallery mẫu có sao và bóng 9 hướng. Schema 10 đọc 1–9, backup trước khi ghi. Xem docs/UPDATE-RELIABILITY.md và docs/VALIDATION.md về phạm vi kiểm chứng, giới hạn và hướng dẫn phát hành.

# V1.11.0 — Focus & Privacy (22/10/2026)

- **Kính phóng đại (M):** kéo vùng đặt kính tại chỗ, chỉnh 1.25–8×, chữ nhật hoặc ellipse, màu/dày viền và bật/tắt viền. Di chuyển kính để lấy vùng khác; hình ảnh bên trong giữ hướng của ảnh. Kính lấy ảnh gốc đã áp các vùng che đang bật, không lấy chữ/shape chú thích, không tạo thêm chi tiết thật.
- **Spotlight (O):** giữ sáng vùng chọn, làm tối phần ngoài; chữ nhật/ellipse, màu và độ tối 0–100%. Nhiều vùng sáng được hợp lại; màu/độ tối lấy Spotlight trên cùng. Lớp nền spotlight luôn dưới các chú thích; dùng menu chuột phải sắp xếp các spotlight để chọn style chung.
- **Highlight (H):** chữ nhật/ellipse, màu và độ đậm 0–100%. Dữ liệu highlight cũ giữ nền chữ nhật và alpha 78 như trước.
- **Blur / Pixel / Che kín:** chọn hình vùng chữ nhật/ellipse; xoay, resize, đổi màu che và mức Blur/Pixel 4–64. Chuyển ba kiểu che tại cùng vùng bằng Áp dụng. Che kín luôn đặc 100%, độc lập opacity chung. Lớp che đặc luôn phủ trên chú thích, kính và filter.
- Bộ lọc làm việc theo tile, có halo cho blur và cache giới hạn. Khi có che đặc, mẫu filter được tô che trước khi tính blur/pixel để tránh kéo màu từ phần đã che ra ngoài. Mẫu ngoài crop được thay trắng; kính không lấy ảnh ngoài crop. Cache cập nhật khi vị trí/màu/trạng thái che hoặc crop thay đổi.
- Các hiệu ứng là đối tượng sửa lại được: kéo/resize/xoay, nhóm, khóa/ẩn, Undo/Redo, Ctrl+C/V, Ctrl+D, lưu/mở lại. Bấm trực tiếp để chỉnh từ công cụ vẽ khác; Alt+kéo để tạo mới chồng lên vùng đã có.
- 5 preset mới: Kính rõ 2x, Spotlight nền tối, Che thông tin, Làm mờ mạnh, Highlight vàng. Có thể lưu preset riêng gồm hình vùng, mức zoom/opacity, màu và mức xử lý. Chọn preset → Chỉ dùng preset cho nét mới để chuyển công cụ tương ứng; Áp dụng trên đối tượng giữ loại công cụ hiện tại.
- **Kiểm tra trước khi chia sẻ:** thống kê vùng che đặc/filter/đang ẩn và vùng vượt ảnh/crop; nút kiểm tra ở Actual Pixels ngay trong Editor. Copy/xuất nhắc trạng thái che. PNG/JPEG xuất là ảnh đã ghép; thư viện/dự án vẫn giữ ảnh gốc. Blur/pixel và highlight không thay thế che đặc khi cần giấu dữ liệu.

Editor schema 9 đọc 1–8; backup sidecar .pre-v1.11.bak trước ghi lần đầu. Giữ backup cũ, Media, workflow và URL update; không đổi đường dẫn cài. Giữ các bản vá startup, chuyển ảnh và capture pixel lẻ. Build và logic được kiểm tra trên Linux; WPF/cài/update cần nghiệm thu Windows.

# V1.10.0 — Text & Callout Studio (22/10/2026)

- Chọn đối tượng → Ctrl+C → Ctrl+V để dán một bản vẫn sửa được. Giữ nhóm, kiểu chữ và liên kết connector nội bộ; tạo ID mới, tháo liên kết ra ngoài lựa chọn. Dán sang ảnh khác giữ số pixel, không tự co giãn. Mỗi lần dán là một Undo. Ctrl+D tiếp tục nhân bản như cũ.
- Khi không chọn đối tượng, Ctrl+C vẫn copy ảnh đã chỉnh. Ctrl+Shift+C luôn copy ảnh đã chỉnh. Khi đang gõ trong ô chữ, Ctrl+C/V vẫn là copy/paste văn bản. Bản này chưa nhận bitmap ngoài Captovix thành image layer.
- Chọn shape đã tô → bỏ dấu Tô nền hoặc bấm Không tô nền: bỏ nền ngay, giữ màu để bật lại. Nền Text/Callout có checkbox riêng trong Text & Callout Studio. Ctrl+Z phục hồi được.
- Chọn/di chuyển/resize trực tiếp từ mọi công cụ vẽ (ngoại trừ Crop). Bấm đường viền khung trong suốt, hoặc phần nền khung đã tô, rồi kéo; inspector tự theo loại đối tượng được chọn. Kéo vùng trống để vẽ tiếp bằng công cụ đang bật; giữ Alt khi kéo để vẽ đối tượng mới chồng lên đối tượng cũ. Vẫn có Chọn (V), chọn nhiều, Layers và menu chuột phải.
- Text: font cài trong Windows, đậm/nghiêng/gạch chân, màu chữ riêng, căn trái/giữa/phải khi có chiều rộng, padding, wrap theo chiều rộng nhập và giãn dòng. Mỗi đối tượng dùng một kiểu chữ; chưa có rich text theo từng từ.
- Callout: chữ nhật có đuôi, bo góc có đuôi, ellipse có đuôi, chữ nhật không đuôi và bo góc không đuôi. Màu chữ/nền/viền độc lập; căn dọc trên/giữa/dưới, tự tăng chiều cao khi bật Giãn hộp theo chữ. Tay nắm đuôi kế thừa, kéo được khi callout đã chọn.
- Step: tròn, vuông bo góc, hình thoi; tự tăng số khi tạo; đánh lại số các bước được chọn, hoặc các bước hiện và không khóa nếu không chọn. Giới hạn 1–9999; giữ nhóm/khóa và Undo.
- Preset riêng lưu thêm kiểu chữ/callout/step; các preset cũ vẫn đọc được. Thiếu font trên máy khác thì xem/xuất dùng Segoe UI và hiện cảnh báo. Double-click chữ/callout hoặc F2 để sửa trên ảnh; Enter xuống dòng, Ctrl+Enter hoàn tất, Esc hủy. IME cần nghiệm thu trên Windows.

Schema 8; backup sidecar .pre-v1.10.bak trước lần lưu đầu từ schema cũ. Giữ workflow, ảnh gốc, URL update và root cài. 830 kiểm tra logic/static đã đạt; WPF chỉ cross-build tại nơi build Linux, chờ nghiệm thu Windows.

# V1.9.0 — Arrows & Connectors (22/10/2026)

- Route Straight/Elbow/Curve trên Arrow/Line; control point, anchor target ID/U/V, cập nhật endpoint trước commit; clone/remap/detach/delete/Undo.
- Schema 7 và backup sidecar .pre-v1.9.bak; giữ capture/image fidelity và startup readiness guard V1.8.1.
- Hỗ trợ OEM punctuation gồm Ctrl+OEM_3 (`), label chính xác, validate ngay lúc nhập; vẫn đăng ký global hotkey qua RegisterHotKey/MOD_NOREPEAT, xung đột phục hồi cấu hình cũ.
- Action Element/SourceMode 3, UIA MTA helper process có timeout, highlight click-through, con lăn cha/con, giữ-kéo fallback; dọn hook/process/overlay khi hoàn tất/hủy.
- Đưa nhóm, sắp xếp lớp, căn và chia đều khỏi inspector vào menu chuột phải canvas; khóa/ẩn/copy/export tại menu.
- Bổ sung ConnectorChecks, ConnectorWindowsChecks và checklist Windows. Chỉ công bố test logic và cross-build; nghiệm thu desktop chưa chạy.

## Lịch sử phiên bản trước

## V1.8.1 — Startup Recovery (22/10/2026)

Sửa NullReferenceException tại SyncProperties khi ComboBox phát SelectionChanged trong InitializeComponent. Callback chỉ chạy sau khi hoàn tất dựng XAML và nối các thành phần Editor; chặn đồng bộ lồng nhau. Giữ schema 6, ảnh gốc, sidecar, preset, URL update và đường dẫn cài.

Build và kiểm tra trên Linux; bộ kiểm tra WPF được cross-build, chưa thực thi Windows. Bộ hồi quy Windows gồm khởi tạo MainWindow thật, callback trước trạng thái sẵn sàng và đổi kiểu sau khởi tạo. Cần kiểm tra mở app trên máy Windows của Boss.

# Changelog

## 1.8.0 — Object Control & Layers

- Layers tích hợp Editor: thứ tự hiển thị theo paint order, chọn đối tượng phía dưới, chọn nhiều, tên riêng, khóa và ẩn. Khóa/ẩn theo nhóm; renderer bỏ layer ẩn, giữ layer khóa khi xuất. Lớp bảo vệ vẫn luôn phủ trên chú thích.
- Annotation thêm Name/Locked/Visible/Rotation. Schema 6 đọc 1–5; backup `.pre-v1.8.bak` trước lần ghi đè sidecar cũ đầu tiên; từ chối schema tương lai.
- RotateTransform dùng chung pivot local cho vẽ/hit/bounds/handles; input gốc lấy inverse transform của ảnh. Xoay riêng/nhóm bằng vòng tròn, Shift 15°. Resize một hình đã xoay giữ điểm neo; nhóm xoay dùng tỷ lệ đồng đều.
- Vùng che/làm mờ/pixel hóa xoay clip hình ở vị trí đích; lấy pixel gốc ở ROI đích thay vì xoay bitmap đã lọc. Giữ Cover opaque và lớp bảo vệ.
- Inspector X/Y/W/H/góc số, giữ tỷ lệ, kiểm tra đầu vào trước commit, Undo/Redo trọn thao tác. Label tách khung trước xoay của một hình với khung bao/góc tương đối của nhóm.
- Snap mép/tâm với crop và đối tượng hiện, guide riêng không xuất vào ảnh; Ctrl tạm bỏ snap, toggle lưu AppSettings. Lọc đối tượng chọn bằng một lượt theo nhóm để tránh tìm kiếm lặp.
- Giữ chuyển ảnh V1.5.1, capture GDI V1.6.1, shape/style/preset V1.7 và installer/updater cùng dữ liệu gốc.
- Thêm ObjectControlChecks và ObjectWindowsChecks; cập nhật bộ cũ theo schema 6 và script Windows build. Windows runtime chưa được nghiệm thu tại nơi build.

Tiếp theo **V1.9 — Arrows, Lines & Connectors**: đường gấp khúc/cong và điểm điều khiển; connector gắn shape theo ID, đi theo khi chuyển/resize, duy trì liên kết qua lưu, duplicate, delete và Undo.

## 1.7.0 — Shapes & Stroke Studio

- 9 hình kín; đường thẳng; mũi tên một/hai đầu, 4 kiểu đầu và cỡ đầu tùy chỉnh.
- Màu HEX viền/nền độc lập, opacity, không viền/không nền, độ dày, solid/dash/dot/dash-dot. Bo góc và sao cấu hình được.
- Hình học dùng chung cho render/hit test, chọn vùng tô hoặc mép hình rỗng, kéo/resize/sao chép giữ style và ID phù hợp; Shift khóa góc ngay cả tại mép ảnh.
- Đổi loại hình kín tại inspector; áp dụng nhiều thuộc tính thành một Undo/Redo. Nhấn Enter áp dụng, Esc đồng bộ lại trường. Nét gạch/chấm được giữ khi export/reopen.
- 3 preset shape mới và lưu preset đủ trường. Nút chỉ dùng preset cho nét mới tách khỏi áp dụng lên lựa chọn hiện có. Preset cũ tiếp tục tải được.
- Schema 5 đọc 1–4; style mặc định tương thích hình cũ. Lần lưu sidecar schema cũ đầu tiên giữ `.pre-v1.7.bak` trước khi ghi. Không hỗ trợ app cũ ghi file schema 5.
- Giữ V1.6.1 capture GDI đúng pixel, V1.5.1 unload và toàn bộ config updater/installer/tray. Không thêm editor video.
- Thêm ShapeStudioChecks, ShapeWindowsChecks và UI inspector/migration cases vào EditorWindowsChecks; script Build-Windows chạy các bộ mới. Linux chạy logic; Windows runtime chưa được nghiệm thu tại nơi build.

Tiếp theo **V1.8 — Object Control & Layers**: layer panel, khóa/ẩn/đổi tên/thứ tự, X/Y/W/H/góc xoay, cải thiện multi-select/snap/group/căn chỉnh và chia đều.

## V1.6.1 — Exact Desktop Capture Hotfix

- Sửa chụp vùng/toàn màn hình bị engine hạ kích thước lẻ về chẵn (ví dụ 1467×386 → 1466×386).
- Dùng Win32 GDI BitBlt cho ảnh desktop SDR, DIB top-down 32-bit đúng pixel; không resize, không bù cột giả. Ghép vùng các màn hình theo tọa độ vật lý, phần không có màn hình để đen.
- Giữ tùy chọn con trỏ với hotspot; giải phóng DC, bitmap, icon sau chụp. Ảnh PNG opaque SDR8, metadata 96 DPI không thay số pixel.
- Chụp cửa sổ riêng và quay video tiếp tục backend hiện có. HDR/màu rộng và nội dung được bảo vệ chưa được hỗ trợ/đánh giá.
- Chỉ hiển thị hướng dẫn runtime khi gặp lỗi nạp DLL; lỗi khác chỉ rõ errors.log.
- Probe Windows thêm ảnh rộng lẻ, cao lẻ, cả hai lẻ, 1×1 và hồi quy 1467×386. Chưa chạy probe trên Windows trong môi trường build.
- Không đổi schema, URL update, dữ liệu hay đường dẫn cài đặt. Giữ sửa lỗi chuyển ảnh V1.5.1.

## 1.6.0 — Image Fidelity

- Readout riêng cho kích thước gốc, đầu ra xuất/copy và zoom vật lý. Actual Pixels giữ 1 pixel đầu ra = 1 pixel màn hình, bù scale Windows.
- Giữ mức zoom vật lý khi Editor chuyển màn hình; fit tự tính lại. Zoom không tham gia công thức file đầu ra.
- Crop/xoay 90°/lật và canvas pixel-aligned không resize dùng nhánh copy BGRA8 trực tiếp; chỉ chú thích/hiệu ứng được render rồi ghép riêng. Vùng không chỉnh không phải đi qua resampling WPF.
- Canh nguyên pixel cho canvas dư lẻ để tránh đặt ảnh tại nửa pixel. Bố cục giữa có thể lệch nửa pixel so với cách nội suy trước; không thay tọa độ đối tượng lưu trong tài liệu.
- Chuẩn hóa output PNG và clipboard PNG/Bitmap ở 96 DPI metadata, giữ số pixel. Xuất file qua temporary/validate kích thước rồi thay đích. JPEG vẫn lossy 95%, ghép trắng phần trong suốt, không tự đổi W×H.
- Ảnh chụp màn hình/vùng có kích thước trả về khác pixel yêu cầu sẽ báo lỗi và giữ pending, không tự resize để che sai lệch. Chụp cửa sổ ghi nhận native extent thực tế thay vì kéo giãn theo invisible frame bounds.
- Thêm kiểm chứng SDR tương tác trên Windows: renderer/PNG/clipboard, mẫu pixel và kích thước full-display trên từng monitor; ghi JSON theo DPI và tọa độ thật. Có script tổng hợp scale còn thiếu.
- Giữ bản vá chuyển ảnh V1.5.1, dữ liệu ảnh/chú thích schema 4 và cấu hình cập nhật/tray/hotkey.

Trạng thái: đã triển khai source/build và 525 kiểm tra logic/tĩnh; 9 kiểm tra SDK gói thật sau đóng gói. Bộ WPF/PNG đã biên dịch, chưa thực thi trên Windows trong môi trường Linux này. Chưa nghiệm thu matrix capture thật 100/125/150/175/200%, monitor âm hoặc kéo Editor giữa monitor. HDR/wide gamut và giữ 16-bit/profile màu không thuộc cam kết SDR8 của bản này.

Tiếp theo V1.7 — Shapes & Stroke Studio: nhiều shape (rectangle/bo góc/ellipse/tam giác/đa giác/ngôi sao...), màu viền và nền riêng, opacity, độ dày, nét liền/đứt/chấm/đứt-chấm, preset; vẽ xong vẫn chọn/kéo/resize và lưu sửa lại được. V1.8 sau đó nâng điều khiển đối tượng và bảng layer.


## 1.5.1 — Editor Switch Fix

- Sửa NullReferenceException khi chuyển giữa các ảnh: UnloadEditor đã xóa canvas nhưng callback VisualChanged còn đọc Layout qua phiên chỉnh sửa cũ.
- Dọn session trước khi canvas phát sự kiện unload; callback chỉ đọc layout khi session/canvas khớp và có ảnh, xóa thông tin kích thước/zoom khi rỗng.
- Bảng thuộc tính ảnh cũng xử lý trạng thái thiếu layout, không dùng dữ liệu ảnh cũ trong lúc chuyển.
- Bổ sung 11 ca hồi quy WPF: surface trống với session cũ, unload lặp, chuyển ảnh khác kích thước, lưu chỉnh sửa, tách sự kiện session cũ, 30 vòng đổi ảnh, bỏ chọn và chọn ảnh mới.
- Giữ ảnh gốc, sidecar schema 4, thư viện, cấu hình workflow và nguồn cập nhật GitHub. Không reset dữ liệu để sửa lỗi.

Phạm vi kiểm chứng: build/publish Windows x64 và test logic/SDK có thể chạy tại Linux. Bộ hồi quy WPF đã bổ sung và biên dịch; chưa thực thi trên Windows thật. Cần nghiệm thu chuyển ảnh theo WINDOWS-TEST-CHECKLIST.md.

Phiên bản kế tiếp: V1.6 — Image Fidelity. Rà capture→file→preview→clipboard→export để giữ đúng pixel nguồn, Actual Pixels, PNG gốc và nhiều màn hình/DPI. V1.7 tiếp tục Shapes & Stroke Studio; V1.8 nâng Object Control & Layers.


## 1.5.0 — Capture Workflow

- Khay hệ thống: chụp ảnh, preset, mở Editor, hủy/dừng phiên, thoát hoàn toàn.
- Cấu hình 5 phím tắt ảnh trong Editor; tắt/đổi phím, phát hiện trùng và báo xung đột Windows, khôi phục phím trước khi đổi thất bại.
- Chụp lại vùng thành công gần nhất với kiểm tra màn hình, tọa độ vật lý, độ phân giải và DPI.
- Preset chụp riêng: chế độ, màn hình, trễ, con trỏ, mở Editor/lưu nền, tự copy. Cửa sổ luôn chọn lại khi chụp ảnh.
- Lưu ảnh trước, tự copy tùy chọn, thông báo mở đúng ảnh; clipboard lỗi không làm mất file. Đếm ngược và chọn vùng/cửa sổ có thể hủy bằng Esc.
- Đóng xuống khay / khởi động Windows tùy chọn, mặc định tắt. Mở thủ công vào Editor phóng to; không tự chụp khi đăng nhập.
- Lưu cấu hình atomic riêng, bảo vệ file hỏng/schema mới và sao lưu trước khôi phục. Metadata workflow được đưa vào backup trước update.
- Giữ các tính năng chỉnh ảnh V1.4, chọn đường dẫn cài đặt và nguồn cập nhật GitHub mặc định.

Kiểm thử UI Windows/tray, chụp thật, clipboard, nhiều màn hình/DPI, autorun và cài đặt/update cần chạy trên Windows theo docs/WINDOWS-TEST-CHECKLIST.md. Không xem cross-build là đã kiểm thử máy thật.

Phiên bản tiếp theo dự kiến: V1.6 — Library & Export. Video nâng cao dành cho V2.0.

# Captovix changelog

## 1.4.0 — Image Transform & Styling

- Xoay trái/phải 90°, lật ngang/dọc theo ảnh đang hiển thị.
- Resize ảnh theo pixel, giữ tỷ lệ hoặc nhập riêng rộng/cao; khôi phục cỡ gốc theo crop/xoay hiện tại.
- Canvas riêng với 9 vị trí đặt ảnh. Canvas nhỏ hơn ảnh cắt phần vượt khung, không xóa dữ liệu gốc.
- Thêm khoảng đệm, viền, màu nền hoặc nền trong suốt; bóng đổ mềm của khung với độ mềm/lệch/đậm.
- Mẫu khung Sáng/Tối/Bỏ kiểu. Xem kích thước đầu ra gồm canvas, đệm, viền và bóng.
- Ảnh/chú thích gốc giữ nguyên tọa độ. Preview, thao tác chuột, inline text, thumbnail, copy và xuất ảnh dùng cùng phép biến đổi; hỗ trợ undo/redo và lưu mở lại.
- PNG/clipboard giữ trong suốt; JPEG ghép nền trắng ở phần trong suốt. Giới hạn đầu ra 32.768 px mỗi chiều và 100 triệu pixel.
- Schema 4 mở chú thích schema 1/2/3; không mở ngược file đã lưu bằng app V1.3 trở xuống. Giữ cơ chế backup trước cập nhật.

Mở ảnh → bảng bên phải → Ảnh & khung · V1.4. Các trường số/màu áp dụng khi bấm nút Áp dụng của từng nhóm; xoay/lật và preset áp dụng ngay. Bộ cài giữ chức năng chọn thư mục (kể cả D:\Captovix\Captovix), nguồn GitHub mặc định và Editor mở phóng to. Video giữ nguyên tính năng hiện tại.

Đã chạy kiểm tra logic/tĩnh/SDK và biên dịch Windows x64; chưa chạy UI/render/bộ cài trên Windows thật trong môi trường đóng gói.


## 1.3.0 — Advanced Annotation

- Sửa chữ trực tiếp trên ảnh: công cụ T, double-click chữ/callout hoặc F2. Enter xuống dòng, Ctrl+Enter hoàn tất, Escape hủy phần đang nhập. Có thể sửa qua bảng thuộc tính.
- Callout (Q): kéo hộp chú thích, nhập chữ trực tiếp, kéo tay nắm tại đầu đuôi để đổi điểm chỉ. Chữ tự xuống dòng trong hộp, nội dung vượt chiều cao được cắt trong ảnh xuất; kéo tăng hộp để hiển thị đủ.
- Vẽ tự do (F): nét nhiều điểm, chọn/kéo/đổi kích thước, đổi màu/nét, lưu/mở lại và xuất PNG/JPEG.
- Nhân bản Ctrl+D; nhóm Ctrl+G; bỏ nhóm Ctrl+Shift+G. Chọn một thành viên sẽ chọn cả nhóm; nhóm phẳng, gộp các nhóm đã chọn; bản sao có mã nhóm độc lập.
- Đưa lên/xuống một lớp hoặc trước/sau cùng. Ctrl+] / Ctrl+[; thêm Shift để tới đầu/cuối. Thứ tự được lưu và hoàn tác; vùng che vẫn phủ trên chú thích.
- Chia đều khoảng cách ngang/dọc của ít nhất ba đối tượng hoặc nhóm. Giữ hai đầu, tính khoảng trống theo kích thước thực tế và giữ nguyên bố cục bên trong nhóm.
- Undo/redo cho các thao tác mới; giữ tối đa 100 bước và giới hạn bộ nhớ lịch sử. Định dạng chú thích schema 3 đọc được schema 1/2; bản cũ không đọc được file mới đã lưu.

Giữ cơ chế cập nhật có URL mặc định, bộ cài chọn ổ/thư mục đã sửa V1.2.1 và dữ liệu ảnh/video tại nơi hiện có. Mở Editor mặc định phóng to. Video giữ chức năng hiện tại.

Đã chạy kiểm tra logic/SDK, cross-build WPF và biên dịch NSIS. Chưa chạy giao diện, bộ cài hoặc thiết bị chụp/quay trên Windows thật trong môi trường đóng gói.


## 1.2.1 — Installer Path Fix

- Sửa lỗi bộ cài từ chối thư mục mới chưa tồn tại, bao gồm D:\Captovix\Captovix.
- Thay lệnh NSIS GetFullPathName trong bước xác thực bằng Win32 GetFullPathNameW, chuẩn hóa trước khi tạo thư mục và không yêu cầu thư mục tồn tại.
- Giữ kiểm tra ổ cục bộ cố định, quyền ghi, thư mục trống và vùng dữ liệu riêng. Tách thông báo để thấy đúng nguyên nhân nếu một kiểm tra khác không đạt.
- Giữ các tính năng Image Editor UX của V1.2.0, nguồn cập nhật và AppId/schema dữ liệu.

Để chuyển bản đã cài từ C sang D: đóng app, gỡ Captovix qua Windows Settings, chạy Setup này và chọn D:\Captovix\Captovix. Thư viện ảnh/video giữ tại vị trí hiện có. Nếu V1.2.0 bị chặn trước khi cài, chỉ cần đóng bộ cài cũ và chạy bản này.

Chưa thực thi trên Windows thật trong môi trường đóng gói. Kèm harness NSIS kiểm tra chuẩn hóa thư mục mới và checklist Windows.


## 1.2.0 — Image Editor UX (2026-10-20)

- Zoom bằng con lăn quanh vị trí chuột; nút +/- và 100% giữ tâm khung nhìn.
- Giữ Space + kéo chuột trái để di chuyển khung nhìn; nhả Space/chuột hoặc mất focus để kết thúc. Escape trong lúc pan khôi phục vị trí trước khi kéo.
- Crop chờ xác nhận: kéo vùng, sửa bằng 8 tay nắm, di chuyển vùng, nhập X/Y/rộng/cao theo pixel ảnh gốc; Enter/Áp dụng để hoàn tất, Escape để hủy. Phím mũi tên tinh chỉnh 1 px hoặc Shift 10 px.
- Shift khi vẽ crop tạo hình vuông; Shift kéo góc giữ tỷ lệ trong giới hạn làm tròn pixel. Ctrl+A khi crop chọn toàn khung hiện tại.
- Alt+click luân phiên chọn các đối tượng chồng lấp. Giữ thao tác nhóm, bắt dính, đổi kích thước và undo/redo V1.1.
- Setup có bước chọn thư mục cài, ví dụ D:\Apps\Captovix; kiểm tra thư mục trống/quyền ghi/ổ cục bộ và giữ thư mục dữ liệu riêng.
- Nhận diện đường dẫn đã cài. Để chuyển bản cài hiện có sang ổ khác: gỡ app trong Windows Settings rồi chạy lại Setup, chọn thư mục mới; ảnh/video vẫn giữ nguyên. Cập nhật trong app giữ đường dẫn cũ.

Giữ nguồn GitHub mặc định, AppId và dữ liệu/schema 2. Copy/Xuất ảnh áp dụng vùng crop đang chờ; hủy hộp thoại Xuất không áp dụng crop. Video giữ chức năng hiện tại. Chưa chạy UI/bộ cài/updater trên Windows thật trong môi trường đóng gói.

## 1.1.0 — Image Annotation (2026-10-20)

- Thêm Làm mờ, Pixel hóa và Che kín; điều chỉnh mức xử lý 4–64 px, giữ ảnh gốc và xuất PNG/JPEG đã xử lý.
- 6 preset màu/nét/chữ có sẵn; lưu tối đa 20 preset riêng và xóa mẫu riêng.
- Chọn nhiều bằng Shift/Ctrl+click, kéo vùng trống hoặc Ctrl+A; di chuyển, căn chỉnh, đổi kiểu và xóa cùng lúc.
- Căn trái/giữa/phải/trên/dưới: một đối tượng căn theo ảnh/crop, nhiều đối tượng căn theo nhóm.
- Tay nắm ở 8 hướng; góc cho chữ/số để đổi cỡ theo tỷ lệ; mũi tên giữ hai đầu. Giữ Shift để khóa hướng/tỷ lệ; bắt dính mép/tâm ảnh khi kéo, Ctrl để bỏ bắt dính.
- Click chọn không tạo thay đổi ngoài ý muốn; Escape/hủy capture chuột khôi phục thao tác; outline khung/ellipse có hit-test theo nét.
- Giữ undo/redo, lưu tự động và nguồn cập nhật GitHub mặc định. Backup cập nhật bổ sung preset riêng.

Chú thích V1.0 được đọc và nâng lên schema 2 khi lưu. Giữ bản backup trước cập nhật; các bản V1.0 không đọc được tài liệu mới. Vùng che kín luôn phủ trên các hiệu ứng/chú thích khác. Làm mờ/pixel hóa không thay thế che kín khi cần ẩn hoàn toàn thông tin. Chia sẻ ảnh đã xuất; ảnh gốc và Edits vẫn giữ nội dung để tiếp tục chỉnh sửa.

Chưa chạy WPF/Setup/Update.exe trên Windows thật trong môi trường đóng gói. Xem docs/WINDOWS-TEST-CHECKLIST.md.

## 1.0.3 — Default Update Source (2026-10-20)

- Gắn sẵn nguồn GitHub Releases công khai: https://github.com/huyvd250797/captovix-releases.
- Cài mới rồi chọn Cập nhật → Kiểm tra cập nhật, không cần nhập URL.
- Giữ cấu hình nguồn riêng đã lưu, lựa chọn tự kiểm tra và lịch sử ảnh/video/chú thích.
- Duy trì sửa lỗi EXE_ARGS và bộ cài có tiến trình/log từ V1.0.2.
- Tăng phiên bản gói lên 1.0.3 để máy V1.0.2 nhận qua updater sau khi chủ app Publish release mới.

Máy V1.0.2 đã lưu đúng nguồn có thể cập nhật trong app. Cũng có thể chạy Setup V1.0.3 trực tiếp. Nguồn rỗng đã lưu vẫn được giữ theo lựa chọn cũ; trường hợp này nhập URL một lần. Portable chỉ kiểm tra/mở trang tải thủ công. Chưa thực thi cài đặt và cập nhật trên Windows thật trong môi trường đóng gói.

## 1.0.2 — Installer Recovery (2026-10-20)

- Sửa lỗi bộ cài V1.0.1 bị đứng ở CoreSetup.exe: bỏ tham số truyền ứng dụng sau `--` gây panic `EXE_ARGS` trong Velopack 1.2.161 (issue #1039).
- Hiển thị cửa sổ tiến trình/lỗi của bộ cài lõi thay vì chạy silent; bộ cài ngoài hiển thị 3 giai đoạn.
- Log chi tiết cố định tại `%LOCALAPPDATA%\HuyVo\Captovix\Logs\setup-core.log`, giữ lần trước thành `.previous`.
- Nút hoàn tất kiểm tra app đã mở để tránh chạy thêm bản thứ hai.
- Giữ cơ chế chỉ gỡ NSIS V1.0 sau khi cài bản mới thành công, dữ liệu ngoài thư mục cài được giữ.
- Thêm kiểm tra hồi quy hợp đồng tham số/lệnh cài; giữ các chức năng updater và Unified Editor của V1.0.1.

Có thể cài trực tiếp V1.0.0 → V1.0.2, không cần cài V1.0.1 trước. Nguồn phát hành mặc định vẫn trống. Cần kiểm tra trên Windows thực tế trước khi triển khai rộng.


# Captovix changelog

## 1.0.1 — Update Foundation (2026-10-19)

- Kiểm tra phiên bản mới ngay trong Editor; tự kiểm tra mỗi 24 giờ hoặc bấm thủ công.
- Nguồn GitHub Releases công khai hoặc thư mục HTTPS, có thể cấu hình trong ứng dụng.
- Hiển thị phiên bản mới và nội dung thay đổi; tải nền, tiến trình, hủy, thử lại và hoãn.
- Bản cài đặt: khởi động lại để áp dụng cập nhật bằng Velopack.
- Lưu chú thích, cấu hình và thư viện; sao lưu metadata/Edits trước khi áp dụng. Chặn cập nhật khi đang chụp/quay hoặc lưu lỗi.
- Kiểm tra AppId, phiên bản stable tăng dần, kích thước và SHA-256 gói.
- Bộ cài đầu tiên chuyển từ NSIS V1.0 sang Velopack; dữ liệu nằm riêng và được giữ lại.
- Portable chỉ kiểm tra và mở nguồn tải thủ công. App tiếp tục mở ở chế độ Maximized.

Nguồn phát hành chưa được thiết lập sẵn. Cấu hình trước khi kiểm tra cập nhật trực tuyến.


# Changelog

## 1.0.0 — Captovix Unified Editor

- Brand, namespace, assembly, app icon, launcher and installer renamed Captovix.
- Editor starts maximized; capture restores the previous window state.
- Editable arrows, rectangles, ellipses, multiline text, translucent highlights and numbered steps.
- Selection, moving, endpoint/corner handles, color, stroke, font/text/number properties and deletion.
- Non-destructive crop with restore-original-frame and undo/redo (100 operations per current image opening).
- Sidecar JSON autosave/reopen with original-content hash checks and invalid-document backup.
- Shared render path for editor and full-resolution PNG/JPEG/clipboard export; thumbnails reflect saved edits.
- V0.1 metadata/settings migration without modifying or duplicating original media.
- Capture countdown cancellation on close; original-file overwrite prevention.
- 38 editor/history/migration checks, 12 geometry checks, Windows-specific renderer/document/UI checks in source.

Builds and core tests do not establish Windows desktop runtime verification. See docs/VALIDATION.md.

## 0.1.0 — Capture Foundation

Initial single-window capture/recording foundation, local media library and Windows packaging.
