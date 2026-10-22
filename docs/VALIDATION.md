# Captovix V1.9.0 — Trạng thái kiểm chứng

## Đã chạy trong môi trường Linux

- Các bộ test pure .NET: ConnectorChecks, ObjectControlChecks, ShapeStudioChecks, ImageFidelityChecks, CaptureWorkflowChecks, GeometryChecks, EditorCoreChecks, AnnotationChecks, AdvancedAnnotationChecks, ImageAppearanceChecks, ViewportChecks, UpdateChecks; InstallerContractChecks kiểm tra hợp đồng argument/install-path trong source.
- ConnectorChecks có 81 kiểm tra, gồm giao thức tiến trình thật trên Linux với fixture: timeout, kill/restart, response lỗi và hủy. Fixture không giả làm kết quả đã test UIA Windows. Tổng logic/static 781; thêm 9 kiểm tra SDK gói thực tế = 790.
- Cross-build ứng dụng net10.0-windows x64 và năm dự án WPF: ConnectorWindowsChecks, EditorWindowsChecks, ObjectWindowsChecks, ShapeWindowsChecks, FidelityWindowsChecks. Không thực thi các dự án Windows tại đây.
- Self-contained publish, full nupkg Velopack, NSIS wrapper; kiểm tra CRC ZIP, package AppId/version, feed SHA1/SHA256/size và DLL đồng nhất với publish.
- Tích hợp UpdateChecks với gói full thực tế qua SDK Velopack, bao gồm từ chối hash sai.

Số lượng và kết quả cụ thể được ghi trong docs/PACKAGE-VALIDATION.txt sau khi hoàn tất kiểm tra gói.

## Chưa chạy / không được coi đã đạt

Khởi động và dùng WPF trên Windows, Ctrl+` toàn hệ thống/khay/xung đột thật, UIA browser/modal/grid và provider treo, low-level mouse hook nuốt click/cancel, mixed DPI và monitor âm, GDI/cursor/window capture, clipboard, cài D:\Captovix\Captovix, ký số prerequisite và updater restart trên desktop. Cần nghiệm thu theo checklist trước triển khai rộng. Không có môi trường Windows thật tại nơi build.

HDR/màu rộng, chụp cuộn, OCR, DOM/browser extension, auto-routing tránh shape, nhiều đỉnh tùy ý và Video Editor chưa thuộc phạm vi V1.9.0. Không đăng GitHub Release hoặc push source từ môi trường này.
