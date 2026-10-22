# Captovix V1.11.1 — Update Reliability

## Cập nhật ổn định

Nguồn mặc định: https://github.com/huyvd250797/captovix-releases/releases/latest/download

App đọc trực tiếp releases.win.json tại nguồn stable, không dùng danh sách GitHub Releases bị giới hạn 10 mục. Khi có nhiều full assets, chọn bản stable cao nhất đúng AppId HuyVo.Captovix / kênh win, kiểm tra tên gói, size và SHA-256. Snapshot feed được dùng thống nhất giữa chẩn đoán và SDK. Chỉ báo không có bản mới sau khi đọc được feed hợp lệ; hiện phiên bản nguồn, URL thực đọc và phiên bản đang chạy. Feed rỗng, sai app, thiếu hash, HTTP 404/403, timeout và lỗi mạng báo rõ; không đổi thành thông báo mới nhất.

Cấu hình cũ trỏ đúng repository của Captovix được chuyển sang URL stable, giữ lựa chọn tắt tự kiểm tra. Nguồn riêng và cấu hình hỏng được giữ nguyên, không âm thầm chuyển nhà phát hành. Nguồn kiểu GitHub nhập dạng repository cũng được phân giải thành /releases/latest/download. Cấu hình URL/hotkey hệ thống vẫn có nút Lưu nguồn/Lưu cấu hình để kiểm tra kết nối hoặc đăng ký tổ hợp phím; thuộc tính chỉnh ảnh bên dưới không cần Áp dụng.

## Đăng bản phát hành

Giải nén Update-Packages ZIP. Upload cả 5 assets vào cùng một GitHub Release public:

1. HuyVo.Captovix-1.11.1-full.nupkg
2. releases.win.json
3. assets.win.json
4. RELEASES
5. HuyVo.Captovix-win-Setup.exe

README-PUBLISH.txt là hướng dẫn, không phải feed. Không upload Source ZIP thay nupkg. Không upload nguyên ZIP Update-Packages thay các file bên trong. Đặt tag v1.11.1, không Draft, không Pre-release, đánh dấu Set as latest release. Feed và gói phải cùng bản, không sửa tên/hash sau đóng gói. Lần bàn giao này chưa đăng lên GitHub.

Nếu app v1.9 vẫn dùng GithubSource cũ: chọn nguồn Thư mục HTTPS, dán URL stable ở trên, Lưu nguồn và Kiểm tra; hoặc chạy HuyVo.Captovix-win-Setup.exe bên trong ZIP khi app không mở được. Đóng Captovix trước khi chạy Setup thủ công. Chọn đúng root đang cài, ví dụ D:\Captovix\Captovix; không chọn thư mục current bên trong. Bản cài được quản lý tiếp tục giữ root hiện có. Chỉ đổi root sau quy trình gỡ/cài được hướng dẫn trong INSTALL-LOCATION.md. Bản portable chỉ thông báo/tải thủ công.

## Xóa ảnh đã chụp

Nút × trên từng thumbnail hoặc Xóa ảnh/video đang chọn chuyển ảnh gốc, sidecar chú thích, thumbnail và backup sidecar vào %LOCALAPPDATA%\HuyVo\Captovix\Trash. Có xác nhận trước khi xóa. Delete khi focus trong thư viện xóa nội dung; Delete trong Editor vẫn xóa đối tượng. Khôi phục lần xóa vừa rồi phục hồi nguyên byte trong phiên đang mở; khôi phục từ chối ghi đè file đã tồn tại. receipt.json trong mỗi thư mục Trash giữ đường dẫn và metadata nếu cần phục hồi thủ công sau khi đóng app. Không tự dọn Trash; thao tác này không phải xóa vĩnh viễn và không giải phóng dung lượng ngay.

## Điều khiển khung nhìn

- Ctrl + lăn chuột: zoom tại vị trí con trỏ.
- Lăn chuột: cuộn dọc, không zoom.
- Shift + lăn chuột: cuộn ngang. Khi đồng thời Ctrl và Shift, Ctrl được ưu tiên zoom.
- Space + kéo tiếp tục di chuyển khung nhìn. Không cuộn/zoom giữa một thao tác vẽ hoặc kéo đang diễn ra. Zoom không thay đổi số pixel file xuất.

## Viền tự động khi chụp

