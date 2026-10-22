# Captovix V1.7.0 — Shapes & Stroke Studio

## Mới trong V1.7.0

- **Hình dạng (S):** chữ nhật, chữ nhật bo góc, ellipse/tròn, tam giác, thoi, thang, ngũ giác, lục giác, sao. Có **Đường (L)** và mũi tên (A), 1/2 đầu với tam giác đặc/mở/thoi/tròn.
- **Viền/nền riêng:** HEX và bảng màu nhanh; bật/tắt viền hoặc nền; độ đậm 0–100%; độ dày 1–40 px; nét liền, đứt, chấm, đứt–chấm. Bo góc 0–1000 px (render giới hạn theo kích thước), sao 3–20 cánh/lõi 10–90%, cỡ đầu mũi tên 0=tự động hoặc đến 160 px.
- Vẽ xong nhấn **V**, chọn và kéo/resize tay nắm; Shift giữ tỷ lệ ở góc hoặc khóa hướng đường mỗi 45°. Đổi loại hình kín giữ ID/vị trí/kích thước. **Ctrl+Z/Ctrl+Y** hoàn tác/làm lại cả lần chỉnh.
- Ba preset mới: Khoanh lỗi đỏ, Khung ghi chú xanh, Nền nhấn vàng. Lưu preset riêng đầy đủ. **Áp dụng** đổi lựa chọn hiện tại; **Chỉ dùng preset cho nét mới** giữ đối tượng cũ.
- Giữ bản vá GDI chụp vùng/toàn màn hình đúng pixel V1.6.1 và sửa lỗi chuyển ảnh V1.5.1. App vẫn mở Editor phóng to, giữ đường dẫn cài hiện tại và URL cập nhật mặc định.

**Cách dùng:** mở/chụp ảnh → Hình dạng → chọn hình bên phải → chỉnh viền/nền/nét rồi bấm Áp dụng hoặc Enter → kéo trên ảnh. Muốn sửa lại: V → chọn hình → đổi thuộc tính → Áp dụng. Màu nhanh ở bảng màu sắc là màu viền; hàng màu nền nhanh chỉ đổi nền. Các ô sửa được áp dụng cùng lúc, không tự lưu từng ký tự. Chuyển lựa chọn trước khi bấm Áp dụng sẽ bỏ các giá trị chưa áp dụng. Xem `docs/SHAPES-STROKE-STUDIO.md`.

**Dữ liệu:** schema 5 đọc và nâng schema 1–4, mặc định giữ kiểu cũ (viền đặc, không tô nền). Khi lưu sidecar cũ lần đầu, giữ nguyên byte cũ ở `Edits/<id>.json.pre-v1.7.bak`; nếu backup thất bại thì không ghi đè sidecar. Không mở ngược file schema 5 bằng V1.6 trở xuống; nếu cần rollback, thoát app và khôi phục bản backup tương ứng. Ảnh gốc không bị sửa.

**Kiểm chứng:** test logic và cross-build trên Linux; chưa chạy WPF/GDI, thao tác chuột, clipboard, installer/update hoặc DPI trên Windows thật. Trạng thái: triển khai/build, chờ nghiệm thu Windows. Chưa đăng GitHub Release. Chỉ bàn giao Update-Packages và Source.


### V1.6.1 — Bản vá chụp ảnh kích thước lẻ

Vùng 1467×386 phải tạo PNG 1467×386. Ảnh vùng/toàn màn hình dùng Win32 GDI BitBlt lấy pixel desktop SDR; không dùng Screenshot mode vốn ép chẵn của ScreenRecorderLib. Chụp cửa sổ và video giữ backend cũ. Hiệu năng/màu HDR, các cửa sổ bảo vệ, máy Windows thực tế vẫn cần kiểm chứng.

**Cập nhật:** giải nén Update-Packages, đăng 5 release assets lên GitHub Release `v1.7.0` rồi bấm Kiểm tra cập nhật trong app. Hoặc thoát Captovix ở khay hệ thống, chạy `HuyVo.Captovix-win-Setup.exe` trong ZIP. Giữ nguyên đường dẫn cài hiện tại (kể cả `D:\Captovix\Captovix`); không cần gỡ app hoặc xóa ảnh. ZIP Source chỉ dành cho phát triển.

**Kiểm tra sau cài:** thử vùng 1467×386, 1466×387, 1467×387, 1×1; bật/tắt con trỏ; chụp sát mép và qua hai màn hình. Chạy chức năng Kiểm chứng SDR để lưu report, rồi thử từng mức Windows scale. Log: `%LOCALAPPDATA%\HuyVo\Captovix\errors.log`. Bản vá này chưa được chạy chụp/installer trên Windows thật tại nơi build.

