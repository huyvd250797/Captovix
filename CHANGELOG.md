# V1.41.0 — Groups & Layout Editing

- Nhóm cha/con nhiều cấp, cây chọn đối tượng, scope chỉnh con, giữ nhóm khi căn/resize/move/sắp lớp, thuộc tính dùng chung, clipboard/nhân bản độc lập và schema21/backup.
- Popup/common message form có outline/theme và phản hồi sáng viền khi click ngoài modal; không nháy ẩn cửa sổ. MenuThêm hai cột/có chiều sâu.
- Thư viện: menu chuột phải Mở vị trí tệp.
- Không đổi runtime/dependency/license/backend; không DB migration. Native Windows/DPI/installer acceptance còn chờ.

# V1.40.0 — Move & Background Recovery

- Mask pixel nền độc lập với Mockup: ghim nguồn khi kéo/resize, giữ khi xóa, không mang mask qua copy/duplicate/template. Bảo vệ vùng lân cận, UI xem/chỉnh màu/mặt nạ/vùng giữ; hỗ trợ lịch sử100thao tác lưu cùng tài liệu, Undo/Redo trong phiên.
- Schema20, backup sidecar trước nâng. Sampling hiệu ứng và xuất ảnh kể cả0annotation đều áp dụng phục hồi nền. Nguồn nguyên vẹn, freeze/detach/cache theo thread giữ nền cũ không tái xuất qua magnifier/blur.
- Chụp ảnh chọn vùng/modal: tùy chọn Enter, mặc định thả chuột chụp ngay; tự lưu. Quay/cuộn giữ xác nhận cũ.
- Bỏ phím Theo cấu hình/Chọn cửa sổ, mô tả dài, menu Vùng pixel. Phím công cụ tùy chỉnh đơn/tổ hợp, chống trùng/collision và không kích hoạt khi gõ/chọn/kéo; Thêm dùng chung tuyến công cụ động, hỗ trợPrtSc key-up.
- Sidebar nhãn/nút màu/input/spacing gọn và cân đối, giữ chức năng/theme. V1.40.0 đồng bộ assembly/UI/host/installer/feed.
- Không migration mới/backend redeploy; giữ protocol/config/runtime1.39. Nền phẳng trước, chưa phục hồi ảnh/gradient tự động. Chưa publish/chưa nghiệm thu Windows native.

## Lịch sử

# V1.39.0 — Element Scan & Review

- M2: quét vùng/toàn ảnh, OCR line confidence + phân tích hình ảnh; text/title/button/input/block; ghép chữ-control/label-input, loại trùng, đánh dấu không chắc chắn.
- Màn rà soát riêng: chọn tường minh, sửa khung kéo4góc hoặcX/Y/W/H, đổi loại/nội dung/màu/liên kết; thêm/bỏ khung, preview, hủy/quét lại/dừng. Apply một bước Undo/Redo, giữ nguồn và lưu/mở lại.
- Schema19 cho loại mới/scan/liên kết; backup sidecar trước nâng; paste/duplicate remap liên kết và bỏ vùng che nguồn ở bản sao.
- Bỏ nút Mockup cố định, đặt trong Thêm + ô động cũ, thêm icon Mockup; giữ tương thích lệnh gần đây từ1.38.
- Popup trial/perpetual theo trạng thái, ẩn KEY mặc định khi đã kích hoạt; trial có upgrade KEY, ngày giờ/số ngày còn lại; check chỉ khi perpetual còn quyền.
- Claims bổ sung activatedAt tùy chọn đã ký; migration202611040001 trả ngày activation gốc từ devices.created_at, không đổi policy hay dữ liệu. Không deployEdgeFunction mới; cần chạy SQL rồi refresh cho chứng nhận cũ.
- Bump app/UI/installer/nupkg/feed1.39.0; giữ runtime/OCR/licenseconfig/capture foundations. Chưa publish, chưa nghiệm thu native Windows hay chạy SQL sản xuất.

## Lịch sử

# V1.38.0 — Text & Button Editing

- M1: kéo vùng chữ/title/nút, OCR vie/eng có hủy, xem trước và sửa thủ công; không ghi đè nội dung gõ khi OCR về muộn.
- Đối tượng chữ/nút riêng: font/cỡ/đậm/căn/đệm, màu chữ/nền che/nền nút/viền/bo góc; nhấp đúp hoặc bảng thuộc tính để sửa tiếp.
- Che nội dung nguồn bằng màu phẳng, vùng che ghim tại vị trí nguồn khi di chuyển thân. Gốc giữ riêng; Undo/Redo, JSON sidecar và dự án giữ metadata. Copy/duplicate/template không che nhầm ảnh đích.
- Schema18 có backup sidecar cũ trước khi nâng; không DB/backend/runtime/dependency changes. Giữ Capture Bounds1.37.2 và Image Layer Thread Safety1.37.3.
- Nâng assembly/UI/installer/nupkg/feed lên1.38.0; chưa tự publish. Chưa nghiệm thu native Windows.

## Lịch sử

## V1.37.3 — Image Layer Thread Safety

- Sửa cache PNG dùng BitmapFrame decoder-backed khi chuyển giữa MTA load/UI/STA save: chuyển thành bitmap pixel độc lập đãfreeze.
- PNG encode chuẩn hóa BitmapFrame trước khi tạo encoderframe; giữ dữ liệu PNG sidecar hiện có.
- Chặn MessageBox lỗi mở chồng bằng gate chốngreentrant, Handled trước khi mởmodal.
- Thêm Windows regression tái hiện paste/save/switch/reopen/render và kiểm pixel/sidecar qua25lượt; cross-build, chưa chạy Windows.
- Giữ mọi sửa1.37.2 và thông tin hiện có. Không migration mới, không thay backend/license/runtime.

## V1.37.2 — Capture Bounds Fix

- Sửa gợi ý cửa sổ thô vượt biên ảnh giữ, đồng bộ selection hiển thị và xác nhận.
- Giới hạn kéo/resize/keyboard theo frame, giữ kích thước lúc move và tỉ lệ Shift trong làm tròn pixel.
- Guard cuối trước lưu cho tọa độ cũ/repeat, cập nhật kích thước ảnh/metadata theo vùng thực.
- Không recapture overlay; Crop vẫn kiểm giới hạn và log vùng/biên ảnh chi tiết.
- Tăng version app/UI/installer/feed lên1.37.2 để nâng từ1.37.1. Không migration mới, không đổi license/backend/runtime.

## V1.37.1 — Performance & Responsiveness

- Tăng số phiên bản app/assembly, tiêu đề cửa sổ, installer và gói Full lên1.37.1 để updater nhận bản vá từ1.37.0.
- Bao gồm toàn bộ sửa Callout: click ngoài lưu, chỉ kéo mới tạo, tâm dời thân giữ đuôi và kéo ngoài tâm dời cả cụm.
- Không đổi dependency/runtime, license, backend hay schema. Không cần migration mới.
- Release assets phải được đăng lên stable latest trước khi app cũ kiểm tra thấy bản mới.

## Sửa click ngoài và thêm tâm kéo thân — giữ V1.37.0

- Click ngoài kết thúc nhập và lưu phiên sửa; không chuyển click đó thành một lần tạo mới.
- Chỉ nhấn giữ/kéo mới tạo Callout; bấm đơn không thêm Callout, popup nhập hay lịch sử Undo.
- Tâm xanh khi chọn: kéo thân, giữ tip đuôi ở tọa độ ảnh; kéo ngoài tâm: dời cả Callout. Hoạt động khi xoay, giữ kích thước/nội dung và Undo/Redo. Với Callout cũ, chuyển pivot sang thân trong cùng thao tác có Undo để bảo toàn vị trí hiển thị.
- Tay nắm chỉ thuộc giao diện, không xuất vào PNG/copy. Giữ schema tối đa17, không cần migrationDB/backend.

## Hiệu chỉnh Callout — giữ V1.37.0

Thay cơ chế thân cố định bằng kéo giãn2trục theo mô phỏng đã duyệt. Đuôi ghim điểm đầu, nhấn đơn vẫn có hộp mặc định; bỏ ép tăng kích thước hộp trống khi thả/click ngoài. Tự chọn cạnh nối đuôi theo hướng kéo. Giữ toàn bộ tối ưu hiệu năng,30ảnh và công cụ đang chọn. Không migration/backend.

