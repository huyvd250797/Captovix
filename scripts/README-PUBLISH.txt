V1.42.0 cross-build: native Windows acceptance vẫn cần hoàn tất trước khi publish stable/Latest.
Captovix V1.42.0 — Element Recognition & Fidelity

Sửa lỗi resourceTreeViewItem trùng và parentpopup từ1.41. Giữ dữ liệu/KEY/trial/cấu hình/schema21/runtime/OCR. Không cần migration mới/deploy backend.

Máy đang không mở được1.41: đóng Captovix cả Task Manager/khay, chạy FullSetup1.42.0 hoặc HuyVo.Captovix-win-Setup.exe vào đúng đường dẫn cũ. Không uninstall/xóa thư viện. Nếu còn lỗi, gửi %LOCALAPPDATA%\HuyVo\Captovix\errors.log mới nhất.

Để auto-update, tạo stable GitHub Releasev1.42.0, publish và đặtLatest, upload đủ5file rời:
HuyVo.Captovix-1.42.0-full.nupkg
HuyVo.Captovix-win-Setup.exe
RELEASES
assets.win.json
releases.win.json
Không chỉ upload ZIP/source. Chưa tự publish lần bàn giao này. Máy không chạy được app cần cài tay.

Chi tiết docs/STARTUP-POPUP-RECOVERY-V1411.md và VALIDATION.md. Chưa nghiệm thu nativeWindows/cài đè/updater/theme/DPI.
