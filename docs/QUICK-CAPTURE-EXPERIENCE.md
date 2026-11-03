# Captovix V1.31.0 — Quick Capture Experience

## Cài đặt và cập nhật

Giữ bản quyền, media và thư mục dữ liệu hiện có. Đóng Captovix/kết thúc quay, giải nén Update-Packages và chạy `HuyVo.Captovix-win-Setup.exe`, chọn đường dẫn đang dùng, ví dụ `D:\Captovix\Captovix`. Không cần gỡ app hoặc xóa dữ liệu. Không cần chạy migration mới, không cần triển khai lại Supabase Edge Function hay trang quản trị.

Để cập nhật trong app: upload năm assets bên trong Update-Packages ZIP lên stable GitHub Release `v1.31.0`: full.nupkg, canonical Setup.exe, RELEASES, assets.win.json, releases.win.json. Source ZIP dùng để phát triển; không thay cho binary update. Gói này giữ nguyên URL/issuer/public key của source V1.30 đã khớp với update Boss gửi. Không đưa private key vào source hoặc Release.

## Chụp nhanh bằng chuột

1. Bấm phím tắt đã cấu hình hoặc Chụp ảnh. App nhớ chế độ Vùng/Cửa sổ/Toàn màn hình/Phần tử khi chọn từ thao tác chụp; Chụp lại vùng là thao tác riêng, không đổi chế độ mặc định.
2. Chế độ Vùng phải kéo thủ công; để chuột đứng yên không tự chọn cả web. Phần tử/modal tiếp tục gợi ý khối theo chế độ riêng.
3. Kéo vùng rồi thả. Khung nét đứt vẫn cho phép kéo cạnh/góc hoặc di chuyển. Trỏ chuột đổi thành mũi tên resize theo cạnh/góc.
4. Bấm **Enter một lần** hoặc nút camera để chụp. Esc/chuột phải hủy. Không có bước Enter chốt vùng trung gian.

Thanh điều khiển được thu gọn; ưu tiên phía dưới/trên/trái/phải ngoài vùng chọn. Nếu vùng chiếm toàn màn hình, không tồn tại vị trí ngoài vùng: thanh nằm ở góc xa điểm thả chuột để giảm che mục tiêu. Trong lúc kéo/chỉnh biên, thanh được ẩn. Các thành phần này không nằm trong PNG: đường chụp vùng/phần tử tiếp tục cắt từ snapshot RAM đã giữ trước lớp phủ.

Viền chọn vùng có lớp đen mảnh + nét trắng mảnh để nhìn trên nền sáng/tối/màu; độ dày được quy đổi theo DPI màn hình. Đây là viền canh chọn trên màn hình, không tự thêm viền vào file ảnh. Tùy chọn viền ảnh khi chụp hiện có vẫn theo cấu hình của Boss.

## Đường tọa độ và kính lúp

- Đường ngang/dọc X/Y đi theo con trỏ trên lớp phủ; khe nhỏ tại giao điểm giúp nhìn pixel đang canh. Bấm **H** hoặc icon tọa độ để bật/tắt.
- Kính lúp giữ mẫu 15 × 15 pixel từ ảnh đã giữ, thu nhỏ từ160 ×190 DIP xuống112 ×138 DIP, viền mỏng hơn, hiển thị X/Y. Bấm **M** hoặc icon kính lúp để bật/tắt.
- Hai lựa chọn được nhớ riêng theo máy/tài khoản Windows trong `quick-capture.json`. Không đổi dữ liệu ảnh, bản quyền hoặc backend.
- Kính lúp và đường tọa độ phục vụ chọn vùng; không xuất vào ảnh. Mẫu tại biên màn hình lấy từ snapshot có giới hạn; HDR/WCG vẫn cần đánh giá riêng như V1.30.

## Chụp bằng bàn phím

Trong chế độ Vùng: đặt chuột tại góc đầu, bấm **Space** để đặt góc, dùng các phím mũi tên mở rộng vùng; Shift+mũi tên đi10pixel mỗi lần. Enter kết thúc lựa chọn và chụp trong cùng một lần bấm. Hoặc bấm một phím mũi tên khi chưa có vùng để tạo khung8 ×8pixel tại con trỏ; sau đó mũi tên di chuyển, Ctrl+mũi tên đổi kích thước, Shift tăng bước10pixel, Enter chụp. Esc hủy.

Trong chế độ Phần tử/modal: con lăn/↑↓ chọn cha/con khi chưa khóa. Click khối để khóa, chỉnh cạnh rồi Enter. Tab bỏ chọn để chọn lại. M/H vẫn bật/tắt kính và tọa độ.

## Crop áp dụng khi thả chuột

Chọn Crop: khung ban đầu bao toàn ảnh/canvas hiện tại. Kéo vùng bên trong ảnh để tạo vùng crop mới, hoặc kéo tay nắm để đổi biên; thả chuột áp dụng ngay và trở về Chọn. Kéo di chuyển một vùng crop số đã chuẩn bị cũng áp dụng khi thả. Click không kéo không crop. Kéo vùng không hợp lệ không áp dụng.