# Captovix V1.37.0 — Performance & Responsiveness

Nâng từ source V1.36.0 Boss gửi; tập trung hiệu năng, giữ công cụ và sửa lại Callout. Không thay dependency/runtime hay cấu hình license/backend.

Đã triển khai:

- Giảm công việc trên đường khởi động: danh sách thumbnail từ50 còn30, nhận diện media đã có bằng HashSet thay quét toàn bộ metadata cho từng file; danh sách font chỉ nạp khi mở picker, sửa shortcut sau lần vẽ giao diện đầu tiên.
- Mở/chuyển ảnh ở nền, chỉ một worker giải mã cùng lúc. Hủy yêu cầu đang đợi; generation/cancellation loại kết quả cũ khi chuyển nhanh hoặc đóng cửa sổ. Giải mã native đang chạy sẽ hoàn tất rồi bị bỏ nếu không còn cần; không hứa ngắt WIC giữa chừng.
- Lưu tự động ở nền bằng snapshot độc lập, không serialize/deserialize vòng trên UI. Gộp thay đổi bằng timer; không chạy nhiều autosave cùng lúc. Revision ngăn báo Đã lưu cho nội dung mới chưa ghi. Khi chuyển ảnh/đóng/cập nhật, đợi worker cũ rồi lưu cuối, tránh bản cũ ghi đè bản mới.
- Kéo/resize: giữ cache chữ/hình từ1.36, tránh tìm lại đối tượng chính trong toàn bộ danh sách ở mỗi node; bỏ quét connector lặp khi đã ở trong phiên nhập. Zoom/hover dùng lại drawing của cảnh khi nội dung không đổi; chỉnh sửa, redaction, chữ inline và tương tác làm hết hạn cache.
- Đóng/thay ảnh giải phóng drawing, cache chữ/đuôi và bitmap sampling từng giữ ảnh cũ, cùng cache PNG giải mã. Worker autosave cũng giải phóng cache theo thread sau khi xong; không ép GC trên UI. Undo vẫn giữ ngân sách sẵn có.
- Giữ công cụ đang chọn khi hoàn tất nhập chữ/Callout, chụp/chuyển ảnh, crop và hủy thao tác bằng Esc. Chọn công cụ khác mới thay đổi công cụ; khi bấm lệnh chuyên biệt yêu cầu chế độ Chọn (ví dụ sửa lớp ảnh), giữ hành vi riêng của lệnh đó.
- Danh sách gần đây tối đa30 nội dung, kể cả ảnh lịch sử được ghim tạm; không xóa ảnh cũ, xem đầy đủ trong Quản lý thư viện.
- Callout chỉ xuất hiện sau khi nhấn giữ và kéo đủ4px hiển thị; bấm đơn hoặc rung chuột nhẹ không tạo đối tượng/Undo/popup mới. Click ngoài lúc nhập kết thúc và lưu phiên đang sửa, chặn lần click đó đi xuống vùng vẽ. Khi chọn Callout, tâm xanh có dấu cộng cho phép kéo thân, giữ tip đuôi cố định trên ảnh; kéo phần thân ngoài tâm di chuyển cả Callout. Node đuôi và resize/xoay giữ chức năng riêng. Vẫn kéo giãn2trục lúc tạo, giữ công cụ và các mẫu hiện có.
- Bổ sung đo có giới hạn trong RAM cho dựng cửa sổ/first paint, mở ảnh, autosave và frame editor; không đưa lại menu chẩn đoán đã bỏ.

Đo trước/sau logic trên cùng host Linux/.NET10, 5.000 chú thích có một Callout20.000ký tự,3lượt làm nóng +11lượt đo lấy trung vị:

| Phép đo | V1.36 | V1.37 |
| --- | ---: | ---: |
| 1.000 lần Begin trong cùng phiên sửa | 3,818ms | 0,0194ms |
| Tạo snapshot5.000chú thích | 133,9469ms | 6,518ms |
| Cấp phát mỗi snapshot | 20.472.582byte | 3.321.275byte |

Đây là microbenchmark logic, không phải đo startup/chuột/FPS/RAM Windows toàn app. JSON thô ở docs/PERFORMANCE-V137-before.json và after.json. Có script chạy lại và fixture Windows; chưa có số đo Windows để khẳng định không lag hoặc hết rò rỉ RAM trên mọi máy.


# Captovix V1.36.0 — Text & Callout Polish

Nâng thẳng từ source V1.33.0 theo yêu cầu Boss, bỏ qua phát hành V1.34.0 và V1.35.0. Giữ chức năng đã có; không tuyên bố hoàn thành các phạm vi bị bỏ qua.

- Callout bắt đầu từ đuôi: điểm nhấn đầu tiên được giữ cố định, kéo để tạo thân ở bên trái/phải. Thân mặc định căn giữa theo chiều dọc quanh điểm đuôi, đệm30px, tối thiểu172×96px; bấm đơn tạo thân220×120px. Đuôi có node riêng; resize thân không kéo theo đuôi (kể cả khi xoay).
- Auto-fit mặc định giữ kích thước đã vẽ và chỉ nở thêm theo nội dung đang nhập, xuống dòng theo chiều rộng. Không tự co hộp khi xóa chữ; vẫn sửa rộng/cao độc lập bằng node/các trường thuộc tính. Mẫu đã lưu giữ lựa chọn bật/tắt auto-fit của người dùng.
- Nhập trực tiếp trên ảnh: có chữ Việt/IME, căn dọc của con trỏ theo hộp; click ngoài/Ctrl+Enter hoàn tất, Esc hủy, Enter xuống dòng. Composition được hoàn tất trước khi đóng phiên nhập; không lưu chữ preedit dở.
- Di chuyển Callout dài tái sử dụng dàn chữ và bản vẽ đã đóng băng; không dựng lại outline toàn bộ glyph hay ghép hình đuôi mỗi lần đổi vị trí. Cập nhật khi gõ gộp theo nhịp render, không chờ bộ hẹn giờ400ms của thuộc tính. Cache yếu theo đối tượng và riêng từng thread, không giữ lịch sử chữ không giới hạn.
- Thuộc tính/preset áp dụng đúng loại Chữ/Callout/Bước; chọn mẫu Callout cập nhật bố cục và tự nở nếu cần, không đổi vị trí đuôi. Không đổi schema17 hay phá mẫu cũ.
- Khôi phục logo và tên Captovix gọn ở góc trái hàng menu; bỏ menu chẩn đoán, báo cáo hiệu năng và hướng dẫn kiểm thử, cùng mục Máy & màn hình trong Thiết lập. Giữ công cụ chụp/chỉnh ảnh, khôi phục, cập nhật và bản quyền.
- Tọa độ chụp dùng lõi vàng2pixel vật lý, viền đen0.25pixel mỗi bên thay lõi cam1pixel/viền0.5pixel trước đây.
- Giữ snapshot ảnh ở pixel vật lý và xuất PNG/copy không giảm theo zoom. Xem100% để đánh giá pixel thật; Vừa khung là bản xem đã thu nhỏ. Không tăng nét giả hay upscale nguồn.
- Updater luôn chọn gói Full của stable mới nhất, không bắt cài bản trung gian. V1.31→V1.33/V1.36 được kiểm thử chọn đích bằng SDK thật; gói V1.36 được kiểm tra tải/hash với locator V1.31. Backup dữ liệu trước áp dụng và xác minh SHA-256 giữ nguyên.


# Captovix V1.33.0 — Scrolling Capture Quality

