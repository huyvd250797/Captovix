# Captovix V1.41.0 — Groups & Layout Editing

M4: nhóm lồng nhau, cây đối tượng/chọn cha-con, di chuyển panel/sửa label riêng, thuộc tính chung và bố cục giữ qua lưu/mở lại. Kế thừa toàn bộ M3 từ source1.40 Boss gửi.

- **Thêm → Nhóm & bố cục** mở cây trong bảng thuộc tính, dùng ô công cụ động/phím tùy chỉnh như cũ. Ctrl+G nhóm, Ctrl+Shift+G bỏ cấp nhóm; Vào nhóm/Chọn cha/Toàn cây hoặc Alt+click chọn riêng/chồng lấp.
- Căn hàng/chia khoảng/sắp lớp theo nhóm hay con đang chọn. **Thuộc tính chung…** thay riêng màu chữ/nền/cỡ chữ, ô trống giữ nguyên; có Undo/Redo.
- Popup Captovix đồng bộ viền/theme, thông báo/xác nhận dùng form chung; click ngoài modal làm sáng viền. Menu Thêm vừa hai cột, có viền/bóng. Hộp chọn tệp Windows vẫn native.
- Chuột phải ảnh/video ở thư viện trái/dưới → **Mở vị trí tệp**, Explorer chọn file gốc.
- Bộ cài đầy đủ: `Captovix-v1.41.0-Full-Setup.exe`; source: `Captovix V1.41.0 — Groups & Layout Editing.zip`; update: `Captovix-v1.41.0-Update-Packages.zip`.
- Thoát app cả khay, chạy Full Setup vào đường dẫn đang dùng. Không uninstall/xóa dữ liệu. Runtime/OCR/license/backend giữ nguyên. **Không cần chạy migration mới. Không deploy lại backend.**
- Nhóm lồng nhau dùng schema21 và backup sidecar `.pre-v1.41.bak`; trước rollback cần sao lưu. Đọc hướng dẫn [M4](docs/GROUPS-LAYOUT-EDITING-V141.md) và [kiểm tra](docs/VALIDATION.md).
- Build Windows bằng `scripts/Build-Windows.ps1` như hướng dẫn nền bên dưới. Pure checks: `python scripts/Run-Checks.py`; UI contract: `python scripts/Check-M4-UI.py`. Các fixture WPF cần Windows.
- Chưa nghiệm thu native Windows/cài đặt/hiệu ứng popup/DPI. Chưa push/publish GitHub. Muốn app nhận1.41: publish đủ năm release asset, đặt stable/Latest.
- Theo roadmap hiện có: **V1.42.0 — Storage & Package Optimization**; Library & Export Experience chuyển backlog chưa gán số do Boss chọn M4 cho1.41. Chưa xây tiếp.

## Tài liệu phiên bản trước

# Captovix V1.40.0 — Move & Background Recovery

M3 trên source/gói1.39.0: tách mặt nạ nền gốc khỏi phần tử Mockup, di chuyển/resize/xóa/nhân bản giữ đúng vị trí nền cũ; có vùng giữ lân cận, xem trước/chỉnh mặt nạ, nhật ký và Undo/Redo.

- **Thêm → Mockup** tạo/quét phần tử như M1/M2; di chuyển/resize/Delete/Ctrl+D. **Thêm → Nền & mặt nạ** để kiểm màu, mặt nạ và kéo vùng giữ. Nền phẳng trước; chưa inpaint ảnh/gradient, vùng chưa chắc chắn được cảnh báo.
- **Thiết lập → Chụp & quay → Nhấn Enter để xác nhận chụp**: tắt mặc định = kéo thả chụp ngay; bật = chỉnh vùng rồi Enter. Tự lưu, không đổi luồng quay/cuộn.
- **Phím tắt:** bỏ Theo cấu hình/Chọn cửa sổ và mô tả dài; thêm toàn bộ công cụ/phím đơn/tổ hợp, kiểm tra trùng và phím lõi. Công cụ Thêm mở bằng phím cũng cập nhật ô động. Bỏ menu Vùng pixel; vẫn dùng công cụ/phím pixel.
- Thu gọn/căn lại sidebar: nhãn ngắn, ô nhập cân đối, nút màu một dòng; giá trị HEX/quy tắc0 ở tooltip. Giữ theme và chức năng.
- Bộ cài đầy đủ: `Captovix-v1.40.0-Full-Setup.exe`. Hoặc `Captovix-v1.40.0-Update-Packages.zip`, giải nén chạy `HuyVo.Captovix-win-Setup.exe`. Thoát app cả khay, cài vào đường dẫn hiện tại, không uninstall.
- Source: `Captovix V1.40.0 — Move & Background Recovery.zip`. Chạy `python scripts/Run-Checks.py`; trên Windows build bằng `scripts/Build-Windows.ps1` như hướng dẫn nền. Runtime/dependency/licenseconfig/OCR giữ nguyên.
- Tài liệu M3 dùng schema20; tự backup sidecar cũ.pre-v1.40.bak. Các bản trước1.40 không đọc tài liệu20. Nhật ký100thao tác được lưu; Undo stack trong phiên hiện hành.
- **Không cần chạy migration mới. Không cần deploy lại backend.** KEY/trial và lịch sử dịch vụ giữ nguyên.
- Chưa nghiệm thu native Windows. Chi tiết docs/MOVE-BACKGROUND-RECOVERY-V140.md và docs/VALIDATION.md. Chưa push/publishGitHub; muốn updater nhận1.40, publish đủ5releaseassets và đặtLatest theo scripts/README-PUBLISH.txt.
- Tiếp theo roadmap: V1.41.0 — Library & Export Experience; chưa xây trong bản này.

