# Captovix V1.24.0 — Capture Precision

- Chụp vùng: kéo → chỉnh cạnh/góc nếu cần → Enter hoặc nút máy ảnh là chụp, bỏ bước chọn hành động dư. Nét đứt chuyển động chậm báo vùng chưa chụp. Mũi tên cuộn dọc/ngang nằm ngay vùng chọn, nền trong suốt; video chỉ có xác nhận vùng. Esc/chuột phải hủy.
- Giữ nền capture pixel và kiểm topology/DPI sau hẹn giờ. Chọn phần tử cha/con bằng con lăn, ưu tiên cạnh/góc modal/cửa sổ; vùng cố định/gần đây kế thừa nền trước. Độ tương thích UIA phụ thuộc ứng dụng; không cam kết nhận diện mọi UI.
- Quản lý thư viện: cây năm/tháng/ngày, ảnh dạng ô tự xuống dòng theo chiều rộng; tải100ảnh/lần, nút Xem thêm. Tùy chọn Ẩn ngày tháng không có hình bỏ cả tháng trống. Ctrl/Shift chọn nhiều, mở/xóa/xuất giữ nguyên. Bỏ đường vào màn hình quản lý bộ sưu tập cũ; dữ liệu tag/collection giữ, backup/export còn trong menu.
- Chọn công cụ áp dụng mẫu đã lưu gần nhất trong họ công cụ; chọn mẫu cụ thể vẫn ưu tiên mẫu vừa bấm. Đã lưu mở sẵn; sao trên preview chỉ hiện hover/focus. Sắp xếp công cụ bằng kéo-thả trên thanh công cụ hoặc Hiển thị → Sắp xếp không gian → Sắp xếp công cụ cá nhân; thứ tự giữ trên máy.
- Callout vẽ kéo giữ nguyên thân khi thả, tắt tự co chữ cho đối tượng mới để nhập không đổi thân. Bật Tự co trong thuộc tính nếu muốn. Một click giữ kích thước mặc định. Tay nắm đuôi màu cam, tay nắm thân xanh/trắng dạng tròn. Đuôi vẫn tự do.
- Footer/nút update gọn hơn. Giới thiệu ghi: Ý tưởng, định hướng sản phẩm và thiết kế trải nghiệm: Võ Đức Huy (HuyVo). ©2026HuyVo. All rights reserved.

## Cài/update

Giải nén Update-Packages, đóng app và chạy HuyVo.Captovix-win-Setup.exe, hoặc đăng đủ5asset lên GitHub Release stable/latest tagv1.24.0 để app kiểm tra cập nhật. Không đăng chỉ file ZIP. Source ZIP dùng phát triển, không dùng cài. Không tự publish trong lần bàn giao này. Setup chưa ký số.

## Giới hạn và nghiệm thu

Build/publish Windows x64 tự chứa.NET10.0.13; giữ stack/logo/update source/schema và dữ liệu. Đã chạy7bộ logic/static liên quan,620assertions, và cross-build RefinementWindowsChecks; chưa chạy WPF/Windows desktop. Kiểm nupkg và integrity gói riêng sau đóng gói.

Cần Windows nghiệm thu:1Enter/chuột/nút camera, hủy/countdown/quay, arrows cuộn, nhiều DPI100–200%và tọa độ âm; ảnh/clipboard/PNG đúng pixel; thư viện co giãn/lazy/empty month/multi-select/delete; chọn mẫu gần nhất sau restart/chọn mẫu thủ công; callout thân/đuôi/IME/Undo; caption/taskbar/footer/install-update. Không có screenshot UI chạy thật hoặc tuyên bố đã kiểm thị giác. Margins cũ vẫn theo giới hạn1.23.1, chữ không kéo méo glyph hai chiều.

## Tiếp theo V1.25 — Scrolling Capture Quality

Cải thiện mối nối dọc/ngang, vùng header/footer cố định, phát hiện khung lặp/lỗi, bộ trang mẫu để kiểm không mất/lặp dòng/cột. Ưu tiên bản vá phản hồi Windows1.24 trước.

## Lịch sử phiên bản trước

Bản vá hiện tại **V1.23.1 — Resize & Capture UX**: resize tự do/Shift, vùng chụp chờ xác nhận, pixel selection handles, tự thu margins mới, thông báo update và dọn inspector. Chờ nghiệm thu Windows. Tiếp theo V1.24 Capture Precision: cha/con phần tử, vùng cố định/gần đây, nhiều màn hình và DPI.

# Captovix — Roadmap chuyên sâu về ảnh