Trong cấu hình chụp, bật Viền sẵn khi chụp, chọn màu và độ dày 1–128 px. Thay đổi hợp lệ được lưu ngay; không cần Áp dụng. Ảnh mới nhận cấu hình dưới dạng trình bày của tài liệu, không ghi đè PNG gốc. Ví dụ ảnh gốc 1000 × 600 với viền 4 px xuất thành 1008 × 608. Auto-copy dùng ảnh đã có viền. Sửa hoặc bỏ viền tại nhóm Viền · nền · bóng đổ trong Editor; ảnh cũ không tự nhận cấu hình mới. CapturePhotoStyle có version riêng để bổ sung cấu hình về sau; preset chụp hiện có vẫn điều khiển nguồn/delay/cursor/copy, cấu hình viền là toàn cục.

## Thuộc tính và mẫu trực quan

Thay đổi số tự áp dụng sau khoảng 400 ms hoặc khi rời ô; checkbox/combo và chọn màu áp dụng ngay. Giá trị chưa hợp lệ giữ tài liệu hợp lệ trước đó và hiển thị lỗi. Chuyển ảnh, công cụ hoặc lưu/xuất sẽ hoàn tất giá trị đang chờ; pending không áp sang đối tượng khác. Crop vẫn cần xác nhận vì là một thao tác vùng, đánh lại số và chọn preset cũ là lệnh riêng.

Hai ô Viền và Nền mở bảng màu, có Trong suốt: không viền / không nền. Có bảng màu nhanh và nhập #RRGGBB tự áp dụng khi đủ ký tự. Chữ có màu chữ riêng, không có lựa chọn chữ trong suốt. Callout có màu chữ và viền riêng. Với ảnh, hai ô Viền ảnh / Nền ảnh hoạt động tương tự.

Mẫu hình và kiểu nét hiển thị trực tiếp bằng màu/nền/nét/bóng hiện tại: hình chữ nhật, bo góc, ellipse, tam giác, thoi, đa giác, sao; nét liền, đứt, chấm, đứt chấm; đường thẳng/gấp khúc/cong và đầu mũi tên. Bấm mẫu để thay hình/kiểu trên đối tượng tương thích và đặt công cụ vẽ tiếp theo. Bấm ☆ ngay mẫu để lưu, ★ để bỏ; không cần đặt tên preset. Tối đa 64 mẫu, lưu style-favorites.json. Mẫu lưu kiểu và màu, không lưu vị trí/ID/kích thước cụ thể. Preset cũ vẫn đọc được trong mục thu gọn.

## Bóng 9 hướng

Lưới 3 × 3 chọn trái/trên/phải và các góc. Ô giữa hàng dưới = thẳng xuống (offset X=0, Y dương); ô giữa = bóng quanh (X=Y=0). Mỗi shape kín và callout có bật/tắt, khoảng cách 0–80 px, độ mềm 0–40 px, độ đậm 0–100%. Hướng tính theo trục ảnh, kể cả đối tượng xoay. Bóng vector nhiều lớp tạo độ mềm; không phải Gaussian blur vật lý. Đường/mũi tên và Text thường chưa có bóng riêng. Bóng không đổi tay nắm/hit-test/số pixel xuất; bóng chạm mép ảnh hoặc crop bị cắt theo biên. Nếu cần khoảng trống, tăng canvas/đệm ảnh. Bóng toàn ảnh có lưới hướng riêng và giữ thông số độ mềm/đậm đã có; bóng toàn ảnh có thể tăng kích thước xuất.

## Dữ liệu, build và nghiệm thu

Schema tài liệu 10 đọc 1–9. Trước lần ghi schema cũ, giữ .pre-v1.11.1.bak nguyên byte; không ghi đè schema tương lai. Clipboard đối tượng, nhân bản, preset và Undo/Redo giữ bóng riêng độc lập. Cấu hình capture/update/mẫu sao có backup trước khi sửa. Ảnh gốc và các vùng che V1.11 giữ pipeline hiện có. App vẫn mở maximized, một Editor. Không phát triển thêm editor video.

Build bằng Build.cmd hoặc scripts/Build-Windows.ps1 với .NET 10 SDK, NSIS 3; các bộ WPF chỉ chạy được trên Windows. Xem VALIDATION.md và WINDOWS-TEST-CHECKLIST.md về kết quả thực tế và phần chờ nghiệm thu. Chưa thể khẳng định đạt cài đặt, input, mixed-DPI và render Windows chỉ dựa vào cross-build.

Bản kế tiếp dự kiến V1.12 — Selection & Pixel Editing: chọn vùng chữ nhật/ellipse/lasso, di chuyển/cắt/copy/paste vùng pixel thành lớp ảnh sửa lại được, tô màu/xóa vùng và Undo/Redo. Cần giữ pixel gốc, tách rõ chọn đối tượng và chọn vùng ảnh; video tiếp tục để sau.