## Lịch sử và hướng dẫn nền

# Captovix V1.39.0 — Element Scan & Review

M2 trên source V1.38.0: quét vùng/toàn ảnh bằng OCR + phân tích màu/viền, gợi ý chữ/title/nút/ô nhập/khối, sửa khung/loại/chữ và chọn phần tử trước khi áp dụng. Không chọn sẵn, không tự tạo hàng loạt. Giữ ảnh gốc, Apply một bước Undo/Redo, lưu/mở lại có metadata.

- Luồng dùng: **Thêm → Mockup → Quét vùng / Quét toàn ảnh → rà soát/sửa/chọn → Xem sau áp dụng → Áp dụng phần tử đã chọn**. Có Sửa một vùng chữ/nút để dùng lại M1. Mockup có icon riêng và dùng ô công cụ động; công cụ mới tiếp tục đưa vào Thêm.
- Trial: trạng thái30ngày, ngày giờ bắt đầu/hết hạn, số ngày còn lại và nút nhập KEY vĩnh viễn. Perpetual: check Đã kích hoạt/ngày giờ kích hoạt, ẩn ô KEY mặc định.
- **Có migration mới dành cho chủ dịch vụ:** chạy `supabase/migrations/202611040001_activation_timestamp.sql` trong Supabase SQL Editor sau nền1.29.1, rồi Xác minh lại. File TXT rời được bàn giao để copy. Không reset/trial mới/cấp lại KEY và không cần deploy lại Edge Function. Xem docs/ACTIVATION-METADATA-V139.md.
- Full setup: `Captovix-v1.39.0-Full-Setup.exe`; hoặc updateZIP `Captovix-v1.39.0-Update-Packages.zip`, giải nén chạy `HuyVo.Captovix-win-Setup.exe`. Thoát app cả khay, cài vào đường dẫn hiện tại; không uninstall. Bộ cài chứa .NET10.0.13 self-contained + VC++ prerequisite.
- Source: `Captovix V1.39.0 — Element Scan & Review.zip`. Build Windows như hướng dẫn nền. Bắt đầu với `python scripts/Run-Checks.py`; build bằng `scripts/Build-Windows.ps1`. Protocol.Shared cũng có cập nhật nên phải build/copy cả Licensing.Shared.dll.
- Nâng tài liệu M2 sang schema19 khi cần, giữ backup `.pre-v1.39.bak`; M1 thuần vẫn18. Bản cũ không mở tài liệu19.
- Giới hạn: tối đa20MP/vùng,200khung/500dòng OCR,120phần tử/lần Apply. Đây là gợi ý cần rà soát, không phải DOM thật; chỉ che nền phẳng, chưa phục hồi gradient/ảnh và font chính xác.
- Đã chạy logic/crypto/handler checks và cross-build app/fixtureWindows. Chưa nghiệm thu native Windows và chưa thực thi migration trên Supabase thật. Xem docs/VALIDATION.md, docs/ELEMENT-SCAN-REVIEW-V139.md.
- Chưa push/publish GitHub. Muốn app cũ/onlineSetup nhận1.39.0, publish đủ5assets theo scripts/README-PUBLISH.txt và đặt Latest. Các mốc tiếp theo chưa được chọn xây dựng.

## Lịch sử và hướng dẫn nền

# Captovix V1.38.0 — Text & Button Editing

M1 trên source V1.37.3: kéo chọn vùng label/title/nút, OCR Việt/Anh, tạo đối tượng sửa được; đổi nội dung, màu chữ, nền nút, viền và bo góc. Nền màu phẳng được che cùng đối tượng, ảnh gốc giữ riêng; Cancel không đổi tài liệu, áp dụng/sửa có Undo/Redo và lưu/mở lại.