Ứng dụng Windows x64 chụp ảnh, quay màn hình và chú thích ảnh trong **một cửa sổ Editor**. Cấu hình chụp/quay nằm ngay trong Editor. © 2026 HuyVo. All rights reserved.

## Bắt đầu

**Cài thủ công:** giải nén `Captovix-v1.7.0-Update-Packages.zip`, chạy `HuyVo.Captovix-win-Setup.exe` rồi mở shortcut Captovix. Bộ cài cho chọn thư mục, kèm .NET runtime; không cần SDK/Visual Studio trên máy dùng. Microsoft VC++ Runtime có thể cần quyền quản trị nếu máy chưa có.

**Portable:** script build vẫn có thể tạo bản portable khi cần; lần bàn giao này chỉ có Update-Packages và Source, không kèm file portable riêng.

Máy mục tiêu: Windows x64, ưu tiên Windows 11; Windows 10 22H2 cần kiểm thử thực tế. Windows N/KN cần Media Feature Pack. App chưa có bản x86/ARM64 hoặc chứng thư ký mã thương mại. Không tắt Windows Defender hay cơ chế bảo vệ của máy.

## Image Fidelity V1.6

Dưới canvas có **Gốc**, **Xuất / Copy** và **Zoom** riêng. Actual Pixels đặt 1 pixel đầu ra bằng 1 pixel màn hình; nếu Windows scale 150% thì zoom nội bộ được bù, không tự resize file. Vừa khung có thể trông mềm vì nội suy preview; không làm file bị nhỏ đi. Khi đã chọn resize trong Ảnh & khung, đầu ra dùng kích thước đó và UI hiển thị rõ có resize.

Crop, xoay 90°, lật và đặt canvas không resize giữ nguyên pixel BGRA8 vùng không chỉnh sửa. Chú thích render riêng rồi ghép; PNG/copy dùng cùng ảnh đầu ra, không dùng thumbnail. Canvas có khoảng dư lẻ được canh nguyên pixel (giữa lệch tối đa nửa pixel so với cách cũ) để tránh blur do vị trí 0,5 pixel. Ảnh gốc không bị ghi đè. Explicit resize vẫn có nội suy theo yêu cầu; không quảng cáo nguồn 1080p phóng lớn là chi tiết 4K thật.

PNG và clipboard PNG chuẩn hóa 96 DPI metadata, không thay số pixel. Clipboard có PNG giữ alpha và Bitmap tương thích; ứng dụng nhận có thể chọn format, nén/resize khi dán. JPEG 95% có mất mát, nền trong suốt ghép trắng; ưu tiên PNG cho chữ nhỏ. Pipeline này là SDR 8-bit BGRA, không cam kết giữ HDR, wide gamut, profile ICC hoặc 16-bit. Không tự tăng độ nét/AI upscale.

### Kiểm chứng trên Windows thật

Môi trường tạo bản này là Linux. Unit tests và cross-build không thay capture, WPF, clipboard hay test nhiều màn hình thực tế. **V1.6 đang chờ nghiệm thu Windows**, không ghi đã đạt matrix chỉ từ tính toán DPI.

Trong Editor mở **Máy và màn hình → Kiểm chứng ảnh SDR trên máy này**. Tắt HDR trước; xác nhận dialog. Phép thử dùng clipboard, hiển thị ảnh mẫu, chụp mẫu và kiểm tra kích thước từng màn hình. Dùng Esc để hủy. Kết quả nằm tại `%LOCALAPPDATA%\HuyVo\Captovix\FidelityChecks\<lượt chạy>\report.json`, kèm ảnh mẫu và kết quả render; không tự gửi ra ngoài. Full-desktop test chỉ kiểm tra kích thước, file thử được xóa khi hoàn tất. Nếu probe bị kết thúc đột ngột có thể còn pending.

Chạy lại khi Windows ở **100%, 125%, 150%, 175%, 200%**. Mỗi report chỉ chứng minh DPI đã quan sát ở lượt đó. Thử monitor phụ bên trái/trên (tọa độ âm). Script `Summarize-Fidelity.ps1` trong thư mục app/source `scripts` đọc các report pass của 1.7.0 để báo scale còn thiếu và monitor âm; exit 2 nếu matrix thiếu. Chưa có report thật trong gói bàn giao.

Thử kéo chính Editor qua các monitor ở Actual Pixels và zoom tự chọn, rồi crop/xoay/copy/xuất và so pixel. Probe di chuyển cửa sổ mẫu, chưa thay thế kiểm tra thao tác Editor. Chi tiết và mẫu ghi kết quả: `docs/IMAGE-FIDELITY.md`, `docs/WINDOWS-FIDELITY-MATRIX.md`.

### Chạy kiểm tra từ source (Windows)

