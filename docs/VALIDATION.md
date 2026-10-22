## V1.8.1 — Startup Recovery (22/10/2026)

Sửa NullReferenceException tại SyncProperties khi ComboBox phát SelectionChanged trong InitializeComponent. Callback chỉ chạy sau khi hoàn tất dựng XAML và nối các thành phần Editor; chặn đồng bộ lồng nhau. Giữ schema 6, ảnh gốc, sidecar, preset, URL update và đường dẫn cài.

Build và kiểm tra trên Linux; bộ kiểm tra WPF được cross-build, chưa thực thi Windows. Bộ hồi quy Windows gồm khởi tạo MainWindow thật, callback trước trạng thái sẵn sàng và đổi kiểu sau khởi tạo. Cần kiểm tra mở app trên máy Windows của Boss.

# Validation — Captovix V1.8.0

Nền attachment Captovix-v1.7.0-Source(1).zip, đọc cục bộ (SHA256 fe4687ccf6fce49ec805909c09c31708874bddf986c7a692ab25f5170854022f). Giữ SDK .NET 10.0.100/runtime Windows 10.0.13, ScreenRecorderLib 7.0.1, Velopack SDK/CLI 1.2.161 và NSIS 3.08; annotation schema lên 6. Không reset dữ liệu hay đổi URL update.

## Đã chạy trên Linux

ObjectControl 42, ShapeStudio 115, ImageFidelity 122, CaptureWorkflow 53, Geometry 30, EditorCore 38, Annotation 76, AdvancedAnnotation 49, ImageAppearance 61, Viewport 48, Update core 41, InstallerContract 25: **700 logic/static checks**. ObjectControl gồm migration 1–5, metadata copy/roundtrip/history, khóa/ẩn nhóm, các mutation guard, numeric transform, inverse/distance/bounds cho 2000 góc ngẫu nhiên, điểm neo resize, group rotation, snap/axis/bypass và 100 preview commit một Undo. Canvas candidates lọc nhóm bị khóa/ẩn bằng một lượt.

App và ObjectWindowsChecks/ShapeWindowsChecks/EditorWindowsChecks/FidelityWindowsChecks cross-built. Publish self-contained win-x64 và biên dịch NSIS wrapper. SDK checks nupkg thật và ZIP/identity/hash/size/DLL/default URL xác minh sau đóng gói; kết quả cuối ở PACKAGE-VALIDATION.txt trong Source ZIP.

## Chưa chạy trên Windows

Không có Windows host/VM/native UI. Không chạy các bộ WPF trên Windows; chỉ compile, không tính vào 700. ObjectWindowsChecks bao gồm rotate/render/hit/hide/lock, pixel nền, export theo zoom, vùng che xoay và inspector Layers/frame/metadata/Undo. Các bộ cũ giữ kiểm tra shape, fidelity và lifecycle chuyển ảnh. Tất cả đều chưa thực thi tại đây.

Chưa nghiệm thu chuột thật, layout, các mức DPI 100/125/150/175/200%, monitor âm/kéo Editor, GDI/cursor/clipboard, installer/update/restart và đường dẫn D. Không có report Windows đạt trong gói. V1.8 triển khai/build, CHỜ NGHIỆM THU WINDOWS. Chưa đăng GitHub Release.

## Giới hạn

SDR8, chưa đánh giá HDR/ICC/16-bit. Group có thành viên xoay chỉ resize đồng tỷ lệ, chữ/số qua font có giới hạn; một đối tượng dùng frame trước xoay, nhóm dùng AABB/góc tương đối. Ẩn layer loại khỏi file xuất, khóa chỉ ngăn chỉnh sửa. Mask xoay filter ROI world rồi clip. Tay nắm có thể bị cắt ở mép viewport, có ô góc để thao tác. Schema 6 không dùng ngược trên app cũ; khôi phục backup khi rollback. Không phát triển video hoặc connector trong V1.8.

Đường/mũi tên ngang/dọc có W hoặc H bằng 0, dùng tâm thật của hai đầu mút; khung số cho đổi hướng/chiều này và từ chối đoạn dài dưới 2 px. Giữ tỷ lệ tắt ở đoạn có một chiều 0. Group vẫn dùng khung bao tối thiểu 1 px cho tay nắm.