- Cách dùng: **Mockup → kéo vùng → xem lại OCR/chọn Label, Title hoặc Nút → chỉnh → Áp dụng**. Nhấp đúp hoặc Sửa Mockup để sửa tiếp. Chi tiết, giới hạn và checklist: [docs/MOCKUP-M1-V138.md](docs/MOCKUP-M1-V138.md).
- Bộ cài/update: `Captovix-v1.38.0-Update-Packages.zip`; thoát app cả khay, giải nén và chạy `HuyVo.Captovix-win-Setup.exe` vào đường dẫn hiện tại. Không uninstall.
- Source: `Captovix V1.38.0 — Text & Button Editing.zip`. Windows x64/.NET10 SDK/NSIS3. Chạy `python scripts/Run-Checks.py`, build bằng `scripts/Build-Windows.ps1`.
- Nâng sidecar lên schema18 chỉ khi dùng Mockup; giữ `.pre-v1.38.bak` khi nâng sidecar cũ. Không DB migration/deploy backend. Bản cũ không mở được tài liệu schema18.
- Muốn app cũ nhận1.38.0: publish đủ5releaseassets và đặt Latest theo `scripts/README-PUBLISH.txt`. Chưa tự publish. Bộ cài trực tuyến hiện có tiếp tục dùng được sau khi publish.
- Logic/cross-build/package được kiểm, xem docs/VALIDATION.md; chưa nghiệm thu WPF/IME/OCR/Setup trên máy Windows.
- M1 chỉ hỗ trợ thay nội dung trên nền phẳng, chưa quét tự động toàn giao diện hoặc phục hồi nền phức tạp. Tiếp theo Mockup: **M2 — Element Scan & Review**, chưa gán phiên bản. Các roadmap cũ phía dưới là lịch sử.

## Lịch sử và hướng dẫn nền

# Captovix V1.37.3 — Image Layer Thread Safety

Bản vá khẩn trên sourceV1.37.2 Boss gửi: chụp ảnh1/ảnh2, dán ảnh2 vào ảnh1, chuyển thư viện rồi mở lại ảnh1 có thể báo `The calling thread cannot access this object because a different thread owns it` và mở chồng hộp thoại lỗi.

- Cacheảnh dán/ảnh nền chỉnh sửa chuyển sang bitmapBGRA độc lập, đãfreeze, không giữ BitmapFrame còn liên kết BitmapDecoder của luồng tải.
- Đọc ở worker, vẽ UI và lưu thumbnail nền dùng cùng dữ liệu pixel đã tách; không sửa dữ liệu PNG đã lưu trong sidecar, không bỏ lớp ảnh.
- PNGencoder tách BitmapFrame trước khi encode để tránh kéo metadata/decoder vào luồng khác.
- Đánh dấu lỗi Dispatcher đã xử lý trước khi mởMessageBox; thêm gate chống reentrant để không mở nhiều hộp thoại chồng nhau. Lỗi đầu vẫn được ghi log và hiển thị; lỗi độc lập về sau vẫn báo bình thường.
- Kế thừa sửa vượtbiên CaptureBounds1.37.2, Callout và tối ưu1.37. Không đổi schema/license/backend/runtime/dependency.

## Cài bản vá

1. Gói cập nhật: `Captovix-v1.37.3-Update-Packages.zip`. Thoát app cả khay, giải nén rồi chạy `HuyVo.Captovix-win-Setup.exe` vào thư mục hiện tại. Không uninstall, không xóa thư viện hoặc sidecar.
2. Source: `Captovix V1.37.3 — Image Layer Thread Safety.zip`.
3. Không cần chạy migration mới, không deploybackend, giữ key/ảnh/lớp ảnh đã lưu. Không tự flatten hoặc ghi đè ảnh gốc.
4. Để updater/Setuptrực tuyến thấy1.37.3: đưa đủ5releaseassets trongZIP lên stableRelease1.37.3 và đặtLatest. Bàn giao chưa tự publishGitHub. Launcher native hiện có tiếp tục dùng được.
5. Build Windows theo hướng dẫn nền bên dưới. Kiểm Windows hồi quy: `dotnet run --project tests/ImageLayerThreadWindowsChecks -c Release -p:Platform=x64`; luồng này cần WindowsDesktop runtime. Đọc docs/IMAGE-LAYER-THREAD-V1373.md.
6. Tiếp theo roadmap vẫn **V1.38.0 — Capture & Image Reliability**. Bản vá này không thay phạm vi roadmap.

