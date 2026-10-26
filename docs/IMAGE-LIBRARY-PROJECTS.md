# Captovix V1.18 — Image Library & Projects

## Thư viện và dự án

Tệp → Quản lý thư viện, hoặc icon thư viện ở chân danh sách. Chọn một ảnh/video để đổi tên, tag cách nhau dấu phẩy, bộ sưu tập và yêu thích. Collection là nhóm logic trong metadata, không phải thao tác di chuyển thư mục. Bộ lọc hỗ trợ tất cả/yêu thích/ảnh/video/có bản chỉnh/chưa có bản chỉnh, kết hợp tag/collection và từ tìm kiếm ở icon kính lúp. Có bản chỉnh nghĩa là có sidecar dự án, kể cả khi sidecar chỉ chứa recipe viền. Yêu thích lên đầu, trong mỗi nhóm mới nhất trước. Bỏ bộ lọc để thấy lại toàn bộ thư viện.

Chọn thư mục media mới bằng icon lưu thư mục đã có từ1.17: không tự chuyển ảnh/video cũ. File metadata vẫn ở `%LOCALAPPDATA%\HuyVo\Captovix\library.json`, Edits và cache thumbnail riêng; không reset dữ liệu cũ. Tệp → Xem ảnh gốc mở bản gốc riêng để đối chiếu với Editor đã chỉnh.

Bố cục → Lưu/Mở dự án `.cvproject` giữ một ảnh nhiều lớp, ảnh nguồn và các lớp ảnh nhúng. `.cvtemplate` là mẫu bố cục. Tệp → Sao lưu thư viện tạo `.cvlibrary` chứa toàn bộ nội dung trong thư viện, kể cả nội dung đang bị ẩn bởi bộ lọc, byte media gốc, JSON chỉnh sửa (bao gồm lớp nhúng), tên/tag/collection/favorite. Không sao lưu Trash, undo history trong RAM, scroll-session nguồn, cấu hình cá nhân/phím tắt/theme, mẫu công cụ hay đường dẫn cài đặt. Bản sao chứa cả **ảnh gốc trước khi che thông tin**, chỉ chia sẻ PNG đã xuất nếu cần che dữ liệu.

Nhập `.cvlibrary` kiểm cấu trúc/tên entry/hash/giới hạn trước; ảnh và sidecar được kiểm theo schema trước khi commit. Mỗi nội dung nhận mã mới, giữ thư viện hiện có; nhập hai lần sẽ tạo hai bản sao. File đến thư mục media hiện tại, sidecar đến Edits; không dùng đường dẫn tuyệt đối trong metadata gói. Nhập/sao lưu chạy kiểm file ở nền, khóa workspace trong thao tác. Hủy từ menu khay; nếu đã bước commit thì hoàn tất thao tác rồi thoát. Thiếu file gốc/quyền/dung lượng phải báo lỗi; không bỏ qua âm thầm để tạo bản sao thiếu ảnh.

Giới hạn nền tảng bản này:2.000 nội dung,2GB tổng dữ liệu giải nén,512MB/media,140MB/sidecar,8MB manifest. Video vẫn chỉ giữ/copy file; không thêm biên tập video. Thư viện quá lớn cần tách ra hoặc chờ chức năng chia gói trong bản sau. Backup app tự tạo trước cập nhật vẫn chỉ là metadata; không thay thế `.cvlibrary`.

## Bố cục và công cụ

Kéo đường phân cách bên trái/bên phải để đổi rộng thư viện/thuộc tính; trong chế độ thư viện ngang kéo đường phía trên để đổi cao. Bố cục lưu riêng theo tài khoản Windows/máy. Thumbnail thư viện ngang đổi kích thước theo chiều cao, bỏ phần tiêu đề lớn chiếm một cột. Khi kéo, Editor chuyển sang Vừa khung; chỉ đổi zoom xem, không resize file xuất.

Mục Mẫu đã lưu nằm đầu thuộc tính; lọc theo nhóm công cụ đang dùng, khi Chọn mà chưa chọn đối tượng có thể duyệt toàn bộ mẫu. ☆ lưu kiểu hiện tại của khung, mũi tên, callout, chữ, bước, freehand và các hiệu ứng. Mỗi mẫu giữ kiểu/màu/nét/thuộc tính; không giữ nội dung chữ riêng hay pixel từ ảnh nguồn. Chọn/Crop/vùng pixel có bookmark để gọi lại công cụ, không lưu vùng chọn theo tọa độ của ảnh khác. Lớp ảnh được lưu trong project. Nhấn sao lần nữa để bỏ; tối đa64 kiểu vẽ và16 bookmark; file lỗi không bị tự ghi đè. Preview chỉ có hình, tên ở tooltip. Icon vector cùng nét1.25px.

