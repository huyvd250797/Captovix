# Chọn thư mục cài Captovix V1.2

## Cài mới

Chạy Captovix-v1.2.1-Setup.exe → Tiếp → trang thư mục → chọn thư mục riêng, ví dụ D:\Apps\Captovix → Cài đặt. Nguồn cập nhật đã được gắn sẵn.

V1.2.1 sửa lỗi từ chối đường dẫn mới: có thể nhập `D:\Captovix\Captovix` dù cả hai thư mục chưa tồn tại. Bộ cài sẽ tạo chúng sau khi xác thực.

Thư mục phải thuộc ổ đĩa cục bộ cố định, không phải gốc ổ; thư mục mới phải trống và người dùng hiện tại có quyền ghi. Bộ cài thử quyền ghi trước khi cài. Không chọn thư mục ảnh/video, thư mục cha/con của %LOCALAPPDATA%\HuyVo\Captovix hoặc thư mục chứa dữ liệu khác. Không yêu cầu tắt bảo vệ Windows.

Đây vẫn là bản cài theo người dùng. Nếu thiếu VC++ Runtime, thành phần chính thức của Microsoft có thể yêu cầu quyền quản trị. Chọn ổ khác không biến app thành bản cài cho toàn bộ người dùng.

## Đã có Captovix ở ổ C, muốn chuyển sang D

1. Kết thúc chụp/quay và đóng Captovix.
2. Trong Windows Settings → Apps, gỡ Captovix. Dữ liệu ảnh/video/chú thích nằm ngoài thư mục cài và được giữ.
3. Chạy Setup V1.2.1, chọn D:\Apps\Captovix và hoàn tất.
4. Mở app, xác nhận thư viện còn nguyên và nguồn cập nhật đúng.

Không cần xóa thủ công thư mục dữ liệu. Setup nhận diện bản Velopack đã cài và mặc định dùng lại thư mục đó. Nếu đổi thư mục khi bản cũ vẫn còn, hướng dẫn gỡ trước được hiển thị; bộ cài không âm thầm tạo hai root hoặc gỡ bản đang dùng. Chuyển từ NSIS V1.0 vẫn giữ cơ chế cài mới thành công rồi mới gỡ bản NSIS cũ đã nhận diện.

## Cập nhật về sau

Update.exe và shortcut trỏ vào đường dẫn đã chọn; cập nhật trong app tiếp tục thay binaries tại đó. Không có bước chọn thư mục khi cập nhật trong app. Setup lưu vị trí để các lần chạy sau tìm lại; có fallback từ registry của Velopack và đường dẫn mặc định cũ.

Đường dẫn cài ứng dụng và đường dẫn dữ liệu là hai vị trí riêng. Chọn D cho app không di chuyển ảnh/video khỏi nơi đang lưu. Media V0.1 có thể còn tham chiếu ở thư mục CaptureFoundation; tiếp tục giữ thư mục này. Di chuyển thư viện không thuộc V1.2.

Chưa thực thi wizard, cài ở ổ D, cập nhật hoặc gỡ trên Windows thật trong môi trường đóng gói. Checklist có ca kiểm thử đường dẫn có khoảng trắng/tiếng Việt/quyền ghi/ổ khác.