## Lịch sử và hướng dẫn nền

# Captovix V1.37.2 — Capture Bounds Fix

Sửa lỗi `FrozenFrame.Crop`: vùng chọn vượt ra ngoài ảnh màn hình đã giữ. Log Boss gửi xác nhận đường lỗi ở Crop/Save; không có tọa độ trong log cũ để xác định chính xác thao tác đã gây ra.

- Gợi ý cửa sổ được giới hạn ngay theo ảnh màn hình đã giữ, kể cả cửa sổ phóng to có viền ngoài màn hình và cửa sổ nằm một phần ngoài desktop.
- Tọa độ hiển thị/xác nhận đồng bộ. Kéo di chuyển dừng ở mép, giữ kích thước; resize và phím mũi tên không vượt biên. Shift kéo vuông/giữ tỉ lệ tiếp tục được giữ trong sai số làm tròn pixel.
- Vùng chụp lại hoặc tọa độ cũ được giới hạn lần cuối trước khi ghi ảnh và metadata. Nếu phần còn lại quá nhỏ/ngoài toàn bộ, yêu cầu chọn lại; không tạo ảnh rỗng.
- Giữ ảnh màn hình chụp trước overlay để lưu, không chụp lại màn hình có thanh công cụ. Không scale/resample và không làm mờ ảnh.
- Guard Crop vẫn chặt; nếu phát sinh trường hợp khác, log bổ sung tọa độ vùng chọn và biên ảnh để xác định nguyên nhân.
- Kế thừa đầy đủ sửa Callout và tối ưu1.37.1; không đổi runtime/dependency/license/backend/schema.

## Cài đặt và phát hành

1. Gói update: `Captovix-v1.37.2-Update-Packages.zip`. Giải nén; thoát app cả khay rồi chạy `HuyVo.Captovix-win-Setup.exe` vào thư mục hiện tại. Không uninstall.
2. Source: `Captovix V1.37.2 — Capture Bounds Fix.zip`. Build Windows như các hướng dẫn nền phía dưới.
3. Để máy1.37.1 nhận cập nhật: upload đủ5releaseassets trong gói update lên stableRelease1.37.2, đặtLatest. Không chỉ upload ZIP/source. Bàn giao này chưa tự publish.
4. File `Captovix-Setup.exe` native trực tuyến đã bàn giao tiếp tục dùng được sau khi release1.37.2 thànhLatest; không cần dựng lại launcher.
5. Không cần chạy migration mới, không deploy lại backend/admin, giữ ảnh và key hiện có.
6. Chạy `dotnet run --project tests/CaptureBoundsChecks -c Release` và các kiểm thử cũ liên quan; trên Windows chạy thêm `tests/CaptureBoundsWindowsChecks`. Phạm vi kiểm tra thực tế ở `docs/CAPTURE-BOUNDS-V1372.md` và `docs/VALIDATION.md`.
7. Tiếp theo roadmap: **V1.38.0 — Capture & Image Reliability**; bản vá này chỉ sửa vượt biên, chưa triển khai toàn phạm vi1.38.

## Lịch sử và hướng dẫn nền

# Captovix V1.37.1 — Performance & Responsiveness

Bản vá V1.37.1 kế thừa toàn bộ hiệu chỉnh Callout của V1.37.0: click ngoài lưu và không tạo bản sao; chỉ kéo mới tạo; kéo tâm dời thân giữ đuôi, kéo ngoài tâm dời cả Callout. Tăng phiên bản assembly/UI/installer/feed để máy đang dùng V1.37.0 nhận bản mới sau khi phát hành đủ release assets.

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

## Cài đặt và build

1. Bộ cài: `Captovix-v1.37.1-Update-Packages.zip`. Thoát cả app/khay, giải nén và chạy `HuyVo.Captovix-win-Setup.exe` vào thư mục hiện tại. Không uninstall.
2. Source: `Captovix V1.37.1 — Performance & Responsiveness.zip`. Windows x64, .NET10 SDK và NSIS3. Mở thư mục gốc, chạy `python scripts/Run-Checks.py`, sau đó `powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1`. Các hướng dẫn nền phía dưới tiếp tục áp dụng.
3. Kiểm đo: `python scripts/Benchmark-Performance.py`; Windows chạy thêm `dotnet run --project tests/PerformanceWindowsChecks -c Release -p:Platform=x64`. Xem `docs/PERFORMANCE-RESPONSIVENESS-V137.md`.
4. Không cần chạy migration mới, không deploy lại backend/admin; license public config và dữ liệu hiện hữu giữ nguyên. Schema17 giữ nguyên; backup trước rollback.
5. Chưa push GitHub/deploy/publish. Để bản cũ thấy1.37.1, upload đủ5releaseassets trong ZIP lên stable release1.37.1 (latest), không chỉ upload ZIP/source. Updater dùng Full nên không cần cài trung gian.
6. Tiếp theo roadmap đã điều chỉnh: **V1.38.0 — Capture & Image Reliability**: nhiều màn hình/DPI, chọn vùng/phần tử/chụp app/chụp cuộn, đối chiếu ảnh và phục hồi khi hủy/lỗi lưu. Chưa triển khai bản1.38.

