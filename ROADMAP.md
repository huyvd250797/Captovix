# Roadmap Captovix — cập nhật V1.36.0

Giữ roadmap ảnh gần nhất đã chốt; video tính sau.

| Phiên bản | Tên | Nội dung chính | Kỹ thuật/dữ liệu | Điều kiện hoàn thành | Trạng thái/phụ thuộc |
| --- | --- | --- | --- | --- | --- |
| V1.30.0 | Capture Fidelity & Reliability | Snapshot/pixel, DPI, Editor/copy/PNG, Vừa khung/100% | Snapshot vật lý | Pixel đúng, multi-DPI | Đã bàn giao |
| V1.31.0 | Quick Capture Experience | Toolbar gọn, chỉnh biên, Enter, kính lúp, Crop/Step/Stamp | Preferences/công cụ | Luồng chụp nhanh | Đã bàn giao |
| V1.32.0 | Smart Element Capture Refinement | Phần tử/modal, kéo vùng, chữ/stamp, KEY/hotkeys | Cache/probe, schema17 chữ | Nhận diện và chỉnh sửa ổn định | Đã bàn giao |
| V1.33.0 | Scrolling Capture Quality | Mối nối, mất/lặp dòng/lệch cột, chờ ổn định, header/footer, sửa/bỏ khung; UI/drag/license theo Boss | Client; backend trial không reset đã có, không DB migration | Logic/build/package; nghiệm thu Windows theo checklist | Đã triển khai; logic/build đạt, native Windows còn chờ |
| V1.34.0 | Capture Workflow & Recovery | Global hotkey, vùng gần đây, menu hẹn giờ, vị trí lưu, phục hồi và lỗi lưu | Capture/session/lưu file | Chụp liên tục, lỗi không mất ảnh | Bỏ qua phát hành theo yêu cầu Boss; không mở rộng phạm vi |
| V1.35.0 | Object Editing Precision | Chọn/kéo/resize, đa chọn/nhóm/khóa/căn chỉnh/Smart Guides, canvas tự khớp | Object geometry/history | Undo/Redo đúng hình/vị trí | Bỏ qua phát hành theo yêu cầu Boss; giữ chức năng sẵn có |
| V1.36.0 | Text & Callout Polish | IME/chữ trực tiếp, hộp/đuôi độc lập, mẫu thuộc tính | Typography/geometry | Nhập/sửa/lưu đúng bố cục | Đã triển khai; logic/build/package đạt, native Windows còn chờ |
| V1.37.0 | Image Tools & OCR Usability | Kéo vùng nhận chữ/copy, zoom/pan, màu/làm nét/Clone/phối cảnh, trước/sau/hủy | Pixel tools | Xem trước và hoàn tác đáng tin | Chưa triển khai toàn phạm vi |
| V1.38.0 | Library & Export Experience | Thumbnail/tìm kiếm/OCR, chọn nhiều/xóa/khôi phục, xuất hàng loạt | Library/export | Cuộn mượt, xuất đúng cấu hình | Chưa triển khai toàn phạm vi |
| V1.39.0 | Image Performance & Release Quality | Mở/chuyển/render/RAM; theme/icon/menu/lỗi; hồi quy Windows/nâng cấp/phục hồi | Performance/QA | Đo và nghiệm thu bộ tiêu chí | Chưa triển khai |

Capture Presets & Repeat không thay thế phiên bản nào của roadmap này.


Đi thẳng V1.33→V1.36; bản kế tiếp V1.37 giữ nguyên tên/phạm vi đã chốt. Không tạo roadmap thay thế.

## Lịch sử



# Roadmap Captovix — V1.32.0