- Chụp cuộn: đối chiếu dày theo các hàng của vùng overlap và nhiều dải cột; khung lệch ngang hoặc mối nối không chắc chắn cần kiểm tra/xác nhận. Không kéo giãn, nội suy hay tự làm mờ pixel để che mối nối.
- Lấy khung thủ công và tự động đều đợi ổn định, kiểm tra từng vùng nhỏ để tránh bỏ qua loading/animation; không thêm khung chưa ổn định sau thời gian chờ. Giới hạn kiểm tra ổn định không đảm bảo nhận ra trang vẫn đang chờ dữ liệu mạng khi hình đang đứng yên; có thể lấy lại khung.
- Tự phát hiện header/footer cố định khi hai khung đủ chi tiết và ghép phần thân chắc chắn; giữ vùng đầu/cuối một lần. Có tùy chọn tắt và nhập số pixel thủ công, hỗ trợ cả cuộn dọc/ngang.
- Xem trước mối nối, bật dấu chỉ dẫn, sửa overlap và xác nhận các điểm chưa chắc; bỏ khung bất kỳ trong phiên. Nguồn gốc và phiên trước được giữ khi tạo revision mới để khôi phục.
- Chụp ảnh được chính Captovix: giữ giao diện app/popup hiện có trong nền đã đóng băng. Chọn nguồn cửa sổ có thể bao gồm Captovix; selector/kính lúp/thanh điều khiển vẫn không lọt vào file chụp.
- Công cụ đưa lên hàng Chụp/Quay theo thứ tự: Khung, Callout, Mũi tên, Chữ, Crop, Làm mờ, Chọn vùng, Bước, Nhận chữ, ô công cụ động, Thêm. Chọn vùng là vùng pixel trên ảnh; Chọn đối tượng có trong Thêm và phím V.
- Thêm chứa các công cụ phụ (Chọn đối tượng, Phóng đại, Spotlight, Highlight, Vẽ tự do, Canvas…) và các Studio/Stamp hiện có; dùng lại bằng ô công cụ động cạnh Thêm.
- Bỏ nút Mở file/Copy ảnh/Xuất file khỏi hàng công cụ. Các lệnh vẫn có trong menu Tệp và phím tắt tương ứng. Thuộc tính bên phải kéo lên ngang hàng công cụ; kích thước/zoom thu gọn một dòng, tăng chiều cao canvas.
- Kéo công cụ A lên B để xem trước hoán đổi; thả lưu vị trí, Esc hủy về bố cục trước. Thêm và ô công cụ động giữ ở cuối. Bố cục V1.33 dùng file riêng để lần đầu hiện đúng thứ tự mới; giữ file bố cục cũ.
- Giữ ảnh thư viện khoảng 160ms rồi kéo: cung cấp PNG đã có chú thích qua FileDrop/Bitmap cho Word, Explorer, trình duyệt upload Drive và ứng dụng hỗ trợ kéo file. Không tự tải lên Drive. Kéo nội bộ vào canvas vẫn giữ cơ chế ghép ảnh.
- Popup bản quyền có logo/tiêu đề/mô tả ngắn, ô KEY và nút Kích hoạt; chức năng nâng cao ở Xem thêm. Link không có KEY hiển thị lời mời hoặc lịch sử dùng thử.
- Máy đã trial không được kích hoạt lại 30 ngày: chặn trong UI và Request dưới khóa đồng thời, lưu chứng nhận trial đã ký ở registry DPAPI; hiển thị ngày bắt đầu (hạn gốc trừ 30 ngày) và ngày hết hạn. Backend hiện có giữ nguyên hạn gốc khi nhận lại trial, không reset 30 ngày. Không thay cặp khóa hoặc dữ liệu server.
- Trạng thái vĩnh viễn còn quyền: đèn xanh lá có quầng sáng + Đã kích hoạt. Trial còn quyền: đèn vàng; trạng thái hết hạn/chờ xác minh hiển thị riêng.

Không cần chạy migration mới. Không cần deploy lại backend/admin bản quyền. Không thay mô hình đóng gói/dung lượng đã tạm giữ.

## Lịch sử

# Captovix V1.32.0 — Smart Element Capture Refinement

- Chụp mặc định gộp gợi ý phần tử/modal và kéo vùng. Click khóa phần tử → chỉnh cạnh → Enter chụp; kéo trực tiếp tạo vùng thủ công. ↑/↓ chuyển cha/con; Tab chọn lại; S bật/tắt gợi ý.
- Nhận diện có bộ nhớ hierarchy ngắn hạn, hủy truy vấn khi chuột rời vị trí, bỏ phản hồi cũ và giữ ngưỡng ổn định. Provider không phản hồi vẫn kéo chọn được.
- Đường X/Y vàng cam có viền đen mảnh; H bật/tắt, M bật/tắt kính lúp. Overlay và toolbar không đi vào ảnh nền đã giữ.
- Popup KEY gọn theo theme: Nhập KEY → Kích hoạt. Tôi không có KEY mở lời mời dùng thử 30 ngày; chỉ gọi trial khi bấm xác nhận. Xác minh/trả máy/khôi phục đặt trong Xem thêm; bỏ mã máy và dấu khóa khỏi UI.
- Thiết lập chụp/quay chia nhóm nguồn, sau chụp, preset và video; giữ cấu hình cũ và phím cũ.
- Phím tắt cho phép phím đơn và tổ hợp Ctrl/Alt/Shift/Win; nhận PrtSc cả key-up. Windows kiểm tra xung đột và phím dành riêng; không chiếm phím bằng hook ngầm.
- Thư viện 39 stamp vector tự thiết kế, chia nhóm. Hình xem trước căn theo giới hạn nét vẽ, không bị cắt. Chọn mẫu rồi click lên ảnh để đặt; click tiếp thêm stamp; chọn lại đối tượng/tay nắm để sửa, Esc về Chọn.
- Chữ: kéo tạo khung với Text xem trước, nhập trực tiếp sau thả; resize theo cạnh/góc, reflow giữ cỡ chữ. Khung cố định giới hạn phần chữ hiển thị; tăng chiều cao nếu chữ tràn. Chữ cũ giữ nguyên hành vi.
- Mũi tên bỏ khung chữ nhật khi chọn/rê; mũi tên thẳng có hai đầu và node xoay xanh lá. Mũi tên cong vẫn giữ đường cong; node điều khiển cong của công cụ Đường được giữ riêng.
- Node xoay của các đối tượng/nhóm đổi xanh lá.
- Cuộn thường: dọc; Shift+cuộn: ngang; Ctrl+cuộn: zoom. Trong Editor zoom ảnh; trong panel/popup zoom nội dung UI 65–200%, không đổi dữ liệu xuất. Selector không dùng con lăn đổi phần tử; dùng ↑/↓.

Không cần chạy migration mới. Không cần deploy lại backend/admin bản quyền. Giữ nguyên public license config và cặp khóa server; không gọi tạo key/trial trong kiểm thử tự động.

## Lịch sử

# V1.31.0 — Quick Capture Experience

- Thu gọn overlay controls, thêm crosshair X/Y và lựa chọn M/H được nhớ; thêm chọn vùng bằng bàn phím và nhớ chế độ chụp.
- Viền chọn vùng và kính lúp mảnh hơn; lens nhỏ hơn, giữ snapshot RAM/cắt pixel gốc.
- Crop mouse-up tự áp dụng một giao dịch Undo; nhập số vẫn xác nhận riêng. Kéo trong khung full ban đầu tạo vùng mới.
- Bước chọn viền/nền độc lập, trường optional schema16; giữ legacy rendering khi chưa sửa, bảo vệ sidecar mới khỏi app cũ.
- Stamp gallery12 vector mới trong nút riêng; ô công cụ động trái Thêm, persistence và icon riêng, giữ cặp khi đổi bố cục toolbar.
- Update Settings/popup dùng chung chuẩn hóa ghi chú và typography.
- Giữ nguyên licensing/backend/admin/migrations/public config/runtime/dependencies/installer path và logo từ V1.30.
- Không cần migration mới, không deploy/publish. Native Windows acceptance vẫn cần thực hiện.

---

# V1.30.0 — Capture Fidelity & Reliability

- Desktop snapshot RAM trước lưu Editor/selector; bỏ capture live lần hai khi Enter cho vùng/element/display/repeat. Hẹn giờ trước snapshot, giữ topology và thời điểm.
- Backdrop opaque từ snapshot, kính lúp pixel15×15, X/Y thực (âm khi màn phụ), M ẩn/hiện; giữ tay nắm/nét đứt/Enter một lần; bỏ animations và tham chiếu bitmap khi kết thúc overlay.
- PNG lưu kiểm tra BGRA decode bằng ảnh render, original capture không resize. Giữ đường nguyên pixel crop/rotate/flip, nguyên pipeline clipboardPNG96DPI.
- Zoom readout phân biệt Vừa khung/100%/Tùy chỉnh; đánh dấu nút chế độ và tooltip về DPI/file output.
- Thêm FrozenCaptureChecks, CaptureFidelityWindowsChecks và checklist Windows; không tuyên bố đã chạy Windows thực từ cross-build.
- License config/backend/Supabase migrations và schema ảnh15 giữ nguyên. Không cần migration mới. Không deploy production.