```powershell
dotnet run --project tests/ImageFidelityChecks -c Release
dotnet run --project tests/FidelityWindowsChecks -c Release -p:Platform=x64
dotnet run --project tests/EditorWindowsChecks -c Release -p:Platform=x64
```

FidelityWindowsChecks kiểm tra renderer/PNG không cần capture. Muốn capture/clipboard thật phải dùng probe tương tác trong app. Các giá trị DPI mô phỏng trong test không được coi là đã test Windows Scale thật.

## Bản vá V1.5.1

Sửa lỗi NullReferenceException khi chuyển ảnh tại `InitializeEditor → EditorSurface.Refresh → UnloadEditor`. Phiên và canvas được dọn theo thứ tự nhất quán; readout/bảng thuộc tính không đọc layout rỗng. Không xóa ảnh, reset thư viện hay reset JSON để sửa lỗi này.

Chỉ bàn giao hai file: **Update-Packages.zip** và **Source.zip**. Bộ cài nằm bên trong Update-Packages dưới tên `HuyVo.Captovix-win-Setup.exe`; nếu cần cài thủ công, giải nén và chạy file đó. Không có Setup/Portable tách riêng trong lần bàn giao này.

Sau khi cập nhật, thử chọn ảnh A → B → A nhiều lần; thử ảnh có crop/viền/chú thích, bỏ chọn và chọn ảnh mới. Kiểm tra lỗi mới trong `%LOCALAPPDATA%\HuyVo\Captovix\errors.log` theo thời gian phát sinh; log lỗi trước cập nhật vẫn được giữ. Các ca UI cần nghiệm thu trên Windows thật.

## Capture Workflow V1.5

Mở **Thiết lập chụp, preset & phím tắt** ngay trong Editor. Cấu hình ảnh và quay vẫn dùng chung một cửa sổ Editor phóng to. Các hộp chọn cửa sổ/vùng và đếm ngược chỉ xuất hiện trong phiên capture.

- **Chụp nền:** chọn `Lưu nền + thông báo`; ảnh PNG vẫn được ghi vào thư viện. Nhấp thông báo để mở đúng ảnh. Có thể bật `Tự copy ảnh` độc lập. Nếu clipboard bận, ảnh vẫn được giữ và có thể copy trong Editor. Nếu Windows chưa tạo được biểu tượng khay, app mở Editor sau khi chụp để luôn có cách truy cập.
- **Khay hệ thống:** nhấp biểu tượng để mở Editor; nhấp phải để chụp theo cấu hình, vùng, cửa sổ, màn hình, chụp lại vùng, dùng preset hoặc thoát. Windows có thể đặt biểu tượng trong vùng biểu tượng ẩn. Thông báo có thể bị Windows/Do Not Disturb ẩn; ảnh vẫn lưu trong Nội dung.
- **Phím ảnh mặc định:** Ctrl+Shift+1 theo cấu hình; +5 chọn vùng; +6 chọn cửa sổ; +7 chụp màn hình đang cấu hình; +8 chụp lại vùng. Đổi phím hoặc bỏ chọn từng dòng trong `Phím tắt & chạy nền`, rồi **Lưu cấu hình Workflow**. Nhấn Tab để sang dòng khác. Khi đang nhập phím, hotkey tạm ngừng đăng ký để không kích hoạt capture; rời ô sẽ đăng ký lại cấu hình đã lưu. Phím xung đột sẽ báo rõ; các nút chụp vẫn dùng được. Không dùng F12; phím quay +2/+3/+4 giữ nguyên.
- **Chụp lại vùng:** chỉ lưu vùng sau khi chụp thành công, theo pixel vật lý. Thay màn hình, vị trí, độ phân giải hoặc DPI sẽ yêu cầu chọn lại. Không ghi nhớ HWND cửa sổ: mỗi lần chụp ảnh cửa sổ đều chọn lại từ danh sách đang mở.
- **Preset chụp:** có Hướng dẫn / Chụp nhanh / Báo cáo; chọn và bấm Áp dụng, hoặc gọi trực tiếp từ menu khay. Cấu hình chế độ, màn hình, trễ, con trỏ, hành động sau chụp và copy; nhập tên rồi Lưu preset mới. Tối đa 20 tên riêng; có thể xóa preset. Preset chụp tách biệt với preset chú thích/màu ảnh. Màn hình trong preset bị tháo sẽ báo chọn màn hình mới.
- **Chạy nền:** `Đóng xuống khay` và `Khởi động cùng Windows` đều **tắt mặc định**. Chọn và Lưu cấu hình để bật. `Ẩn vào khay khi Windows khởi động` chỉ áp dụng khi app được mở bằng cơ chế autorun; mở app thủ công luôn vào Editor phóng to. Không tự chụp khi khởi động. Menu khay → Thoát để đóng hoàn toàn.
- **Hủy:** Esc khi chọn vùng/cửa sổ/đếm ngược; nếu Esc toàn cục bị app khác chiếm, dùng Esc trong overlay hoặc menu khay → Hủy phiên. Video đã bắt đầu phải dừng và hoàn tất MP4; không cắt file ngang. Bấm chụp liên tiếp không tạo nhiều phiên đồng thời.
- Ảnh mới được lưu trước khi tự copy/thông báo. Chụp nền giữ nguyên tài liệu đang chỉnh; mở Editor sau chụp sẽ chọn ảnh mới. Đóng/thoát lưu chú thích trước. Update dừng tray/hotkey trước restart và phục hồi nếu restart thất bại.

