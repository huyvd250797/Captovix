# Captovix V1.6 — Image Fidelity

## Luồng dữ liệu

Capture nguồn vật lý → PNG pending → đọc kích thước thực → xác minh vùng/màn hình → commit PNG gốc và history. Nếu output khác W×H vùng chọn, không resize bù và không ghi nhận thành công; pending được giữ. Window capture dùng kích thước native do backend trả về vì GetWindowRect có thể bao gồm phần viền không hiện trong frame; không kéo giãn để khớp metadata yêu cầu.

Editor mở bitmap đầy đủ (không DecodePixelWidth thumbnail). Tài liệu giữ crop, annotation và appearance trong tọa độ ảnh gốc. Thumbnail 240 px là cache riêng. Zoom/DPI chỉ điều khiển viewport, không truyền vào kích thước file render.

Nhánh copy pixel dùng BGRA8: crop và affine trực giao 90°/flip có hệ số nguyên ±1, vị trí canvas nguyên pixel. Frame/bóng và overlay được render riêng, ghép alpha vào nền. Vùng overlay alpha bằng 0 giữ nguyên mọi byte nguồn; nguồn opaque ghi trực tiếp, không chuyển premultiplied rồi unpremultiply vùng không chỉnh. Nếu người dùng đặt resize, renderer WPF có nội suy theo kích thước recipe, không giả vờ giữ chi tiết chưa có. Copy và PNG/JPEG dùng cùng Surface.Flatten không có maxWidth; chỉ thumbnail mới gọi render giới hạn chiều rộng.

Canvas dư lẻ trước đây có thể đặt ảnh ở nửa pixel và gây nội suy. V1.6 canh nguyên pixel, dồn pixel dư sang bên phải/dưới ở anchor giữa; hình cũ có thể lệch nửa pixel về bố cục trung tâm nhưng không đổi dữ liệu nguồn hay vị trí đối tượng trong JSON.

## Hiển thị

- Gốc: PixelWidth × PixelHeight bitmap đầy đủ.
- Xuất/Copy: kích thước layout sau crop/rotate/resize/canvas/padding/viền/bóng.
- Zoom: Surface.Zoom × Windows DPI scale, là mức phóng đại vật lý của đầu ra.
- Actual Pixels: Surface.Zoom = 1 / DPI scale. 100% nghĩa 1 output pixel = 1 screen pixel, kể cả Windows 125/150/175/200%.
- Khi chuyển monitor, Actual Pixels giữ 100%, zoom tự chọn giữ tỷ lệ vật lý, Fit tính lại khung nhìn. Các phép mô phỏng DPI chỉ chứng minh math/handler, chưa chứng minh hệ điều hành và GPU đã vẽ đúng trên máy thật.
- Preview dùng nearest-neighbor tại mức phóng đại vật lý nguyên và high-quality khi thu nhỏ/tỷ lệ lẻ. Fit có thể trông mềm; file không bị giảm pixel.

## Copy và file

PNG + Bitmap cùng có trên clipboard, với số pixel giống bản render và metadata 96 DPI. Ứng dụng nhận có thể chọn Bitmap thay PNG, xử lý alpha khác hoặc tự nén/resize; không thể cam kết mọi app dán giống nhau. Nghiệm thu stream PNG trong clipboard và cả app đích Boss sử dụng (Word/Paint/Teams nếu có).

PNG không mất mát trong phạm vi BGRA8 đã giải mã. PNG đầu ra không đi qua thumbnail. JPEG 95% có mất mát, trong suốt ghép trắng bằng số học pixel, giữ W×H. File xuất được ghi temporary cùng thư mục, xác minh kích thước rồi thay đích; lỗi không phá file đích cũ. Ảnh gốc thư viện không bị ghi đè.

Không cam kết giữ raw 16-bit, profile ICC/wide gamut, HDR, tone mapping hay khớp màu màn hình được quản lý màu đặc biệt. Metadata DPI không tự tạo thêm chi tiết. Không thêm sharpen/upscale AI trong phiên bản này.

## Công cụ nghiệm thu Windows