OCR và Hỗ trợ chỉ còn một lối vào **OCR & chỉnh chữ**; trong cùng cửa sổ có nhận chữ, sửa/copy/TXT và chỉnh trên nền màu phẳng/tách lớp. Giữ zoom, Space pan, lens, preview bắt buộc và Undo. Không thêm AI inpainting hoặc tự khôi phục nền phức tạp.

## Modal, logo và capture

Cửa sổ thường/modal dùng thanh tiêu đề WPF và WindowChrome để giữ kéo/resize/system menu; X lấy màu Text hiện tại, hover đỏ; không phụ thuộc vào màu glyph native trước hover. Overlay chụp trong suốt không dùng titlebar này. Theme sáng/tối/system vẫn lưu theo máy. Các hộp thoại file/folder/màu của Windows do hệ điều hành hiển thị.

About lấy artwork256px thay cho frame ICO nhỏ. App/runtime/Setup/updater vẫn dùng artwork đã chốt từ v117; mã asset không cần chạy theo số release. PE audit kiểm cả Setup tránh MUI2 ghi đè icon. Windows có thể giữ icon pin cache: cần kiểm sau cập nhật thực tế, không tự xóa cache hệ thống.

Chụp phần tử: rê gần cạnh/góc trên của container giúp ưu tiên modal/khối; rê lên trên cùng cửa sổ giúp chọn toàn cửa sổ. Kích thước vùng vẫn theo physical pixels, tolerance theo DPI. Con lăn lên/xuống chọn cha/con giữ nguyên. UIA phụ thuộc khả năng ứng dụng nguồn cung cấp cây accessibility: HTML/canvas/custom renderer không có cấu trúc có thể chỉ thấy cửa sổ; giữ và kéo là fallback. Không có cam kết nhận diện chính xác mọi modal ở mọi app.

Sau chọn vùng hoặc phần tử (ảnh), có nút Chụp/Enter, Hủy/Esc, mũi tên **↓** sát cạnh dưới và **→** sát cạnh phải. Mũi tên mở cấu hình và phiên chụp cuộn hiện có, dùng đúng vùng vừa chọn; direction được điền sẵn và vẫn đổi được. Đây là hai hướng ghép đang hỗ trợ; chưa ghép ngược lên/trái, không giả nhận biết khả năng cuộn của mọi app. Chụp lại vùng cũ vẫn chụp nhanh, không bắt chọn lại hành động. Không thêm bước này vào quay video.

Viền khi chụp: ảnh mới có `Padding=0`, `BorderWidth=n`, kích thước W+2n × H+2n; ảnh gốc nguyên byte và không scale. Ảnh cũ có padding đã lưu vẫn giữ để tránh tự sửa project của người dùng.

## Nghiệm thu Windows cần thực hiện

- Mở/chuyển nhiều ảnh đã có từ1.17; sửa rồi lọc/đổi thứ tự; crash recovery không mất sidecar; xóa/khôi phục giữ tag/favorite.
- Tag/collection/rename/favorite tồn tại sau khởi động; lọc kết hợp, bỏ lọc, empty state; video metadata vẫn dùng được.
- Export thư viện → máy/tài khoản khác import → so byte media/hash, lớp ảnh/callout/connector/crop/render và nhãn; thiếu media/corrupt manifest/hash, hết đĩa, Hủy; thư viện cũ còn nguyên.
- Kéo cả ba splitter ở Windows scale100/125/150/175/200%, màn hình phụ tọa độ âm, cửa sổ hẹp; thumbnail/thuộc tính reflow, file xuất không đổi pixel, mở lại giữ kích thước.
- Sáng/tối/system: X thấy ngay, đóng/modal owner, caption drag/doubleclick/maximize/resize/Alt+F4, keyboard focus; OCR zoom/lens/IME; About ở64px nét.
- ☆ từng công cụ và chọn lại sau restart, sao nhiều mẫu khác family, remove; không lưu pixel nguồn nhạy cảm vào style.
- Element modal/window/grid thật và browser accessibility; góc trên/cạnh/giữa/scroll parent; stale window move; giữ kéo fallback.
- ↓/→ vùng chọn trên từng màn/DPI, Enter/Esc, gần mép màn không mất nút; chọn hướng sẵn, auto/manual scroll, cancel/recovery, nối preview. UIA/capture không ghi UI overlay vào ảnh.
- Viền1/3/8/128px: pixel ảnh không đổi, không khoảng hở; crop/rotate/zoom/export/copy vẫn đúng.
- Cài mới D:\Captovix\Captovix và update1.17→1.18; popup, restart, AppID/shortcut/logo/EXE path. Không coi cross-build Linux là nghiệm thu các bước Windows này.