---

# V1.29.1 — Configured Windows build

- Nhúng đúng license-config.json công khai Boss cung cấp: URL Supabase Edge, issuer và RSA public key 3072 bit.
- Giữ nguyên chương trình, giao thức bản quyền, authority và database; không phát sinh migration, không tạo trial/key giả trên production.
- Endpoint health/ready đã được kiểm tra HTTP 200. Luồng kích hoạt/cài đặt trên Windows thật còn cần Boss nghiệm thu.
- Bàn giao Setup trực tiếp, Update-Packages và source đã cấu hình. Không thay admin/config.json mẫu hoặc redeploy hệ thống Boss đang chạy.

# V1.29.1 — Serverless Licensing

- Thay backend ASP.NET Core/admin token bằng Edge Function TypeScript và Supabase Auth + allowlist; bỏ yêu cầu Docker/VPS khỏi quy trình hiện hành.
- Chuyển quyết định license sang RPC PostgreSQL giao dịch; giữ RSA-PSS/SHA-256, device proof/nonce, trial/lease/slot/recovery và schema cũ. Migration additive và giữ dữ liệu legacy.
- Cloudflare Pages static admin: login/refresh trong RAM, audit theo UID, CORS exact origin, CSP/no-store; public config không chứa service-role/private key.
- Công cụ trình duyệt ngoại tuyến tạo/nạp PKCS#8, export Secrets và public config; giữ authority khi nâng cấp. GitHub Actions build Windows bằng public Variables, không tự Release.
- App Windows cập nhật version/lỗi quota/config và chặn private key ở public config. Các công cụ ảnh/capture/model/schema giữ từ V1.29.0; không thay chất lượng pixel/render.
- Thêm TypeScript/DOM, role/RPC transaction và interop C#↔WebCrypto checks. Chưa deploy Supabase/Auth/Cloudflare/Actions hoặc thực thi Windows thật.
- Bản giao review vẫn trống production URL/key; chỉ chụp vùng. Xem docs/SERVERLESS-LICENSING.md trước phát hành.

---

# V1.29.0 — Licensing Operations & Recovery

- Gộp nền tảng V1.28 theo lựa chọn của Boss; không giả định V1.28 đã được phát hành.
- Thêm giao thức RSA-PSS, device proof, nonce/timestamp chống replay, trial 30 ngày và lease có thời hạn.
- API PostgreSQL với slot giao dịch, key hash, recovery rotation/cooldown và audit; migrations riêng + RLS cho role server.
- Trang quản trị tạo key, tra cứu/phân trang, đổi slot, thu hồi/khôi phục, trả máy, đổi recovery, quản lý/gia hạn/sửa định danh trial, audit JSON theo trang.
- UI Windows: trạng thái footer; dùng thử/kích hoạt/trả/khôi phục/xác minh và mã hỗ trợ. Gate clipboard, model, click/keyboard/hotkey/tray, Studio/OCR/export; nội bộ checkpoint dữ liệu trước hạn chế.
- DPAPI CurrentUser + mốc giờ registry, monotonic runtime; cấu hình URL/public key nhúng assembly. Chưa có service production: fail restricted, không cấp đầy đủ quyền ngầm.
- Giữ pipeline capture pixel, editor schema15 và fix sidecar V1.27.1/2; installer/brand/feed giữ nền cũ.
- Sửa fixture schema future ở ba bộ kiểm tra cũ từ số15 cố định thành MaximumSupportedVersion+1 (15 hiện đã hỗ trợ từ V1.27).
- Hướng dẫn triển khai/vận hành chi tiết trong docs/LICENSING-GUIDE.md. Chưa chạy Windows thực tế hoặc deploy API production.

---

# Captovix V1.27.2 — Color Picker & Studio Access

Nền nâng cấp: Source ZIP V1.27.1 Boss gửi. Bản vá V1.27.1 về lưu schema15 được giữ nguyên. Không tích hợp cấp phép trong bản này.

## Thay đổi sử dụng

### Bút hút màu

Trong bảng chọn màu dùng chung có nút icon bút hút màu **Lấy màu trên màn hình**: bấm nút, rê bút tới pixel để xem ô màu và mã #RRGGBB, click áp dụng màu cho đúng mục vừa mở, Esc/chuột phải hủy không đổi màu. Hỗ trợ bảng màu viền, nền, chữ, highlight/spotlight/viền kính, pixel fill, lớp ảnh, canvas, viền chụp và nút lấy màu trong cá nhân hóa màu giao diện. Chọn màu khác/native palette và nhập HEX vẫn giữ.

Picker đóng băng màn hình tại thời điểm bật bút; lấy màu pixel của ảnh màn hình này, không thay đổi kích thước ảnh hoặc clipboard. Nếu muốn lấy pixel gốc trong ảnh đang xem thu nhỏ, dùng Thêm → Lấy màu từ ảnh rồi zoom100%. Màn hình HDR/màu rộng chưa kiểm chứng; không hứa khớp màu quản lý ICC. Các nút màu thuộc OCR assistance cũ hiện đang ẩn không thay đổi.

Picker giữ ảnh màn hình trong RAM, không lưu file, giới hạn tổng40MP. Lớp phủ hiển thị ảnh này để nhìn và lấy cùng một pixel, không lấy màu HUD. HUD đặt lệch trỏ, cập nhật40ms; GDI DC/bitmap release trong finally. Đóng cửa sổ gọi picker thì kết thúc picker. Bấm chọn chỉ áp dụng1lần, không truyền click xuống ứng dụng nền. Cần nghiệm thu DPI và nhiều màn hình thật.

### Thêm công cụ / stamp

Thanh công cụ Editor có **Thêm** (icon ba chấm). Bấm mở flyout theo theme sáng/tối gồm chỉnh màu/làm nét, lấy màu từ ảnh, Clone, phối cảnh, chỉnh nghiêng, tách nền theo màu, mask, so sánh Trước/Sau, watermark và khôi phục nền gốc.

Stamp có8mẫu hình xem trước được render bằng renderer của app, gồm đúng/sai/đã duyệt/cần sửa/phím tắt/bàn phím/con trỏ. Bấm một mẫu để chèn ngay nhóm chữ/khung chỉnh sửa được, có Undo. Có nút thêm logo/ảnh cá nhân. Bỏ mục Studio ảnh khỏi menu Vùng pixel để không trùng lặp. Thêm tham gia sắp xếp thanh công cụ như các nút khác.

### Chọn vùng thủ công

Chế độ **Vùng chọn** không chạy UI Automation, không gợi ý web/cửa sổ và không tự tạo vùng dù chờ lâu. Phải giữ chuột kéo; thả ra chỉnh cạnh hoặc di chuyển rồi Enter chụp. Nút nhận diện và phím S không hiện/không bật trong chế độ vùng thủ công. Vùng chụp cuộn thủ công giữ cùng nguyên tắc.

Muốn nhận diện modal/bảng/khối, chọn **Phần tử/modal** trong cấu hình chụp hoặc Chụp & quay → Chụp phần tử/modal. Chế độ Cửa sổ/Toàn màn hình và Chụp lại vùng vẫn là các thao tác riêng có chủ đích; không bị thay đổi.

## Cài và phát hành

Giải nén Captovix-v1.27.2-Update-Packages.zip, đóng app và chạy HuyVo.Captovix-win-Setup.exe để cài trực tiếp. Muốn update trong app: đăng đủ5asset bên trong ZIP (full.nupkg, Setup.exe, RELEASES, assets.win.json, releases.win.json) lên Release stable/latest tag v1.27.2 của repo phát hành đã cấu hình. Không chỉ đăng ZIP. Chưa tự publish GitHub. Setup chưa ký số.

