Captovix V1.41.0 — Groups & Layout Editing

Không cần migration mới/deploy backend. Giữ KEY/trial/API/database. Nhóm schema21 có backup .pre-v1.41.bak; backup thư viện trước rollback. App1.40 không đọc nhóm lồng schema21.

Tạo GitHub Release v1.41.0 trong huyvd250797/captovix-releases, publish stable và đặt Latest. Upload đủ 5 asset rời:
HuyVo.Captovix-1.41.0-full.nupkg
HuyVo.Captovix-win-Setup.exe
RELEASES
assets.win.json
releases.win.json

Không chỉ upload ZIP/source. Máy1.40 và bản cũ nhận sau publish. Bộ Captovix-Setup.exe online hiện có tiếp tục dùng nguồn Latest. Lần bàn giao này chưa tự publish.

Cài tay: thoát Captovix cả khay, chạy Full Setup hoặc HuyVo.Captovix-win-Setup.exe vào đường dẫn hiện tại; không uninstall/xóa thư viện. Runtime.NET/VC++/OCR giữ nguyên.

Nghiệm thu Windows theo docs/GROUPS-LAYOUT-EDITING-V141.md và VALIDATION.md, gồm nestedpanel/label/Undo/save/restore, popup sáng-tối/DPI/ngoài-modal, More và Explorer. HostLinux chỉ build Windows, chưa chạy WPF.
