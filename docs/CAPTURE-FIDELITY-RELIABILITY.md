# V1.30.0 — Capture Fidelity & Reliability

## Dùng bản nâng cấp

Đóng Captovix, giải nén Update-Packages và chạy `HuyVo.Captovix-win-Setup.exe`, chọn đường dẫn hiện có (ví dụ `D:\Captovix\Captovix`). Không gỡ app hoặc xóa media. Bản này giữ nguyên byte cấu hình public license của Boss từ Source V1.29.1; không thay authority, không redeploy Edge/admin, không đổi database. **Không cần chạy migration mới.**

## Chụp đúng khoảnh khắc

1. Dùng nút Chụp ảnh hoặc hotkey đã cấu hình; chọn chế độ Vùng hoặc Phần tử/modal.
2. Captovix ẩn cửa sổ của mình, đợi compositor hoàn tất lượt present rồi đọc một bộ pixel desktop vào RAM trước khi hiện lớp chọn vùng. Không chờ lưu sidecar/khởi tạo UIA rồi mới giữ ảnh. Thời điểm được ghi trong metadata ảnh và status sau chụp.
3. Trên lớp chọn, màn hình là ảnh đã giữ. Dữ liệu phía sau tiếp tục thay đổi không làm đổi ảnh nguồn. Quét kéo, thả, chỉnh các cạnh/góc, Enter một lần để lưu vùng từ đúng bộ pixel đó.
4. Có kính lúp 15 × 15 pixel, mỗi ô là một pixel gốc; ô giữa có viền đen/trắng. Hiển thị X/Y tọa độ pixel thật, có thể âm trên màn hình phụ. Kính lúp nằm cạnh con trỏ, tự đổi phía khi gần mép. Bấm **M** hoặc icon kính lúp để ẩn/hiện khi cần xem dữ liệu dưới HUD.
5. Esc/chuột phải để hủy. Ảnh toàn desktop chỉ ở RAM; khi hủy không lưu full snapshot. Khi thành công chỉ ghi vùng được chọn. Đóng overlay và bỏ các tham chiếu snapshot/animation sau phiên.

Đây là giữ hình ảnh đang hiển thị, không dừng chương trình khác hay tắt thông báo của Windows. Thông báo xuất hiện **sau lần giữ ảnh** không được lấy vào PNG, vì Enter không đọc desktop lần hai. Không cam kết bắt đúng một timestamp phần cứng ở thời điểm phím xuống: Windows phải gửi hotkey, app ẩn cửa sổ và compositor phải present; độ trễ thực tế cần đo trên máy Windows. Nhiều màn hình được BitBlt lần lượt trong một lượt capture, không hứa atomic một frame trên mọi GPU.

Nếu đã cấu hình hẹn giờ, đếm ngược diễn ra **trước** lúc giữ ảnh; không đếm lại sau khi chọn. Chụp lại vùng và toàn màn hình dùng snapshot mới của lần gọi mới. Chụp cửa sổ native, quay và chụp cuộn vẫn cần nguồn live riêng; không dùng một frame tĩnh để giả thành video hoặc ảnh cuộn. Modal/element lấy pixel từ snapshot nhưng UIA vẫn đọc cấu trúc live; khối không nhận diện đúng có thể quét vùng thủ công ngay. Không hứa đã đóng băng accessibility tree của ứng dụng khác.

Snapshot tối đa **64 triệu pixel**, mỗi chiều tối đa32768; tính cả khoảng trống giữa màn hình trong virtual desktop. Vượt giới hạn thì báo lỗi rõ, không giảm độ phân giải tự động. Khoảng trống giữa màn hình là đen đục. Ảnh gốc sử dụng pixel vật lý SDR8; HDR/màu rộng/ảnh bảo vệ/desktop khóa/UAC cần đánh giá riêng. Ảnh từ máy remote nếu đã bị nén/mờ thì không tự có thêm chi tiết bằng việc chụp lại.

## Vừa khung và 100%

- **Vừa khung:** nút được đánh dấu, readout hiển thị `Vừa khung · …%`. Tỷ lệ này chỉ để xem ảnh trong vùng làm việc; thu nhỏ có thể làm chữ trông mềm hơn trên màn hình.
- **100%:** readout `100% · pixel thật`, một pixel đầu ra trên một pixel màn hình, bù Windows DPI 100–200%. Đổi màn hình khi ở 100% giữ chế độ pixel thật. Tooltip giải thích zoom và DPI.
- **Ctrl + con lăn:** zoom tùy chỉnh; con lăn thường cuộn dọc; Shift + con lăn cuộn ngang. Readout chuyển sang `Tùy chỉnh · …%`.
- Kích thước **Gốc** là capture đã lưu. **Xuất / Copy** là đầu ra sau crop/khung/chú thích/biến đổi. Bật viền tự động, canvas hoặc chủ động resize có thể làm đầu ra khác ảnh gốc; UI hiển thị kích thước thực đó.