Source ZIP có README, source và fixture. SDK10.0.100; WPF Windows x64; runtime10.0.13 self-contained; không nâng dependency.

```powershell
dotnet build src/Captovix/Captovix.csproj -c Release -p:Platform=x64
dotnet run --project tests/ShellLayoutChecks -c Release
dotnet run --project tests/CompositionChecks -c Release
dotnet run --project tests/SmartCaptureChecks -c Release
dotnet run --project tests/CaptureWorkflowChecks -c Release
# Windows thật: kiểm thử lưu schema15 và UI Studio cơ bản
dotnet run --project tests/ImageStudioWindowsChecks -c Release -p:Platform=x64
```

## Nghiệm thu

Logic/cấu trúc chạy trên Linux và cross-build WPF. Chưa thực thi desktop Windows, do đó các kiểm tra sau vẫn chờ: bút lấy màu nền trắng/đen/màu, biên màn hình âm và DPI100/125/150/175/200%, click/Esc không đổi clipboard, lấy đúng màu cho nhiều bảng và modal cấu hình, không tự chọn cả web khi chế độ Vùng chờ10giây, chế độ Modal vẫn nhận diện, flyout sáng/tối không tràn và stamp đúng ảnh xem trước, lưu/đổi ảnh/Undo sau thêm stamp.

## Kế tiếp

V1.28 — Licensing Foundation là đề xuất riêng về dùng thử30ngày, key kích hoạt có chữ ký số và khóa quyền tập trung; cần chốt chính sách internet và nguồn máy chủ trước khi xây dựng. Các cải tiến Quick Capture/Studio tiếp tục trong roadmap ảnh, chưa tự triển khai ở bản vá này.


---

## Lịch sử

# Captovix V1.27.1 — Document Save Recovery

Sửa lỗi V1.27.0: bộ đọc hỗ trợ schema15 nhưng DocumentStore.Save chặn priorVersion>14, báo sai “Không ghi đè sidecar từ phiên bản app mới hơn”. Reader/writer nay dùng chung EditorDocument.MaximumSupportedVersion=15. Vẫn từ chối ghi đè schema tương lai >15. Không xóa, đổi tên hoặc hạ version file người dùng. Không thêm chức năng bản quyền trong bản vá này.

Cập nhật: giữ bản hiện tại mở nếu còn chỉnh sửa chưa lưu; thử Xuất ảnh/Lưu dự án để giữ bản đang thấy trước khi đóng. Sau đó cài Setup trong Update-Packages hoặc phát hành đủ5asset như hướng dẫn nền, mở lại ảnh và Ctrl+S. Dữ liệu đã lưu trước lỗi được đọc lại; chỉnh sửa chỉ còn trong RAM mà chưa lưu/xuất không thể tự khôi phục sau khi đóng app.

Kiểm tra: CompositionChecks bổ sung roundtrip schema15, từ chối schema tương lai và hợp đồng giới hạn reader/writer. Fixture ImageStudioWindowsChecks bổ sung save15 hai lần, reopen/save, kiểm tra file tương lai không bị đổi. Fixture được cross-build, chưa thực thi trên Windows thật; không mô tả đã nghiệm thu luồng lưu thực tế. Build/publish Windows x64 và kiểm tra gói/updater thực tế trong bước đóng gói.

Bản kế tiếp dự kiến: Licensing Foundation theo phương án vừa mô tả, chỉ xây dựng sau khi Boss chọn chính sách internet và phạm vi triển khai máy chủ. Không tích hợp licensing vào hotfix lưu ảnh.


---

# Captovix V1.27.0 — Capture & Image Studio

Nâng cấp từ `Captovix-v1.26.0-Source.zip` Boss gửi. Bản1.27 theo phạm vi mới Boss chọn: nhận diện modal mượt hơn, viền chọn tương phản và nhóm công cụ chỉnh ảnh; kính lúp/nhập W×H trong roadmap cũ chưa triển khai ở bản này.

## Chụp phần tử

- Cache danh sách cửa sổ300ms và tái sử dụng hierarchy phần tử giữa các truy vấn; cache có thể cập nhật highlight trong lúc helper UIA đang bận. Giảm thời gian ổn định con trỏ từ120 xuống65ms, timer40ms; lần gợi ý đầu không chờ vòng truy vấn thứ hai, thay đổi khối vẫn có hysteresis.
- Không vẽ lại mọi lớp phủ ở mỗi MouseMove. Shade chỉ invalidate khi vùng hở đổi. Truy vấn UIA vẫn ở process cách ly, có timeout và hủy; Enter kiểm tra lại cửa sổ trước khi chụp.
- Khung chọn có nền viền đen và nét đứt trắng chuyển động: có thành phần tương phản đồng thời trên nền sáng, tối hoặc nhiều màu; không cần đọc màu màn hình liên tục. Viền và toolbar đóng trước khi chụp, không ghi vào PNG.
- Cải tiến mã nguồn chưa phải bằng chứng độ trễ trên Windows thật. Provider tự vẽ/remote có thể chỉ nhận diện được cửa sổ; kéo vùng thủ công vẫn dùng ngay trong cùng overlay.

## Công cụ và thao tác

Mở **Vùng pixel → Studio ảnh**; thanh công cụ chính/thư viện vẫn giữ gọn.

| Công cụ | Cách dùng / chức năng thực tế | Giới hạn |
| --- | --- | --- |
| Smart Guides | Khi kéo đối tượng, đường căn có sẵn nay thêm khoảng cách pixel tới đối tượng gần nhất theo ngang/dọc. Ctrl tạm bỏ bắt cạnh theo cơ chế cũ. | Chỉ hiển thị khi di chuyển; chưa tự phân bố khoảng cách bằng nhau. |
| Eyedropper | Chọn Viền/Nền/Chữ, zoom và click pixel ảnh đã render để áp dụng cho đối tượng đang chọn và mặc định công cụ. | Lấy một pixel; chưa trung bình vùng. |
| Clone | Alt + click chọn nguồn, rồi kéo cọ; nguồn được chụp lại đầu mỗi nét để không kéo lặp pixel mới tô. Chỉnh bán kính. | Cọ tròn cạnh cứng, chưa có healing/content-aware. |
| Phối cảnh | Kéo4góc theo thứ tự trái trên → phải trên → phải dưới → trái dưới; lưới hỗ trợ; bấm Xem trước phối cảnh rồi Áp dụng. | Tứ giác lồi; đầu ra giữ kích thước nền ảnh, có nội suy; chú thích không biến dạng theo. |
| Chỉnh nghiêng | Góc -45 tới45 độ, lưới và xem trước. | Giữ kích thước nền ảnh, phần ngoài ảnh bị cắt; có thể crop sau. |
| Điều chỉnh ảnh | Sáng, tương phản, nhiệt độ màu, bão hòa; xem ảnh khi mở, chốt bước và hoàn tác trong cửa sổ. | Xử lý SDR; không phải chỉnh màu HDR. |
| Sharpen | Điều chỉnh0–2 trong Màu sắc & làm nét; không thay kích thước. | Unsharp nhẹ theo lân cận3×3; không phục hồi chi tiết đã mất. |
| Stamp | Dấu đúng/sai/duyệt/cần sửa/phím tắt; đối tượng chữ và khung nhóm có thể sửa. Thêm logo/ảnh cá nhân qua chọn file; dùng Lưu mẫu bố cục để giữ tài nguyên. | Chưa có kho stamp riêng/import gói stamp. |
| Image Mask | Chọn lớp ảnh để đổi chữ nhật/ellipse/bo góc; giữ PNG nguồn để thay mask. Không chọn lớp thì áp mask vào nền, có Undo/khôi phục nền gốc. | Nền áp alpha vào PixelBase, muốn phục hồi dùng Undo hoặc Khôi phục nền ảnh gốc. |
| Comparison | Dùng ảnh hiện tại làm Trước, chọn file ảnh Sau; tạo bản ghép mới với2ô bằng nhau, nhãn và giữ tỷ lệ từng ảnh. | Chưa có thanh kéo before/after. |
| Watermark | Chữ hoặc logo,9vị trí, độ hiện. Trong Editor watermark là lớp ảnh di chuyển/resize được. Xuất hàng loạt hỗ trợ chữ/logo,9vị trí, độ hiện và lưu cấu hình. | Watermark chữ trong Editor raster hóa thành lớp ảnh; thay chữ bằng cách tạo lại. Khi batch chọn logo, logo thay cho watermark chữ. |
| Background Removal | Click xóa vùng nền liên thông theo màu/dung sai; cọ khôi phục lấy pixel ảnh khi mở. | Nền tương đối đồng nhất; chưa tách người/tóc bằng AI. Phần mô hình AI vẫn để giai đoạn Sau theo ưu tiên Boss. |
| Export Profiles | Tệp → Xuất hàng loạt/PDF: đặt tên và lưu cấu hình PNG/JPEG/PDF, kích thước, tên file, watermark; chọn lại hoặc xóa. | Tối đa30cấu hình trên máy; logo base64 tối đa8MB; file trước bị hỏng được ghi log. |