| Phiên bản | Mục tiêu | Chức năng chính | Kỹ thuật/dữ liệu | Điều kiện hoàn thành | Trạng thái/phụ thuộc |
| --- | --- | --- | --- | --- | --- |
| V1.32.0 | Smart Element Capture Refinement | Chụp mặc định; X/Y vàng cam; popup KEY; settings/hotkeys; stamp/chữ/arrow; scroll thống nhất | Hierarchy cache/cancel stale probe; schema17 riêng cho fixed text; config license giữ nguyên | Logic/build/package đạt; nghiệm thu Windows native theo checklist | Đã triển khai; logic/build đạt, native Windows còn chờ |
| V1.33.0 | Scrolling Capture Quality | Sửa sọc/mất/lặp dòng, header/footer cố định, cảnh báo mối nối, phục hồi phiên | Stitching và kiểm tra khung; không đổi license | Bộ trang mẫu dọc/ngang ghép đủ nội dung; hủy/lỗi phục hồi được | Chưa xây; sau V1.32 |
| Backlog | Capture Presets & Repeat | Nhập X/Y/W/H trong selector, tỷ lệ, vùng gần đây | Pixel geometry/topology | Chọn nhanh, không vượt giới hạn | Để sau theo roadmap Boss chọn |

Không thay mô hình đóng gói/dung lượng trong V1.32. Hai ZIP bàn giao; không migration/backend redeploy. Video giữ phạm vi hiện có.

## Lịch sử

# Roadmap Captovix — V1.31.0

| Phiên bản | Mục tiêu | Chức năng | Kỹ thuật/dữ liệu | Điều kiện hoàn thành | Trạng thái/phụ thuộc |
| --- | --- | --- | --- | --- | --- |
| V1.30.0 | Capture Fidelity & Reliability | Snapshot/pixel, zoom rõ ràng | Giữ nền capture và config | Ma trận Windows/multi-DPI | Source nền đã đối chiếu update Boss gửi |
| V1.31.0 | Quick Capture Experience | Toolbar gọn, crosshair, lens nhỏ, keyboard capture, nhớ mode; crop release; Step paints; stamp gallery/dynamic tool; update notes | Client preferences, optional schema16 cho kiểu Bước; không DB/backend change | Chuột/phím chụp Enter một lần; overlay không vào ảnh; crop/Undo; paint/stamp/recent tool đúng; Windows QA | Đã triển khai; build/logic xác minh ở môi trường phát triển, Windows native còn chờ |
| V1.32 (đề xuất) | Capture Presets & Repeat | Tỷ lệ/kích thước, nhập X/Y/W/H ngay selector, vùng gần đây/chụp lại | Preferences theo máy, pixel geometry và topology guard | Chọn nhanh không vượt màn hình, giữ pixel, mouse/keyboard parity | Chưa xây; sau khi Boss chọn |

Giữ backlog key rotation/MFA/ops của licensing; không triển khai ngầm. Hai ZIP bàn giao; không cần migration mới hoặc redeploy backend/admin. Không tự đăng GitHub Release.

## Lịch sử

# Roadmap Captovix — cập nhật V1.30.0

| Phiên bản | Mục tiêu | Chức năng chính | Kỹ thuật/dữ liệu | Điều kiện hoàn thành | Trạng thái/phụ thuộc |
| --- | --- | --- | --- | --- | --- |
| V1.29.1 | Serverless Licensing | API Supabase, admin Pages, app public config | Giữ authority/dữ liệu đã có | Hệ thống bản quyền nghiệm thu trên Windows | Source Boss gửi làm nền; không deploy lại |
| V1.30.0 | Capture Fidelity & Reliability | Giữ ảnh trước selector, pixel magnifier; rõ Vừa khung/100%; PNG/zoom pixel invariant | Snapshot RAM, GDI vật lý, bounded memory, topology/DPI guard, kiểm tra PNG | PNG đúng kích thước; pixel không chỉnh giữ nguyên; đổi zoom không đổi output; ma trận Windows100–200% và nhiều màn đạt | Đã triển khai; build/logic kiểm tra ở Linux; nghiệm thu Windows thật còn chờ |
| V1.31 (đề xuất) | Capture Workflow & Precision | Nhập X/Y/W/H ngay selector; preset kích thước/tỷ lệ; vùng gần đây ngay selector | UI/persistence client, không buộc DB mới | Luồng chụp nhanh/canh chính xác, giữ pixel; Windows QA | Chưa xây; sau V1.30 |