PNG capture gốc có đúng W × H vùng chọn, không ép số chẵn như video. Lưu ảnh, copy và export sử dụng `EditorSurface.Flatten()` → renderer kích thước tài liệu, không dùng độ zoom hay ảnh thumbnail. Đường render nguyên pixel giữ BGRA vùng không có chỉnh sửa với các biến đổi integer (crop, xoay90°, flip); resize chủ động vẫn dùng nội suy. Save PNG kiểm tra decode lại đúng kích thước và toàn bộ BGRA trước khi thay file đích. Copy cung cấp PNG lossless và Bitmap96 DPI; ứng dụng nhận clipboard có thể tự đổi cách hiển thị/kích thước.

## Kiểm tra đã chạy và phần còn chờ

`tests/FrozenCaptureChecks`: crop âm/dương qua màn hình, màu từng pixel/kính lúp, vùng lẻ, không sửa backing buffer khi sửa crop trả về, 500 vùng ngẫu nhiên, bounds/memory cap, đường xác nhận dùng snapshot và lifecycle.

Các suite ImageFidelity/Geometry/Viewport/CaptureWorkflow/SmartCapture kiểm tra math 100/125/150/175/200%, transform và nguồn pixel. `tests/CaptureFidelityWindowsChecks` có 20 tổ hợp DPI/zoom nguồn nhân tạo qua Editor/PNG; chỉ chạy được trên Windows. Tổ hợp DPI nhân tạo không thay cho việc đổi scale Windows thật.

Chạy bộ kiểm tra thông thường:

```powershell
python scripts/Run-Checks.py
```

Trên desktop Windows dùng riêng cho kiểm thử, có thể chạy thêm clipboard thật:

```powershell
dotnet run --project tests/CaptureFidelityWindowsChecks -c Release -p:Platform=x64 -- --clipboard
```

Mở `docs/capture-pixel-test.html` trong trình duyệt để có vạch1pixel vật lý, checkerboard và đồng hồ thay đổi liên tục. Trang chạy local, không upload ảnh; đây là mẫu kiểm tra, không phải chứng nhận đã đạt. Với nút banner3giây: bấm nút rồi hotkey Captovix trước3giây, chờ5giây ở selector, chụp; banner phát sinh sau snapshot không được có trong PNG. Đây là banner DOM thử nghiệm; toast Windows thật vẫn cần test riêng.

### Ma trận nghiệm thu Windows thật — chưa thực hiện trong môi trường Linux

| Trường hợp | Cách kiểm tra | Kết quả cần đạt | Trạng thái |
| --- | --- | --- | --- |
| 100%,125%,150%,175%,200% từng màn | Mở bảng pixel/text cố định; chụp vùng lẻ601×333 | Capture PNG601×333; so BGRA với phần tương ứng của snapshot | Chờ Windows |
| Hai màn DPI khác nhau | 100% +150%, đổi vị trí trái/phải/trên | Không lệch gốc; vùng qua đường nối giữ đúng kích thước | Chờ Windows |
| Màn phụ tọa độ âm | Đặt màn phụ bên trái/trên, lấy vùng qua0 | Không mất dòng/cột tại0, không scale2lần | Chờ Windows |
| Giữ khoảnh khắc | Mở đồng hồ/animation; hotkey, chờ5–10giây, tạo toast sau đó | Selector giữ ảnh ban đầu; PNG là pixel ban đầu, không toast phát sinh sau snapshot | Chờ Windows |
| Kính lúp | Bảng checkerboard1px, kéo/resize ở cả hai màn | Ô giữa khớp pixel tại X/Y, không chụp kèm HUD | Chờ Windows |
| Hủy và chụp lại | Hủy, đổi nội dung, hotkey lại | Snapshot mới, không tái dùng frame cũ, không full-frame trên đĩa | Chờ Windows |
| Đổi DPI/rút màn trong selector | Đổi cấu hình display sau snapshot | Báo cần chụp lại, không ghi PNG bị scale/lệch | Chờ Windows |
| Editor/copy/export | So PNG ở Vừa khung,100%,50%,200%; clipboardPNG | Cùng kích thước/hash pixel nếu tài liệu không đổi | Chờ Windows |
| Chú thích nhỏ/crop/xoay | Vẽ vùng nhỏ, xuất, kiểm tra pixel vùng ngoài chú thích; xoay90° | Vùng không sửa giữ pixel; thay đổi hình học đúng lựa chọn | Chờ Windows |
| Cài/update/license | Cài đèV1.29.1, restart, trial/key hiện có | Media/license giữ nguyên, logo/icon đúng, không migration | Chờ Windows |

Nhóm HDR/WCG có checklist riêng; bản này không tự ánh xạ màu HDR rồi tuyên bố khớp RGB tuyệt đối. Chưa chạy WPF/GDI/clipboard/installer hoặc ma trận nhiều màn trên Windows thật tại lần bàn giao Linux. Các điều kiện nghiệm thu runtime chỉ được đánh dấu đạt khi có bằng chứng từ Windows.

Tài liệu API nền tảng: [DwmFlush](https://learn.microsoft.com/windows/win32/api/dwmapi/nf-dwmapi-dwmflush), [WPF BitmapScalingMode](https://learn.microsoft.com/dotnet/api/system.windows.media.bitmapscalingmode?view=windowsdesktop-10.0), [CroppedBitmap](https://learn.microsoft.com/dotnet/api/system.windows.media.imaging.croppedbitmap?view=windowsdesktop-10.0).
