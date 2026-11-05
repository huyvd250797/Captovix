# V1.41.1 — Startup & Popup Recovery

## Nguyên nhân trong source V1.41.0

1. `Themes.xaml` có hai Style ngầm cùng TargetType=TreeViewItem trong cùng ResourceDictionary. WPF dùng kiểu đích làm khóa cho Style không có x:Key; khóa này bị trùng lúc nạp resource. `App.InitializeComponent()` nạp Themes trước khi tạo MainWindow và trước `OnStartup`, nên handler DispatcherUnhandledException chưa được đăng ký. Đây là lỗi có thể chặn app ngay lúc mở và không hiện hộp thoại cũ.
2. `WindowChromeStudio` đặt root làm Window.Content rồi `PopupPresentation` gán root đó làm Border.Child mà chưa tách khỏi Window. Một phần tử WPF không được có hai logical parent; lỗi này ảnh hưởng cửa sổ/popup dùng khung mới, kể cả popup bản quyền mở đầu phiên.

Đã kiểm bằng source Boss gửi và bộ kiểm tra tái hiện; chưa có errors.log từ máy Boss để đối chiếu toàn bộ lỗi native cụ thể trên máy đó.

## Sửa trong bản vá

- Bỏ Style TreeViewItem trùng, giữ nguyên template cây và các màu theme của style gốc.
- Tách Window.Content trước khi đưa root vào khung popup. Giữ viền/bóng, theme và hiệu ứng outline; không tháo tính năng M4.
- Bọc đoạn tạo/nạp/chạy WPF bằng catch ở entry point. Nếu lỗi xảy ra trước OnStartup: ghi lỗi/inner stack vào `%LOCALAPPDATA%\HuyVo\Captovix\errors.log`, hiện thông báo native không phụ thuộc theme và trả exit code lỗi.
- Giữ Velopack.Run và các worker OCR/element/fidelity trước WPF như cũ.
- Thêm StartupResourceChecks: kiểm khóa resource ở từng scope, khóa Style ẩn/explicit type, named styles hợp lệ, dictionary override hợp lệ; tái hiện duplicate từ ZIP1.41 đã gửi.
- Thêm StartupWindowsChecks để tải BAML thật và thử cửa sổ/modal sáng-tối, logical parent, templateTreeView và mở lại Popup. Chưa chạy fixture này trên hostLinux.

## Khôi phục máy đang không mở được app

1. Đóng mọi tiến trình Captovix đang còn ở Task Manager và khay hệ thống.
2. Chạy `Captovix-v1.41.1-Full-Setup.exe`, chọn đúng đường dẫn đang cài. Không gỡ app và không xóa dữ liệu/bản quyền.
3. Mở lại, kiểm tiêu đề V1.41.1; thử Thêm, KEY/trạng thái bản quyền và Nhóm & đối tượng.
4. Nếu vẫn lỗi, gửi errors.log mới nhất tại đường dẫn trên cùng thông báo lỗi khởi động. Bản vá không khẳng định đã xử lý mọi nguyên nhân môi trường chưa có log.

Bộ cài đầy đủ giữ runtime.NET10.0.13 self-contained, Visual C++ prerequisite và OCR như1.41. Ảnh/video, sidecar nhóm schema21, lịch sử, KEY/trial và cấu hình không bị reset. Không cần chạy migration mới; không deploy lại backend. Không cần chạy app1.41 để cài đè.

## Phát hành và kiểm tra

- Publish năm asset rời trong Update-Packages lên stable Releasev1.41.1, đặt Latest. Không chỉ tải ZIP lên. Lần bàn giao này chưa publish.
- Máy không khởi động được1.41 cần cài tay bằng Full Setup; chức năng cập nhật trong app chỉ dùng được khi app đang chạy.
- Windows thật còn chờ nghiệm thu: khởi động bản cài trên dữ liệu cũ, hộp lỗi/bản quyền/thiết lập/More/group ở sáng-tối, DPI/màn phụ, update/cài đè/restart. Cross-build không thay thế kiểm native.
