# Capture Foundation — V0.1.0

Ứng dụng Windows kiểm chứng chụp ảnh/quay màn hình, do HuyVo định hướng sản phẩm. Tên **Capture Foundation** là tên kỹ thuật tạm thời, chưa phải tên thương hiệu đã tra cứu.

## Cách dùng nhanh

**Cách thuận tiện: chạy `Capture-Foundation-v0.1.0-Setup.exe`.** Bộ cài có sẵn runtime .NET, tạo shortcut Desktop/Start Menu và mục gỡ cài đặt. Không cần Visual Studio hay .NET SDK trên máy người dùng. Nếu thiếu Microsoft Visual C++ Runtime x64, bộ cài đi kèm thành phần chính thức của Microsoft và Windows sẽ hỏi quyền quản trị để cài nó. Không tắt Windows Defender hoặc bảo vệ hệ thống.

**Không bắt buộc cài app:** giải nén đầy đủ gói Portable vào một thư mục, chạy `Launch.cmd` hoặc `CaptureFoundation.exe`. Cần Microsoft Visual C++ Runtime x64 đã có trên máy. Nếu thiếu, chạy `Install.cmd` trong thư mục Portable hoặc dùng Setup.exe. `Install.cmd` cần Internet lần đầu nếu phải tải VC Runtime, kiểm tra chữ ký Microsoft rồi mới chạy. Cách này chép app vào thư mục Programs của người dùng và tạo shortcut Desktop; bản Setup.exe còn có Start Menu và trình gỡ cài đặt.

Máy đích: Windows x64, ưu tiên Windows 11; Windows 10 22H2 cần kiểm thử trên máy thực tế. Windows N/KN cần Media Feature Pack. Không có bản x86/ARM64 ở V0.1. Không dùng Vercel/Netlify để chạy app Windows này.

## V0.1 làm được gì?

- Một cửa sổ Editor, cấu hình chụp/quay ngay bên trong; không có màn hình Capture độc lập.
- Chụp PNG: kéo chọn vùng, chọn cửa sổ, hoặc toàn bộ một màn hình đang chọn.
- Quay MP4/H.264 từ cùng các nguồn; mic và tiếng máy có thể bật/tắt độc lập, dùng thiết bị mặc định Windows tại lúc bắt đầu.
- Kích thước theo nguồn, 15/30/60 FPS cấu hình, bitrate mục tiêu 8 Mbps; có tùy chọn mã hóa GPU. Đây là cấu hình yêu cầu, không phải cam kết hiệu năng thực tế.
- Vùng đi qua nhiều màn hình được chia/cắt và ghép theo tọa độ pixel vật lý. Cả tọa độ âm và màn hình xếp trên/dưới được xử lý trong thuật toán.
- Thanh điều khiển quay tạm thời: đồng hồ, tạm dừng/tiếp tục, kết thúc. Có yêu cầu Windows loại thanh này khỏi ảnh quay; nếu API không xác nhận, thông báo và kéo thanh ra ngoài vùng quay.
- Danh sách ảnh/video chung; ảnh có thumbnail, video có thẻ VIDEO. Xem ảnh vừa khung/100% pixel; xem và tua video.
- Copy ảnh, xuất PNG/JPEG/MP4, nhập file, đổi tên và xóa có xác nhận.
- Lưu lịch sử/cấu hình trên máy. File chụp/quay chỉ được nhận vào lịch sử khi engine xác nhận hoàn tất. File `pending` sau lỗi được giữ riêng để chẩn đoán.
- Báo cáo JSON thông tin OS, runtime, màn hình, DPI, phím tắt đã đăng ký và vùng chọn gần nhất.

## Thao tác

1. Chọn **Vùng chọn**, **Cửa sổ** hoặc **Toàn màn hình**. Với cửa sổ/màn hình, chọn nguồn trong danh sách kế bên; dùng ↻ để làm mới.
2. Chọn trễ 0/3/5 giây, con trỏ; với video chọn thêm mic/tiếng máy/FPS/GPU.
3. Nhấn **Chụp ảnh** hoặc **Quay màn hình**. Editor ẩn khi chọn vùng và capture.
4. Kéo chọn vùng, thả chuột để xác nhận; Esc hoặc chuột phải để hủy.
5. Khi quay, dùng thanh điều khiển hoặc phím tắt. Sau khi kết thúc, chờ hoàn tất MP4; Editor hiện lại và chọn nội dung mới.
6. Chọn nội dung cũ ở trái, copy hoặc xuất file. Nút **Báo cáo máy & màn hình** ở phải giúp ghi nhận máy kiểm thử.

| Phím tắt | Thao tác |
|---|---|
| Ctrl + Shift + 1 | Chụp theo cấu hình |
| Ctrl + Shift + 2 | Bắt đầu quay |
| Ctrl + Shift + 3 | Kết thúc quay |
| Ctrl + Shift + 4 | Tạm dừng/tiếp tục quay |

App phải đang chạy để nhận phím tắt. Nếu trùng app khác, trạng thái sẽ báo; dùng nút Editor thay thế. V0.1 chưa có biểu tượng tray hoặc tự chạy cùng Windows. Đóng cửa sổ là thoát app khi không capture. Đang quay thì yêu cầu đóng sẽ kết thúc ghi trước; cần đóng lại sau khi lưu xong.

## Dữ liệu và cài đặt

