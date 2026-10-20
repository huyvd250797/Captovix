# Validation — Captovix V1.2.1

## Installer Path Fix — 20/10/2026

- Confirmed NSIS v3.08 GetFullPathName checks existence and can clear the output for a nonexistent destination. Replaced destination normalization with Win32 GetFullPathNameW (lexical, no existence check).
- InstallerContractChecks: 25 static checks passed, including 8 new regressions. These do not execute the Windows installer.
- InstallerPathChecks.nsi: compiled developer-only harness for 12 Windows path cases, including D:\Captovix\Captovix, a dynamically named missing nested folder, Vietnamese/spaces/trailing separator/parent segments and root/relative/UNC rejection. Not executed on Windows here.
- WPF publish and NSIS installer compilation completed. Real Velopack package checked with UpdateChecks (core plus SDK integration); package hashes/sizes/version/source and archive contents verified before delivery.
- No Editor behavior changes from V1.2.0. Windows wizard/custom D destination/write access/upgrade/uninstall still require a real Windows run. Static checks cannot prove those runtime operations.

The previous V1.2.0 validation record follows for context.

# Validation — Captovix V1.2.0

Source đầu vào: Captovix-v1.1.0-Source(1).zip do người dùng cung cấp. Ngày đóng gói: 20/10/2026.

## Logic / build

- GeometryChecks 12; EditorCoreChecks 38; AnnotationChecks 76; ViewportChecks 48; UpdateChecks core 41; InstallerContractChecks 17. Tổng 232 checks logic/tĩnh đã chạy.
- ViewportChecks bao gồm zoom/pan bounds/anchor, crop rounding/validation/8 handles/ratio, 4000 randomized resize cases và giao dịch crop một bước undo.
- InstallerContractChecks là kiểm tra tĩnh, không chạy Windows installer. NSIS compilation xác nhận cú pháp/nguồn đóng gói.
- EditorWindowsChecks bổ sung API pending crop/apply/cancel/export và tiếp tục renderer/preset/annotation checks; chỉ cross-build, không chạy WPF tại đây.
- Sau khi tạo nupkg, chạy thêm 9 SDK integration checks với bản cài mô phỏng 1.1.0 để phát hiện/tải/xác minh 1.2.0; kiểm tra hash/size/default source/ZIP trước bàn giao.

## Giới hạn

Chưa có Windows desktop trong môi trường đóng gói: chưa chạy zoom/pan/crop bằng chuột thật, WPF render/clipboard, wizard thư mục/cài D/gỡ hoặc Update.exe. Cần thực hiện WINDOWS-TEST-CHECKLIST.md trước triển khai rộng. Không tự Publish Release GitHub.

Đổi đường dẫn app dùng Setup sau khi gỡ bản managed cũ; updater không tự di chuyển thư mục. Thư viện ảnh/video giữ ở vị trí hiện có. Schema vẫn 2. Bộ cài chưa có Authenticode của HuyVo.