Boss chọn V1.30 cho capture thay đề xuất Licensing Hardening trước đó. Key rotation/MFA/ops vẫn là backlog, không triển khai ngầm trong bản này. Bàn giao hai ZIP; không cần migration mới, không push/deploy/publish khi chưa giao.

## Lịch sử

# Bản cấu hình Windows V1.29.1

Đã nhận public config của Boss; nhúng vào app Windows, kiểm tra endpoint health/ready. Không cần chạy migration mới. Backend/admin do Boss triển khai trước đó; lần này không deploy lại. Nghiệm thu kích hoạt/trial/chuyển máy trên Windows còn chờ.

Phiên bản tiếp theo đề xuất vẫn là V1.30 — Licensing Hardening & Release: luân chuyển khóa ký, MFA/phân quyền admin, giám sát dịch vụ và phục hồi thiết bị. Chưa triển khai.

# Roadmap hiện tại

| Phiên bản | Mục tiêu | Nội dung | Kỹ thuật/dữ liệu | Điều kiện hoàn thành | Trạng thái/phụ thuộc |
| --- | --- | --- | --- | --- | --- |
| V1.29.0 | Licensing Operations & Recovery | Trial/key, trả máy, recovery, admin/audit | WPF + C# API/PostgreSQL | Backend có cấu hình, nghiệm thu Windows | Source nền; backend C# được thay thế trong V1.29.1 |
| V1.29.1 | Serverless Licensing | Supabase Edge/RPC, Auth allowlist, Pages admin, offline keygen, Actions build | TypeScript WebCrypto tương thích C#, migration additive, rate/nonce/slot DB | Supabase/Pages cấu hình thật, build public config, nghiệm thu nhiều máy Windows | Đã xây source/build/test; chưa deploy production và Windows runtime acceptance |
| V1.30 (đề xuất) | Licensing Hardening & Release | Key ring/rotation, MFA/quyền admin chi tiết, giám sát quota/backup, phục hồi/hardware | Migration tăng dần, staging và key transition an toàn | Nghiệm thu Windows/Cloud trên môi trường xác định, không reset dữ liệu | Chưa xây; sau khi V1.29.1 được cấu hình và nghiệm thu |

Bàn giao mỗi bản đúng hai ZIP Update-Packages và Source; Setup.exe ở trong Update-Packages. Không tự xuất bản review chưa cấu hình. App ảnh/capture hiện có được giữ; dịch vụ license không lưu ảnh/video.

## Lịch sử

# Roadmap hiện tại

| Phiên bản | Mục tiêu | Chức năng | Kỹ thuật/dữ liệu | Điều kiện hoàn thành | Trạng thái/phụ thuộc |
| --- | --- | --- | --- | --- | --- |
| V1.27.2 | Chọn màu, truy cập Studio, capture thủ công | Eyedropper, Thêm/Stamp, tách region/modal | WPF, schema15 giữ nguyên | Build + package đã kiểm tra; Windows cần nghiệm thu | Source nền đã giữ |
| V1.28 → gộp V1.29 | Nền tảng bản quyền | Trial, key, chứng nhận, khóa công cụ | RSA-PSS/DPAPI/API/PostgreSQL | Có API HTTPS, public config và test Windows | Đã xây source trong V1.29; chưa triển khai production |
| V1.29.0 | Licensing Operations & Recovery | Trả máy, lost-device recovery, cooldown, admin key/slot/trial/audit | Role RLS, nonce, giao dịch chống vượt slot, guide/compose/bootstrap | Cài/gỡ/update/DPAPI và nhiều máy Windows qua backend thật | Source/build/package review có; production URL/private key chưa cấu hình; Windows chưa nghiệm thu |
| V1.30 (đề xuất) | Licensing Hardening & Release | Luân chuyển khóa ký, nhiều admin/MFA, proxy/rate limit lớn, quy trình hỗ trợ lỗi lease | Key ring, danh tính người vận hành, telemetry chỉ metadata, migration tăng dần | QA Windows và phục hồi/rollback trên staging, không reset data | Chưa xây; sau khi V1.29 được cấu hình và nghiệm thu |

