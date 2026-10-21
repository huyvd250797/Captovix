# Validation — Captovix V1.4.0

Source đầu vào: Captovix-v1.3.0-Source.zip do Boss cung cấp. SDK 10.0.100, runtime Windows 10.0.13, Velopack SDK/CLI 1.2.161 và ScreenRecorderLib 7.0.1 giữ nguyên.

## Kiểm tra đã chạy

GeometryChecks 12; EditorCoreChecks 38; AnnotationChecks 76; AdvancedAnnotationChecks 49; ImageAppearanceChecks 61; ViewportChecks 48; UpdateChecks core 41; InstallerContractChecks 25: tổng 350 logic/tĩnh. Sau đóng gói có 9 ca SDK trên nupkg thực tế, tổng 359.

ImageAppearanceChecks kiểm tra identity/crop translation, 16 tổ hợp xoay/lật, inverse mapping, resize, rotate sau flip, các neo canvas, clipping và hit exclusion, tổng kích thước khung/bóng, JSON schema 1–4, undo/redo/reset, giới hạn cấp phát và 2000 trường hợp ma trận ngẫu nhiên. Các ca viewport tiếp tục có 4000 crop resize ngẫu nhiên.

Cross-build app và EditorWindowsChecks, publish win-x64 self-contained, biên dịch NSIS. Harness Windows mở rộng để kiểm tra pixel xoay/lật, alpha nền/bóng, kích thước xuất, inspector resize/aspect/reset và phối hợp inline text. Harness chỉ biên dịch tại Linux, chưa thực thi WPF.

Kiểm tra gói: phiên bản/AppId/hash/size/feed, URL cập nhật mặc định, DLL payload giống nupkg/Portable, ZIP CRC và whitelist release assets; bản Setup canonical giống file standalone. Không tự Publish GitHub Release.

## Chưa kiểm chứng trên Windows thật

Chưa chạy chuột/focus/IME khi xoay/lật, renderer WPF, clipboard/JPEG, bộ cài ở D, update restart hoặc capture/audio/DPI end-to-end. Build/logic không thay các kiểm tra này. Xem WINDOWS-TEST-CHECKLIST.md trước dùng rộng.

## Giới hạn

Schema 4 không mở ngược bằng app V1.3 trở xuống. Canvas nhỏ cắt phần ngoài khung nhưng giữ dữ liệu nguồn. Shadow là bóng mềm của khung chữ nhật. Đầu ra tối đa 32.768 px mỗi chiều và 100 triệu pixel. Chú thích giữ trong tọa độ ảnh gốc; resize không thêm chi tiết ảnh. Cấu hình chưa Áp dụng chưa nằm trong tài liệu. Không có chứng thư Authenticode của HuyVo. Video giữ tính năng hiện tại.