Tùy chọn ảnh sau chụp được dùng ngay khi chụp và lưu cùng phiên; thay đổi hotkey/khởi động/chế độ đóng cần bấm **Lưu cấu hình Workflow**. Cấu hình tại `%LOCALAPPDATA%\HuyVo\Captovix\capture-workflow.json`; bản lỗi hoặc schema mới được giữ nguyên, không tự ghi đè. **Khôi phục mặc định** sao lưu file trước rồi tạo cấu hình mới, đồng thời tắt autorun. Gỡ cài đặt xóa đăng ký autorun do Captovix tạo và giữ dữ liệu ảnh/chú thích.

Autorun dùng HKCU Run (không cần quyền quản trị); Windows/Task Manager có thể trì hoãn hoặc vô hiệu hóa. Đường dẫn portable cần giữ nguyên; dùng bộ cài để cập nhật trong app. Thay nơi cài đặt phải gỡ bản cũ trước theo hướng dẫn hiện có.

## Phát hành V1.7.0

- Máy đã cài bản có updater: giải nén `Captovix-v1.7.0-Update-Packages.zip`, tải **5 asset bên trong** (trừ README-PUBLISH.txt) lên GitHub Release ổn định tag `v1.7.0` của `huyvd250797/captovix-releases`. Không tải chỉ một ZIP làm asset updater, không dùng Source code ZIP.
- Năm asset: `HuyVo.Captovix-1.7.0-full.nupkg`, `HuyVo.Captovix-win-Setup.exe`, `RELEASES`, `assets.win.json`, `releases.win.json`. Không sửa nội dung feed/hash sau khi build.
- Trên app cũ: Kiểm tra cập nhật → Tải bản cập nhật → Khởi động lại để cập nhật. Nếu chưa đăng release thì app cũ chưa thấy V1.7.0. Source URL mặc định đã giữ nguyên, không cần nhập lại.
- Cài thủ công bằng `HuyVo.Captovix-win-Setup.exe` sau khi giải nén Update-Packages: thoát Captovix hoàn toàn bằng menu khay trước. Giữ đường dẫn cũ; vẫn cho chọn thư mục cài mới khi máy chưa có bản quản lý.
- Source ZIP dùng cho phát triển; Portable ZIP để chạy giải nén; Setup EXE để cài nhiều máy. Bản build chưa ký chứng thư Authenticode, có thể gặp cảnh báo Windows.

## Image Transform & Styling V1.4

- Mở ảnh, mở **Ảnh & khung · V1.4** ở bảng thuộc tính bên phải.
- Xoay/lật, đổi kích thước ảnh/canvas; viền/nền/đệm/bóng và mẫu Sáng/Tối.
- Ảnh gốc và chú thích tiếp tục chỉnh được, không bị ghép phẳng khi áp dụng các thay đổi.
- Hướng dẫn: [IMAGE-TRANSFORM-STYLING.md](docs/IMAGE-TRANSFORM-STYLING.md).
- Schema 4 đọc file cũ 1/2/3. Không dùng app V1.3 trở xuống để mở lại file đã lưu bởi V1.4; giữ backup Edits nếu cần quay lại.

## Advanced Annotation V1.3

- Sửa chữ tại ảnh, callout, vẽ tự do; nhân bản, nhóm, sắp xếp trước/sau, chia đều khoảng cách.
- Hướng dẫn và phím tắt: [ADVANCED-ANNOTATION.md](docs/ADVANCED-ANNOTATION.md).
- Schema 3 đọc được file cũ; không mở ngược tài liệu mới đã lưu bằng app V1.2 trở xuống. Giữ bản sao Edits trước khi nâng cấp thủ công nếu cần quay lại.

## Sửa lỗi thư mục cài đặt (giữ từ V1.2.1)

- Sửa bước chuẩn hóa đường dẫn để chấp nhận thư mục chưa tồn tại, ví dụ `D:\Captovix\Captovix`. Bộ cài tự tạo thư mục sau khi kiểm tra an toàn.
- Phân biệt thông báo sai đường dẫn, loại ổ đĩa và trùng thư mục dữ liệu.
- V1.2.0 nếu chưa cài được: đóng bộ cài cũ, chạy Setup V1.3.0; không cần cài các phiên bản trung gian.