Ctrl+Z hoàn tác cả lần crop, Ctrl+Y làm lại. Ảnh gốc giữ riêng; kích thước xuất lấy đúng vùng crop. Nhập X/Y/W/H vẫn có Xem vùng và **Áp dụng kích thước**, phục vụ thao tác nhập số; không bắt buộc Enter sau thao tác kéo chuột. Esc trong lúc kéo hủy thao tác đang diễn ra.

## Màu công cụ Bước

Chọn Bước hoặc một số bước đã vẽ: bảng bên phải có hai ô Viền/Nền cùng bảng chọn màu, bút lấy màu và Trong suốt. Viền và nền độc lập; màu chữ/số chỉnh riêng ở phần chữ. Tạo mới, nhân bản, copy/paste, Undo/Redo và mẫu Đã lưu giữ các thuộc tính này.

Ảnh cũ không tự đổi màu hoặc độ trong suốt của Bước: trước khi Boss chỉnh kiểu, renderer giữ nền cũ/viền trắng cũ. Chú thích có màu viền/nền Bước mới dùng schema16; V1.31 đọc được schema1–16, vẫn từ chối bản tương lai. **Bản cũ V1.30 không đọc được sidecar schema16**; khi cần rollback dùng bản sao lưu trước nâng cấp, không tự hạ số phiên bản JSON. PNG gốc không đổi và không cần database migration. Các tài liệu chỉ dùng mask/opacity như V1.30 vẫn giữ schema15.

## Stamp và ô công cụ động

Thêm hiển thị nút **Stamp**, không rải toàn bộ mẫu ngay trong flyout công cụ. Bấm Stamp mở bộ12 hình vector tự vẽ cho Captovix: check xanh, X đỏ, dấu duyệt, cảnh báo, thông tin, hỏi, sao, tim, cờ, mục tiêu, ghim và ý tưởng. Không dùng lại tài nguyên của Snagit. Các stamp kiểu chữ/khung cũ không nằm trong danh sách mới; đối tượng đã chèn trong ảnh cũ được giữ.

Chọn stamp để chèn vào ảnh dưới dạng lớp PNG trong suốt256 ×256, có thể di chuyển, resize, copy/nhân bản và Undo. Preview và đối tượng dùng cùng một hình được render; logo/ảnh riêng dùng nút Thêm logo/ảnh. Stamp là tài nguyên hình; không phải text/callout cần sửa chữ.

Ngay trái Thêm có một ô công cụ động. Chọn Stamp/Watermark/Clone/... từ Thêm sẽ lập tức cập nhật icon và tên tại ô này. Bấm ô để gọi lại công cụ mà không cần mở Thêm; Stamp mở lại danh sách stamp. Ghi nhớ công cụ gần nhất qua lần mở app trong `last-studio-tool.json`. Mỗi công cụ có icon riêng. Khi sắp xếp thanh công cụ, cặp ô động–Thêm vẫn giữ cạnh nhau.

## Thông tin cập nhật

Màn hình Thiết lập/Cập nhật và popup báo có bản mới dùng chung bộ chuẩn hóa nội dung: bỏ dấu Markdown của tiêu đề, bold/code/link, hiển thị danh sách dễ đọc và cùng giới hạn5000ký tự. Settings dùng thẻ nội dung có khoảng đệm, chữ13/line-height21; popup cùng cỡ chữ. Không chạy HTML/script từ ghi chú phát hành. Release notes V1.31 chỉ chứa mô tả tính năng cho người dùng; thông tin build/kiểm thử đặt trong docs/VALIDATION.md.

## Build và nghiệm thu

Dependencies/runtime giữ theo V1.30: WPF .NET10, Windows x64 self-contained10.0.13, NSIS3, Velopack1.2.161, Tesseract5.2.0, ScreenRecorderLib7.0.1. Không nâng stack trong bản này.

```powershell
python scripts/Run-Checks.py
powershell -ExecutionPolicy Bypass -File scripts/Build-Windows.ps1
```

Workflow GitHub Actions đã có tiếp tục dùng public Variables đã cấu hình; không cần tạo lại authority. Source ZIP mới có tên `Captovix V1.31.0 — Quick Capture Experience.zip`. Update-Packages có Setup.exe. Không tự push, deploy hoặc publish Release.

Kiểm tra trên Windows thật trước phát hành rộng: DPI100/125/150/175/200%, nhiều màn hình tọa độ âm, toolbar khi chọn sát mép/full-screen, độ tương phản/resize cursor, M/H/Space/arrows/Enter/Esc, modal nhận diện, PNG không chứa overlay và pixel giữ nguyên; crop thả/Undo; màu Bước cũ/mới/copy/save/reopen; icon động/pair drag/restart; từng stamp preview/thêm/resize; thông tin cập nhật sáng/tối và bản quyền khi hết trial. Cross-build Linux không thay thế các kiểm tra native này.