Mỗi bản tiếp tục bàn giao hai file Update-Packages ZIP (chứa Setup.exe) và Source ZIP. Không phát hành package chưa gắn dịch vụ license; app không tự cấp trial/perpetual khi API lỗi. Các công cụ ảnh và cải thiện capture trước đó được giữ, không thay bằng licensing.

## Lịch sử roadmap

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

# Roadmap Captovix — ưu tiên trải nghiệm chụp ảnh

Kế hoạch này thay thế đề xuất V1.26 Composition trong lịch sử bên dưới. Boss chọn nâng cấp Smart Element Capture trực tiếp từ1.25.0, bản phát hành kế tiếp mang số **1.26.0**; không tăng số nhảy theo tên đề xuất trước đó.

| Phiên bản | Mục tiêu | Chức năng chính | Công việc kỹ thuật / dữ liệu | Điều kiện hoàn thành | Phụ thuộc / trạng thái |
| --- | --- | --- | --- | --- | --- |
| V1.26 — Smart Element Capture | Chọn modal, bảng, khối thuận tiện | Một lớp phủ; cha/con; click khóa; kéo vùng fallback;8handles; toolbar gọn;3nút thư viện; PNG pixel check | Session dùng chung, helper UIA cách ly, HWND fallback, hysteresis, physical pixel math; giữ schema | Logic/build pass; overlay/provider/capture trên bộ ứng dụng thật đạt checklist | Đã xây dựng, kiểm tra tự động; chờ nghiệm thu Windows |
| V1.27 — Precision Selection & Magnifier | Căn vùng nhỏ chính xác | Kính lúp, nhậpW×H, khóa chiều/tỷ lệ, edge snap có bypass | Model ràng buộc theo pixel và UI nhập gọn | Nhập/kéo cho đúng pixel ở các DPI | Đề xuất sau1.26 |
| V1.28 — Quick Capture & Repeat | Chụp lặp ít thao tác | Tùy chọn thả để chụp hoặcEnter; vùng gần đây có thumbnail; vùng lưu tên; chụp liên tiếp | Profile capture theo máy, chốngchiếmfocus, repeat topology | Chụp lại đúng vùng; không lặp hotkey/chiếmfocus | Đề xuất |
| V1.29 — Capture & Share Flow | Hoàn thành nhanh sau chụp | Thanh nhỏ Copy/Save/Editor/OCR/Chụp tiếp; hành vi mặc định theo chế độ | Luồng hậu xử lý, phục hồi clipboard/lưu lỗi | Ảnh không mất khi thao tác chia sẻ lỗi | Đề xuất |
| V1.30 — Delayed & Transient Capture | Chụp menu và tooltip dễ hơn | Countdown, con trỏ, đóngbăngmànhình có phân biệt live/frozen | Snapshot và lifecycle overlay; hủy/retry | Không chụp lớpphủ, giữ đúng menu theo chế độ | Đề xuất |
| V1.31 — Guided Scrolling Capture | Chụp dài dễ hiểu | Chọn dải cố định trựcquan; tiếntrình/pause/stop; đánhdấumốinối; chụplạikhunglỗi | Flow và dữ liệu review nối trên nền1.25 | Trang mẫu không mất/lặp dòng/cột | Đề xuất |
| V1.32 — Capture Reliability & Polish | Ổn định trên Windows mục tiêu | Ma trận app/DPI/multimonitor; độtrễ/flicker/focus; log hỗtrợ | Benchmark và kiểmthử Windows lặp lại | Bằngchứng pixel và UX đạt từng trườnghợp | Đề xuất; kiểm tra cơ bản vẫn bắt buộc mỗi bản |

Chỉnh sửa ảnh và template tiếp tục sau nhóm ưu tiên capture; video phát triển sau. Không coi chức năng chưa nghiệm thu Windows là đã đạt điều kiện hoàn thành thực tế.

---

## Roadmap lịch sử (đề xuất V1.26 cũ đã được thay thế)

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