## Lịch sử và hướng dẫn nền

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

## Cài đặt và bàn giao

- `Captovix-v1.36.0-Update-Packages.zip`: giải nén, thoát Captovix, chạy `HuyVo.Captovix-win-Setup.exe`, giữ đường dẫn cài hiện tại. Không uninstall.
- `Captovix V1.36.0 — Text & Callout Polish.zip`: source, tests và hướng dẫn build. Windows x64/.NET10 SDK/NSIS3; chạy `python scripts/Run-Checks.py`, sau đó `powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1`.
- Cập nhật trong app chỉ thấy bản mới sau khi publisher đưa đủ5assets trong ZIP lên stable release v1.36.0. Không upload duy nhất ZIP hoặc source thay assets. Không tự push/publish trong lần bàn giao này.
- Không migration DB; không redeploy Supabase/admin. Public config bản quyền, dữ liệu và endpoint update giữ nguyên từ source Boss gửi. Không gọi production để tạo KEY/trial khi kiểm thử.
- Build/logic/package đã kiểm theo `docs/VALIDATION.md`. Chưa thực thi WPF/Setup/IME native tại Linux; checklist Windows ở `docs/TEXT-CALLOUT-POLISH-V136.md`.
- Phiên bản tiếp theo theo roadmap đã có: **V1.37.0 — Image Tools & OCR Usability**: kéo vùng nhận chữ/copy, zoom/pan và khả năng xem trước/hoàn tác các công cụ pixel. Chưa xây phiên bản này.

## Lịch sử và hướng dẫn nền

# Captovix V1.33.0 — Scrolling Capture Quality

Chụp cuộn ổn định hơn, cảnh báo mối nối/lệch cột, nhận vùng cố định, bỏ khung và chỉnh overlap. Toolbar chuyển lên trên để tăng canvas, chụp được chính app, kéo ảnh đã chỉnh ra ngoài và hoàn thiện popup/trạng thái bản quyền.

- Source: `Captovix V1.33.0 — Scrolling Capture Quality.zip`.
- Update/Setup: `Captovix-v1.33.0-Update-Packages.zip`. Thoát app và chạy Setup trong ZIP vào đường dẫn đang cài; không uninstall.
- Hướng dẫn/checklist: `docs/SCROLLING-CAPTURE-QUALITY-V133.md`.
- Không cần chạy migration mới. Public license config, Supabase backend và admin giữ nguyên. Trial server hiện có không tự reset khi gửi yêu cầu lặp.
- Windows x64, .NET10 SDK, NSIS3: `python scripts/Run-Checks.py` rồi `powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1`.
- Chưa push GitHub/deploy/publish release. Upload đủ các release assets trong ZIP lên stable GitHub Release v1.33.0 nếu muốn cập nhật trong app.
- Dữ liệu ảnh/schema17 giữ nguyên. Bố cục toolbar V1.33 lưu riêng, không ghi đè bố cục cũ. Trial history được ghi DPAPI theo user Windows; chống tạo trial mới sau xóa/cài lại vẫn dựa vào định danh máy và backend.
- Phiên bản tiếp theo giữ roadmap: V1.34.0 — Capture Workflow & Recovery.

## Hướng dẫn nền và lịch sử

# Captovix V1.32.0 — Smart Element Capture Refinement

Bản Windows chụp ảnh/chỉnh sửa: Chụp mặc định gộp phần tử/modal + kéo vùng; popup KEY tối giản; phím tắt PrtSc/phím đơn; Stamp click để đặt; khung chữ kéo/resize; node xoay xanh lá và quy tắc cuộn thống nhất.