- App cài tại `%LOCALAPPDATA%\Programs\CaptureFoundation`.
- Dữ liệu tại `%LOCALAPPDATA%\HuyVo\CaptureFoundation`.
- `Media\`: ảnh/video gốc.
- `library.json`: lịch sử; `settings.json`: cấu hình. V0.1 dùng JSON ghi nguyên tử để giữ nền tảng đơn giản, sẽ chuyển SQLite khi mở rộng quản lý ở bản sau.
- `errors.log`, `recorder.log`, `diagnostics-*.json`: thông tin kiểm tra lỗi trên máy.
- Gỡ bản Setup không xóa media/cấu hình. Muốn dọn toàn bộ, sao lưu trước rồi xóa thư mục dữ liệu thủ công.
- File ảnh/video không được gửi lên server. Không cần tài khoản hoặc database online. Báo cáo chẩn đoán chỉ được ghi ra máy, không tự gửi đi.

## Giới hạn V0.1 và trạng thái kiểm chứng

**Đây là bản kiểm chứng nền tảng, chưa phải bản production đã chứng nhận chạy tốt trên mọi máy Windows.** Đã cross-build Windows x64, kiểm tra XAML/C# và các test hình học trong môi trường Linux. Chưa có phiên Windows desktop thật trong môi trường bàn giao để xác nhận end-to-end mic, loopback, nhiều màn hình, DPI, giao diện và bộ cài. Xem `docs/VALIDATION.md` và thực hiện `docs/WINDOWS-TEST-CHECKLIST.md` trên máy Windows.

- V0.1 chưa có mũi tên/chữ/highlight/crop ảnh; chưa có cắt, ghép, chú thích video. Editor ở đây là khung xem/quản lý media và cấu hình, đúng phạm vi foundation; mockup trước là hướng thiết kế cho các bản hoàn thiện.
- Chưa hỗ trợ HDR/tone mapping, chụp cuộn, webcam, OCR hoặc tự cập nhật.
- Toàn màn hình là toàn bộ màn hình đã chọn. Muốn lấy nhiều màn hình, dùng kéo vùng đi qua các màn hình; hiệu quả ghép qua nhiều GPU cần kiểm thử.
- PNG giữ pixel gốc; video H.264 là nén mất dữ liệu, không phải lossless. Vùng video được làm chẵn kích thước, có thể bỏ tối đa một hàng/cột ở cạnh phải/dưới. Cửa sổ thay đổi kích thước trong lúc quay cần kiểm thử riêng.
- Màn hình HDR có thể sai màu; nên kiểm thử SDR trước. Nội dung được ứng dụng/Windows bảo vệ, màn hình UAC/secure desktop, cửa sổ thu nhỏ có thể không capture được. App không vượt các giới hạn này.
- Không có chọn từng thiết bị âm thanh, giảm ồn, chống vọng hoặc tự chuyển thiết bị giữa phiên. Dùng tai nghe khi thu cả mic và tiếng máy để giảm vọng.
- Nếu crash/mất điện trước khi hoàn tất, file MP4 `pending` có thể không phát được; app không cam kết cứu bản quay đó. Không hiển thị file `pending` như bản thành công.
- Setup/app hiện chưa có chứng thư ký mã thương mại. Việc Windows báo nhà phát hành chưa xác minh cần được đánh giá theo chính sách máy; không tắt tính năng bảo vệ.
- Danh sách dài tải ảnh thu nhỏ khi mở app; tối ưu hàng nghìn media nằm ở bản sau.

## Build source trên Windows

Cần .NET **10 SDK**, NSIS **3** (để tạo `.exe` installer), Internet để restore NuGet và tải VC Runtime. Dùng PowerShell tại thư mục source:

```powershell
.\Build.cmd
# Chỉ tạo app portable, không yêu cầu NSIS:
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build-Windows.ps1 -SkipInstaller
```

Kết quả ở `artifacts`: `win-x64\`, ZIP portable, Setup.exe. Gói source không bao gồm runtime nhị phân; gói portable và Setup đã có bản chạy sẵn. SDK không bắt buộc đúng patch nhưng cần dòng 10.0. Runtime self-contained được pin `10.0.13`; cập nhật patch khi bảo trì.

Build trực tiếp:

```powershell
dotnet run --project tests/GeometryChecks/GeometryChecks.csproj -c Release
dotnet publish src/CaptureFoundation/CaptureFoundation.csproj -c Release -r win-x64 --self-contained true -p:Platform=x64 -o artifacts/win-x64
```

Workflow `.github/workflows/windows-build.yml` có thể build trên GitHub Actions sau khi đưa source vào repo. Workflow chỉ kiểm tra build/test tự động, không thay thế kiểm thử màn hình/âm thanh trên máy có desktop.

## Cấu trúc source

- `src/CaptureFoundation/MainWindow.*`: Editor, cấu hình và luồng capture.
- `CaptureEngine.cs`: adapter ScreenRecorderLib 7.0.1; Desktop Duplication, Windows Graphics Capture cho cửa sổ, Media Foundation/WASAPI.
- `RegionPicker.cs`, `Native.cs`, `Geometry.cs`: lớp chọn vùng, Win32 và pixel nhiều màn hình.
- `MediaStore.cs`: file media, thumbnail, lịch sử và cấu hình.
- `RecordingBar.cs`: điều khiển quay tạm thời.
- `installer/`: NSIS installer; `scripts/`: build, portable launcher và cài theo người dùng.
- `tests/GeometryChecks/`: test clipping, tọa độ âm, cross-monitor, căn chẵn H.264.
- `docs/`: kết quả build và checklist Windows.

## Phiên bản tiếp theo

**V1.0 — Unified Editor:** hoàn thiện ổn định theo kết quả V0.1, thêm chú thích ảnh cơ bản, undo/redo và lưu dữ liệu chỉnh sửa tách khỏi bản gốc. Sau đó V1.1 hoàn thiện công cụ ảnh, V1.2 chỉnh sửa video.

© 2026 HuyVo. All rights reserved. Thành phần bên thứ ba theo giấy phép riêng trong `THIRD-PARTY-NOTICES.txt`.
