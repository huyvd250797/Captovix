Captovix V1.36.0 — Text & Callout Polish

Thoát Captovix rồi chạy HuyVo.Captovix-win-Setup.exe vào đường dẫn hiện tại. Không uninstall. Giữ dữ liệu ảnh, cấu hình và license; không cần migration/redeploy backend.

Cập nhật thẳng từ V1.31/V1.33: upload ĐỦ5assets lên stable Release v1.36.0:
HuyVo.Captovix-1.36.0-full.nupkg
HuyVo.Captovix-win-Setup.exe
RELEASES
assets.win.json
releases.win.json
Đánh dấu release latest, giữ kênh win/AppId. Không chỉ upload ZIP, không cần tạo các bản trung gian. Gói Full chứa toàn bộ runtime/app; không phải bản vá nối tiếp. Bản cũ có updater tương thích sẽ chọn latest. Bản portable dùng Setup thủ công. Không tự publish trong lần bàn giao này.

Kiểm thử Windows thật: IME/chuột/zoom/DPI/cài đè và restart update. Schema17 giữ nguyên từ source1.33; backup trước rollback về app cũ hơn.