Vào Máy và màn hình → Kiểm chứng ảnh SDR trên máy này. Có thể gọi Captovix.exe --fidelity-check tại thư mục cài đặt, không tự kích hoạt qua launch thông thường. Probe là tiến trình riêng, không mở Editor và không tham gia mutex Editor; Velopack hook vẫn chạy trước khi vào probe.

Dialog yêu cầu SDR do người dùng xác nhận, KHÔNG tự phát hiện HDR. Probe thay clipboard bằng ảnh mẫu; kiểm tra 22 ca renderer/PNG, thêm clipboard PNG, rồi từng màn hình có mẫu pixel vùng, kích thước toàn màn hình và mẫu native-window. Mẫu bao gồm đường luân phiên 1 pixel và ô màu. Native-window tìm nguyên mẫu 1:1 trong frame để tránh giả định border/caption có cùng extent với GetWindowRect.

Kết quả JSON và ảnh mẫu ở %LOCALAPPDATA%\HuyVo\Captovix\FidelityChecks\<run>. Full-desktop PNG được xóa sau xác minh kích thước; nếu hủy đột ngột có thể còn pending. Report ghi version, Windows OS, DPI thật của monitor và cửa sổ, tọa độ, các ca pass/fail. Không có ảnh hoặc report đã chạy Windows trong gói bàn giao này.

Chạy ở đủ 100/125/150/175/200%; có monitor âm; kiểm tra kéo chính Editor qua monitor. Summarize-Fidelity.ps1 tổng hợp report pass của V1.6.1 và chỉ báo scale đã quan sát, không tự khẳng định hoàn tất manual checklist. Source có hai harness WPF (EditorWindowsChecks và FidelityWindowsChecks), phải chạy trên Windows.

## Bằng chứng hiện có tại lần bàn giao

525 logic/tĩnh trên Linux (122 mới, gồm 2000 pixel permutation ngẫu nhiên), build WPF và publish x64. 9 SDK integration trên nupkg thật sau đóng gói. Chưa chạy renderer WPF, PNG codec, Clipboard, native screen/window capture và chuyển monitor thực tế. Không được chuyển trạng thái nghiệm thu sang đạt chỉ vì build thành công.

Tham khảo API chính thức: Microsoft RenderTargetBitmap, BitmapScalingMode, Clipboard.SetImage/SetDataObject và Desktop Duplication. Các tài liệu API không chứng minh app đã qua kiểm thử Windows:
- https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.rendertargetbitmap
- https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.bitmapscalingmode
- https://learn.microsoft.com/en-us/dotnet/api/system.windows.clipboard.setdataobject
- https://learn.microsoft.com/en-us/windows/desktop/direct3ddxgi/desktop-dup-api


## Hotfix V1.6.1

Ảnh desktop (vùng và toàn màn hình) sử dụng DesktopPhotoCapture: GDI BitBlt vào DIB top-down BGRA32 đúng kích thước, không StretchBlt. Các màn hình được cắt/ghép theo pixel vật lý; khe trống để đen, alpha desktop đặt 255. PNG qua ImageTransfer.Save và kiểm tra kích thước. Con trỏ vẽ qua CopyIcon/GetIconInfo/DrawIconEx và hotspot. Mọi handle native được giải phóng trong finally. Chụp cửa sổ vẫn là WGC/ScreenRecorderLib, video vẫn căn chẵn. Không coi GDI SDR8 là giải pháp HDR/ICC. Probe dùng cùng CaptureEngine nên gọi đúng backend mới cho desktop.

Nguyên nhân: InitializeRects của ScreenRecorderLib gọi MakeEven cho output và source; nguồn có OutputSize cũng bị MakeRectEven. Việc chỉ tăng kích thước output/copy cột cuối không khôi phục pixel đã mất. Vì vậy guard không bị gỡ bỏ và không resize ảnh lỗi.

Nguồn tham khảo: https://github.com/sskodje/ScreenRecorderLib/blob/master/ScreenRecorderLibNative/RecordingManager.cpp ; https://github.com/sskodje/ScreenRecorderLib/blob/master/ScreenRecorderLibNative/ScreenCaptureManager.cpp ; https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-bitblt .