- Chi tiết và hướng dẫn nghiệm thu: `docs/SMART-ELEMENT-CAPTURE-REFINEMENT.md`.
- Source: `Captovix V1.32.0 — Smart Element Capture Refinement.zip`.
- Bộ cài/cập nhật: `Captovix-v1.32.0-Update-Packages.zip`, chứa Setup và các release assets. Không uninstall trước khi nâng cấp.
- Không cần chạy migration mới. Không cần cấu hình/deploy lại Supabase hoặc admin Cloudflare. Public config đã có được giữ nguyên; không đóng gói khóa riêng/server secrets.
- Build Windows x64: .NET10 SDK, NSIS3; `python scripts/Run-Checks.py` rồi `powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1`. Script kiểm tra logic/native tương ứng môi trường và tạo hai ZIP.
- Nghiệm thu cài đặt/chuột/phím/DPI/clipboard/IME cần Windows thật. Linux chỉ xác minh logic, cross-build và toàn vẹn package; không tự kết luận native QA đạt.
- Không push GitHub/deploy/publish release trong lần bàn giao này. Hướng dẫn build/publish chi tiết bên dưới vẫn áp dụng; tên phiên bản hiện tại là V1.32.0.
- Phiên bản tiếp theo: V1.33.0 — Scrolling Capture Quality: xử lý mất/lặp dòng, sọc ghép, header/footer cố định và cảnh báo mối nối chưa chắc chắn. Chưa triển khai.

## Hướng dẫn nền và lịch sử phiên bản

# Captovix V1.31.0 — Quick Capture Experience

Nâng từ source V1.30 có payload DLL khớp gói update Boss đính kèm. Giữ nguyên public license config, Supabase/Pages và dữ liệu media. **Không cần chạy migration mới hoặc redeploy backend/admin.**

- Chụp vùng: thanh gọn, ưu tiên ngoài vùng; kéo/chỉnh cạnh rồi Enter một lần chụp. Viền tương phản mảnh theo DPI, resize cursor theo cạnh/góc.
- Đường X/Y theo con trỏ; H bật/tắt. Kính lúp112 ×138 DIP, viền mảnh; M bật/tắt; nhớ lựa chọn. Space + phím mũi tên hỗ trợ chọn vùng bằng bàn phím.
- Nhớ chế độ chụp được chọn; giữ pipeline snapshot/pixel V1.30 để toolbar/kính/tọa độ không vào PNG.
- Crop kéo rồi thả áp dụng ngay, Ctrl+Z hoàn tác; nhập số dùng Áp dụng kích thước.
- Bước chọn màu viền/nền riêng, hỗ trợ Trong suốt và màu số. Sidecar mới dùng schema16 khi có kiểu Bước mới; ảnh cũ giữ hiển thị cũ. Bản cũ không đọc schema16, cần backup trước rollback.
- Stamp mở bằng một nút, có12 hình vector mới gồm check xanh và X đỏ; preview và hình chèn thống nhất.
- Ô công cụ động ngay trái Thêm nhớ công cụ chọn gần nhất, icon riêng; dùng lại không cần mở Thêm.
- Nội dung cập nhật trong Settings/popup dùng chung định dạng dễ đọc, không hiện dấu Markdown thô.

Hướng dẫn thao tác/kiểm thử: [docs/QUICK-CAPTURE-EXPERIENCE.md](docs/QUICK-CAPTURE-EXPERIENCE.md). Đóng app, giải nén Update-Packages và chạy HuyVo.Captovix-win-Setup.exe vào đường dẫn cũ. Để update qua app, đăng năm assets lên stable Release v1.31.0; không chỉ upload ZIP. Không tự publish lần này.

Build .NET10 SDK/Windows x64/NSIS3 như hướng dẫn trước. `python scripts/Run-Checks.py` chạy logic và cross-build WPF trên Linux; WPF/input/DPI/Setup cần nghiệm thu Windows thật. Bàn giao đúng hai ZIP update và source, Setup ở trong ZIP update.

Kế tiếp đề xuất **V1.32 — Capture Presets & Repeat**: mẫu kích thước/tỷ lệ, nhập X/Y/W/H trong selector, chọn vùng gần đây và quản lý chụp lại nhanh; giữ kiểm tra pixel và nghiệm thu đa màn hình. Chưa triển khai.

## Lịch sử và hướng dẫn cấu hình trước đó

# Captovix V1.30.0 — Capture Fidelity & Reliability

Nâng từ source V1.29.1 Boss cung cấp. Giữ nguyên public license config đã gắn; backend Supabase/Pages, authority, dữ liệu bản quyền và media không thay đổi. **Không cần chạy migration mới hoặc redeploy backend/admin.**

## Các thay đổi