## Mới trong V1.2 — Image Editor UX

- Con lăn zoom tại con trỏ; giữ Space + chuột trái để pan, không sửa chú thích.
- Crop có vùng xem trước, 8 tay nắm và ô X/Y/rộng/cao. Enter/Áp dụng để hoàn tất; Escape hủy; phím mũi tên tinh chỉnh.
- Alt+click chọn đối tượng nằm dưới khi chồng lấp; giữ các công cụ ảnh V1.1.
- Setup có trang chọn thư mục cài, có thể chọn ổ D/E và thư mục riêng có quyền ghi.
- Xem [IMAGE-EDITOR-UX.md](docs/IMAGE-EDITOR-UX.md) và [INSTALL-LOCATION.md](docs/INSTALL-LOCATION.md).

Muốn chuyển bản đã cài ở ổ C sang ổ khác: gỡ Captovix trong Windows Settings (dữ liệu được giữ), chạy Setup V1.3.0 và chọn thư mục mới. Cập nhật trong app giữ thư mục cài hiện tại. Đổi thư mục cài không di chuyển ảnh/video.

## Mới trong V1.1 — Image Annotation

- Làm mờ (B), Pixel hóa (P), Che kín (K); mức xử lý 4–64 px.
- 6 preset có sẵn; lưu/xóa tối đa 20 preset riêng trong bảng thuộc tính.
- Shift/Ctrl+click, kéo vùng trống hoặc Ctrl+A để chọn nhiều; kéo, căn chỉnh, đổi kiểu hoặc Delete cho cả nhóm.
- Căn theo khung ảnh/crop khi chọn 1 đối tượng, theo biên nhóm khi chọn nhiều.
- Tay nắm ở 8 hướng; kéo góc chữ/số để đổi cỡ. Shift khóa hướng/tỷ lệ; Ctrl trong lúc kéo bỏ bắt dính mép/tâm ảnh.
- Giữ undo/redo, tự lưu, ảnh gốc và URL cập nhật mặc định.
- Hướng dẫn đầy đủ: [IMAGE-ANNOTATION.md](docs/IMAGE-ANNOTATION.md).

Chia sẻ ảnh PNG/JPEG qua **Copy ảnh/Xuất file**. **Che kín** phù hợp khi cần ẩn hoàn toàn; ảnh gốc và tài liệu chỉnh sửa vẫn được giữ để sửa lại. Tài liệu mới dùng schema 5, đọc được dữ liệu V1.0 nhưng không nên mở ngược bằng app V1.0 sau khi lưu mới.

## Sửa lỗi cài đặt V1.0.2

- Bỏ đối số ứng dụng sau `--` làm CoreSetup.exe panic `EXE_ARGS` rồi đứng chờ.
- Cửa sổ cài lõi hiện tiến trình/lỗi, bootstrap hiện 3 giai đoạn; ghi log vào `%LOCALAPPDATA%\HuyVo\Captovix\Logs\setup-core.log` và giữ log lần trước.
- Kiểm tra app đã mở trước khi bấm Hoàn tất để tránh cảnh báo mở trùng.
- Có thể đi thẳng **V1.0.0 → V1.0.2**, không cần cài V1.0.1. Nếu bộ cài V1.0.1 cũ còn kẹt, kết thúc CoreSetup.exe đúng lần cài đó rồi đóng bộ cài trước khi chạy bản mới.
- Chi tiết: [INSTALLER-RECOVERY.md](docs/INSTALLER-RECOVERY.md). Bản V1.0.3 gắn sẵn URL phát hành, không cần nhập nguồn khi cài mới.

## Chức năng cập nhật giữ từ V1.0.1

- Mục **Cập nhật ứng dụng** trong Editor và nút **Cập nhật** ở thanh trạng thái; không tạo màn hình cấu hình riêng.
- Kiểm tra thủ công hoặc mỗi 24 giờ; phiên bản mới/nội dung thay đổi; tải nền, tiến trình, hủy, thử lại và hoãn.
- Bản Setup dùng Velopack để áp dụng gói sau khi chọn **Khởi động lại để cập nhật**. Không tự đóng app hoặc tự cài khi khởi động.
- Chặn áp dụng lúc capture; lưu chú thích, thư viện, thiết lập và sao lưu JSON/Edits trước khi thoát. Giữ media gốc và mở lại nội dung đang chọn.
- Chấp nhận nguồn GitHub Releases công khai hoặc thư mục HTTPS. Kiểm tra AppId, stable/newer, kích thước và SHA-256 của gói.
- Bộ cài chuyển từ NSIS V1.0 sang Velopack ở `%LOCALAPPDATA%\HuyVo.Captovix`; thư mục dữ liệu hiện có được giữ lại.
- **Nguồn phát hành mặc định:** `https://github.com/huyvd250797/captovix-releases`. Cài mới rồi bấm **Cập nhật → Kiểm tra cập nhật**; không cần nhập URL. Cấu hình riêng đã lưu được ưu tiên. Nếu trước đây đã lưu nguồn rỗng để tắt nguồn, cần cấu hình lại một lần. Xem [hướng dẫn phát hành](docs/UPDATE-GUIDE.md).

