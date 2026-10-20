# Captovix V1.1.0 — Image Annotation

Ứng dụng Windows x64 chụp ảnh, quay màn hình và chú thích ảnh trong **một cửa sổ Editor**. Cấu hình chụp/quay nằm ngay trong Editor. © 2026 HuyVo. All rights reserved.

## Bắt đầu

**Khuyên dùng:** chạy `Captovix-v1.1.0-Setup.exe`, hoàn tất cài đặt rồi mở shortcut **Captovix**. Bộ cài đã kèm .NET runtime; không cần Visual Studio/.NET SDK trên máy người dùng. Nếu thiếu Microsoft Visual C++ Runtime x64, bộ cài chạy thành phần chính thức của Microsoft; bước đó có thể cần quyền quản trị.

**Portable:** giải nén toàn bộ `Captovix-v1.1.0-Portable-Windows-x64.zip`, chạy `Launch.cmd` hoặc `Captovix.exe`. Không chạy trực tiếp trong ZIP. Máy cần VC++ Runtime x64. `Install.cmd` chỉ cài VC++ Runtime nếu thiếu rồi mở bản Portable tại thư mục giải nén; không chép app thành bản cài đặt. Muốn cập nhật trong app, dùng Setup.exe. Script kiểm tra chữ ký Microsoft của bản runtime tải về. Internet chỉ cần cho bước tải prerequisite nếu máy còn thiếu.

Máy mục tiêu: Windows x64, ưu tiên Windows 11; Windows 10 22H2 cần kiểm thử thực tế. Windows N/KN cần Media Feature Pack. App chưa có bản x86/ARM64 hoặc chứng thư ký mã thương mại. Không tắt Windows Defender hay cơ chế bảo vệ của máy.

## Mới trong V1.1 — Image Annotation

- Làm mờ (B), Pixel hóa (P), Che kín (K); mức xử lý 4–64 px.
- 6 preset có sẵn; lưu/xóa tối đa 20 preset riêng trong bảng thuộc tính.
- Shift/Ctrl+click, kéo vùng trống hoặc Ctrl+A để chọn nhiều; kéo, căn chỉnh, đổi kiểu hoặc Delete cho cả nhóm.
- Căn theo khung ảnh/crop khi chọn 1 đối tượng, theo biên nhóm khi chọn nhiều.
- Tay nắm ở 8 hướng; kéo góc chữ/số để đổi cỡ. Shift khóa hướng/tỷ lệ; Ctrl trong lúc kéo bỏ bắt dính mép/tâm ảnh.
- Giữ undo/redo, tự lưu, ảnh gốc và URL cập nhật mặc định.
- Hướng dẫn đầy đủ: [IMAGE-ANNOTATION.md](docs/IMAGE-ANNOTATION.md).

Chia sẻ ảnh PNG/JPEG qua **Copy ảnh/Xuất file**. **Che kín** phù hợp khi cần ẩn hoàn toàn; ảnh gốc và tài liệu chỉnh sửa vẫn được giữ để sửa lại. Tài liệu mới dùng schema 2, đọc được dữ liệu V1.0 nhưng không nên mở ngược bằng app V1.0 sau khi lưu mới.

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
6. Dùng **Crop**, kéo và thả vùng muốn giữ để áp dụng ngay. Undo hoặc **Khôi phục khung ảnh gốc** để lấy lại.
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
| Ctrl+A | Chọn tất cả chú thích |
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

- App cài: `%LOCALAPPDATA%\HuyVo.Captovix`.
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

Chưa có cắt/ghép/chú thích video (lộ trình V1.2), HDR/tone mapping, webcam, OCR, chụp cuộn, lựa chọn từng thiết bị âm thanh, tray. Chưa có rollback dữ liệu tự động, xác thực nguồn private, rollout theo nhóm hoặc kiểm tra chữ ký nhà phát hành của gói. Portable kiểm tra và mở trang tải thủ công; cập nhật trong app cần bản Setup. Nguồn cập nhật đã gắn sẵn; phải phát hành gói có phiên bản cao hơn để app nhận cập nhật. Nội dung bảo vệ và secure desktop/UAC có thể không capture được. Không cam kết phục hồi MP4 đang ghi nếu mất điện/crash. FPS thực tế phụ thuộc GPU/CPU/nguồn chụp, không được suy ra từ giá trị cấu hình. Export ảnh giới hạn 100 triệu pixel để tránh cấp phát bộ nhớ quá lớn.

## Build source trên Windows

Cần .NET 10 SDK, Internet cho NuGet và Velopack CLI đã pin trong `.config/dotnet-tools.json`; NSIS 3 để tạo bộ cài chuyển đổi. Chạy tại thư mục source:

```powershell
.\Build.cmd
# Chỉ tạo portable:
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build-Windows.ps1 -SkipInstaller
```

Script chạy kiểm tra hình học, editor/history/migration, updater và Windows renderer/document, sau đó publish app self-contained. Velopack đóng gói feed/gói full, còn NSIS bọc Setup để chuyển V1.0 và kèm prerequisite offline. Kết quả ở `artifacts\`: thư mục `win-x64`, ZIP Portable, Setup.exe và Update-Packages.zip. `-SkipInstaller` bỏ bước bọc NSIS; Setup thô lúc đó không xử lý gỡ NSIS V1.0. Dùng bản Setup đầy đủ để chuyển đổi. Khi tạo Setup lần đầu, script tải VC++ Runtime từ Microsoft và kiểm tra chữ ký trước khi đóng gói. Runtime .NET được pin 10.0.13; cập nhật patch khi bảo trì.

```powershell
dotnet run --project tests/GeometryChecks/GeometryChecks.csproj -c Release
dotnet run --project tests/EditorCoreChecks/EditorCoreChecks.csproj -c Release
dotnet run --project tests/EditorWindowsChecks/EditorWindowsChecks.csproj -c Release -p:Platform=x64
```

`.github/workflows/windows-build.yml` build và chạy các kiểm tra trên Windows sau khi đưa source vào repo. Không thay thế kiểm thử màn hình/âm thanh bằng máy desktop thật.

## Công nghệ

C#/.NET 10, WPF/XAML, Win32/PerMonitorV2, ScreenRecorderLib 7.0.1 (Desktop Duplication/Windows Graphics Capture/Media Foundation/WASAPI), tài liệu chú thích JSON, Velopack 1.2.161 cho cập nhật và NSIS bootstrap cho lần cài/chuyển đổi đầu tiên. Giấy phép thành phần bên thứ ba nằm trong `licenses/` và `THIRD-PARTY-NOTICES.txt`.

## Phiên bản tiếp theo

**V1.2 — Recording & Video Tools:** cắt đầu/cuối video, chọn đoạn xuất, timeline/tua và hoàn thiện ghi hình/âm thanh.