- Chụp vùng/phần tử dùng một snapshot desktop giữ trong RAM trước selector, Enter lấy đúng pixel từ snapshot thay vì chụp lại sau khi chọn. Kèm kính lúp15×15pixel, X/Y, phím M ẩn/hiện. Ghi thời điểm giữ ảnh vào metadata.
- PNG gốc đúng pixel vật lý và kích thước vùng chọn; giám sát topology/DPI, hỗ trợ tọa độ âm, không silently downscale. Ngưỡng64MP cho snapshot để giới hạn bộ nhớ.
- Editor phân biệt Vừa khung /100% /Tùy chỉnh; nút chế độ có trạng thái chọn. Zoom không đi vào pipeline file/copy. Save PNG kiểm tra cả kích thước và BGRA đọc lại.
- Giữ các công cụ ảnh, bản quyền/khôi phục/serverless và updater từ V1.29.1; không đụng private key hoặc tạo key/trial trên production.

Hướng dẫn và ma trận nghiệm thu: [docs/CAPTURE-FIDELITY-RELIABILITY.md](docs/CAPTURE-FIDELITY-RELIABILITY.md). Ảnh desktop là SDR8; HDR/WCG đánh giá riêng. Chụp cuộn, video và native-window capture tiếp tục dùng nguồn live riêng.

## Cài/cập nhật

Đóng Captovix → giải nén Update-Packages → chạy `HuyVo.Captovix-win-Setup.exe` → chọn đường dẫn cũ, ví dụ D:\Captovix\Captovix. Giữ thư mục dữ liệu và license; không gỡ app/xóa dữ liệu. Setup nằm trong ZIP update, chỉ bàn giao hai file Source và Update-Packages. Để cập nhật qua app, upload năm release assets trong ZIP lên stable GitHub Release v1.30.0 như hướng dẫn README-PUBLISH.txt.

## Build và kiểm tra

Dùng .NET10 SDK và Windows x64; runtime Windows10.0.13, dependencies pin từV1.29.1. Dùng NSIS3 để dựng bộ cài. Source có GitHub Actions chạy build Windows bằng public Variables đã hướng dẫn ở bản trước, không tự publish Release. Khi chỉ nâng app, không ghi đè admin/config.json mẫu lên Pages đang chạy.