## Giữ ảnh và hoàn tác

Các chỉnh nền dùng `PixelBasePng`; ảnh gốc trên đĩa không bị ghi đè. Áp dụng vào Editor là một bước Undo; hủy cửa sổ không sửa tài liệu. Công cụ nền xử lý nền hiện tại, chú thích giữ nguyên vị trí. Biến đổi phối cảnh/chỉnh nghiêng có nội suy theo lựa chọn chủ động của người dùng. Zoom không thay kích thước xuất. Giới hạn nền40MP, PNG và tài liệu vẫn chịu giới hạn an toàn của PixelStudio. Trong cửa sổ chỉnh ảnh, giữ tối đa8bước và256MB snapshot; vượt giới hạn sẽ xóa lịch sử cục bộ cũ. Có nút Khôi phục ảnh khi mở. Menu Khôi phục nền ảnh gốc xóa toàn bộ chỉnh nền nhưng giữ chú thích; Undo phục hồi được.

Lớp ảnh có mask/opacity dùng document version15 để bản cũ từ chối thay vì âm thầm làm sai ảnh. Bản mới vẫn đọc tài liệu12–14; không cần reset thư viện. Sao lưu dữ liệu trước nâng cấp. Batch watermark cùng kích thước ghép overlay vào pixel gốc, vùng ngoài watermark giữ byte; resize chỉ thực hiện khi cấu hình xuất yêu cầu.

## Cài và phát hành

- Giải nén `Captovix-v1.27.0-Update-Packages.zip`, đóng app và chạy `HuyVo.Captovix-win-Setup.exe` để cài trực tiếp. Installer chọn đường dẫn hiện có; chữ ký số chưa có.
- Để app cập nhật qua nguồn đã gắn: đăng đủ5asset (full.nupkg, Setup.exe, RELEASES, assets.win.json, releases.win.json) lên Release stable/latest tag `v1.27.0` của repository phát hành hiện có. Không chỉ đăng ZIP hoặc Source.zip. Chưa tự publish.
- Gửi Source ZIP1.27.0 cho lần nâng cấp sau. Build SDK10.0.100, WPF Windows x64, runtime10.0.13 tự chứa; dependency không đổi.

```powershell
dotnet build src/Captovix/Captovix.csproj -c Release -p:Platform=x64
dotnet run --project tests/ImageStudioChecks -c Release
dotnet run --project tests/SmartCaptureChecks -c Release
dotnet run --project tests/CompositionChecks -c Release
dotnet run --project tests/BatchChecks -c Release
# Windows thật:
dotnet run --project tests/ImageStudioWindowsChecks -c Release -p:Platform=x64
dotnet run --project tests/SmartCaptureWindowsFixture -c Release -p:Platform=x64
```

## Kiểm chứng và giới hạn bàn giao

Đã build/publish và chạy các kiểm tra logic pixel, geometry/hysteresis, composition/schema, fidelity, connector/probe, shell, updater, object-control và batch trên Linux. Các dự án Windows được cross-build; chưa chạy WPF/overlay/UIA/GDI, cài đặt và update GUI trên Windows. Không khẳng định đã đạt độ mượt trên máy Boss hoặc đã nhận diện đúng mọi ứng dụng.

Nghiệm thu Windows: thử modal/bảng ở Edge/Chrome, ứng dụng Win32 và UltraViewer; di chuyển nhanh qua con/cha trong lúc provider bận; đứng yên không nhảy khối; nền trắng/đen/màu; click khóa và resize; Enter1lần. Thử từng mode Studio, Cancel/Apply/Undo/Redo, đổi ảnh, ảnh alpha, xuất lại dự án15, batch nhiều ảnh với profile logo9vị trí. Kiểm tra DPI100–200%, nhiều màn hình âm và restore app sau update.

## Bản tiếp theo đề xuất

**V1.28 — Quick Capture & Studio Polish:** chụp lặp/vùng gần đây, lựa chọn thả để chụp hoặcEnter; cải thiện preview/cọ, quản lý stamp cá nhân riêng và nghiệm thu độ trễ/độ chính xác trên bộ ứng dụng Windows mục tiêu. Tách chủ thể AI tiếp tục đánh giá riêng.


---

## Lịch sử (phạm vi V1.27 ở trên thay thế đề xuất V1.27 cũ)

# Captovix V1.26.0 — Smart Element Capture

Nâng cấp từ source `Captovix-v1.25.0-Source(1).zip` Boss gửi. Bản này ưu tiên Smart Element Capture thay cho đề xuất Composition trước đó. Source và gói cài/cập nhật đã xây dựng; nghiệm thu tương tác trên Windows thật còn chờ. Không tự đăng GitHub Release.

## Thay đổi

- Chế độ Vùng và Phần tử dùng cùng một lớp phủ. Ảnh vùng cũng bật nhận diện; video giữ kéo vùng thủ công.
- Rê để gợi ý khối. Con lăn lên / phím ↑ hoặc ←: khối cha; xuống / ↓ hoặc →: khối con. Thứ tự dựa trên cây lồng nhau, có chỉ số cấp.
- Click khóa khối, rồi kéo 8 tay nắm chỉnh cạnh/góc; kéo trong khối đã khóa để di chuyển. Góc kéo tự do; Shift giữ tỷ lệ. Enter chụp một lần, không mở bước xác nhận thứ hai.
- Trước khi khóa, giữ chuột kéo luôn tạo vùng thủ công ngay cả trên khối đang gợi ý; không cần thoát chế độ. Tab bỏ khóa/chọn lại; S bật/tắt nhận diện; Esc/chuột phải hủy.
- Sau khóa: phím mũi tên dịch 1 pixel; Shift + mũi tên 10 pixel; Ctrl + mũi tên thay kích thước (Ctrl+Shift: 10 pixel).
- Gợi ý đổi khối phải có hai kết quả xác nhận liên tiếp; giữ cấp cha/con người dùng đã chọn khi còn phù hợp. Không xóa khung chỉ vì bắt đầu truy vấn. UI Automation chạy trong tiến trình phụ có giới hạn thời gian; bổ sung metadata child HWND cho ứng dụng Win32 cũ. Cửa sổ đổi vị trí phải chọn lại trước chụp.
- Bỏ hai bảng hướng dẫn/nút lớn cố định ở góc trên. Thay bằng một thanh nhỏ có camera + Enter, thông tin kích thước/cấp và tooltip hướng dẫn. Ưu tiên ngoài vùng chụp, tự đổi phía khi sát cạnh. Ẩn toàn bộ thanh khi kéo; khung chỉ giữ tay nắm nhỏ. Khi chọn kín màn hình, không có chỗ ngoài vùng: thanh nhỏ đặt sát đáy, đóng trước lấy ảnh.
- Thanh thư viện dọc/ngang chỉ có **Xóa – Quản lý thư viện – Đổi hướng**. Icon đổi hướng thể hiện bố cục đích, tooltip ghi rõ chuyển ngang hay dọc. Tìm, khôi phục, thư mục lưu và xuất vẫn truy cập qua menu Tệp.
- Pixel ảnh vùng/phần tử/toàn màn hình lấy trực tiếp bằng BitBlt, không StretchBlt. Worker đặt PerMonitorV2 và khôi phục DPI context sau chụp. Kiểm tra lại toàn bộ byte BGRA sau lưu PNG; sai kích thước/pixel thì báo lỗi, không âm thầm resize.
- Giảm khoảng trống quanh ảnh trong Editor từ48 xuống24 DIP để ảnh vừa khung lớn hơn. Ảnh nhỏ mặc định không phóng lớn vượt100% pixel; ảnh lớn vẫn thấy toàn bộ ở chế độ vừa khung. Tooltip zoom giải thích cách kiểm tra ở100%. Zoom không tham gia pipeline xuất.

