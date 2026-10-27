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

