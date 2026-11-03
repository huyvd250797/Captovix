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