## Giới hạn và kiểm chứng

Lấy đúng pixel không tạo thêm chi tiết chưa tồn tại trên màn hình. Preview vừa khung dưới100% phải thu nhỏ; đánh giá độ nét ở **Rõ nét · 100%** hoặc mở PNG gốc. Không tự sharpen/upscale vì có thể đổi chữ, viền và QR. UltraViewer/Remote Desktop/emulator có thể đã scale/nén ảnh trước khi Captovix chụp; app không khôi phục pixel gốc của máy từ xa. Màu HDR/wide gamut, nội dung bảo vệ, cửa sổ bị che và provider không hỗ trợ là trường hợp đánh giá riêng. Backend cửa sổ native và quay video giữ nguyên.

UIA phụ thuộc ứng dụng đích. Trình duyệt cần cung cấp cây accessibility; app tự vẽ/remote có thể chỉ cung cấp toàn cửa sổ. Khi đó tắt S hoặc kéo chọn thủ công ngay. Không cam kết nhận diện mọi modal/bảng trong mọi ứng dụng.

Đã chạy kiểm tra logic trên Linux và cross-build WPF. Chưa chạy overlay, UIA ứng dụng thật, GDI/PNG thực tế, clipboard, DPI màn hình thật hoặc installer GUI trên Windows trong lần bàn giao này. Xem `docs/VALIDATION.md` và checklist bên dưới.

## Cách cài / cập nhật

1. Sao lưu thư viện và cấu hình. Đóng Captovix.
2. Giải nén `Captovix-v1.26.0-Update-Packages.zip`, chạy `HuyVo.Captovix-win-Setup.exe` để cài trực tiếp; có chọn đường dẫn theo installer hiện có.
3. Để update từ app: đăng đủ5asset trong ZIP lên Release stable/latest tag `v1.26.0` của `huyvd250797/captovix-releases`: full.nupkg, Setup.exe, RELEASES, assets.win.json, releases.win.json. Không chỉ đăng ZIP hoặc source. Chọn Kiểm tra cập nhật trong app.
4. Source dùng phát triển; không dùng Source.zip làm gói update. Bộ cài chưa ký số. Không ghi đè dữ liệu để xử lý lỗi cài.

## Dựng lại

Giải nén Source ZIP, mở thư mục `Captovix-v1.26.0`. Dùng .NET SDK10.0.100, NSIS3 và vpk1.2.161; runtime self-contained10.0.13 đã pin. Không đổi dependency trong bản này.

```powershell
dotnet build src/Captovix/Captovix.csproj -c Release -p:Platform=x64
dotnet run --project tests/SmartCaptureChecks -c Release
dotnet run --project tests/ConnectorChecks -c Release
dotnet run --project tests/ImageFidelityChecks -c Release
dotnet run --project tests/ShellLayoutChecks -c Release
dotnet run --project tests/InstallerContractChecks -c Release -- .
# Chạy trên Windows để kiểm tra renderer/PNG (không thay cho bộ test desktop):
dotnet run --project tests/FidelityWindowsChecks -c Release -p:Platform=x64
# Fixture tương tác modal/bảng; mở Captovix riêng để thử chụp:
dotnet run --project tests/SmartCaptureWindowsFixture -c Release -p:Platform=x64
```

Các script build/package Windows có trong `scripts/`. Giữ AppID, baked update-source, schema dữ liệu và logo đã duyệt. Xem hướng dẫn nền bên dưới về build/install/publish; chỉ dẫn phiên bản cũ là lịch sử, version hiện tại1.26.0.

## Nghiệm thu Windows cần thực hiện

| Bộ ứng dụng / điều kiện | Thao tác | Kết quả cần đạt | Trạng thái |
| --- | --- | --- | --- |
| Fixture WPF trong source | Modal, DataGrid, child/parent, UIA | Khối đúng vị trí/pixel, không phát sinh click vào ứng dụng đích | Chờ chạy Windows |
| Edge/Chrome trang có modal và bảng | Rê giữa block, gần cạnh modal; wheel cha/con; khóa rồi resize | Chọn đúng cấp đã chủ động chọn, không nhảy cấp khi đứng yên | Chờ chạy Windows |
| Explorer / ứng dụng Win32 | Cửa sổ, bảng, vùng con; fallback thủ công | Không treo provider; kéo vùng được ngay | Chờ chạy Windows |
| UltraViewer / ứng dụng tự vẽ | Provider chỉ có khung ngoài | Không nhận nhầm widget máy từ xa; kéo chọn bằng tay thành công | Chờ chạy Windows |
| DPI100/125/150/175/200%, màn hình tọa độ âm | Chọn/resize/move xuyên màn hình, Enter/Esc | Tọa độ và PNG đúng pixel; không còn toolbar/shade trong ảnh | Chờ chạy Windows |
| Cửa sổ đích di chuyển trong khi khóa | Di chuyển cửa sổ rồi Enter | Từ chối vùng stale, yêu cầu chọn lại | Chờ chạy Windows |
| GDI pattern và ảnh chữ/QR ở100% | Trợ giúp → Kiểm chứng chất lượng ảnh | PNG đúng pixel, crop/rotate/copy/export không resize ngoài ý muốn | Chờ chạy Windows |
| Thư viện hai hướng, filmstrip96–260DIP | Đổi hướng, kéo splitter, xóa đa chọn | Luôn đúng3nút, icon đích đúng và đủ chỗ | Chờ chạy Windows |
| Update từ1.25.0 và Setup ở ổD | Kiểm tra cập nhật/cài tại D:\Captovix\Captovix | Đúng1.26.0, giữ media/style/theme/logo/phím tắt | Chờ chạy Windows |

## Tiếp theo đề xuất

**V1.27 — Precision Selection & Magnifier:** kính lúp theo trỏ, nhập W×H bằng pixel, khóa riêng chiều rộng/cao, tỷ lệ thông dụng, bắt cạnh có thể tạm bỏ qua. Chỉ triển khai sau khi Boss chọn và sau nghiệm thu lớp phủ1.26 trên bộ ứng dụng mục tiêu.


---

## Tài liệu các phiên bản trước

# Captovix V1.25.0 — Scrolling Capture Quality

## Chụp cuộn

- Tìm overlap hai bước: lấy ứng viên nhanh rồi kiểm tra dày hơn, gồm lân cận, để giảm nhận nhầm điểm nối hoặc khung trùng. Giữ copy pixel nguyên bản, không blend/resize tại mối nối.
- Tự cuộn chờ theo cấu hình, rồi so tối đa3cặp khung cách180ms để kiểm tra nội dung đã đứng yên. Nếu còn chuyển động, tạm dừng; Tiếp tục thử lại vị trí hiện tại trước khi cuộn thêm. Có chế độ lấy thủ công cho ứng dụng khó.
- Header/footer khi cuộn dọc, hoặc dải trái/phải khi cuộn ngang: nhập pixel đầu/cuối, bỏ khỏi phép ghép, tùy chọn giữ dải đầu khung đầu và dải cuối khung cuối đúng một lần. Phiên ngang có vùng cố định mới không mở bằng bản cũ; ảnh kết quả PNG vẫn mở bình thường.
- Khung trùng không append;3lần không có nội dung mới dừng tự động, vẫn cảnh báo có thể nguồn không nhận cuộn. Khung không chắc dừng tự động, yêu cầu xác nhận/sửa overlap trước xuất; kích thước thay đổi bị từ chối.
- Xem cặp khung tại điểm nối bằng vùng cuộn pixel; khung trước mở ở cuối hướng cuộn, khung sau ở đầu. Có toàn ảnh Fit/100%, guide opt-in không vào file xuất. Sửa overlap/loại khung giữ nguồn, revision và phiên hash để mở/ghép lại.
- Giới hạn giữ24khung,4MP/khung,64MPnguồn,40MPkết quả,32768px/cạnh theo pipeline hiện có. Không cam kết trang động, video, tải vô hạn hoặc mọi ứng dụng; không phải công cụ tự đọc toàn DOM.