Hiện tại: **V1.23.1 — Text & Callout Refinement**, đã xây dựng và kiểm tra logic/build, chờ nghiệm thu Windows. Một Editor native Windows; mở mặc định phóng to. Ưu tiên ảnh, giữ quay màn hình hiện có; editor video chốt riêng sau khi ổn định nhánh ảnh.

| Phiên bản | Mục tiêu | Chức năng chính | Kỹ thuật và dữ liệu | Điều kiện hoàn thành | Phụ thuộc/thứ tự |
| --- | --- | --- | --- | --- | --- |
| V1.16 — OCR & Text Extraction | Lấy chữ từ ảnh | Toàn ảnh/vùng chọn; Việt/Anh; sửa/copy/TXT; Hủy; Crop có khung/Enter; titlebar theme; mở100%pixel | Tesseract5.2.0 + model đóng gói/hash; worker riêng; snapshot đã render; không đổi schema12 | Logic/build đã kiểm; chờ Windows nhận dạng, hủy, Crop, theme, DPI và cài/update | Đã có từ1.16; được kế thừa |
| V1.17 — Assisted Image Editing | Sửa nội dung screenshot có kiểm soát | Thay chữ OCR, gợi ý dòng, preview; nền màu phẳng; tách lớp ảnh; zoom/lens; update popup/fly; overlay capture; menu/controls/media folder/icon repair | Snapshot đã render;2đối tượng/1Undo; không đổi schema12; không dùng AI/cloud inpainting | Source/logic/build đã kiểm, chờ Windows. Nền phức tạp/chữ/font tự khôi phục chưa triển khai | Đã kế thừa; vá lỗi thực tế bằng patch khi cần |
| V1.18 — Image Library & Projects | Quản lý nhiều ảnh/dự án | Tag, collection logic, favorite, tìm/lọc; ảnh gốc/bản chỉnh; .cvlibrary backup/import; sidebar resize; OCR hợp nhất; titlebar, favorite công cụ, element edge/scroll arrows, snug border | Metadata cộng thêm; cache thumbnail; bundle original bytes/sidecar/embedded layers; staging/hash/new IDs/rollback; giữ schema12 | Logic/build đã kiểm; chờ Windows chuyển máy/capture/theme/layout/install-update. Folder vật lý là thư mục media chọn sẵn, collection là nhóm logic; không tự di chuyển media cũ | Đã kế thừa; kiểm thử Windows trước triển khai rộng |
| V1.19 — Batch Export & Documentation | Xuất hàng loạt/tài liệu, thư viện gọn | PNG/JPEG/PDF A4; preset/name preview/watermark/Cancel/Retry/CSV;50recent/archive nhóm ngày-tháng-năm; multi-delete/modal; scrollbar/hover; reorder/soft icons/common properties/sample bounds; seam guide opt-in | STA tuần tự + atomic no-overwrite; PDF losslessRGB; metadata cũ giữ; schema12/2; preference mới | Logic/build và PDF parser/render độc lập đã kiểm, chờ Windows desktop. Chưa WebP/OCR text layer/PDF>100trang/batch>40MP | Đã kế thừa; sau library/composition |
| V1.20 — Image Release & Performance | Tối ưu cache/recovery và UX ảnh | Cache budget/nạp thumbnail nền; thread cache; render diagnostics; last-good; cây lịch sử; caption/icon/sample/footer gọn; nới canvas/ngoài ảnh | Cache16/256MiB,2worker; opt-in schema13, cũ12; không reset media; benchmark pixel-copy tổng hợp4K/8K | Logic/build đã kiểm; **chờ Windows profile/UI/install/update nghiệm thu**, chưa tile renderer/peak RAM/HDR | Đã kế thừa; patch phản hồi Windows khi cần |
| V1.21 — Layout Precision | Chỉnh toàn canvas chính xác | Bốn cạnh bằng số, nền màu/trong suốt; crop cả annotation; vẽ trực tiếp ngoài ảnh/tự nới mép; sửa chữ/callout; cây thư viện ẩn ngày trống | Pixel đầu ra, giữ source/annotation, schema14; không reset media | Logic/build kiểm tại bản này; chờ Windows UX/DPI/render/install/update | Đã xây dựng; patch phản hồi Windows trước |
| V1.22 — Object Editing Precision | Bố trí/chọn/chỉnh đối tượng chính xác | Hit stack/hover; căn3tham chiếu, chia đều theo output; snap/guide; trường số; tỷ lệ cạnh/góc, nhóm/font/callout; ngoài ảnh | Giữ schema12/13/14, thêm settings máy; không resize nguồn/canvas; Undo/Redo | Logic/build và gói kiểm tại bản này; chờ Windows mouse/DPI/render/install/update | Đã xây dựng; patch Windows trước1.23 |
| V1.23 — Text & Callout Refinement | Chữ dễ sửa, workspace gọn | Nhập chữ trong suốt, tự co thân/giữ đích đuôi; exact-edge canvas; Fit, compact metadata, inspector/menu/caption/work area | Giữ schema12/13/14; native IME,1Undo; không tự thu canvas | Logic/build/package kiểm; chờ Windows IME/render/taskbar/DPI/install-update | Đã xây dựng; patch Windows trước1.24 |
| V1.24 — Capture Precision | Chụp vùng chính xác | Chuyển phần tử con/cha; vùng cố định/gần đây; nhiều màn hình | UIA/native fallback theo app | Dự kiến; đo pixel/capture mẫu | Sau1.23 |
| V1.25 — Scrolling Capture Quality | Cải thiện mối nối | Ghép dọc/ngang, khung cố định, phát hiện lặp và cảnh báo | Bộ mẫu cuộn và sửa mối nối | Dự kiến; không mất/lặp dòng/cột trên mẫu hỗ trợ | Sau1.24 |
| V1.26 — Image Composition Studio | Ghép ảnh hướng dẫn | Lưới/hàng/cột, khoảng cách, nền/viền/bóng, tiêu đề/số bước | Lớp ảnh và mẫu | Dự kiến; ghép/export/project khớp | Sau1.25 |
| V1.27 — Library & Project Organization | Quản lý lâu dài | Album/thẻ/tìm kiếm, project/tài nguyên, sao lưu và phát hiện thất lạc | Index/media không mất file | Dự kiến; thư viện lớn và phục hồi | Sau1.26 |
| V1.28 — Export & Documentation Pro | Xuất bộ ảnh/tài liệu | Preset chất lượng, quy tắc đặt tên, PDF/sắp trang/xem trước | Pipeline export và giới hạn RAM | Dự kiến; preview khớp file | Sau1.27 |
| V1.29 — Performance & Large Images | Tối ưu theo đo thực tế | Ảnh lớn/nhiều đối tượng/Undo; đánh giá render từng vùng khi cần | Benchmark Windows | Dự kiến; số liệu trước/sau cùng môi trường | Sau1.28 |
| V2.0 — Image Studio Release | Chốt nhánh ảnh ổn định | Hồi quy capture/chỉnh sửa/project/xuất/update, tài liệu | Ma trận Windows/DPI | Dự kiến; nghiệm thu luồng chính | Sau1.29 |
| Nhánh video — chốt riêng | Biên tập video | Chốt riêng cắt video, chú thích, âm thanh và xuất | Kế hoạch riêng theo nguồn lực/nhu cầu | Boss chọn phạm vi trước xây dựng | Sau nhánh ảnh |

