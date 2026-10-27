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

# Captovix V1.23.1 — Resize & Capture UX

- Kéo đối tượng mặc định tự do; chỉ Shift giữ tỷ lệ. Bỏ ép tỷ lệ theo loại đối tượng, giữ giới hạn cỡ chữ và thân callout.
- Chọn vùng pixel chữ nhật/ellipse có tay nắm chỉnh kích thước; vùng chụp màn hình giữ sau thả chuột, kéo lại cạnh/góc hoặc chọn lại, Enter/nút icon máy ảnh xác nhận; Esc hủy.
- Fit ảnh sử dụng nhiều diện tích hơn, không thay pixel xuất.
- Theo dõi phần canvas tự nới riêng; kéo vào trong/xóa đối tượng sẽ tính lại biên toàn bộ annotation nhìn thấy, giữ phần mở rộng thủ công cũ. Những margins đã tạo từ bản1.23.0 chưa có dấu tự động được giữ, dùng Crop để bỏ một lần.
- Popup update khi app hoạt động lại; Cập nhật mở trang cập nhật, không tự tải ngay; Để sau/đóng dùng animation về nút cập nhật. Animation tôn trọng thiết lập Windows.
- Ẩn mục Preset cũ và Trình bày toàn ảnh ở thanh phải; dữ liệu mẫu cũ giữ nguyên. Giữ công cụ ngang, thuộc tính bên phải và tùy chọn thư viện nằm dưới; nhận diện tím Captovix.

Chưa nghiệm thu Windows thật: resize/text/IME, capture đa màn hình/DPI, taskbar, animation, install/update. Không tuyên bố giao diện sao chép hoặc tương đương hoàn toàn Snagit. Resize chữ giữ glyph theo font, không kéo méo glyph theo hai trục; không có affine text deformation.

# Captovix V1.23.1 — Text & Callout Refinement

Windows x64 · C#/.NET10 WPF · SDK10.0.100 · runtime tự chứa10.0.13 · ScreenRecorderLib7.0.1 · Velopack1.2.161 · Tesseract5.2.0 x64 + model Việt/Anh kèm app.

Captovix = Capture + Tools + Visual + eXperience. Bộ công cụ chụp, ghi màn hình và biên tập hình ảnh trực quan.

## Chức năng mới

- Ảnh mới/mở lại mặc định Vừa khung. Vùng thao tác quanh ảnh có nền workspace, không hiển thị checkerboard như một khung ảnh lớn; kích thước xuất vẫn theo pixel ảnh/canvas thực.
- Vẽ hoặc kéo chú thích ra ngoài sẽ nới đúng cạnh bị vượt theo biên render, gồm viền, đuôi và bóng. Một callout ở trên không tự thêm viền trái/phải/dưới; nới và đối tượng chung một Undo.
- Sửa chữ trực tiếp trên ảnh bằng ô nhập trong suốt, không nền đen/viền ô nhập, không chồng hai lớp chữ. Ctrl+Enter hoàn tất, Enter xuống dòng, Esc hủy; bấm ngoài hoàn tất và về Chọn. Ctrl+B/I/U đổi kiểu toàn đối tượng.
- Callout co chiều cao theo chữ và đệm, giữ rộng người dùng chọn khi đủ chỗ; đuôi tự do giữ đích khi đổi nội dung. Mẫu và ảnh dùng chung hình học/đệm/giãn dòng/căn lề; đuôi bo tròn nối theo biên thật.
- Thanh phải chia thẻ Màu & kích thước → Hình dạng & kiểu nét → Chữ & bố trí → Hiệu ứng; cài đặt sâu gấp gọn. Font gõ/tìm được, B/I/U dạng nút, mẫu đã lưu theo công cụ.
- Tên ảnh/thời gian gọn, ngày đầy đủ trong tooltip; icon có khoảng đệm. Menu nhóm theo thao tác thực tế. Caption cả ba nút có hover; cửa sổ tối đa theo work area Windows để footer không nằm dưới taskbar.

Hướng dẫn/giới hạn: docs/TEXT-CALLOUT-REFINEMENT.md.

## Cài / cập nhật

Giải nén Captovix-v1.23.1-Update-Packages.zip, đóng Captovix cả tray, chạy HuyVo.Captovix-win-Setup.exe. Máy mới chọn thư mục riêng, ví dụ D:\\Captovix\\Captovix; managed install cũ giữ installroot. App có .NET runtime; installer có Visual C++ prerequisite. Setup chưa có chữ ký nhà phát hành.

Hoặc publisher đăng riêng5asset trong Update-Packages lên public stable Release tag v1.23.1, Set as latest: full.nupkg, HuyVo.Captovix-win-Setup.exe, releases.win.json, assets.win.json, RELEASES. Khi đó Trợ giúp → Kiểm tra cập nhật dùng URL ổn định gắn sẵn. Source ZIP chỉ dành cho phát triển, không dùng cài app. Bản bàn giao chưa tự đăng GitHub.

## Build từ source

Cài .NET10 SDK và NSIS3 trên Windows. Mở PowerShell tại thư mục source:

```powershell
dotnet restore src/Captovix/Captovix.csproj
dotnet run --project src/Captovix/Captovix.csproj -c Release -p:Platform=x64
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Kết quả ở artifacts. scripts/Run-Checks.py chạy các bộ logic và thực thi WPF trên Windows; trên Linux chỉ cross-build WPF. Đóng app trước cài/update. Không thêm dấu -- trước --installto trong installer vì gây lỗi EXE_ARGS.

## Dữ liệu

%LOCALAPPDATA%\\HuyVo\\Captovix: Media, Edits, library.json, appearance-settings.json, recent-capture-regions.json, ScrollSessions, OcrJobs, media-location.json, Brand, branding.log, capture.log và errors.log. Thư mục media có thể chọn riêng cho file mới; file cũ giữ đường dẫn. Metadata/Edits không chuyển theo media. Giữ dữ liệu khi update; sao lưu toàn thư mục trước nâng cấp/rollback. Tài liệu Editor đọc schema12/13/14; schema phụ thuộc chức năng đã dùng. Phiên cuộn mới schema2 đọc phiên1; app1.14 không mở phiên2. Khi loại khung, phiên gốc vẫn giữ và revision là thư mục mới. Không tự xóa frame nguồn; dọn phiên đã hết nhu cầu bằng Explorer.

## Kiểm tra và tiếp tục

29 bộ logic/static đạt1910 assertions;18 bộ WPF chỉ cross-build trên Linux. Main Windows x64 build/publish và nupkg/feed/installer/ZIP có kiểm riêng. Chưa chạy Windows desktop: UI/IME/taskbar/DPI/capture/install-update cần nghiệm thu theo docs/TEXT-CALLOUT-REFINEMENT.md và docs/VALIDATION.md.

Tiếp theo **V1.24 — Capture Precision**: làm rõ chọn phần tử cha/con, vùng cố định/gần đây, preview tọa độ/kích thước pixel, kiểm capture nhiều màn hình và các mức scale. Chỉ cam kết loại app đã nghiệm thu; UIA/native fallback cần đánh giá theo ứng dụng. Ưu tiên patch phản hồi Windows1.23 trước. Gửi SourceZIP1.23 này để tiếp tục.
