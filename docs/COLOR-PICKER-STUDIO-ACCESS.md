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