## Chức năng Editor giữ từ V1.0

- Tên hiển thị, file chạy, icon và bộ cài đổi thành **Captovix**.
- Khi mở app, Editor mặc định **tối đa cửa sổ (Maximized)**: tận dụng vùng làm việc, vẫn có thanh tiêu đề/nút Windows. Đây không phải chế độ ẩn toàn bộ viền/taskbar.
- Sau capture, khôi phục đúng trạng thái cửa sổ trước khi chụp/quay, giữ Maximized nếu đang dùng.
- Chú thích ảnh: mũi tên, khung chữ nhật, ellipse, chữ nhiều dòng, highlight trong suốt, số bước tự tăng.
- Chọn/di chuyển đối tượng; kéo hai tay nắm để sửa đầu mũi tên hoặc kích thước khung/ellipse/highlight. Thay màu, nét, cỡ chữ/số, nội dung chữ hoặc số thứ tự ở bảng thuộc tính.
- Crop không phá ảnh gốc; khôi phục đầy đủ khung ảnh bất kỳ lúc nào. Chú thích ngoài vùng crop vẫn được giữ để lấy lại sau.
- Undo/redo tối đa 100 thao tác trong lượt mở ảnh hiện tại. Lịch sử undo không được ghi xuống đĩa và khởi tạo lại khi đổi ảnh/mở lại app; bản chú thích cuối cùng vẫn được lưu.
- Tự lưu chú thích/crop thành tài liệu riêng; mở lại vẫn sửa được từng đối tượng. Thumbnail cập nhật theo bản đã chỉnh.
- Copy/xuất PNG hoặc JPEG bao gồm chú thích và crop, độc lập mức zoom. Không cho xuất đè các file gốc đang quản lý trong thư viện. JPEG dùng nền trắng cho vùng trong suốt.
- Kiểm tra hash/kích thước ảnh gốc khi mở bản chú thích. Nếu dữ liệu không khớp hoặc bị lỗi, giữ bản sao `.invalid-*` và thông báo, không áp lớp chú thích lên ảnh khác.
- Tiếp nhận lịch sử/cấu hình từ V0.1 khi Captovix chưa có thư viện.

## Cách chỉnh sửa ảnh

1. Chụp ảnh mới hoặc bấm **Mở file** để nhập ảnh/video.
2. Chọn ảnh ở danh sách trái. Thanh công cụ ảnh sẽ xuất hiện.
3. Chọn công cụ, kéo trên ảnh để vẽ. Với **Chữ**, click vị trí rồi nhập nội dung ở bảng thuộc tính bên phải; `Ctrl+Enter` để hoàn tất.
4. Với **Bước**, click để đặt số tự tăng; có thể sửa số trong bảng thuộc tính.
5. Dùng **Chọn** để click đối tượng, kéo di chuyển hoặc sửa thuộc tính. Delete chỉ xóa đối tượng đang chọn.
6. Dùng **Crop**, kéo vùng muốn giữ, sửa tay nắm hoặc nhập kích thước rồi bấm **Áp dụng crop**. Undo hoặc **Khôi phục khung ảnh gốc** để lấy lại.
7. Chờ nhãn **Đã lưu**. Thao tác vẽ/di chuyển tự lưu sau khoảng 0,55 giây; nhập chữ được gom theo nhịp dừng gõ khoảng 0,7 giây rồi tự lưu. `Ctrl+S` lưu ngay.
8. Chọn **Copy ảnh** hoặc **Xuất file** để lấy ảnh hoàn chỉnh. Ảnh gốc luôn lưu riêng.

Bảng cấu hình chụp/quay mặc định thu gọn để dành vùng chỉnh sửa; mở mục **Thiết lập chụp & quay** để thay nguồn, con trỏ, thời gian chờ, mic/tiếng máy, FPS và mã hóa GPU.

## Phím tắt