```powershell
python scripts/Run-Checks.py
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Kết quả Source: `Captovix V1.30.0 — Capture Fidelity & Reliability.zip`; update: `Captovix-v1.30.0-Update-Packages.zip`. Không đưa private key/service-role/password vào source hoặc Actions Variables. Hướng dẫn serverless cũ bên dưới chỉ dành cấu hình lần đầu; **lần nâng này không cần thực hiện lại**.

Kiểm tra Linux chỉ chạy được logic/geometry và cross-build WPF; chưa thay cho nghiệm thu Windows100–200%, GDI/clipboard/multi-monitor/Setup. Xem docs/VALIDATION.md. Chưa push GitHub/deploy/publish Release trong lần bàn giao.

## Bản kế tiếp đề xuất

**V1.31 — Capture Workflow & Precision:** nhập trực tiếp X/Y/W/H trong selector, mẫu kích thước/tỷ lệ và chọn nhanh vùng gần đây ngay lúc chụp; tiếp tục nghiệm thu Windows theo bộ ảnh kiểm thử. Chưa xây bản này.

## Hướng dẫn hệ thống trước đó

# Captovix V1.29.1 — Serverless Licensing

Ứng dụng chụp/chỉnh ảnh Windows với Editor thống nhất. Bản này chuyển dịch vụ bản quyền V1.29.0 sang **Supabase Edge Functions + PostgreSQL**, trang quản trị **Cloudflare Pages**, giữ các công cụ ảnh/capture và giao thức RSA-PSS hiện có.

**Bản cấu hình cho hệ thống của Boss:** `src/Captovix/license-config.json` đã được thay bằng cấu hình công khai Boss cung cấp và nhúng vào app lúc build. API `/health` và `/ready` đã trả HTTP 200 khi kiểm tra ngày 03/11/2026. Giữ nguyên issuer/public key, không tạo lại private key và không đổi dữ liệu license trên server.

`admin/config.json` trong source vẫn là mẫu trống; trang admin Boss đã deploy đang có cấu hình riêng. Không upload đè thư mục admin mẫu này lên Pages đang hoạt động. Lần bàn giao này chỉ cấu hình/build Windows; không deploy lại Edge Function/admin, không cần chạy migration mới.

Nâng cấp từ V1.27.2: đóng Captovix/kết thúc quay, chạy Setup mới, chọn đường dẫn cài hiện có (ví dụ D:\Captovix\Captovix), không gỡ app hay xóa thư mục dữ liệu. Mở Bản quyền & khôi phục để dùng thử hoặc nhập key đã tạo trên trang quản trị. Chưa kiểm thử WPF/DPAPI/Setup trên Windows thật; việc API sẵn sàng chưa chứng minh toàn bộ luồng kích hoạt hoặc private key server khớp public key đã nhận.

Đọc [docs/CONFIGURED-WINDOWS-BUILD.md](docs/CONFIGURED-WINDOWS-BUILD.md) để cài/cập nhật. Giữ version V1.29.1 cho bản cấu hình đầu tiên này; không phát hành song song review chưa cấu hình cùng version. Nếu review V1.29.1 đã được đăng cho người dùng, cần tăng version trước khi phát hành gói khác.

## Triển khai không cần Docker/VPS

Hướng dẫn từng bước: **[docs/SERVERLESS-LICENSING.md](docs/SERVERLESS-LICENSING.md)**.

1. Tạo/chọn project Supabase; SQL Editor chạy `supabase/SETUP-ALL.sql` (additive, không reset).
2. Tạo user quản trị trong Auth, đưa đúng UID vào `captovix_license.admins`.
3. Mở `tools/LicenseKeygen.html` bằng Edge/Chrome để tạo/nạp khóa ngoại tuyến. Lưu private key/edge-secrets.env ngoài Source. Chỉ public config được copy vào app/admin.
4. Supabase Secrets: khai báo signing key B64, issuer, lease/cooldown, origin trang quản trị.
5. Deploy **một file** `supabase/functions/licensing/index.ts`, tên function `licensing`, tắt gateway Verify JWT (code vẫn xác minh device proof và admin Auth/allowlist). Kiểm tra `/health` và `/ready`.
6. Điền `admin/config.json`, upload **chỉ `admin/`** lên Cloudflare Pages Direct Upload. Đặt `ADMIN_ORIGINS` đúng URL Pages.
7. Dùng GitHub Actions workflow đã bàn giao để nhúng public URL/public key và build hai ZIP trên runner Windows. Chi tiết Variables/Actions trong guide; không cần SDK trên máy Boss. Chưa chạy workflow trong tài khoản Boss.
8. Nghiệm thu Windows/kích hoạt/chuyển máy rồi mới phát hành update assets.

## Công nghệ và bảo mật

- WPF .NET 10 x64, self-contained runtime Windows 10.0.13; Tesseract/ScreenRecorderLib/Velopack giữ bản pin từ source nền.
- Edge Function TypeScript dùng WebCrypto + fetch, không dependency runtime. Giao dịch PostgreSQL/RPC chống vượt slot và replay.
- Quản trị Supabase Auth + allowlist riêng, audit theo UID; session chỉ RAM của tab. Không nhúng service-role/signing private key vào Pages hoặc app.
- Trial 30 × 24h; lease trial tối đa48h, key vĩnh viễn tối đa30d offline mặc định. Vĩnh viễn không có hạn trả phí nhưng cần xác minh định kỳ.
- Khi hết quyền khóa chỉnh/copy/paste/OCR/Studio/export/video/capture nâng cao; chỉ chụp kéo vùng. Giữ media/chú thích và checkpoint công việc đang có.
- Chuyển máy, recovery code một lần/rotation/cooldown72h, quản trị slot/trial/repair/thu hồi/audit. Thu hồi không tức thì trên máy offline còn lease.

## Phát triển và kiểm tra

Node24+ để chạy kiểm tra backend/DOM; .NET10 SDK cho core/WPF; Windows/NSIS3 để dựng installer và nghiệm thu. NPM chỉ phục vụ test, không cần cho Dashboard deployment.

```powershell
npm ci
npm run check
npm test
python scripts/Run-Checks.py
```

`npm run test:integration` chỉ dùng database KIỂM THỬ PostgreSQL đã migration; xem guide. Auth transport trong kiểm tra là fixture, chưa chứng minh Supabase Auth production. Linux chỉ cross-build WPF, không thực thi Windows.

Build local Windows sau copy public config:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Build chặn cấu hình thiếu hoặc private key trong public field. `-AllowUnconfiguredLicensing` chỉ để review hạn chế. `scripts/Package-Source.py` tạo Source ZIP sạch, từ chối file secret có tên chuẩn; không bỏ kiểm tra để đóng gói secrets.

## Bàn giao và bước kế tiếp

Hai file: `Captovix-v1.29.1-Update-Packages.zip` (có canonical Setup.exe) và `Captovix-v1.29.1-Source.zip`. Không push GitHub, deploy hoặc publish Release tự động. Khi tiếp tục, gửi Source ZIP mới nhất, giữ public config đã dùng và mô tả yêu cầu; không gửi private key/service-role/password.

Đề xuất tiếp **V1.30 — Licensing Hardening & Release**: key rotation/key ring, MFA/quyền quản trị chi tiết, giám sát quota/backup và nghiệm thu Windows/staging đầy đủ. Chưa xây phiên bản đó.