## Cập nhật và giao diện

- Khi tự kiểm tra đang bật: kiểm tra nguồn khi mở app, không bị mốc đã kiểm24giờ của phiên trước chặn; trong phiên tối đa mỗi30phút. Popup một lần mỗi phiên bản trong phiên chạy, chờ Editor có thể tương tác; Để sau/đóng thu về nút cập nhật, Cập nhật mở trang kiểm tra/cập nhật.
- Tôn trọng tùy chọn tắt tự kiểm tra. Popup chỉ xuất hiện khi nguồn trả về phiên bản cao hơn. Release phải stable/latest và có đủ5asset, không chỉ SourceZIP/UpdateZIP; lỗi mạng/feed hiện trong trang cập nhật và log, không báo sai là đã mới nhất. Chưa tự publish.
- Màn hình trống chỉ giới thiệu Captovix bằng logo/nội dung, bỏ hai dòng shortcut cố định và tooltip cứng. Phím tắt cấu hình vẫn hoạt động theo cài đặt máy.
- Rê viền vùng chụp hiện con trỏ hai đầu theo cạnh/góc; chỉnh xong Enter/nút máy ảnh chụp. Giữ viền nét đứt từ1.24.
- Dải thư viện ngang: nút icon đầy đủ chuyển sang cụm gọn bên cạnh ảnh; bỏ dấu… trong chế độ này, thư viện dọc vẫn giữ overflow. Thanh phân cách có nền và con trỏ resize; kéo thay chiều cao96–260DIP, lưu trên máy; lần đầu từ cấu hình cũ thu về tối đa112DIP. Thumbnail co theo chiều cao, không dùng hàng riêng cho nút.
- Footer căn giữa theo chiều dọc để copyright, trạng thái và nút update thẳng hàng.

## OCR gọn

Mở OCR → kéo vùng trên ảnh → thả chuột tự nhận chữ → sửa/copy kết quả. Mở đầu không phủ tím toàn ảnh. Chỉ giữ ảnh có zoom, chọn toàn ảnh/nhận lại/hủy, ô kết quả và sao chép/đóng. Mặc định Việt+Anh, tự phân tích bố cục, xử lý trên máy. Bỏ danh sách dòng, thay chữ/nền/font, tách lớp, preview/apply và chọn bố cục/ngôn ngữ khỏi giao diện OCR. Không chỉnh bitmap nguồn. Hủy/đóng vẫn chờ worker kết thúc; lỗi hiện thông báo cụ thể.

## Kiểm chứng và cài đặt

6bộ logic/static đạt232assertions:21mới về overlap/native pixel/transpose/vùng cố định/cancellation; thêm4policy kiểm tra cập nhật theo phiên.3bộ WPF cross-build trên Linux; main Windows x64 self-contained.NET10.0.13 build/publish. Chạy UpdateChecks với nupkg thật và Verify-Packages sau đóng gói. **Chưa chạy Windows desktop hoặc nghiệm thu ảnh cuộn thực tế.** Số liệu bản cũ bên dưới là lịch sử.

Nghiệm thu Windows cần: bộ trang mẫu văn bản/bảng dọc-ngang có header/footer, không mất/lặp dòng/cột; trang tải chậm/animation và retry tại cùng vị trí; cuối trang/Cancel/restore/loại khung; mối nối/PNG đúng pixel; popup khởi động và sau30phút/opt-out; resize con trỏ/DPI100–200%/monitorâm; dải ngang thấp/cao và tất cả nút; OCR chọn vùng/zoom/worker/copy; cài/update/logo/dữ liệu. Chưa có screenshot giao diện mới chạy thật. Không suy diễn kiểm tra tổng hợp thành hỗ trợ mọi app.

Giải nén Update-Packages, đóng app, chạy HuyVo.Captovix-win-Setup.exe. Để phát hành: đăng đủ5asset lên huyvd250797/captovix-releases, tagv1.25.0, stable/latest. Không đổi tên/byte. Backup dữ liệu trước nâng cấp; giữ media/settings/logo/stable source. Setup chưa ký số.

## Tiếp theo V1.26 — Image Composition Studio

Ghép ảnh theo hàng/cột/lưới, chỉnh khoảng cách/nền/viền/bóng, tiêu đề và đánh số bước, mẫu bố cục dùng lại. Ưu tiên patch phản hồi Windows1.25 trước.

## Lịch sử phiên bản trước

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

# Captovix V1.23.1 — Resize & Capture UX

- Kéo đối tượng mặc định tự do; chỉ Shift giữ tỷ lệ. Bỏ ép tỷ lệ theo loại đối tượng, giữ giới hạn cỡ chữ và thân callout.
- Chọn vùng pixel chữ nhật/ellipse có tay nắm chỉnh kích thước; vùng chụp màn hình giữ sau thả chuột, kéo lại cạnh/góc hoặc chọn lại, Enter/nút icon máy ảnh xác nhận; Esc hủy.
- Fit ảnh sử dụng nhiều diện tích hơn, không thay pixel xuất.
- Theo dõi phần canvas tự nới riêng; kéo vào trong/xóa đối tượng sẽ tính lại biên toàn bộ annotation nhìn thấy, giữ phần mở rộng thủ công cũ. Những margins đã tạo từ bản1.23.0 chưa có dấu tự động được giữ, dùng Crop để bỏ một lần.
- Popup update khi app hoạt động lại; Cập nhật mở trang cập nhật, không tự tải ngay; Để sau/đóng dùng animation về nút cập nhật. Animation tôn trọng thiết lập Windows.
- Ẩn mục Preset cũ và Trình bày toàn ảnh ở thanh phải; dữ liệu mẫu cũ giữ nguyên. Giữ công cụ ngang, thuộc tính bên phải và tùy chọn thư viện nằm dưới; nhận diện tím Captovix.

Chưa nghiệm thu Windows thật: resize/text/IME, capture đa màn hình/DPI, taskbar, animation, install/update. Không tuyên bố giao diện sao chép hoặc tương đương hoàn toàn Snagit. Resize chữ giữ glyph theo font, không kéo méo glyph theo hai trục; không có affine text deformation.

# V1.23.1 — Text & Callout Refinement ·27/10/2026

- Ảnh mới/mở lại mặc định Vừa khung. Vùng thao tác quanh ảnh có nền workspace, không hiển thị checkerboard như một khung ảnh lớn; kích thước xuất vẫn theo pixel ảnh/canvas thực.
- Vẽ hoặc kéo chú thích ra ngoài sẽ nới đúng cạnh bị vượt theo biên render, gồm viền, đuôi và bóng. Một callout ở trên không tự thêm viền trái/phải/dưới; nới và đối tượng chung một Undo.
- Sửa chữ trực tiếp trên ảnh bằng ô nhập trong suốt, không nền đen/viền ô nhập, không chồng hai lớp chữ. Ctrl+Enter hoàn tất, Enter xuống dòng, Esc hủy; bấm ngoài hoàn tất và về Chọn. Ctrl+B/I/U đổi kiểu toàn đối tượng.
- Callout co chiều cao theo chữ và đệm, giữ rộng người dùng chọn khi đủ chỗ; đuôi tự do giữ đích khi đổi nội dung. Mẫu và ảnh dùng chung hình học/đệm/giãn dòng/căn lề; đuôi bo tròn nối theo biên thật.
- Thanh phải chia thẻ Màu & kích thước → Hình dạng & kiểu nét → Chữ & bố trí → Hiệu ứng; cài đặt sâu gấp gọn. Font gõ/tìm được, B/I/U dạng nút, mẫu đã lưu theo công cụ.
- Tên ảnh/thời gian gọn, ngày đầy đủ trong tooltip; icon có khoảng đệm. Menu nhóm theo thao tác thực tế. Caption cả ba nút có hover; cửa sổ tối đa theo work area Windows để footer không nằm dưới taskbar.

- Thêm kiểm tra biên canvas/render/text và work area. Windows runtime acceptance còn chờ; xem docs/VALIDATION.md.

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