| Phím | Thao tác |
|---|---|
| Ctrl+Shift+1 | Chụp theo cấu hình |
| Ctrl+Shift+2 | Bắt đầu quay |
| Ctrl+Shift+3 | Kết thúc quay |
| Ctrl+Shift+4 | Tạm dừng/tiếp tục quay |
| Ctrl+Z / Ctrl+Y hoặc Ctrl+Shift+Z | Undo / redo chú thích |
| Ctrl+S | Lưu phần chỉnh sửa |
| Ctrl+C | Copy ảnh đã chỉnh, khi không đang nhập chữ |
| V / A / R / E | Chọn / Mũi tên / Khung / Ellipse |
| T / H / N / C | Chữ / Highlight / Số bước / Crop |
| B / P / K | Làm mờ / Pixel hóa / Che kín |
| Con lăn | Zoom quanh vị trí chuột |
| Space + kéo chuột trái | Di chuyển khung nhìn |
| Alt+click | Chọn lớp đối tượng tiếp theo dưới con trỏ |
| Ctrl+A | Chọn tất cả chú thích / toàn khung khi crop |
| Shift/Ctrl+click | Thêm/bỏ đối tượng khỏi vùng chọn |
| Delete | Xóa các đối tượng đang chọn |
| Phím mũi tên / Shift+mũi tên | Di chuyển đối tượng 1 / 10 pixel gốc |
| Esc | Hủy thao tác kéo đang thực hiện, về công cụ Chọn |

Khi đang gõ trong ô chữ, các phím sửa văn bản vẫn dành cho ô chữ. Phím tắt capture toàn hệ thống chỉ hoạt động khi app đang chạy; nếu trùng app khác, Editor thông báo. Hãy thả chuột hoàn tất nét vẽ trước khi dùng lệnh chỉnh sửa bằng bàn phím. Khi đang kéo vùng làm mờ/pixel, Editor dùng màu che tạm; thả chuột để áp hiệu ứng.

## Chụp và quay kế thừa V0.1

- Chụp vùng/cửa sổ/toàn bộ một màn hình đang chọn; ảnh gốc PNG.
- Kéo vùng có thể đi qua nhiều màn hình; thuật toán dùng pixel vật lý, hỗ trợ tọa độ X/Y âm.
- Video MP4/H.264 theo kích thước nguồn, cấu hình 15/30/60 FPS và bitrate mục tiêu 8 Mbps, tùy chọn mã hóa GPU.
- Mic và tiếng máy bật/tắt độc lập, dùng thiết bị mặc định Windows khi bắt đầu. Dùng tai nghe khi thu cả hai để giảm vọng âm.
- Thanh điều khiển quay tạm thời có pause/resume/stop. Windows được yêu cầu loại thanh này khỏi bản quay; khả năng thực tế phụ thuộc hệ thống và cần kiểm thử.
- Có xem/tua video, nhập file, tìm kiếm, đổi tên, xóa có xác nhận và xuất MP4.
- Quay video là nén mất dữ liệu, không phải lossless. Vùng video cần kích thước chẵn, có thể bỏ tối đa một hàng/cột ở cạnh phải/dưới.

## Nâng cấp từ V0.1

Đóng Capture Foundation cũ trước khi cài/mở Captovix. Khi chưa có thư viện Captovix, app nhập bản sao lịch sử và cấu hình từ `%LOCALAPPDATA%\HuyVo\CaptureFoundation`.

**Ảnh/video cũ tiếp tục được tham chiếu tại đường dẫn cũ, không bị chép lại hoặc di chuyển. Không xóa thư mục dữ liệu CaptureFoundation nếu còn muốn dùng các nội dung đó.** App mới tạo nội dung tại thư mục Captovix. Có thể gỡ chương trình V0.1; bộ gỡ cũ chỉ xóa app, giữ dữ liệu. Nên sao lưu cả hai thư mục dữ liệu khi chuyển sang máy khác.

Lịch sử đã có trong Captovix không bị ghi đè khi mở app lần sau. Phiên bản này không đồng bộ thư viện liên tục giữa hai app.

## Lưu trữ và riêng tư