Đã xây dựng trước đó: V1.0–1.5 nền Editor/capture/update/annotation; V1.6 fidelity; V1.7 shape/stroke; V1.8 layers; V1.9 connectors/element capture; V1.10 text/callout; V1.11 privacy; V1.11.1 update/thuộc tính trực quan; V1.12 pixel editing; V1.13 composition/template; V1.14 scroll thủ công/logo/theme; V1.15 tự cuộn/ghép ngang; V1.15.1 capture reliability/dark mode/logo runtime; V1.16 OCR/Crop/titlebar/100%pixel. Xem lịch sử chi tiết trong docs/ROADMAP-HISTORY-pre-v1.16.md. Không suy diễn build/logic thành nghiệm thu Windows.

Chuẩn xuyên suốt: PNG giữ pixel gốc, zoom không đổi file; annotation sửa lại được; không ghi đè schema mới bằng app cũ; source/project và bản ảnh đã che để chia sẻ là hai loại khác nhau. 100%pixel không tạo thêm chi tiết nguồn; HDR/wide gamut và tương thích từng app chụp cuộn cần kiểm riêng.

Mỗi bản bàn giao đúng2file: Update-PackagesZIP (kèmSetup) và SourceZIP. Không tự publish/GitHub/deploy nếu chưa được giao. Phiên bản tiếp theo phải có mô tả nội dung cụ thể.

Nghiên cứu tiếp ngoài phạm vi1.17: inpainting nền phức tạp, tự đoán font, tự nhận dạng mọi UI component và tự gợi ý vùng nhạy cảm. Các phần này cần đánh giá engine/dữ liệu và tiêu chí riêng, không coi là tính năng đã hoàn thành.