- App cài mặc định: `%LOCALAPPDATA%\HuyVo.Captovix`, hoặc thư mục đã chọn trong Setup.
- Dữ liệu mới: `%LOCALAPPDATA%\HuyVo\Captovix`.
- `Media\`: file gốc; `Edits\`: lớp chú thích/crop JSON; `Thumbnails\`: ảnh thu nhỏ của bản chỉnh.
- `library.json`, `settings.json`: lịch sử và cấu hình. Chưa dùng SQLite.
- `update-settings.json`: nguồn phát hành, tùy chọn và lần kiểm tra gần nhất.
- `annotation-presets.json`: preset riêng; được bổ sung vào backup trước cập nhật.
- `Backups/before-update-*.zip`: metadata và JSON/Edits trước cập nhật; không chứa media.
- `editor-resume.json`: nội dung cần mở lại sau cập nhật, được xóa sau khi đọc thành công.
- Dữ liệu được ghi qua file tạm rồi thay thế trong cùng thư mục. File chụp/quay chỉ đưa vào lịch sử khi engine xác nhận hoàn tất. File `.pending.*` sau lỗi không được coi là bản quay thành công.
- App hoạt động offline, không đăng nhập và không gửi ảnh/video lên server. Nút báo cáo chẩn đoán chỉ tạo file trên máy, không tự gửi.
- Gỡ Captovix giữ dữ liệu. Xóa một nội dung trong Editor sẽ xóa file gốc và bản chú thích sau xác nhận.

## Trạng thái kiểm chứng và giới hạn

Đã cross-build Windows x64 và chạy kiểm tra logic ở Linux. Không có Windows desktop thật trong phiên xây dựng để xác nhận UI, clipboard, kết quả render PNG/JPEG, bộ cài, capture, DPI và âm thanh end-to-end. Bộ kiểm tra WPF renderer dành Windows được kèm trong source và workflow, đã biên dịch nhưng chưa chạy tại môi trường bàn giao. Xem `docs/VALIDATION.md` và `docs/WINDOWS-TEST-CHECKLIST.md`.

Chưa có cắt/ghép/chú thích video (lộ trình V2.0), HDR/tone mapping, webcam, OCR, chụp cuộn, lựa chọn từng thiết bị âm thanh, tray. Chưa có rollback dữ liệu tự động, xác thực nguồn private, rollout theo nhóm hoặc kiểm tra chữ ký nhà phát hành của gói. Portable kiểm tra và mở trang tải thủ công; cập nhật trong app cần bản Setup. Nguồn cập nhật đã gắn sẵn; phải phát hành gói có phiên bản cao hơn để app nhận cập nhật. Nội dung bảo vệ và secure desktop/UAC có thể không capture được. Không cam kết phục hồi MP4 đang ghi nếu mất điện/crash. FPS thực tế phụ thuộc GPU/CPU/nguồn chụp, không được suy ra từ giá trị cấu hình. Export ảnh giới hạn 100 triệu pixel để tránh cấp phát bộ nhớ quá lớn.

## Build source trên Windows

Cần .NET 10 SDK, Internet cho NuGet và Velopack CLI đã pin trong `.config/dotnet-tools.json`; NSIS 3 để tạo bộ cài chuyển đổi. Chạy tại thư mục source:

```powershell
.\Build.cmd
# Chỉ tạo portable:
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build-Windows.ps1 -SkipInstaller
```

Script chạy kiểm tra hình học, editor/history/migration, updater và Windows renderer/document, sau đó publish app self-contained. Velopack đóng gói feed/gói full, còn NSIS bọc Setup để chuyển V1.0 và kèm prerequisite offline. Kết quả ở `artifacts\`: thư mục `win-x64`, ZIP Portable, Setup.exe và Update-Packages.zip. `-SkipInstaller` bỏ bước bọc NSIS; Setup thô lúc đó không xử lý gỡ NSIS V1.0. Dùng bản Setup đầy đủ để chuyển đổi. Khi tạo Setup lần đầu, script tải VC++ Runtime từ Microsoft và kiểm tra chữ ký trước khi đóng gói. Runtime .NET được pin 10.0.13; cập nhật patch khi bảo trì.

```powershell
dotnet run --project tests/CaptureWorkflowChecks/CaptureWorkflowChecks.csproj -c Release
dotnet run --project tests/GeometryChecks/GeometryChecks.csproj -c Release
dotnet run --project tests/EditorCoreChecks/EditorCoreChecks.csproj -c Release
dotnet run --project tests/AdvancedAnnotationChecks/AdvancedAnnotationChecks.csproj -c Release
dotnet run --project tests/EditorWindowsChecks/EditorWindowsChecks.csproj -c Release -p:Platform=x64
```

`.github/workflows/windows-build.yml` build và chạy các kiểm tra trên Windows sau khi đưa source vào repo. Không thay thế kiểm thử màn hình/âm thanh bằng máy desktop thật.

## Công nghệ

C#/.NET 10, WPF/XAML, Win32/PerMonitorV2, GDI BitBlt cho ảnh desktop SDR, ScreenRecorderLib 7.0.1 (Desktop Duplication/Windows Graphics Capture/Media Foundation/WASAPI), tài liệu chú thích JSON, Velopack 1.2.161 cho cập nhật và NSIS bootstrap cho lần cài/chuyển đổi đầu tiên. Giấy phép thành phần bên thứ ba nằm trong `licenses/` và `THIRD-PARTY-NOTICES.txt`.

## Phiên bản tiếp theo

**V1.8 — Object Control & Layers:** bảng layer chọn/đổi tên/khóa/ẩn/sắp xếp đối tượng; nhập X/Y/W/H và góc xoay; cải thiện chọn nhiều, snap, group và căn/chia đều. Video tiếp tục hoãn. Xem ROADMAP.md cho kế hoạch đến V1.20.
